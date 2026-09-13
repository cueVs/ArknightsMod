using Terraria;
using Terraria.ModLoader;
using System;
using Terraria.ID;
namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
    public class PaleCorolla : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 28;
			Item.value = Item.sellPrice(0, 6, 0, 0);
			Item.rare = ItemRarityID.Purple;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PaleCorollaPlayer>().count++;
        }
    }

    public class PaleCorollaPlayer : ModPlayer
    {
        public int count; // 记录佩戴的饰品数量

        public override void ResetEffects()
        {
            count = 0;
        }


        public float Multiplier => (float)Math.Pow(1.3, count);

        public override void UpdateLifeRegen()
        {
            if (count > 0)
            {
 
                Player.lifeRegen = (int)(Player.lifeRegen * Multiplier);
            }
        }

        public override void GetHealLife(Item item, bool quickHeal, ref int healValue)
        {
            if (count > 0)
            {

                healValue = (int)(healValue * Multiplier);
            }
        }
    }
}