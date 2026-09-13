using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Common.Items;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
    public class KingsArmor : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[Type] = true;

        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
			Item.value = Item.sellPrice(0, 6, 0, 0);
			Item.rare = ItemRarityID.Purple;
            Item.accessory = true;

            Item.GetGlobalItem<KingsGlobalItem>().isKingItem = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {

            int baseDefense = GetProgressionDefense();
            player.statDefense += baseDefense;

            // 生命值转换防御（30%生命值按10:1转换）
            player.statDefense += (int)(player.statLifeMax2 * 0.3f / 10f);
            player.statLifeMax2 = (int)(player.statLifeMax2 * 0.7f);


        }


        private int GetProgressionDefense()
        {
            // 默认值（肉前）
            int defense = 3;

            // 检测世界进度
            if (NPC.downedMoonlord) return 20;    // 月球领主后
            if (NPC.downedAncientCultist) return 15; // 拜月教徒后
            if (NPC.downedPlantBoss) return 10;     // 世纪之花后

            // 检测机械三王是否全部击败
            bool allMechsDown = NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3;
            if (allMechsDown) return 7;

            if (Main.hardMode) return 5;   // 血肉墙后

            return defense;
        }
    }
}