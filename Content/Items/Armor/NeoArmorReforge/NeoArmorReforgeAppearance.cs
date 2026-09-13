using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.NeoArmorReforge
{
	// 外观形态切换（HelmetForm，比如泥岩的头盔造型）相关的注册/绘制逻辑。
	// 时装（NeoArmorReforgeVanityItem）和套装（NeoArmorReforgeSetPiece）共用同一份——形态切换只影响
	// 外观，两个 ItemID 各自独立记录 HelmetForm，互不影响，但"怎么注册贴图槽位、
	// 怎么画"是同一套规则。
	internal static class NeoArmorReforgeAppearance
	{
		// 槽位 ID 一律在「注册的那一刻」就记录下来（AddEquipTexture 的返回值），
		// 运行时直接查这两张表，不再靠 EquipLoader.GetEquipSlot 按名字反查——
		// 名字反查一旦因为拼写/注册时机/命名冲突对不上就会返回 -1，而 -1 会让原版
		// 相关判断静默跳过，表现出来就是"切换了但外观没变"，没有任何报错，极难定位。
		// key 用「拥有者的 ModItem.Name」（时装是 MudrockHead，套装是 MudrockHeadSet），
		// 两者天然不冲突。
		private static readonly Dictionary<string, int> DefaultSlotByOwner = new();
		private static readonly Dictionary<string, int> AltSlotByOwner = new();

		// 待隐藏原版部位的 (装备类型, 槽位) 清单，统一攒着，等 PostSetupContent 再一次性写入。
		// 原因见 HideVanillaSkin 上方的说明。
		private static readonly List<(EquipType Type, int Slot)> PendingHideVanillaSkin = new();

		public static void Unload() {
			DefaultSlotByOwner.Clear();
			AltSlotByOwner.Clear();
			PendingHideVanillaSkin.Clear();
		}

		// 库存图标：形态切换开启且声明了替代图标时画替代图标；否则返回 true 交给原版画默认图标。
		public static bool DrawInventoryIcon(SpriteBatch spriteBatch, Vector2 position, Color drawColor, float scale, bool helmetForm, string altIconTexture) {
			if (!helmetForm || altIconTexture == null)
				return true;

			Texture2D tex = ModContent.Request<Texture2D>(altIconTexture).Value;
			spriteBatch.Draw(tex, position, null, drawColor, 0f, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
			return false;
		}

		// [AutoloadEquip] 给时装自动注册"穿戴外观"贴图时，实际读取的文件是
		// "<Texture>_Head"/"_Body"/"_Legs"（跟纯图标 <Texture> 是两张不同的图：图标是
		// 单帧小图，这张才是给角色穿戴动画用的完整帧表）——这个后缀拼接是属性内部
		// 悄悄做的，AddEquipTexture 本身只会老老实实加载传给它的那个路径，不会自动
		// 拼后缀。手动调用 AddEquipTexture 时必须自己拼上，对齐 [AutoloadEquip] 的约定。
		public static string EquipTextureSuffix(EquipType type) => type switch {
			EquipType.Head => "_Head",
			EquipType.Body => "_Body",
			EquipType.Legs => "_Legs",
			_ => "",
		};

		// 记录「默认（未切换）形态」的槽位 ID。
		// 套装件是自己手动 AddEquipTexture 的，注册时就能拿到返回值，直接调这个存下来；
		// 时装是 [AutoloadEquip] 自动注册的，拿不到返回值，只能等 PostSetupContent
		// 阶段按名字查一次再存（那个阶段注册肯定已完成，查得到，见 NeoArmorReforgeEquipSystem）。
		public static void RecordDefaultSlot(string ownerName, int slot) {
			if (slot >= 0)
				DefaultSlotByOwner[ownerName] = slot;
		}

		// 给"形态切换后的穿戴外观"注册一个独立的装备贴图槽位，槽位名固定用 ownerName + "Alt"。
		//
		// ⚠ 这里的 item 参数必须传 null，不能传实际的 ModItem 实例——AddEquipTexture
		// 传入非 null 的 item 时，会把 idToSlot[item.Type][type] 这条反向映射（"这个物品
		// 在这个装备类型下的槽位是谁"）指向本次注册的槽位。物品自己"默认形态"的槽位
		// 早已在它自身注册时绑定过一次（时装靠 [AutoloadEquip]，套装靠 Load() 里那次
		// AddEquipTexture）；这里如果再传同一个 item 注册 "Alt" 槽位，会把那条反向映射
		// 覆盖成 Alt 槽位，导致不管 HelmetForm 是什么，穿戴贴图都解析回同一个，切换
		// 形同虚设——本项目 commit c475501 已经踩过一次这个坑。
		public static void RegisterAltEquip(Mod mod, EquipType type, string ownerName, string altTexture) {
			if (altTexture == null)
				return;

			int slot = EquipLoader.AddEquipTexture(mod, altTexture, type, null, ownerName + "Alt");
			if (slot < 0)
				return;

			AltSlotByOwner[ownerName] = slot;
			// 替代外观也要和默认外观一样隐藏原版对应的身体部位，否则切换过去之后会
			// 出现"头盔和原版头发/皮肤同时画出来"的重影。
			HideVanillaSkin(type, slot);
		}

		// 把原版对应部位的皮肤/头部隐藏掉，默认槽位和 Alt 槽位都要设。
		//
		// ⚠ 这里只登记，不立刻写入——ArmorIDs.*.Sets 的那些数组会在 Load 阶段结束后被
		// 整体重置一次，任何在 Load() 里写进去的值都会被抹掉。tModLoader 的加载顺序是：
		//     ModContent.Load()
		//       ├ LoadModContent(...)  第一遍 → Mod.Load / AddContent / ModType.Load()
		//       ├ ResizeArrays()       → EquipLoader.ResizeAndFillArrays()
		//       │                        → LoaderUtils.ResetStaticMembers(typeof(ArmorIDs))  ★ 清空
		//       ├ LoadModContent(...)  第二遍 → SetStaticDefaults
		//       └ LoadModContent(...)  第三遍 → PostSetupContent
		// 套装件（NeoArmorReforgeSetPiece.Load）和替代外观（RegisterAltEquip）都是在第一遍里注册的，
		// 当场写 ArmorIDs 必定被 ★ 处清空，表现出来就是"穿上套装/切换形态后，原版的
		// 裸体胳膊、皮肤、头发和干员贴图一起画出来"，且没有任何报错。
		// 所以统一攒到 ApplyPendingHideVanillaSkin()，由 NeoArmorReforgeEquipSystem 在
		// PostSetupContent 里调用——那时清空早已发生，写进去的值才留得住。
		public static void HideVanillaSkin(EquipType type, int slot) {
			if (slot >= 0)
				PendingHideVanillaSkin.Add((type, slot));
		}

		// 由 NeoArmorReforgeEquipSystem.PostSetupContent 统一调用，真正落盘。
		public static void ApplyPendingHideVanillaSkin() {
			foreach ((EquipType type, int slot) in PendingHideVanillaSkin)
				WriteHideVanillaSkin(type, slot);

			PendingHideVanillaSkin.Clear();
		}

		private static void WriteHideVanillaSkin(EquipType type, int slot) {
			switch (type) {
				case EquipType.Head:
					if (slot >= 0 && slot < ArmorIDs.Head.Sets.DrawHead.Length)
						ArmorIDs.Head.Sets.DrawHead[slot] = false;
					break;
				case EquipType.Body:
					if (slot >= 0 && slot < ArmorIDs.Body.Sets.HidesArms.Length) {
						ArmorIDs.Body.Sets.HidesTopSkin[slot] = true;
						ArmorIDs.Body.Sets.HidesArms[slot] = true;
					}
					break;
				case EquipType.Legs:
					if (slot >= 0 && slot < ArmorIDs.Legs.Sets.HidesBottomSkin.Length)
						ArmorIDs.Legs.Sets.HidesBottomSkin[slot] = true;
					break;
			}
		}

		// 按当前形态把物品的 headSlot/bodySlot/legSlot 指向"默认槽位"还是"Alt 槽位"。
		//
		// ⚠ 绝对不能因为"这个物品没有替代外观"就提前 return 什么都不做——套装件是手动
		// 注册的，没有 [AutoloadEquip] 帮它在 SetDefaults 阶段设好槽位，完全依赖这个
		// 方法赋值；一旦对"无替代外观"的干员直接返回，那些干员的套装件槽位会一直是 0，
		// 原版就不认为它是防具，装备栏和时装栏都塞不进去。
		public static void ApplyEquipSlot(Item item, EquipType type, string ownerName, bool helmetForm) {
			bool hasAlt = AltSlotByOwner.TryGetValue(ownerName, out int altSlot);
			bool hasDefault = DefaultSlotByOwner.TryGetValue(ownerName, out int defaultSlot);

			int slot = helmetForm && hasAlt ? altSlot : hasDefault ? defaultSlot : -1;
			if (slot < 0)
				return;

			switch (type) {
				case EquipType.Head:
					item.headSlot = slot;
					break;
				case EquipType.Body:
					item.bodySlot = slot;
					break;
				case EquipType.Legs:
					item.legSlot = slot;
					break;
			}
		}
	}
}
