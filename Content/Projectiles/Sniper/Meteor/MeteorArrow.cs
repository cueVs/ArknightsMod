using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace ArknightsMod.Content.Projectiles.Sniper.Meteor
{
	// 流星弓 S1 穿墙箭：穿透5个目标，超出所有玩家视野后自毁
	public class MeteorArrow : ModProjectile
	{
		// 复用原版木箭贴图，不需要额外 PNG 文件
		public override string Texture => "Terraria/Images/Projectile_3";

		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.arrow = true;
			Projectile.tileCollide = false;      // 穿过实心块
			Projectile.penetrate = 5;            // 最多命中 5 个目标
			Projectile.timeLeft = 1800;          // 30 秒上限兜底
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
		}

		public override void AI() {
			// 箭矢旋转跟随速度方向
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			// 微弱重力（与原版箭一致）
			Projectile.ai[0]++;
			if (Projectile.ai[0] >= 15f) {
				Projectile.ai[0] = 15f;
				Projectile.velocity.Y += 0.1f;
				if (Projectile.velocity.Y > 16f) Projectile.velocity.Y = 16f;
			}

			// 超出所有活跃玩家视野范围后自毁
			if (IsOutOfAllPlayersView())
				Projectile.Kill();
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			// 命中目标施加碎甲（-35% 防御 5秒）
			target.AddBuff(BuffID.BrokenArmor, 5 * 60);
		}

		private bool IsOutOfAllPlayersView() {
			float halfW = Main.screenWidth * 0.6f;
			float halfH = Main.screenHeight * 0.6f;
			foreach (Player p in Main.player) {
				if (!p.active || p.dead) continue;
				Vector2 center = p.Center;
				if (Projectile.Center.X >= center.X - halfW &&
				    Projectile.Center.X <= center.X + halfW &&
				    Projectile.Center.Y >= center.Y - halfH &&
				    Projectile.Center.Y <= center.Y + halfH)
					return false; // 至少在一名玩家视野内
			}
			return true; // 所有玩家都看不到了
		}
	}
}
