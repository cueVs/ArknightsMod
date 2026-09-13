using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using ArknightsMod.Content.Items.Weapons.Defender.NoirCorne;

namespace ArknightsMod.Content.Projectiles.Defender.NoirCorne
{
    public class NoirShield_Projectile : ModProjectile
    {
        Player player => Main.player[Projectile.owner];
        Item item => player.HeldItem;

        int disableAttack = 0;

        public override void SetDefaults()
        {
            Projectile.hide = true;
            Projectile.damage = 0;
            Projectile.width = 26;
            Projectile.height = 34;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
        }

        public override bool ShouldUpdatePosition() => false;

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles,
            List<int> behindNPCs, List<int> behindProjectiles,
            List<int> overPlayers, List<int> overWiresUI)
        {
            overPlayers.Add(index);
            base.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);

        }

        public override void AI()
        {
            if (!player.active || player.dead || item.type != ModContent.ItemType<NoirShield>())
            {
                Projectile.Kill();
                return;
            }
            if (disableAttack>0) disableAttack--;

            if (Projectile.ai[1] <= 0)
            {
                float t = Math.Clamp(Projectile.ai[0] / 40f, 0, 1);

                Projectile.rotation = Projectile.rotation.AngleLerp(0.2f * player.direction, t);
                Projectile.Center = Vector2.Lerp(
                    Projectile.Center,
                    player.MountedCenter + new Vector2(-5 * player.direction, 6 + player.gfxOffY),
                    t
                );

                Projectile.ai[0]++;

                if ((player.controlUseTile || player.controlUseItem )&& disableAttack==0)
                {
                    player.direction = Main.MouseWorld.X > player.Center.X ? 1 : -1;
                    Projectile.ai[0] = 0;
                    Projectile.ai[1] += 0.07f;
                }   

                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Quarter, 0);
            }
            else
            {
                player.itemAnimation = player.itemTime = 4;

                // 如果还在按右键，推盾动画继续增强
                if (player.controlUseTile && disableAttack == 0 && Projectile.ai[1] < 0.5f)
                {
                    Projectile.ai[1] += 0.07f;
                }
                else if (player.controlUseItem && disableAttack == 0 && Projectile.ai[1] < 0.5f)
                {
                    Projectile.ai[1] += 0.07f;

                    if (Projectile.ai[1] > 0.5f)
                    {
                        doNoirShieldDamage();
                        disableAttack = 30;
                    }
                }
                else
                {
                    Projectile.ai[1] -= 0.07f;
                }

                // 正前方的位移
                Projectile.rotation = Projectile.rotation.AngleLerp(0, Projectile.ai[1]);
                Projectile.Center = Vector2.Lerp(
                    Projectile.Center,
                    player.MountedCenter + new Vector2(32 * player.direction, player.gfxOffY),
                    Projectile.ai[1]
                );

                // 完全伸手
                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -MathHelper.PiOver2 * player.direction);
            }

            // 重置 timeLeft 让盾存在
            Projectile.timeLeft = 2;
        }


        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            Main.spriteBatch.Draw(
                tex,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                tex.Size() / 2,
                0.9f,
                player.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
                0
            );

            return false;
        }

        public bool doNoirShieldDamage()
        {
            
            int damage = item.damage;
            float knockback = item.knockBack;
            int owner = Projectile.owner;
            // bool doDamage = False;
            // 用一个前方矩形区域当作碰撞判定
            Rectangle hitbox = new Rectangle(
                (int)(Projectile.Center.X - 20 ),
                (int)(Projectile.Center.Y - 34),
                40,
                34
            );

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];

                if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                {
                    if (hitbox.Intersects(npc.Hitbox))
                    {
                        NPC.HitInfo info = new();
                        bool crit = Main.rand.Next(100) < item.crit;
                        info.Damage = (int)(item.damage * (crit ? 2f : 1f) * Main.rand.NextFloat(0.95f, 1.051f));
                        info.Knockback = item.knockBack;
                        info.Crit = crit;
                        info.DamageType = item.DamageType;
                        npc.StrikeNPC(info);
                        return true;
                    }
                }
            }
            return false;
        }

    }
}