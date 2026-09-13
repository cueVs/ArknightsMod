using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Content.Items.Weapons.Defender.Beagle;
using ArknightsMod.Content.Items.Weapons.Defender.Durnar;
using ArknightsMod.Content.Items.Weapons.Defender.Vulcan;
using ArknightsMod.Players;
using Microsoft.Build.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RuneSKill.Content.NeedTool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Defender.Vulcan
{
	public class Vulcan_Hammer : ModProjectile
	{
		public SwingHelper.SwingHelper helper;
		public Player player => Main.player[Projectile.owner];
		public override void SetDefaults()
		{
			Projectile.width = 10; // ?�������?�����
			Projectile.height = 10; // ?�������?��?�
			Projectile.friendly = true; // ?������?��?���
			Projectile.penetrate = -1; // ?�������?�?
			Projectile.tileCollide = false; // ?���?����?��?
			Projectile.usesLocalNPCImmunity = true; // ?��?�����?
			Projectile.ownerHitCheck = true; // ?��?�����?���������?�����??�?�����?�?��?����?�?
			Projectile.DamageType = DamageClass.MeleeNoSpeed; // ?����?��??����
			Projectile.ignoreWater = true;
			Projectile.localNPCHitCooldown = 11;
		}

		public override void OnSpawn(IEntitySource source) {
			helper = new SwingHelper.SwingHelper(20, 2, true)
				.SetPlayer(player)
				.SetProj(Projectile)
				.SetTex(TextureAssets.Projectile[Projectile.type].Value)
				.SetSwingRad(MathF.PI);
			helper.handleLength = new Vector2(4,0);
			helper.swordLength = new Vector2(24,0);
		}
		public enum WeaponState { Move,Swing,Defende}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => helper.Colliding(targetHitbox);

		private bool press = false;
		public WeaponState state = WeaponState.Move;
		public override void AI() {
			if(player.HeldItem.type != ModContent.ItemType<Vulcan_Weapon>())
				Projectile.Kill();
			switch (state) {
				case WeaponState.Move:
					helper.Move();
					if (PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed && !press) {
						state = WeaponState.Swing;
						Vector2 mouse = Main.MouseWorld - player.Center;
						helper.PointMouseRad(MathF.Atan2(mouse.Y, mouse.X));
					}

					if (Main.mouseRight)
						state = WeaponState.Defende;
					break;
				case WeaponState.Swing:
					if (helper.Swing()) {
						state = WeaponState.Move;
						helper.ReloadIndex();
						helper.swingTime = 0;
					}
					break;
				case WeaponState.Defende:
					helper.Move();
					if (!Main.mouseRight)
						state = WeaponState.Move;
					break;
			}
			if (press && PlayerInput.MouseInfo.LeftButton != ButtonState.Pressed)
				press = false;
		}

		public override bool PreDraw(ref Color lightColor) {
			SpriteBatch sb = Main.spriteBatch;
			helper.DrawBlade(sb);
			if(state==WeaponState.Swing)
				helper.DrawTrip(SwingHelper.SwingHelper.SwingEffect.Zero,Color.DarkSlateGray,sb);
			return false;
		}
	}
}
