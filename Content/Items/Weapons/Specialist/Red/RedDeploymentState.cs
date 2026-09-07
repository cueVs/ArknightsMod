using System;

namespace ArknightsMod.Content.Items.Weapons.Specialist.Red;

// 与游戏对象无关的部署门槛和计时，可独立测试边界；不以速度持续超标来反复触发。
internal sealed class RedDeploymentState
{
    internal const int Duration = 5 * 60;
    internal const float LandingSpeed = 8f;
    internal int Cooldown { get; private set; }
    internal int Remaining { get; private set; }
    internal void Tick()
    {
        if (Cooldown > 0) Cooldown--;
        if (Remaining > 0) Remaining--;
    }
    internal bool Start()
    {
        if (Cooldown > 0) return false;
        Cooldown = Remaining = Duration;
        return true;
    }
    internal void CancelEffect() => Remaining = 0;
    internal void Receive(int remaining) => Cooldown = Remaining = Math.Clamp(remaining, 0, Duration);
    // 原版 TeleportationStyleID：传送机、混沌/和谐传送杖、传送门。回城/出生不算。
    internal static bool IsCombatTeleport(int style) => style is 0 or 1 or 4;
    internal static bool IsDashStart(bool wasDashing, bool dashing, float oldX, float newX,
        bool directionalInput, bool allowVelocityFallback) =>
        (!wasDashing && dashing && Math.Abs(newX) >= 6f)
        || (!dashing && !wasDashing && allowVelocityFallback && directionalInput
            && Math.Abs(newX) >= 12f && Math.Abs(newX - oldX) >= 6f);
}
