using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.NeoArmorReforge
{
	// 每帧无条件把"当前穿着的 NeoArmorReforge 装备"的 headSlot/bodySlot/legSlot 刷成与其
	// 形态（HelmetForm）相符的槽位。
	//
	// 为什么不能只靠 ModItem.UpdateEquip / UpdateVanity 来同步槽位——反射读原版实现
	// 确认过的事实：
	//   1. player.head/body/legs 只在 Player.Update 里被赋值，直接读
	//      armor[0..2].headSlot / bodySlot / legSlot（ApplyEquipVanity 根本不碰
	//      head/body/legs，它只处理 wings 之类的东西）；
	//   2. ItemLoader.UpdateEquip 的调用点在 Player.UpdateEquips 里被
	//      UpdateEquips_CanItemGrantBenefits 这类前置判断挡着。
	// 也就是说，时装（Item.vanity = true）穿在盔甲栏时，那两个钩子有可能一个都不走，
	// 于是"切换形态"永远同步不到 Item 的槽位字段上，表现为"图标变了、穿戴贴图不变"。
	// 这里绕开所有前置判断，每帧兜底刷一遍，无论时装还是套装、放盔甲栏还是时装栏
	// 都能正确生效。
	internal sealed class NeoArmorReforgeAppearancePlayer : ModPlayer
	{
		// Player.armor 布局：0~2 盔甲（头/身/腿），3~9 饰品，
		// 10~12 社交（时装）栏的头/身/腿，13~19 社交饰品。
		private static readonly int[] ArmorAndVanitySlots = [0, 1, 2, 10, 11, 12];

		public override void UpdateEquips() {
			foreach (int index in ArmorAndVanitySlots)
				SyncSlot(Player.armor[index]);
		}

		private static void SyncSlot(Item item) {
			if (item == null || item.IsAir)
				return;

			switch (item.ModItem) {
				case NeoArmorReforgeVanityItem vanity when vanity.SlotType is EquipType vanitySlot:
					NeoArmorReforgeAppearance.ApplyEquipSlot(item, vanitySlot, vanity.Name, vanity.HelmetForm);
					break;

				case NeoArmorReforgeSetPiece piece:
					NeoArmorReforgeAppearance.ApplyEquipSlot(item, piece.SlotType, piece.Name, piece.HelmetForm);
					break;
			}
		}
	}
}
