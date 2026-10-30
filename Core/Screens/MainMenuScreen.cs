#region File Description
//-----------------------------------------------------------------------------
// MainMenuScreen.cs
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
    /// The main menu screen is the first thing displayed when the game starts up.
    /// </summary>
    /// <remarks>
    /// This class is somewhat similar to one of the same name in the 
    /// GameStateManagement sample.
    /// </remarks>
    class MainMenuScreen : MenuScreen
    {
        #region Initialization


        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MainMenuScreen()
        {
			MenuEntry playGameMenuEntry = new MenuEntry(Strings.Play_Game);
            MenuEntry optionsMenuEntry = new MenuEntry(Strings.Options);
            MenuEntry aboutMenuEntry = new MenuEntry(Strings.About);
            MenuEntry creditsMenuEntry = new MenuEntry(Strings.Credits);
            MenuEntry exitMenuEntry = new MenuEntry(Strings.Exit);

            playGameMenuEntry.Selected += PlayGameMenuEntrySelected;
            optionsMenuEntry.Selected += OptionsMenuEntrySelected;
            aboutMenuEntry.Selected += AboutMenuEntrySelected;
            creditsMenuEntry.Selected += CreditsMenuEntrySelected;
            exitMenuEntry.Selected += ExitMenuEntrySelected;

            MenuEntries.Add(playGameMenuEntry);
            MenuEntries.Add(optionsMenuEntry);
            MenuEntries.Add(aboutMenuEntry);
            MenuEntries.Add(creditsMenuEntry);
            MenuEntries.Add(exitMenuEntry);
        }

        /// <summary>
        /// Load all content.
        /// </summary>
        public override void LoadContent()
        {
            base.Initialize(); // TODO Move this somewhere better

            World.ArenaManager.LoadContent(ScreenManager.Game.Content);
            base.LoadContent();
        }
        #endregion

        #region Handle Input


        /// <summary>
        /// Event handler for when the Play Game menu entry is selected.
        /// </summary>
        void PlayGameMenuEntrySelected(object sender, EventArgs e)
        {
            // starting a game has its own screens: leave the logo and the fireworks out of them
            SetBackgroundQuiet(true);
            ScreenManager.AddScreen(new MultiplayerMenuScreen());
        }

        void SetBackgroundQuiet(bool quiet)
        {
            foreach (var screen in ScreenManager.Screens)
            {
                if (screen is BackgroundScreen background)
                {
                    background.Quiet = quiet;
                    break;
                }
            }
        }

        /// <summary>
        /// Brings the logo and the fireworks back when the player returns here from starting a game.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus, bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                SetBackgroundQuiet(false);
            }
        }

        /// <summary>
        /// Event handler for when the Options menu entry is selected.
        /// </summary>
        void OptionsMenuEntrySelected(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new OptionsMenuScreen());
        }


        /// <summary>
        /// Event handler for when the Exit menu entry is selected.
        /// </summary>
        void ExitMenuEntrySelected(object sender, EventArgs e)
        {
            OnCancel();
        }


        /// <summary>
        /// When the user cancels the main menu, ask if they want to exit the sample.
        /// </summary>
        protected override void OnCancel()
        {
			MessageBoxScreen messageBox = new MessageBoxScreen(Strings.Exit_Prompt);
            messageBox.Accepted += ExitMessageBoxAccepted;
            ScreenManager.AddScreen(messageBox);
        }

        /// <summary>
        /// Event handler for when the user selects ok on the "are you sure
        /// you want to exit" message box.
        /// </summary>
        void ExitMessageBoxAccepted(object sender, EventArgs e)
        {
            ScreenManager.Game.Exit();
        }

        private void AboutMenuEntrySelected(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new AboutMenuScreen());
        }

        private void CreditsMenuEntrySelected(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new CreditsMenuScreen());
        }

        
        #endregion
    }
}
