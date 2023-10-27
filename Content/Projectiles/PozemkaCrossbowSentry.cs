using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ID;
using System;
using System.Linq;
using Terraria.Audio;
using ArknightsMod.Common.Players;
using Terraria.DataStructures;
using ArknightsMod.Content.Projectiles;
using ArknightsMod.Content.Items.Weapons;

namespace ArknightsMod.Content.Projectiles
{
	public class PozemkaCrossbowSentry : ModProjectile
	{
		private NPC target;
		private const int Cooldown = 30;


		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 8;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults()
		{
			Projectile.width = 64;
			Projectile.height = 38;

			Projectile.friendly = false; // Can the projectile deal damage to enemies?
			Projectile.hostile = false; // Can the projectile deal damage to the player?
			Projectile.penetrate = 1; // How many monsters the projectile can penetrate.
			Projectile.tileCollide = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.ownerHitCheck = true; // Prevents hits through tiles. Most melee weapons that use projectiles have this

			Projectile.alpha = 0; // How transparent to draw this projectile. 0 to 255. 255 is completely transparent.

			// turret
			Projectile.sentry = true;
			Projectile.netImportant = true;
			Projectile.timeLeft = Projectile.SentryLifeTime;

		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();

			Projectile.velocity.Y++; // gravity
			if (modPlayer.SummonMode || Main.LocalPlayer.HeldItem.ModItem is not PozemkaCrossbow) {
				Projectile.Kill();
			}

			//if (player.HasMinionAttackTargetNPC) {
			//	target = Main.npc[player.MinionAttackTargetNPC];
			//}
			//Projectile.direction = target.direction;
			//Projectile.spriteDirection = Projectile.direction;


			//if (Projectile.ai[0] > 0) {
			//	Projectile.ai[0]--;
			//	Projectile.spriteDirection = target.direction;
			//	Projectile.frame++;
			//	if (Projectile.frame == 3) {
			//		Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<PozemkaCrossbowSentryProjectile>(), Projectile.damage, 5f, Projectile.owner);
			//	}
			//	if (Projectile.frame == 9) {
			//		Projectile.frame = 0;
			//	}
			//}
			//else if (target != null && Projectile.velocity.Y == 0) {
			//	// SoundEngine.PlaySound(Sounds.Rattle, Projectile.Center);
			//	Projectile.ai[0] = Cooldown;
			//	Projectile.frame = 0; //this frame is skipped because we always add 1 to it after, but we want this to happen
			//	Projectile.spriteDirection = target.direction;
			//}


		}

		public override bool? CanCutTiles() {
			return false;
		}

		public override bool MinionContactDamage() {
			return false;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) {
			fallThrough = false;
			return true;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			return false;
		}

	}


}
