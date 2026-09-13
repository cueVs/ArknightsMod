using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content
{
	/// <summary>
	/// 本模组所有自定义热键的集中注册处，由主类 <see cref="ArknightsMod"/> 的 Load() 调用
	/// （这是 tModLoader 文档指定的注册位置）。新增热键统一加在 <see cref="Register"/> 里。
	///
	/// 注意：热键的默认键只会写进"新建"的按键配置档。老存档玩家（配置档早就存在）
	/// 装上新版本后，新热键会是未绑定状态，需要自己去「设置-控制」绑一次——
	/// 这不是 bug，改热键名/默认键也解决不了，反而会把已经绑好的玩家重置掉。
	/// </summary>
	public static class ArknightsKeybinds
	{
		public static ModKeybind RosmontisTacticalDeploy { get; private set; }

		/// <summary>
		/// 统一的"技能开启"热键，默认 C。取代了原来分散在各武器里的
		/// 右键/下+右键判定——玩家可以在游戏内「设置-控制」里自由改绑
		/// （在原版按键列表下方的模组按键区，不是「模组配置」页面）。
		/// </summary>
		public static ModKeybind SkillActivate { get; private set; }

		internal static void Register(Mod mod) {
			RosmontisTacticalDeploy = KeybindLoader.RegisterKeybind(mod, "Rosmontis Tactical Deploy", "Z");
			SkillActivate = KeybindLoader.RegisterKeybind(mod, "Skill Activate", "C");
		}

		internal static void Unregister() {
			RosmontisTacticalDeploy = null;
			SkillActivate = null;
		}

		/// <summary>
		/// 技能开启键当前是否按住。武器一律走这个方法，不要直接读 SkillActivate.Current——
		/// ModKeybind 只反映本地客户端的按键状态，不经过网络同步；直接在 CanUseItem/Shoot
		/// 这类对任意 Player 实例都会调用的钩子里读它，在联机时会把本地玩家的按键状态
		/// 错误地套用到其他玩家身上。这里用 whoAmI == Main.myPlayer 卡住，只在本地玩家
		/// 自己的判定里生效，和原来 player.altFunctionUse（每个玩家各自同步的字段）在
		/// 效果上保持一致。
		/// </summary>
		public static bool SkillActivatePressed(Player player) =>
			player.whoAmI == Main.myPlayer && SkillActivate.Current;

		/// <summary>
		/// 技能开启键当前实际绑定的按键名，给 tooltip 显示用（玩家改绑后这里也会跟着变，
		/// 不会出现"提示写着 C，其实玩家已经改绑成别的键"这种过时文案）。
		/// </summary>
		public static string SkillActivateKeyDisplay =>
			string.Join("/", SkillActivate.GetAssignedKeys(Terraria.GameInput.InputMode.Keyboard));
	}
}
