using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Content.Items.Weapons.Defender.Beagle;
using ArknightsMod.Content.Items.Weapons.Defender.Cuora;
using ArknightsMod.Content.Items.Weapons.Defender.Durnar;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Defender.Cuora;
public class CuoraProj_Player : ModPlayer
{
	public bool DefensiveStance = false;//龟龟形态！！！
	public bool Active1 = false;
	public override void PostUpdate()
	{
		var it = Player.HeldItem;
		if (it.type == ModContent.ItemType<CuoraWeapon>()) {
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<Cuora_Bat>()] == 0)
				Projectile.NewProjectile(Player.GetSource_FromThis(), Player.MountedCenter - Main.screenPosition,
					Vector2.One, ModContent.ProjectileType<Cuora_Bat>()
					, it.damage, it.knockBack);
			if (Player.ownedProjectileCounts[ModContent.ProjectileType<Cuora_Shield>()] == 0)
				Projectile.NewProjectile(Player.GetSource_FromThis(), Player.MountedCenter - Main.screenPosition,
					Vector2.One, ModContent.ProjectileType<Cuora_Shield>()
					, it.damage, it.knockBack);
		}
	}
	private int time = 0;
	public bool UnderAttack = false;
	public float UnderRad = 0;
	public int byTime = 0;

	public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo) {
		UnderAttack = true;
		Vector2 pos = npc.Center - Player.MountedCenter;
		UnderRad = MathF.Atan2(pos.Y, pos.X);
	}

	public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo) {
		UnderAttack = true;
		Vector2 pos = proj.Center - Player.MountedCenter;
		UnderRad = MathF.Atan2(pos.Y, pos.X);
	}

	public override void UpdateEquips()
	{
		var it = Player.HeldItem;
		var modPlayer = Player.GetModPlayer<WeaponPlayer>();

		if (DefensiveStance && !modPlayer.SkillActive) {
			DefensiveStance = false;
			time = 0;
		}
		if (DefensiveStance) {
			Player.moveSpeed *= 0.9f;
			Player.statDefense *= 2.3f;
			Player.statLife += (Player.statLifeMax2)/300;
			if(Player.statLife > Player.statLifeMax2)
				Player.statLife = Player.statLifeMax2;

		}

		if (Active1 && !modPlayer.SkillActive)
			Active1 = false;
		if(Active1)
			Player.statDefense *= 1.8f;
	}
}