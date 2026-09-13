using System;
using ArknightsMod.Content.Buffs;
using ArknightsMod.Common.Particle;
using ArknightsMod.Content.Projectiles.BasePROJ;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Blaze
{
	/// <summary>
	/// 链锯离心甩出的炽血锯屑。弹幕本体由独立的液滴轮廓、流体拖尾和热芯绘制塑形，
	/// 周围再叠现有发光粒子及血/火 Dust；两套表现互不替代。
	/// </summary>
	public sealed class BlazeScarletEjecta : ModProjectile
	{
		private const int ArmTicks = 10;
		private const int TileGraceTicks = 12;
		private const int Life = 150;
		// 普通、延长及大招终爆的血液喷射夹角统一扩大 30%，不改变数量或伤害。
		private const float SpraySpreadMultiplier = 1.3f;
		private bool impactPlayed;

		private int Age => (int)Projectile.localAI[0];
		private float Power => MathHelper.Clamp(Projectile.ai[0], 0.5f, 1.5f);

		public override string Texture => "Terraria/Images/Extra_98";

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 14;
			ProjectileID.Sets.TrailingMode[Type] = 0;
		}

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = 1;
			Projectile.timeLeft = Life;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.usesIDStaticNPCImmunity = true;
			Projectile.idStaticNPCHitCooldown = 10;
		}

		public override bool? CanDamage() => Age >= ArmTicks ? null : false;

		public override void AI() {
			Projectile.localAI[0]++;
			Projectile.tileCollide = Age >= TileGraceTicks;
			Projectile.rotation = Projectile.velocity.ToRotation();

			// 先保持短促的直线喷射，再逐步坠落，表现炽热液滴与金属碎屑混合后的重量。
			if (Age >= 7)
				Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.18f, 16f);
			Projectile.velocity.X *= 0.996f;
			Lighting.AddLight(Projectile.Center, new Vector3(0.72f, 0.08f, 0.025f) * Power);

			if (Main.dedServ || Age <= 2)
				return;

			if (Main.rand.NextBool(2)) {
				Color color = Main.rand.NextBool(3)
					? new Color(255, 178, 72)
					: new Color(226, 12, 38);
				var particle = new DefaultParticle(
					Projectile.Center - Projectile.velocity * Main.rand.NextFloat(0.05f, 0.22f),
					-Projectile.velocity * Main.rand.NextFloat(0.015f, 0.045f),
					Main.rand.Next(11, 19), Main.rand.NextFloat(0.2f, 0.42f) * Power, color, true) {
					Deformation = new Vector2(0.25f, Main.rand.NextFloat(1.2f, 2.1f))
				};
				particle.Spawn();
			}

			if (Main.rand.NextBool(7)) {
				Dust drip = Dust.NewDustPerfect(Projectile.Center,
					Main.rand.NextBool(4) ? DustID.Torch : DustID.Blood,
					Projectile.velocity * -0.08f + Main.rand.NextVector2Circular(0.8f, 0.8f),
					25, new Color(255, 45, 38), Main.rand.NextFloat(0.75f, 1.15f));
				drip.noGravity = false;
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (damageDone > 0)
				target.AddBuff(ModContent.BuffType<WeaponBleedingDebuff>(), 180);
			SpawnImpact(Projectile.velocity.SafeNormalize(Vector2.UnitY));
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			SpawnImpact(oldVelocity.SafeNormalize(Vector2.UnitY));
			return true;
		}

		public override void OnKill(int timeLeft) {
			if (!impactPlayed)
				SpawnImpact(Projectile.velocity.SafeNormalize(Vector2.UnitY));
		}

		private void SpawnImpact(Vector2 incoming) {
			if (impactPlayed)
				return;
			impactPlayed = true;
			if (Main.dedServ)
				return;

			Vector2 rebound = -incoming.SafeNormalize(-Vector2.UnitY);
			for (int i = 0; i < 7; i++) {
				Vector2 velocity = rebound.RotatedByRandom(1.2f) * Main.rand.NextFloat(2.4f, 7.5f);
				Color color = i % 3 == 0 ? new Color(255, 190, 88) : new Color(232, 10, 36);
				var particle = new DefaultParticle(Projectile.Center, velocity, Main.rand.Next(16, 29),
					Main.rand.NextFloat(0.32f, 0.68f) * Power, color, false) {
					Deformation = new Vector2(0.3f, Main.rand.NextFloat(1.25f, 2.2f))
				};
				particle.Spawn();
			}
			BlazeVisuals.SpawnMetalSparks(Projectile.Center, rebound, 6, Power, 6.5f);
			SoundEngine.PlaySound(SoundID.NPCHit18 with {
				Volume = 0.24f,
				Pitch = 0.24f,
				PitchVariance = 0.18f,
				MaxInstances = 8
			}, Projectile.Center);
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			SpriteBatch spriteBatch = Main.spriteBatch;
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
			float fadeIn = MathHelper.Clamp(Age / 4f, 0f, 1f);
			float fadeOut = MathHelper.Clamp(Projectile.timeLeft / 10f, 0f, 1f);
			float fade = Math.Min(fadeIn, fadeOut);
			float pulse = 0.9f + MathF.Sin(Age * 0.65f) * 0.1f;

			// AlphaBlend 层给弹幕真正的红黑液体轮廓；这部分即使粒子被关闭也始终存在。
			DrawLiquidSilhouette(spriteBatch, pixel, glow, fade, pulse);

			BaseHeldMeleeSupport.BeginAdditive(spriteBatch);
			DrawLiquidHeat(spriteBatch, glow, fade, pulse);
			for (int i = Projectile.oldPos.Length - 1; i >= 1; i--) {
				if (Projectile.oldPos[i] == Vector2.Zero)
					continue;
				float strength = 1f - i / (float)Projectile.oldPos.Length;
				Vector2 center = Projectile.oldPos[i] + Projectile.Size * 0.5f;
				BaseHeldMeleeSupport.DrawSharpTear(center, Projectile.oldRot[i], new Color(178, 4, 30),
					24f * Projectile.scale * Power, 7f * Projectile.scale, fade * 0.2f * strength);
			}

			BaseHeldMeleeSupport.DrawSharpTear(Projectile.Center, Projectile.rotation, new Color(238, 12, 38),
				38f * Projectile.scale * Power * pulse, 14f * Projectile.scale, 0.78f * fade);
			BaseHeldMeleeSupport.DrawSharpTear(Projectile.Center, Projectile.rotation, new Color(255, 205, 112),
				23f * Projectile.scale * Power * pulse, 4.5f * Projectile.scale, 0.76f * fade);
			BaseHeldMeleeSupport.EndAdditive(spriteBatch);
			return false;
		}

		private void DrawLiquidSilhouette(SpriteBatch spriteBatch, Texture2D pixel, Texture2D glow,
			float fade, float pulse) {
			Rectangle source = new(0, 0, 1, 1);
			Vector2 glowOrigin = glow.Size() * 0.5f;
			Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
			Vector2 normal = new(-forward.Y, forward.X);
			float bodyScale = Projectile.scale * Power;

			// 先连起历史位置，再用互相重叠的软质液团盖住几何接缝，形成不断被拉长的血液丝带。
			Vector2 previous = Projectile.Center;
			for (int i = 0; i < Projectile.oldPos.Length; i++) {
				if (Projectile.oldPos[i] == Vector2.Zero)
					continue;
				Vector2 center = Projectile.oldPos[i] + Projectile.Size * 0.5f;
				float strength = 1f - i / (float)Projectile.oldPos.Length;
				Vector2 delta = center - previous;
				if (delta.LengthSquared() > 0.25f) {
					float width = MathHelper.Lerp(1.2f, 9.5f * bodyScale, strength * strength);
					spriteBatch.Draw(pixel, previous - Main.screenPosition, source,
						new Color(76, 0, 14, 205) * (fade * strength), delta.ToRotation(),
						new Vector2(0f, 0.5f), new Vector2(delta.Length(), width), SpriteEffects.None, 0f);
				}

				float blob = (0.055f + strength * 0.105f) * bodyScale;
				float wobble = MathF.Sin(Age * 0.52f - i * 0.9f) * 2.2f * strength;
				spriteBatch.Draw(glow, center + normal * wobble - Main.screenPosition, null,
					new Color(92, 0, 17, 225) * (fade * strength * 0.9f),
					delta.LengthSquared() > 0.25f ? delta.ToRotation() : Projectile.rotation, glowOrigin,
					new Vector2(blob * 1.42f, blob), SpriteEffects.None, 0f);
				previous = center;
			}

			Vector2 drawCenter = Projectile.Center - Main.screenPosition;
			float mainLength = (0.23f + Power * 0.08f) * Projectile.scale * pulse;
			float mainWidth = (0.14f + Power * 0.045f) * Projectile.scale;
			spriteBatch.Draw(glow, drawCenter, null, new Color(88, 0, 14, 238) * fade,
				Projectile.rotation, glowOrigin, new Vector2(mainLength, mainWidth), SpriteEffects.None, 0f);
			spriteBatch.Draw(glow, drawCenter - forward * 5f + normal * 3f, null,
				new Color(132, 2, 24, 218) * (fade * 0.82f), Projectile.rotation, glowOrigin,
				new Vector2(mainLength * 0.58f, mainWidth * 0.72f), SpriteEffects.None, 0f);
			spriteBatch.Draw(glow, drawCenter - forward * 8f - normal * 3.5f, null,
				new Color(64, 0, 12, 205) * (fade * 0.72f), Projectile.rotation, glowOrigin,
				new Vector2(mainLength * 0.42f, mainWidth * 0.55f), SpriteEffects.None, 0f);
		}

		private void DrawLiquidHeat(SpriteBatch spriteBatch, Texture2D glow, float fade, float pulse) {
			Vector2 origin = glow.Size() * 0.5f;
			Vector2 forward = Projectile.velocity.SafeNormalize(Vector2.UnitX);
			Vector2 normal = new(-forward.Y, forward.X);
			Vector2 center = Projectile.Center - Main.screenPosition;
			float scale = Projectile.scale * Power;

			spriteBatch.Draw(glow, center, null, new Color(236, 8, 36, 185) * (fade * 0.72f),
				Projectile.rotation, origin, new Vector2(0.18f, 0.09f) * scale * pulse, SpriteEffects.None, 0f);
			spriteBatch.Draw(glow, center + normal * 2f, null, new Color(255, 98, 44, 175) * (fade * 0.56f),
				Projectile.rotation, origin, new Vector2(0.11f, 0.045f) * scale, SpriteEffects.None, 0f);
			spriteBatch.Draw(glow, center + forward * 7f, null, new Color(255, 218, 142, 165) * (fade * 0.5f),
				Projectile.rotation, origin, new Vector2(0.055f, 0.028f) * scale, SpriteEffects.None, 0f);
		}

		internal static void SpawnCone(IEntitySource source, Vector2 position, Vector2 direction, int count,
			float spread, float minSpeed, float maxSpeed, int damage, float knockback, int owner, float power = 1f) {
			if (owner != Main.myPlayer)
				return;

			direction = direction.SafeNormalize(Vector2.UnitX);
			for (int i = 0; i < count; i++) {
				Vector2 velocity = direction.RotatedByRandom(spread * SpraySpreadMultiplier)
					* Main.rand.NextFloat(minSpeed, maxSpeed);
				velocity.Y -= Main.rand.NextFloat(0.35f, 1.5f);
				Spawn(source, position + Main.rand.NextVector2Circular(4f, 4f), velocity,
					damage, knockback, owner, power);
			}
		}

		/// <summary>在给定总夹角内均匀铺开弹幕；用于需要清楚读出方向的终爆扇面。</summary>
		internal static void SpawnFan(IEntitySource source, Vector2 position, Vector2 direction, int count,
			float totalSpread, float minSpeed, float maxSpeed, int damage, float knockback,
			int owner, float power = 1f) {
			if (owner != Main.myPlayer || count <= 0)
				return;

			direction = direction.SafeNormalize(Vector2.UnitX);
			float start = -totalSpread * SpraySpreadMultiplier * 0.5f;
			for (int i = 0; i < count; i++) {
				float factor = count == 1 ? 0.5f : i / (float)(count - 1);
				float angle = MathHelper.Lerp(start, -start, factor);
				// 只给极小扰动，保持整束仍能明确读成扇面，而不是退化成随机喷泉。
				angle += Main.rand.NextFloat(-0.012f, 0.012f);
				Vector2 velocity = direction.RotatedBy(angle) * Main.rand.NextFloat(minSpeed, maxSpeed);
				Spawn(source, position + Main.rand.NextVector2Circular(3f, 3f), velocity,
					damage, knockback, owner, power);
			}
		}

		internal static void SpawnRadial(IEntitySource source, Vector2 position, int count,
			float minSpeed, float maxSpeed, int damage, float knockback, int owner, float power = 1f) {
			if (owner != Main.myPlayer)
				return;

			float offset = Main.rand.NextFloat(MathHelper.TwoPi);
			for (int i = 0; i < count; i++) {
				float angle = offset + MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.13f, 0.13f);
				Vector2 velocity = angle.ToRotationVector2() * Main.rand.NextFloat(minSpeed, maxSpeed);
				velocity.Y -= Main.rand.NextFloat(0.5f, 2.6f);
				Spawn(source, position + Main.rand.NextVector2Circular(7f, 7f), velocity,
					damage, knockback, owner, power);
			}
		}

		private static void Spawn(IEntitySource source, Vector2 position, Vector2 velocity,
			int damage, float knockback, int owner, float power) {
			int index = Projectile.NewProjectile(source, position, velocity,
				ModContent.ProjectileType<BlazeScarletEjecta>(), Math.Max(1, damage), knockback,
				owner, MathHelper.Clamp(power, 0.5f, 1.5f));
			if (Main.projectile.IndexInRange(index)) {
				Main.projectile[index].scale = Main.rand.NextFloat(0.82f, 1.18f);
				Main.projectile[index].netUpdate = true;
			}
		}
	}
}
