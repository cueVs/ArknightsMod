using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Endfield.Striker.Yvonne
{
	// 纯时装：旧系统里就没有生命加成/防御/配方，ArmorSets.hjson 也没有任何套装文案，
	// 所以不声明 SetProfile（见 NeoArmorReforgeVanityItem 文档第 8 节）。
	[AutoloadEquip(EquipType.Legs)]
	public class YvonneLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override int Value => 560000;
	}
}
