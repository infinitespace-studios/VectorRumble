using System;
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
            smallFont = ScreenManager.Game.Content.LoadLocalized<SpriteFont>("Fonts/retroSmall");
            mediumFont = ScreenManager.Game.Content.LoadLocalized<SpriteFont>("Fonts/retroMedium");

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

            if (input.MenuSelect && World.ShipManager.SelectedPlayers.Count > 0)
            {
                var arenaSelection = new ArenaSelectionScreen
                {
                    ScreenManager = this.ScreenManager
                };
                arenaSelection.Initialize();
                ScreenManager.AddScreen(arenaSelection);
            }

            if (HasSelectedPlayer(PlayerIndex.One))
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

            if (HasSelectedPlayer(PlayerIndex.Two))
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

            if (HasSelectedPlayer(PlayerIndex.Three))
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

            if (HasSelectedPlayer(PlayerIndex.Four))
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

        private bool HasSelectedPlayer(PlayerIndex playerIndex)
        {
            for (int i = 0; i < World.ShipManager.SelectedPlayers.Count; i++)
            {
                if (World.ShipManager.SelectedPlayers[i].PlayerStringToIndex == playerIndex)
                {
                    return true;
                }
            }
            return false;
        }

        private static void ChangePlayerColour(PlayerIndex playerIndex)
        {
            Ship player = null;
            for (int i = 0; i < World.ShipManager.SelectedPlayers.Count; i++)
            {
                if (World.ShipManager.SelectedPlayers[i].PlayerStringToIndex == playerIndex)
                {
                    player = World.ShipManager.SelectedPlayers[i];
                    break;
                }
            }
            if (player != null)
            {
                player.ChangeColor();
            }
        }

        private void ChangePlayerShip(PlayerIndex playerIndex)
        {
            Ship player = null;
            for (int i = 0; i < World.ShipManager.SelectedPlayers.Count; i++)
            {
                if (World.ShipManager.SelectedPlayers[i].PlayerStringToIndex == playerIndex)
                {
                    player = World.ShipManager.SelectedPlayers[i];
                    break;
                }
            }
            if (player == null)
            {
                return;
            }
            
            // Find spare ships (ships without PlayerIndex)
            int spareShipCount = 0;
            for (int i = 0; i < World.ShipManager.Ships.Length; i++)
            {
                if (string.IsNullOrEmpty(World.ShipManager.Ships[i].PlayerIndex))
                {
                    spareShipCount++;
                }
            }
            
            if (spareShipCount > 0)
            {
                // Find the spare ship at spainShipIndex
                int currentSpareIndex = 0;
                Ship ship = null;
                for (int i = 0; i < World.ShipManager.Ships.Length; i++)
                {
                    if (string.IsNullOrEmpty(World.ShipManager.Ships[i].PlayerIndex))
                    {
                        if (currentSpareIndex == spainShipIndex)
                        {
                            ship = World.ShipManager.Ships[i];
                            break;
                        }
                        currentSpareIndex++;
                    }
                }
                
                if (ship != null)
                {
                    //Swap ships 
                    player.Selected = false;
                    ship.PlayerIndex = player.PlayerIndex;
                    player.PlayerIndex = string.Empty;
                    ship.Selected = true;
                    World.ShipManager.UpdateSelectedList();
                    var pIndex = World.Actors.IndexOf(player);
                    World.Actors.RemoveAt(pIndex);
                    World.Actors.Add(ship);

                    spainShipIndex++;
                    if (spainShipIndex > spareShipCount - 1)
                        spainShipIndex = 0;
                }
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

            // Draw ships with PlayerIndex (available ships) in sorted order by PlayerStringToIndex
            // This ensures Player 1, 2, 3, 4 appear in the correct screen positions
            for (PlayerIndex playerIndex = PlayerIndex.One; playerIndex <= PlayerIndex.Four; playerIndex++)
            {
                // Find ship for this player index
                Ship shipToDraw = null;
                int shipPosition = 0;
                
                for (int i = 0; i < World.ShipManager.Ships.Length; ++i)
                {
                    if (!string.IsNullOrEmpty(World.ShipManager.Ships[i].PlayerIndex))
                    {
                        if (World.ShipManager.Ships[i].PlayerStringToIndex == playerIndex)
                        {
                            shipToDraw = World.ShipManager.Ships[i];
                            // Calculate position based on player index (1-4)
                            shipPosition = (int)playerIndex + 1;
                            break;
                        }
                    }
                }
                
                if (shipToDraw != null && shipToDraw.Selected)
                {
                    lineBatch.Begin();
                    pos.X = shipPosition * (viewport.Width / 5);
                    pos.Y = viewport.Height / 3;
                    shipToDraw.Position = pos;
                    shipToDraw.Draw((float)gameTime.ElapsedGameTime.TotalSeconds, lineBatch);
                    lineBatch.End();

                    // Prepare the rect for position for control rendering
                    rect.X = (int)(pos.X - (shipToDraw.Radius * 4) - 5);

                    // Fade the popup alpha during transitions.
                    Color shipColor = new Color(shipToDraw.Color.R, shipToDraw.Color.G, shipToDraw.Color.B, (int)TransitionAlpha);

                    // Offset the Name's X/Y positions a little
                    pos.X -= shipToDraw.Radius + 8; // TODO Centre correctly based on name length
                    pos.Y += shipToDraw.Radius + (ScreenManager.Font.LineSpacing * 1.1f);

                    ScreenManager.SpriteBatch.Begin();

                    // Now draw our controls
                    rect.Y = (viewport.Height / 3);
                    rect.Width = 160;
                    rect.Height = 155;

                    switch (playerIndex)
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
