#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using VectorRumble.Networking;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// Lists the LAN games on the network and lets the player host or join one.
    /// </summary>
    class LanBrowserScreen : MenuScreen
    {
        #region Fields

        // seconds between searches for games
        const float SearchInterval = 3f;

        readonly List<AvailableNetworkSession> sessions = new List<AvailableNetworkSession>();

        // a host or join in progress (null when idle)
        Task<NetworkSession> sessionTask;
        string busyText;

        // a search for games in progress (null when idle)
        Task<AvailableNetworkSessionCollection> findTask;
        float searchTimer;

        // why the last host or join failed
        string errorText;

        // what is listed now, to tell when the list has to be rebuilt
        string listedKey;

        #endregion

        #region Initialization

        public LanBrowserScreen()
        {
            RebuildEntries();
        }

        #endregion

        #region Entries

        // the entries are: host, a join for each game found, back
        int EntryHost => 0;
        int EntryBack => sessions.Count + 1;

        void RebuildEntries()
        {
            int selected = SelectedEntry;
            MenuEntries.Clear();

            var host = new MenuEntry(Strings.Lan_Host_A_Game);
            host.Selected += (s, e) => Host();
            MenuEntries.Add(host);

            foreach (AvailableNetworkSession session in sessions)
            {
                AvailableNetworkSession joining = session;
                var join = new MenuEntry(string.Format(Strings.Lan_Join_Game, LanLobby.DisplayName(session.HostGamertag)));
                join.Selected += (s, e) => Join(joining);
                MenuEntries.Add(join);
            }

            // An "Enter address..." entry would go here, to join a game whose address the player
            // types in, for when the game cannot be found by searching.
            // It is left out for now: typing an address needs a text-entry screen that works with a
            // gamepad and with touch, which the game does not have yet. When it does, add the entry
            // and make it call
            //     Join(MultiplayerSessionService.DirectSession(address));
            // MultiplayerSessionService.DirectSession is kept for it.
            //
            // var address = new MenuEntry(Strings.Lan_Enter_Address);
            // address.Selected += (s, e) => { /* ask for the address, then Join(...) */ };
            // MenuEntries.Add(address);

            var back = new MenuEntry(Strings.Back);
            back.Selected += OnCancel;
            MenuEntries.Add(back);

            SelectedEntry = selected;
            listedKey = ListKey();
        }

        string ListKey() => string.Join("|", sessions.Select(s => s.SessionId));

        #endregion

        #region Host and join

        bool Busy => sessionTask != null;

        void Host()
        {
            if (Busy)
                return;

            errorText = null;
            busyText = Strings.Lan_Starting_Game;
            sessionTask = MultiplayerSessionService.CreateHostSessionAsync(MultiplayerSessionService.MaxPlayers);
        }

        void Join(AvailableNetworkSession session)
        {
            if (Busy)
                return;

            errorText = null;
            busyText = string.Format(Strings.Lan_Joining, LanLobby.DisplayName(session.HostGamertag));
            sessionTask = MultiplayerSessionService.JoinSessionAsync(session);
        }

        void FinishSessionTask()
        {
            Task<NetworkSession> task = sessionTask;
            sessionTask = null;
            busyText = null;

            if (task.IsFaulted || task.IsCanceled)
            {
                errorText = string.Format(Strings.Lan_Could_Not_Connect,
                    task.Exception?.GetBaseException().Message ?? "cancelled");
                NetLog.Write("Host or join failed: " + errorText);
                return;
            }

            NetworkSession session = task.Result;
            if (IsExiting)
            {
                // the player left while it was connecting
                session.Dispose();
                return;
            }

            var lobby = new LanLobbyScreen(new NetworkManager(session));
            ScreenManager.AddScreen(lobby);
        }

        #endregion

        #region Search

        void UpdateSearch(float elapsedTime)
        {
            if (findTask != null)
            {
                if (!findTask.IsCompleted)
                    return;

                Task<AvailableNetworkSessionCollection> task = findTask;
                findTask = null;
                searchTimer = SearchInterval;

                sessions.Clear();
                if (!task.IsFaulted && !task.IsCanceled)
                {
                    foreach (AvailableNetworkSession session in task.Result)
                    {
                        if (MultiplayerSessionService.IsCompatible(session))
                            sessions.Add(session);
                    }
                }

                if (ListKey() != listedKey)
                    RebuildEntries();
                return;
            }

            searchTimer -= elapsedTime;
            if (searchTimer <= 0f)
            {
                searchTimer = SearchInterval;
                findTask = MultiplayerSessionService.FindSessionsAsync();
            }
        }

        #endregion

        #region Update and Draw

        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (sessionTask != null && sessionTask.IsCompleted)
                FinishSessionTask();

            // look for games only while this screen is the one in front
            if (!otherScreenHasFocus && !coveredByOtherScreen && !IsExiting && !Busy)
                UpdateSearch((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            string status = busyText ?? errorText;
            if (status == null)
                status = findTask != null ? Strings.Lan_Searching : (sessions.Count == 0 ? Strings.Lan_No_Games : null);

            if (status != null)
            {
                var viewport = Display.Viewport;
                Vector2 size = ScreenManager.Font.MeasureString(status);
                Vector2 position = new Vector2((viewport.Width - size.X) / 2f, viewport.Height * 0.15f);
                Color color = new Color(255, 255, 255, (int)TransitionAlpha);

                ScreenManager.SpriteBatch.Begin();
                ScreenManager.SpriteBatch.DrawString(ScreenManager.Font, status, position, color);
                ScreenManager.SpriteBatch.End();
            }
        }

        #endregion
    }
}
