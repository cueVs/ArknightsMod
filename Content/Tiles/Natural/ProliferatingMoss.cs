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
	// 2x2，只长在丛林草地上（丛林地形本身就是靠这个地面种类判定的，不需要额外条件）。
	public class ProliferatingMoss : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = false;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.AnchorValidTiles = new int[] { TileID.JungleGrass };
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			RareCollectibleVisuals.ApplyDrawOffset();
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(90, 160, 60), Language.GetText("Mods.ArknightsMod.Tiles.ProliferatingMoss.MapEntry"));

			DustType = DustID.JungleGrass;
			HitSound = SoundID.Grass;

			RegisterItemDrop(ModContent.ItemType<Items.Material.ProliferatingMoss>());
		}

		public override void RandomUpdate(int i, int j) {
			RareCollectibleVisuals.EmitAmbientSparkle(i, j, 2, 2);
		}
	}
}
