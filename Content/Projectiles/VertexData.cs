using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArknightsMod.Content.Projectiles
{
    public struct VertexData : IVertexType
    {
        private static VertexDeclaration _vertexDeclaration = new VertexDeclaration(new VertexElement[3]
        {
            new VertexElement(0,VertexElementFormat.Vector2,VertexElementUsage.Position,0),
            new VertexElement(8,VertexElementFormat.Vector3,VertexElementUsage.TextureCoordinate,0),
            new VertexElement(20,VertexElementFormat.Color,VertexElementUsage.Color,0)
        });
        public Vector2 Position;
        public Vector3 TexCoord;
        public Color Color;
        public VertexData(Vector2 position, Vector3 texCoord, Color color)
        {
            Position = position;
            TexCoord = texCoord;
            Color = color;
        }
        public VertexDeclaration VertexDeclaration
        {
            get => _vertexDeclaration;
        }
    }
}