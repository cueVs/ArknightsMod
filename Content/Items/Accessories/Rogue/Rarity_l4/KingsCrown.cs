using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Common.Items;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4
{
    public class KingsCrown : ModItem
    {
        public override void SetStaticDefaults()
        {
            // …Ë÷√king±Í«©
            ItemID.Sets.ItemNoGravity[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
			Item.value = Item.sellPrice(0, 16, 0, 0);
			Item.rare = ItemRarityID.Purple;
            Item.accessory = true; 

     
            Item.GetGlobalItem<KingsGlobalItem>().isKingItem = true;
        }

        private int CountKingItems(Player player)
        {
            int count = 0;
            for (int i = 3; i < 8; i++) 
            {
                Item accessory = player.armor[i];
                if (!accessory.IsAir &&
                    accessory.type != Type && 
                    accessory.TryGetGlobalItem(out KingsGlobalItem kingItem) &&
                    kingItem.isKingItem)
                {
                    count++;
                }
            }
            return count;
        }

        private bool HasFallenSovereignForm(Player player)
        {
            for (int i = 3; i < 8 + player.extraAccessorySlots; i++) 
            {
                if (i < player.armor.Length)
                {
                   Item accessory = player.armor[i];
                   
                    if (!accessory.IsAir && accessory.type == ModContent.ItemType<Rarity_l3.FallenSovereignForm>())
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
                int kingCount = CountKingItems(player) + 1; 

                float damageBonus = 0f;

                if (kingCount >= 3)
                {
                    damageBonus = 1.5f; 
                }
                else if (kingCount > 0) 
                {
                    damageBonus = 0.5f; 
                }
       

                if (damageBonus > 0f)
                {
                    player.GetDamage(DamageClass.Generic) += damageBonus;
                }
            }
        }
    }
}