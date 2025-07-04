using ArknightsMod.Common.Damageclasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace ArknightsMod.Content.Projectiles
{
	public class SpellDamageModification : GlobalProjectile
	{
		public override void OnSpawn(Projectile projectile, IEntitySource source) {
			// 检测是否在法术弹幕列表中
			if (SpellDamageConfig.SpellProjectiles.Contains(projectile.type)) {
				// 标记为法术伤害（不影响原伤害类型）
				projectile.GetGlobalProjectile<SpellDamageMarker>().IsSpellDamage = true;
			}
		}
	}
	public class SpellDamageMarker : GlobalProjectile
	{
		public override bool InstancePerEntity => true; // 关键修复

		public bool IsSpellDamage { get; set; } // 每个弹幕独立存储
	}

}
