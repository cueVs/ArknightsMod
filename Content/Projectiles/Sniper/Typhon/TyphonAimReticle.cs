using ArknightsMod.Content.Items.Weapons.Sniper.Typhon;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Typhon
{
    public class TyphonAimReticle : ModProjectile
    {
        public const float MouseSearchRadius = 500f;
        public const float ReticleAreaRadius = 120f;
        private const int  FrameCount        = 8;

        public override string Texture => "ArknightsMod/Content/Items/Weapons/Sniper/Typhon/skill3_aim";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = FrameCount;
        }

        public override void SetDefaults()
        {
            Projectile.width        = 128;
            Projectile.height       = 128;
            Projectile.aiStyle      = -1;
            Projectile.tileCollide  = false;
            Projectile.friendly     = false;
            Projectile.hostile      = false;
            Projectile.penetrate    = -1;
            Projectile.timeLeft     = 99999;
            Projectile.netImportant = true;
        }

        public override bool? CanDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            var modPlayer = owner.GetModPlayer<WeaponPlayer>();
            bool s3Active = !owner.dead && owner.active
                && owner.HeldItem.ModItem is TyphonBow
                && modPlayer.Skill == 2 && modPlayer.SkillActive;

            if (!s3Active)
            {
                Projectile.Kill();
                return;
            }

            NPC? target = FindNearestEnemy(Main.MouseWorld, MouseSearchRadius);
            if (target != null)
            {
                Projectile.Center = target.Center;
                Projectile.alpha  = 0;
            }
            else
            {
                Projectile.Center = Main.MouseWorld;
                Projectile.alpha  = 140;
            }

            if (++Projectile.frameCounter >= 5)
            {
                Projectile.frameCounter = 0;
                Projectile.frame = (Projectile.frame + 1) % FrameCount;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex   = TextureAssets.Projectile[Type].Value;
            int frameH      = tex.Height / FrameCount;
            Rectangle src   = new Rectangle(0, frameH * Projectile.frame, tex.Width, frameH);
            Vector2 origin  = new Vector2(tex.Width / 2f, frameH / 2f);
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float alphaMult = 1f - Projectile.alpha / 255f;
            Color color = Color.White * alphaMult;
            Main.spriteBatch.Draw(tex, drawPos, src, color, 0f, origin, 1f, SpriteEffects.None, 0f);
            return false;
        }

        public static Vector2? GetCurrentPos(int playerWho)
        {
            int reticleType = ModContent.ProjectileType<TyphonAimReticle>();
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.owner == playerWho && p.type == reticleType)
                    return p.Center;
            }

            return null;
        }

        public static bool TryGetSnappedChaseNpc(int ownerWho, [NotNullWhen(true)] out NPC? npc)
        {
            npc = null;
            if (ownerWho < 0 || ownerWho >= Main.maxPlayers)
                return false;
            Player plr = Main.player[ownerWho];
            if (!plr.active)
                return false;

            int reticleType = ModContent.ProjectileType<TyphonAimReticle>();
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.owner != ownerWho || p.type != reticleType)
                    continue;

                if (p.alpha > 100)
                    return false;

                Vector2 c = p.Center;
                const float pickSlop = 96f;
                float pickSlopSq = pickSlop * pickSlop;

                NPC? best = null;
                float bestD = pickSlopSq;
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (!n.CanBeChasedBy(plr) || n.friendly || n.life <= 0 || n.dontTakeDamage)
                        continue;
                    if (n.Hitbox.Contains(c.ToPoint()))
                    {
                        npc = n;
                        return true;
                    }
                    float d = Vector2.DistanceSquared(n.Center, c);
                    if (d < bestD)
                    {
                        bestD = d;
                        best = n;
                    }
                }

                npc = best;
                return npc != null;
            }

            return false;
        }

        public static bool TryGetLockedColumnRainAnchor(int ownerWho, out Vector2 anchorWorld)
        {
            anchorWorld = default;
            if (ownerWho < 0 || ownerWho >= Main.maxPlayers || !Main.player[ownerWho].active)
                return false;

            int reticleType = ModContent.ProjectileType<TyphonAimReticle>();
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.owner != ownerWho || p.type != reticleType)
                    continue;

                if (p.alpha > 100)
                    return false;

                anchorWorld = p.Center;
                return true;
            }

            return false;
        }

        private static NPC? FindNearestEnemy(Vector2 origin, float range)
        {
            NPC? best = null;
            float bestDistSq = range * range;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.friendly || npc.life <= 0 || npc.dontTakeDamage) continue;
                float d = Vector2.DistanceSquared(npc.Center, origin);
                if (d < bestDistSq)
                {
                    bestDistSq = d;
                    best = npc;
                }
            }
            return best;
        }
    }
}
