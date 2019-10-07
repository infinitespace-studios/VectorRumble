#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace VectorRumble
{
    class BaseHUDScreen : GameScreen
    {

        #region Fields
        protected ContentManager content;

        protected LineBatch lineBatch;

        protected SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        protected Texture2D player3Controls;
        protected Texture2D player4Controls;
        protected Texture2D gamepadControls;
#if ANDROID || IOS
        Texture2D gamePadTexture;
#endif
        Texture2D starTexture;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        public BaseHUDScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.25);
            TransitionOffTime = TimeSpan.FromSeconds(0.25);
        }

        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
            {
                content = new ContentManager(ScreenManager.Game.Services, "Content");
            }
            spriteBatch = new SpriteBatch(ScreenManager.GraphicsDevice);
            spriteFont = content.Load<SpriteFont>("Fonts/retroLarge");
            starTexture = content.Load<Texture2D>("Textures/blank");
            player3Controls = content.Load<Texture2D>("Textures/player1-controls");
            player4Controls = content.Load<Texture2D>("Textures/player2-controls");
            gamepadControls = content.Load<Texture2D>("Textures/gamepad-controls");


            lineBatch = new LineBatch(ScreenManager.GraphicsDevice);

            // update the projection in the line-batch
            lineBatch.SetProjection(Matrix.CreateOrthographicOffCenter(0.0f,
                ScreenManager.GraphicsDevice.Viewport.Width,
                ScreenManager.GraphicsDevice.Viewport.Height, 0.0f, 0.0f, 1.0f));

#if ANDROID || IOS
            gamePadTexture = content.Load<Texture2D>("Textures/gamepad");
            
            ThumbStickDefinition thumbStickLeft = new ThumbStickDefinition();
            thumbStickLeft.Position = new Vector2(10,400);
            thumbStickLeft.Texture = gamePadTexture;
            thumbStickLeft.TextureRect = new Rectangle(2,2,68,68);
            
            GamePad.LeftThumbStickDefinition = thumbStickLeft;
            
            ThumbStickDefinition thumbStickRight = new ThumbStickDefinition();
            thumbStickRight.Position = new Vector2(240,400);
            thumbStickRight.Texture = gamePadTexture;
            thumbStickRight.TextureRect = new Rectangle(2,2,68,68);
            
            GamePad.RightThumbStickDefinition = thumbStickRight;
#endif
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            if (spriteBatch != null)
            {
                spriteBatch.Dispose();
                spriteBatch = null;
            }

            if (lineBatch != null)
            {
                lineBatch.Dispose();
                lineBatch = null;
            }

            content.Unload();
        }
        #endregion

        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            lineBatch.Begin();

            // draw all actors
            foreach (Actor actor in World.Actors)
            {
                if (actor.Dead == false)
                {
                    actor.Draw(elapsedTime, lineBatch);
                }
            }

            lineBatch.End();

            DrawHud(elapsedTime);

#if ANDROID || IOS
            GamePad.Draw(gameTime, spriteBatch);
#endif
            // draw the stars
            spriteBatch.Begin();
            World.Starfield.Draw(spriteBatch, starTexture);
            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition > 0)
                ScreenManager.FadeBackBufferToBlack(255 - TransitionAlpha);
        }

        /// <summary>
        /// Draw the user interface elements of the game (scores, etc.).
        /// </summary>
        /// <param name="elapsedTime">The amount of elapsed time, in seconds.</param>
        private void DrawHud(float elapsedTime)
        {
            spriteBatch.Begin();

            Vector2 position = new Vector2(128, 64);
            int offset = (1280) / 5;
            float scale = 1.0f;

            var playerIndexes = (PlayerIndex[])Enum.GetValues(typeof(PlayerIndex));
            foreach (var index in playerIndexes)
            {
                string message = string.Empty;
                var caps = GamePad.GetState(index);

                // Only Draw Join message if no one is playing yet.
                if (!World.ShipManager.Players.Any())
                {
                    switch (index)
                    {
                        case PlayerIndex.One:
                            message = caps.IsConnected ? Strings.Hold_A_To_Join : Strings.Connect_Gamepad;
                            break;
                        case PlayerIndex.Two:
                            message = caps.IsConnected ? Strings.Hold_A_To_Join : Strings.Connect_Gamepad;
                            break;
                        case PlayerIndex.Three:
                            message = Strings.Press_W_To_Join;
                            break;
                        case PlayerIndex.Four:
                            message = Strings.Press_Up_To_Join;
                            break;
                        default:
                            message = Strings.No_Input_Device_Connected;
                            break;
                    }
                }

                if (World.ShipManager.SelectedPlayers.Any())
                {
                    var player = World.ShipManager.SelectedPlayers.Where(p => p.PlayerStringToIndex == index).ToArray();
                    if (player != null && player.Length > 0)
                        message = string.Format(Strings.Score, player[0].Name, player[0].Score);
                }

                Vector2 size = spriteFont.MeasureString(message) * scale;
                position.X = ((int)index + 1) * offset - size.X / 2;
                spriteBatch.DrawString(spriteFont, message, position, Color.Yellow, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 1.0f);

            }

            spriteBatch.End();
        }
    }

    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    /// <remarks>
    /// This class is somewhat similar to one of the same name in the 
    /// GameStateManagement sample.
    /// </remarks>
    class GameplayScreen : BaseHUDScreen
    {
        #region Fields
        bool gameOver;
        #endregion

        #region Initialization
        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.0);
            TransitionOffTime = TimeSpan.FromSeconds(1.0);
        }


        /// <summary>
        /// Initialize the game, after the ScreenManager is set, but not every time
        /// the graphics are reloaded.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            // start up the music
            World.AudioManager.PlayMusic("gameMusic");

            // start up the game
            World.StartNewGame();
            gameOver = false;
        }
        #endregion

        #region Update and Draw
        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            // if this screen is leaving, then stop the music
            if (IsExiting)
            {
                World.AudioManager.StopMusic();
            }
            else if ((otherScreenHasFocus == true) || (coveredByOtherScreen == true))
            {
                // make sure nobody's controller is vibrating
                for (int i = 0; i < 4; i++)
                {
                    GamePad.SetVibration((PlayerIndex)i, 0f, 0f);
                }
                if (gameOver == false)
                {
                    for (int i = 0; i < World.Ships.Length; i++)
                    {
                        World.Ships[i].ProcessInput(gameTime.TotalGameTime.Seconds,
                            true);
                    }
                }
            }
            else
            {
                // check for a winner
                if (gameOver == false)
                {
                    for (int i = 0; i < World.Ships.Length; i++)
                    {
                        if (World.Ships[i].Score >= WorldRules.ScoreLimit)
                        {
                            ScreenManager.AddScreen(new GameOverScreen(string.Format(Strings.Player_X_Wins,
                                (i + 1))));
                            gameOver = true;
                            break;
                        }
                    }
                }
                // update the world
                if (gameOver == false)
                {
                    World.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                }
            }
        }


        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (input.PauseGame)
            {
                // If they pressed pause, bring up the pause menu screen.
                ScreenManager.AddScreen(new PauseMenuScreen());
            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            if (WorldRules.NeonEffect) {
                ScreenManager.BloomComponent.BeginDraw();
            }

            lineBatch.Begin();

            // draw all particle systems
            foreach (ParticleSystem particleSystem in World.ParticleSystems)
            {
                if (particleSystem.IsActive)
                {
                    particleSystem.Draw(lineBatch);
                }
            }

            // draw the walls
            World.SelectedArena.DrawWalls(lineBatch);

            lineBatch.End();

            if (WorldRules.NeonEffect)
            {
                ScreenManager.BloomComponent.Draw(gameTime);
            }

            base.Draw(gameTime);
        }
#endregion
    }
}
