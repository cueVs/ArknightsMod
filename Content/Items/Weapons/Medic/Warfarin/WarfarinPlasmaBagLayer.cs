using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Medic.Warfarin;

// 复用原版持杖 DrawData 的完整变换，挂点不会因朝向、缩放或手臂姿势而漂移。
public sealed class WarfarinPlasmaBagLayer : PlayerDrawLayer
{
    private const string BagTexture = "ArknightsMod/Content/Items/Weapons/WarfarinWeaponTrueExtra";
    // 64×64 单帧本体上的杖顶挂点；以后调整美术时只需改这里。
    private static readonly Vector2 StaffPin = new(48f, 4f);

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.HeldItem);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        Player player = drawInfo.drawPlayer;
        if (drawInfo.shadow != 0f || player.dead || player.HeldItem.ModItem is not WarfarinStaff)
            return;
        Texture2D staff = TextureAssets.Item[player.HeldItem.type].Value;
        // 找到这一帧实际绘出的本体；收杖、隐身等没有本体时也不画悬空的袋子。
        for (int i = drawInfo.DrawDataCache.Count - 1; i >= 0; i--)
        {
            DrawData body = drawInfo.DrawDataCache[i];
            if (body.texture != staff)
                continue;
            Rectangle frame = body.sourceRect ?? staff.Bounds;
            Vector2 pin = StaffPin - frame.TopLeft();
            if ((body.effect & SpriteEffects.FlipHorizontally) != 0)
                pin.X = frame.Width - pin.X;
            if ((body.effect & SpriteEffects.FlipVertically) != 0)
                pin.Y = frame.Height - pin.Y;
            Vector2 anchor = body.position + ((pin - body.origin) * body.scale).RotatedBy(body.rotation);
            float swing = player.GetModPlayer<WarfarinBagMotion>().Sample(anchor + Main.screenPosition);
            Texture2D bag = ModContent.Request<Texture2D>(BagTexture).Value;
            // 上端中点就是钉子；袋子不继承杖身旋转或镜像，重力方向始终朝屏幕下方。
            DrawData hangingBag = new(bag, anchor, null, body.color, swing,
                new Vector2(bag.Width * .5f, 1f), body.scale, SpriteEffects.None);
            hangingBag.shader = body.shader;
            drawInfo.DrawDataCache.Add(hangingBag);
            return;
        }
    }
}

// 纯视觉状态，每个玩家独立；同一游戏帧的重复绘制不会重复积分。
public sealed class WarfarinBagMotion : ModPlayer
{
    private ulong lastTick;
    private bool initialized;
    private Vector2 previousAnchor, previousVelocity;
    private float angle, angularVelocity;

    internal float Sample(Vector2 anchor)
    {
        ulong tick = Main.GameUpdateCount;
        if (!initialized || tick - lastTick > 5 || Vector2.DistanceSquared(anchor, previousAnchor) > 160f * 160f)
        {
            initialized = true;
            lastTick = tick;
            previousAnchor = anchor;
            previousVelocity = Vector2.Zero;
            angle = angularVelocity = 0f;
            return angle;
        }
        if (lastTick == tick)
            return angle;
        float ticks = tick - lastTick;
        Vector2 velocity = (anchor - previousAnchor) / ticks;
        float acceleration = (velocity.X - previousVelocity.X) / ticks;
        // 水平加速度带动轻摆，重力回正与阻尼让袋子停下来后自然收敛。
        angularVelocity += MathHelper.Clamp(acceleration * .006f, -.07f, .07f);
        for (int i = 0; i < (int)ticks; i++)
        {
            angularVelocity = (angularVelocity - angle * .045f) * .88f;
            angle += angularVelocity;
            float limited = MathHelper.Clamp(angle, -.24f, .24f);
            if (limited != angle)
                angularVelocity *= -.2f;
            angle = limited;
        }
        previousAnchor = anchor;
        previousVelocity = velocity;
        lastTick = tick;
        return angle;
    }
}
