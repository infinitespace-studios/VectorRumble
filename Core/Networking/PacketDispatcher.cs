using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Net;

namespace VectorRumble.Networking
{
    /// <summary>
    /// Who a packet type may come from. Replaces the per-handler host/client checks
    /// scattered through the handlers.
    /// </summary>
    public enum PacketAudience
    {
        Any,            // accept from anybody
        FromHost,       // we are a client and the host sent it
        FromClient,     // we are the host and a client sent it
    }

    /// <summary>
    /// Routes received packets to the handler registered for their type, dropping
    /// packets from the wrong side and logging (not swallowing) malformed ones.
    /// </summary>
    public sealed class PacketDispatcher
    {
        struct Entry
        {
            public PacketAudience Audience;
            public Action<NetworkGamer, BinaryReader> Handler;
        }

        readonly Dictionary<PacketType, Entry> handlers = new Dictionary<PacketType, Entry>();

        public int ErrorCount { get; private set; }
        public int RejectedCount { get; private set; }
        public int UnknownCount { get; private set; }

        public void Register(PacketType type, PacketAudience audience,
            Action<NetworkGamer, BinaryReader> handler)
        {
            handlers[type] = new Entry { Audience = audience, Handler = handler };
        }

        public void Unregister(PacketType type)
        {
            handlers.Remove(type);
        }

        public void Clear()
        {
            handlers.Clear();
        }

        /// <summary>
        /// Dispatch one payload (type byte followed by the packet body)
        /// </summary>
        internal void Dispatch(NetworkGamer sender, byte[] payload, bool localIsHost)
        {
            if (payload.Length == 0)
                return;

            PacketType type = (PacketType)payload[0];

            Entry entry;
            if (!handlers.TryGetValue(type, out entry))
            {
                UnknownCount++;
                Debug.WriteLine("[NET] No handler for packet type " + payload[0]);
                return;
            }

            bool accepted =
                entry.Audience == PacketAudience.Any ||
                (entry.Audience == PacketAudience.FromHost && !localIsHost && sender != null && sender.IsHost) ||
                (entry.Audience == PacketAudience.FromClient && localIsHost);

            if (!accepted)
            {
                RejectedCount++;
                Debug.WriteLine("[NET] Rejected " + type + " from " + (sender == null ? "?" : sender.Gamertag));
                return;
            }

            try
            {
                using (MemoryStream stream = new MemoryStream(payload, 1, payload.Length - 1, false))
                using (BinaryReader reader = new BinaryReader(stream))
                    entry.Handler(sender, reader);
            }
            catch (Exception ex)
            {
                // a bad packet must not take the game down, but it must not vanish either
                ErrorCount++;
                Debug.WriteLine("[NET] Error handling " + type + " from " +
                    (sender == null ? "?" : sender.Gamertag) + ": " +
                    ex.GetType().Name + " " + ex.Message);
            }
        }
    }
}
