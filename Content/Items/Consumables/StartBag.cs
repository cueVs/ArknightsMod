using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.ModLoader.IO;
using System.IO;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;

namespace ArknightsMod.Content.Items.Consumables
{
	public class StartBag: ModItem
	{
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Example CanStack Item: Gift Bag");
			// Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}"); // References a language key that says "Right Click To Open" in the language of the game

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			Item.maxStack = 999;
			Item.consumable = true;
			Item.width = 38;
			Item.height = 30;
			Item.rare = ItemRarityID.White;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void ModifyItemLoot(ItemLoot itemLoot) {
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.DoctorHood>()));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.DoctorJacket>()));
            itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.DoctorPants>()));
        }
    }
}
