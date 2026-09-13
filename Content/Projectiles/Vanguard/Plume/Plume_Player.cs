using ArknightsMod.Content.Items.Weapons.Vanguard.Plume;
using ArknightsMod.Content.Projectiles.Guard.Frostleaf;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Vanguard.Plume
{
	public class Plume_Player : ModPlayer
	{
		public override void PostUpdate() {
			if(Player.HeldItem.type == ModContent.ItemType<PlumePike>())
			{
				if(Player.ownedProjectileCounts[ModContent.ProjectileType<PlumeSpearStab>()] == 0)
					Projectile.NewProjectile(Player.GetSource_FromThis(),Player.MountedCenter-Main.screenPosition,Vector2.One,ModContent.ProjectileType<PlumeSpearStab>()
						,Player.HeldItem.damage,Player.HeldItem.knockBack);
			}
		}
	}
}
