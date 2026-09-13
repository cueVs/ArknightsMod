using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using System;
namespace ArknightsMod.Content.Projectiles.Rogue.Dedication
{
    public static class VertexDrawingHelper
    {
        private static BasicEffect _basicEffect;
        private static GraphicsDevice _graphicsDevice;

        private static void EnsureEffect(GraphicsDevice device)
        {
            if (_basicEffect == null || _graphicsDevice != device)
            {
                _graphicsDevice = device;
                _basicEffect = new BasicEffect(device);
                _basicEffect.TextureEnabled = true;
                _basicEffect.VertexColorEnabled = false;
                _basicEffect.Alpha = 1.0f;
            }
        }

        public static void DrawTexture(Texture2D texture, Rectangle destinationRectangle, Color color, bool additive = true, float rotation = 0f)
        {
            GraphicsDevice device = Main.graphics.GraphicsDevice;
            EnsureEffect(device);

          
            Vector2 center = new Vector2(destinationRectangle.X + destinationRectangle.Width / 2f,
                                        destinationRectangle.Y + destinationRectangle.Height / 2f);
            float halfWidth = destinationRectangle.Width / 2f;
            float halfHeight = destinationRectangle.Height / 2f;

          
            Vector2 topLeft = new Vector2(-halfWidth, -halfHeight);
            Vector2 topRight = new Vector2(halfWidth, -halfHeight);
            Vector2 bottomLeft = new Vector2(-halfWidth, halfHeight);
            Vector2 bottomRight = new Vector2(halfWidth, halfHeight);

        
            float cos = (float)Math.Cos(rotation);
            float sin = (float)Math.Sin(rotation);

            Vector2 rotatedTopLeft = new Vector2(topLeft.X * cos - topLeft.Y * sin, topLeft.X * sin + topLeft.Y * cos);
            Vector2 rotatedTopRight = new Vector2(topRight.X * cos - topRight.Y * sin, topRight.X * sin + topRight.Y * cos);
            Vector2 rotatedBottomLeft = new Vector2(bottomLeft.X * cos - bottomLeft.Y * sin, bottomLeft.X * sin + bottomLeft.Y * cos);
            Vector2 rotatedBottomRight = new Vector2(bottomRight.X * cos - bottomRight.Y * sin, bottomRight.X * sin + bottomRight.Y * cos);

 
            Vector3 vTopLeft = new Vector3(center.X + rotatedTopLeft.X, center.Y + rotatedTopLeft.Y, 0);
            Vector3 vTopRight = new Vector3(center.X + rotatedTopRight.X, center.Y + rotatedTopRight.Y, 0);
            Vector3 vBottomLeft = new Vector3(center.X + rotatedBottomLeft.X, center.Y + rotatedBottomLeft.Y, 0);
            Vector3 vBottomRight = new Vector3(center.X + rotatedBottomRight.X, center.Y + rotatedBottomRight.Y, 0);

            VertexPositionTexture[] vertices = new VertexPositionTexture[6];
            vertices[0] = new VertexPositionTexture(vTopLeft, new Vector2(0, 0));
            vertices[1] = new VertexPositionTexture(vTopRight, new Vector2(1, 0));
            vertices[2] = new VertexPositionTexture(vBottomLeft, new Vector2(0, 1));
            vertices[3] = new VertexPositionTexture(vBottomRight, new Vector2(1, 1));
            vertices[4] = new VertexPositionTexture(vBottomLeft, new Vector2(0, 1));
            vertices[5] = new VertexPositionTexture(vTopRight, new Vector2(1, 0));

            int width = device.Viewport.Width;
            int height = device.Viewport.Height;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, width, height, 0, -1, 1);
            _basicEffect.World = Matrix.Identity;
            _basicEffect.View = Matrix.Identity;
            _basicEffect.Projection = projection;
            _basicEffect.Texture = texture;
            _basicEffect.DiffuseColor = color.ToVector3();
            _basicEffect.Alpha = color.A / 255f;

            RasterizerState originalRasterizerState = device.RasterizerState;
            BlendState originalBlendState = device.BlendState;
            DepthStencilState originalDepthStencilState = device.DepthStencilState;

            device.RasterizerState = RasterizerState.CullNone;

            if (additive)
            {
                device.BlendState = BlendState.Additive;
            }
            else
            {
                device.BlendState = BlendState.AlphaBlend;
            }

            device.DepthStencilState = DepthStencilState.None;

            foreach (EffectPass pass in _basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, 2);
            }

            device.RasterizerState = originalRasterizerState;
            device.BlendState = originalBlendState;
            device.DepthStencilState = originalDepthStencilState;
        }
    }
}