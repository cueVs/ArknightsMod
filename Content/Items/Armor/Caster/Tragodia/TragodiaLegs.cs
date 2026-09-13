using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Caster.Tragodia
{
	// 纯时装：不声明 SetProfile，不生成套装件，无套装加成/配方（见 NeoArmorReforgeVanityItem 文档第 8 节）。
	[AutoloadEquip(EquipType.Legs)]
	public class TragodiaLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 5;

		public override int Value => 480000;
	}
}
