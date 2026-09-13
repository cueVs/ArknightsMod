using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ArknightsMod.Content.Projectiles.Guard.Saki
{
    public class SakiSwordBlack : ModProjectile
    {
        //起始位置
        private Vector2 startVector;
        private Vector2 vector;
        public float Timer;

        //挥舞速度
        private float speed;

        //挥舞速度修正，和玩家近战速度挂钩
        private float SwingSpeed;

        public override void SetStaticDefaults()
        {

        }

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.penetrate = -1;
            Projectile.alpha = 0;
            Length = 50f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Rot = MathHelper.ToRadians(3f);
        }

        public ref float Length
        {
            get
            {
                return ref Projectile.localAI[0];
            }
        }

        public ref float Rot
        {
            get
            {
                return ref Projectile.localAI[1];
            }
        }
        public int hasPlused;
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Player player = Main.player[Projectile.owner];

            //使击退始终远离玩家
            float hitDir = target.position.X - player.position.X;
            if (hitDir < 0) { modifiers.HitDirectionOverride = -1; } else { modifiers.HitDirectionOverride = 1; }

			Projectile.NewProjectile
				(new EntitySource_Parent(Projectile), target.Center, Vector2.Zero,
				ProjectileType<SakiSlashHit>(), 0, 0, Projectile.owner);
		}
        public float SetSwingSpeed(float speed)
        {
            Player player = Main.player[Projectile.owner];
            return speed / player.GetAttackSpeed(DamageClass.Melee);
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (player.noItems || player.CCed || player.dead || !player.active)
            {
                Projectile.Kill();
            }

            //基础属性设置
            SwingSpeed = 1f;
            player.heldProj = Projectile.whoAmI;
            vector = startVector.RotatedBy(Rot) * Length;
            Projectile.Center = player.Center + vector;
            player.ChangeDir(Projectile.velocity.X > 0 ? 1 : -1);

            //设置剑贴图旋转
            if (Projectile.spriteDirection == 1)
            {
                Projectile.rotation = (Projectile.Center - player.Center).ToRotation() + MathHelper.PiOver4;
            }
            else
            {
                Projectile.rotation = (Projectile.Center - player.Center).ToRotation() - MathHelper.Pi - MathHelper.PiOver4;
            }

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, (player.Center - Projectile.Center).ToRotation() + 1.5707964f);

            if (Timer == 0f)
            {
                float rot2 = Math.Abs((Main.MouseWorld - player.Center).ToRotation());
                double scale1 = Math.Sin(rot2) + 1f;
                SoundEngine.PlaySound(SoundID.Item71, new Vector2?(Projectile.Center));
                startVector = PolarVector(1f, Projectile.velocity.ToRotation() + 2.5f * Projectile.spriteDirection * Projectile.ai[0]);
                speed = MathHelper.ToRadians(30f);
            }

            Timer++;
            if (Timer < 9f * SwingSpeed)
            {
                Rot -= speed / SwingSpeed * Projectile.spriteDirection * Projectile.ai[0];
            }
            else
            {
                Rot -= speed / SwingSpeed * Projectile.spriteDirection * Projectile.ai[0];
                speed *= 0.4f;
            }
            
            if (Math.Abs(speed) <= 0.001f)
            {
                Projectile.alpha += 15;
                if (Projectile.alpha > 150)
                {
                    Projectile.Kill();
                }
                Projectile.netUpdate = true;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            SpriteEffects spriteEffects = (Projectile.spriteDirection == -1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 v = PolarVector(4f, (Projectile.Center - player.Center).ToRotation());
            Vector2 origin = new Vector2(texture.Width / 2f, texture.Height / 2f);
            Main.spriteBatch.Draw(texture, Projectile.Center - v - Main.screenPosition + Vector2.UnitY * Projectile.gfxOffY, texture.Frame(), Projectile.GetAlpha(lightColor) * 2,
                Projectile.rotation, origin, Projectile.scale, spriteEffects, 0);

			//DrawSlash();
			return false;
        }
        public override bool ShouldUpdatePosition()
        {
            return false;
        }
		public static Vector2 PolarVector(float radius, float theta) {
			return new Vector2((float)Math.Cos((double)theta), (float)Math.Sin((double)theta)) * radius;
		}
	}
}
