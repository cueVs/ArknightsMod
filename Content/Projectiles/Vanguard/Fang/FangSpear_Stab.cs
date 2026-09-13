using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace ArknightsMod.Content.Projectiles.Vanguard.Fang
{
    public class FangSpear_Stab : ModProjectile
    {
        protected virtual float HoldoutRangeMin => 48f;
        protected virtual float HoldoutRangeMax => 96f;

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.Spear);
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];
            int duration = player.itemAnimationMax;

            player.heldProj = Projectile.whoAmI;

            if (Projectile.timeLeft > duration)
            {
                Projectile.timeLeft = duration;
            }

            Projectile.velocity = Vector2.Normalize(Projectile.velocity);

            float returnDuration = duration * 0.8f;
            float progress;

            if (Projectile.timeLeft < returnDuration)
            {
                progress = Projectile.timeLeft / returnDuration;
            }
            else
            {
                progress = 1 - (Projectile.timeLeft - returnDuration) / (duration - returnDuration);
            }

            Projectile.Center = player.MountedCenter + Vector2.SmoothStep(Projectile.velocity * HoldoutRangeMin, Projectile.velocity * HoldoutRangeMax, progress);

            if (Projectile.spriteDirection == -1)
            {
                Projectile.rotation += MathHelper.ToRadians(45f);
            }
            else
            {
                Projectile.rotation += MathHelper.ToRadians(135f);
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for ( int i = 0; i < Projectile.damage / 2; i++ )
            {
                int dust = Dust.NewDust(Projectile.Center, 0, 0, DustID.SilverCoin);
                Main.dust[dust].noGravity = true;
            }
            base.OnHitNPC(target, hit, damageDone);
        }

    }
}
