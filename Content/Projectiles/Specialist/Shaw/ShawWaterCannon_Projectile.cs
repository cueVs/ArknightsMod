using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Specialist.Shaw
{
    public class ShawWaterCannon_Projectile : ModProjectile
    {
        Player player => Main.player[Projectile.owner];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.Opacity = 0.9f;
            Projectile.timeLeft = 900;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 4;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            base.AI();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(3, 3);
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Water_Snow);
                Main.dust[dust].noGravity = true;
                Main.dust[dust].velocity = vel;
                Main.dust[dust].scale = 2;
                Main.dust[dust].alpha = 95;
            }
            base.OnHitNPC(target, hit, damageDone);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            var vertices = new List<VertexData>();
            for (int i = 0; i < 10; i += 1)
            {
                if (Projectile.oldPos[i] != Vector2.Zero)
                {
                    float uvX = i / 9f;

                    vertices.Add(new VertexData(
                        Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(6, 0).RotatedBy(Projectile.oldRot[i] - MathHelper.ToRadians(-90)),
                        new Vector3(uvX, 0, 1),
                        lightColor * (1 - i / 10f)
                    ));

                    vertices.Add(new VertexData(
                        Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(6, 0).RotatedBy(Projectile.oldRot[i] - MathHelper.ToRadians(90)),
                        new Vector3(uvX, 1, 1),
                        lightColor * (1 - i / 10f)
                    ));
                }

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicWrap,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Specialist/Shaw/ShawWaterCannon_Projectile_Tail").Value;
                if (vertices.Count >= 5)
                {
                    Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicWrap,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Specialist/Shaw/ShawWaterCannon_Projectile_Subtail").Value;
                if (vertices.Count >= 5)
                {
                    Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
                }
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
            }

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Main.EntitySpriteDraw(
               texture,
               Projectile.Center - Main.screenPosition,
               null,
               Projectile.GetAlpha(lightColor) * Projectile.Opacity,
               0,
               new Vector2(texture.Width / 2, texture.Height / 2),
               Projectile.scale,
               SpriteEffects.None,
               0
               );

            return false;
        }
    }
}
