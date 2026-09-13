using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4
{
    public class ScatteredPoems : ModItem
    {
        

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
			Item.value = Item.sellPrice(0, 16, 0, 0);
			Item.rare = ItemRarityID.Master;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            // 召唤伤害提升
            player.GetDamage(DamageClass.Summon) += 0.30f;

            // 召唤暴击率(无效)
            player.GetCritChance(DamageClass.Summon) += 5;

            // 增加召唤栏（方舟原版为不消耗部署栏）
            player.maxMinions += 3;
        }
    }
}