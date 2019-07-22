#region File Description
//-----------------------------------------------------------------------------
// OptionsMenuScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Linq;
using Microsoft.Xna.Framework;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// The options screen is brought up over the top of the main menu
    /// screen, and gives the user a chance to configure the game
    /// in various hopefully useful ways.
    /// </summary>
    /// <remarks>
    /// This class is similar to one of the same name in the GameStateManagement sample.
    /// </remarks>
    class OptionsMenuScreen : MenuScreen
    {
        #region Fields


		static string[] asteroidDensity = { 
				Strings.Asteroid_Density_None,
				Strings.Asteroid_Density_Low,
				Strings.Asteroid_Density_Medium,
				Strings.Asteroid_Density_High,
			};
        static int currentAsteroidDensity = 2;

        static string[] arenas => World.ArenaManager.Arenas.Select(a => a.Name).ToArray();
        static int blurIntensity = 5;
        static bool controllersCanShootInAllDirections;
        static string currentArena = string.Empty;
        static int currentArenaIndex;
        static bool motionBlur = true;
        static bool neonEffect = true;
        static int scoreLimit = 10;

        MenuEntry arenaMenuEntry = new MenuEntry(String.Empty);
        MenuEntry asteroidDensityMenuEntry = new MenuEntry(String.Empty);
        MenuEntry blurIntensityMenuEntry = new MenuEntry(String.Empty);
        MenuEntry controllersCanShootInAllDirectionsMenuEntry = new MenuEntry(String.Empty);
        MenuEntry fullScreenMenuEntry = new MenuEntry(String.Empty);
        MenuEntry motionBlurMenuEntry = new MenuEntry(String.Empty);
        MenuEntry neonEffectMenuEntry = new MenuEntry(String.Empty);
        MenuEntry scoreLimitMenuEntry = new MenuEntry(String.Empty);

        GraphicsDeviceManager gdm;
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor populates the menu with empty strings: the real values
        /// are filled in by the Update method to reflect the changing settings.
        /// </summary>
        public OptionsMenuScreen()
        {
            blurIntensity = WorldRules.BlurIntensity;
            controllersCanShootInAllDirections = WorldRules.ControllersCanShootInAllDirections;
            currentArena = WorldRules.DefaultArena;
            currentArenaIndex = Array.IndexOf(arenas, currentArena);
            currentAsteroidDensity = (int)WorldRules.AsteroidDensity;
            motionBlur = WorldRules.MotionBlur;
            neonEffect = WorldRules.NeonEffect;
            scoreLimit = WorldRules.ScoreLimit;

            asteroidDensityMenuEntry.Selected += AsteroidDensityMenuEntrySelected;
            arenaMenuEntry.Selected += ArenaMenuEntrySelected;
            blurIntensityMenuEntry.Selected += BlurIntensityMenuEntrySelected;
            controllersCanShootInAllDirectionsMenuEntry.Selected += ControllersCanShootInAllDirectionsMenuEntrySelected;
            fullScreenMenuEntry.Selected += FullScreenMenuEntrySelected;
            motionBlurMenuEntry.Selected += MotionBlurMenuEntrySelected;
            neonEffectMenuEntry.Selected += NeonEffectMenuEntrySelected;
            scoreLimitMenuEntry.Selected += ScoreLimitMenuEntrySelected;

            MenuEntries.Add(arenaMenuEntry);
            MenuEntries.Add(asteroidDensityMenuEntry);
            MenuEntries.Add(blurIntensityMenuEntry);
            MenuEntries.Add(controllersCanShootInAllDirectionsMenuEntry);
#if !USE_DEFAULT_DEVICE_SETTINGS
            MenuEntries.Add(fullScreenMenuEntry);
#endif
            MenuEntries.Add(motionBlurMenuEntry);
            MenuEntries.Add(neonEffectMenuEntry);
            MenuEntries.Add(scoreLimitMenuEntry);
        }
        #endregion

        #region Handle Input


        /// <summary>
        /// Updates the options screen, filling in the latest values for the menu text.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            arenaMenuEntry.Text = string.Format(Strings.Arena, currentArena);
            asteroidDensityMenuEntry.Text = string.Format (Strings.Asteroid_Density, asteroidDensity[currentAsteroidDensity]);
            blurIntensityMenuEntry.Text = string.Format(Strings.Blur_Intesity, blurIntensity);
            controllersCanShootInAllDirectionsMenuEntry.Text = string.Format(Strings.Controllers_Can_Shoot_In_All_Directions, controllersCanShootInAllDirections);
            motionBlurMenuEntry.Text = string.Format (Strings.Motion_Blur,motionBlur);
			neonEffectMenuEntry.Text = string.Format (Strings.Neon_Effect ,neonEffect);
            scoreLimitMenuEntry.Text = string.Format(Strings.Score_Limit, scoreLimit);
#if !USE_DEFAULT_DEVICE_SETTINGS
            if (gdm == null) {
				gdm = ScreenManager.Game.Services.GetService<GraphicsDeviceManager> ();
			}
			fullScreenMenuEntry.Text = string.Format (Strings.Display_Mode, !gdm.IsFullScreen ? Strings.Windowed : Strings.FullScreen);
#endif
        }


        /// <summary>
        /// Event handler for when the Score Limit menu entry is selected.
        /// </summary>
        void ScoreLimitMenuEntrySelected(object sender, EventArgs e)
        {
            scoreLimit += 5;
            if (scoreLimit > 25)
                scoreLimit = 5;
        }

        /// <summary>
        /// Event handler for when the Score Limit menu entry is selected.
        /// </summary>
        void BlurIntensityMenuEntrySelected(object sender, EventArgs e)
        {
            blurIntensity += 1;
            if (blurIntensity > 10)
                blurIntensity = 1;
        }

        /// <summary>
        /// Event handler for when the Asteroid Density menu entry is selected.
        /// </summary>
        void AsteroidDensityMenuEntrySelected(object sender, EventArgs e)
        {
            currentAsteroidDensity = (currentAsteroidDensity + 1) %
                asteroidDensity.Length;
        }


        /// <summary>
        /// Event handler for when the Wall Style menu entry is selected.
        /// </summary>
        void ArenaMenuEntrySelected(object sender, EventArgs e)
        {
            currentArenaIndex = (currentArenaIndex + 1) % arenas.Length;
            currentArena = arenas[currentArenaIndex];
        }


        /// <summary>
        /// Event handler for when the Motion Blur menu entry is selected.
        /// </summary>
        void MotionBlurMenuEntrySelected(object sender, EventArgs e)
        {
            motionBlur = !motionBlur;
        }


        /// <summary>
        /// Event handler for when the NeonEffect menu entry is selected.
        /// </summary>
        void NeonEffectMenuEntrySelected(object sender, EventArgs e)
        {
            neonEffect = !neonEffect;
        }

        void FullScreenMenuEntrySelected (object sender, EventArgs e)
        {
			gdm.ToggleFullScreen ();
        }

        private void ControllersCanShootInAllDirectionsMenuEntrySelected(object sender, EventArgs e)
        {
            controllersCanShootInAllDirections = !controllersCanShootInAllDirections;
        }

        /// <summary>
        /// When the user cancels the options screen, go back to the main menu.
        /// </summary>
        protected override void OnCancel()
        {
            WorldRules.AsteroidDensity =  (AsteroidDensity)Enum.Parse(typeof(AsteroidDensity), asteroidDensity[currentAsteroidDensity], true);
            WorldRules.BlurIntensity = blurIntensity;
            WorldRules.ControllersCanShootInAllDirections = controllersCanShootInAllDirections;
            WorldRules.MotionBlur = motionBlur;
            WorldRules.DefaultArena = currentArena;
            WorldRules.NeonEffect = neonEffect;
            WorldRules.ScoreLimit = scoreLimit;

            ExitScreen();
        }


#endregion
    }
}
