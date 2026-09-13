using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	/// <summary>
	/// 类凝结核，稀有度：蓝
	/// </summary>
	public class CoagulativeNodule : ArknightsMaterial
	{
		public override int Rarity => 2;
		public override void SafeSetStaticDefaults() {
			ItemID.Sets.SortingPriorityMaterials[Item.type] = 58;
		}
	}
}
