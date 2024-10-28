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
using Terraria.Audio;
using System.Reflection.Metadata;



namespace ArknightsMod.Content.NPCs.Enemy.Seamonster
{
	[AutoloadBossHead]
	public class TheFirstToTalk : ModNPC
	{
		public override void SetDefaults() {
			NPC.width = 60;
			NPC.height = 60;
			NPC.damage = 48;
			NPC.scale = 2f;
			NPC.defense = 12;
			NPC.lifeMax = 5500;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 60000;
			NPC.knockBackResist = 0f;
			NPC.aiStyle = -1; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic. // Use vanilla zombie's type when executing AI code. (This also means it will try to despawn during daytime)
			AnimationType = -1;
			NPC.npcSlots = 5;
			NPC.boss = true;
			NPC.lavaImmune = true;
			Main.npcFrameCount[Type] = 64;
			NPC.friendly = false;
			NPC.noGravity = false;
			if (Main.expertMode || Main.masterMode) {
				NPC.lifeMax = (int)(NPC.lifeMax * 0.8);
				NPC.damage = (int)(NPC.damage * 0.8);
			}
			
		}
		private int walkframe = 16;
		private int shootframe = 27;
		private int rushframe = 39;
		private int skillstart = 50;
		private int skillframe = 55;
		private int skillend = 64;
		private bool walk=true;
		private bool shoot;
		private bool rush;
		private bool skill;
		private bool leave;
		private int SP=900;
		private int jumpCD;
		private int shootCD = 200;
		private int rushCD = 400;
		private int rushtime;
		private int shoottime;
		private int skilltime;
		private int walktime;
		private int Framespeed = 10;

		private Vector2 rushd = new Vector2();
		private bool HoleBelow() {
			//width of npc in tiles
			int tileWidth = 4;
			int tileX = (int)(NPC.Center.X / 16f) - tileWidth;
			if (NPC.velocity.X > 0) //if moving right
			{
				tileX += tileWidth;
			}
			int tileY = (int)((NPC.position.Y + NPC.height) / 16f);
			for (int y = tileY; y < tileY + 2; y++) {
				for (int x = tileX; x < tileX + tileWidth; x++) {
					if (Main.tile[x, y].HasTile) {
						return false;
					}
				}
			}
			return true;

		}
		public override void FindFrame(int frameHeight) {
			NPC.TargetClosest(true);
			NPC.spriteDirection = -NPC.direction;
			Player p = Main.player[NPC.target];
			NPC.frameCounter++;
			if (NPC.frameCounter >= Framespeed) {
				NPC.frame.Y += frameHeight;
				NPC.frameCounter = 0;
			}
			if (walk) {
				if (walktime == 1) {
					NPC.frame.Y = 0;
				}
				if (NPC.frame.Y >= 15 * frameHeight) {
					NPC.frame.Y = 0;
				}
				
			}
			if (shoot) {
				if (shoottime == 1) {
					NPC.frame.Y = 16 * frameHeight;
				}
				if (NPC.frame.Y >= 26 * frameHeight || NPC.frame.Y <= 15*frameHeight) {
					NPC.frame.Y = 16* frameHeight;
				}
				
			}
			if (rush) {
				if (rushtime == 1) {
					NPC.frame.Y = 27 * frameHeight;
				}
				if (NPC.frame.Y >= 38 * frameHeight || NPC.frame.Y <= 26*frameHeight) {
					NPC.frame.Y = 27 * frameHeight;
				}

			}
			if (skill) {
				if (skilltime == 1) {
					NPC.frame.Y = 39 * frameHeight;
				}
				if (NPC.frame.Y >= 55 * frameHeight && skilltime<=400) {
					NPC.frame.Y = 50 * frameHeight;
				}
				if (skilltime == 401) {
					NPC.frame.Y = 55 * frameHeight;
				}
				if (NPC.frame.Y >=64*frameHeight && skilltime >= 400) {
					NPC.frame.Y = 1 * frameHeight;
				}
			}
		}



		public override void AI() {
			SP++;
			jumpCD++;
			NPC.TargetClosest(true);
			Player p = Main.player[NPC.target];
			int directionchoose = p.Center.X - NPC.Center.X >= 0 ? 1 : -1;
			float diffX = p.Center.X - NPC.Center.X;
			float diffY = p.Center.Y - NPC.Center.Y;
			float distance = (float)Math.Sqrt(Math.Pow(diffX / 16, 2) + Math.Pow(diffY / 16, 2));//到玩家的距离（格数）
			float acceleration = 0.1f;
			float maxSpeed = 2f;
			float angle = (float)Math.Atan((p.Center.Y - NPC.Center.Y) / (p.Center.X - NPC.Center.X));
			Music =MusicLoader.GetMusicSlot("ArknightsMod/Music/TFTT");
			if (walk) {
				jumpCD++;
				shootCD++;
				rushCD++;
				if (NPC.Center.X > (p.Center.X + 10)) {
					NPC.velocity.X -= acceleration;
					if (NPC.velocity.X > 0)
						NPC.velocity.X -= acceleration;
					if (NPC.velocity.X < -maxSpeed)
						NPC.velocity.X = -maxSpeed;
				}
				if (NPC.Center.X <= (p.Center.X - 10)) {
					NPC.velocity.X += acceleration;
					if (NPC.velocity.X < 0)
						NPC.velocity.X += acceleration;
					if (NPC.velocity.X > maxSpeed)
						NPC.velocity.X = maxSpeed;
				}
				if (NPC.collideX == true && jumpCD > 100) {
					NPC.velocity.Y = -7f;
					jumpCD = 0;
				}
				if ((NPC.position.Y - p.position.Y > 80 || HoleBelow()) && jumpCD > 100) {
					NPC.velocity.Y = -7f;
					jumpCD = 0;
				}
				if (rushCD >= 480 && distance<=40) {
					rush = true;
					walk = false;
					rushCD = 0;
				}
				if (shootCD >= 300 && SP<= 1200) {
					shoot = true;
					walk = false;
					shootCD = 0;
				}
				if (SP >= 1320) {
					skill = true;
					walk = false;
					SP = 0;
				}
				if (distance>=64 || p.dead) {
					leave = true;
					walk = false;
				}
			}
			if (rush) {
				walk = false;
				skill = false;
				shoot = false;
				rushtime++;
				if (rushtime == 1) {
					NPC.velocity.Y = -6f;
					rushd = (p.Center - NPC.Center).SafeNormalize(Vector2.Zero);
				}
				if (rushtime ==25) {
					NPC.velocity = 13 * rushd;
					
					
				}
				if (rushtime<=90 && rushtime>=25)
					if (NPC.collideX) {
						NPC.velocity.X *= 0.98f;
					}
				if (rushtime == 91) {
					NPC.velocity.X = 0;
					
				}
				if (rushtime >= 21) {
					Dust dust;
					dust = Dust.NewDustDirect(new Vector2(NPC.position.X, NPC.position.Y + 200), NPC.width, NPC.height, DustID.WhiteTorch, 0f, -7f, 50, default(Color), 3f);
					dust.noGravity = true;
				}
				if (rushtime >= 100) {
					rush = false;
					walk = true;
					rushtime = 0;
				}
			}
			if (shoot) {
				walk = false;
				skill = false;
				rush = false;
				shoottime++;
				NPC.velocity.X = 0;
				if (shoottime == 100) {
					shoot = false;
					walk = true;
					shoottime = 0;
				}
			}
			if (skill) {
				skilltime++;
				NPC.dontTakeDamage = true;
				NPC.velocity.X = 0;
				Dust dust;
				dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.WaterCandle, 0f, 0f, 50, default(Color), 3f);
				dust.noGravity = false;
				if (skilltime == 500) {
					walk = true;
					skill = false;
					skilltime = 0;
					NPC.dontTakeDamage = false;
				}

			}
			if (leave) {
				NPC.noTileCollide = true;

			}
		}
		

	}
	

}