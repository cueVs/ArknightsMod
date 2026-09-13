using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs
{
    public static class DeathDebrisSystem
    {
        public static void SpawnDebris(NPC npc, bool isBoss = false)
        {
            if (Main.netMode == Terraria.ID.NetmodeID.Server)
                return;

            string texturePath = npc.ModNPC?.Texture;
            if (string.IsNullOrEmpty(texturePath))
                return;

            Texture2D tex;
            try
            {
                tex = ModContent.Request<Texture2D>(texturePath, AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                return;
            }

            int frameCount = Math.Max(1, Main.npcFrameCount[npc.type]);
            int frameH = tex.Height / frameCount;
            Rectangle frameRect = new Rectangle(0, 0, tex.Width, frameH);

            Color[] framePixels = new Color[tex.Width * frameH];
            try
            {
                tex.GetData(0, frameRect, framePixels, 0, framePixels.Length);
            }
            catch
            {
                return;
            }

            List<Point> opaquePixels = BuildOpaqueList(framePixels, tex.Width, frameH);
            if (opaquePixels.Count < 16)
                return;

            int minCount = isBoss ? 3 : 1;
            int maxCount = isBoss ? 5 : 3;
            int count = Main.rand.Next(minCount, maxCount + 1);

            for (int i = 0; i < count; i++)
            {
                // 碎块总数 <= 2 时强制大块，>= 3 时允许小碎块
                bool largePiece = count <= 2;
                DebrisInfo info = GenerateDebris(framePixels, opaquePixels, tex.Width, frameH, isBoss, largePiece);
                if (info == null)
                    continue;

                Vector2 spawnPos = npc.Center + Main.rand.NextVector2Circular(
                    npc.width * 0.22f, npc.height * 0.22f);

                float burstAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                float burstSpeed = Main.rand.NextFloat(2.2f, 6.2f);
                Vector2 vel = new Vector2((float)Math.Cos(burstAngle) * burstSpeed, (float)Math.Sin(burstAngle) * burstSpeed - Main.rand.NextFloat(1.6f, 3.4f));

                int projIndex = Projectile.NewProjectile(
                    npc.GetSource_Death(),
                    spawnPos, vel,
                    ModContent.ProjectileType<DeathDebrisProjectile>(),
                    0, 0f, Main.myPlayer);

                if (projIndex >= 0 && projIndex < Main.maxProjectiles)
                {
                    Projectile p = Main.projectile[projIndex];
                    if (p.ModProjectile is DeathDebrisProjectile debrisProj)
                        debrisProj.InitDebris(texturePath, info, frameH);
                }
            }
        }

        private static List<Point> BuildOpaqueList(Color[] pixels, int width, int height)
        {
            var list = new List<Point>(pixels.Length / 4);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (pixels[y * width + x].A > 30)
                        list.Add(new Point(x, y));
            return list;
        }

        private static DebrisInfo GenerateDebris(
            Color[] pixels, List<Point> opaquePixels,
            int texW, int texH, bool isBoss, bool largePiece = false)
        {
            int minHalf, maxHalf;
            if (largePiece)
            {
                // 少量碎块：强制大块，取 60%~100% 贴图范围
                minHalf = Math.Max(5, Math.Min(texW, texH) * 3 / 10);
                maxHalf = Math.Max(minHalf + 1, Math.Min(texW, texH) / 2);
            }
            else
            {
                // 多块模式：允许小碎块
                minHalf = isBoss ? 8 : 5;
                maxHalf = isBoss
                    ? Math.Max(minHalf + 1, Math.Min(texW, texH) / 2)
                    : Math.Max(minHalf + 1, Math.Min(texW, texH) / 3);
            }

            const int maxAttempts = 24;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Point center = opaquePixels[Main.rand.Next(opaquePixels.Count)];

                int halfW = Main.rand.Next(minHalf, maxHalf + 1);
                int halfH = Main.rand.Next(minHalf, maxHalf + 1);

                int x0 = Math.Max(0, center.X - halfW);
                int y0 = Math.Max(0, center.Y - halfH);
                int x1 = Math.Min(texW - 1, center.X + halfW);
                int y1 = Math.Min(texH - 1, center.Y + halfH);

                int w = x1 - x0 + 1;
                int h = y1 - y0 + 1;
                if (w < 5 || h < 5)
                    continue;

                Color[] debrisPixels = CutIrregularShape(pixels, texW, texH, x0, y0, w, h, center);

                int opaqueCount = 0;
                foreach (Color c in debrisPixels)
                    if (c.A > 30) opaqueCount++;

                if (opaqueCount < w * h / 5)
                    continue;

                return new DebrisInfo
                {
                    Pixels = debrisPixels,
                    Width = w,
                    Height = h,
                    SourceRect = new Rectangle(x0, y0, w, h)
                };
            }
            return null;
        }

        private static Color[] CutIrregularShape(
            Color[] srcPixels, int srcW, int srcH,
            int x0, int y0, int w, int h, Point center)
        {
            var result = new Color[w * h];

            const int angleSamples = 16;
            float[] radiusNoise = new float[angleSamples];
            for (int i = 0; i < angleSamples; i++)
                radiusNoise[i] = 0.50f + Main.rand.NextFloat() * 0.50f;

            float cx = center.X - x0;
            float cy = center.Y - y0;
            float rx = Math.Max(1f, w / 2f);
            float ry = Math.Max(1f, h / 2f);

            for (int ly = 0; ly < h; ly++)
            {
                for (int lx = 0; lx < w; lx++)
                {
                    int sx = x0 + lx;
                    int sy = y0 + ly;
                    if (sx < 0 || sx >= srcW || sy < 0 || sy >= srcH)
                        continue;

                    Color srcColor = srcPixels[sy * srcW + sx];
                    if (srcColor.A <= 30)
                        continue;

                    float dx = lx - cx;
                    float dy = ly - cy;

                    float normDist = (float)Math.Sqrt(
                        (dx * dx) / (rx * rx) + (dy * dy) / (ry * ry));

                    float angle = (float)Math.Atan2(dy, dx);
                    float normalizedAngle = (angle + MathHelper.Pi) / MathHelper.TwoPi;
                    float sampleF = normalizedAngle * angleSamples;
                    int sampleA = (int)sampleF % angleSamples;
                    int sampleB = (sampleA + 1) % angleSamples;
                    float t = sampleF - (int)sampleF;
                    float radiusFactor = MathHelper.Lerp(radiusNoise[sampleA], radiusNoise[sampleB], t);

                    if (normDist > radiusFactor)
                        continue;

                    result[ly * w + lx] = srcColor;
                }
            }
            return result;
        }
    }

    public class DebrisInfo
    {
        public Color[] Pixels;
        public int Width;
        public int Height;
        public Rectangle SourceRect;
    }
}
