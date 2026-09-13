using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Tiles.Infrastructure
{
	public abstract class InfrastructureTile : ModTile
	{
		/// <summary>
		/// 是否与黑色舰桥体融合
		/// </summary>
		/// <returns></returns>
		public virtual bool ShouldMergeWithBlackBridgeStructure() => true;
		/// <summary>
		/// 基础设置，已包含：<code>
		/// Main.tileSolid[Type] = true;
		/// Main.tileBlockLight[Type] = true;
		/// Main.tileMergeDirt[Type] = true;
		/// 与黑色舰桥融合默认true，如需要修改请重写ShouldMergeWithBlackBridgeStructure()
		/// </code>
		/// 还需要写：<code>
		/// DustType = type;
		/// AddMapEntry(color);
		/// </summary>
		public virtual void SetDefaults() {

		}
		public sealed override void SetStaticDefaults()
		{
			Main.tileSolid[Type] = true;
			Main.tileBlockLight[Type] = true;
			Main.tileMergeDirt[Type] = true;
			if (ShouldMergeWithBlackBridgeStructure())
			{
				Main.tileMerge[Type][ModContent.TileType<BlackBridgeStructureTile>()] = true;
				Main.tileMerge[ModContent.TileType<BlackBridgeStructureTile>()][Type] = true;
			}
			SetDefaults();
		}
		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
	}
}
