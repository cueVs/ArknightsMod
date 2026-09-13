using System;
using ArknightsMod.Content.Projectiles.BasePROJ;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Hellagur
{
	/// <summary>S3「满月」专属血月刀气。只承担视觉表现并受物块阻挡，伤害统一由 2.0 倍母刀结算。</summary>
	public class HellagurBloodSlash : ModProjectile
	{
		private const int Life = 22;
		private const float BackReach = 32f;
		private const float FrontReach = 60f;

		public override string Texture => "Terraria/Images/MagicPixel";

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailCacheLength[Type] = 6;
			ProjectileID.Sets.TrailingMode[Type] = 0;
		}

		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.friendly = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = Life;
		}

		public override bool? CanDamage() => false;

		public override void AI() {
			if (Projectile.velocity.LengthSquared() < 0.01f) {
				Projectile.Kill();
				return;
			}

			Projectile.rotation = Projectile.velocity.ToRotation();
			Vector2 direction = Projectile.rotation.ToRotationVector2();
			// 用视觉前缘预探测墙体，不能只等 22px 的中心碰撞盒撞墙，否则 60px 刀光仍会先穿出墙面。
			if (!Collision.CanHitLine(Projectile.Center, 1, 1,
				Projectile.Center + direction * FrontReach, 1, 1)) {
				SpawnWallImpact(Projectile.velocity);
				Projectile.Kill();
				return;
			}
			Projectile.velocity *= 0.955f;
			float intensity = MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
			Lighting.AddLight(Projectile.Center, new Vector3(0.82f, 0.12f, 0.045f) * (0.75f + intensity * 0.25f));

			if (!Main.dedServ && Main.rand.NextBool(2)) {
				Vector2 normal = new(-direction.Y, direction.X);
				float along = Main.rand.NextFloat(-BackReach, FrontReach);
				Vector2 position = Projectile.Center + direction * along + normal * Main.rand.NextFloat(-7f, 7f);
				Dust dust = Dust.NewDustPerfect(position, Main.rand.NextBool(5) ? DustID.GoldFlame : DustID.Blood,
					Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(0.8f, 0.8f), 30,
					new Color(255, 90, 48), Main.rand.NextFloat(0.8f, 1.25f));
				dust.noGravity = true;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			SpawnWallImpact(oldVelocity);
			return true;
		}

		private void SpawnWallImpact(Vector2 oldVelocity) {
			if (!Main.dedServ) {
				SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.3f, Pitch = -0.25f }, Projectile.Center);
				for (int i = 0; i < 12; i++) {
					Dust dust = Dust.NewDustPerfect(Projectile.Center,
						i % 4 == 0 ? DustID.GoldFlame : DustID.Blood,
						-oldVelocity.SafeNormalize(Vector2.UnitX).RotatedByRandom(0.9f) * Main.rand.NextFloat(1.2f, 3.8f),
						35, new Color(255, 108, 55), Main.rand.NextFloat(0.75f, 1.2f));
					dust.noGravity = true;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
			Texture2D slashSmear = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Projectiles/Guard/Hellagur/HellagurBloodSlashBoss").Value;
			SpriteBatch spriteBatch = Main.spriteBatch;
			float fade = GetFade();
			float lowHealth = MathHelper.Clamp(Projectile.ai[0], 0f, 1f);
			Color crimson = Color.Lerp(new Color(164, 18, 34), new Color(246, 36, 48), lowHealth);
			Color deepRed = Color.Lerp(new Color(72, 4, 18), new Color(154, 12, 28), lowHealth);
			Color paleEdge = Color.Lerp(new Color(184, 174, 172), new Color(255, 218, 178), lowHealth);
			Vector2 origin = slashSmear.Size() * 0.5f;
			Vector2 direction = Projectile.rotation.ToRotationVector2();
			// 新图的主刃轴为纵向，逆转 90 度后始终沿飞行方向展开。
			float slashRotation = Projectile.rotation - MathHelper.PiOver2;
			float slashScale = MathHelper.Lerp(0.31f, 0.35f, lowHealth);

			// 只保留一层紧贴刀形的暗色背板，不再用 MagicPixel 拼巨大黑月牙。
			spriteBatch.Draw(slashSmear, Projectile.Center - Main.screenPosition, null,
				new Color(24, 0, 7, 230) * (0.68f * fade), slashRotation, origin,
				slashScale * 1.08f, SpriteEffects.None, 0f);

			BaseHeldMeleeSupport.BeginAdditive(spriteBatch);
			for (int i = Projectile.oldPos.Length - 1; i >= 1; i--) {
				if (Projectile.oldPos[i] == Vector2.Zero)
					continue;
				float history = 1f - i / (float)Projectile.oldPos.Length;
				Vector2 oldCenter = Projectile.oldPos[i] + Projectile.Size * 0.5f;
				float echoScale = slashScale * MathHelper.Lerp(0.74f, 0.94f, history);
				spriteBatch.Draw(slashSmear, oldCenter - Main.screenPosition, null,
					deepRed * (fade * history * 0.22f), slashRotation, origin,
					echoScale, SpriteEffects.None, 0f);
			}

			spriteBatch.Draw(slashSmear, Projectile.Center - Main.screenPosition, null,
				crimson * (0.82f * fade), slashRotation, origin,
				slashScale, SpriteEffects.None, 0f);
			spriteBatch.Draw(slashSmear, Projectile.Center + direction * 2f - Main.screenPosition, null,
				paleEdge * (0.48f * fade), slashRotation, origin,
				slashScale * 0.72f, SpriteEffects.None, 0f);
			BaseHeldMeleeSupport.DrawSharpTear(Projectile.Center - direction * 3f, Projectile.rotation,
				crimson, 82f, 7f, 0.42f * fade);
			BaseHeldMeleeSupport.DrawSharpTear(Projectile.Center + direction * 9f, Projectile.rotation,
				paleEdge, 56f, 2.4f, 0.54f * fade);
			BaseHeldMeleeSupport.DrawGlow(glow, Projectile.Center + direction * 28f,
				crimson * (0.34f * fade), 0.16f * fade);
			BaseHeldMeleeSupport.DrawGlow(glow, Projectile.Center + direction * 31f,
				paleEdge * (0.24f * fade), 0.07f * fade);

			BaseHeldMeleeSupport.EndAdditive(spriteBatch);
			return false;
		}

		private float GetFade() {
			float age = Life - Projectile.timeLeft;
			float fadeIn = MathHelper.Clamp(age / 3f, 0f, 1f);
			float fadeOut = MathHelper.Clamp(Projectile.timeLeft / 7f, 0f, 1f);
			return Math.Min(fadeIn, fadeOut);
		}
	}
}
