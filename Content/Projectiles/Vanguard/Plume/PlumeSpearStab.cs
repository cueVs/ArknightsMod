using ArknightsMod.Content.Items.Weapons.Guard.Frostleaf;
using ArknightsMod.Content.Items.Weapons.Vanguard.Plume;
using ArknightsMod.Content.Projectiles.Guard.Frostleaf;
using ArknightsMod.Content.SwingHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Vanguard.Plume
{
	public class PlumeSpearStab : ModProjectile
	{public SwingHelper.SwingHelper helper;
		public Player player => Main.player[Projectile.owner];
		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10; // ?�������?��?�
			Projectile.friendly = true; // ?������?��?���
			Projectile.penetrate = -1; // ?�������?�?
			Projectile.tileCollide = false; // ?���?����?��?
			Projectile.usesLocalNPCImmunity = true; // ?��?�����?
			Projectile.ownerHitCheck = true; // ?��?�����?���������?�����??�?�����?�?��?����?�?
			Projectile.DamageType = DamageClass.MeleeNoSpeed; // ?����?��??����
			Projectile.ignoreWater = true;
			Projectile.localNPCHitCooldown = 14;
		}

		private enum WeaponState{Move,Swing,Stab}

		public override void OnSpawn(IEntitySource source) {
			helper = new SwingHelper.SwingHelper(20, 2)
				.SetPlayer(player)
				.SetProj(Projectile)
				.SetTex(TextureAssets.Projectile[Projectile.type].Value)
				.SetSwingRad(MathF.PI);
			helper.SetSetoff(new Vector2(-18, 0));
			helper.handleLength = new Vector2(36, 10);
			helper.swordLength = new Vector2(70, 10);
			helper.SetScale(new Vector2(1f, 1f));
			helper.lagTime = 8;
			helper.MaxChargetime = 10;
		}

		private int attack = 0;
		private bool press = false;
		private WeaponState state = WeaponState.Move;
		public override void AI() {
			if(player.dead||player.HeldItem.type != ModContent.ItemType<PlumePike>())
				Projectile.Kill();
			switch (state) {
				case WeaponState.Move:
					helper.Move();
					if (PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed && !press) {
						press = true;
						Vector2 mouse = Main.MouseWorld - player.Center;
						helper.PointMouseRad(MathF.Atan2(mouse.Y, mouse.X));
						helper.SetStabRad(MathF.Atan2(mouse.Y, mouse.X));
						if (attack == 0) {
							helper.ResetTime(8);
							state = WeaponState.Swing;
							attack += 1;
						}
						else {
							helper.ResetTime(8);
							state = WeaponState.Stab;
							attack =0;
						}
					}
					break;
				case WeaponState.Swing:
					if (helper.Swing()) {
						state = WeaponState.Move;
						helper.ReloadIndex();
					}
					break;
				case WeaponState.Stab:
					if (helper.Stab()) {
						helper.ReloadIndex();
						helper.ResetTime(8);
						helper.SetScale(new Vector2(1f, 1f));
						state = WeaponState.Move;
					}
					break;
			}
			if(press&&PlayerInput.MouseInfo.LeftButton==ButtonState.Pressed)
				press = false;
		}

		public override bool PreDraw(ref Color lightColor) {
			SpriteBatch sb = Main.spriteBatch;
			helper.DrawBlade(sb);
			if(state == WeaponState.Swing)
				helper.DrawTrip(SwingHelper.SwingHelper.SwingEffect.Zero,lightColor,sb);
			return false;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => helper.Colliding(targetHitbox);
	}
}