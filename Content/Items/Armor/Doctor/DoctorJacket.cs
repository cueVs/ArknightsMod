using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Doctor
{
	// 纯时装：旧系统里就没有生命加成/防御/配方，ArmorSets.hjson 也没有任何套装文案，
	// 所以不声明 SetProfile（见 NeoArmorReforgeVanityItem 文档第 8 节）。
	[AutoloadEquip(EquipType.Body)]
	public class DoctorJacket : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 1;
	}
}
