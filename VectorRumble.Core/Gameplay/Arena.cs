using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.Xna.Framework;

namespace VectorRumble
{
    internal class Arena
    {
        /// <summary>
        /// The name of this arena
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Colour of the walls
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// The walls in the game.
        /// </summary>
        public Vector2[] Walls { get; set; }

        /// <summary>
        /// Draw the walls.
        /// </summary>
        /// <param name="lineBatch">The LineBatch to render to.</param>
        public void DrawWalls(LineBatch lineBatch)
        {
            if (lineBatch == null)
            {
                throw new ArgumentNullException(nameof(lineBatch));
            }

            // draw each wall-line
            if (Walls != null)
            {
                int viewportWidth = lineBatch.GraphicsDevice.Viewport.Width;
                int viewportHeight = lineBatch.GraphicsDevice.Viewport.Height;

                Vector2 startLine;
                Vector2 endLine;

                for (int wall = 0; wall < Walls.Length / 2; wall++)
                {
                    startLine = new Vector2(Walls[wall * 2].X * viewportWidth, Walls[wall * 2].Y * viewportHeight);
                    endLine = new Vector2(Walls[wall * 2 + 1].X * viewportWidth, Walls[wall * 2 + 1].Y * viewportHeight);
                    lineBatch.DrawLine(startLine, endLine, Color);
                }
            }
        }

        #region Loading
        internal static Arena Load(XElement root)
        {
            var result = new Arena
            {
                Name = root.Element("Name").Value,
                Color = Helper.ColorParse(root.Element("Color").Value),
            };

            var points = root.XPathSelectElement("./Walls").Value;
            var p = points.Split(' ');
            var plist = new List<Vector2>();
            for (int i = 0; i < p.Length; i += 2)
            {
                plist.Add(new Vector2
                {
                    X = float.Parse(p[i]),
                    Y = float.Parse(p[i + 1]),
                });
            }

            result.Walls = plist.ToArray();
            return result;
        }
        #endregion
    }
}
