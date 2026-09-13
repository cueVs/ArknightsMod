using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.NeoArmorReforge
{
	// 部位标记类：让框架能用 `is NeoArmorReforgeVanityHead` 之类的写法识别"这是哪个部位"，
	// 同时把 EquipType 告诉基类（用于注册/切换替代外观贴图、决定套装件注册到哪个
	// 装备类型下）。本身不含任何逻辑——新增干员时继承对应部位的这三个类之一。
	public abstract class NeoArmorReforgeVanityHead : NeoArmorReforgeVanityItem
	{
		internal sealed override EquipType? SlotType => EquipType.Head;
	}

	public abstract class NeoArmorReforgeVanityBody : NeoArmorReforgeVanityItem
	{
		internal sealed override EquipType? SlotType => EquipType.Body;
	}

	public abstract class NeoArmorReforgeVanityLegs : NeoArmorReforgeVanityItem
	{
		internal sealed override EquipType? SlotType => EquipType.Legs;
	}
}
