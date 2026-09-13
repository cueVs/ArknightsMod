using System;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.NeoArmorReforge
{
	// NeoArmor Reforge：由时装类（NeoArmorReforgeVanityItem）的 SetProfile 属性提供，声明
	// "这件时装升级成套装之后长什么样、需要什么材料、装备时有什么效果"。
	// NeoArmorReforgeSetLoader 靠这份数据自动生成对应的套装物品（NeoArmorReforgeSetPiece），
	// 作者不需要再手写一个 Set 类。
	//
	// 只是一个纯数据容器：不要往这里加行为/逻辑，那些应该放到 OnHelmetActive /
	// OnFullSetActive 的回调里，或者放到干员自己的 ModPlayer 里。
	public sealed class NeoArmorReforgeSetProfile
	{
		/// <summary>套装件提供的防御力。时装永远是 0（框架强制），只有套装才有防御。</summary>
		public int Defense;

		/// <summary>套装件提供的最大生命值加成。</summary>
		public int LifeBonus;

		/// <summary>
		/// 套装/头盔效果文案的统一前缀，例如 "Mods.ArknightsMod.ArmorSets.Mornia"。
		/// 实际读取 {前缀}.HeadName / {前缀}.HelmetEffect / {前缀}.SetEffect / {前缀}.SetBonus。
		/// 这些 key 本来就已经写在 ArmorSets.hjson 里，旧 NeoArmor 系统从没真正用起来过。
		/// </summary>
		public string LocalizationPrefix;

		/// <summary>
		/// 除了"这件时装本身 ×1"之外，升级还需要哪些额外材料。直接对传入的 Recipe 操作，
		/// 不需要自己 Register()——框架会补上制造加工站和注册。
		/// </summary>
		public Action<Recipe> Materials;

		/// <summary>
		/// 头部套装单独装备在头盔栏时每帧调用。不需要自己判断"是不是头盔栏"，
		/// 框架只会在头部件、且确实穿在盔甲栏时才调用。
		/// </summary>
		public Action<Player> OnHelmetActive;

		/// <summary>
		/// 头/身/腿三件套装全部穿在盔甲栏时每帧调用（在 OnHelmetActive 之后）。
		/// "同一套"的判定依据是三件时装处于同一个命名空间，见 NeoArmorReforgeSetLoader。
		/// </summary>
		public Action<Player> OnFullSetActive;

		/// <summary>
		/// 三件套激活时 player.setBonus 显示的文本 key，对应 ArmorSets.hjson 里的 SetBonus。
		/// 为 null 时不设置（某些干员的套装效果不适合用一行简述概括）。
		/// 只需要在"头部件"的 Profile 上填，身体/腿部填了也不会被用到。
		/// </summary>
		public string SetBonusKey;
	}
}
