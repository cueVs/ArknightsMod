using ArknightsMod.Content.Items.Weapons.Guard.Entelechia;
using ArknightsMod.Content.Projectiles.Defender.Durnar;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Entelechia
{
	public class EntelechiaScythe_Player : ModPlayer
	{
		public override void PostUpdate()
		{
			var it = Player.HeldItem;
			if(it.type == ModContent.ItemType<EntelechiaScythe>())
			{
				if(Player.ownedProjectileCounts[ModContent.ProjectileType<EntelechiaScythe_Projectile>()] == 0)
					Projectile.NewProjectile(Player.GetSource_FromThis(),Player.MountedCenter-Main.screenPosition,Vector2.One,ModContent.ProjectileType<EntelechiaScythe_Projectile>()
						,it.damage,it.knockBack);
			}
		}
	}
}

