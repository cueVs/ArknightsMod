using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System;

namespace ArknightsMod.Content.Projectiles.Caster.Lava
{
	public class Lava_Dagger_Explode : ModProjectile
	{
		public override void SetStaticDefaults() { }

		public override void SetDefaults()
		{
			Projectile.width = Projectile.height = 96;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 120;
			Projectile.Opacity = 1f;
			Projectile.scale = 0.1f;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override void AI()
		{
			Vector2 center = Projectile.Center;

			Projectile.width = (int)(96 * Projectile.scale);
			Projectile.height = (int)(96 * Projectile.scale);

			Projectile.Center = center;

			if (Projectile.timeLeft > 30)
			{
				Projectile.rotation = Projectile.velocity.ToRotation();

				for (int i = 0; i < 6; i++) {
					Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
					dust.noGravity = true;
				}
			}
			else
			{
				Projectile.velocity = Vector2.Zero;

				if (Projectile.timeLeft > 25)
				{
					float progress = 1f - (Projectile.timeLeft - 25) / 5f;
					Projectile.scale = MathHelper.Lerp(0.1f, 0.8f, progress);
				}
				else
			{
					float progress = 1f - (float)Projectile.timeLeft / 25f;
					Projectile.scale = MathHelper.Lerp(0.8f, 1f, progress);
					Projectile.Opacity = (float)Projectile.timeLeft / 50f;
				}
			}

			if (Projectile.wet)
			{
				Projectile.Kill();
			}
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (target.Hitbox.Distance(Projectile.Center) > Projectile.width / 2f * Projectile.scale)
				return false;

			return base.CanHitNPC(target);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
			{
			StartExplosion();
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			StartExplosion();
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			StartExplosion();
			return false;
		}

		private void StartExplosion()
		{
			if (Projectile.timeLeft > 30)
			{
				Projectile.tileCollide = false;
				Projectile.timeLeft = 30;

				SoundEngine.PlaySound(SoundID.Item14, Projectile.position);

				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < 4; j++)
					{
						float angle = i * MathHelper.PiOver4;
						Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)).RotatedByRandom(MathHelper.PiOver4 / 2) * Main.rand.NextFloat(0f, 2f);

						Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
						dust.velocity = velocity;
					}
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			if (Projectile.timeLeft > 30)
			{
				return false;
			}
			else
			{
				Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

				Color color = Color.White * Projectile.Opacity;

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState,
					DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				ArknightsMod.LavaExplosionShaderEffect.Value.Parameters["opacity"].SetValue(Projectile.Opacity);
				ArknightsMod.LavaExplosionShaderEffect.Value.Parameters["orangeThreshold"].SetValue(0.8f);
				ArknightsMod.LavaExplosionShaderEffect.Value.Parameters["yellowThreshold"].SetValue(0.9f);
				ArknightsMod.LavaExplosionShaderEffect.Value.CurrentTechnique.Passes[0].Apply();

				for (int i = 0; i < 2; i++)
				{
					Main.EntitySpriteDraw(
						texture,
						Projectile.Center - Main.screenPosition,
						null,
						color,
						0f,
						texture.Size() * 0.5f,
						Projectile.scale,
						SpriteEffects.None,
						0
					);
				}

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
					DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

				return false;
			}

			//模组目前不考虑多人，这些貌似目前不需要（？）
			/*
			public override bool CanHitPlayer(Player target)
			{
				if (target.Hitbox.Distance(Projectile.Center) > Projectile.width / 2f * Projectile.scale)
					return false;

				return base.CanHitPlayer(target);
			}

			public override bool CanHitPvp(Player target)
			{
				if (target.Hitbox.Distance(Projectile.Center) > Projectile.width / 2f * Projectile.scale)
					return false;

				return base.CanHitPvp(target);
			}
			*/
		}
	}
}