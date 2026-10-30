using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Net;

namespace VectorRumble.Networking
{
    /// <summary>
    /// Keeps the ships of a LAN match in step. Every machine simulates its own ship
    /// and sends its state about 20 times a second; this applies the states received
    /// for the other ships through <see cref="RemoteShipInterpolator"/>.
    /// 
    /// The host passes each client's state on to the other clients and checks that a
    /// client only reports its own ship.
    /// </summary>
    internal sealed class ShipStateSync : IDisposable
    {
        // seconds between state packets
        public const double SendInterval = 0.05;

        readonly NetworkManager network;
        readonly string[] slotIds;                  // network gamer id for each player slot
        readonly int localSlot;
        readonly RemoteShipInterpolator[] interpolators;

        double clock;                               // game time in seconds
        double sendTimer;
        ushort sequence;

        public ShipStateSync(NetworkManager network, IList<string> slotIds, int localSlot)
        {
            this.network = network;
            this.slotIds = new string[slotIds.Count];
            slotIds.CopyTo(this.slotIds, 0);
            this.localSlot = localSlot;

            interpolators = new RemoteShipInterpolator[this.slotIds.Length];
            for (int i = 0; i < interpolators.Length; i++)
                interpolators[i] = new RemoteShipInterpolator();

            network.Dispatcher.Register(PacketType.ShipState, PacketAudience.Any, OnShipState);
        }

        /// <summary>
        /// Packets from a client that claimed another player's slot
        /// </summary>
        public int SpoofedCount { get; private set; }

        /// <summary>
        /// Move the remote ships to where the network says they are. <paramref name="ships"/> is
        /// indexed by player slot.
        /// </summary>
        public void ApplyRemoteStates(float elapsedTime, Ship[] ships)
        {
            clock += elapsedTime;

            for (int slot = 0; slot < interpolators.Length && slot < ships.Length; slot++)
            {
                if (slot == localSlot || ships[slot] == null || ships[slot].Dead ||
                    !interpolators[slot].HasState)
                    continue;

                interpolators[slot].Sample(clock, out var position, out var rotation,
                    out var velocity, out var shield);
                ships[slot].ApplyRemoteState(position, rotation, velocity, shield);
            }
        }

        /// <summary>
        /// Send the local ship's state when it is due
        /// </summary>
        public void SendLocalState(float elapsedTime, Ship localShip)
        {
            sendTimer += elapsedTime;
            if (sendTimer < SendInterval)
                return;

            // keep the rhythm without bursting after a slow frame
            sendTimer = Math.Min(sendTimer - SendInterval, SendInterval);

            if (localShip == null || !localShip.Playing || localShip.Dead || network.HasEnded)
                return;

            network.Broadcast(localShip.CaptureState(localSlot, sequence++), false);
        }

        void OnShipState(NetworkGamer sender, BinaryReader reader)
        {
            ShipStatePacket state = ShipStatePacket.Deserialize(reader);

            if (state.Slot >= slotIds.Length || state.Slot == localSlot)
                return;

            if (network.IsHost)
            {
                if (slotIds[state.Slot] != sender.GamerId)
                {
                    SpoofedCount++;
                    return;
                }

                network.Relay(state, false, sender);
            }

            interpolators[state.Slot].Add(state, clock);
        }

        public void Dispose()
        {
            network.Dispatcher.Unregister(PacketType.ShipState);
        }
    }
}
