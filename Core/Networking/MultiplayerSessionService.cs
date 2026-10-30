using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Net;

namespace VectorRumble.Networking
{
    /// <summary>
    /// Creates, finds and joins LAN (UDP / SystemLink) sessions.
    /// </summary>
    public static class MultiplayerSessionService
    {
        // Bump when packet formats change so mismatched builds do not join each other.
        public const int ProtocolVersion = 1;

        /// <summary>The most players in one match.</summary>
        public const int MaxPlayers = 4;

        const string GameName = "VectorRumble";
        const string GamePropertyKey = "Game";
        const string ProtocolPropertyKey = "Protocol";
        const string InProgressPropertyKey = "InProgress";

        /// <summary>
        /// Select the default UDP / SystemLink transport, and keep this game's sessions apart
        /// from other games that use the same library. Call once at start-up.
        /// </summary>
        public static void Initialize()
        {
            NetworkSession.GameId = GameName;
            NetworkServiceProvider.ResetToDefault();
        }

        public static async Task<NetworkSession> CreateHostSessionAsync(int maxPlayers)
        {
            Debug.WriteLine("[NET] Hosting session for up to " + maxPlayers + " players");

            // one local gamer per machine (a LAN match is one player per machine)
            NetworkSession session = await NetworkSession.CreateAsync(
                NetworkSessionType.SystemLink, 1, maxPlayers, 0, null);

            // CreateAsync does not copy the properties it is given into the session,
            // so they are set here; the advertisement picks them up
            session.SessionProperties[GamePropertyKey] = GameName;
            session.SessionProperties[ProtocolPropertyKey] = ProtocolVersion;

            // added now (not later) because the advertisement reads the properties on another
            // thread, and changing the value of an existing key is safe while adding a key is not
            session.SessionProperties[InProgressPropertyKey] = false;

            return session;
        }

        /// <summary>
        /// Host: stop advertising the session as joinable once the match has started
        /// </summary>
        public static void MarkInProgress(NetworkSession session)
        {
            session.SessionProperties[InProgressPropertyKey] = true;
        }

        public static Task<AvailableNetworkSessionCollection> FindSessionsAsync()
        {
            Debug.WriteLine("[NET] Searching for sessions");
            return NetworkSession.FindAsync(NetworkSessionType.SystemLink, 1, null);
        }

        /// <summary>
        /// A session at a known address, for joining without discovery. The Net library
        /// plays gameplay traffic on port 31338.
        /// </summary>
        /// <remarks>
        /// Nothing calls this yet. It is kept for the "Enter address..." entry on the LAN browser,
        /// which is left out for now because typing an address needs a text-entry story on touch
        /// devices (see the LAN browser screen when it is added).
        /// </remarks>
        public static AvailableNetworkSession DirectSession(IPAddress address)
        {
            return new AvailableNetworkSession(
                "SystemLinkSession", "host", 1, MaxPlayers - 1, 0,
                NetworkSessionType.SystemLink, new Dictionary<string, object>(),
                "direct-" + address, UdpTransport.ToPeer(new IPEndPoint(address, 31338)));
        }

        public static Task<NetworkSession> JoinSessionAsync(AvailableNetworkSession session)
        {
            Debug.WriteLine("[NET] Joining session " + session.SessionId);
            return NetworkSession.JoinAsync(session);
        }

        /// <summary>
        /// True if an advertised session is a VectorRumble session we can talk to
        /// </summary>
        public static bool IsCompatible(AvailableNetworkSession session)
        {
            object game, protocol;
            if (session.SessionProperties == null ||
                !session.SessionProperties.TryGetValue(GamePropertyKey, out game) ||
                !session.SessionProperties.TryGetValue(ProtocolPropertyKey, out protocol))
                return false;

            if (!GameName.Equals(game) || !(protocol is int) || (int)protocol != ProtocolVersion)
                return false;

            // a match that has started cannot be joined
            object inProgress;
            return !(session.SessionProperties.TryGetValue(InProgressPropertyKey, out inProgress) &&
                     inProgress is bool && (bool)inProgress);
        }
    }
}
