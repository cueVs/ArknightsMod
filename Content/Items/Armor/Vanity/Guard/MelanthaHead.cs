using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ID;

namespace ArknightsMod.Content.Items.Armor.Vanity.Guard
{
	// See also: ExampleCostume
	[AutoloadEquip(EquipType.Head)]
	public class MelanthaHead : ModItem
	{
		public override void Load() {
			// The code below runs only if we're not loading on a server
			if (Main.netMode == NetmodeID.Server)
				return;

			// Add equip textures
			EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Back}", EquipType.Back, this);
		}
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Arknights Doctor's Hood");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			if (Main.netMode == NetmodeID.Server)
				return;
			ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
		}

		public override void SetDefaults() {
			Item.width = 32;
			Item.height = 52;
			Item.rare = ItemRarityID.Orange; // Same rarity as Arknights
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
