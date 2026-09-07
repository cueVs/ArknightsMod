using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using ArknightsMod.Content.Projectiles.Rogue.Dedication;
using System;

namespace ArknightsMod.Content.Projectiles.Rogue.Dedication.ShootProj
{
    public class DedicationTrackingProj : ModProjectile
    {
        private bool exploded = false;
        private NPC targetNPC = null;
        private float trackingRange = 200f; 

        public override void SetStaticDefaults()
        {
          
            Main.projFrames[Projectile.type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 1; 
            Projectile.height = 1;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false; 
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 120; 
            Projectile.penetrate = 1;
            Projectile.aiStyle = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.hide = true; 
        }

        public override void AI()
        {
           
            if (targetNPC == null || !targetNPC.active || targetNPC.life <= 0)
            {
                FindNearestTarget();
            }
            
        
            if (targetNPC != null && targetNPC.active && targetNPC.life > 0)
            {
                float distanceToTarget = Vector2.Distance(Projectile.Center, targetNPC.Center);
                
                if (distanceToTarget <= trackingRange)
                {
                    
                    Projectile.Center = targetNPC.Center;
                }
                else
                {
                    
                    Vector2 direction = targetNPC.Center - Projectile.Center;
                    direction.Normalize();
                    Projectile.velocity = direction * 20f; 
                }
            }
            
       
            if (targetNPC == null || !targetNPC.active)
            {
                FindNearestTarget();
            }
            
           
            Projectile.timeLeft = Math.Min(Projectile.timeLeft, 120);
        }

        private void FindNearestTarget()
        {
            float closestDistance = trackingRange;
            NPC closestNPC = null;
            
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5)
                {
                    float distance = Vector2.Distance(Projectile.Center, npc.Center);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = npc;
                    }
                }
            }
            
            targetNPC = closestNPC;
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.owner != Main.myPlayer)
                return;

            if (!exploded)
            {
                exploded = true;
                SpawnAllEffects();
            }
        }

        private void SpawnAllEffects()
        {
            Vector2 center = Projectile.Center;
            int owner = Projectile.owner;
            var source = Projectile.GetSource_FromThis();

         
            Projectile.NewProjectile(source, center, Vector2.Zero,
                ModContent.ProjectileType<ProjLightCircle>(), 0, 0f, owner);

            Projectile.NewProjectile(source, center, Vector2.Zero,
                ModContent.ProjectileType<ProjLightCore>(), 0, 0f, owner);

            Projectile.NewProjectile(source, center, Vector2.Zero,
                ModContent.ProjectileType<Light_horizontal>(), 0, 0f, owner);

            Projectile.NewProjectile(source, center, Vector2.Zero,
                ModContent.ProjectileType<Light_Vertical>(), 0, 0f, owner);

            Projectile.NewProjectile(source, center, Vector2.Zero,
                ModContent.ProjectileType<BlackExplosion>(), 0, 0f, owner);

           
            int particleCount = Main.rand.Next(10, 19);
            for (int i = 0; i < particleCount; i++)
            {
                
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
               
                float speed = Main.rand.NextFloat(2f, 9f);
                Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;

                float startSize = Main.rand.NextFloat(12f, 24f);
                
                float endSize = Main.rand.NextFloat(2f, 6f);
              
                int lifetime = Main.rand.Next(60, 101);

            
                int particleProj = Projectile.NewProjectile(source, center, Vector2.Zero,
                    ModContent.ProjectileType<ParticleExplosionParticle>(), 0, 0f, owner);

             
                var modParticle = Main.projectile[particleProj].ModProjectile as ParticleExplosionParticle;
                if (modParticle != null)
                {
                    modParticle.Initialize(lifetime, startSize, endSize, velocity, center);
                }
            }
        }

        
        public override bool PreDraw(ref Color lightColor)
        {
            return false;
        }
    }
}
