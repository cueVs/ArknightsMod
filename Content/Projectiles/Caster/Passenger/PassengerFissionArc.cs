using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Utilities;

namespace ArknightsMod.Content.Projectiles.Caster.Passenger;

// 移植自 Fran/Core/Graphics/FranFissionArc.cs：保持原来的分叉几何、展开和熄灭算法。
// 每次放电只构建一次裂纹，仅依赖 Terraria 的单像素纹理，无需安装 Fran。
// Drawing requires an additive SpriteBatch and Terraria's 1x1 pixel; no shader or external assets.
internal sealed class PassengerFissionArc
{
    private const int PointLimit = 132;
    private readonly List<Branch> branches = new();
    private readonly UnifiedRandom random;
    private int pointCount;

    private sealed class Branch
    {
        public Vector2[] Points;
        public float Width;
        public float RevealStart;
        public float RevealSpan;
        public int Depth;
    }

    public PassengerFissionArc(Vector2 offset, int seed, float width, int depth = 2)
    {
        random = new UnifiedRandom(seed);
        if (!IsFinite(offset) || offset.LengthSquared() < 4f || offset.LengthSquared() > 2400f * 2400f)
            return;
        Build(Vector2.Zero, offset, MathHelper.Clamp(width, 0.6f, 7f),
            0, Math.Clamp(depth, 0, 2), 0f, 1f);
    }

    private void Build(Vector2 start, Vector2 end, float width, int depth, int maxDepth,
        float revealStart, float revealSpan)
    {
        float length = Vector2.Distance(start, end);
        if (length < 7f || pointCount >= PointLimit - 3)
            return;
        int count = Math.Min(PointLimit - pointCount,
            Math.Clamp((int)(length / (depth == 0 ? 32f : 20f)) + 3, 4, depth == 0 ? 28 : 8));
        pointCount += count;
        Vector2 axis = (end - start) / length;
        Vector2 normal = new(-axis.Y, axis.X);
        Vector2[] points = new Vector2[count];
        points[0] = start;
        points[^1] = end;
        float bend = MathHelper.Clamp(length * 0.075f, 2f, 24f) / (1f + depth * 0.3f);
        float displacement = 0f;
        for (int i = 1; i < count - 1; i++)
        {
            float t = (i + random.NextFloat(-0.24f, 0.24f)) / (count - 1f);
            displacement = displacement * -0.18f + random.NextFloat(-bend, bend);
            points[i] = Vector2.Lerp(start, end, t) + normal * displacement *
                (0.35f + 0.65f * MathF.Sin(t * MathHelper.Pi));
        }
        branches.Add(new Branch
        {
            Points = points, Width = width, Depth = depth,
            RevealStart = revealStart, RevealSpan = revealSpan
        });

        if (depth >= maxDepth)
            return;
        // Spaced anchor candidates prevent dense webs and keep the main direction legible.
        int forkCount = depth == 0 ? Math.Clamp((int)(length / 100f), 1, 5) : 1;
        for (int fork = 0; fork < forkCount; fork++)
        {
            int anchor = Math.Clamp((int)((fork + random.NextFloat(0.65f, 1.35f)) *
                (count - 2f) / (forkCount + 1f)), 1, count - 2);
            Vector2 localAxis = (points[anchor + 1] - points[anchor]).SafeNormalize(axis);
            float side = random.NextBool() ? 1f : -1f;
            Vector2 direction = localAxis.RotatedBy(side * random.NextFloat(0.48f, 1.05f));
            float reach = Math.Min(112f, length * random.NextFloat(0.16f, 0.30f));
            float anchorTime = revealStart + revealSpan * anchor / (count - 1f);
            Build(points[anchor], points[anchor] + direction * reach, width * 0.48f,
                depth + 1, maxDepth, anchorTime, revealSpan * reach / length * 0.7f);
        }
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 origin, Color color, float age,
        float lifetime, float growthFrames = 3f, float opacity = 1f)
    {
        if (!IsFinite(origin) || age < 0f || age >= lifetime || opacity <= 0f)
            return;
        float growth = MathHelper.Clamp(age / Math.Max(0.5f, growthFrames), 0f, 1.4f);
        float life = MathHelper.Clamp(age / Math.Max(1f, lifetime), 0f, 1f);
        float flash = age < growthFrames + 1f ? 1.12f : 1f;
        foreach (Branch branch in branches)
        {
            float extinction = branch.Depth == 0 ? 1f : branch.Depth == 1 ? 0.78f : 0.57f;
            float fade = 1f - Utils.GetLerpValue(extinction * 0.45f, extinction, life, true);
            float reveal = MathHelper.Clamp((growth - branch.RevealStart) /
                Math.Max(0.01f, branch.RevealSpan), 0f, 1f);
            if (fade <= 0f || reveal <= 0f)
                continue;
            float visibleSegments = reveal * (branch.Points.Length - 1);
            Color tint = branch.Depth == 0 ? color : Color.Lerp(color, PassengerLightningVisuals.Core, 0.2f);
            for (int i = 0; i < branch.Points.Length - 1 && i < visibleSegments; i++)
            {
                Vector2 a = origin + branch.Points[i];
                Vector2 b = origin + Vector2.Lerp(branch.Points[i], branch.Points[i + 1],
                    Math.Min(1f, visibleSegments - i));
                float along = i / (branch.Points.Length - 1f);
                float taper = branch.Depth == 0 ? 0.82f + 0.18f * along : 1f - along * 0.72f;
                float width = branch.Width * taper * (0.75f + fade * 0.25f);
                float strength = fade * flash * opacity;
                DrawLine(spriteBatch, a, b, tint * (strength * 0.12f), width * 5f);
                DrawLine(spriteBatch, a, b, tint * (strength * 0.54f), width * 1.9f);
                DrawLine(spriteBatch, a, b, Color.Lerp(tint, PassengerLightningVisuals.Core, 0.86f) * strength,
                    Math.Max(0.65f, width * 0.62f));
            }
        }
    }

    internal static void DrawLine(SpriteBatch spriteBatch, Vector2 a, Vector2 b, Color color, float width)
    {
        Vector2 edge = b - a;
        float length = edge.Length();
        if (!float.IsFinite(length) || length < 0.05f || !float.IsFinite(width))
            return;
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, a - Main.screenPosition,
            new Rectangle(0, 0, 1, 1), color, edge.ToRotation(), new Vector2(0f, 0.5f),
            new Vector2(length, width), SpriteEffects.None, 0f);
    }

    private static bool IsFinite(Vector2 p) => float.IsFinite(p.X) && float.IsFinite(p.Y);
}
