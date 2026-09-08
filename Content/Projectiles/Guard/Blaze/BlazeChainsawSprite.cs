using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace ArknightsMod.Content.Projectiles.Guard.Blaze;

/// <summary>煌专用三帧锯身绘制，不修改共用近战绘制层或伤害判定。</summary>
internal static class BlazeChainsawSprite
{
    internal const string TexturePath = "ArknightsMod/Content/Items/Weapons/Guard/Blaze/BlazeGreatsword";
    internal const int FrameCount = 3;
    internal const int FrameWidth = 106;
    internal const int FrameHeight = 50;
    // 握点移至锯身中心稍上方，使锯身在手中略微下沉；左右翻转时保持握点稳定。
    private static readonly Vector2 Grip = new(24f, 20f);
    private static readonly Vector2 Tip = new(102f, 22f);

    internal static int FrameAt(int age, int ticksPerFrame)
        => Math.Max(0, age) / Math.Max(1, ticksPerFrame) % FrameCount;

    internal static void DrawHeld(Texture2D sheet, Vector2 gripWorld, float angle,
        bool faceLeft, float reach, Color color, int frame)
    {
        Rectangle source = new(0, Math.Clamp(frame, 0, FrameCount - 1) * FrameHeight, FrameWidth, FrameHeight);
        Vector2 axis = Tip - Grip;
        float nativeAngle = axis.ToRotation();
        Vector2 origin = Grip;
        if (faceLeft)
            origin.Y = FrameHeight - origin.Y;
        // 只调整视觉缩放，使锯尖对齐现有攻击端点，不从纹理反推或放大攻击范围。
        Main.spriteBatch.Draw(sheet, gripWorld - Main.screenPosition, source, color,
            faceLeft ? angle + nativeAngle : angle - nativeAngle, origin, reach / axis.Length(),
            faceLeft ? SpriteEffects.FlipVertically : SpriteEffects.None, 0f);
    }
}
