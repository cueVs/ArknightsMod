using ArknightsMod.Common;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles.Natural
{
	// 2x4 的回声玉米，只能长在城镇范围内（附近有已安家的镇民）的草皮上，
	// “城镇范围”的判定见 Systems/NaturalGrowthSystem.cs 的 IsNearTownNPC。
	public class EchoCorn : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = false;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 4;
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16 };
			TileObjectData.newTile.AnchorValidTiles = new int[] { TileID.Grass };
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			RareCollectibleVisuals.ApplyDrawOffset();
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(230, 210, 90), Language.GetText("Mods.ArknightsMod.Tiles.EchoCorn.MapEntry"));

			DustType = DustID.GrassBlades;
			HitSound = SoundID.Grass;

			RegisterItemDrop(ModContent.ItemType<Items.Material.EchoCorn>());
		}

		public override void RandomUpdate(int i, int j) {
			RareCollectibleVisuals.EmitAmbientSparkle(i, j, 2, 4);
		}
	}
}
