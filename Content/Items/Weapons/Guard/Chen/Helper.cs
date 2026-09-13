using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria;
using System;

namespace ArknightsMod.Content.Items.Weapons.Guard.Chen
{
    public static class Helper
    {
        public static void KZ_QuicklyDraw_Proj(this Projectile proj, float scale = 114514, Color col = default, float? rotation = 0, Vector2 Center = default, Texture2D tx = default, SpriteEffects spE = SpriteEffects.FlipVertically)
        {
            Player player = Main.player[proj.owner];
            Texture2D TX = tx == default ? TextureAssets.Projectile[proj.type].Value : tx;
            Color Col = col == default ? Lighting.GetColor((int)(player.Center.X / 16f), (int)(player.Center.Y / 16f)) : col;


            if (player != null)
            {
                float sc = scale == 114514 ? 1 * player.HeldItem.scale : scale;
                SpriteEffects spe = spE == SpriteEffects.FlipVertically ? player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None : spE;
                Vector2 Cent = Center == default ? proj.Center + new Vector2(0, -4).RotatedBy(proj.rotation) : Center;

                float Dir = spe == SpriteEffects.None ? 1 : -1;

                float Ro = rotation.HasValue ? proj.rotation - MathHelper.PiOver4 * Dir : rotation.Value;

                Main.spriteBatch.Draw(TX,
                                      Cent - Main.screenPosition,
                                      null,
                                      Col,
                                      Ro,
                                      new Vector2((TX.Width / 2 - TX.Width / 2 * Dir), TX.Height),
                                      sc,
                                      spe,
                                      0);
            }
        }

        public static void KZ_QuicklyDraw_Proj(this Projectile proj, Vector2? scale = null, Color col = default, float rotation = -114514f, Vector2 Center = default, Texture2D tx = default, SpriteEffects? spE = null)
        {
            Player player = Main.player[proj.owner];
            Texture2D TX = tx == default ? TextureAssets.Projectile[proj.type].Value : tx;
            Color Col = col == default ? Lighting.GetColor((int)(player.Center.X / 16f), (int)(player.Center.Y / 16f)) : col;
            Vector2 Cent = Center == default ? proj.Center : Center;

            if (player != null)
            {
                Vector2 sc = !scale.HasValue ? new Vector2(0.8f, 0.8f) : scale.Value;
                SpriteEffects spe = !spE.HasValue ? player.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None : spE.Value;
                float Ro = rotation == -114514 ? proj.rotation - MathHelper.PiOver4 * (spe == SpriteEffects.None ? 1 : -1) : rotation;

                Main.spriteBatch.Draw(TX,
                                      Cent - Main.screenPosition,
                                      null,
                                      Col,
                                      Ro,
                                      new Vector2((TX.Width / 2 - TX.Width / 2 * (spe == SpriteEffects.None ? 1 : -1)), TX.Height),
                                      sc * player.HeldItem.scale,
                                      spe,
                                      0);
            }
        }
        public static Vector2 KZ_Ellipse_Position(float a, float b, float rota, float next_time = 0, float change_roat = 0)
        {
            if (rota + next_time + change_roat == 0) return new Vector2(a, 0);
            //�òݸ�ֽ�Ƴ����Ĺ�ʽ
            float y = (float)Math.Pow((a * a) / (1 / (float)Math.Tan(rota + next_time + change_roat)
                / (float)Math.Tan(rota + next_time + change_roat) + a * a / b / b), 0.5);
            //Main.NewText((long)((rota + next_time + change_roat) * 57.3));
            float x = y / (float)Math.Tan(rota + next_time + change_roat);

            if (Math.Sin(rota + next_time + change_roat) > 0)
            {
                return new Vector2(x, y).RotatedBy(-change_roat);
            }
            else
            {
                return -new Vector2(x, y).RotatedBy(-change_roat);
            }
        }
        /// <summary>
        /// ���������npc
        /// </summary>
        /// <param name="center">���ڼ���npc���õ�ľ��룬���ܳ���Dis</param>
        /// <param name="Dis">��Զ����</param>
        /// <param name="FindCenter">��������</param>
        /// <param name="AttackBossFirst"></param>
        /// <returns></returns>
        public static NPC KZ_FindClosestNPC(this Vector2 center, float Dis = -1, Vector2? FindCenter = null, bool AttackBossFirst = true)
        {
            float MinNPCDistent = float.MaxValue;
            NPC npc = null;
            foreach (var np in Main.npc)
            {
                if (np.CanBeChasedBy() && np.active && !np.friendly)
                {
                    if (Dis <= -1)
                    {
                        if (np.boss && AttackBossFirst)
                        {
                            return np;
                        }

                        if (!FindCenter.HasValue)
                        {
                            if (MinNPCDistent > Vector2.Distance(center, np.Center))
                            {
                                npc = np;
                                MinNPCDistent = Vector2.Distance(center, np.Center);
                            }
                        }
                        else
                        {
                            if (MinNPCDistent > Vector2.Distance(FindCenter.Value, np.Center))
                            {
                                npc = np;
                                MinNPCDistent = Vector2.Distance(FindCenter.Value, np.Center);
                            }

                        }
                    }
                    else
                    {
                        if (Vector2.Distance(np.Center, center) < Dis)
                        {
                            if (np.boss && AttackBossFirst)
                            {
                                return np;
                            }
                            if (!FindCenter.HasValue)
                            {
                                if (MinNPCDistent > Vector2.Distance(center, np.Center))
                                {
                                    npc = np;
                                    MinNPCDistent = Vector2.Distance(center, np.Center);
                                }
                            }
                            else
                            {
                                if (MinNPCDistent > Vector2.Distance(FindCenter.Value, np.Center))
                                {
                                    npc = np;
                                    MinNPCDistent = Vector2.Distance(FindCenter.Value, np.Center);
                                }

                            }
                        }
                    }
                }
            }
            return npc;
        }

    }
}