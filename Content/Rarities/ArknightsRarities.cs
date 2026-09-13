using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Rarities
{
    public class ArknightsRarities : ModRarity
    {
        public override Color RarityColor => new Color((byte)(255), (byte)(20 ), (byte)(15 ),225);

        public override int GetPrefixedRarity(int offset, float valueMult)
        {
            return Type;
        }
    }
}