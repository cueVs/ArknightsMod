using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Rogue.Dedication
{
    public class BlackExplosion : ModProjectile
    {
        private bool spawned = false;

        
        private static class Config
        {
            public const float OrbitRadius = 15f;       
            public const float RadiusVariation = 2f;     
        }

        public override void SetStaticDefaults()
        {
        }

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 5;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            if (!spawned)
            {
                SpawnParticles();
                spawned = true;
            }
        }

        private void SpawnParticles()
        {
            Random rand = new Random();
            int particleCount = rand.Next(5, 9);

        
            List<float> fixedAngles = new List<float>();

         
            for (int i = 0; i < 4; i++)
            {
                float angle = 135f + i * 30f;
                fixedAngles.Add(angle * MathHelper.Pi / 180f);
            }

            
            fixedAngles.Add(-315f * MathHelper.Pi / 180f);

          
            foreach (float angle in fixedAngles)
            {
                SpawnParticleWithVariation(angle, rand, isSpecial: true);
            }

    
            int remainingCount = particleCount - fixedAngles.Count;
            if (remainingCount > 0)
            {
                for (int i = 0; i < remainingCount; i++)
                {
                    float angle = (float)(rand.NextDouble() * MathHelper.TwoPi);
                    SpawnParticleWithVariation(angle, rand, isSpecial: false);
                }
            }
        }

        private void SpawnParticleWithVariation(float baseAngle, Random rand, bool isSpecial)
        {
         
            float angleVariation = (float)(rand.NextDouble() - 0.5) * MathHelper.Pi / 18f;
            float finalAngle = baseAngle + angleVariation;

       
            float radius = Config.OrbitRadius + (float)(rand.NextDouble() - 0.5) * Config.RadiusVariation;
         
            Vector2 orbitOffset = new Vector2((float)Math.Cos(finalAngle), (float)Math.Sin(finalAngle)) * radius;
            Vector2 spawnPosition = Projectile.Center + orbitOffset;


            int totalFrames = rand.Next(25, 30);

       
            float startSize = Math.Max(3f, 5f + (float)(rand.NextDouble() - 0.5) * 1f);

      
            float midSize = Math.Max(10f, 15f + (float)(rand.NextDouble() - 0.5) * 2f);

    
            float endSize = Math.Max(12f, 22f + (float)(rand.NextDouble() - 0.5) * 4f);

       
            int peakFrame1 = Math.Max(1, Math.Min(totalFrames - 2, (int)(totalFrames * (0.2f + (float)rand.NextDouble() * 0.1f))));

         
            int valleyFrame = Math.Max(peakFrame1 + 1, Math.Min(totalFrames - 1, (int)(totalFrames * (0.4f + (float)rand.NextDouble() * 0.1f))));

            
            float outwardSpeed = 2f + (float)rand.NextDouble() * 3f;
            if (isSpecial)
            {
                outwardSpeed *= 1.3f;
            }


            int delay = rand.Next(0, 4);

   
            int particleIndex = Projectile.NewProjectile(
                Projectile.GetSource_FromAI(),
                spawnPosition,
                Vector2.Zero,
                ModContent.ProjectileType<BlackExplosionParticle>(),
                0,
                0f,
                Projectile.owner
            );

            Projectile particle = Main.projectile[particleIndex];
            BlackExplosionParticle modParticle = particle.ModProjectile as BlackExplosionParticle;
            if (modParticle != null)
            {
   
                modParticle.Initialize(totalFrames, startSize, midSize, endSize, peakFrame1, valleyFrame, outwardSpeed, finalAngle, Projectile.Center, orbitOffset);
                particle.timeLeft = totalFrames + delay;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}