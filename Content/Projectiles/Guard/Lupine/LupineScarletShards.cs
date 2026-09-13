using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Lupine
{
	// 右键：在玩家身后散射出数枚绯红碎影，归航至光标点后引爆
	public class LupineScarletRightClickController : ModProjectile
	{
		private const float ScatterSpeed = 10f;
		public override string Texture => "Terraria/Images/MagicPixel";

		public override void SetDefaults() {
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.timeLeft = 60;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.hide = true;
		}

		public override bool ShouldUpdatePosition() => false;

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (!player.active || player.dead) { Projectile.Kill(); return; }

			Vector2 target = new Vector2(Projectile.ai[0], Projectile.ai[1]);
			Projectile.Center = player.MountedCenter;

			if (Projectile.localAI[0] == 0f) {
				Projectile.localAI[0] = 1f;
				if (Main.myPlayer != Projectile.owner) return;
				Vector2 back = player.MountedCenter - new Vector2(player.direction * 42f, 18f);
				for (int i = 0; i < 4; i++) {
					float ang = MathHelper.TwoPi * (i / 4f);
					Vector2 scatterVel = ang.ToRotationVector2() * ScatterSpeed;
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), back, scatterVel,
						ModContent.ProjectileType<LupineScarletSkillShard>(),
						player.GetWeaponDamage(player.HeldItem), 2f, Projectile.owner,
						target.X, target.Y);
				}
			}
		}

		public override bool PreDraw(ref Color lightColor) => false;
	}

	public class LupineScarletSkillShard : ModProjectile
	{
		private const int ScatterTime = 18;
		private const float HomeSpeed = 18f;
		private const string ShardTexPath = "ArknightsMod/Content/Projectiles/Guard/Lupine/LupineShard";

		private const int SpiralArms = 2;
		private const float SpiralRadius = 10f;
		private const float SpiralSpeed = 0.38f;
		private const float SpiralStretch = 14f;

		public override string Texture => ShardTexPath;

		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 240;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Melee;
		}

		public override void AI() {
			Vector2 target = new Vector2(Projectile.ai[0], Projectile.ai[1]);

			float targetRot = Projectile.velocity.ToRotation();
			float rotDiff = MathHelper.WrapAngle(targetRot - Projectile.rotation);
			Projectile.rotation += MathHelper.Clamp(rotDiff, -0.18f, 0.18f);

			Projectile.localAI[0]++;
			Projectile.localAI[1] += SpiralSpeed;

			bool isHoming = Projectile.localAI[0] > ScatterTime && Projectile.localAI[0] < ScatterTime + 90;
			if (isHoming) {
				Vector2 toTarget = target - Projectile.Center;
				if (toTarget.LengthSquared() < 0.001f) toTarget = Vector2.UnitY;
				toTarget.Normalize();

				float currentSpeed = MathHelper.Lerp(Projectile.velocity.Length(), HomeSpeed, 0.06f);
				float currentAngle = Projectile.velocity.ToRotation();
				float desiredAngle = toTarget.ToRotation();
				float angleDiff = MathHelper.WrapAngle(desiredAngle - currentAngle);
				float newAngle = currentAngle + MathHelper.Clamp(angleDiff, -0.10f, 0.10f);
				Projectile.velocity = newAngle.ToRotationVector2() * currentSpeed;
			} else if (Projectile.localAI[0] >= ScatterTime + 90) {
				float currentSpeed = MathHelper.Lerp(Projectile.velocity.Length(), HomeSpeed, 0.06f);
				Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * currentSpeed;
			} else {
				Projectile.velocity *= 0.96f;
			}

			Lighting.AddLight(Projectile.Center, 0.85f, 0.10f, 0.20f);
			SpawnSpiralTrail();

			if (isHoming && Vector2.DistanceSquared(Projectile.Center, target) < 26f * 26f)
				Projectile.Kill();
		}

		private void SpawnSpiralTrail() {
			float phase = Projectile.localAI[1];
			Vector2 velDir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
			Vector2 perpDir = velDir.RotatedBy(MathHelper.PiOver2);

			for (int arm = 0; arm < SpiralArms; arm++) {
				float armPhase = phase + arm * (MathHelper.TwoPi / SpiralArms);
				Vector2 spiralOffset = perpDir * (float)Math.Sin(armPhase) * SpiralRadius
									 - velDir * (float)Math.Cos(armPhase) * (SpiralRadius * 0.5f);
				Vector2 spawnPos = Projectile.Center - velDir * SpiralStretch + spiralOffset;

				Dust core = Dust.NewDustPerfect(spawnPos, DustID.RedTorch,
					velDir * Main.rand.NextFloat(-1.8f, 0f) + spiralOffset * 0.12f,
					0, new Color(255, 60, 60), Main.rand.NextFloat(1.4f, 2.0f));
				core.noGravity = true;
				core.fadeIn = 0.8f;

				Dust halo = Dust.NewDustPerfect(spawnPos + Main.rand.NextVector2Circular(5f, 5f),
					DustID.RedTorch, Main.rand.NextVector2Circular(1.5f, 1.5f),
					80, new Color(200, 30, 60), Main.rand.NextFloat(1.0f, 1.6f));
				halo.noGravity = true;

				if (Main.rand.NextBool(2)) {
					Dust white = Dust.NewDustPerfect(spawnPos, DustID.WhiteTorch, Vector2.Zero, 180,
						new Color(255, 220, 220), Main.rand.NextFloat(0.5f, 0.9f));
					white.noGravity = true;
				}
			}

			if (Main.rand.NextBool(2)) {
				Dust glow = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f),
					DustID.RedTorch, Main.rand.NextVector2Circular(1.0f, 1.0f),
					120, new Color(255, 50, 80), Main.rand.NextFloat(0.7f, 1.2f));
				glow.noGravity = true;
			}
		}

		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 18; i++) {
				float ang = MathHelper.TwoPi * i / 18f;
				Vector2 vel = ang.ToRotationVector2() * Main.rand.NextFloat(2f, 5f);
				Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.RedTorch, vel,
					0, new Color(255, 70, 70), Main.rand.NextFloat(1.0f, 1.6f));
				d.noGravity = true;
			}

			if (Main.myPlayer != Projectile.owner) return;
			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
				ModContent.ProjectileType<LupineScarletExplosion>(),
				Projectile.damage, Projectile.knockBack, Projectile.owner);
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D tex = ModContent.Request<Texture2D>(ShardTexPath).Value;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Vector2 origin = new Vector2(0f, tex.Height * 0.5f);
			float rot = Projectile.rotation;

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
				SamplerState.LinearClamp, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			Color glowColor = new Color(255, 60, 60, 0) * 0.55f;
			Main.spriteBatch.Draw(tex, drawPos, null, glowColor, rot, origin, 1.30f, SpriteEffects.None, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
				SamplerState.LinearClamp, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			Main.spriteBatch.Draw(tex, drawPos, null, lightColor, rot, origin, 1.0f, SpriteEffects.None, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}
	}

	public class LupineScarletExplosion : ModProjectile
	{
		private const int Time = 12;
		public override string Texture => "Terraria/Images/MagicPixel";

		public override void SetDefaults() {
			Projectile.width = 96;
			Projectile.height = 96;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = Time;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.hide = true;
		}

		public override void AI() {
			if (Projectile.localAI[0] == 0f) {
				Projectile.localAI[0] = 1f;
				SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
				for (int i = 0; i < 28; i++) {
					Vector2 v = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(6f, 12f);
					int d = Dust.NewDust(Projectile.Center, 0, 0, DustID.RedTorch, v.X, v.Y, 120, default, 2.2f);
					Main.dust[d].noGravity = true;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor) => false;
	}
}
