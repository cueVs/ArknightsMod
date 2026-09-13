using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System;

namespace ArknightsMod.Content.Projectiles.Caster._12F
{
	public class _12F_Projectile : ModProjectile
	{
		public override void SetStaticDefaults() { }

		private Vector2 spawnPos;
		private Player player => Main.player[Projectile.owner];

		private Item item => player.HeldItem;

		public override void SetDefaults()
		{
			Projectile.width = Projectile.height = 96;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 200;
			Projectile.Opacity = 1f;
			Projectile.scale = 0.1f;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.friendly = true;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override void OnSpawn(IEntitySource source)
		{
			Player player = Main.player[Projectile.owner];
			spawnPos = new Vector2(
				player.Center.X + Main.rand.NextFloat(-100f, 100f), // 左右随机
				player.Center.Y - 500f                               // 天上 800 像素
			);
			Projectile.Center = spawnPos;


		}

		public override void AI()
		{
			Vector2 center = Projectile.Center;

			Projectile.width = (int)(96 * Projectile.scale);
			Projectile.height = (int)(96 * Projectile.scale);

			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.timeLeft > 200-(int)(item.useTime*2/3)){
				Projectile.velocity = Vector2.Zero;
			}
			else if(Projectile.velocity == Vector2.Zero){
				
				Vector2 dir = Main.MouseWorld - spawnPos;
				dir.Normalize();
				Projectile.velocity = dir * 24f; 
			}
			if (Projectile.timeLeft > 30)
			{
				for (int i = 0; i < 6; i++)
				{
					Dust dust = Dust.NewDustDirect(
						Projectile.position,
						Projectile.width,
						Projectile.height,
						DustID.Torch
					);
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
					float progress = 1f - Projectile.timeLeft / 25f;
					Projectile.scale = MathHelper.Lerp(0.8f, 1f, progress);
					Projectile.Opacity = Projectile.timeLeft / 50f;
				}
			}

			if (Projectile.wet)
				Projectile.Kill();
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
						texture.Size(),
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

		}
	}
}