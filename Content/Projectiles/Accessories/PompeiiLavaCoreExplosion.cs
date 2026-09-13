using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Accessories
{
	// 庞贝的熔岩核心 —— 受重伤时以玩家为中心的熔岩爆炸。
	// 视觉全部由 Dust 构成，不绘制任何贴图（PreDraw 返回 false），
	// 所以这个弹幕不需要配套 .png 素材。
	// 伤害值由 PompeiiLavaCorePlayer 在生成时算好传入（已含近战加成），
	// 本类不再做任何伤害缩放。
	public class PompeiiLavaCoreExplosion : ModProjectile
	{
		// 爆炸判定半径（像素）。160 ≈ 10 格。
		private const float Radius = 160f;

		// 命中敌人后附加的"着火了!"持续时间（帧）。
		private const int OnFireDuration = 60 * 5;

		// 存在帧数：只需要够判定一次 + 播完 Dust。
		private const int LifeTime = 24;

		// 贴图不存在，指向一张已有的小图避免加载失败；反正 PreDraw 里不画。
		public override string Texture => "Terraria/Images/Projectile_1";

		public override void SetDefaults() {
			Projectile.width = (int)(Radius * 2f);
			Projectile.height = (int)(Radius * 2f);
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;   // 仅影响暴击/近战专属钩子，不再乘加成
			Projectile.penetrate = -1;
			Projectile.timeLeft = LifeTime;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;         // 每个敌人本次爆炸只吃一次伤害
		}

		public override void OnSpawn(Terraria.DataStructures.IEntitySource source) {
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);   // 爆炸音
			SpawnBurstDust();
		}

		public override void AI() {
			// 爆炸中心保持在生成位置（不跟随玩家移动），并持续照亮
			Projectile.velocity = Vector2.Zero;
			Lighting.AddLight(Projectile.Center, 1.4f, 0.7f, 0.2f);

			// 余焰：存在期间零星再冒一些火星
			if (!Main.dedServ && Projectile.timeLeft > LifeTime / 2) {
				for (int i = 0; i < 4; i++) {
					Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(Radius, Radius);
					Dust d = Dust.NewDustPerfect(pos, DustID.Torch,
						Main.rand.NextVector2Circular(2f, 2f), 100,
						default, Main.rand.NextFloat(1.0f, 1.8f));
					d.noGravity = true;
				}
			}
		}

		// 圆形判定：默认矩形会让角落里的敌人也被打到，这里收成真正的圆
		public override bool? CanHitNPC(NPC target) {
			if (target.Hitbox.Distance(Projectile.Center) > Radius)
				return false;
			return base.CanHitNPC(target);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire, OnFireDuration);
		}

		// 爆炸瞬间的环形火焰喷发
		private void SpawnBurstDust() {
			if (Main.dedServ)
				return;

			// 外扩的火环
			const int rings = 3;
			for (int r = 0; r < rings; r++) {
				int count = 26 + r * 10;
				float speed = 6f + r * 4f;
				for (int i = 0; i < count; i++) {
					float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.05f, 0.05f);
					Vector2 dir = angle.ToRotationVector2();
					Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch,
						dir * speed * Main.rand.NextFloat(0.7f, 1.15f), 100,
						default, Main.rand.NextFloat(1.4f, 2.4f));
					d.noGravity = true;
				}
			}

			// 中心的高温金色核心
			for (int i = 0; i < 24; i++) {
				Dust g = Dust.NewDustPerfect(Projectile.Center, DustID.GoldFlame,
					Main.rand.NextVector2Circular(5f, 5f), 120,
					default, Main.rand.NextFloat(1.2f, 2.0f));
				g.noGravity = true;
			}

			// 少量带重力的熔岩溅射，落下来更有"熔岩"感
			for (int i = 0; i < 14; i++) {
				Dust l = Dust.NewDustPerfect(Projectile.Center, DustID.Lava,
					Main.rand.NextVector2Circular(7f, 7f) - Vector2.UnitY * Main.rand.NextFloat(1f, 4f),
					0, default, Main.rand.NextFloat(1.0f, 1.6f));
				l.noGravity = false;
			}
		}

		public override bool PreDraw(ref Color lightColor) => false;   // 纯 Dust 表现，不画贴图
	}
}
