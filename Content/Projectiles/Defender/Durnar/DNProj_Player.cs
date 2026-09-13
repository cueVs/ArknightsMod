using ArknightsMod.Content.Items.Weapons.Defender.Beagle;
using ArknightsMod.Content.Items.Weapons.Defender.Durnar;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Defender.Durnar
{
    public enum ProjMode {Move,Attack,Defender}
    public class DNProj_Player : ModPlayer
    {
	    public bool ShieldAttackMode = false;
		public override void PostUpdate()
        {
            var it = Player.HeldItem;
            if(it.type == ModContent.ItemType<DN_Weapon>())
            {
                if(Player.ownedProjectileCounts[ModContent.ProjectileType<DN_Sword>()] == 0)
                    Projectile.NewProjectile(Player.GetSource_FromThis(),Player.MountedCenter-Main.screenPosition,Vector2.One,ModContent.ProjectileType<DN_Sword>()
                    ,it.damage,it.knockBack);
                if(Player.ownedProjectileCounts[ModContent.ProjectileType<DN_Shield>()] == 0)
                    Projectile.NewProjectile(Player.GetSource_FromThis(),Player.MountedCenter-Main.screenPosition,Vector2.One,ModContent.ProjectileType<DN_Shield>()
                        ,it.damage,it.knockBack);
            }
        }

		public override void UpdateEquips()
        {
            var it = Player.HeldItem;
            var modPlayer = Player.GetModPlayer<WeaponPlayer>();

            if (ShieldAttackMode && !modPlayer.SkillActive)
	            ShieldAttackMode = false;
        }
    }
}