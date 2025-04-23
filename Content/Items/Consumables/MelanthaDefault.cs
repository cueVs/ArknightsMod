using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Consumables
{
	public class MelanthaDefault : ModItem
	{
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Example CanStack Item: Gift Bag");
			// Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}"); // References a language key that says "Right Click To Open" in the language of the game

			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.maxStack = 9999;
			Item.consumable = true;
			Item.width = 36;
			Item.height = 58;
			Item.rare = ItemRarityID.White;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRule rule = ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Guard.MelanthaHead>(), 1);
			rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Guard.MelanthaBody>(), 1));
			rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Guard.MelanthaLegs>(), 1));

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
