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
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
#endregion

namespace VectorRumble
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    /// <remarks>
    /// This class is somewhat similar to one of the same name in the 
    /// GameStateManagement sample.
    /// </remarks>
    class GameplayScreen : GameScreen
    {
        #region Fields
        BloomComponent bloomComponent;
        ContentManager content;
        LineBatch lineBatch;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        Texture2D starTexture;
		Texture2D gamePadTexture;
		Texture2D controls;
		float controlsTransition;
		float direction = 1;
        World world;
        AudioManager audio;
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
        public void Initialize()
        {
            
            // create the world
            world = new World(new Vector2(ScreenManager.GraphicsDevice.Viewport.Width,
                ScreenManager.GraphicsDevice.Viewport.Height));

            // retrieve the audio manager
            audio = (AudioManager)ScreenManager.Game.Services.GetService(
                typeof(AudioManager));
            world.AudioManager = audio;
            
			// start up the music
            audio.PlayMusic("gameMusic");
			
            // start up the game
            world.StartNewGame();
            gameOver = false;
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
            lineBatch = new LineBatch(ScreenManager.GraphicsDevice);
            spriteFont = content.Load<SpriteFont>("Fonts/retroLarge");
            starTexture = content.Load<Texture2D>("Textures/blank");
			controls = content.Load<Texture2D> ("Textures/controls");

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
                audio.StopMusic();
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
                    for (int i = 0; i < world.Ships.Length; i++)
                    {
                        world.Ships[i].ProcessInput(gameTime.TotalGameTime.Seconds, 
                            true);
                    }
                }
            }
            else
            {
                // check for a winner
                if (gameOver == false)
                {
                    for (int i = 0; i < world.Ships.Length; i++)
                    {
                        if (world.Ships[i].Score >= WorldRules.ScoreLimit)
                        {
							ScreenManager.AddScreen(new GameOverScreen(string.Format (Strings.Player_X_Wins,
								(i + 1))));
                            gameOver = true;
                            break;
                        }
                    }
                }
                // update the world
                if (gameOver == false)
                {
                    world.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
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
                throw new ArgumentNullException("input");

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
				ScreenManager.BloomComponent.BeginDraw ();
			}

            float elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            lineBatch.Begin();

            // draw all actors
            foreach (Actor actor in world.Actors)
            {
                if (actor.Dead == false)
                {
                    actor.Draw(elapsedTime, lineBatch);
                }
            }

            // draw all particle systems
            foreach (ParticleSystem particleSystem in world.ParticleSystems)
            {
                if (particleSystem.IsActive)
                {
                    particleSystem.Draw(lineBatch);
                }
            }

            // draw the walls
            world.DrawWalls(lineBatch);

            lineBatch.End();

            // draw the stars
            spriteBatch.Begin();
            world.Starfield.Draw(spriteBatch, starTexture);
            spriteBatch.End();

            if (WorldRules.NeonEffect)
            {
				ScreenManager.BloomComponent.Draw (gameTime);
            }

            DrawHud(elapsedTime);
			
#if ANDROID || IOS || WINDOWS_PHONE			
			GamePad.Draw(gameTime, spriteBatch);
#endif			

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
			bool anyPlaying = false;

            for (int i = 0; i < world.Ships.Length; ++i)
            {
                string message;
				var index = (PlayerIndex)i;
				var caps = GamePad.GetCapabilities (index);

                if (world.Ships[i].Playing)
                {
					anyPlaying = true;
					message = string.Format (Strings.Score, (i +1), world.Ships [i].Score );
                }
                else
                {
					message = Strings.Hold_A_To_Join;
					if (index == PlayerIndex.One && !caps.IsConnected)
						message = Strings.Press_Z_To_Join;
					else if (index == PlayerIndex.Two && !caps.IsConnected)
						message = Strings.Press_M_To_Join;
					else if (!caps.IsConnected)
						message = Strings.Connect_Gamepad;
                }

                float scale = 1.0f;

                Vector2 size = spriteFont.MeasureString(message) * scale;
                position.X = (i + 1) * offset - size.X / 2;
                spriteBatch.DrawString(spriteFont, message, position, 
                    world.Ships[i].Color, 0.0f, Vector2.Zero, scale, 
                    SpriteEffects.None, 1.0f);
            }

			if (!anyPlaying) {
				controlsTransition = TransitionAlpha;
			}
			else {
				controlsTransition -= (elapsedTime * 255);
				if ((controlsTransition <= 0) || (controlsTransition >= 255)) {
					controlsTransition = MathHelper.Clamp (controlsTransition, 0, 255);
				}
			}

			if (controlsTransition > 0) {
				// Render a Controls texture.
				var bounds = ScreenManager.GraphicsDevice.Viewport.Bounds;

				int desiredWidth = bounds.Width - (bounds.Width / 20);
				int desiredHeight = (int)(controls.Height * (desiredWidth / (float)controls.Width));

				var rect = new Rectangle ((bounds.Width - desiredWidth) / 2, (bounds.Height - desiredHeight) / 2, desiredWidth, desiredHeight);

				Rectangle rect2 = new Rectangle (rect.X - 1, rect.Y - 1, rect.Width + 2, rect.Height + 2);
				ScreenManager.DrawRectangle (rect2, new Color (255, 255, 0,
					(int)(192.0f * (float)controlsTransition / 255.0f)));
				ScreenManager.DrawRectangle (rect, new Color (0, 0, 0,
					(int)(232.0f * (float)controlsTransition / 255.0f)));

				float preferredAspect = controls.Width / (float)controls.Height;
				float outputAspect =rect.Width / (float)rect.Height;


				Rectangle dst;
				if (outputAspect <= preferredAspect) {
					// output is taller than it is wider, bars on top/bottom
					int presentHeight = (int)((rect.Width / preferredAspect) + 0.5f);
					int barHeight = (rect.Height - presentHeight) / 2;
					dst = new Rectangle (rect.Left, rect.Top + barHeight, rect.Width, presentHeight);
				}
				else {
					// output is wider than it is tall, bars left/right
					int presentWidth = (int)((rect.Height * preferredAspect) + 0.5f);
					int barWidth = (rect.Width - presentWidth) / 2;
					dst = new Rectangle (rect.Left + barWidth, rect.Top, presentWidth, rect.Height);
				}
				dst.Inflate (-20, -20);

				spriteBatch.Draw (controls, destinationRectangle: dst, color: Color.White);
			}

            spriteBatch.End();
        }
        #endregion
    }
}
