using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ObjectData;

namespace ArknightsMod.Common
{
	// 稀有自然采集物（血蕈/霜晶树/回声玉米...）共用的绘制表现：整体绘制向下偏移，让贴图底部
	// 预留的“嵌入地面”像素被地面方块盖住，并偶尔冒出闪亮粒子。各自的 ModTile 在 SetStaticDefaults
	// 里调 ApplyDrawOffset，在 RandomUpdate 里调 EmitAmbientSparkle。
	// 偏移量取 2px：实测这批贴图最底部 2 行像素统一画成了同一种深绿色（贴近草地色），
	// 是特意留出来给地面方块盖住的“埋入”占位色，1px 盖不全会露馅。
	// 恋家果吊在天花板上，是唯一的例外，没有走这个共用偏移（它自己在 HomesickFruit.cs 里单独设了向上4px）。
	public static class RareCollectibleVisuals
	{
		public const int DrawYOffsetPixels = 2;

		public static void ApplyDrawOffset() {
			TileObjectData.newTile.DrawYOffset = DrawYOffsetPixels;
		}

		public static void EmitAmbientSparkle(int i, int j, int width, int height) {
			if (!Main.rand.NextBool(90)) return; // 出现得慢一点，不要糊成一片

			Vector2 pos = new Vector2(i, j) * 16f + new Vector2(Main.rand.Next(width * 16), Main.rand.Next(height * 16));
			Dust dust = Dust.NewDustPerfect(pos, DustID.TreasureSparkle, new Vector2(0f, -0.3f), 150, default, 1f);
			dust.noGravity = true;
		}
	}
}
