using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;

namespace VectorRumble.Networking
{
    /// <summary>
    /// The lobby state for a LAN game. The host owns it: clients tell the host what they
    /// want (ship, colour, ready) and the host sends everybody the resulting lobby and the
    /// rules of the match. Slot order (host first, then in joining order) becomes the player
    /// index in the match.
    /// 
    /// This class has no UI so it can be tested without the game.
    /// </summary>
    public sealed class LanLobby : IDisposable
    {
        readonly NetworkManager net;
        readonly int shipCount;
        readonly IList<string> arenas;
        readonly List<LobbyPlayerInfo> players = new List<LobbyPlayerInfo>();
        MatchRules rules;
        bool disposed;

        // this player's own choices; what we send is always based on these, never on the
        // lobby the host last echoed back (which lags behind quick changes)
        byte localShip;
        Color localColor = Color.White;
        bool localReady;

        // raised from NetworkManager.Update (game thread)
        public event Action Changed;
        public event Action<MatchStartPacket> MatchStarting;
        public event Action JoinRefused;                    // we arrived after the match started

        /// <summary>
        /// True once the host has told us we arrived after the match started (the event can fire
        /// while no screen is listening, for instance during ship selection)
        /// </summary>
        public bool WasRefused { get; private set; }

        /// <param name="shipCount">How many ships a player can choose between</param>
        /// <param name="arenas">The names of the arenas the host can pick for a "Random" match (not "Random" itself)</param>
        /// <param name="rules">The rules to start with (the host's; a client's are replaced by the host's)</param>
        public LanLobby(NetworkManager net, int shipCount, IList<string> arenas, MatchRules rules)
        {
            this.net = net;
            this.shipCount = shipCount;
            this.arenas = arenas;
            this.rules = rules.Clone();

            if (net.IsHost)
            {
                net.Dispatcher.Register(PacketType.LobbyUpdate, PacketAudience.FromClient, OnLobbyUpdate);
                net.GamerJoined += OnGamerJoined;
                net.GamerLeft += OnGamerLeft;

                AddPlayer(net.LocalGamer);
                foreach (NetworkGamer gamer in net.Session.RemoteGamers)
                    AddPlayer(gamer);
            }
            else
            {
                net.Dispatcher.Register(PacketType.LobbySync, PacketAudience.FromHost, OnLobbySync);
                net.Dispatcher.Register(PacketType.MatchStart, PacketAudience.FromHost, OnMatchStart);
                net.Dispatcher.Register(PacketType.JoinRefused, PacketAudience.FromHost, OnJoinRefused);

                // let the host know who we are
                SendLocalChoice();
            }
        }

        public IList<LobbyPlayerInfo> Players
        {
            get { return players; }
        }

        /// <summary>
        /// The rules of the match. The host sets them with <see cref="SetRules"/>; a client sees the host's.
        /// </summary>
        public MatchRules Rules
        {
            get { return rules; }
        }

        /// <summary>
        /// Slot (player index) of this machine, or -1 before the host has told us
        /// </summary>
        public int LocalSlot
        {
            get
            {
                string id = net.LocalGamer.GamerId;
                for (int i = 0; i < players.Count; i++)
                    if (players[i].Id == id)
                        return i;
                return -1;
            }
        }

        public int LocalShip
        {
            get { return localShip; }
        }

        public Color LocalColor
        {
            get { return localColor; }
        }

        public bool LocalReady
        {
            get { return localReady; }
        }

        public LobbyPlayerInfo LocalPlayer
        {
            get
            {
                int slot = LocalSlot;
                return slot < 0 ? null : players[slot];
            }
        }

        public bool IsHost
        {
            get { return net.IsHost; }
        }

        /// <summary>
        /// The host can start once at least two people are in and all of them are ready
        /// </summary>
        public bool CanStart
        {
            get
            {
                if (!net.IsHost || players.Count < 2)
                    return false;
                foreach (LobbyPlayerInfo player in players)
                    if (!player.Ready)
                        return false;
                return true;
            }
        }

        #region Local actions

        public void SetLocalShip(int ship)
        {
            localShip = (byte)((ship % shipCount + shipCount) % shipCount);
            ApplyLocalChoice();
        }

        public void SetLocalColor(Color color)
        {
            localColor = color;
            ApplyLocalChoice();
        }

        public void SetLocalReady(bool ready)
        {
            localReady = ready;
            ApplyLocalChoice();
        }

        void ApplyLocalChoice()
        {
            if (net.IsHost)
            {
                LobbyPlayerInfo me = LocalPlayer;
                if (me == null)
                    return;
                me.Ship = localShip;
                me.Color = localColor;
                me.Ready = localReady;
                SyncAll();
            }
            else
            {
                SendLocalChoice();
            }
        }

        void SendLocalChoice()
        {
            net.SendToHost(new LobbyUpdatePacket { Ship = localShip, Color = localColor, Ready = localReady }, true);
        }

        /// <summary>
        /// Host only: change the rules of the match
        /// </summary>
        public void SetRules(MatchRules newRules)
        {
            if (!net.IsHost)
                return;

            rules = newRules.Clone();
            SyncAll();
        }

        /// <summary>
        /// Host only: begin the match on every machine
        /// </summary>
        public bool TryStart()
        {
            if (!CanStart)
                return false;

            // every machine plays the same arena, so a "Random" one is picked here, once
            MatchRules matchRules = rules.Clone();
            if (matchRules.IsRandomArena && arenas.Count > 0)
                matchRules.Arena = arenas[new Random().Next(arenas.Count)];

            MatchStartPacket start = new MatchStartPacket
            {
                Rules = matchRules,
                Seed = new Random().Next(),
                Players = new List<LobbyPlayerInfo>(players),
            };

            // the library refuses new players once the session is playing; the property hides
            // the game from other players' lists (the library keeps advertising it)
            net.Session.AllowJoinInProgress = false;
            MultiplayerSessionService.MarkInProgress(net.Session);
            net.Session.StartGame();
            net.Broadcast(start, true);

            if (MatchStarting != null)
                MatchStarting(start);
            return true;
        }

        #endregion

        void OnJoinRefused(NetworkGamer sender, BinaryReader reader)
        {
            WasRefused = true;
            if (JoinRefused != null)
                JoinRefused();
        }

        #region Host

        void AddPlayer(NetworkGamer gamer)
        {
            if (gamer == null || FindById(gamer.GamerId) != null || players.Count >= MultiplayerSessionService.MaxPlayers)
                return;

            players.Add(new LobbyPlayerInfo
            {
                Id = gamer.GamerId,
                Name = DisplayName(gamer.Gamertag),
                Ship = 0,
                Color = Color.White,
                Ready = false,
            });
        }

        void OnGamerJoined(NetworkGamer gamer)
        {
            AddPlayer(gamer);
            SyncAll();
        }

        void OnGamerLeft(NetworkGamer gamer)
        {
            LobbyPlayerInfo left = FindById(gamer.GamerId);
            if (left != null)
            {
                players.Remove(left);
                SyncAll();
            }
        }

        void OnLobbyUpdate(NetworkGamer sender, BinaryReader reader)
        {
            LobbyUpdatePacket update = LobbyUpdatePacket.Deserialize(reader);

            LobbyPlayerInfo player = FindById(sender.GamerId);
            if (player == null)
                return;

            player.Ship = (byte)(update.Ship % shipCount);
            player.Color = update.Color;
            player.Ready = update.Ready;
            SyncAll();
        }

        void SyncAll()
        {
            net.Broadcast(new LobbySyncPacket
            {
                Rules = rules,
                Players = new List<LobbyPlayerInfo>(players),
            }, true);

            if (Changed != null)
                Changed();
        }

        #endregion

        #region Client

        void OnLobbySync(NetworkGamer sender, BinaryReader reader)
        {
            LobbySyncPacket sync = LobbySyncPacket.Deserialize(reader);

            players.Clear();
            players.AddRange(sync.Players);
            rules = sync.Rules;

            if (Changed != null)
                Changed();
        }

        void OnMatchStart(NetworkGamer sender, BinaryReader reader)
        {
            MatchStartPacket start = MatchStartPacket.Deserialize(reader);
            if (MatchStarting != null)
                MatchStarting(start);
        }

        #endregion

        LobbyPlayerInfo FindById(string id)
        {
            foreach (LobbyPlayerInfo player in players)
                if (player.Id == id)
                    return player;
            return null;
        }

        /// <summary>
        /// Gamertags look like "name_1a2b3c4d"; show just the name
        /// </summary>
        public static string DisplayName(string gamertag)
        {
            if (string.IsNullOrEmpty(gamertag))
                return "Player";
            return Regex.Replace(gamertag, "_[0-9a-fA-F]{8}$", "");
        }

        public void Dispose()
        {
            if (disposed)
                return;

            disposed = true;
            net.Dispatcher.Unregister(PacketType.LobbyUpdate);
            net.Dispatcher.Unregister(PacketType.LobbySync);
            net.Dispatcher.Unregister(PacketType.MatchStart);
            net.Dispatcher.Unregister(PacketType.JoinRefused);
            net.GamerJoined -= OnGamerJoined;
            net.GamerLeft -= OnGamerLeft;
        }
    }
}
