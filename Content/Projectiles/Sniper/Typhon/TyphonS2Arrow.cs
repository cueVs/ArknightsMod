using ArknightsMod.Content.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Typhon
{
    public class TyphonS2Arrow : ModProjectile
    {
        private const float HomingTurnRate  = 0.15f;
        private const float HomingRange     = 1000f;
        private const int   TrailLength     = 14;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = TrailLength;
            ProjectileID.Sets.TrailingMode[Projectile.type]     = 2;
        }

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.WoodenArrowFriendly);
            Projectile.DamageType  = DamageClass.Ranged;
            Projectile.friendly    = true;
            Projectile.hostile     = false;
            Projectile.arrow       = true;
            Projectile.extraUpdates = 1;
            Projectile.aiStyle     = -1;
            Projectile.tileCollide = true;
            Projectile.timeLeft    = 240;
        }

        public override void AI()
        {
            Projectile.position += Projectile.velocity;
            Projectile.rotation  = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.localAI[0] += 1f;

            NPC target = ResolveTarget();
            if (target == null) return;

            float speed = Projectile.velocity.Length();
            if (speed < 0.001f) return;
            Vector2 desired = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * speed;
            Vector2 newDir = Vector2.Lerp(Projectile.velocity, desired, HomingTurnRate)
                                    .SafeNormalize(Vector2.Zero);
            Projectile.velocity = newDir * speed;
        }

        private NPC? ResolveTarget()
        {
            // ai[0] 中存的是 whoAmI+1；为 0 表示发射时未找到目标
            int idx = (int)Projectile.ai[0] - 1;
            if (idx >= 0 && idx < Main.maxNPCs)
            {
                NPC pinned = Main.npc[idx];
                if (pinned.active && !pinned.friendly && pinned.life > 0 && pinned.CanBeChasedBy(Projectile))
                    return pinned;
            }

            NPC? best = null;
            float bestDistSq = HomingRange * HomingRange;
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (!npc.CanBeChasedBy(Projectile)) continue;
                float d = Vector2.DistanceSquared(npc.Center, Projectile.Center);
                if (d < bestDistSq)
                {
                    bestDistSq = d;
                    best = npc;
                }
            }
            return best;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.DisableCrit();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.owner >= 0 && Projectile.owner < Main.maxPlayers)
                TyphonSkillArrowStun.TryApply(target, Main.player[Projectile.owner], Projectile);

            if (Main.myPlayer != Projectile.owner) return;
            if (Main.rand.NextFloat() < 0.4f)
            {
                target.AddBuff(BuffID.Confused, 60);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            int count = 0;
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (i > 0 && Projectile.oldPos[i] == Vector2.Zero) break;
                count++;
            }
            if (count < 2) return true;

            Texture2D pixel  = TextureAssets.MagicPixel.Value;
            var pixelRect    = new Rectangle(0, 0, 1, 1);
            var origin       = new Vector2(0.5f, 0.5f);

            for (int i = 0; i < count - 1; i++)
            {
                Vector2 a = Projectile.oldPos[i]     + Projectile.Size / 2f - Main.screenPosition;
                Vector2 b = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;

                Vector2 delta = b - a;
                float length  = delta.Length();
                if (length < 0.5f) continue;
                float rot = delta.ToRotation();

                float t         = (float)i / (count - 1);
                float head      = 1f - t;
                float alpha     = head * head;
                float thickness = MathHelper.Lerp(8f, 2f, t);

                Main.spriteBatch.Draw(
                    pixel, a, pixelRect,
                    new Color(150, 80, 230) * (alpha * 0.55f),
                    rot, origin,
                    new Vector2(length, thickness * 1.6f),
                    SpriteEffects.None, 0f);

                Main.spriteBatch.Draw(
                    pixel, a, pixelRect,
                    new Color(230, 180, 255) * (alpha * 0.9f),
                    rot, origin,
                    new Vector2(length, thickness * 0.5f),
                    SpriteEffects.None, 0f);
            }

            return true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            Color tint = Color.Lerp(lightColor, Color.MediumPurple, 0.45f);
            tint.A = lightColor.A;
            return tint;
        }
    }
}
