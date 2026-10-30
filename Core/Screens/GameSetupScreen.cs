#region Using Statements
using System;
using System.Linq;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// Where the rules of the match are chosen, before the players pick their ships.
    /// Left and right step a value back and forth; selecting it steps it forward.
    /// </summary>
    class GameSetupScreen : MenuScreen
    {
        #region Fields

        static readonly string[] asteroidDensities =
        {
            Strings.Asteroid_Density_None,
            Strings.Asteroid_Density_Low,
            Strings.Asteroid_Density_Medium,
            Strings.Asteroid_Density_High,
        };

        const int MinScoreLimit = 5;
        const int MaxScoreLimit = 25;
        const int ScoreLimitStep = 5;

        readonly MenuEntry arenaMenuEntry = new MenuEntry(String.Empty);
        readonly MenuEntry asteroidDensityMenuEntry = new MenuEntry(String.Empty);
        readonly MenuEntry scoreLimitMenuEntry = new MenuEntry(String.Empty);
        readonly MenuEntry shootInAllDirectionsMenuEntry = new MenuEntry(String.Empty);
        readonly MenuEntry startMenuEntry = new MenuEntry(Strings.Play_Game);

        PreviewPainter previews;

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor fills in the menu contents. The text of the entries that hold a
        /// value is filled in by Update.
        /// </summary>
        public GameSetupScreen()
        {
            arenaMenuEntry.Selected += (s, e) => ChangeArena(1);
            arenaMenuEntry.Adjusted += (s, e) => ChangeArena(e.Direction);
            asteroidDensityMenuEntry.Selected += (s, e) => ChangeAsteroidDensity(1);
            asteroidDensityMenuEntry.Adjusted += (s, e) => ChangeAsteroidDensity(e.Direction);
            scoreLimitMenuEntry.Selected += (s, e) => ChangeScoreLimit(1);
            scoreLimitMenuEntry.Adjusted += (s, e) => ChangeScoreLimit(e.Direction);
            shootInAllDirectionsMenuEntry.Selected += (s, e) => ToggleShootInAllDirections();
            shootInAllDirectionsMenuEntry.Adjusted += (s, e) => ToggleShootInAllDirections();
            startMenuEntry.Selected += StartMenuEntrySelected;

            MenuEntries.Add(arenaMenuEntry);
            MenuEntries.Add(asteroidDensityMenuEntry);
            MenuEntries.Add(scoreLimitMenuEntry);
            MenuEntries.Add(shootInAllDirectionsMenuEntry);
            MenuEntries.Add(startMenuEntry);
        }

        public override void LoadContent()
        {
            previews = new PreviewPainter(ScreenManager.GraphicsDevice);
            base.LoadContent();
        }

        #endregion

        #region Handle Input

        /// <summary>
        /// Updates the screen, filling in the latest values for the menu text.
        /// </summary>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime, bool otherScreenHasFocus,
                                                                               bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            MatchRules rules = MatchRules.Current;
            arenaMenuEntry.Text = string.Format(Strings.Arena, rules.Arena);
            asteroidDensityMenuEntry.Text = string.Format(Strings.Asteroid_Density, asteroidDensities[(int)rules.AsteroidDensity]);
            scoreLimitMenuEntry.Text = string.Format(Strings.Score_Limit, rules.ScoreLimit);
            shootInAllDirectionsMenuEntry.Text = string.Format(Strings.Controllers_Can_Shoot_In_All_Directions, rules.ControllersCanShootInAllDirections);
        }

        /// <summary>
        /// Draws the menu, with a small picture of the chosen arena beside it.
        /// </summary>
        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Draw(gameTime);

            previews.DrawArena(ScreenManager, PreviewPainter.FindArena(MatchRules.Current.Arena),
                PreviewPainter.ArenaAreaAbove, TransitionAlpha);
        }

        void ChangeArena(int direction)
        {
            string[] arenas = World.ArenaManager.Arenas.Select(a => a.Name).ToArray();
            int index = Array.IndexOf(arenas, MatchRules.Current.Arena);
            index = ((index + direction) % arenas.Length + arenas.Length) % arenas.Length;
            MatchRules.Current.Arena = arenas[index];
        }

        void ChangeAsteroidDensity(int direction)
        {
            int count = asteroidDensities.Length;
            int index = ((int)MatchRules.Current.AsteroidDensity + direction + count) % count;
            MatchRules.Current.AsteroidDensity = (AsteroidDensity)index;
        }

        void ChangeScoreLimit(int direction)
        {
            int limit = MatchRules.Current.ScoreLimit + direction * ScoreLimitStep;
            if (limit > MaxScoreLimit)
                limit = MinScoreLimit;
            else if (limit < MinScoreLimit)
                limit = MaxScoreLimit;
            MatchRules.Current.ScoreLimit = limit;
        }

        void ToggleShootInAllDirections()
        {
            MatchRules.Current.ControllersCanShootInAllDirections = !MatchRules.Current.ControllersCanShootInAllDirections;
        }

        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void StartMenuEntrySelected(object sender, EventArgs e)
        {
            var shipSelection = new ShipSelectionScreen {
                ScreenManager = this.ScreenManager
            };
            shipSelection.Initialize();
            ScreenManager.AddScreen(shipSelection);
        }

        #endregion
    }
}
