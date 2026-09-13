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
	// 2x2，只在下雨（含雪地下的降水）时低概率长在草皮上，见 NaturalGrowthSystem 里的 ExtraCondition。
	public class WaveSpray : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = false;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.AnchorValidTiles = new int[] { TileID.Grass };
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			RareCollectibleVisuals.ApplyDrawOffset();
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(120, 180, 230), Language.GetText("Mods.ArknightsMod.Tiles.WaveSpray.MapEntry"));

			DustType = DustID.Water;
			HitSound = SoundID.Splash;

			RegisterItemDrop(ModContent.ItemType<Items.Material.WaveSpray>());
		}

		public override void RandomUpdate(int i, int j) {
			RareCollectibleVisuals.EmitAmbientSparkle(i, j, 2, 2);
		}
	}
}
