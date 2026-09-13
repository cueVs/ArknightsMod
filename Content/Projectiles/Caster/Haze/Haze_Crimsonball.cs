using ArknightsMod.Content.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Haze
{
	public class Haze_Crimsonball : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Projectiles/Caster/Haze/Haze_Magicball";

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

			Lighting.AddLight(Projectile.Center, 0.37f, 0.42f, 0.7f);

			if (Main.rand.NextBool(8)) {
				Dust d = Dust.NewDustDirect(Projectile.position, 0, 0, ModContent.DustType<Haze_CircleDust>());
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {

		}

		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 8; i++) {
				float angle = i * MathHelper.PiOver4;
				Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)).RotatedByRandom(MathHelper.PiOver4 / 2) * Main.rand.NextFloat(0f, 12f);

				Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<Haze_CircleDust>());
				dust.velocity = velocity;
			}

			Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextFloat(0f, MathHelper.Pi).ToRotationVector2().SafeNormalize(Vector2.One), ModContent.ProjectileType<Haze_Effect>(), 0, 0, Projectile.owner);

			base.OnKill(timeLeft);
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D texture = TextureAssets.Projectile[Type].Value;

			for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type]; i++) {
				float factor = 1 - (float)i / ProjectileID.Sets.TrailCacheLength[Type];

				Main.EntitySpriteDraw(texture,
					Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition,
					null,
					Color.Lerp(Color.White, Color.Black, (float)i / (ProjectileID.Sets.TrailCacheLength[Type] - 1) * 2),
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
				Color.White,
				Projectile.rotation,
				texture.Size() / 2,
				new Vector2(1),
				SpriteEffects.None,
				0
				);
			
			for (int i = 0; i < (ProjectileID.Sets.TrailCacheLength[Type] / 2); i++) {
				float factor = 1 - (float)i / (ProjectileID.Sets.TrailCacheLength[Type] / 2);
				float t = (float)i / ((ProjectileID.Sets.TrailCacheLength[Type] / 2) - 1);
				Color color;

				if (t <= 0.3f) {
					color = Color.White;
				}
				else if (t <= 0.5f) {
					float fac = (t - 0.3f) / 0.2f;
					color = Color.Lerp(Color.White, Color.DeepSkyBlue, fac);
				}
				else {
					color = Color.DeepSkyBlue;
				}

				Main.EntitySpriteDraw(texture,
					Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition,
					null,
					color,
					Projectile.oldRot[i],
					texture.Size() / 2,
					new Vector2(1) * factor,
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
				new Vector2(1f),
				SpriteEffects.None,
				0
				);

			var vertices = new List<VertexData>();

			for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Type] / 2; i++) {
				if (Projectile.oldPos[i] != Vector2.Zero) {
					float uvX = (float)i / (ProjectileID.Sets.TrailCacheLength[Type] / 2 - 1);
					float dynamicUvX = uvX - Main.GlobalTimeWrappedHourly * 2f;

					float alpha;
					if (i <= (ProjectileID.Sets.TrailCacheLength[Type] / 2 - 1) / 2) {
						alpha = (float)i / ((ProjectileID.Sets.TrailCacheLength[Type] / 2 - 1) / 2);
					}
					else {
						alpha = 1 - (float)(i - (ProjectileID.Sets.TrailCacheLength[Type] / 2 - 1) / 2) / ((ProjectileID.Sets.TrailCacheLength[Type] / 2 - 1) - (ProjectileID.Sets.TrailCacheLength[Type] / 2 - 1) / 2);
					}

					Color color = Color.Red * alpha;

					vertices.Add(new VertexData(
						Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(16, -16).RotatedBy(Projectile.oldRot[i]), new Vector3(dynamicUvX, 0, 1), color
					));
					vertices.Add(new VertexData(
						Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(-16, -16).RotatedBy(Projectile.oldRot[i]), new Vector3(dynamicUvX, 1, 1), color
					));
				}
			}

			for (int i = 0; i < 2; i++) {
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointWrap,
					DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
				Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Caster/Haze/Haze_Flow").Value;
				if (vertices.Count >= 5) {
					Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
				}
				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
					DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			}

			return false;
		}
	}
}
