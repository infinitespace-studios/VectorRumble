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

        static int blurIntensity = 5;
        static bool motionBlur = true;
        static bool neonEffect = true;

        MenuEntry blurIntensityMenuEntry = new MenuEntry(String.Empty);
        MenuEntry fullScreenMenuEntry = new MenuEntry(String.Empty);
        MenuEntry motionBlurMenuEntry = new MenuEntry(String.Empty);
        MenuEntry neonEffectMenuEntry = new MenuEntry(String.Empty);

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
            motionBlur = WorldRules.MotionBlur;
            neonEffect = WorldRules.NeonEffect;

            blurIntensityMenuEntry.Selected += BlurIntensityMenuEntrySelected;
            fullScreenMenuEntry.Selected += FullScreenMenuEntrySelected;
            motionBlurMenuEntry.Selected += MotionBlurMenuEntrySelected;
            neonEffectMenuEntry.Selected += NeonEffectMenuEntrySelected;

            MenuEntries.Add(blurIntensityMenuEntry);
#if !USE_DEFAULT_DEVICE_SETTINGS
            MenuEntries.Add(fullScreenMenuEntry);
#endif
            MenuEntries.Add(motionBlurMenuEntry);
            MenuEntries.Add(neonEffectMenuEntry);
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

            blurIntensityMenuEntry.Text = string.Format(Strings.Blur_Intesity, blurIntensity);
            motionBlurMenuEntry.Text = string.Format (Strings.Motion_Blur,motionBlur);
			neonEffectMenuEntry.Text = string.Format (Strings.Neon_Effect ,neonEffect);
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
        void BlurIntensityMenuEntrySelected(object sender, EventArgs e)
        {
            blurIntensity += 1;
            if (blurIntensity > 10)
                blurIntensity = 1;
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
            WorldRules.BlurIntensity = blurIntensity;
            WorldRules.MotionBlur = motionBlur;
            WorldRules.NeonEffect = neonEffect;

            ExitScreen();
        }


#endregion
    }
}
