using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ID;

namespace ArknightsMod.Content.Items.Armor.Vanity.Specialist
{
	// See also: ExampleCostume
	[AutoloadEquip(EquipType.Legs)]
	public class TexalterLegs : ModItem
	{
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Arknights Doctor's Pants");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

			if (Main.netMode == NetmodeID.Server)
				return;
			ArmorIDs.Legs.Sets.HidesBottomSkin[Item.legSlot] = true;
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 8;
			Item.rare = ItemRarityID.LightPurple; // Same rarity as Arknights (LightPurple = star 6)
			Item.vanity = true;
		}

		//public override void AddRecipes()
		//{
		//    Recipe recipe = CreateRecipe();
		//    recipe.AddRecipeGroup(RecipeGroupID.Wood, 2);
		//    recipe.AddTile(TileID.WorkBenches);
		//    recipe.Register();
		//}
	}
}
