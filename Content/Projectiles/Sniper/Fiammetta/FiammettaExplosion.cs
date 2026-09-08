using System;
using ArknightsMod.Common.Particle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.CameraModifiers;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Fiammetta
{
	/// <summary>
	/// 所有菲亚梅塔炮击共用的伤害爆炸。生成纹理只负责形状，颜色、层次、冲击环和粒子均由代码控制，
	/// 因而同一素材可以分别表达普通炮击、S2 终爆、S3 内外圈和沿途灼痕。
	/// </summary>
	public sealed class FiammettaExplosion : ModProjectile
	{
		private const int StandardVisualLifetime = 8;
		private const int NormalVisualLifetime = 14;
		private const int ReponiteVisualLifetime = 18;
		private int Mode => Math.Clamp((int)Projectile.ai[0], 0, 4);
		private float Radius => Projectile.width * 0.5f;
		private float VisualLifetime => Mode switch
		{
			0 => NormalVisualLifetime,
			3 => ReponiteVisualLifetime,
			_ => StandardVisualLifetime
		};

		public override string Texture => "Terraria/Images/MagicPixel";

		public override void SetDefaults()
		{
			Projectile.width = 132;
			Projectile.height = 132;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = StandardVisualLifetime;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override void OnSpawn(IEntitySource source)
		{
			int size = Mode switch
			{
				1 => 154,
				2 => 196,
				3 => 216,
				4 => 100,
				_ => 136
			};
			Vector2 center = Projectile.Center;
			Projectile.width = size;
			Projectile.height = size;
			Projectile.Center = center;
			Projectile.timeLeft = (int)VisualLifetime;
		}

		public override bool? CanDamage() => Projectile.localAI[0] <= 2f ? null : false;

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float nearestX = MathHelper.Clamp(Projectile.Center.X, targetHitbox.Left, targetHitbox.Right);
			float nearestY = MathHelper.Clamp(Projectile.Center.Y, targetHitbox.Top, targetHitbox.Bottom);
			return Vector2.DistanceSquared(Projectile.Center, new Vector2(nearestX, nearestY)) <= Radius * Radius;
		}

		public override void AI()
		{
			if (Projectile.localAI[0] == 0f)
			{
				FiammettaVisuals.SpawnExplosionParticles(Projectile.Center, Radius, Mode);
				if (!Main.dedServ)
				{
					SoundEngine.PlaySound(SoundID.Item14 with
					{
						Volume = Mode == 4 ? 0.46f : Mode == 3 ? 0.96f : 0.78f,
						Pitch = Mode == 4 ? 0.24f : Mode == 3 ? -0.32f : -0.12f,
						PitchVariance = 0.09f,
						MaxInstances = 8
					}, Projectile.Center);
					if (Mode is 2 or 3)
						SoundEngine.PlaySound(SoundID.Item74 with
						{
							Volume = Mode == 3 ? 0.66f : 0.38f,
							Pitch = Mode == 3 ? -0.38f : -0.22f
						}, Projectile.Center);
					if (Mode == 3)
						SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with
						{
							Volume = 0.52f,
							Pitch = -0.28f,
							MaxInstances = 4
						}, Projectile.Center);

					Player viewer = Main.LocalPlayer;
					if (viewer.active && Vector2.DistanceSquared(viewer.Center, Projectile.Center) < 1000f * 1000f)
					{
						float strength = Mode switch { 2 => 7.5f, 3 => 13.4f, 4 => 2.4f, _ => 5.2f };
						Vector2 direction = (viewer.Center - Projectile.Center).SafeNormalize(Vector2.UnitY);
						Main.instance.CameraModifiers.Add(new PunchCameraModifier(
							Projectile.Center, direction, strength, Mode == 3 ? 7.2f : 5.5f,
							Mode == 4 ? 4 : Mode == 3 ? 12 : 7));
					}
				}
			}

			if (!Main.dedServ && Mode == 3)
			{
				if (Projectile.localAI[0] == 2f)
					FiammettaVisuals.SpawnReponiteAftershock(Projectile.Center, Radius, 0);
				else if (Projectile.localAI[0] == 5f)
					FiammettaVisuals.SpawnReponiteAftershock(Projectile.Center, Radius, 1);
			}

			float lightProgress = MathHelper.Clamp(Projectile.localAI[0] / VisualLifetime, 0f, 1f);
			Lighting.AddLight(Projectile.Center,
				new Vector3(1f, 0.22f, 0.035f) * (1f - lightProgress)
				* (Mode == 3 ? 1.65f : 1f));
			Projectile.localAI[0]++;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
		{
			// “你须偿还”的小内圈承担真正的高倍率；外圈仍保留大范围压制，但不会全屏吃满伤害。
			if (Mode == 3 && Vector2.Distance(target.Center, Projectile.Center) <= 62f)
				modifiers.SourceDamage *= 2.05f;
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float progress = MathHelper.Clamp(Projectile.localAI[0] / VisualLifetime, 0f, 1f);
			FiammettaVisuals.DrawExplosion(Projectile.Center, Projectile.width, progress, Mode);
			return false;
		}

		internal static int Spawn(IEntitySource source, Vector2 position, int damage, float knockback,
			int owner, int mode, int critChance)
		{
			int index = Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<FiammettaExplosion>(),
				damage, knockback, owner, mode);
			if (Main.projectile.IndexInRange(index))
				Main.projectile[index].CritChance = critChance;
			return index;
		}
	}

	/// <summary>
	/// 菲亚梅塔专用绘制库。爆炸 PNG 是灰度发光遮罩：代码负责红、橙、金三层着色，
	/// 原版 SharpTears 负责高速拖尾，因此没有任何灾厄素材或着色器依赖。
	/// </summary>
	internal static class FiammettaVisuals
	{
		private const string ExplosionPath =
			"ArknightsMod/Content/Projectiles/Sniper/Fiammetta/Assets/FiammettaExplosionBloom";
		private const string ShockRingPath =
			"ArknightsMod/Content/Textures/circle_03";
		private const string FlamePath =
			"ArknightsMod/Content/Textures/muzzle_02";
		private const string SparkPath =
			"ArknightsMod/Content/Textures/spark_07";

		internal static void BeginAdditive()
		{
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
				DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		internal static void EndAdditive()
		{
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
				DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
		}

		internal static void DrawExplosion(Vector2 worldPosition, float diameter, float progress, int mode)
		{
			if (Main.dedServ)
				return;
			if (mode == 3)
			{
				DrawReponiteSupernovaExplosion(worldPosition, diameter, progress);
				return;
			}
			if (mode == 0)
			{
				DrawNormalMortarExplosion(worldPosition, diameter, progress);
				return;
			}

			Texture2D bloom = ModContent.Request<Texture2D>(ExplosionPath).Value;
			Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
			Vector2 position = worldPosition - Main.screenPosition;
			float explosiveEase = 1f - MathF.Pow(1f - progress, 3f);
			float fade = MathF.Pow(1f - progress, 1.35f);
			float baseScale = diameter / Math.Max(1f, bloom.Width);
			float rotation = Main.GlobalTimeWrappedHourly * 0.34f + mode * 0.41f;

			BeginAdditive();
			Main.spriteBatch.Draw(glow, position, null, new Color(255, 39, 12) * (fade * 0.72f),
				0f, glow.Size() * 0.5f, (diameter / glow.Width) * (0.9f + explosiveEase * 0.35f),
				SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(bloom, position, null, new Color(216, 19, 12) * (fade * 0.92f),
				rotation, bloom.Size() * 0.5f, baseScale * (0.58f + explosiveEase * 0.72f), SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(bloom, position, null, new Color(255, 98, 25) * (fade * 0.88f),
				-rotation * 0.72f, bloom.Size() * 0.5f, baseScale * (0.42f + explosiveEase * 0.56f), SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(bloom, position, null, new Color(255, 234, 159) * (fade * 0.76f),
				rotation * 1.31f, bloom.Size() * 0.5f, baseScale * (0.22f + explosiveEase * 0.34f), SpriteEffects.None, 0f);

			EndAdditive();
		}

		private static void DrawNormalMortarExplosion(Vector2 worldPosition, float diameter, float progress)
		{
			Texture2D bloom = ModContent.Request<Texture2D>(ExplosionPath).Value;
			Texture2D ring = ModContent.Request<Texture2D>(ShockRingPath).Value;
			Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
			Vector2 position = worldPosition - Main.screenPosition;
			float rotation = Main.GlobalTimeWrappedHourly * 0.46f;
			float expansion = 1f - MathF.Pow(1f - progress, 3.2f);
			float bodyFade = MathF.Pow(1f - progress, 1.18f);
			float flash = MathF.Pow(1f - MathHelper.Clamp(progress / 0.2f, 0f, 1f), 2f);

			// 一小圈暗红火团压住白热中心，让普通爆炸保持炮弹的实体重量。
			for (int i = 0; i < 4; i++)
			{
				float angle = MathHelper.TwoPi * i / 4f + rotation * 0.12f;
				Vector2 offset = angle.ToRotationVector2() * diameter * expansion * 0.1f;
				float lobeScale = diameter / Math.Max(1f, bloom.Width) * (0.26f + expansion * 0.46f);
				Main.spriteBatch.Draw(bloom, position + offset, null,
					new Color(54, 7, 3, 185) * (bodyFade * 0.5f), rotation + i * 1.31f,
					bloom.Size() * 0.5f, lobeScale, SpriteEffects.None, 0f);
			}

			BeginAdditive();
			float glowScale = diameter / Math.Max(1f, glow.Width);
			Main.spriteBatch.Draw(glow, position, null, new Color(255, 242, 196) * (flash * 0.96f),
				0f, glow.Size() * 0.5f, glowScale * (0.72f + expansion * 0.48f), SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(glow, position, null, new Color(255, 56, 16) * (bodyFade * 0.74f),
				0f, glow.Size() * 0.5f, glowScale * (1.02f + expansion * 0.52f), SpriteEffects.None, 0f);

			float bloomScale = diameter / Math.Max(1f, bloom.Width);
			Main.spriteBatch.Draw(bloom, position, null, new Color(210, 20, 9) * (bodyFade * 0.9f),
				rotation, bloom.Size() * 0.5f, bloomScale * (0.42f + expansion * 0.82f), SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(bloom, position, null, new Color(255, 102, 24) * (bodyFade * 0.82f),
				-rotation * 0.76f, bloom.Size() * 0.5f, bloomScale * (0.3f + expansion * 0.58f), SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(bloom, position, null, new Color(255, 229, 151) * (bodyFade * 0.7f),
				rotation * 1.18f, bloom.Size() * 0.5f, bloomScale * (0.16f + expansion * 0.31f), SpriteEffects.None, 0f);

			// 保留冲击环与爆炸火球。
			DrawExpandingRing(ring, position, diameter, progress, 0.01f, 0.48f,
				0.34f, 1.72f, new Color(255, 112, 38), 0.68f);
			EndAdditive();
		}

		private static void DrawReponiteSupernovaExplosion(Vector2 worldPosition, float diameter, float progress)
		{
			Texture2D bloom = ModContent.Request<Texture2D>(ExplosionPath).Value;
			Texture2D ring = ModContent.Request<Texture2D>(ShockRingPath).Value;
			Texture2D flame = ModContent.Request<Texture2D>(FlamePath).Value;
			Texture2D spark = ModContent.Request<Texture2D>(SparkPath).Value;
			Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
			Vector2 position = worldPosition - Main.screenPosition;
			float rotation = Main.GlobalTimeWrappedHourly * 0.48f;
			float expansion = 1f - MathF.Pow(1f - progress, 4f);
			float bodyFade = MathF.Pow(1f - progress, 0.78f);
			float flash = MathF.Pow(1f - MathHelper.Clamp(progress / 0.22f, 0f, 1f), 2f);

			// 先用 AlphaBlend 放一层暗色烟团，给后面的高亮火球提供实体重量。
			float smokeFade = MathHelper.Clamp((1f - progress) * 1.28f, 0f, 1f);
			for (int i = 0; i < 5; i++)
			{
				float angle = MathHelper.TwoPi * i / 5f + rotation * 0.18f;
				Vector2 offset = angle.ToRotationVector2() * diameter * (0.05f + expansion * 0.12f);
				float lobeScale = diameter / Math.Max(1f, bloom.Width)
					* (0.32f + expansion * (0.58f + i * 0.035f));
				Main.spriteBatch.Draw(bloom, position + offset, null,
					new Color(40, 4, 3, 190) * (smokeFade * 0.58f), rotation + i * 1.17f,
					bloom.Size() * 0.5f, lobeScale, SpriteEffects.None, 0f);
			}

			BeginAdditive();

			// 白热中心只存在极短时间；先打眼，再让红橙火球接管画面。
			float glowScale = diameter / Math.Max(1f, glow.Width);
			Main.spriteBatch.Draw(glow, position, null, new Color(255, 246, 214) * (0.98f * flash),
				0f, glow.Size() * 0.5f, glowScale * (1.1f + expansion * 0.65f), SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(glow, position, null, new Color(255, 56, 18) * (bodyFade * 0.84f),
				0f, glow.Size() * 0.5f, glowScale * (1.55f + expansion * 0.9f), SpriteEffects.None, 0f);

			float bloomScale = diameter / Math.Max(1f, bloom.Width);
			Main.spriteBatch.Draw(bloom, position, null, new Color(178, 12, 10) * (bodyFade * 0.96f),
				rotation, bloom.Size() * 0.5f, bloomScale * (0.42f + expansion * 1.22f), SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(bloom, position, null, new Color(255, 79, 19) * (bodyFade * 0.92f),
				-rotation * 0.73f, bloom.Size() * 0.5f, bloomScale * (0.3f + expansion * 0.92f), SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(bloom, position, null, new Color(255, 211, 119) * (bodyFade * 0.78f),
				rotation * 1.34f, bloom.Size() * 0.5f, bloomScale * (0.18f + expansion * 0.54f), SpriteEffects.None, 0f);

			DrawExpandingRing(ring, position, diameter, progress, 0.02f, 0.66f,
				0.42f, 2.8f, new Color(255, 77, 22), 0.84f);
			DrawExpandingRing(ring, position, diameter, progress, 0.13f, 0.86f,
				0.28f, 2.05f, new Color(255, 224, 154), 0.68f);

			// 类似超新星爆点的轴向高速火星，但使用菲亚梅塔自己的红金配色与本项目纹理。
			float rayFade = MathF.Pow(1f - MathHelper.Clamp(progress / 0.36f, 0f, 1f), 1.4f);
			for (int i = 0; i < 8; i++)
			{
				float angle = MathHelper.TwoPi * i / 8f + rotation * 0.24f;
				Color rayColor = i % 2 == 0 ? new Color(255, 238, 188) : new Color(255, 104, 30);
				Main.spriteBatch.Draw(spark, position, null, rayColor * (rayFade * 0.72f), angle,
					spark.Size() * 0.5f,
					new Vector2(diameter * 1.55f / spark.Width, diameter * 0.42f / spark.Height),
					SpriteEffects.None, 0f);
			}

			float flameFade = MathF.Pow(1f - progress, 1.08f);
			for (int i = 0; i < 4; i++)
			{
				float angle = MathHelper.TwoPi * i / 4f + MathHelper.PiOver4;
				Vector2 offset = angle.ToRotationVector2() * diameter * expansion * 0.2f;
				Main.spriteBatch.Draw(flame, position + offset, null,
					new Color(255, 110, 35) * (flameFade * 0.42f), angle + MathHelper.PiOver2,
					flame.Size() * 0.5f, diameter / flame.Width * (0.46f + expansion * 0.58f),
					SpriteEffects.None, 0f);
			}

			EndAdditive();
		}

		private static void DrawExpandingRing(Texture2D ring, Vector2 position, float diameter,
			float progress, float start, float end, float startScale, float endScale,
			Color color, float opacity)
		{
			float ringProgress = Utils.GetLerpValue(start, end, progress, true);
			if (ringProgress <= 0f || ringProgress >= 1f)
				return;
			float eased = 1f - MathF.Pow(1f - ringProgress, 3f);
			float fade = MathF.Sin(MathHelper.Pi * ringProgress);
			float scale = diameter / Math.Max(1f, ring.Width)
				* MathHelper.Lerp(startScale, endScale, eased);
			Main.spriteBatch.Draw(ring, position, null, color * (fade * opacity), 0f,
				ring.Size() * 0.5f, scale, SpriteEffects.None, 0f);
		}

		internal static void DrawShellTrail(Projectile projectile, Color outer, Color inner, float width)
		{
			if (Main.dedServ)
				return;

			Texture2D tear = TextureAssets.Extra[ExtrasID.SharpTears].Value;
			Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
			BeginAdditive();
			for (int i = projectile.oldPos.Length - 1; i >= 0; i--)
			{
				if (projectile.oldPos[i] == Vector2.Zero)
					continue;
				float age = 1f - i / (float)Math.Max(1, projectile.oldPos.Length - 1);
				Vector2 center = projectile.oldPos[i] + projectile.Size * 0.5f;
				Vector2 next = i > 0 && projectile.oldPos[i - 1] != Vector2.Zero
					? projectile.oldPos[i - 1] + projectile.Size * 0.5f
					: projectile.Center;
				float rotation = (next - center).SafeNormalize(Vector2.UnitY).ToRotation();
				Color color = Color.Lerp(outer, inner, age) * (age * age * 0.68f);
				float length = MathHelper.Lerp(18f, 58f, age);
				Vector2 scale = new(width * age / Math.Max(1f, tear.Width), length / Math.Max(1f, tear.Height));
				Main.spriteBatch.Draw(tear, center - Main.screenPosition, null, color,
					rotation + MathHelper.PiOver2, tear.Size() * 0.5f, scale, SpriteEffects.None, 0f);
			}

			Main.spriteBatch.Draw(glow, projectile.Center - Main.screenPosition, null,
				inner * 0.82f, 0f, glow.Size() * 0.5f, new Vector2(0.16f, 0.10f), SpriteEffects.None, 0f);
			EndAdditive();
		}

		internal static void DrawFlyingCore(Vector2 worldPosition, float rotation, float scale)
		{
			if (Main.dedServ)
				return;

			Texture2D tear = TextureAssets.Extra[ExtrasID.SharpTears].Value;
			Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
			Vector2 position = worldPosition - Main.screenPosition;
			BeginAdditive();
			Main.spriteBatch.Draw(glow, position, null, new Color(255, 49, 17) * 0.82f,
				0f, glow.Size() * 0.5f, new Vector2(0.25f, 0.13f) * scale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(tear, position, null, new Color(255, 231, 165) * 0.92f,
				rotation, tear.Size() * 0.5f,
				new Vector2(11f / Math.Max(1f, tear.Width), 46f / Math.Max(1f, tear.Height)) * scale,
				SpriteEffects.None, 0f);
			EndAdditive();
		}

		internal static void SpawnExplosionParticles(Vector2 center, float radius, int mode)
		{
			if (Main.dedServ)
				return;

			int count = mode switch { 2 => 48, 3 => 56, 4 => 18, _ => 34 };
			for (int i = 0; i < count; i++)
			{
				Vector2 direction = Main.rand.NextVector2Unit();
				float speed = Main.rand.NextFloat(2.5f, 9.5f) * (0.75f + radius / 180f);
				Color color = Color.Lerp(new Color(226, 27, 14), new Color(255, 232, 151), Main.rand.NextFloat());
				var particle = new DefaultParticle(center + direction * Main.rand.NextFloat(2f, radius * 0.28f),
					direction * speed, Main.rand.Next(16, 31), Main.rand.NextFloat(0.34f, 0.76f), color, true)
				{
					Deformation = new Vector2(Main.rand.NextFloat(0.18f, 0.38f), Main.rand.NextFloat(1.8f, 3.1f))
				};
				particle.Spawn();
			}

			for (int i = 0; i < count / 2; i++)
			{
				Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(2.2f, 8.2f);
				Dust dust = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(radius * 0.15f, radius * 0.15f),
					i % 3 == 0 ? DustID.GoldFlame : DustID.Torch, velocity, 35,
					new Color(255, 74, 24), Main.rand.NextFloat(0.9f, 1.55f));
				dust.noGravity = true;
			}

			if (mode == 0)
				SpawnNormalVanillaFireDust(center, radius);

			if (mode == 3)
				SpawnReponiteSupernovaBurst(center, radius);
		}

		private static void SpawnNormalVanillaFireDust(Vector2 center, float radius)
		{
			// 较粗的火焰尘埃贴着爆心冲出，和细长的项目粒子形成两种不同轮廓。
			for (int i = 0; i < 14; i++)
			{
				Vector2 direction = Main.rand.NextVector2Unit();
				int dustType = i % 5 == 0 ? DustID.SolarFlare
					: i % 3 == 0 ? DustID.GoldFlame : DustID.Torch;
				Dust flame = Dust.NewDustPerfect(
					center + direction * Main.rand.NextFloat(3f, radius * 0.22f), dustType,
					direction * Main.rand.NextFloat(3.6f, 8.8f) + Main.rand.NextVector2Circular(0.7f, 0.7f),
					Main.rand.Next(20, 55), new Color(255, 101, 29), Main.rand.NextFloat(1.05f, 1.72f));
				flame.noGravity = true;
				flame.fadeIn = Main.rand.NextFloat(0.35f, 0.75f);
			}

			// 少量慢余烬负责收尾；保留重力，避免所有火粒子都像同一圈放射线。
			for (int i = 0; i < 8; i++)
			{
				Vector2 velocity = new(Main.rand.NextFloat(-3.1f, 3.1f), Main.rand.NextFloat(-5.2f, -1.4f));
				Dust ember = Dust.NewDustPerfect(center + Main.rand.NextVector2Circular(radius * 0.18f,
					radius * 0.12f), i % 3 == 0 ? DustID.GoldFlame : DustID.Torch, velocity,
					Main.rand.Next(55, 95), new Color(255, 62, 18), Main.rand.NextFloat(0.72f, 1.12f));
				ember.noGravity = false;
			}
		}

		private static void SpawnReponiteSupernovaBurst(Vector2 center, float radius)
		{
			// 高速长火星提供爆炸的第一拍，暗红烟尘提供慢一拍的体积；两者速度区间刻意分离。
			for (int i = 0; i < 42; i++)
			{
				Vector2 direction = Main.rand.NextVector2Unit();
				float speed = Main.rand.NextFloat(10f, 25f) * (0.85f + radius / 260f);
				float roll = Main.rand.NextFloat();
				Color color = roll < 0.18f ? new Color(255, 244, 211)
					: roll < 0.56f ? new Color(255, 163, 67) : new Color(224, 36, 18);
				var spark = new DefaultParticle(center + direction * Main.rand.NextFloat(4f, radius * 0.18f),
					direction * speed, Main.rand.Next(22, 39), Main.rand.NextFloat(0.32f, 0.7f), color, true)
				{
					Deformation = new Vector2(Main.rand.NextFloat(0.1f, 0.2f), Main.rand.NextFloat(3.4f, 5.4f))
				};
				spark.Spawn();
			}

			for (int i = 0; i < 34; i++)
			{
				Vector2 direction = Main.rand.NextVector2Unit();
				Vector2 velocity = direction * Main.rand.NextFloat(1.4f, 5.8f);
				Dust smoke = Dust.NewDustPerfect(center + direction * Main.rand.NextFloat(4f, radius * 0.34f),
					DustID.Smoke, velocity, Main.rand.Next(65, 125), new Color(82, 16, 10),
					Main.rand.NextFloat(1.35f, 2.35f));
				smoke.noGravity = false;
			}
		}

		internal static void SpawnReponiteAftershock(Vector2 center, float radius, int phase)
		{
			if (Main.dedServ)
				return;

			int count = phase == 0 ? 24 : 16;
			float angularOffset = Main.rand.NextFloat(MathHelper.TwoPi);
			for (int i = 0; i < count; i++)
			{
				Vector2 direction = (angularOffset + MathHelper.TwoPi * i / count).ToRotationVector2();
				Vector2 position = center + direction * radius * (phase == 0 ? 0.34f : 0.52f);
				Color color = i % 4 == 0 ? new Color(255, 240, 195)
					: phase == 0 ? new Color(255, 128, 38) : new Color(213, 31, 17);
				var particle = new DefaultParticle(position,
					direction * Main.rand.NextFloat(phase == 0 ? 7f : 4f, phase == 0 ? 13f : 9f),
					Main.rand.Next(15, 25), Main.rand.NextFloat(0.28f, 0.55f), color, true)
				{
					Deformation = new Vector2(0.14f, phase == 0 ? 3.6f : 2.7f)
				};
				particle.Spawn();
			}
		}
	}
}
