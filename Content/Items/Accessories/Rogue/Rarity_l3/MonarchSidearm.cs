using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Common.Items;
namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
    public class MonarchSidearm : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 20;
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
                player.GetAttackSpeed(DamageClass.Generic) += 0.50f;
            }
        }
    }
}