using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace VectorRumble
{
    class ShipSelectionScreen : BaseHUDScreen
    {
        SpriteFont smallFont;
        SpriteFont mediumFont;
        int spainShipIndex = 0;

        /// <summary>
        /// Loads graphics content for this screen.
        /// </summary>
        public override void LoadContent()
        {
            smallFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/retroSmall");
            mediumFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/retroMedium");

            World.ShipManager.LoadContent(ScreenManager.Game.Content);
            World.AddCastOfAvailableShips();

            base.LoadContent();
        }

        /// <summary>
        /// Responds to user input, accepting or cancelling the message box.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input.MenuCancel)
            {
                ExitScreen();
            }

            if (input.MenuSelect && World.ShipManager.SelectedPlayers.Any())
            {
                var arenaSelection = new ArenaSelectionScreen
                {
                    ScreenManager = this.ScreenManager
                };
                arenaSelection.Initialize();
                ScreenManager.AddScreen(arenaSelection);
            }

            if (World.ShipManager.SelectedPlayers.Any(p => p.PlayerStringToIndex == PlayerIndex.One))
            {
                if (input.IsNewButtonPress(Buttons.DPadUp) || input.IsNewButtonPress(Buttons.LeftThumbstickUp))
                {
                    ChangePlayerColour(PlayerIndex.One);
                }

                if (input.IsNewButtonPress(Buttons.RightShoulder))
                {
                    ChangePlayerShip(PlayerIndex.One);
                }
            }

            if (World.ShipManager.SelectedPlayers.Any(p => p.PlayerStringToIndex == PlayerIndex.Two))
            {
                if (input.IsNewButtonPress(Buttons.DPadUp) || input.IsNewButtonPress(Buttons.LeftThumbstickUp))
                {
                    ChangePlayerColour(PlayerIndex.Two);
                }

                if (input.IsNewButtonPress(Buttons.RightShoulder))
                {
                    ChangePlayerShip(PlayerIndex.Two);
                }
            }

            if (World.ShipManager.SelectedPlayers.Any(p => p.PlayerStringToIndex == PlayerIndex.Three))
            {
                if (input.IsNewKeyPress(Keys.W))
                {
                    ChangePlayerColour(PlayerIndex.Three);
                }

                if (input.IsNewKeyPress(Keys.Tab))
                {
                    ChangePlayerShip(PlayerIndex.Three);
                }
            }

            if (World.ShipManager.SelectedPlayers.Any(p => p.PlayerStringToIndex == PlayerIndex.Four))
            {
                if (input.IsNewKeyPress(Keys.Up))
                {
                    ChangePlayerColour(PlayerIndex.Four);
                }

                if (input.IsNewKeyPress(Keys.RightAlt))
                {
                    ChangePlayerShip(PlayerIndex.Four);
                }
            }
        }

        private static void ChangePlayerColour(PlayerIndex playerIndex)
        {
            var player = World.ShipManager.SelectedPlayers.First(p => p.PlayerStringToIndex == playerIndex);
            player.ChangeColor();
        }

        private void ChangePlayerShip(PlayerIndex playerIndex)
        {
            var player = World.ShipManager.SelectedPlayers.First(p => p.PlayerStringToIndex == playerIndex);
            var spareShips = World.ShipManager.SpareShips;
            if (spareShips != null && spareShips.Length > 0)
            {
                //Swap ships 
                var ship = spareShips[spainShipIndex];
                player.Selected = false;
                ship.PlayerIndex = player.PlayerIndex;
                player.PlayerIndex = string.Empty;
                ship.Selected = true;
                World.ShipManager.UpdateSelectedList();
                var pIndex = World.Actors.IndexOf(player);
                World.Actors.RemoveAt(pIndex);
                World.Actors.Add(ship);

               spainShipIndex++;
                if (spainShipIndex > spareShips.Length - 1)
                    spainShipIndex = 0;
            }
        }

        /// <summary>
        /// Draws the message box.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;

            var pos = new Vector2();
            var rect = new Rectangle();

            for (int i = 0; i < World.ShipManager.AvailableShips.Length; ++i)
            {
                if (World.ShipManager.AvailableShips[i].Selected)
                {
                    lineBatch.Begin();
                    pos.X = (i + 1) * (viewport.Width / 5);
                    pos.Y = viewport.Height / 3;
                    World.ShipManager.AvailableShips[i].Position = pos;
                    World.ShipManager.AvailableShips[i].Draw((float)gameTime.ElapsedGameTime.TotalSeconds, lineBatch);
                    lineBatch.End();

                    // Prepare the rect for position for control rendering
                    rect.X = (int)(pos.X - (World.ShipManager.AvailableShips[i].Radius * 4) - 5);

                    // Fade the popup alpha during transitions.
                    Color shipColor = new Color(World.ShipManager.AvailableShips[i].Color.R, World.ShipManager.AvailableShips[i].Color.G, World.ShipManager.AvailableShips[i].Color.B, (int)TransitionAlpha);

                    // Offset the Name's X/Y positions a little
                    pos.X -= World.ShipManager.AvailableShips[i].Radius + 8; // TODO Centre correctly based on name length
                    pos.Y += World.ShipManager.AvailableShips[i].Radius + (ScreenManager.Font.LineSpacing * 1.1f);

                    ScreenManager.SpriteBatch.Begin();

                    // Now draw our controls
                    rect.Y = (viewport.Height / 3);
                    rect.Width = 160;
                    rect.Height = 155;

                    switch (World.ShipManager.AvailableShips[i].PlayerStringToIndex)
                    {
                        case PlayerIndex.One:
                            ScreenManager.SpriteBatch.Draw(gamepadControls, rect, Color.White);

                            // Draw Controller Labels
                            pos.X = rect.X + 25;
                            pos.Y = rect.Y + 46;
                            ScreenManager.SpriteBatch.DrawString(mediumFont, Strings.Move, pos, Color.LimeGreen);
                            pos.X += 80;
                            pos.Y -= 37;
                            ScreenManager.SpriteBatch.DrawString(mediumFont, Strings.Fire, pos, Color.Red);
                            pos.X += 44;
                            pos.Y += 61;
                            ScreenManager.SpriteBatch.DrawString(mediumFont, Strings.Mine, pos, Color.Yellow);
                            break;
                        case PlayerIndex.Two:
                            ScreenManager.SpriteBatch.Draw(gamepadControls, rect, Color.White);

                            // Draw Controller Labels
                            pos.X = rect.X + 25;
                            pos.Y = rect.Y + 46;
                            ScreenManager.SpriteBatch.DrawString(mediumFont, Strings.Move, pos, Color.LimeGreen);
                            pos.X += 80;
                            pos.Y -= 37;
                            ScreenManager.SpriteBatch.DrawString(mediumFont, Strings.Fire, pos, Color.Red);
                            pos.X += 44;
                            pos.Y += 61;
                            ScreenManager.SpriteBatch.DrawString(mediumFont, Strings.Mine, pos, Color.Yellow);
                            break;
                        case PlayerIndex.Three:
                            rect.X -= 22;
                            ScreenManager.SpriteBatch.Draw(player3Controls, rect, Color.White);

                            // Draw Keyboard Labels
                            pos.X = rect.X;
                            pos.Y = rect.Y + 9;
                            ScreenManager.SpriteBatch.DrawString(mediumFont, Strings.Fire, pos, Color.Red);
                            pos.X += 130;
                            pos.Y += 40;
                            ScreenManager.SpriteBatch.DrawString(mediumFont, Strings.Move, pos, Color.LimeGreen);
                            pos.X -= 40;
                            pos.Y += 65;
                            ScreenManager.SpriteBatch.DrawString(mediumFont, Strings.Mine, pos, Color.Yellow);
                            break;
                        case PlayerIndex.Four:
                            rect.X -= 22;
                            ScreenManager.SpriteBatch.Draw(player4Controls, rect, Color.White);

                            // Draw Keyboard Labels
                            pos.X = rect.X;
                            pos.Y = rect.Y + 9;
                            ScreenManager.SpriteBatch.DrawString(mediumFont, Strings.Fire, pos, Color.Red);
                            pos.X += 130;
                            pos.Y += 40;
                            ScreenManager.SpriteBatch.DrawString(mediumFont, Strings.Move, pos, Color.LimeGreen);
                            pos.X -= 40;
                            pos.Y += 65;
                            ScreenManager.SpriteBatch.DrawString(mediumFont, Strings.Mine, pos, Color.Yellow);
                            break;
                    }
                    ScreenManager.SpriteBatch.End();
                }
            }

            // Fade the popup alpha during transitions.
            Color textColor = new Color(255, 255, 255, (int)TransitionAlpha);

            // Center the message text in the viewport.
            Vector2 viewportSize = new Vector2(viewport.Width, viewport.Height);
            Vector2 usageTextSize = smallFont.MeasureString(Strings.Choose_Your_Ship);
            Vector2 usageTextPosition = (viewportSize - usageTextSize) / 2;
            // Position it in the lower 3rd of the screen
            usageTextPosition.Y += (viewport.Height / 3) + ScreenManager.Font.LineSpacing * 1.1f;

            // Draw the message text.
            ScreenManager.SpriteBatch.Begin();
            ScreenManager.SpriteBatch.DrawString(smallFont, Strings.Choose_Your_Ship, usageTextPosition, textColor);

            usageTextSize = smallFont.MeasureString(Strings.Up_To_Change_Colour_Fire_To_Change_Ship);
            usageTextPosition = (viewportSize - usageTextSize) / 2;
            // Position it in the lower 3rd of the screen + 1
            usageTextPosition.Y += (viewport.Height / 3) + ScreenManager.Font.LineSpacing * 2.2f;
            ScreenManager.SpriteBatch.DrawString(smallFont, Strings.Up_To_Change_Colour_Fire_To_Change_Ship, usageTextPosition, textColor);
            ScreenManager.SpriteBatch.End();
        }

        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                              bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            World.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
    }
}
