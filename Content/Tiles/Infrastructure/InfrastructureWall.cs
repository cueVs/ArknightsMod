using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Tiles.Infrastructure
{
	public abstract class InfrastructureWall : ModWall
	{
		/// <summary>
		/// 基础设置，已包含：<code>
		/// Main.wallHouse[Type] = true;
		/// </code>
		/// 还需要写：<code>
		/// DustType = type;
		/// AddMapEntry(color);
		/// RegisterItemDrop(type);</code>
		/// </summary>
		public virtual void SetDefaults() {

		}
		public sealed override void SetStaticDefaults()
		{
			Main.wallHouse[Type] = true;
			SetDefaults();
		}

		public override void NumDust(int i, int j, bool fail, ref int num) {
			num = fail ? 1 : 3;
		}
	}
}