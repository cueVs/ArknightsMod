using ArknightsMod.Content.NPCs.Enemy.OF.Pmp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Common.NPCDeathDebris
{
    public class NPCDebrisSystem : GlobalNPC
    {
        public override void OnKill(NPC npc)
        {
            if (npc.ModNPC is Pompeii)
                return;

            TrySpawnDynamicDebris(npc, npc.boss, 0);
        }

        /// <summary>
        /// 从 NPC 当前贴图中指定帧行裁切碎块并抛出。单机/客户端可见侧调用（与 OnKill 逻辑一致）。
        /// </summary>
        /// <param name="frameRowIndex">竖条精灵表中第几帧（从 0 计）。庞贝尾杀驻场结束时应传当前动画帧。</param>
        public static void TrySpawnDynamicDebris(NPC npc, bool isBoss, int frameRowIndex)
        {
            if (Main.dedServ)
                return;

            if (npc.ModNPC == null || npc.ModNPC.Mod?.Name != "ArknightsMod")
                return;

            int minPieces = isBoss ? 3 : 1;
            int maxPieces = isBoss ? 5 : 3;
            int pieceCount = Main.rand.Next(minPieces, maxPieces + 1);

            string texturePath = npc.ModNPC.Texture;
            Texture2D texture;
            try
            {
                texture = ModContent.Request<Texture2D>(texturePath, AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                return;
            }

            if (texture == null || texture.IsDisposed)
                return;

            int frameCount = Main.npcFrameCount[npc.type];
            if (frameCount < 1)
                frameCount = 1;

            int frameHeight = frameCount > 1 ? texture.Height / frameCount : texture.Height;
            frameRowIndex = Math.Clamp(frameRowIndex, 0, frameCount - 1);

            int texW = texture.Width;
            int texH = frameHeight;
            Color[] framePixels = new Color[texW * texH];
            try
            {
                texture.GetData(0, new Rectangle(0, frameRowIndex * frameHeight, texW, texH), framePixels, 0, texW * texH);
            }
            catch
            {
                return;
            }

            List<Point> opaquePixels = new List<Point>();
            for (int py = 0; py < texH; py++)
                for (int px = 0; px < texW; px++)
                    if (framePixels[py * texW + px].A > 30)
                        opaquePixels.Add(new Point(px, py));

            if (opaquePixels.Count == 0)
                return;

            for (int i = 0; i < pieceCount; i++)
            {
                int pieceW, pieceH;
                if (pieceCount <= 2)
                {
                    pieceW = Main.rand.Next(Math.Max(4, texW * 3 / 5), texW + 1);
                    pieceH = Main.rand.Next(Math.Max(4, texH * 3 / 5), texH + 1);
                }
                else
                {
                    pieceW = Main.rand.Next(Math.Max(4, texW / 5), Math.Max(6, texW * 7 / 10 + 1));
                    pieceH = Main.rand.Next(Math.Max(4, texH / 5), Math.Max(6, texH * 7 / 10 + 1));
                }

                Point seed = opaquePixels[Main.rand.Next(opaquePixels.Count)];
                int rx = Math.Clamp(seed.X - pieceW / 2, 0, Math.Max(0, texW - pieceW));
                int ry = Math.Clamp(seed.Y - pieceH / 2, 0, Math.Max(0, texH - pieceH));
                pieceW = Math.Min(pieceW, texW - rx);
                pieceH = Math.Min(pieceH, texH - ry);

                if (pieceW <= 0 || pieceH <= 0)
                    continue;

                Color[] piecePixels = new Color[pieceW * pieceH];
                for (int py = 0; py < pieceH; py++)
                    for (int px = 0; px < pieceW; px++)
                        piecePixels[py * pieceW + px] = framePixels[(ry + py) * texW + (rx + px)];

                int opaqueCount = 0;
                foreach (var c in piecePixels)
                    if (c.A > 30) opaqueCount++;
                if (opaqueCount < 4)
                    continue;

                ApplyIrregularMask(piecePixels, pieceW, pieceH);

                Texture2D pieceTexture = new Texture2D(Main.graphics.GraphicsDevice, pieceW, pieceH);
                pieceTexture.SetData(piecePixels);

                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                float speed = Main.rand.NextFloat(2.4f, 6.8f);
                Vector2 vel = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed - Main.rand.NextFloat(1.8f, 3.2f));

                Vector2 spawnPos = npc.Center + Main.rand.NextVector2Circular(npc.width * 0.14f, npc.height * 0.14f);

                int projIndex = Projectile.NewProjectile(
                    npc.GetSource_Death(),
                    spawnPos,
                    vel,
                    ModContent.ProjectileType<NPCDebrisProjectile>(),
                    0, 0f, Main.myPlayer
                );

                if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                {
                    var proj = Main.projectile[projIndex];
                    if (proj.ModProjectile is NPCDebrisProjectile debrisProj)
                    {
                        debrisProj.SetDebrisTexture(pieceTexture);
                        debrisProj.SetGroundRollEligible(Main.rand.NextFloat() < 0.42f);
                    }
                }
            }
        }

        private static void ApplyIrregularMask(Color[] pixels, int w, int h)
        {
            int vertexCount = Main.rand.Next(5, 9);
            Vector2[] vertices = new Vector2[vertexCount];
            float cx = w / 2f;
            float cy = h / 2f;

            for (int v = 0; v < vertexCount; v++)
            {
                float angleV = v * MathHelper.TwoPi / vertexCount + Main.rand.NextFloat(-0.3f, 0.3f);
                float radiusX = cx * Main.rand.NextFloat(0.45f, 0.92f);
                float radiusY = cy * Main.rand.NextFloat(0.45f, 0.92f);
                vertices[v] = new Vector2(
                    cx + radiusX * (float)Math.Cos(angleV),
                    cy + radiusY * (float)Math.Sin(angleV)
                );
            }

            for (int py = 0; py < h; py++)
                for (int px = 0; px < w; px++)
                    if (!PointInPolygon(new Vector2(px + 0.5f, py + 0.5f), vertices))
                        pixels[py * w + px] = Color.Transparent;
        }

        private static bool PointInPolygon(Vector2 point, Vector2[] polygon)
        {
            int n = polygon.Length;
            bool inside = false;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                float xi = polygon[i].X, yi = polygon[i].Y;
                float xj = polygon[j].X, yj = polygon[j].Y;
                if ((yi > point.Y) != (yj > point.Y) &&
                    point.X < (xj - xi) * (point.Y - yi) / (yj - yi) + xi)
                    inside = !inside;
            }
            return inside;
        }
    }
}
