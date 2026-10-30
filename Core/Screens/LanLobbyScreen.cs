#region Using Statements
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using VectorRumble.Networking;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// The waiting room of a LAN game: who is in, which ships they chose, who is ready. The host
    /// chooses the rules and starts the match.
    /// </summary>
    class LanLobbyScreen : MenuScreen
    {
        #region Fields

        static readonly string[] playerNames = { "One", "Two", "Three", "Four" };

        static readonly string[] asteroidDensities =
        {
            Strings.Asteroid_Density_None,
            Strings.Asteroid_Density_Low,
            Strings.Asteroid_Density_Medium,
            Strings.Asteroid_Density_High,
        };

        readonly NetworkManager net;
        LanLobby lobby;                         // null once the match has started or the player has left
        MatchStartPacket pendingStart;          // the match to start as soon as the screen can change
        string notice;                          // what to tell the player (the host cannot start yet, for instance)

        PreviewPainter previews;

        MenuEntry shipEntry, colourEntry, readyEntry;
        MenuEntry arenaEntry, asteroidEntry, scoreEntry, shootEntry, startEntry;

        #endregion

        #region Initialization

        /// <param name="net">The session of the game. The screen takes it over, and ends it if the player leaves.</param>
        public LanLobbyScreen(NetworkManager net)
        {
            this.net = net;
        }

        public override void LoadContent()
        {
            // the ships a player can choose between are loaded fresh for each game
            World.ShipManager.LoadContent(ScreenManager.Game.Content);

            // the host picks "Random" in the arena, so it is chosen among the arenas that come with the game
            string[] arenas = World.ArenaManager.BuiltInArenas
                .Select(a => a.Name).Where(n => n != Strings.Arena_Random).ToArray();

            previews = new PreviewPainter(ScreenManager.GraphicsDevice);

            lobby = new LanLobby(net, World.ShipManager.BuiltInCount, arenas, MatchRules.Current);
            lobby.MatchStarting += start => pendingStart = start;
            lobby.SetLocalColor(RandomColor());

            BuildEntries();
            base.LoadContent();
        }

        static Color RandomColor()
        {
            return new Color(ParticleSystem.random.Next(256), ParticleSystem.random.Next(256), ParticleSystem.random.Next(256));
        }

        void BuildEntries()
        {
            shipEntry = new MenuEntry(string.Empty);
            colourEntry = new MenuEntry(Strings.Lan_Change_Colour);
            readyEntry = new MenuEntry(string.Empty);
            shipEntry.Selected += (s, e) => lobby?.SetLocalShip(lobby.LocalShip + 1);
            shipEntry.Adjusted += (s, e) => lobby?.SetLocalShip(lobby.LocalShip + e.Direction);
            colourEntry.Selected += (s, e) => lobby?.SetLocalColor(RandomColor());
            readyEntry.Selected += (s, e) => lobby?.SetLocalReady(!lobby.LocalReady);
            readyEntry.Adjusted += (s, e) => lobby?.SetLocalReady(!lobby.LocalReady);

            MenuEntries.Add(shipEntry);
            MenuEntries.Add(colourEntry);
            MenuEntries.Add(readyEntry);

            if (net.IsHost)
            {
                // only the host can change the rules, which every player then plays by
                arenaEntry = new MenuEntry(string.Empty);
                asteroidEntry = new MenuEntry(string.Empty);
                scoreEntry = new MenuEntry(string.Empty);
                shootEntry = new MenuEntry(string.Empty);
                startEntry = new MenuEntry(Strings.Lan_Start_Game);

                arenaEntry.Selected += (s, e) => ChangeRules(r => ChangeArena(r, 1));
                arenaEntry.Adjusted += (s, e) => ChangeRules(r => ChangeArena(r, e.Direction));
                asteroidEntry.Selected += (s, e) => ChangeRules(r => ChangeAsteroids(r, 1));
                asteroidEntry.Adjusted += (s, e) => ChangeRules(r => ChangeAsteroids(r, e.Direction));
                scoreEntry.Selected += (s, e) => ChangeRules(r => ChangeScoreLimit(r, 1));
                scoreEntry.Adjusted += (s, e) => ChangeRules(r => ChangeScoreLimit(r, e.Direction));
                shootEntry.Selected += (s, e) => ChangeRules(r => r.ControllersCanShootInAllDirections = !r.ControllersCanShootInAllDirections);
                shootEntry.Adjusted += (s, e) => ChangeRules(r => r.ControllersCanShootInAllDirections = !r.ControllersCanShootInAllDirections);
                startEntry.Selected += StartEntrySelected;

                MenuEntries.Add(arenaEntry);
                MenuEntries.Add(asteroidEntry);
                MenuEntries.Add(scoreEntry);
                MenuEntries.Add(shootEntry);
                MenuEntries.Add(startEntry);
            }

            var leaveEntry = new MenuEntry(Strings.Lan_Leave);
            leaveEntry.Selected += OnCancel;
            MenuEntries.Add(leaveEntry);
        }

        #endregion

        #region Rules (host)

        void ChangeRules(Action<MatchRules> change)
        {
            if (lobby == null)
                return;

            MatchRules rules = lobby.Rules.Clone();
            change(rules);
            lobby.SetRules(rules);
            // the next game starts from these rules too
            MatchRules.Current = rules.Clone();
        }

        // only the arenas that come with the game, and Random: the rest exist only on this machine
        void ChangeArena(MatchRules rules, int direction)
        {
            string[] arenas = World.ArenaManager.BuiltInArenas.Select(a => a.Name).ToArray();
            int index = Array.IndexOf(arenas, rules.Arena);
            index = ((index + direction) % arenas.Length + arenas.Length) % arenas.Length;
            rules.Arena = arenas[index];
        }

        void ChangeAsteroids(MatchRules rules, int direction)
        {
            int count = asteroidDensities.Length;
            rules.AsteroidDensity = (AsteroidDensity)(((int)rules.AsteroidDensity + direction + count) % count);
        }

        void ChangeScoreLimit(MatchRules rules, int direction)
        {
            int limit = rules.ScoreLimit + direction * 5;
            if (limit > MatchRules.MaxScoreLimit)
                limit = MatchRules.MinScoreLimit;
            else if (limit < MatchRules.MinScoreLimit)
                limit = MatchRules.MaxScoreLimit;
            rules.ScoreLimit = limit;
        }

        void StartEntrySelected(object sender, EventArgs e)
        {
            notice = lobby != null && lobby.TryStart() ? null : Strings.Lan_Waiting_For_Players;
        }

        #endregion

        #region Update

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (lobby == null)
                return;

            net.Update();
            UpdateEntryText();

            if (pendingStart != null)
            {
                StartMatch(pendingStart);
            }
            else if (lobby.WasRefused)
            {
                Leave(Strings.Lan_Already_Started);
            }
            else if (net.HasEnded)
            {
                Leave(Strings.Lan_Host_Ended);
            }
        }

        void UpdateEntryText()
        {
            shipEntry.Text = string.Format(Strings.Lan_Ship, World.ShipManager.Ships[lobby.LocalShip].Name);
            readyEntry.Text = string.Format(Strings.Lan_Ready, lobby.LocalReady);

            if (net.IsHost)
            {
                MatchRules rules = lobby.Rules;
                arenaEntry.Text = string.Format(Strings.Arena, rules.Arena);
                asteroidEntry.Text = string.Format(Strings.Asteroid_Density, asteroidDensities[(int)rules.AsteroidDensity]);
                scoreEntry.Text = string.Format(Strings.Score_Limit, rules.ScoreLimit);
                shootEntry.Text = string.Format(Strings.Controllers_Can_Shoot_In_All_Directions, rules.ControllersCanShootInAllDirections);
            }
        }

        #endregion

        #region Leaving and starting

        protected override void OnCancel()
        {
            Leave(null);
        }

        // go back to the browser, ending the session
        void Leave(string message)
        {
            DisposeLobby();
            net.Dispose();
            ExitScreen();

            if (message != null)
                ScreenManager.AddScreen(new MessageBoxScreen(message));
        }

        void DisposeLobby()
        {
            lobby?.Dispose();
            lobby = null;
            pendingStart = null;
        }

        // every machine builds the same match from the host's start packet
        void StartMatch(MatchStartPacket start)
        {
            int slot = start.Players.FindIndex(p => p.Id == net.LocalGamer.GamerId);
            if (slot < 0)
            {
                Leave(Strings.Lan_Not_In_Match);
                return;
            }

            MatchRules.Current = start.Rules;

            // a ship flies for one player: if two chose the same, the later one gets the next along
            ShipManager manager = World.ShipManager;
            int[] assignment = ShipAssignment.Assign(start.Players.Select(p => (int)p.Ship).ToArray(), manager.BuiltInCount);

            foreach (Ship ship in manager.Ships)
            {
                ship.PlayerIndex = string.Empty;
                ship.Selected = false;
            }

            Ship[] ships = new Ship[start.Players.Count];
            for (int i = 0; i < ships.Length; i++)
            {
                Ship ship = manager.Ships[assignment[i]];
                ship.PlayerIndex = playerNames[i];
                ship.SetColor(start.Players[i].Color);
                ship.Selected = true;
                ships[i] = ship;
            }
            manager.UpdateSelectedList();

            // the match takes the session over from here, and ends it when the game is left
            new NetworkMatch(World, net, start, slot, ships);
            DisposeLobby();

            LoadingScreen.Load(ScreenManager, LoadGameplayScreen, true);
        }

        void LoadGameplayScreen(object sender, EventArgs e)
        {
            GameplayScreen gameplayScreen = new GameplayScreen
            {
                ScreenManager = this.ScreenManager
            };
            gameplayScreen.Initialize();
            ScreenManager.AddScreen(gameplayScreen);
        }

        #endregion

        #region Draw

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (lobby == null)
                return;

            var viewport = Display.Viewport;
            var font = ScreenManager.Font;
            var spriteBatch = ScreenManager.SpriteBatch;
            int alpha = TransitionAlpha;

            // who is in: a card for each player, with their ship in their colour
            const float cardWidth = 280f;
            const float shipSize = 84f;
            float titleY = viewport.Height * 0.03f;

            spriteBatch.Begin();
            var title = Strings.Lan_Players;
            spriteBatch.DrawString(font, title,
                new Vector2((viewport.Width - font.MeasureString(title).X) / 2f, titleY), new Color(255, 255, 255, alpha));
            spriteBatch.End();

            float shipY = titleY + font.LineSpacing + shipSize / 2f + 8f;
            for (int i = 0; i < lobby.Players.Count; i++)
            {
                LobbyPlayerInfo player = lobby.Players[i];

                // the player's own card follows their choices at once, not when the host has echoed them back
                int shipChoice = player.Ship;
                Color color = player.Color;
                bool ready = player.Ready;
                if (i == lobby.LocalSlot)
                {
                    shipChoice = lobby.LocalShip;
                    color = lobby.LocalColor;
                    ready = lobby.LocalReady;
                }

                float centreX = viewport.Width / 2f + (i - (lobby.Players.Count - 1) / 2f) * cardWidth;
                Color tint = new Color(color.R, color.G, color.B, alpha);

                if (shipChoice < World.ShipManager.BuiltInCount)
                {
                    previews.DrawShip(World.ShipManager.Ships[shipChoice], tint, new Vector2(centreX, shipY), shipSize);
                }

                spriteBatch.Begin();
                float lineY = shipY + shipSize / 2f + 8f;
                string name = player.Name;
                spriteBatch.DrawString(font, name, new Vector2(centreX - font.MeasureString(name).X / 2f, lineY), tint);
                lineY += font.LineSpacing;
                string readyText = string.Format(Strings.Lan_Ready, ready);
                spriteBatch.DrawString(font, readyText, new Vector2(centreX - font.MeasureString(readyText).X / 2f, lineY),
                    new Color(255, 255, 255, alpha));
                spriteBatch.End();
            }

            // the arena the host chose, which every player sees as the host changes it
            previews.DrawArena(ScreenManager, PreviewPainter.FindArena(lobby.Rules.Arena), PreviewPainter.ArenaAreaBeside(ScreenManager), alpha);

            spriteBatch.Begin();

            // what the host chose, or why the game cannot start
            string status = notice;
            if (status == null && !net.IsHost)
            {
                MatchRules rules = lobby.Rules;
                status = string.Format(Strings.Lan_Rules, rules.Arena, asteroidDensities[(int)rules.AsteroidDensity], rules.ScoreLimit);
            }
            if (status != null)
            {
                spriteBatch.DrawString(font, status,
                    new Vector2((viewport.Width - font.MeasureString(status).X) / 2f, viewport.Height * 0.40f),
                    new Color(255, 255, 255, alpha));
            }

            spriteBatch.End();
        }

        #endregion
    }
}
