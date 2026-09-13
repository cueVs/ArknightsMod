using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ArknightsMod.Content.Graphics3D;

public struct Vertex3D : IVertexType
{
    private static readonly VertexDeclaration _decl = new(new[]
    {
        new VertexElement(0,  VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Vector4, VertexElementUsage.Color, 0),
        new VertexElement(28, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
        new VertexElement(40, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
    });

    public Vector3 Position;
    public Vector4 Color;
    public Vector3 Normal;
    public Vector2 TexCoord;

    public Vertex3D(Vector3 position, Color color, Vector3 normal)
    {
        Position = position;
        Color = color.ToVector4();
        Normal = normal;
    }

    public Vertex3D(Vector3 position, Vector4 color, Vector3 normal)
    {
        Position = position;
        Color = color;
        Normal = normal;
    }


    public VertexDeclaration VertexDeclaration => _decl;
    public static VertexDeclaration Declaration => _decl;
}
