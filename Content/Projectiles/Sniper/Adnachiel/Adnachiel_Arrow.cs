using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace ArknightsMod.Content.Projectiles.Sniper.Adnachiel
{
	public class Adnachiel_Arrow : ModProjectile
	{
		Player player => Main.player[Projectile.owner];
		private float Skill => (int)Projectile.ai[2];
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 15;
		}

		public override void SetDefaults() {
			Projectile.width = Projectile.height = 10;
			Projectile.timeLeft = 1200;
			Projectile.aiStyle = -1;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.arrow = true;
			Projectile.friendly = true;
		}

		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi / 2;
			Projectile.ai[0] += 1f;
			if (Projectile.ai[0] >= 15f) {
				Projectile.ai[0] = 15f;
				Projectile.velocity.Y += 0.1f;
			}

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			if (Projectile.velocity.Y > 16f) {
				Projectile.velocity.Y = 16f;
			}
		}
		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

			float[] angles = { 0f, 60f, 120f, 180f, 240f ,300f};
			for (int i = 0; i < angles.Length; i++) {
				float angle = angles[i] + Main.rand.NextFloat(-30f, 30f);
				float speed = Main.rand.NextFloat(2f, 4f);

				Vector2 vel = Vector2.UnitX.RotatedBy(MathHelper.ToRadians(angle)) * speed;

				Projectile.NewProjectileDirect(
					Projectile.GetSource_FromAI(),
					Projectile.Center,
					vel,
					ModContent.ProjectileType<Adnachiel_Arrow_Effect>(),
					0,
					0,
					player.whoAmI,
					0,
					0,
					Skill
				);
			}

			Projectile.NewProjectileDirect(
				Projectile.GetSource_FromAI(),
				Projectile.Center,
				Vector2.Zero,
				ModContent.ProjectileType<Adnachiel_Arrow_Circle>(),
				0,
				0,
				player.whoAmI,
				0,
				0,
				Skill
			);
		}
		public override bool PreDraw(ref Color lightColor) {
			Projectile.ai[1] += 0.2f;
			if (Skill == 1) {
				return true;
			}
			else if (Skill == 2) {
				return false;
			}
			else {
				var vertices = new List<VertexData>();
				for (int i = 0; i < 10; i += 1) {
					if (Projectile.oldPos[i] != Vector2.Zero) {
						float uvX = i / 9f;

						vertices.Add(new VertexData(
							Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(16, 0).RotatedBy(Projectile.oldRot[i]),
							new Vector3(uvX, 0, 1),
							Color.White * (0.5f - i / 20f)
						));

						vertices.Add(new VertexData(
							Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(16, 0).RotatedBy(Projectile.oldRot[i] - MathHelper.Pi),
							new Vector3(uvX, 1, 1),
							Color.White * (0.5f - i / 20f)
						));
					}

					Main.spriteBatch.End();
					Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicWrap,
						DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
					Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.Extra[196].Value;
					if (vertices.Count >= 5) {
						Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
					}
					Main.spriteBatch.End();
					Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
						DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				}
				return true;
			}
		}
	}
}