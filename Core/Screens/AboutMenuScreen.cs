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
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// The About screen is brought up over the top of the main menu
    /// screen and tell you a bit about the game.
    /// </summary>
    class AboutMenuScreen : MenuScreen
    {
        #region Fields
        MenuEntry builtWithMonoGame = new MenuEntry(Strings.Built_With_MonoGame);
        MenuEntry fontBy = new MenuEntry(string.Format(Strings.Font_By, "GNU Unifont Glyphs", "Unifoundry.com"));
        MenuEntry gameEnhancements = new MenuEntry(string.Format(Strings.Game_Enhancements, "Dean Ellis & Dominique Louis"));
        MenuEntry githubRepo = new MenuEntry(string.Format(Strings.Github_Repo, "infinitespace-studios/VectorRumble"));

        const string MonoGameUri = "https://monogame.net";
        const string FontUri = "https://payhip.com/b/xNA0";
        const string GitHubUri = "https://github.com/infinitespace-studios/VectorRumble";
        #endregion

        #region Initialization


        /// <summary>
        /// Constructor populates the menu with empty strings. The real values
        /// are filled in by the Update method to reflect the changing settings.
        /// </summary>
        public AboutMenuScreen()
        {
            fontBy.Selected += FontByMenuEntrySelected;
            builtWithMonoGame.Selected += BuiltWithMonoGameMenuEntrySelected;
            gameEnhancements.Selected += GameEnhancementsMenuEntrySelected;
            githubRepo.Selected += GithubRepoMenuEntrySelected;

            MenuEntries.Add(builtWithMonoGame);
            MenuEntries.Add(fontBy);
            MenuEntries.Add(gameEnhancements);
            MenuEntries.Add(githubRepo);
            var assembly = typeof(Arena).Assembly;
            MenuEntries.Add(new MenuEntry(assembly.GetName().Version.ToString()));
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
        /// Event handler for when the Built With MonoGame menu entry is selected.
        /// </summary>
        void BuiltWithMonoGameMenuEntrySelected(object sender, EventArgs e)
        {
            ProcessStart(MonoGameUri);
        }

        /// <summary>
        /// Event handler for when the Font menu entry is selected.
        /// </summary>
        void FontByMenuEntrySelected(object sender, EventArgs e)
        {
            ProcessStart(FontUri);
        }

        /// <summary>
        /// Event handler for when the GameEnhancements menu entry is selected.
        /// </summary>
        private void GameEnhancementsMenuEntrySelected(object sender, EventArgs e)
        {
            // Just there for show :)
        }

        /// <summary>
        /// Event handler for when the GithubRepo menu entry is selected.
        /// </summary>
        private void GithubRepoMenuEntrySelected(object sender, EventArgs e)
        {
            ProcessStart(GitHubUri);
        }

        /// <summary>
        /// When the user cancels the options screen, go back to the main menu.
        /// </summary>
        protected override void OnCancel()
        {
            ExitScreen();
        }


        public static async void ProcessStart(string uri)
        {
            await Task.Run(() => { try { System.Diagnostics.Process.Start(uri); } catch { } });
        }
        #endregion
    }
}
