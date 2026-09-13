using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.KroosAlter
{
    public class KroosAlterCrossbow_Arrow : ModProjectile
    {
        Player player => Main.player[Projectile.owner];
        private float Skill => (int)Projectile.ai[2];
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.timeLeft = 1200;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.arrow = true;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] >= 15f)
            {
                Projectile.ai[0] = 15f;
                Projectile.velocity.Y += 0.2f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            if (Projectile.velocity.Y > 16f)
            {
                Projectile.velocity.Y = 16f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);

            float[] angles = { 0f, 72f, 144f, 216f, 288f };
            for (int i = 0; i < angles.Length; i++)
            {
                float angle = angles[i] + Main.rand.NextFloat(-24f, 24f);
                float speed = Main.rand.NextFloat(2f, 4f);

                Vector2 vel = Vector2.UnitX.RotatedBy(MathHelper.ToRadians(angle)) * speed;

                Projectile.NewProjectileDirect(
                    Projectile.GetSource_FromAI(),
                    Projectile.Center,
                    vel,
                    ModContent.ProjectileType<KroosAlterCrossbow_Effect>(),
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
                ModContent.ProjectileType<KroostheKeenGlint_Crossbow_Circle2>(),
                0,
                0,
                player.whoAmI,
                0,
                0,
                Skill
            );
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() <= 0.4f)
            {
                target.AddBuff(31, 6);
                base.OnHitNPC(target, hit, damageDone);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			Projectile.ai[1] += 0.2f;
            #region 一技能
            if (modPlayer.Skill == 0 && modPlayer.SkillActive)
            {
                if (Main.rand.NextBool(2))
                {
                    var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
                    d.noGravity = true;
                }

                var vertices = new List<VertexData>();
                for (int i = 0; i < 15; i += 1)
                {
                    if (Projectile.oldPos[i] != Vector2.Zero)
                    {
                        float uvX = i / 14f;

                        vertices.Add(new VertexData(
                            Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(16, 0).RotatedBy(Projectile.oldRot[i]),
                            new Vector3(uvX, 0, 1),
                            Color.DarkOrange * (1f - i / 15f)
                        ));

                        vertices.Add(new VertexData(
                            Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(16, 0).RotatedBy(Projectile.oldRot[i] - MathHelper.Pi),
                            new Vector3(uvX, 1, 1),
                            Color.DarkOrange * (1f - i / 15f)
                        ));
                    }
                }
                for (int i = 0; i < 2; i += 1)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicWrap,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.Extra[196].Value;
                    if (vertices.Count >= 5)
                    {
                        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
                    }
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                return true;
            }
            #endregion
            #region 二技能
            else if (modPlayer.Skill == 1 && modPlayer.SkillActive)
            {
                var d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.YellowTorch);
                d.noGravity = true;
                if (Main.rand.NextBool(4))
                {
                    var d2 = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.CoralTorch);
                    d2.scale = 1.5f;
                    d2.noGravity = true;
                }

                var vertices1 = new List<VertexData>();

                float timeOffset = Main.GlobalTimeWrappedHourly % 1f;

                for (int i = 0; i < 15; i += 1)
                {
                    if (Projectile.oldPos[i] != Vector2.Zero)
                    {
                        float uvX = i / 14f;
                        float progress = i / 4f;

                        float dynamicUvX = (uvX - timeOffset * 2) % 1f;

                        var color = Color.Lerp(new Color(255, 237, 0), new Color(9, 161, 130), progress);

                        // 透明度渐变
                        float alphaFactor = 1f;
                        if (i < 3)
                        {
                            alphaFactor = i / 2f;
                        }
                        else
                        {
                            alphaFactor = 1 - i / 15f;
                        }
                        color *= alphaFactor;

                        vertices1.Add(new VertexData(
                            Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(32, -0).RotatedBy(Projectile.oldRot[i]),
                            new Vector3(dynamicUvX, 0, 1),
                            color
                        ));

                        vertices1.Add(new VertexData(
                            Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(32, 0).RotatedBy(Projectile.oldRot[i] - MathHelper.Pi),
                            new Vector3(dynamicUvX, 1, 1),
                            color
                        ));
                    }
                }

                for (int i = 0; i < 2; i++)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicWrap,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.Extra[196].Value;
                    if (vertices1.Count >= 5)
                    {
                        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices1.ToArray(), 0, vertices1.Count - 2);
                    }
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }

                Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
                Main.EntitySpriteDraw(
                    tex,
                    Projectile.Center - Main.screenPosition,
                    null,
                    Color.White,
                    Projectile.rotation,
                    new Vector2(tex.Width / 2, 0),
                    Projectile.scale,
                    SpriteEffects.None,
                    0
                );
                Texture2D glow = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Sniper/KroosAlter/KroosAlterCrossbow_ArrowGlow").Value;
                Main.EntitySpriteDraw(
                    glow,
                    Projectile.Center - Main.screenPosition,
                    null,
                    Color.White,
                    Projectile.rotation,
                    new Vector2(glow.Width / 2, 0),
                    Projectile.scale,
                    SpriteEffects.None,
                    0
                    );

                return false;
            }
            #endregion
            #region 普攻
            else
            {
                var vertices = new List<VertexData>();
                for (int i = 0; i < 10; i += 1)
                {
                    if (Projectile.oldPos[i] != Vector2.Zero)
                    {
                        float uvX = i / 9f;

                        vertices.Add(new VertexData(
                            Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(16, 0).RotatedBy(Projectile.oldRot[i]),
                            new Vector3(uvX, 0, 1),
                            Color.DarkOrange * (0.5f - i / 20f)
                        ));

                        vertices.Add(new VertexData(
                            Projectile.oldPos[i] + Projectile.Size / 2 - Main.screenPosition + new Vector2(16, 0).RotatedBy(Projectile.oldRot[i] - MathHelper.Pi),
                            new Vector3(uvX, 1, 1),
                            Color.DarkOrange * (0.5f - i / 20f)
                        ));
                    }

                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicWrap,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                    Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.Extra[196].Value;
                    if (vertices.Count >= 5)
                    {
                        Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices.ToArray(), 0, vertices.Count - 2);
                    }
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
                        DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                }
                return true;
            }
            #endregion
        }
    }
}
