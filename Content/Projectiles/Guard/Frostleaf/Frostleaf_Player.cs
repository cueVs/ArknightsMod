using ArknightsMod.Content.Items.Weapons.Guard.Chen;
using ArknightsMod.Content.Items.Weapons.Guard.Frostleaf;
using ArknightsMod.Content.Projectiles.Guard.Entelechia;
using ArknightsMod.Content.SwingHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Frostleaf
{
	public class Frostleaf_Player : ModPlayer
	{
		public override void PostUpdate() {
			if(Player.HeldItem.type == ModContent.ItemType<FrostleafAxe>())
			{
				if(Player.ownedProjectileCounts[ModContent.ProjectileType<Frostleaf_Axe>()] == 0)
					Projectile.NewProjectile(Player.GetSource_FromThis(),Player.MountedCenter-Main.screenPosition,Vector2.One,ModContent.ProjectileType<Frostleaf_Axe>()
						,Player.HeldItem.damage,Player.HeldItem.knockBack);
			}
		}
	}
}

