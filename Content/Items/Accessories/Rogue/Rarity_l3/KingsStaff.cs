using ArknightsMod.Common.Items;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
    public class KingsStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
			Item.value = Item.sellPrice(0, 6, 0, 0);
			Item.rare = ItemRarityID.Purple;
            Item.accessory = true;

            Item.GetGlobalItem<KingsGlobalItem>().isKingItem = true;
        }

        private bool HasFallenSovereignForm(Player player)
        {
            for (int i = 3; i < 8 + player.extraAccessorySlots; i++)
            {
                if (i < player.armor.Length)
                {
                    Item accessory = player.armor[i];

                    if (!accessory.IsAir && accessory.type == ModContent.ItemType<FallenSovereignForm>())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            bool isLowHealth = (float)player.statLife / player.statLifeMax2 <= 0.3f;
            bool hasFallenSovereign = HasFallenSovereignForm(player);


            if (isLowHealth || hasFallenSovereign)
            {

                if (player.statMana < player.statManaMax2 &&
                    player.GetModPlayer<CrownedAmplifierPlayer>().manaTimer++ >= 60)
                {
                    player.statMana += 10;
                    player.GetModPlayer<CrownedAmplifierPlayer>().manaTimer = 0;
                }


                player.GetModPlayer<CrownedAmplifierPlayer>().lifeRegenBonus =
                    (int)(player.statLifeMax2 * 0.015f);
            }
            else
            {

                player.GetModPlayer<CrownedAmplifierPlayer>().manaTimer = 0;
                player.GetModPlayer<CrownedAmplifierPlayer>().lifeRegenBonus = 0;
            }
        }
    }

    public class CrownedAmplifierPlayer : ModPlayer
    {
        public int manaTimer;
        public int lifeRegenBonus;

        public override void ResetEffects()
        {
            manaTimer = 0;
            lifeRegenBonus = 0;
        }

        public override void UpdateLifeRegen()
        {
            if (lifeRegenBonus > 0)
            {
                Player.lifeRegen += lifeRegenBonus;
            }
        }
    }
}