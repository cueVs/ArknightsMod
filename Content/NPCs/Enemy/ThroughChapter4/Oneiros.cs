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
using System.Threading;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;


namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	public class Oneiros : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 9;
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}
		public override void SetDefaults() {
			NPC.width = 20;
			NPC.height = 40;
			NPC.damage = 18;
			NPC.defense = 10;
			NPC.lifeMax = 340;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath3;
			NPC.value = 100f;
			NPC.knockBackResist = 0.35f;
			NPC.aiStyle = -1;
			NPC.scale = 2f;
			NPC.noGravity = true;
		}


		private int AttackCD = 0;
		private bool attack = false;
		private bool walk = true;
		private int Framespeed = 7;
		private int framecounter;
		private int attackframeY;
		private float maxspeed = 2.0f;
		private int jumpCD = 0;
		private int walktimer;
		private Vector2 Targetpos1;
		private Vector2 Targetpos2;
		private Vector2 Vplus;
		private int directionchoose;
		private int Holdtime;

		public override void FindFrame(int frameHeight) {

			attackframeY = 4 * frameHeight;
			NPC.TargetClosest(true);
			NPC.frameCounter++;
			if (NPC.frame.Y == 0) {
				NPC.frame.Y = 0;
			}
			if (NPC.frameCounter >= Framespeed) {
				NPC.frame.Y += frameHeight;
				NPC.frameCounter = 0;
			}
			if (walk == true && NPC.frame.Y >= attackframeY) {
				NPC.frame.Y = 0;
			}
			if (attack == true && (NPC.frame.Y < (attackframeY) || NPC.frame.Y > (8 * frameHeight))) {
				NPC.frame.Y = attackframeY;
			}

		}
		public override void AI() {
			Player p = Main.player[NPC.target];
			directionchoose = p.Center.X - NPC.Center.X >= 0 ? 1 : -1;
			float angle = (float)Math.Atan((p.Center.Y - NPC.Center.Y) / (p.Center.X - NPC.Center.X));
			if (walk == true) {
				walktimer++;
				Dust dust;
				dust = Dust.NewDustDirect(Targetpos1, 22, 22, DustID.IceTorch, 0, 0, 0, default, 4);
				dust.noGravity = true;
			}
			NPC.TargetClosest(true);
			if (p.position.X - NPC.position.X > 0) {
				Targetpos1.X = (p.position.X - Math.Min(((720-walktimer) * Main.screenWidth / 1080), Main.screenWidth / 3));
				Targetpos1.Y = (p.position.Y -100- ((NPC.life / NPC.lifeMax) * 0.6f + 0.4f) * Math.Max((720 - walktimer), 0) * Main.screenHeight / 1440);
			}
			if (p.position.X - NPC.position.X <= 0) {
				Targetpos1.X = (p.position.X + Math.Min(((720 - walktimer) * Main.screenWidth / 1080), Main.screenWidth / 3));
				Targetpos1.Y = (p.position.Y -100 - ((NPC.life / NPC.lifeMax) * 0.6f + 0.4f) * Math.Max((720 - walktimer), 0) * Main.screenHeight / 1440);

			}
			if (p.position.X - NPC.position.X > 0) {
				Targetpos2 = new Vector2(p.position.X -  (int)(Main.screenWidth*0.5), p.position.Y-Main.screenHeight/3);

			}
			if (p.position.X - NPC.position.X <= 0) {
				Targetpos2 = new Vector2(p.position.X + (int)(Main.screenWidth * 0.5), p.position.Y - Main.screenHeight / 3);

			}
			if (walk) {
				Vplus = new Vector2(Math.Max(-0.2f,Math.Min((Targetpos1.X - NPC.position.X)/750,0.2f)) , Math.Max(-0.2f, Math.Min((Targetpos1.Y - NPC.position.Y)/520, 0.2f)));
				if (Math.Abs(NPC.velocity.X) <= 1.75f) {
					NPC.velocity.X += Math.Max(-0.2f, Math.Min((Targetpos1.X - NPC.position.X) / 750, 0.2f));
				}
				if (NPC.velocity.X >= 1.75f) {
					NPC.velocity.X = 1.75f;
				}
				if (NPC.velocity.X <= -1.75f) {
					NPC.velocity.X = -1.75f;
				}

				if (Math.Abs(NPC.velocity.Y) <= 1.75f) {
					NPC.velocity.Y += Math.Max(-0.2f, Math.Min((Targetpos1.Y - NPC.position.Y) / 520, 0.2f));
				}
				if (NPC.velocity.Y >= 1.75f) {
					NPC.velocity.Y = 1.75f;
				}
				if (NPC.velocity.Y <= -1.75f) {
					NPC.velocity.Y = -1.75f;
				}
			}
			if (attack) {
				if (Holdtime <= 120) {
					NPC.velocity.X = 0;
					NPC.velocity.Y = 0;
				}
				Vplus = new Vector2(Math.Max(-0.3f, Math.Min((Targetpos2.X - NPC.position.X) / 750, 0.3f)), Math.Max(-0.3f, Math.Min((Targetpos2.Y - NPC.position.Y) / 520, 0.3f)));
				if (Math.Abs(NPC.velocity.X) < 2.8f) {
					NPC.velocity.X += Math.Max(-0.3f, Math.Min((Targetpos2.X - NPC.position.X) / 750, 0.3f));
				}
				if (NPC.velocity.X >= 2.8f) {
					NPC.velocity.X = 2.8f;
				}
				if (NPC.velocity.X <= -2.8f) {
					NPC.velocity.X = -2.8f;
				}
				if (Math.Abs(NPC.velocity.Y) < 2.8f) {
					NPC.velocity.Y += Math.Max(-0.3f, Math.Min((Targetpos2.Y - NPC.position.Y) / 520, 0.3f));
				}
				if (NPC.velocity.Y >= 2.8f) {
					NPC.velocity.Y = 2.8f;
				}
				if (NPC.velocity.Y <= -2.8f) {
					NPC.velocity.Y = -2.8f;
				}
			}
			if (walk) {
				AttackCD++;
				if (AttackCD >= 200 && Math.Abs(NPC.position.X - p.position.X)<208 && Math.Abs(NPC.position.Y - p.position.Y) < 208) {
					Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(NPC.position.X, NPC.position.Y + 20), new Vector2(directionchoose * 0.8f, 0).RotatedBy(angle), ModContent.ProjectileType<Snowcastershoot>(), 16, 0.8f);
					attack = true;
					walk = false;
					AttackCD = 0;
				}
			}
			if (attack) {
				Holdtime++;
				if (Math.Abs(NPC.position.X - Targetpos2.X) < 400) {
					walk = true;
					attack = false;
					walktimer = 0;
					Holdtime = 0;
				}
			}
		}
	}
}
