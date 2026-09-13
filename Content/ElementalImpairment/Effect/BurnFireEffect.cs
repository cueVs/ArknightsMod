using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.ElementalImpairment.Effect
{
    public static class BurnFireEffect
    {

        public static void Play(NPC npc)
        {
            if (npc == null || !npc.active) return;

            Projectile.NewProjectile(
                npc.GetSource_FromAI(),
                npc.Center,
                Vector2.Zero,
                ModContent.ProjectileType<BurnFireSpawner>(),
                0, 0, Main.myPlayer,
                ai0: npc.whoAmI
            );
        }
    }

    public class BurnFireSpawner : ModProjectile
    {
        public override string Texture => "ArknightsMod/Content/ElementalImpairment/Effect/FireParticle";
        private int timer;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 600;         
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            int who = (int)Projectile.ai[0];
            if (who < 0 || who >= Main.npc.Length) { Projectile.Kill(); return; }
            NPC npc = Main.npc[who];
            if (npc == null || !npc.active) { Projectile.Kill(); return; }

            Projectile.Center = npc.Center;

            // 计算 NPC 尺寸缩放因子（基准 16px）
            float npcSize = Math.Max(npc.width, npc.height);
            float sizeScale = npcSize / 16f;
            if (sizeScale < 0.5f) sizeScale = 0.5f;
            if (sizeScale > 3f) sizeScale = 3f;   

           
            timer++;
            if (timer % 3 == 0)
            {
                int baseCount = Main.rand.Next(1, 3);       
                int count = (int)(baseCount * sizeScale);
                if (count < 1) count = 1;
                if (count > 6) count = 6;                  

                for (int i = 0; i < count; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float npcRadius = Math.Max(npc.width, npc.height) * 0.5f;
                    float dist = Main.rand.NextFloat(0f, npcRadius * 0.75f ); 
                    Vector2 spawnPos = npc.Center + angle.ToRotationVector2() * dist;
                    Vector2 velocity = new Vector2(
                        Main.rand.NextFloat(-0.5f, 0.5f),
                        -Main.rand.NextFloat(1f, 2.5f)
                    );

                    
                    int projIndex = Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        spawnPos,
                        velocity,
                        ModContent.ProjectileType<BurnFireParticle>(),
                        0, 0, Main.myPlayer
                    );

                    if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                    {
               
                        float baseScale = 0.1f;
                        float randFactor = Main.rand.NextFloat(0.7f, 1.3f);
                        float finalScale = baseScale * randFactor * sizeScale;
                        Main.projectile[projIndex].scale = finalScale;
                    }
                }
            }
        }
    }

    public class BurnFireParticle : ModProjectile
    {
        public override string Texture => "ArknightsMod/Content/ElementalImpairment/Effect/FireParticle";

		private float rotationSpeed;
        private float initialScale;
        private float maxLife;

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Main.rand.Next(30, 50);
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.alpha = 0;
        }

        public override void AI()
        {
            if (rotationSpeed == 0f)
            {
                rotationSpeed = Main.rand.NextFloat(-0.3f, 0.3f);  
                initialScale = Projectile.scale;
                maxLife = Projectile.timeLeft;
            }

            Projectile.velocity *= 0.98f;
            Projectile.velocity.Y -= 0.03f;
            Projectile.rotation += rotationSpeed;

            float progress = 1f - (float)Projectile.timeLeft / maxLife;
            Projectile.scale = initialScale * (1f - progress * 0.7f);
            Projectile.alpha = (int)(progress * 255);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Color drawColor = new Color(255, 68, 0, 255) * (1f - Projectile.alpha / 255f);

            
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                drawColor,
                Projectile.rotation,
                texture.Size() / 2f,
                Projectile.scale,
                SpriteEffects.None,
                0f
            );

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
                DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}