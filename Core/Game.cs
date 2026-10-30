#region File Description
//-----------------------------------------------------------------------------
// Game.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class VectorRumbleGame : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;

        ScreenManager screenManager;
        AudioManager audioManager;


        public VectorRumbleGame()
        {
            // keeps this game from listing, or joining, the sessions of other games on the network
            Networking.MultiplayerSessionService.Initialize();

            graphics = new GraphicsDeviceManager(this);
			Services.AddService (typeof (GraphicsDeviceManager), graphics);
            Content.RootDirectory = "Content";

#if USE_DEFAULT_DEVICE_SETTINGS
            // Use default device settings defined by platform.
#else
            if (Helper.IsMobile) // iPad or Tablets only
            {
                // the device's own screen: the game is drawn at a fixed size and scaled to it (see Display)
                graphics.IsFullScreen = true;
                graphics.SupportedOrientations = DisplayOrientation.LandscapeLeft;
            }
            else
            {
                graphics.PreferredBackBufferWidth = Display.BaseWidth;
                graphics.PreferredBackBufferHeight = Display.BaseHeight;
                graphics.HardwareModeSwitch = false;
                graphics.IsFullScreen = false;
                IsMouseVisible = true;
            }
#endif
			graphics.SynchronizeWithVerticalRetrace = true;
            Window.AllowUserResizing = false;

            // create the screen manager
            screenManager = new ScreenManager(this);
            Components.Add(screenManager);

            // create the audio manager
            audioManager = new AudioManager(this, 
                "Content/Audio/VectorRumble.xgs", 
                "Content/Audio/VectorRumble.xwb",
                "Content/Audio/VectorRumble.xsb");
            Services.AddService(typeof(AudioManager), audioManager);
        }


        /// <summary>
        /// Overridden from Game.Initialize().  Sets up the ScreenManager.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            // add the background screen to the screen manager
            screenManager.AddScreen(new BackgroundScreen());

            // add the main menu screen to the screen manager
            screenManager.AddScreen(new MainMenuScreen());
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // update the audio manager
            audioManager.Update(gameTime);

            base.Update(gameTime);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.Black);

            // the screen manager owns the real drawing

            base.Draw(gameTime);
        }
    }
}
