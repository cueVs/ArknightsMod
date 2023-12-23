using Terraria;
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
	public class GachaBag : ModItem
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
			Item.height = 32;
			Item.rare = ItemRarityID.White;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRule Chen = ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Guard.ChenHead>(), 2); //1 in 2 (= 50%)
			Chen.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Guard.ChenBody>(), 1));
			Chen.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Guard.ChenLegs>(), 1));

			IItemDropRule Amiya = ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Caster.AmiyaHead>(), 1);
			Amiya.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Caster.AmiyaBody>(), 1));
			Amiya.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Caster.AmiyaLegs>(), 1));

			IItemDropRule rule = Chen;
			rule.OnFailedRoll(Amiya); //Actual Chance of Amiya = (100% - Chen(50%))/Amiya(100%)
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
