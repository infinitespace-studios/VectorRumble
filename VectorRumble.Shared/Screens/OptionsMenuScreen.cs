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

        static string[] wallStyle = { 
			Strings.Wall_Style_None,
			Strings.Wall_Style_One,
			Strings.Wall_Style_Two,
			Strings.Wall_Style_Three,
			Strings.Wall_Style_Random,
		};
        static int currentWallStyle = 0;

        static int scoreLimit = 10;
        static bool motionBlur = true;
		static int blurIntensity = 5;
        static bool neonEffect = true;

        MenuEntry scoreLimitMenuEntry = new MenuEntry(String.Empty);
        MenuEntry asteroidDensityMenuEntry = new MenuEntry(String.Empty);
        MenuEntry wallStyleMenuEntry = new MenuEntry(String.Empty);
        MenuEntry motionBlurMenuEntry = new MenuEntry(String.Empty);
        MenuEntry blurIntensityMenuEntry = new MenuEntry(String.Empty);
        MenuEntry neonEffectMenuEntry = new MenuEntry(String.Empty);
		MenuEntry fullScreenMenuEntry = new MenuEntry (String.Empty);

		GraphicsDeviceManager gdm;
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor populates the menu with empty strings: the real values
        /// are filled in by the Update method to reflect the changing settings.
        /// </summary>
        public OptionsMenuScreen()
        {
            currentAsteroidDensity = (int)WorldRules.AsteroidDensity;
            currentWallStyle = (int)WorldRules.WallStyle;
            scoreLimit = WorldRules.ScoreLimit;
            motionBlur = WorldRules.MotionBlur;
            blurIntensity = WorldRules.BlurIntensity;
            neonEffect = WorldRules.NeonEffect;

            scoreLimitMenuEntry.Selected += ScoreLimitMenuEntrySelected;
            asteroidDensityMenuEntry.Selected += AsteroidDensityMenuEntrySelected;
            wallStyleMenuEntry.Selected += WallStyleMenuEntrySelected;
            motionBlurMenuEntry.Selected += MotionBlurMenuEntrySelected;
            blurIntensityMenuEntry.Selected += BlurIntensityMenuEntrySelected;
            neonEffectMenuEntry.Selected += NeonEffectMenuEntrySelected;
            fullScreenMenuEntry.Selected += FullScreenMenuEntrySelected;

            MenuEntries.Add(scoreLimitMenuEntry);
            MenuEntries.Add(asteroidDensityMenuEntry);
            MenuEntries.Add(wallStyleMenuEntry);
            MenuEntries.Add(motionBlurMenuEntry);
            MenuEntries.Add(blurIntensityMenuEntry);
            MenuEntries.Add(neonEffectMenuEntry);
#if !USE_DEFAULT_DEVICE_SETTINGS
			MenuEntries.Add(fullScreenMenuEntry);
#endif
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

			scoreLimitMenuEntry.Text = string.Format (Strings.Score_Limit, scoreLimit);
			asteroidDensityMenuEntry.Text = string.Format (Strings.Asteroid_Density, asteroidDensity[currentAsteroidDensity]);
			wallStyleMenuEntry.Text = string.Format (Strings.Wall_Style ,wallStyle[currentWallStyle]);
			motionBlurMenuEntry.Text = string.Format (Strings.Motion_Blur,motionBlur);
			neonEffectMenuEntry.Text = string.Format (Strings.Neon_Effect ,neonEffect);
			blurIntensityMenuEntry.Text = string.Format (Strings.Blur_Intesity ,blurIntensity);
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
        void WallStyleMenuEntrySelected(object sender, EventArgs e)
        {
            currentWallStyle = (currentWallStyle + 1) % wallStyle.Length;
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

        /// <summary>
        /// When the user cancels the options screen, go back to the main menu.
        /// </summary>
        protected override void OnCancel()
        {
            WorldRules.AsteroidDensity = 
                (AsteroidDensity)Enum.Parse(typeof(AsteroidDensity), 
                                         asteroidDensity[currentAsteroidDensity], true);
            WorldRules.WallStyle = (WallStyle)Enum.Parse(typeof(WallStyle), 
                wallStyle[currentWallStyle], true);
            WorldRules.ScoreLimit = scoreLimit;
            WorldRules.MotionBlur = motionBlur;
            WorldRules.NeonEffect = neonEffect;
            WorldRules.BlurIntensity = blurIntensity;

            ExitScreen();
        }


#endregion
    }
}
