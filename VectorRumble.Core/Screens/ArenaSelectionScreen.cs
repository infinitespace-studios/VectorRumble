using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VectorRumble
{
    class ArenaSelectionScreen : BaseHUDScreen
    {
        SpriteFont smallFont;
        int[] timeGapBetweenDrawCall = { 250, 375, 525, 700, 850, 1200 }; // TODO Tweak these values
        int timeGameIndex = 0;
        private Arena randomArena;
        private TimeSpan CountDown;
        private bool finalDraw;

        /// <summary>
        /// Loads graphics content for this screen.
        /// </summary>
        public override void LoadContent()
        {
            smallFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/retroSmall");

            CountDown = TimeSpan.FromMilliseconds(timeGapBetweenDrawCall[timeGameIndex]);

            randomArena = World.ArenaManager.Random();
            base.LoadContent();
        }

        /// <summary>
        /// Loading screen callback for activating the gameplay screen.
        /// </summary>
        void LoadGameplayScreen(object sender, EventArgs e)
        {
            GameplayScreen gameplayScreen = new GameplayScreen {
                ScreenManager = this.ScreenManager
            };
            gameplayScreen.Initialize();
            ScreenManager.AddScreen(gameplayScreen);
        }

        /// <summary>
        /// Responds to user input, accepting or cancelling the screen.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input.MenuCancel)
            {
                ExitScreen();
            }

            if (input.MenuSelect)
            {
                LoadingScreen.Load(ScreenManager, LoadGameplayScreen, true);
            }
        }

        /// <summary>
        /// Draws the Arena.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            string arenaName;

            CountDown -= gameTime.ElapsedGameTime;
            if (timeGameIndex < timeGapBetweenDrawCall.Length && !finalDraw && WorldRules.DefaultArena == Strings.Arena_Random)
            {
                lineBatch.Begin();
                randomArena.DrawWalls(lineBatch);
                lineBatch.End();

                if (CountDown.TotalMilliseconds <= 0)
                {
                    randomArena = World.ArenaManager.Random();
                    CountDown = TimeSpan.FromMilliseconds(timeGapBetweenDrawCall[timeGameIndex]);
                    timeGameIndex++;
                }
                arenaName = randomArena.Name;
            }
            else
            {
                finalDraw = true;
                lineBatch.Begin();
                World.SelectedArena.DrawWalls(lineBatch);
                lineBatch.End();

                arenaName = World.SelectedArena.Name;
            }

            // Fade the popup alpha during transitions.
            Color color = new Color(255, 255, 255, (int)TransitionAlpha);

            // Center the message text in the viewport.
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 usageTextSize = smallFont.MeasureString(Strings.Battle_Arena_Is);
            Vector2 usageTextPosition = (viewportSize - usageTextSize) / 2;
            // Position it in the lower 3rd of the screen
            usageTextPosition.Y += (viewport.Height / 3) + ScreenManager.Font.LineSpacing * 1.1f;

            // Draw the message box text.
            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.DrawString(smallFont, string.Format(Strings.Battle_Arena_Is, arenaName), usageTextPosition, color);
            ScreenManager.SpriteBatch.End();
        }
    }
}
