using System;
using Terraria;

namespace ArknightsMod.Content.Projectiles.Defender;

internal static class ShieldGuardRules
{
    internal static bool Supports(Item item) => item.ModItem is IShieldGuardWeapon
        or Items.Weapons.Defender.Cuora.CuoraWeapon
        or Items.Weapons.Defender.Cardigan.CardiWeapon
        or Items.Weapons.Defender.Durnar.DN_Weapon
        or Items.Weapons.Defender.Vulcan.Vulcan_Weapon;

    internal static bool IsRaised(Player player) => Supports(player.HeldItem) && player.active && !player.dead
        && !player.CCed && !player.noItems && player.whoAmI == Main.myPlayer && Main.mouseRight && !player.mouseInterface;

    internal static bool IsFront(Player player, float sourceX) => MathF.Sign(sourceX - player.Center.X) == player.direction;
}

/// <summary>Held shields can extend a successful frontal block without duplicating mitigation.</summary>
public interface IShieldGuardWeapon
{
    void OnGuardSuccess(Player player);
}
