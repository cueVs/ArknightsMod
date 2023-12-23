using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ID;

namespace ArknightsMod.Content.Items.Armor.Vanity.Caster
{
	// See also: ExampleCostume
	[AutoloadEquip(EquipType.Legs)]
	public class AmiyaLegs : ModItem
	{
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Arknights Doctor's Pants");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			if (Main.netMode == NetmodeID.Server)
				return;
			ArmorIDs.Legs.Sets.HidesBottomSkin[Item.legSlot] = true;
		}

		public override void SetDefaults() {
			Item.width = 18;
			Item.height = 8;
			Item.rare = ItemRarityID.Pink; // Same rarity as Arknights
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
