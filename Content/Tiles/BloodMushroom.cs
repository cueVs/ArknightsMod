using ArknightsMod.Common;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace ArknightsMod.Content.Tiles
{
	// 自然生成的 2x2 血蕈丛。不设置 tileCut/tileNoFail，所以武器挥砍/走过不会破坏它，
	// 只有镐子/钻头能挖掉它（MinPick 默认 0，任意镐子都能挖）。
	public class BloodMushroom : ModTile
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

			AddMapEntry(new Color(150, 20, 30), Language.GetText("Mods.ArknightsMod.Tiles.BloodMushroom.MapEntry"));

			DustType = DustID.Blood;
			HitSound = SoundID.Dig;

			RegisterItemDrop(ModContent.ItemType<Items.Material.BloodMushroom>());
		}

		public override void RandomUpdate(int i, int j) {
			RareCollectibleVisuals.EmitAmbientSparkle(i, j, 2, 2);
		}
	}
}
