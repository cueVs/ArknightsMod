using ArknightsMod.Common.Players;
using ArknightsMod.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles
{
	public class PozemkaCrossbowSentry : ModProjectile
	{

		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 8;
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
		}

		public override void SetDefaults() {
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
			int Cooldown = 60;
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();

			Projectile.velocity.Y++; // gravity
			if (modPlayer.SummonMode || Main.LocalPlayer.HeldItem.ModItem is not PozemkaCrossbow) {
				Projectile.Kill();
			}

			NPC target = Projectile.FindTargetWithinRange(800, false);

			if (target != null) {
				// Projectile.rotation = (target.Center - Projectile.Center).ToRotation();
				float x = target.Center.X - Projectile.Center.X;
				float y = target.Center.Y - Projectile.Center.Y - 20;

				float theta = new Vector2(x, y).ToRotation();

				Projectile.rotation = theta;

				if (Projectile.ai[0] > 0) {
					Projectile.ai[0]--;
					if (Projectile.ai[0] < Cooldown - 16) {
						Projectile.frame = 0;
					}
					else if (Projectile.ai[0] % 2 == 0) {
						Projectile.frame++;
					}

					if (Projectile.frame == 3 && Main.myPlayer == Projectile.owner) {
						int damage = (int)Math.Round(Projectile.damage * 0.95);
						Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, 15 * new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)), ModContent.ProjectileType<PozemkaCrossbowSentryProjectile>(), damage, 5f, Projectile.owner);
						if (modPlayer.Skill == 2 && modPlayer.SkillActive) {
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/PozemkaCrossbowSentryProjectileS3"));
						}
						else {
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/PozemkaCrossbowSentryProjectileS0"));
						}
					}
				}
				else {
					Projectile.ai[0] = Cooldown;
					Projectile.frame = 0;
				}
			}
			else {
				Projectile.frame = 0;
			}


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
