using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Estelle
{
	public class EstelleGlovePunch : ModProjectile
	{
		public override string Texture =>
			"ArknightsMod/Content/Items/Weapons/Guard/Estelle/EstelleGlove_protile";

		private const float MaxRange = 60f;     // 冲出最大距离（像素）
		private const int PauseTicks = 12;       // 停顿帧数
		private const float PunchSpeed = 16f;    // 冲出速度（每帧伸展距离）
		private const float RetractSpeed = 14f;  // 收回速度（每帧收缩距离）

		// localAI[0]: 冲出方向角（弧度），OnSpawn 写入，全程不变
		// ai[0]: 阶段 0=冲出 1=停顿 2=收回
		// ai[1]: 当前伸展距离（像素，相对玩家）
		// ai[2]: 停顿倒计时

		private ref float Phase => ref Projectile.ai[0];
		private ref float Extension => ref Projectile.ai[1];
		private ref float PauseTimer => ref Projectile.ai[2];

		public override void SetDefaults() {
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 120;
			Projectile.ignoreWater = true;
		}

		public override void OnSpawn(IEntitySource source) {
			// 记录冲出方向角，后续所有帧通过此角度重建方向向量
			Projectile.localAI[0] = Projectile.velocity.ToRotation();
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			Vector2 dir = Vector2.UnitX.RotatedBy(Projectile.localAI[0]);

			if (Phase == 0) {
				Extension += PunchSpeed;
				if (Extension >= MaxRange) {
					Extension = MaxRange;
					Projectile.friendly = false; // 停顿和收回阶段不伤人
					Phase = 1;
					PauseTimer = PauseTicks;
				}
			} else if (Phase == 1) {
				PauseTimer--;
				if (PauseTimer <= 0)
					Phase = 2;
			} else {
				Extension -= RetractSpeed;
				if (Extension <= 0) {
					Projectile.Kill();
					return;
				}
			}

			// 位置始终锚定玩家，相对玩家偏移冲出方向 × 当前伸展距离
			Projectile.Center = owner.MountedCenter + dir * Extension;
			Projectile.velocity = Vector2.Zero; // 不依赖物理速度移动
		}

		public override bool ShouldUpdatePosition() => false; // 全程手动设置位置

		public override bool PreDraw(ref Color lightColor) {
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 origin = tex.Size() / 2f;
			Vector2 pos = Projectile.Center - Main.screenPosition;
			float rot = Projectile.localAI[0]; // 固定朝冲出方向
			Main.spriteBatch.Draw(tex, pos, null, lightColor, rot, origin, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
