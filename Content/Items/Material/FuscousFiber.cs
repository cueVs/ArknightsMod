using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	/// <summary>
	/// 褐素纤维，稀有度：蓝
	/// </summary>
	public class FuscousFiber : ArknightsMaterial
	{
		public override int Rarity => 2;
		public override void SafeSetStaticDefaults() {
			ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;
		}
	}
}
