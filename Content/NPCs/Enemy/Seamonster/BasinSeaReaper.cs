using Terraria;
using Terraria.Localization;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Microsoft.Xna.Framework;

using Microsoft.Xna.Framework.Graphics;
using System.Security.Cryptography.X509Certificates;
using ArknightsMod.Content.Items.Material;
using log4net.Core;
using System;



namespace ArknightsMod.Content.NPCs.Enemy.Seamonster
{
	public class BasinSeaReaper:ModNPC
	{
		public override void SetDefaults() {
			NPC.width = 80;
			NPC.height = 118;
			NPC.damage = 50;
			NPC.defense = 26;
			NPC.lifeMax = 1000;
			NPC.HitSound = SoundID.NPCHit2;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 32000;
			NPC.knockBackResist = 0.4f;
			NPC.aiStyle = -1; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic. // Use vanilla zombie's type when executing AI code. (This also means it will try to despawn during daytime)
			AnimationType = -1;
			NPC.npcSlots = 5;
			Main.npcFrameCount[Type] = 34;
			NPC.friendly = false;
			NPC.noGravity = false;
			if (Main.expertMode) {
				NPC.lifeMax = (int)(NPC.lifeMax * 0.8);
				NPC.damage = (int)(NPC.damage * 0.8);
			}
		}
		private bool awake;
		private bool sleep=true;
		private float wondertime;
		private float jumpCD;
		private int status;
		private int direction;
		private float blooding;
		private float acceleration = 0.05f;
		private float maxSpeed = 8f;
		private float waketime;
		private float distance;
		private float diffX;
		private float diffY;
		public override void FindFrame(int frameHeight) {
			NPC.TargetClosest(true);
			NPC.spriteDirection = -NPC.direction;
			Player p = Main.player[NPC.target];
			int Startframe = 0;
			int Endframe = 8;
			int Framespeed = 4;
			NPC.frameCounter++;
			if (NPC.frameCounter >= Framespeed) {
				NPC.frame.Y += frameHeight;
				NPC.frameCounter = 0;
			}
			if (sleep) {
				if (NPC.frame.Y >= 7 * frameHeight) {
					NPC.frame.Y = 0;
				}
			}
			if (awake) {
				if (waketime == 1) {
					NPC.frame.Y = 8 * frameHeight;

				}
				if (NPC.frame.Y >= 33 * frameHeight) {
					NPC.frame.Y = 20 * frameHeight;
				}
			}
			
		}

		public override void AI() {
			NPC.TargetClosest();
			Player Player = Main.player[NPC.target];
			diffX = Player.Center.X - NPC.Center.X;
			diffY = Player.Center.Y - NPC.Center.Y;
			distance = (float)Math.Sqrt(Math.Pow(diffX / 16, 2) + Math.Pow(diffY / 16, 2));
			if (NPC.life < NPC.lifeMax * 0.99f) {
				sleep = false;
				awake = true;
			}
			if (sleep) {
				
				jumpCD++;
				if (NPC.ai[3] % 180 == 0) {
					NPC.ai[3] = 0;
					status = Main.rand.Next(5);
					if (status == 1 || status == 3) {
						direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
						NPC.direction = direction;
					}
					if (status == 4) {
						NPC.direction *= -1;
					}
				}
				switch (status) {
					case 0:
						NPC.velocity.X = 0.5f * NPC.direction;
						break;
					case 1:
						NPC.velocity.X = 0.4f * NPC.direction;
						break;
					case 2:
						NPC.velocity.X *= 0;
						break;
					case 3:
						NPC.velocity.X = 0.7f * NPC.direction;
						break;
					case 4:
						NPC.velocity.X = 0.5f * NPC.direction;
						break;
				}
				if (NPC.collideX && jumpCD >= 100) {
					NPC.velocity.Y = -5f;
					jumpCD = 0;
				}
				NPC.ai[3]++;
				
			}
			if (awake) {
				blooding++;
				waketime++;
				if (blooding == 10) {
					NPC.life -= 5;
					blooding = 0;
				}
				jumpCD++;
				if (NPC.Center.X > (Main.player[NPC.target].Center.X + 50)) {
					NPC.velocity.X -= acceleration;
					if (NPC.velocity.X > 0)
						NPC.velocity.X -= acceleration;
					if (NPC.velocity.X < -maxSpeed)
						NPC.velocity.X = -maxSpeed;
				}
				if (NPC.Center.X <= (Main.player[NPC.target].Center.X - 50)) {
					NPC.velocity.X += acceleration;
					if (NPC.velocity.X < 0)
						NPC.velocity.X += acceleration;
					if (NPC.velocity.X > maxSpeed)
						NPC.velocity.X = maxSpeed;
				}
				if (NPC.collideX == true && jumpCD >= 100) {
					NPC.velocity.Y = -12f;
					jumpCD = 0;
				}
				if (NPC.Center.Y > (Main.player[NPC.target].Center.Y - 10) && jumpCD >= 100) {
					NPC.velocity.Y = -12f;
					jumpCD = 0;
				}
				if (distance <= 20) {
					Player.AddBuff(31, 30);
					Player.AddBuff(23, 30);
				}

			}
		}
	}
}
