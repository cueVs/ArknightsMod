using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Specialist.Gravel
{
	// 砾双刀向下挥砍弹幕：贴图剑尖朝正右、剑柄最左端，以柄为支点弧线向下挥砍
	// 面朝左时弧线反转保持"向下"方向一致
	public class GravelSlash : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Items/Weapons/Specialist/Gravel/GravelDualBlades_protile1";

		private const float Radius = 20f;
		private const float HalfArc = 1.3f;

		private float aim;
		private float currentAng;
		private bool faceRight;
		private ref float Progress => ref Projectile.localAI[0];

		public override void SetDefaults() {
			Projectile.width = 40;
			Projectile.height = 40;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ownerHitCheck = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 60;
			Projectile.timeLeft = 60;
			Projectile.scale = 1.1f;
		}

		public override void OnSpawn(IEntitySource source) {
			Player player = Main.player[Projectile.owner];
			aim = (Main.MouseWorld - player.Center).ToRotation();
			faceRight = Math.Cos(aim) >= 0;
			// 面朝右：从 aim-HalfArc（上方）扫到 aim+HalfArc（下方）
			// 面朝左：从 aim+HalfArc（上方）扫到 aim-HalfArc（下方），保持视觉向下
			currentAng = faceRight ? aim - HalfArc : aim + HalfArc;
			SoundEngine.PlaySound(SoundID.Item1, player.Center);
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (player.dead || !player.active) { Projectile.Kill(); return; }

			float step = 0.08f * (40f / Math.Max(1, player.itemAnimationMax));
			Progress += step;
			float p = MathHelper.Clamp(Progress, 0f, 1f);

			currentAng = faceRight
				? aim - HalfArc + 2f * HalfArc * p   // 右：上方→下方
				: aim + HalfArc - 2f * HalfArc * p;  // 左：上方→下方

			Projectile.Center = player.Center + currentAng.ToRotationVector2() * Radius;
			player.ChangeDir(faceRight ? 1 : -1);
			player.heldProj = Projectile.whoAmI;
			player.itemTime = 2;
			player.itemAnimation = 2;
			player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, currentAng - MathHelper.PiOver2);

			if (Progress >= 1f) {
				Projectile.alpha += 40;
				if (Projectile.alpha >= 255) Projectile.Kill();
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			SpriteEffects fx = faceRight ? SpriteEffects.None : SpriteEffects.FlipVertically;
			Vector2 origin = new Vector2(0f, tex.Height / 2f);
			float fade = 1f - Projectile.alpha / 255f;
			Main.spriteBatch.Draw(tex, Main.player[Projectile.owner].Center - Main.screenPosition,
				null, Color.White * fade, currentAng, origin, Projectile.scale, fx, 0f);
			return false;
		}

		public override bool ShouldUpdatePosition() => false;
	}
}
