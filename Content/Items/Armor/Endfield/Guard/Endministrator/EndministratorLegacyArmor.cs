using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Endfield.Guard.Endministrator
{
	// 旧存档仍引用 Endministrator* 名称，映射到现行 EndminFemale* 物品。
	//
	// ⚠ 这三个类继承自对应的时装类，所以会**连 SetProfile 一起继承**。管理员目前是纯时装
	// （SetProfile 为 null），但只要哪天给 EndminFemale* 补上套装，这三个兼容类就会各自
	// 再注册一份重复的套装件和配方。这里显式钉死为 null 把隐患堵掉——兼容类只负责让旧
	// 存档里的物品还能正常显示，不该有自己的套装。
	public class EndministratorHead : EndminFemaleHead
	{
		public override string Texture => ModContent.GetInstance<EndminFemaleHead>().Texture;

		public override NeoArmorReforgeSetProfile SetProfile => null;

		public override void SetVanityDefaults() {
			Item.headSlot = ModContent.GetInstance<EndminFemaleHead>().Item.headSlot; // 复用现行头部装备槽
		}
	}

	public class EndministratorBody : EndminFemaleBody
	{
		public override string Texture => ModContent.GetInstance<EndminFemaleBody>().Texture;

		public override NeoArmorReforgeSetProfile SetProfile => null;

		public override void SetVanityDefaults() {
			Item.bodySlot = ModContent.GetInstance<EndminFemaleBody>().Item.bodySlot; // 复用现行身体装备槽
		}
	}

	public class EndministratorLegs : EndminFemaleLegs
	{
		public override string Texture => ModContent.GetInstance<EndminFemaleLegs>().Texture;

		public override NeoArmorReforgeSetProfile SetProfile => null;

		public override void SetVanityDefaults() {
			Item.legSlot = ModContent.GetInstance<EndminFemaleLegs>().Item.legSlot; // 复用现行腿部装备槽
		}
	}
}
