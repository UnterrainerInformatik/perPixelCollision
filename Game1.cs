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
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Sprites;

namespace Demo.PerPixelCollision
{
    public class Game1 : Game
    {
        public GraphicsDeviceManager GraphicsDeviceManager;
        private SpriteBatch spriteBatch;

        private Texture2D backgroundTexture;

        private Sprite axeSprite;
        private bool[] axeCollisionData;
        private Matrix axeTransform;
        
        private Sprite spikeyBallSprite;
        private bool[] spikeyBallCollisionData;
        private Matrix spikeyBallTransform;

        public Game1()
        {
            GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Window.Position = Point.Zero;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            backgroundTexture = Content.Load<Texture2D>("bg_sharbi");

            Texture2D axeTexture = Content.Load<Texture2D>("axe");
            axeSprite = new Sprite(axeTexture)
            {
                Origin = new Vector2(243, 679),
                Position = new Vector2(400, 0),
                Scale = Vector2.One*0.5f
            };
            axeCollisionData = CollisionTools.GetCollisionData(axeTexture);

            Texture2D spikeyBallTexture = Content.Load<Texture2D>("spike_ball");
            spikeyBallSprite = new Sprite(spikeyBallTexture)
            {
                Position = new Vector2(400, 340)
            };
            spikeyBallCollisionData = CollisionTools.GetCollisionData(spikeyBallTexture);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            KeyboardState keyboardState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            axeSprite.Rotation = MathHelper.ToRadians(180) +
                                  MathHelper.PiOver2*0.8f*(float) Math.Sin(gameTime.TotalGameTime.TotalSeconds);

            spikeyBallSprite.Rotation -= deltaTime*2.5f;
            spikeyBallSprite.Position = new Vector2(mouseState.X, mouseState.Y);

            CollisionTools.GetTransformationMatrix(axeSprite, out axeTransform);
            CollisionTools.GetTransformationMatrix(spikeyBallSprite, out spikeyBallTransform);

            bool collision = CollisionTools.IntersectPixels(ref axeTransform, axeSprite.TextureRegion.Texture.Width,
                axeSprite.TextureRegion.Texture.Height, axeCollisionData, ref spikeyBallTransform,
                spikeyBallSprite.TextureRegion.Texture.Width, spikeyBallSprite.TextureRegion.Texture.Height,
                spikeyBallCollisionData);

            if (collision)
            {
                axeSprite.Color = Color.Red;
                spikeyBallSprite.Color = Color.Red;
            }
            else
            {
                axeSprite.Color = Color.White;
                spikeyBallSprite.Color = Color.White;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.Draw(backgroundTexture,
                new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.Draw(axeSprite);
            spriteBatch.Draw(spikeyBallSprite);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}