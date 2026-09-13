using System;
using ArknightsMod.Common.Configs;
using Terraria;
using Terraria.ModLoader;

// 全局武器数值覆写 GlobalItem
// 当 WeaponStatOverrideConfig.UseGlobalWeaponStats 为 true 时，
// 在每个武器的 SetDefaults 之后，用 WeaponStatOverride 表中的值覆盖对应属性。
// 未在表中定义的武器、或表中未赋值的属性，均不受影响。

namespace ArknightsMod.Common
{
	public class WeaponStatOverrideGlobalItem : GlobalItem
	{
		// 返回 false 表示所有物品共享同一个 GlobalItem 实例，仅在 SetDefaults 时执行一次，性能无影响
		public override bool InstancePerEntity => false;

		public override void SetDefaults(Item item) {
			if (!ModContent.GetInstance<WeaponStatOverrideConfig>().UseGlobalWeaponStats)
				return;

			Type key = item.ModItem?.GetType();
			if (key == null)
				return;

			if (!WeaponStatOverride.Overrides.TryGetValue(key, out var stats))
				return;

			if (stats.Damage.HasValue)
				item.damage = stats.Damage.Value;

			if (stats.UseTime.HasValue)
				item.useTime = stats.UseTime.Value;

			if (stats.UseAnimation.HasValue)
				item.useAnimation = stats.UseAnimation.Value;

			if (stats.ShootSpeed.HasValue)
				item.shootSpeed = stats.ShootSpeed.Value;

			if (stats.KnockBack.HasValue)
				item.knockBack = stats.KnockBack.Value;

			if (stats.Crit.HasValue)
				item.crit = stats.Crit.Value;

			if (stats.Mana.HasValue)
				item.mana = stats.Mana.Value;
		}
	}
}