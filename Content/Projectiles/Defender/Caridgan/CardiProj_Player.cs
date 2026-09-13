using ArknightsMod.Content.Items.Weapons.Defender.Cardigan;
using ArknightsMod.Content.Items.Weapons.Defender.Cuora;
using ArknightsMod.Content.Projectiles.Defender.Cuora;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Defender.Caridgan;

public class CardiProj_Player : ModPlayer
{
	public override void PostUpdate()
	{
		var it = Player.HeldItem;
		if (it.type == ModContent.ItemType<CardiWeapon>()) {
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<Cardi_Sword>()] == 0)
				Projectile.NewProjectile(Player.GetSource_FromThis(), Player.MountedCenter - Main.screenPosition,
					Vector2.One, ModContent.ProjectileType<Cardi_Sword>()
					, it.damage, it.knockBack);
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<Cardi_Shield>()] == 0)
				Projectile.NewProjectile(Player.GetSource_FromThis(), Player.MountedCenter - Main.screenPosition,
					Vector2.One, ModContent.ProjectileType<Cardi_Shield>()
					, it.damage, it.knockBack);
		}
	}
}