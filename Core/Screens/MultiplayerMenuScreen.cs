#region Using Statements
using System;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// Where the player chooses between a game over the network and a game on this machine.
    /// </summary>
    class MultiplayerMenuScreen : MenuScreen
    {
        /// <summary>
        /// Constructor fills in the menu contents.
        /// </summary>
        public MultiplayerMenuScreen()
        {
            MenuEntry lanMenuEntry = new MenuEntry(Strings.Multiplayer_LAN);
            MenuEntry localMenuEntry = new MenuEntry(Strings.Multiplayer_Local);
            MenuEntry backMenuEntry = new MenuEntry(Strings.Back);

            lanMenuEntry.Selected += LanMenuEntrySelected;
            localMenuEntry.Selected += LocalMenuEntrySelected;
            backMenuEntry.Selected += OnCancel;

            MenuEntries.Add(lanMenuEntry);
            MenuEntries.Add(localMenuEntry);
            MenuEntries.Add(backMenuEntry);
        }

        void LanMenuEntrySelected(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new LanBrowserScreen());
        }

        void LocalMenuEntrySelected(object sender, EventArgs e)
        {
            ScreenManager.AddScreen(new GameSetupScreen());
        }
    }
}
