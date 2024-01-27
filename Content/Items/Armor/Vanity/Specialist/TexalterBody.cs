using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ModLoader;
using Terraria.ID;

namespace ArknightsMod.Content.Items.Armor.Vanity.Specialist
{
	// See also: ExampleCostume
	[AutoloadEquip(EquipType.Body)]
	public class TexalterBody : ModItem
	{
		public override void Load() {
			// The code below runs only if we're not loading on a server
			if (Main.netMode == NetmodeID.Server)
				return;

			// Add equip textures
			EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Back}", EquipType.Back, this);
		}
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Arknights Doctor's Jacket");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
			if (Main.netMode == NetmodeID.Server)
				return;

			int cape = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Back);

			ArmorIDs.Body.Sets.HidesTopSkin[Item.bodySlot] = true;
			ArmorIDs.Body.Sets.HidesArms[Item.bodySlot] = true;
			ArmorIDs.Body.Sets.IncludedCapeBack[Item.bodySlot] = cape;
			ArmorIDs.Body.Sets.IncludedCapeBackFemale[Item.bodySlot] = cape;
		}

		public override void SetDefaults() {
			Item.width = 38;
			Item.height = 26;
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
