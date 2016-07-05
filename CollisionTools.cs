// *************************************************************************** 
// This is free and unencumbered software released into the public domain.
// 
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
// 
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
// 
// For more information, please refer to <http://unlicense.org>
// ***************************************************************************

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;

namespace Demo.PerPixelCollision
{
    public class CollisionTools
    {
        /// <summary>
        ///     Gets the transformation matrix from a sprite with a position (top left corner of the texture... down and right is
        ///     +)
        ///     that is rotated around an origin (that is subtracted from the position) and scaled by a value on the X-axis and
        ///     another value on the Y-axis (given by a Vector2).<br />
        ///     BEWARE! Only call this method if any of the input-parameters have changed.
        /// </summary>
        /// <param name="sprite">The sprite to get the transformation matrix from.</param>
        /// <param name="matrix">A reference to the matrix that will act as the result.</param>
        public static void GetTransformationMatrix(Sprite sprite, out Matrix matrix)
        {
            GetTransformationMatrix(sprite.Position, sprite.Origin, sprite.Scale, sprite.Rotation, out matrix);
        }

        /// <summary>
        ///     Gets the transformation matrix for a sprite with a position (top left corner of the texture... down and right is +)
        ///     that is rotated around an origin (that is subtracted from the position) and scaled by a value on the X-axis and
        ///     another value on the Y-axis (given by a Vector2).<br />
        ///     BEWARE! Only call this method if any of the input-parameters have changed.
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="position">The position.</param>
        /// <param name="matrix">A reference to the matrix that will act as the result.</param>
        /// <returns></returns>
        public static void GetTransformationMatrix(Vector2 position, Vector2 origin, Vector2 scale, float rotation,
            out Matrix matrix)
        {
            matrix = Matrix.Identity;
            // First we translate the texture to the zero-point of our coordinate system, so that we may...
            matrix *= Matrix.CreateTranslation(-origin.X, -origin.Y, 0f);
            // scale it from there and most importantly...
            matrix *= Matrix.CreateScale(new Vector3(scale.X, scale.Y, 1f));
            // rotate it around that origin-point instead of the top left corner (that would usually be the origin)...
            matrix *= Matrix.CreateRotationZ(rotation);
            // Then we translate it back, so that it behaves like a normal texture again.
            matrix *= Matrix.CreateTranslation(position.X, position.Y, 0);
        }

        /// <summary>
        ///     Gets the collision-data from a given and already loaded Texture2D.<br />
        ///     BEWARE! This operation is very expensive since it calls Texture2D.GetData().
        ///     So be sure to call this only once per texture and at the beginning of your game.
        ///     Save the result.
        /// </summary>
        /// <param name="tex">The texture to get the collision-data from.</param>
        /// <returns></returns>
        public static bool[] GetCollisionData(Texture2D tex)
        {
            return GetCollisionData(GetColorData(tex), tex.Width, tex.Height);
        }

        /// <summary>
        ///     Gets the collision-data from a color-array of a given and already loaded Texture2D.
        /// </summary>
        /// <param name="data">The data in the form of a color-array.</param>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height or the texture.</param>
        /// <returns></returns>
        public static bool[] GetCollisionData(Color[] data, int width, int height)
        {
            bool[] result = new bool[width*height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[x + y*width] = data[x + y*width].A != 0;
                }
            }
            return result;
        }

        /// <summary>
        ///     Gets the data from a given and already loaded Texture2D.<br />
        ///     BEWARE! This operation is very expensive since it calls Texture2D.GetData().
        ///     So be sure to call this only once per texture and at the beginning of your game.
        ///     Save the result.
        /// </summary>
        /// <param name="tex">The texture to get the data from.</param>
        /// <returns>A color array</returns>
        public static Color[] GetColorData(Texture2D tex)
        {
            var target = new RenderTarget2D(tex.GraphicsDevice, tex.Width, tex.Height);

            tex.GraphicsDevice.SetRenderTarget(target);
            tex.GraphicsDevice.Clear(Color.Transparent);

            var spriteBatch = new SpriteBatch(tex.GraphicsDevice);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.Draw(tex, tex.Bounds, Color.White);
            spriteBatch.End();
            
            tex.GraphicsDevice.SetRenderTarget(null);

            var result = new Color[target.Width * target.Height];
            target.GetData(result);
            return result;
        }

        /// <summary>
        ///     Determines if there is overlap of the non-transparent pixels between two
        ///     sprites.
        /// </summary>
        /// <param name="transformA">World transform of the first sprite.</param>
        /// <param name="widthA">Width of the first sprite's texture.</param>
        /// <param name="heightA">Height of the first sprite's texture.</param>
        /// <param name="dataA">Pixel color data of the first sprite.</param>
        /// <param name="transformB">World transform of the second sprite.</param>
        /// <param name="widthB">Width of the second sprite's texture.</param>
        /// <param name="heightB">Height of the second sprite's texture.</param>
        /// <param name="dataB">Pixel color data of the second sprite.</param>
        /// <returns>
        ///     True if non-transparent pixels overlap; <c>false</c> otherwise
        /// </returns>
        public static bool IntersectPixels(ref Matrix transformA, int widthA, int heightA, bool[] dataA, ref Matrix transformB,
            int widthB, int heightB, bool[] dataB)
        {
            if (dataA == null || dataB == null)
            {
                return false;
            }

            // Switch if A has more pixels than B.
            // This spares us many calculation steps since, in the worst case, we don't have a collision.
            // Then all of the points have to be checked and it's less time consuming to check all points
            // of the smaller texture against the corresponding ones in the bigger one than vice versa.
            Matrix transA = transformA;
            Matrix transB = transformB;
            int wA = widthA;
            int wB = widthB;
            int hA = heightA;
            int hB = heightB;
            bool[] dA = dataA;
            bool[] dB = dataB;
            if (widthA*heightA > widthB*heightB)
            {
                transA = transformB;
                transB = transformA;
                wA = widthB;
                wB = widthA;
                hA = heightB;
                hB = heightA;
                dA = dataB;
                dB = dataA;
            }
            
            // Calculate a matrix which transforms from A's local space into world space and then into B's local space.
            Matrix bInverted = Matrix.Invert(transB);
            Matrix transformAtoB = transA*bInverted;

            // When a point moves in A's local space, it moves in B's local space with a fixed direction and distance
            // proportional to the movement in A. This algorithm steps through A one pixel at a time along A's X and
            // Y axes Calculate the analogous steps in B:
            Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transformAtoB);
            Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transformAtoB);

            // Calculate the top left corner of A in B's local space.
            // This variable will be reused to keep track of the start of each row.
            Vector2 yPosInB = Vector2.Transform(Vector2.Zero, transformAtoB);

            for (int yA = 0; yA < hA; yA++)
            {
                Vector2 posInB = yPosInB;
                for (int xA = 0; xA < wA; xA++)
                {
                    // Round to the nearest pixel.
                    int xB = (int) Math.Round(posInB.X);
                    int yB = (int) Math.Round(posInB.Y);

                    // If the pixel lies within the bounds of B.
                    if (0 <= xB && xB < wB && 0 <= yB && yB < hB)
                    {
                        // Get the colors of the overlapping pixels.
                        bool colorA = dA[xA + yA*wA];
                        bool colorB = dB[xB + yB*wB];

                        // If both pixels are not completely transparent, then an intersection has been found.
                        if (colorA && colorB)
                        {
                            return true;
                        }
                    }
                    posInB += stepX;
                }
                yPosInB += stepY;
            }
            return false;
        }
    }
}