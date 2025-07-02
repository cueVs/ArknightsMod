using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

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
			Item.ResearchUnlockCount = 1;
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
	public class MelanthaHeadLayer : PlayerDrawLayer
	{
		protected override void Draw(ref PlayerDrawSet drawInfo) {
			var drawPlayer = drawInfo.drawPlayer;
			var texture = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Armor/Vanity/Guard/MelanthaHead_Back", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
			int dyeShader = drawPlayer.dye?[1].dye ?? 0;
			Vector2 drawPosition = drawInfo.Center - Main.screenPosition;
			drawPosition += new Vector2(0, drawPlayer.height - 48f);
			drawPosition = new Vector2((int)drawPosition.X, (int)drawPosition.Y);

			if (drawPlayer.armor[10].type == ModContent.ItemType<MelanthaHead>()) {
				var data = new DrawData(texture, drawPosition, null,
					drawInfo.colorArmorBody, drawPlayer.fullRotation, drawInfo.bodyVect, 1f, drawInfo.playerEffect, 0) {
					shader = dyeShader
				};
				drawInfo.DrawDataCache.Add(data);
			}
		}
		public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);
	}
}
