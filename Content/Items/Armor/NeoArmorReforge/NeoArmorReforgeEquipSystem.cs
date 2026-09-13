using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.NeoArmorReforge
{
	// 所有内容注册完之后，补齐 NeoArmorReforge 时装的两件事：
	//   1. 隐藏原版对应的身体部位（不然干员贴图会和原版头发/皮肤重影）；
	//   2. 记录 [AutoloadEquip] 自动注册的那个"默认形态"槽位 ID。
	//
	// 第 2 点必须放在这个阶段：Alt 槽位是我们自己 AddEquipTexture 注册的、当场就能拿到
	// 返回值，而默认槽位是 [AutoloadEquip] 属性替我们注册的、拿不到返回值，只能等注册
	// 全部完成后按名字查一次再存下来。存下来之后，运行时切换形态就只是在两个 int
	// 之间来回切，不再做任何按名字的反查（见 NeoArmorReforgeAppearance 里的说明）。
	//
	// 套装件（NeoArmorReforgeSetPiece）不在这里处理——它是手动注册的，这两件事都已经在它自己的
	// Load() 里做完了。
	internal sealed class NeoArmorReforgeEquipSystem : ModSystem
	{
		public override void PostSetupContent() {
			foreach (ModItem item in ModContent.GetContent<ModItem>()) {
				if (item is not NeoArmorReforgeVanityItem vanity || vanity.SlotType is not EquipType type)
					continue;

				int slot = EquipLoader.GetEquipSlot(item.Mod, item.Name, type);
				NeoArmorReforgeAppearance.HideVanillaSkin(type, slot);
				NeoArmorReforgeAppearance.RecordDefaultSlot(item.Name, slot);
			}

			// 套装件和替代外观在 Load() 阶段登记的那些槽位，连同上面这轮时装的，
			// 到这里才统一写进 ArmorIDs——必须等 EquipLoader.ResizeAndFillArrays()
			// 里的 ResetStaticMembers(typeof(ArmorIDs)) 清空之后，写进去的值才留得住。
			NeoArmorReforgeAppearance.ApplyPendingHideVanillaSkin();
		}
	}
}
