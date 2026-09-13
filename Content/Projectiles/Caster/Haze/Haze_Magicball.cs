using ArknightsMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Haze
{
	public class Haze_Magicball : ModProjectile
	{
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 15;
		}

		public override void SetDefaults() {
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.aiStyle = -1;
			Projectile.timeLeft = 120;
			Projectile.friendly = true;
			Projectile.extraUpdates = 1;
		}

		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (Main.rand.NextBool(4)) {
				Dust d = Dust.NewDustDirect(Projectile.position, 0, 0, ModContent.DustType<Haze_TriangleDust>());
			}
		}

		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 8; i++) {
				float angle = i * MathHelper.PiOver4;
				Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)).RotatedByRandom(MathHelper.PiOver4 / 2) * Main.rand.NextFloat(0f, 8f);

				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.BlueTorch);
				dust.velocity = velocity + Projectile.velocity.SafeNormalize(Vector2.One) * 2;
				dust.scale = 1.5f;
				dust.noGravity = true;
			}

			base.OnKill(timeLeft);
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++) {
				float factor = 1 - (float)i / ProjectileID.Sets.TrailCacheLength[Type];

				Main.EntitySpriteDraw(texture,
					Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition,
					null,
					Color.Lerp(Color.DeepSkyBlue, Color.Blue, (float)i / (ProjectileID.Sets.TrailCacheLength[Type] - 1)),
					Projectile.oldRot[i],
					texture.Size() / 2,
					factor,
					SpriteEffects.None,
					0
					);
			}

			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				Color.DeepSkyBlue,
				Projectile.rotation,
				texture.Size() / 2,
				new Vector2(1),
				SpriteEffects.None,
				0
				);

			for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++) {
				float factor = 1 - (float)i / ProjectileID.Sets.TrailCacheLength[Type];

				Main.EntitySpriteDraw(texture,
					Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition,
					null,
					Color.Lerp(Color.White, Color.Blue, (float)i / (ProjectileID.Sets.TrailCacheLength[Type] - 1)),
					Projectile.oldRot[i],
					texture.Size() / 2,
					new Vector2(0.5f) * factor,
					SpriteEffects.None,
					0
					);
			}

			Main.EntitySpriteDraw(
				texture,
				Projectile.Center - Main.screenPosition,
				null,
				Color.White,
				Projectile.rotation,
				texture.Size() / 2,
				new Vector2(0.5f),
				SpriteEffects.None,
				0
				);

			return false;
		}
	}
}
