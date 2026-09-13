using System;
using ArknightsMod.Content.Buffs.ArmorSets;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor
{
	/// <summary>
	/// 「快速再部署」类干员套装的通用基类。
	///
	/// <para><b>═══ 一、这个基类实现了什么效果 ═══</b></para>
	/// <list type="number">
	///   <item>穿满整套（Head/Body/Legs 三件 NeoArmor 且均已升级为盔甲）持续
	///         <see cref="ChargeUpSeconds"/> 秒后，获得 Buff【再部署准备】。</item>
	///   <item>处于该 Buff 状态时死亡，复活时间按下式缩短：
	///         <code>
	///         x = 干员再部署时间 / 2
	///         新复活时间 = x × (本次原始复活时间 / 基准复活时间)
	///         </code>
	///         即：保持"相对惩罚比例"不变，只把绝对时长压到干员再部署时间的一半量级。
	///         </item>
	///   <item>脱下套装 → Buff 立即消失，蓄力计时清零（需要重新穿满
	///         <see cref="ChargeUpSeconds"/> 秒）。</item>
	/// </list>
	///
	/// <para><b>═══ 二、怎么用（给团队开发者 / AI 工具的模板）═══</b></para>
	/// 新建一个 <c>XxxSetPlayer</c> 继承本类，只需填四个必填项即可：
	/// <code>
	/// internal class TexasAlterSetPlayer : RedeploySetPlayer
	/// {
	///     // ── 必填 ──────────────────────────────
	///     protected override int HeadItemType => ModContent.ItemType&lt;TexasAlterHead&gt;();
	///     protected override int BodyItemType => ModContent.ItemType&lt;TexasAlterBody&gt;();
	///     protected override int LegsItemType => ModContent.ItemType&lt;TexasAlterLegs&gt;();
	///     /// 干员在明日方舟里的再部署时间（秒）。公式里的 x = 这个值 / 2
	///     protected override float RedeployTimeSeconds => 18f;
	///
	///     // ── 选填（不写就用默认值）──────────────
	///     protected override float ChargeUpSeconds => 10f;   // 穿满多久后获得 Buff
	///     protected override string SetBonusKey =>
	///         "Mods.ArknightsMod.ArmorSets.TexasAlter.SetBonus";
	///
	///     // ── 这个套装自己额外的效果，写在扩展点里 ──
	///     protected override void PostUpdateEquipsExtra() {
	///         if (RedeployReady)
	///             Player.moveSpeed += 0.15f;
	///     }
	/// }
	/// </code>
	///
	/// <para><b>⚠ 三、扩展时必须注意（最容易踩的坑）</b></para>
	/// 本类把 <c>ResetEffects</c> / <c>PostUpdateEquips</c> / <c>UpdateDead</c> /
	/// <c>OnRespawn</c> / <c>Kill</c> 这五个钩子都<b>标记为 sealed</b>，
	/// 子类无法直接重写。这是<b>刻意的设计</b>：如果放开让子类重写，一旦子类忘记调用
	/// <c>base.XXX()</c>，再部署逻辑就会静默失效，而且这种 bug 很难被发现。
	/// <br/>
	/// 需要在这些时机加自己的逻辑时，请重写对应的 <c>XxxExtra()</c> 扩展点
	/// （<see cref="ResetEffectsExtra"/> / <see cref="PostUpdateEquipsExtra"/> /
	/// <see cref="UpdateDeadExtra"/> / <see cref="OnRespawnExtra"/> /
	/// <see cref="OnKillExtra"/>），基类保证它们一定会被调用。
	///
	/// <para><b>═══ 四、实现上的两个关键点（改动本类前请先读）═══</b></para>
	/// <list type="bullet">
	///   <item><b>复活时间只在死亡后的第一帧修改一次。</b>
	///         <c>UpdateDead()</c> 是<b>每帧</b>都会被调用的。如果在里面无条件地写
	///         <c>respawnTimer -= N</c>，等于每帧都减一次，几帧内就会归零变成瞬间复活。
	///         本类用 <c>_respawnAdjusted</c> 一次性开关规避。
	///         （注：<c>Content/Items/Armor/Guard/Skadi/SkadiSetPlayer.cs</c> 里的
	///         <c>UpdateDead</c> 正是这个写法，实际效果是秒复活而不是"少等 5 秒"，
	///         属于既有 bug，本次未改动它，发现时可一并修。）</item>
	///   <item><b><c>Player.respawnTimerMax</c> 是 const(3600)，不要试图给它赋值。</b>
	///         它是"复活计时的最大可能值"这一常量，正好等于 60 秒——也就是策划案里
	///         说的"默认复活时间 60s"。本类用它当公式的分母基准
	///         （见 <see cref="DefaultRespawnTicks"/>），而不是手写 3600。
	///         真正需要修改的只有实例字段 <c>Player.respawnTimer</c> 一个。</item>
	/// </list>
	/// </summary>
	public abstract class RedeploySetPlayer : ArknightsArmorPlayer
	{
		// ══════════════════ 子类必须提供 ══════════════════

		/// <summary>套装头部件（NeoArmor）的 ItemType。</summary>
		protected abstract int HeadItemType { get; }

		/// <summary>套装身体件（NeoArmor）的 ItemType。</summary>
		protected abstract int BodyItemType { get; }

		/// <summary>套装腿部件（NeoArmor）的 ItemType。</summary>
		protected abstract int LegsItemType { get; }

		/// <summary>
		/// 该干员的「再部署时间」（秒），取自明日方舟原作数值。
		/// <br/>公式中的 <c>x = RedeployTimeSeconds / 2</c>。
		/// </summary>
		protected abstract float RedeployTimeSeconds { get; }

		// ══════════════════ 子类可选覆盖 ══════════════════

		/// <summary>穿满整套多少秒后获得【再部署准备】。默认 10 秒。</summary>
		protected virtual float ChargeUpSeconds => 10f;

		/// <summary>
		/// 公式里的「默认复活时间」基准，单位 tick。
		/// <para>
		/// 默认直接取原版常量 <c>Player.respawnTimerMax</c>（值 3600 tick = <b>正好 60 秒</b>），
		/// 与策划案里写的"默认复活时间 60s"完全对应，所以不要手写 3600 魔法数字。
		/// </para>
		/// <para>
		/// 补充一个容易混淆的点：原版<b>实际</b>死亡后的复活时长并不总是 60 秒——
		/// 常规死亡约 600 tick（10 秒），有 Boss 存活 / 多人模式下会更长，
		/// 3600 只是"上限"。所以公式里的比值
		/// <c>(本次原始复活时间 / 60秒)</c> 平时通常小于 1，
		/// 这正是"惩罚越重（复活越久）→ 减免后也越久"的设计意图。
		/// </para>
		/// </summary>
		protected virtual float DefaultRespawnTicks => Terraria.Player.respawnTimerMax;

		/// <summary>
		/// 套装说明文本的本地化 key（显示在装备栏的套装效果那一行）。
		/// 返回 null 则不显示。
		/// </summary>
		protected virtual string SetBonusKey => null;

		/// <summary>
		/// 【再部署准备】用哪个 Buff 显示。默认用通用的
		/// <see cref="RedeployReadyBuff"/>，需要专属图标时重写此项。
		/// </summary>
		protected virtual int RedeployBuffType => ModContent.BuffType<RedeployReadyBuff>();

		// ══════════════════ 供子类读取的状态 ══════════════════

		/// <summary>当前是否穿着完整套装（三件均已升级为盔甲并穿在盔甲栏）。</summary>
		public bool SetActive { get; private set; }

		/// <summary>【再部署准备】是否已就绪（穿满套装且蓄力完成）。</summary>
		public bool RedeployReady { get; private set; }

		/// <summary>蓄力进度 0~1，可用于 UI 显示。</summary>
		public float ChargeProgress =>
			ChargeUpTicks <= 0 ? 1f : MathHelper.Clamp(_wearTicks / (float)ChargeUpTicks, 0f, 1f);

		// ══════════════════ 内部状态 ══════════════════

		private int _wearTicks;             // 已连续穿着的帧数
		private bool _readyAtDeath;         // 死亡瞬间是否处于就绪状态（Buff 死后会被清，必须提前锁存）
		private bool _respawnAdjusted;      // 本次死亡是否已经改过复活时间（保证只改一次）

		private int ChargeUpTicks => (int)Math.Round(ChargeUpSeconds * 60f);

		// ══════════════════ 生命周期（sealed，子类用 Extra 扩展点）══════════════════

		public sealed override void ResetEffects() {
			base.ResetEffects();
			SetActive = false;
			// 注意：RedeployReady 不在这里清零，它的清除交给 PostUpdateEquips。
			//
			// 关于死亡期间这几个钩子谁会跑（已通过读 Player.Update 的 IL 确认）：
			//   Player.Update 里 UpdateDead 的调用位置远早于 ResetEffects，
			//   死亡分支执行完 UpdateDead 就提前返回了 —— 也就是说玩家死亡期间
			//   ResetEffects 和 PostUpdateEquips 都**不会**执行，
			//   所有 ModPlayer 字段都会保持死亡前的最后一帧的值。
			// 本类没有依赖这个细节（就绪状态在 Kill 里显式锁存到 _readyAtDeath），
			// 但如果你要在子类里读死亡期间的状态，需要知道这个前提。
			ResetEffectsExtra();
		}

		public sealed override void PostUpdateEquips() {
			base.PostUpdateEquips();

			// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
			// 不再需要旧系统的 hasUpgraded 判断。子类给的三个 ItemType 是**时装**的，
			// 这里用 GetSetType 换算成对应套装件再比对。
			SetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType(HeadItemType)
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType(BodyItemType)
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType(LegsItemType);

			if (SetActive) {
				if (_wearTicks < ChargeUpTicks)
					_wearTicks++;

				bool wasReady = RedeployReady;
				RedeployReady = _wearTicks >= ChargeUpTicks;

				if (RedeployReady) {
					// 每帧续期，保证只要穿着套装 Buff 就一直在
					Player.AddBuff(RedeployBuffType, 2);
					if (!wasReady)
						OnRedeployReadyGained();
				}

				// ⚠ 一般不该走这里：套装文案应该由干员 Head 的 SetProfile.SetBonusKey 统一设置，
				// 两处都设会互相覆盖。这个分支只服务于"没有 SetProfile 却想显示套装文案"的
				// 特殊子类，默认 SetBonusKey 为 null 时不生效。
				if (SetBonusKey != null)
					Player.setBonus = Language.GetTextValue(SetBonusKey);
			}
			else {
				// 脱下套装：蓄力清零 + Buff 立即消失
				if (RedeployReady) {
					RedeployReady = false;
					OnRedeployReadyLost();
				}
				_wearTicks = 0;
				if (Player.HasBuff(RedeployBuffType))
					Player.ClearBuff(RedeployBuffType);
			}

			PostUpdateEquipsExtra();
		}

		public sealed override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			// Buff 在死亡时会被清空，所以必须在这一刻把"是否就绪"锁存下来，
			// 之后 UpdateDead 里读的是这个锁存值，而不是去查 Buff/装备。
			_readyAtDeath = RedeployReady;
			_respawnAdjusted = false;
			OnKillExtra(damage, hitDirection, pvp, damageSource);
		}

		public sealed override void UpdateDead() {
			base.UpdateDead();

			// 只在死后的第一帧修改一次复活时间。
			// UpdateDead 每帧都会被调用，若不加这个开关，会被反复缩短直到秒复活。
			if (_readyAtDeath && !_respawnAdjusted) {
				_respawnAdjusted = true;
				ApplyRedeployRespawn();
			}

			UpdateDeadExtra();
		}

		public sealed override void OnRespawn() {
			base.OnRespawn();
			_readyAtDeath = false;
			_respawnAdjusted = false;
			// 复活后装备判定会在下一次 PostUpdateEquips 重新跑；
			// 蓄力不清零，让"死了之后不用重新等蓄力"，若想改成清零在这里加 _wearTicks = 0。
			OnRespawnExtra();
		}

		// ══════════════════ 核心：复活时间换算 ══════════════════

		/// <summary>
		/// 按公式把本次复活时间压缩：
		/// <code>
		/// x            = RedeployTimeSeconds / 2
		/// 新复活时间   = x × (本次原始复活时间 / DefaultRespawnSeconds)
		/// </code>
		/// 调用时机：死亡后的第一帧（此时 <c>Player.respawnTimer</c> 已由原版赋好初值）。
		/// </summary>
		private void ApplyRedeployRespawn() {
			int originalTicks = Player.respawnTimer;
			if (originalTicks <= 0)
				return;

			float defaultTicks = MathF.Max(DefaultRespawnTicks, 1f);
			float ratio = originalTicks / defaultTicks;      // 当前复活时间 / 默认复活时间(60s)
			float xSeconds = RedeployTimeSeconds * 0.5f;     // x = 再部署时间 / 2

			int newTicks = (int)MathF.Round(xSeconds * 60f * ratio);
			newTicks = Math.Clamp(newTicks, 1, originalTicks); // 只缩短，绝不延长

			Player.respawnTimer = newTicks;

			// 说明：不需要（也不能）去改 Player.respawnTimerMax——
			// 它是原版的 const（值 3600），代表"复活计时的最大可能值"，不是可写字段。
			// 死亡界面的倒计时是直接按 respawnTimer 显示的，改它一个就够。
		}

		// ══════════════════ 扩展点（子类重写这些，不要动上面 sealed 的）══════════════════

		/// <summary>每帧重置自定义效果字段。对应 <c>ResetEffects</c>。</summary>
		protected virtual void ResetEffectsExtra() { }

		/// <summary>
		/// 每帧装备结算。对应 <c>PostUpdateEquips</c>。
		/// 调用时 <see cref="SetActive"/> / <see cref="RedeployReady"/> 已经算好，可直接读。
		/// </summary>
		protected virtual void PostUpdateEquipsExtra() { }

		/// <summary>死亡期间每帧调用。对应 <c>UpdateDead</c>。</summary>
		protected virtual void UpdateDeadExtra() { }

		/// <summary>复活瞬间调用。对应 <c>OnRespawn</c>。</summary>
		protected virtual void OnRespawnExtra() { }

		/// <summary>死亡瞬间调用。对应 <c>Kill</c>。</summary>
		protected virtual void OnKillExtra(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) { }

		/// <summary>【再部署准备】刚刚就绪时调用一次（可用来放音效/粒子提示）。</summary>
		protected virtual void OnRedeployReadyGained() { }

		/// <summary>【再部署准备】刚刚失效时调用一次。</summary>
		protected virtual void OnRedeployReadyLost() { }
	}
}
