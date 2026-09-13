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
	// 2x2，没有特殊刷新条件，长在普通草皮或苔藓上即可。
	public class WitheredMossBall : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = false;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.AnchorValidTiles = new int[] {
				TileID.Grass, TileID.BlueMoss, TileID.GreenMoss, TileID.PurpleMoss, TileID.RedMoss, TileID.BrownMoss
			};
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			RareCollectibleVisuals.ApplyDrawOffset();
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(90, 80, 60), Language.GetText("Mods.ArknightsMod.Tiles.WitheredMossBall.MapEntry"));

			DustType = DustID.Grass;
			HitSound = SoundID.Grass;

			RegisterItemDrop(ModContent.ItemType<Items.Material.WitheredMossBall>());
		}

		public override void RandomUpdate(int i, int j) {
			RareCollectibleVisuals.EmitAmbientSparkle(i, j, 2, 2);
		}
	}
}
