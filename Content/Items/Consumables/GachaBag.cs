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

			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.ChenHead>()));
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.ChenBody>()));
			itemLoot.Add(ItemDropRule.Common(ModContent.ItemType<Armor.Vanity.ChenLegs>()));

			//  int num = Main.rand.Next(4))
			//  switch (num)
			//  {
			//      case 0:
			//  }
		}

		//public override void AddRecipes() {
		//	Recipe recipe = CreateRecipe();
		//	recipe.AddRecipeGroup(RecipeGroupID.Wood, 1);
		//	recipe.AddTile(TileID.WorkBenches);
		//	recipe.Register();
		//}
	}
}
