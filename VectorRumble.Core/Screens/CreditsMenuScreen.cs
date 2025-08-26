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
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// The Credits screen is brought up over the top of the main menu
    /// screen and gives credit where credit is due, by listing contributors.
    /// </summary>
    class CreditsMenuScreen : MenuScreen
    {
        #region Fields
        MenuEntry thodeC = new MenuEntry("Christian Thode");
        const string thodeCGithub = "https://github.com/ThodeC";
        MenuEntry yuanZzzz = new MenuEntry("Yuan-Zzzz");
        const string yuanZzzzGithub = "https://github.com/Yuan-Zzzz";
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor populates the menu with empty strings. The real values
        /// are filled in by the Update method to reflect the changing settings.
        /// </summary>
        public CreditsMenuScreen()
        {
            thodeC.Selected += ThodeCMenuEntrySelected;
            MenuEntries.Add(thodeC);
            
            yuanZzzz.Selected += YuanZzzzMenuEntrySelected;
            MenuEntries.Add(yuanZzzz);
        }

        private void ThodeCMenuEntrySelected(object sender, EventArgs e)
        {
            AboutMenuScreen.ProcessStart(thodeCGithub);
        }

        private void YuanZzzzMenuEntrySelected(object sender, EventArgs e)
        {
            AboutMenuScreen.ProcessStart(yuanZzzzGithub);
        }
        #endregion

        #region Handle Input


        /// <summary>
        /// Updates the About screen, filling in the latest values for the menu text.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // TODO Update dynamic text here 
        }

        /// <summary>
        /// When the user cancels the options screen, go back to the main menu.
        /// </summary>
        protected override void OnCancel()
        {
            ExitScreen();
        }
        #endregion
    }
}

