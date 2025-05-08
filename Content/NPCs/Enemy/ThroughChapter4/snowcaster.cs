using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ModLoader.Utilities;
using Terraria.Localization;
using Terraria.DataStructures;
using ArknightsMod.Content.Items;
using System.Runtime.CompilerServices;
using System;
using Microsoft.Xna.Framework;



namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	public class snowcaster : ModNPC {
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 15;
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}
		public override void SetDefaults() {
			NPC.width = 17;
			NPC.height = 40;
			NPC.damage = 12;
			NPC.defense = 20;
			NPC.lifeMax = 500;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.BlizzardInsideBuildingLoop;
			NPC.value = 100f;
			NPC.knockBackResist = 0.50f;
			NPC.aiStyle = -1;
			NPC.scale = 1f;

		}
		private int AttackCD = 0;
		private bool attack;
		private bool walk = true;
		private int Framespeed = 9;
		private int framecounter;
		private int attackframeY;
		private float maxspeed = 1.1f;
		private int jumpCD = 0;
		private int directionchoose;
		
		public override void FindFrame(int frameHeight) {

			attackframeY = 6 * frameHeight;
			NPC.TargetClosest(true);
			framecounter++;
			if (framecounter >= Framespeed) {
				NPC.frame.Y += frameHeight;
				framecounter = 0;
			}
			if (walk == true && NPC.frame.Y >= attackframeY) {
				NPC.frame.Y = 0;
			}
			if (attack == true && (NPC.frame.Y < (attackframeY) || NPC.frame.Y > (14 * frameHeight))) {
				NPC.frame.Y = attackframeY;
			}

		}
		public override void AI() {
			
			Player p = Main.player[NPC.target];
			directionchoose = p.Center.X - NPC.Center.X >= 0 ? 1 : -1;
			float angle = (float)Math.Atan((p.Center.Y - NPC.Center.Y) / (p.Center.X - NPC.Center.X));
			if (walk == true) {
				NPC.spriteDirection = -NPC.direction;
				AttackCD++;
				if (NPC.position.X - p.position.X < -200 || (0 < NPC.position.X - p.position.X && NPC.position.X - p.position.X < 150)) {
					if (NPC.velocity.X < maxspeed) {
						NPC.velocity.X += 0.3f;
					}
					if (NPC.velocity.X >= maxspeed) {
						NPC.velocity.X = maxspeed;
					}
				}

				if (NPC.position.X - p.position.X > 200 || (0 > NPC.position.X - p.position.X && NPC.position.X - p.position.X > -150)) {
					if (NPC.velocity.X > -maxspeed) {
						NPC.velocity.X += -0.3f;
					}
					if (NPC.velocity.X <= -maxspeed) {
						NPC.velocity.X = -maxspeed;
					}
				}

				if (Math.Abs(NPC.velocity.X) <= 0.5f && attack == false) {
					jumpCD++;
				}
				if (jumpCD >= 400) {
					jumpCD = 0;
					NPC.velocity.Y = -7.2f;
				}
				if (AttackCD >= 280 && Math.Abs(NPC.position.X - p.position.X) <= 400 && !attack && Math.Abs(NPC.position.Y - p.position.Y) <= 200) {
					walk = false;
					attack = true;
					AttackCD = 0;
				}
			}
			if (attack == true) {
				NPC.velocity.X = 0;
				AttackCD++;
				if (AttackCD == 1) {
					Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(NPC.position.X,NPC.position.Y+20), new Vector2(directionchoose * 0.8f, 0).RotatedBy(angle), ModContent.ProjectileType<Snowcastershoot>(), 16, 0.8f);
				}
				if (AttackCD > 54) {
						attack = false;
						walk = true;
						AttackCD = 0;
					
				}
			}

		}
		public override bool? CanFallThroughPlatforms() {
			Player player = Main.player[NPC.target];
			return (player.position.Y + player.height) - (NPC.position.Y + NPC.height) > 0;
		}

	}
	public class Snowcastershoot : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 40;

		}
		public override void SetDefaults() {
			Projectile.width = 22;
			Projectile.height = 22;
			Projectile.damage = 24;
			Projectile.penetrate = 9999;
			
			Projectile.hostile = true;
		}
		private int flytime;
		private Vector2 axis;
		private Vector2 unaxis;
		public override void AI() {
			flytime++;
			Projectile.spriteDirection = -Projectile.direction;
			Projectile.frame = 4;
			Dust dust;
			dust = Dust.NewDustDirect(Projectile.position,22,22,DustID.IceTorch,0,0,0,default,4);
			dust.noGravity = true;
			if (flytime <= 30) {
				Projectile.velocity = Projectile.oldVelocity * 0.95f;

			}
			if (flytime > 30&&flytime<=40) {
				Projectile.velocity *= 1.5f;
			}
			if (flytime > 40) {
				Projectile.velocity *= 1.1f;
			}
			
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Projectile.playerImmune[target.whoAmI] = 10;
			
			if (target.HasBuff(BuffID.Chilled)) {
				target.AddBuff(BuffID.Frozen, 200);
			}
			else {
				target.AddBuff(46, 200);
			}
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			modifiers.ArmorPenetration += 9999f;
			if (Main.expertMode)
				modifiers.FinalDamage *= 0.9f; // 专家模式伤害 ×1.5
			if (Main.masterMode)
				modifiers.FinalDamage *= 0.85f;   // 大师模式伤害 ×2
		}

	}
}
