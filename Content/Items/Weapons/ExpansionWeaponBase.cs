using System;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons
{
	/// <summary>
	/// 由 ArknightsWeaponryExpansion 迁移而来的武器基类。
	/// <br/>原扩展包武器基于 <c>GItem</c>(GlobalItem) 的精英化(Jiedan)/技力字段运作；
	/// 此基类把这些逻辑桥接到主模组的 <see cref="UpgradeWeaponBase"/> + <see cref="WeaponPlayer"/>
	/// 技力条系统上，使迁移后的武器复用主模组的技能条、技能选择 UI 与精英化框架。
	/// </summary>
	public abstract class ExpansionWeaponBase : UpgradeWeaponBase
	{
		/// <summary>
		/// 各精英化阶段对应的基础攻击力，下标 = 精英化阶段(0 / 1 / 2)。
		/// <br/>子类必须至少提供 1 个元素。这是一个<b>扩展点</b>：日后若要做完整的
		/// 「按等级缩放」精英化系统，可把此处替换为按等级查表，而无需改动各武器。
		/// </summary>
		protected abstract int[] EliteDamage { get; }

		/// <summary>允许的最大精英化阶段，由 <see cref="EliteDamage"/> 数组长度推导。</summary>
		public override int EliteStageMax => Math.Max(1, EliteDamage.Length - 1);

		/// <summary>升级/预览界面用的伤害；扩展武器目前按精英化阶段取平值（暂不随等级缩放）。</summary>
		protected override int GetDamage(int level) => EliteDamage[Math.Clamp(EliteStage, 0, EliteDamage.Length - 1)];

		/// <summary>
		/// 按当前精英化阶段把基础攻击力抬升到 <see cref="EliteDamage"/> 对应值。
		/// <br/>约定：子类在 <c>SetDefaults</c> 中把 <c>Item.damage</c> 设为 <c>EliteDamage[0]</c>。
		/// </summary>
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
		{
			int stage = Math.Clamp(EliteStage, 0, EliteDamage.Length - 1);
			if (stage > 0)
				damage.Base += EliteDamage[stage] - EliteDamage[0];
		}

		/// <summary>手持时同步技能图标名，供技力条 UI 取用。</summary>
		public override void HoldItem(Player player)
		{
			var mp = player.GetModPlayer<WeaponPlayer>();
			if (mp.IconName != Name)
				mp.IconName = Name;
		}
	}
}
