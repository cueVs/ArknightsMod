using System;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Specialist.Rope
{
	// 暗索（Rope）的套装效果玩家。主题：钩索师的灵动身法——
	// 头盔：被击中时有 15% 概率闪避（类似原版神圣套装）。
	// 套装：5 秒内既未触发头盔闪避也未受到伤害时，蓄能完毕，下次受击必定闪避（一次性）。
	internal class RopeSetPlayer : ArknightsArmorPlayer
	{
		public bool RopeHelmetActive;
		public bool RopeSetActive;

		private const float DodgeChance = 0.15f; // 头盔的闪避概率
		private const int DodgeWindow = 300;     // 套装蓄能判定窗口（5 秒 = 300 tick）
		private const int DodgeImmuneTime = 60;  // 闪避成功后的无敌帧（1 秒，仿原版神圣套闪避）

		/// <summary>最近一次"触发闪避或实际受伤"的游戏 tick。</summary>
		private ulong lastDodgeOrHurtTick;
		/// <summary>套装蓄能完成标记：下一次受击必定闪避（一次性消耗）。</summary>
		private bool guaranteedDodge;

		public override void ResetEffects() {
			RopeHelmetActive = false;
			RopeSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 RopeHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			RopeHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<RopeHead>();
			RopeSetActive = RopeHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<RopeBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<RopeLegs>();

			// 套装不再齐全（拆下任意一件，或整套脱下）后，已蓄好的"必定闪避"
			// 不再有意义，必须清掉——否则 ModifyHurt 里 guaranteedDodge 分支会
			// 在套装缺失时仍生效一次。
			if (!RopeSetActive)
				guaranteedDodge = false;
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (!RopeHelmetActive)
				return;

			// 套装蓄能判定：距上次"闪避/受伤"满 5 秒 → 蓄好一次必定闪避
			if (RopeSetActive && !guaranteedDodge
				&& Main.GameUpdateCount - lastDodgeOrHurtTick >= (ulong)DodgeWindow)
				guaranteedDodge = true;

			bool dodge;
			if (guaranteedDodge) {
				// 必定闪避：一次性消耗蓄能
				dodge = true;
				guaranteedDodge = false;
			}
			else {
				// 头盔的常规概率闪避
				dodge = Main.rand.NextFloat() < DodgeChance;
			}

			if (!dodge)
				return;

			// 闪避成功。免伤 + 命中无敌：
			//  1) Player.immune = true —— 关键！原版只在"伤害结算命中"分支里才置 immune；
			//     Cancel 免伤路径不会自动置位。而外层 Hurt 的闸门是 flag = !immune：
			//     不置 immune 的话，Boss/眼球怪这类同帧多次判定的接触伤害，在本次被
			//     Cancel 后紧接着的再次 Hurt 仍会放行结算（immuneTime 也只在 immune==true
			//     时才递减，单独加 immuneTime 无效）。置位后同帧及后续都被免疫闸门拦下。
			//  2) immuneTime 兜底 —— 配合 immune，提供一段持续无敌帧。
			//  3) modifiers.Cancel() —— 免掉当前这一次 Hurt。
			//  4) ModifyHurtInfo 兜底 —— 最终 HurtInfo 强制取消并归零，杜绝任何残余结算。
			Player.immune = true;
			Player.immuneTime = Math.Max(Player.immuneTime, DodgeImmuneTime);
			lastDodgeOrHurtTick = Main.GameUpdateCount;
			modifiers.Cancel();
			modifiers.ModifyHurtInfo += (ref Player.HurtInfo hurt) => {
				hurt.Cancelled = true;
				hurt.Damage = 0;
			};

			// 金色粉尘提示（还原原版神圣套闪避的金光）
			for (int i = 0; i < 12; i++) {
				Dust d = Dust.NewDustPerfect(Player.Center, DustID.GoldFlame,
					Main.rand.NextVector2Circular(6f, 6f), 0, default, 1.4f);
				d.noGravity = true;
			}
		}

		// 实际受伤（被闪避 Cancel 的伤害不会走到这里）→ 同样作为事件刷新计时
		public override void OnHurt(Player.HurtInfo info) {
			if (RopeHelmetActive)
				lastDodgeOrHurtTick = Main.GameUpdateCount;
		}
	}
}
