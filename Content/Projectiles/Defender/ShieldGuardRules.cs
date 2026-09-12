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

    internal static bool IsRaised(Player player)
    {
        if (!Supports(player.HeldItem) || !player.active || player.dead || player.CCed || player.noItems)
            return false;
        // 号角是自动炮盾：手持且冷却就绪即举盾，不依赖左右键，冷却结束后自动恢复。
        if (player.HeldItem.ModItem is Items.Weapons.Defender.Horn.HornGrenadeLauncher)
            return player.GetModPlayer<Defender_Player>().CD == 0;
        return player.whoAmI == Main.myPlayer && Main.mouseRight && !player.mouseInterface;
    }

    internal static bool IsFront(Player player, float sourceX) => MathF.Sign(sourceX - player.Center.X) == player.direction;
}

/// <summary>Held shields can extend a successful frontal block without duplicating mitigation.</summary>
public interface IShieldGuardWeapon
{
    void OnGuardSuccess(Player player);
}
