using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VectorRumble
{
    /// <summary>
    /// Draws small pictures of arenas and ships for the screens where they are chosen. They are drawn from the
    /// same lines as the game, scaled down, so there are no pictures to make or ship.
    /// </summary>
    class PreviewPainter
    {
        /// <summary>Where the arena preview goes above a menu: in the middle, over the entries.</summary>
        public static Rectangle ArenaAreaAbove => new Rectangle(
            (Display.BaseWidth - 288) / 2, (int)(Display.BaseHeight * 0.07f), 288, 162);

        /// <summary>
        /// Where a smaller arena preview goes beside a menu, for a screen whose top is full: at the right,
        /// clear of the longest entries, level with the menu's first entry.
        /// </summary>
        public static Rectangle ArenaAreaBeside(ScreenManager screenManager)
        {
            // an entry is drawn centred on its line, so the first one's text starts half a line above the menu's top
            float lineHeight = screenManager.Font.LineSpacing;
            int top = (int)(Display.BaseHeight * MenuScreen.MenuTop - lineHeight / 2f + lineHeight * 0.1f);
            return new Rectangle(Display.BaseWidth - 240 - 24, top, 240, 135);
        }

        readonly LineBatch lineBatch;
        readonly Dictionary<Ship, VectorPolygon> shipShapes = new Dictionary<Ship, VectorPolygon>();
        readonly Dictionary<Ship, float> shipRadii = new Dictionary<Ship, float>();

        public PreviewPainter(GraphicsDevice graphicsDevice)
        {
            lineBatch = new LineBatch(graphicsDevice);
            lineBatch.SetProjection(Matrix.CreateOrthographicOffCenter(0f, Display.BaseWidth, Display.BaseHeight, 0f, 0f, 1f));
        }

        /// <summary>
        /// The arena with the given name, or null.
        /// </summary>
        public static Arena FindArena(string name)
        {
            Arena[] arenas = GameScreen.World?.ArenaManager?.Arenas;
            return arenas?.FirstOrDefault(a => a.Name == name);
        }

        /// <summary>
        /// Draws the arena's walls in a frame. A "Random" arena has no walls to show, so it gets a question mark.
        /// </summary>
        public void DrawArena(ScreenManager screenManager, Arena arena, Rectangle area, int alpha)
        {
            Color frame = new Color(90, 90, 90, alpha);

            lineBatch.Begin();
            Vector2 topLeft = new Vector2(area.Left, area.Top);
            Vector2 topRight = new Vector2(area.Right, area.Top);
            Vector2 bottomLeft = new Vector2(area.Left, area.Bottom);
            Vector2 bottomRight = new Vector2(area.Right, area.Bottom);
            lineBatch.DrawLine(topLeft, topRight, frame);
            lineBatch.DrawLine(topRight, bottomRight, frame);
            lineBatch.DrawLine(bottomRight, bottomLeft, frame);
            lineBatch.DrawLine(bottomLeft, topLeft, frame);

            bool hasWalls = arena?.Walls != null && arena.Walls.Length >= 2;
            if (hasWalls)
            {
                Color wall = new Color(arena.Color.R, arena.Color.G, arena.Color.B, alpha);
                for (int i = 0; i + 1 < arena.Walls.Length; i += 2)
                {
                    lineBatch.DrawLine(Scale(arena.Walls[i], area), Scale(arena.Walls[i + 1], area), wall);
                }
            }
            lineBatch.End();

            if (!hasWalls)
            {
                const string unknown = "?";
                Vector2 size = screenManager.Font.MeasureString(unknown);
                screenManager.SpriteBatch.Begin();
                screenManager.SpriteBatch.DrawString(screenManager.Font, unknown,
                    new Vector2(area.Center.X - size.X / 2f, area.Center.Y - size.Y / 2f), new Color(255, 255, 255, alpha));
                screenManager.SpriteBatch.End();
            }
        }

        // a wall's ends are fractions of the arena's width and height
        static Vector2 Scale(Vector2 point, Rectangle area)
        {
            return new Vector2(area.Left + point.X * area.Width, area.Top + point.Y * area.Height);
        }

        /// <summary>
        /// Draws a ship, pointing up, about <paramref name="size"/> across.
        /// </summary>
        public void DrawShip(Ship ship, Color color, Vector2 center, float size)
        {
            if (!shipShapes.TryGetValue(ship, out VectorPolygon shape))
            {
                // a copy, so drawing the preview never moves the ship's own polygon
                shape = new VectorPolygon(ship.Shape.Points);
                shipShapes[ship] = shape;
                shipRadii[ship] = shape.Points.Max(p => p.Length());
            }

            float radius = Math.Max(shipRadii[ship], 1f);
            shape.Transform(Matrix.CreateScale(size / 2f / radius) * Matrix.CreateTranslation(center.X, center.Y, 0f));

            lineBatch.Begin();
            lineBatch.DrawPolygon(shape, color);
            lineBatch.End();
        }
    }
}
