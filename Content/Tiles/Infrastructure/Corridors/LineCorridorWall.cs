using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace ArknightsMod.Content.Tiles.Infrastructure.Corridors
{
    public class LineCorridorWall : InfrastructureWall
    {
        public override void SetDefaults()
        {
            DustType = DustID.Stone;
            Main.wallLight[Type] = false;
            AddMapEntry(new Color(104, 108, 112));
        }
    }
}
