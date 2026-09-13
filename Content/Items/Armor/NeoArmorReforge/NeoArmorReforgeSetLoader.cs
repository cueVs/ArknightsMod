using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.NeoArmorReforge
{
	// NeoArmor Reforge 的注册中枢：保管"时装 ↔ 套装件"之间的各种映射表。
	//
	// ⚠ 注册入口不在这个类的 Load() 里，而是由每件时装在自己的
	// NeoArmorReforgeVanityItem.Load() 中调用 RegisterSetPieceFor——原因见那边的注释
	// （tModLoader 按类型全名字母序自动加载，在这里统一扫描会静默漏掉字母序排在
	// "NeoArmorReforgeSetLoader" 之后的所有干员）。这个类只负责存表，以及在
	// PostSetupContent 阶段（所有内容注册完、ItemType 都已分配）补齐那些需要用到
	// 时装 ItemType 的表。
	//
	// "同一套"的判定：同一命名空间下的 Head/Body/Legs 三件视为一套。每个干员一个
	// 文件夹/命名空间本来就是本项目既有的约定，所以不需要额外声明归属关系。
	internal sealed class NeoArmorReforgeSetLoader : ModSystem
	{
		private static readonly List<(NeoArmorReforgeVanityItem vanity, NeoArmorReforgeSetPiece piece)> Registered = new();

		private static readonly Dictionary<int, NeoArmorReforgeSetPiece> PieceByVanityType = new();
		private static readonly Dictionary<string, Family> FamilyByNamespace = new();

		// 按套装件自己的 ItemType 反查 Vanity/SlotType，供 NeoArmorReforgeSetPiece 的懒加载
		// 属性使用。这两张表必须在套装件注册的那一刻就填好（不能等 PostSetupContent），
		// 因为 SetDefaults 可能在那之前就被调用（ContentSamples 初始化等）。
		private static readonly Dictionary<int, NeoArmorReforgeVanityItem> VanityByPieceType = new();
		private static readonly Dictionary<int, EquipType> SlotTypeByPieceType = new();

		private sealed class Family
		{
			public int BodyVanityType = -1;
			public int LegsVanityType = -1;
		}

		// 由 NeoArmorReforgeVanityItem.Load() 调用。注意此时调用方（时装）自己的 Type 还没分配
		// （ModType 是先 Load() 再 Register()），所以凡是需要时装 ItemType 的表都只能
		// 延后到 PostSetupContent 再填。
		internal static void RegisterSetPieceFor(NeoArmorReforgeVanityItem vanity) {
			EquipType slotType = vanity switch {
				NeoArmorReforgeVanityHead => EquipType.Head,
				NeoArmorReforgeVanityBody => EquipType.Body,
				NeoArmorReforgeVanityLegs => EquipType.Legs,
				_ => throw new InvalidOperationException(
					$"{vanity.GetType().FullName} 声明了 SetProfile，但不是 NeoArmorReforgeVanityHead/Body/Legs 的子类——" +
					"必须是这三者之一，框架才知道套装件该注册到哪个装备类型下。"),
			};

			var piece = new NeoArmorReforgeSetPiece();
			piece.Init(vanity, slotType);
			vanity.Mod.AddContent(piece);

			// AddContent 返回时 piece.Register() 已经执行过，piece.Type 此刻有效。
			VanityByPieceType[piece.Type] = vanity;
			SlotTypeByPieceType[piece.Type] = slotType;
			Registered.Add((vanity, piece));
		}

		public override void PostSetupContent() {
			foreach ((NeoArmorReforgeVanityItem vanity, NeoArmorReforgeSetPiece piece) in Registered) {
				PieceByVanityType[vanity.Type] = piece;

				string ns = vanity.GetType().Namespace;
				if (!FamilyByNamespace.TryGetValue(ns, out Family family))
					FamilyByNamespace[ns] = family = new Family();

				if (vanity is NeoArmorReforgeVanityBody)
					family.BodyVanityType = vanity.Type;
				else if (vanity is NeoArmorReforgeVanityLegs)
					family.LegsVanityType = vanity.Type;
			}
		}

		public override void Unload() {
			Registered.Clear();
			PieceByVanityType.Clear();
			FamilyByNamespace.Clear();
			VanityByPieceType.Clear();
			SlotTypeByPieceType.Clear();
			NeoArmorReforgeAppearance.Unload();
		}

		internal static NeoArmorReforgeVanityItem GetVanity(int pieceType) =>
			VanityByPieceType.TryGetValue(pieceType, out NeoArmorReforgeVanityItem vanity) ? vanity : null;

		internal static EquipType GetSlotType(int pieceType) =>
			SlotTypeByPieceType.TryGetValue(pieceType, out EquipType type) ? type : default;

		/// <summary>某件头部套装是否已经三件（头/身/腿）都穿在盔甲栏上了。</summary>
		internal static bool IsFullSetEquipped(Player player, NeoArmorReforgeVanityItem headVanity) {
			string ns = headVanity.GetType().Namespace;
			if (!FamilyByNamespace.TryGetValue(ns, out Family family))
				return false;
			if (family.BodyVanityType < 0 || family.LegsVanityType < 0)
				return false;
			if (!PieceByVanityType.TryGetValue(family.BodyVanityType, out NeoArmorReforgeSetPiece bodyPiece))
				return false;
			if (!PieceByVanityType.TryGetValue(family.LegsVanityType, out NeoArmorReforgeSetPiece legsPiece))
				return false;

			return player.armor[1].type == bodyPiece.Type && player.armor[2].type == legsPiece.Type;
		}

		/// <summary>
		/// 取某件时装对应的套装件 ItemType。干员自己的 ModPlayer 里判断"是否穿了套装"
		/// 时用这个，不需要记住自动生成的套装类名，直接传时装类型即可：
		/// <c>player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType&lt;MudrockHead&gt;()</c>
		/// </summary>
		public static int GetSetType<TVanity>() where TVanity : NeoArmorReforgeVanityItem =>
			GetSetType(ModContent.ItemType<TVanity>());

		/// <summary>
		/// 同上，但按时装的 ItemType 查询——给"只拿得到 int 类型、拿不到泛型参数"的地方用
		/// （比如 RedeploySetPlayer 这种由子类填三个 ItemType 的通用基类）。
		/// 查不到返回 -1，而 Item.type 不可能是 -1，所以拿去比较是安全的。
		/// </summary>
		public static int GetSetType(int vanityType) =>
			PieceByVanityType.TryGetValue(vanityType, out NeoArmorReforgeSetPiece piece) ? piece.Type : -1;

		/// <summary>
		/// 某个干员部位（时装或套装，两者都算）当前是否正被"看得见地"穿在身上。
		/// <b>自定义叠加图层的 GetDefaultVisibility 请一律用这个</b>，例如：
		/// <c>return NeoArmorReforgeSetLoader.IsPartVisible&lt;NecrassHead&gt;(player, EquipType.Head) &amp;&amp; !player.dead;</c>
		/// </summary>
		/// <remarks>
		/// ⚠ 不要再写 <c>player.head == EquipLoader.GetEquipSlot(...)</c> 这种"比对装备槽位 ID"的
		/// 判断了。反编译 Terraria.Player.PlayerFrame 可以看到，head/body/legs 先从
		/// armor[0..2] 取值、再被 armor[10..12]（时装栏）覆盖，**之后还会被
		/// Player.SetMatch 重写一遍**（原版的盔甲套装替换机制，会把某些组合的
		/// head/legs 换成别的槽位 ID）。也就是说绘制时刻的 player.head 未必等于我们
		/// 注册时拿到的那个槽位 ID，比对就会静默失败、图层直接不画，且没有任何报错。
		/// 直接看"穿的是哪个物品"才是确定的。
		/// </remarks>
		public static bool IsPartVisible<TVanity>(Player player, EquipType type) where TVanity : NeoArmorReforgeVanityItem =>
			IsPartVisible(player, ModContent.ItemType<TVanity>(), GetSetType<TVanity>(), type);

		private static bool IsPartVisible(Player player, int vanityType, int setType, EquipType type) {
			(int armorIndex, int socialIndex) = type switch {
				EquipType.Head => (0, 10),
				EquipType.Body => (1, 11),
				EquipType.Legs => (2, 12),
				_ => (-1, -1),
			};

			if (armorIndex < 0)
				return false;

			// 时装栏里的东西会盖住盔甲栏的外观（这是原版 PlayerFrame 的规则：
			// armor[10..12] 对应部位的槽位 > 0 时覆盖 armor[0..2]），所以要判断的是
			// "最终显示出来的那一件"。
			Item social = player.armor[socialIndex];
			Item visible = !social.IsAir && SlotOf(social, type) > 0 ? social : player.armor[armorIndex];

			return visible.type == vanityType || (setType > 0 && visible.type == setType);
		}

		private static int SlotOf(Item item, EquipType type) => type switch {
			EquipType.Head => item.headSlot,
			EquipType.Body => item.bodySlot,
			EquipType.Legs => item.legSlot,
			_ => -1,
		};
	}
}
