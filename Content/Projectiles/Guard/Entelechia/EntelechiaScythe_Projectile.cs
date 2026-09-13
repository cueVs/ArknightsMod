using ArknightsMod.Content.Items.Weapons.Guard.Entelechia;
using ArknightsMod.Content.SwingHelper;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ArknightsMod.Content.Projectiles.Guard.Entelechia
{
	public class EntelechiaScythe_Projectile : ModProjectile
	{
		public SwingHelper.SwingHelper Helper;
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
			Projectile.localNPCHitCooldown = 11;
			tex = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Guard/Entelechia/swooshgray").Value;
		}
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Helper.Colliding(targetHitbox);
		public override void OnSpawn(IEntitySource source) {
			player = Main.player[Projectile.owner];
			Helper = new SwingHelper.SwingHelper(10, 3)
				.SetTex(TextureAssets.Projectile[ModContent.ProjectileType<EntelechiaScythe_Projectile>()].Value)
				.SetProj(Projectile)
				.SetPlayer(Main.player[Projectile.owner])
				.SetSwingRad(MathF.PI)
				.SetIndex(30);;
			Helper.handleLength = new Vector2(30, 0);
			Helper.swordLength = new Vector2(73, 0);
			Helper.setoff = new Vector2(-25, 0);
		}
		public SwordState state;
		public Player player;
		private bool press = false;
		public override void AI() {
			if(player.dead||player.HeldItem.type != ModContent.ItemType<EntelechiaScythe>())
				Projectile.Kill();
			switch (state)
			{
				case SwordState.Move:
					Helper.Move();
					if (PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed && !press)
					{
						press = true;
						state = SwordState.Swing;
						Vector2 mouse = Main.MouseWorld - player.Center;
						Helper.PointMouseRad(MathF.Atan2(mouse.Y, mouse.X));
					}
					break;
				case SwordState.Swing:
					if (Helper.Swing())
					{
						state = SwordState.Move;
						Helper.ReloadIndex();
						Helper.swingTime = 0;
					}
					break;
			}

			if (press && PlayerInput.MouseInfo.LeftButton != ButtonState.Pressed)
				press = false;
		}

		public static Texture2D tex;
		public Color[] Colors =  new Color[2]{Color.DarkRed,Color.Black} ;
		public override bool PreDraw(ref Color lightColor) {
			Helper.DrawBlade(Main.spriteBatch);
			if (state == SwordState.Swing)
				Helper.DrawTrip(SwingHelper.SwingHelper.SwingEffect.Zero,Colors , Main.spriteBatch,300,32,SwingHelper.SwingHelper.TripTex.Streamline);
			return false;
		}
	}
}

