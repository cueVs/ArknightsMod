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
	// 4x2，锚定在水体里（AnchorType.Water），只有玩家背包里已经有别的自然物时才会尝试刷新，
	// 具体判定在 BoardVinePlayer 里。
	public class BoardVine : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = false;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Width = 4;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16 };
			TileObjectData.newTile.WaterPlacement = LiquidPlacement.OnlyInLiquid;
			RareCollectibleVisuals.ApplyDrawOffset();
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(70, 110, 70), Language.GetText("Mods.ArknightsMod.Tiles.BoardVine.MapEntry"));

			DustType = DustID.JungleGrass;
			HitSound = SoundID.Grass;

			RegisterItemDrop(ModContent.ItemType<Items.Material.BoardVine>());
		}

		public override void RandomUpdate(int i, int j) {
			RareCollectibleVisuals.EmitAmbientSparkle(i, j, 4, 2);
		}
	}
}
