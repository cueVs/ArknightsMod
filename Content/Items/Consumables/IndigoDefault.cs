using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Consumables
{
	public class IndigoDefault : ModItem
	{
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Example CanStack Item: Gift Bag");
			// Tooltip.SetDefault("{$CommonItemTooltip.RightClickToOpen}"); // References a language key that says "Right Click To Open" in the language of the game

			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.maxStack = 9999;
			Item.consumable = true;
			Item.width = 40;
			Item.height = 56;
			Item.rare = ItemRarityID.White;
		}

		public override bool CanRightClick() {
			return true;
		}

		public override void ModifyItemLoot(ItemLoot itemLoot) {
			IItemDropRule rule = ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Caster.IndigoHead>(), 1);
			rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Caster.IndigoBody>(), 1));
			rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.Caster.IndigoLegs>(), 1));

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
