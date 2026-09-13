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
	// 唯一一个挂在天花板上的稀有采集物：AnchorTop 而不是 AnchorBottom，所以不套用通用的
	// RareCollectibleVisuals 下移配置，单独设置向上偏移4像素，但保留稀有物的闪亮效果
	// （这里用金色火焰粒子，对应“淡金色光芒”）。
	// 估价不走 ModPlayer——种下去之后的估价挂在 HomesickFruitTileEntity 上，见那边的注释。
	public class HomesickFruit : ModTile
	{
		public override void SetStaticDefaults() {
			Main.tileFrameImportant[Type] = true;
			Main.tileLighted[Type] = false;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
			TileObjectData.newTile.Width = 3;
			TileObjectData.newTile.Height = 3;
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16 };
			TileObjectData.newTile.AnchorValidTiles = new int[] {
				TileID.Grass, TileID.BlueMoss, TileID.GreenMoss, TileID.PurpleMoss, TileID.RedMoss, TileID.BrownMoss
			};
			TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
			TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.DrawYOffset = -6;
			TileObjectData.addTile(Type);

			AddMapEntry(new Color(230, 200, 140), Language.GetText("Mods.ArknightsMod.Tiles.HomesickFruit.MapEntry"));

			DustType = DustID.GoldFlame;
			HitSound = SoundID.Grass;
		}

		public override void RandomUpdate(int i, int j) {
			if (!Main.rand.NextBool(60)) return;

			Point16 origin = TileObjectData.TopLeft(i, j);
			Vector2 pos = new Vector2(origin.X, origin.Y) * 16f + new Vector2(Main.rand.Next(48), Main.rand.Next(48));
			Dust dust = Dust.NewDustPerfect(pos, DustID.GoldFlame, new Vector2(0f, 0.2f), 100, default, 1f);
			dust.noGravity = true;
		}

		public override void PlaceInWorld(int i, int j, Item item) {
			int teIndex = ModContent.GetInstance<HomesickFruitTileEntity>().Place(i, j);
			if (Terraria.DataStructures.TileEntity.ByID.TryGetValue(teIndex, out var te) && te is HomesickFruitTileEntity fruitTe)
				fruitTe.Value = item.value;
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY) {
			int value = HomesickFruitTileEntity.MaxValue;
			int teId = ModContent.GetInstance<HomesickFruitTileEntity>().Find(i, j);
			if (teId != -1 && Terraria.DataStructures.TileEntity.ByID.TryGetValue(teId, out var te) && te is HomesickFruitTileEntity fruitTe)
				value = fruitTe.Value;

			ModContent.GetInstance<HomesickFruitTileEntity>().Kill(i, j);

			int itemIndex = Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 48, 48, ModContent.ItemType<Items.Material.HomesickFruit>());
			Main.item[itemIndex].value = value;
			if (Main.netMode == NetmodeID.Server)
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex, 1f);
		}
	}
}
