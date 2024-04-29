﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.ModLoader.IO;
using System.IO;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;
using Terraria.Utilities;
using Terraria.Localization;

namespace ArknightsMod.Content.Items.Consumables
{
	public class MudrockDefault : ModItem
	{
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Example CanStack Item: Gift Bag");
			// Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}"); // References a language key that says "Right Click To Open" in the language of the game

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults() {
			Item.maxStack = 9999;
			Item.consumable = true;
			Item.width = 38;
			Item.height = 50;
			Item.rare = ItemRarityID.White;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRule rule = ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Defender.MudrockHelmet>(), 1);
			rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Defender.MudrockChestplate>(), 1));
			rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Defender.MudrockGreaves>(), 1));

			itemLoot.Add(rule);
		}

		//public override void AddRecipes() {
		//	Recipe recipe = CreateRecipe();
		//	recipe.AddRecipeGroup(RecipeGroupID.Wood, 1);
		//	recipe.AddTile(TileID.WorkBenches);
		//	recipe.Register();
		//}
	}
}
