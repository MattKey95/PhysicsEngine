//This file is only used to draw debug information (lines) and is not used for anything else.

// Author: Tigran Gasparian
// Source: http://blog.tigrangasparian.com
// License: In short, the author is not responsible for anything. You can do whatever you want with this code.
//          It would be nice if you kept the above credits though :)
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LineBatch
{
    /// <summary>
    /// Contains extension methods of the spritebatch class to draw lines.
    /// </summary>
    static class SpriteBatchEx
    {
        /// <summary>
        /// Draws a single line. 
        /// Require SpriteBatch.Begin() and SpriteBatch.End()
        /// </summary>
        /// <param name="begin">Begin point.</param>
        /// <param name="end">End point.</param>
        /// <param name="color">The color.</param>
        /// <param name="width">The width.</param>
        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
        {
            Rectangle r = new Rectangle((int)begin.X, (int)begin.Y, (int)(end - begin).Length()+width, width);
            Vector2 v = Vector2.Normalize(begin - end);
            float angle = (float)Math.Acos(Vector2.Dot(v, -Vector2.UnitX));
            if (begin.Y > end.Y) angle = MathHelper.TwoPi - angle;
            spriteBatch.Draw(TexGen.White, r, null, color, angle, Vector2.Zero, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Draws a single line. 
        /// Doesn't require SpriteBatch.Begin() or SpriteBatch.End()
        /// </summary>
        /// <param name="begin">Begin point.</param>
        /// <param name="end">End point.</param>
        /// <param name="color">The color.</param>
        /// <param name="width">The width.</param>
        public static void DrawSingleLine(this SpriteBatch spriteBatch, Vector2 begin, Vector2 end, Color color, int width = 1)
        {
            spriteBatch.Begin();
            spriteBatch.DrawLine(begin, end, color, width);
            spriteBatch.End();
        }

        /// <summary>
        /// Draws a poly line.
        /// Doesn't require SpriteBatch.Begin() or SpriteBatch.End()
        /// <param name="points">The points.</param>
        /// <param name="color">The color.</param>
        /// <param name="width">The width.</param>
        /// <param name="closed">Whether the shape should be closed.</param>
        public static void DrawPolyLine(this SpriteBatch spriteBatch, Vector2[] points, Color color, int width = 1, bool closed = false)
        {
            spriteBatch.Begin();
            for (int i = 0; i < points.Length - 1; i++)
                spriteBatch.DrawLine(points[i], points[i + 1], color, width);
            if (closed)
                spriteBatch.DrawLine(points[points.Length - 1], points[0], color, width);
            spriteBatch.End();
        }

        public static Texture2D CreateCircle(int radius)
        {
            int outerRadius = radius * 2 + 2; // So circle doesn't go out of bounds
            Texture2D texture = new Texture2D(GraphicsDevice, outerRadius, outerRadius);

            Color[] data = new Color[outerRadius * outerRadius];

            // Colour the entire texture transparent first.
            for (int i = 0; i < data.Length; i++)
                data[i] = Color.Transparent;

            // Work out the minimum step necessary using trigonometry + sine approximation.
            double angleStep = 1f / radius;

            for (double angle = 0; angle < Math.PI * 2; angle += angleStep)
            {
                // Use the parametric definition of a circle: http://en.wikipedia.org/wiki/Circle#Cartesian_coordinates
                int x = (int)Math.Round(radius + radius * Math.Cos(angle));
                int y = (int)Math.Round(radius + radius * Math.Sin(angle));

                data[y * outerRadius + x + 1] = Color.White;
            }

            texture.SetData(data);
            return texture;
        }

        /// <summary>
        /// The graphics device, set this before drawing lines
        /// </summary>
        public static GraphicsDevice GraphicsDevice;

        /// <summary>
        /// Generates a 1 pixel white texture used to draw lines.
        /// </summary>
        static class TexGen
        {
            static Texture2D white = null;
            /// <summary>
            /// Returns a single pixel white texture, if it doesn't exist, it creates one
            /// </summary>
            /// <exception cref="System.Exception">Please set the SpriteBatchEx.GraphicsDevice to your graphicsdevice before drawing lines.</exception>
            public static Texture2D White
            {
                get
                {
                    if (white == null)
                    {
                        if (SpriteBatchEx.GraphicsDevice == null)
                            throw new Exception("Please set the SpriteBatchEx.GraphicsDevice to your GraphicsDevice before drawing lines.");
                        white = new Texture2D(SpriteBatchEx.GraphicsDevice, 1, 1);
                        Color[] color = new Color[1];
                        color[0] = Color.White;
                        white.SetData<Color>(color);
                    }
                    return white;
                }
            }
        }

    }
}
