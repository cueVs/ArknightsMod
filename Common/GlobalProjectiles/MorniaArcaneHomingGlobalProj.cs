using ArknightsMod.Content.Items.Armor.Caster.Mornia;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Common.GlobalProjectiles
{
	// 末柠(Mornia-Cherry) 套装效果的后半段：「奥术弹幕自动追踪最近的敌人」。
	//
	// 为什么用 GlobalProjectile 而不是在 SetPlayer 里遍历弹幕：追踪需要每帧改弹幕自己的
	// velocity，挂在弹幕上才拿得到"这一发是谁打出来的、是什么伤害类型"，也不用去猜
	// 玩家用的是哪件武器——任何魔法弹幕（含其它模组的）只要属主穿着末柠整套就一律生效。
	public class MorniaArcaneHomingGlobalProj : GlobalProjectile
	{
		// 每帧最多转多少度。3° 是本项目既有追踪弹（如泥岩之息/凛冬之弓的雨箭）的手感，
		// 太大就变成"锁头"，太小则几乎看不出来。
		private const float HomingTurnDeg = 3f;

		// 搜敌半径。太大会出现"弹幕追屏幕外的敌人"，600px 约等于屏幕半宽。
		private const float HomingRangePx = 600f;

		public override void AI(Projectile projectile) {
			if (!ShouldHome(projectile))
				return;

			NPC target = FindNearestEnemy(projectile.Center, HomingRangePx);
			if (target == null)
				return;

			// 只旋转速度方向、不改变速率——这样不会把弹幕的射程/穿透手感一起改掉。
			float currentAngle = projectile.velocity.ToRotation();
			float targetAngle = (target.Center - projectile.Center).ToRotation();
			float diff = MathHelper.WrapAngle(targetAngle - currentAngle);
			float maxTurn = MathHelper.ToRadians(HomingTurnDeg);
			projectile.velocity = projectile.velocity.RotatedBy(MathHelper.Clamp(diff, -maxTurn, maxTurn));
		}

		private static bool ShouldHome(Projectile projectile) {
			// 属主必须是有效玩家且穿着末柠整套。projectile.owner 对非玩家弹幕会是
			// Main.maxPlayers（255），先卡掉再取 Main.player 避免越界。
			if (projectile.owner < 0 || projectile.owner >= Main.maxPlayers)
				return false;

			Player owner = Main.player[projectile.owner];
			if (!owner.active || !owner.GetModPlayer<MorniaSetPlayer>().SetActive)
				return false;

			// 只认自己打出去的魔法伤害弹幕。
			if (!projectile.friendly || projectile.hostile || projectile.damage <= 0)
				return false;
			if (!projectile.DamageType.CountsAsClass(DamageClass.Magic))
				return false;

			// 召唤物/哨兵/持握类（长枪突刺、鞭子等）不参与：它们的 velocity 另有含义，
			// 强行旋转会让动作错乱甚至卡在玩家身上。
			if (projectile.minion || projectile.sentry || projectile.aiStyle == 19 || projectile.aiStyle == 75)
				return false;
			if (projectile.velocity.LengthSquared() < 0.01f)
				return false;

			return true;
		}

		private static NPC FindNearestEnemy(Vector2 from, float maxRange) {
			NPC nearest = null;
			float nearestDistSq = maxRange * maxRange;

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || npc.friendly || npc.dontTakeDamage || npc.lifeMax <= 5 || npc.immortal)
					continue;

				float distSq = Vector2.DistanceSquared(from, npc.Center);
				if (distSq >= nearestDistSq)
					continue;

				if (!Collision.CanHitLine(from, 1, 1, npc.Center, 1, 1))
					continue;

				nearest = npc;
				nearestDistSq = distSq;
			}

			return nearest;
		}
	}
}
