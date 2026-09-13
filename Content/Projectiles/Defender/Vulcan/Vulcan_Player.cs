using ArknightsMod.Content.Items.Weapons.Defender.Durnar;
using ArknightsMod.Content.Items.Weapons.Defender.Vulcan;
using ArknightsMod.Content.Projectiles.Defender.Durnar;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Defender.Vulcan
{
	public class Vulcan_Player : ModPlayer
	{
		public override void PostUpdate()
		{
			var it = Player.HeldItem;
			if(it.type == ModContent.ItemType<Vulcan_Weapon>())
			{
				if(Player.ownedProjectileCounts[ModContent.ProjectileType<Vulcan_Hammer>()] == 0)
					Projectile.NewProjectile(Player.GetSource_FromThis(),Player.MountedCenter-Main.screenPosition,Vector2.One,ModContent.ProjectileType<Vulcan_Hammer>()
						,it.damage,it.knockBack);
				if(Player.ownedProjectileCounts[ModContent.ProjectileType<Vulcan_Shield>()] == 0)
					Projectile.NewProjectile(Player.GetSource_FromThis(),Player.MountedCenter-Main.screenPosition,Vector2.One,ModContent.ProjectileType<Vulcan_Shield>()
						,it.damage,it.knockBack);
			}
		}
	}
}

