using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Passenger;

/// <summary>沿用原版电能 Dust 的帧与运动，只覆盖它默认偏白的着色。</summary>
public sealed class PassengerElectricDust : ModDust
{
    public override string Texture => "Terraria/Images/Dust";
    public override void SetStaticDefaults() => UpdateType = DustID.Electric;
    public override Color? GetAlpha(Dust dust, Color lightColor)
        => new Color(dust.color.R, dust.color.G, dust.color.B, 25) * MathHelper.Clamp(dust.scale * 1.5f, 0f, 1f);
}
