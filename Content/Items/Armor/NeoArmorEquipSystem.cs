using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor
{
	// 旧 NeoArmor 系统的静态绘制规则配置：隐藏原版对应的身体部位。
	// 放在 PostSetupContent 而不是 SetStaticDefaults，是为了避免装备槽位还没注册完
	// 时访问 ArmorIDs 数组越界。
	//
	// 新系统（NeoArmor Reforge）有自己的 NeoArmorReforgeEquipSystem，两者互不干扰；
	// 等所有干员都迁移完、旧 NeoArmor 整体删除时，这个文件也一起删掉。
	internal sealed class NeoArmorEquipSystem : ModSystem
	{
		public override void PostSetupContent() {
			foreach (ModItem item in ModContent.GetContent<ModItem>()) {
				switch (item) {
					case NeoArmorHead:
						ConfigureHead(item);
						break;
					case NeoArmorBody:
						ConfigureBody(item);
						break;
					case NeoArmorLegs:
						ConfigureLegs(item);
						break;
				}
			}
		}

		private static void ConfigureHead(ModItem item) {
			int slot = EquipLoader.GetEquipSlot(item.Mod, item.Name, EquipType.Head);
			if (slot < 0 || slot >= ArmorIDs.Head.Sets.DrawHead.Length)
				return;

			ArmorIDs.Head.Sets.DrawHead[slot] = false;
		}

		private static void ConfigureBody(ModItem item) {
			int slot = EquipLoader.GetEquipSlot(item.Mod, item.Name, EquipType.Body);
			if (slot < 0 || slot >= ArmorIDs.Body.Sets.HidesArms.Length)
				return;

			ArmorIDs.Body.Sets.HidesTopSkin[slot] = true;
			ArmorIDs.Body.Sets.HidesArms[slot] = true;
		}

		private static void ConfigureLegs(ModItem item) {
			int slot = EquipLoader.GetEquipSlot(item.Mod, item.Name, EquipType.Legs);
			if (slot < 0 || slot >= ArmorIDs.Legs.Sets.HidesBottomSkin.Length)
				return;

			ArmorIDs.Legs.Sets.HidesBottomSkin[slot] = true;
		}
	}
}
