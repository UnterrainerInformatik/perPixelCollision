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
        public static Matrix GetTransformationMatrix(Sprite sprite)
        {
            return GetTransformationMatrix(sprite.Position, sprite.Origin, sprite.Scale, sprite.Rotation);
        }

        /// <summary>
        ///     Gets the transformation matrix for a sprite with a position (top left corner of the texture... down and right is +)
        ///     that is rotated around an origin (that is subtracted from the position) and scaled by a value on the X-axis and
        ///     another value on the Y-axis (given by a Vector2).
        /// </summary>
        /// <param name="origin">The origin.</param>
        /// <param name="scale">The scale.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public static Matrix GetTransformationMatrix(Vector2 position, Vector2 origin, Vector2 scale, float rotation)
        {
            Matrix result = Matrix.Identity;
            result *= Matrix.CreateTranslation(-origin.X, -origin.Y, 0f);
            result *= Matrix.CreateScale(new Vector3(scale.X, scale.Y, 1f));
            result *= Matrix.CreateRotationZ(rotation);
            result *= Matrix.CreateTranslation(position.X, position.Y, 0);
            return result;
        }

        public static bool[] GetCollisionData(Texture2D tex)
        {
            return GetCollisionData(GetData(tex), tex.Width, tex.Height);
        }

        public static bool[] GetCollisionData(Color[] data, int width, int height)
        {
            bool[] result = new bool[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[x + y * width] = data[x + y * width].A != 0;
                }
            }
            return result;
        }

        /// <summary>
        ///     Gets the data from a given and already loaded Texture2D.
        ///     BEWARE! This operation is very expensive since it calls Texture2D.GetData().
        ///     So be sure to call this only once per texture and at the beginning of your game.
        ///     Save the result.
        /// </summary>
        /// <param name="tex">The texture to get the data from.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">The surface-format you want to load is not supported.</exception>
        public static Color[] GetData(Texture2D tex)
        {
            Color[] result = new Color[tex.Width*tex.Height];
            try
            {
                switch (tex.Format)
                {
                    case SurfaceFormat.Color:
                        tex.GetData(0, new Rectangle(0, 0, tex.Width, tex.Height), result, 0, result.Length);
                        break;
                    default:
                        throw new NotSupportedException("The surface-format you want to load is not supported.");
                }
            }
            catch (InvalidOperationException)
            {
                return null;
            }
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
        public static bool IntersectPixels(Matrix transformA, int widthA, int heightA, bool[] dataA, Matrix transformB,
            int widthB, int heightB, bool[] dataB)
        {
            if (dataB == null)
            {
                return false;
            }
            // Calculate a matrix which transforms from A's local space into
            // world space and then into B's local space.
            Matrix bInverted = Matrix.Invert(transformB);
            Matrix transformAtoB = transformA*bInverted;

            // When a point moves in A's local space, it moves in B's local space with a
            // fixed direction and distance proportional to the movement in A.
            // This algorithm steps through A one pixel at a time along A's X and Y axes
            // Calculate the analogous steps in B:
            Vector2 stepX = Vector2.TransformNormal(Vector2.UnitX, transformAtoB);
            Vector2 stepY = Vector2.TransformNormal(Vector2.UnitY, transformAtoB);

            // Calculate the top left corner of A in B's local space.
            // This variable will be reused to keep track of the start of each row.
            Vector2 yPosInB = Vector2.Transform(Vector2.Zero, transformAtoB);

            // For each row of pixels in A.
            for (int yA = 0; yA < heightA; yA++)
            {
                // Start at the beginning of the row.
                Vector2 posInB = yPosInB;

                // For each pixel in this row.
                for (int xA = 0; xA < widthA; xA++)
                {
                    // Round to the nearest pixel.
                    int xB = (int) Math.Round(posInB.X);
                    int yB = (int) Math.Round(posInB.Y);

                    // If the pixel lies within the bounds of B.
                    if (0 <= xB && xB < widthB && 0 <= yB && yB < heightB)
                    {
                        // Get the colors of the overlapping pixels.
                        bool colorA = dataA[xA + yA*widthA];
                        bool colorB = dataB[xB + yB*widthB];

                        // If both pixels are not completely transparent,
                        if (colorA && colorB)
                        {
                            // then an intersection has been found.
                            return true;
                        }
                    }

                    // Move to the next pixel in the row.
                    posInB += stepX;
                }

                // Move to the next row.
                yPosInB += stepY;
            }

            // No intersection found.
            return false;
        }
    }
}