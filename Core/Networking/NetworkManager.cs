using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Net;

namespace VectorRumble.Networking
{
    /// <summary>
    /// Owns the network session during a LAN game: pumps it every frame, sends packets
    /// (reliably or not, through the library's delivery guarantees) and hands received
    /// packets to the dispatcher.
    /// 
    /// The topology is a star. A client only knows the host, so everything between
    /// two clients is relayed by the host (see <see cref="Relay"/>).
    /// 
    /// Everything runs on the game thread. The Net library raises its events on a
    /// network thread, so they are queued and raised from <see cref="Update"/>.
    /// </summary>
    public sealed class NetworkManager : IDisposable
    {
        // seconds of unacknowledged reliable data before a peer is reported as stalled
        public const double StallTimeout = 5.0;

        const int ReceiveBufferSize = 2048;
        const int MaxPacketsPerUpdate = 1000;

        readonly NetworkSession session;
        readonly PacketDispatcher dispatcher = new PacketDispatcher();
        readonly HashSet<string> stalledPeers = new HashSet<string>();
        readonly byte[] receiveBuffer = new byte[ReceiveBufferSize];

        // events raised by the network thread, delivered by Update
        readonly ConcurrentQueue<NetworkGamer> joinedQueue = new ConcurrentQueue<NetworkGamer>();
        readonly ConcurrentQueue<NetworkGamer> leftQueue = new ConcurrentQueue<NetworkGamer>();
        int sessionEnded;
        string endReason = "";

        bool disposed;

        public NetworkManager(NetworkSession session)
        {
            if (session == null)
                throw new ArgumentNullException("session");

            this.session = session;
            NetLog.Write("Session started, this machine is the " + (session.IsHost ? "host" : "client"));
            session.GamerJoined += OnGamerJoined;
            session.GamerLeft += OnGamerLeft;
            session.SessionEnded += OnSessionEnded;
        }

        public NetworkSession Session
        {
            get { return session; }
        }

        public PacketDispatcher Dispatcher
        {
            get { return dispatcher; }
        }

        public bool IsHost
        {
            get { return session.IsHost; }
        }

        public NetworkRole Role
        {
            get { return session.IsHost ? NetworkRole.Host : NetworkRole.Client; }
        }

        /// <summary>
        /// The gamer on this machine
        /// </summary>
        public NetworkGamer LocalGamer
        {
            get { return session.LocalGamers.Count > 0 ? session.LocalGamers[0] : null; }
        }

        /// <summary>
        /// True once the session has ended (host left, network error, disposed)
        /// </summary>
        public bool HasEnded
        {
            get { return disposed || sessionEnded != 0; }
        }

        /// <summary>
        /// Why the session ended (empty while it is running)
        /// </summary>
        public string EndReason
        {
            get { return endReason; }
        }

        // raised from Update on the game thread
        public event Action<NetworkGamer> GamerJoined;
        public event Action<NetworkGamer> GamerLeft;
        public event Action SessionEnded;
        public event Action<NetworkGamer> PeerStalled;

        /// <summary>
        /// Round trip time to a peer as measured by the session
        /// </summary>
        public TimeSpan RoundTripTime(NetworkGamer peer)
        {
            return peer.RoundtripTime;
        }

        /// <summary>
        /// The remote peer with the given gamer id (null if it is not connected)
        /// </summary>
        public NetworkGamer FindRemote(string id)
        {
            foreach (NetworkGamer peer in session.RemoteGamers)
                if (peer.GamerId == id)
                    return peer;
            return null;
        }

        #region Sending

        /// <summary>
        /// Send a packet to one peer
        /// </summary>
        public void Send(NetworkGamer recipient, INetworkPacket packet, bool reliable)
        {
            SendPayload(recipient, Serialize(packet), reliable);
        }

        /// <summary>
        /// Send a packet to the host (clients only)
        /// </summary>
        public void SendToHost(INetworkPacket packet, bool reliable)
        {
            if (session.IsHost)
                throw new InvalidOperationException("The host has no host to send to.");

            Send(session.Host, packet, reliable);
        }

        /// <summary>
        /// Send a packet to every other machine (the host sends to all clients,
        /// a client sends to the host)
        /// </summary>
        public void Broadcast(INetworkPacket packet, bool reliable)
        {
            Relay(packet, reliable, null);
        }

        /// <summary>
        /// Send a packet to every other machine except one. The host uses this to pass
        /// a packet it received from one client on to the other clients.
        /// </summary>
        public void Relay(INetworkPacket packet, bool reliable, NetworkGamer except)
        {
            byte[] payload = Serialize(packet);

            foreach (NetworkGamer peer in session.RemoteGamers)
                if (except == null || peer.GamerId != except.GamerId)
                    SendPayload(peer, payload, reliable);
        }

        void SendPayload(NetworkGamer recipient, byte[] payload, bool reliable)
        {
            if (HasEnded || recipient == null)
                return;

            LocalGamer.SendData(payload,
                reliable ? SendDataOptions.Reliable : SendDataOptions.None, recipient);
        }

        static byte[] Serialize(INetworkPacket packet)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)packet.Type);
                packet.Serialize(writer);
                writer.Flush();
                return stream.ToArray();
            }
        }

        #endregion

        /// <summary>
        /// Pump the session: receive packets, resend unacknowledged ones and raise
        /// queued events. Call once per frame, before the game simulation.
        /// </summary>
        public void Update()
        {
            if (HasEnded)
            {
                RaiseEvents();
                return;
            }

            session.Update();
            ReceivePackets();

            CheckStalledPeers();
            RaiseEvents();
        }

        void ReceivePackets()
        {
            NetworkGamer local = LocalGamer;
            if (local == null)
                return;

            for (int i = 0; i < MaxPacketsPerUpdate && local.IsDataAvailable; i++)
            {
                NetworkGamer sender;
                int length = local.ReceiveData(receiveBuffer, out sender);

                // data for everyone also comes back to the sender (as in XNA); this game already knows its own
                if (sender == null || sender.IsLocal || length <= 0)
                    continue;

                if (length >= receiveBuffer.Length)
                    Debug.WriteLine("[NET] Packet from " + sender.Gamertag + " may be truncated");

                // the library has already unwrapped the reliable channel and put the
                // payload back in order and without duplicates
                byte[] payload = new byte[length];
                Buffer.BlockCopy(receiveBuffer, 0, payload, 0, length);
                dispatcher.Dispatch(sender, payload, session.IsHost);
            }
        }

        void CheckStalledPeers()
        {
            foreach (NetworkGamer peer in session.RemoteGamers)
            {
                if (peer.UnacknowledgedAge.TotalSeconds > StallTimeout)
                {
                    if (stalledPeers.Add(peer.GamerId))
                    {
                        NetLog.Write("No answer from " + peer.Gamertag + " for " +
                            (int)StallTimeout + " seconds");
                        if (PeerStalled != null)
                            PeerStalled(peer);
                    }
                }
                else
                {
                    stalledPeers.Remove(peer.GamerId);
                }
            }
        }

        void RaiseEvents()
        {
            NetworkGamer gamer;

            while (joinedQueue.TryDequeue(out gamer))
                if (GamerJoined != null)
                    GamerJoined(gamer);

            while (leftQueue.TryDequeue(out gamer))
            {
                stalledPeers.Remove(gamer.GamerId);
                if (GamerLeft != null)
                    GamerLeft(gamer);
            }

            if (sessionEnded == 1)
            {
                sessionEnded = 2;
                if (SessionEnded != null)
                    SessionEnded();
            }
        }

        #region Session events (network thread)

        void OnGamerJoined(object sender, GamerJoinedEventArgs e)
        {
            NetworkGamer gamer = e.Gamer as NetworkGamer;
            if (gamer != null)
            {
                NetLog.Write("Gamer joined: " + gamer.Gamertag);
                joinedQueue.Enqueue(gamer);
            }
        }

        void OnGamerLeft(object sender, GamerLeftEventArgs e)
        {
            NetworkGamer gamer = e.Gamer as NetworkGamer;
            if (gamer != null)
            {
                NetLog.Write("Gamer left: " + gamer.Gamertag);
                leftQueue.Enqueue(gamer);
            }
        }

        void OnSessionEnded(object sender, NetworkSessionEndedEventArgs e)
        {
            endReason = e.EndReason.ToString();
            NetLog.Write("Session ended: " + endReason);
            System.Threading.Interlocked.CompareExchange(ref sessionEnded, 1, 0);
        }

        #endregion

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            session.GamerJoined -= OnGamerJoined;
            session.GamerLeft -= OnGamerLeft;
            session.SessionEnded -= OnSessionEnded;
            session.Dispose();
        }
    }
}
