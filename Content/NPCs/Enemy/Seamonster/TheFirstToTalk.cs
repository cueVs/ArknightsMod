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
using Mono.Cecil;
using ArknightsMod.Common.VisualEffects;
using Terraria.Graphics.Renderers;



namespace ArknightsMod.Content.NPCs.Enemy.Seamonster
{
	public class TFTTRush : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 40;
		}


		public override void SetDefaults() {
			Projectile.width = 13;
			Projectile.height = 8;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = false;
			Projectile.timeLeft = 240;
			Projectile.alpha = 0;
			Projectile.damage = 15;
			Projectile.light = 0.8f;
			Projectile.friendly = false;
			Projectile.hostile = true;

		}
		private int flytime;
		private Vector2 axis;
		private Vector2 unaxis;
		public override void AI() {
			flytime++;
			if (flytime == 1) {
				axis = Projectile.velocity.RotatedBy(-MathHelper.Pi / 6).SafeNormalize(Vector2.Zero);
				unaxis = axis.RotatedBy(-MathHelper.PiOver2).SafeNormalize(Vector2.Zero);

			}
			if (flytime <= 160) {
				Projectile.velocity += unaxis * 0.35f + axis * 0.02f;
			}
		}
		public override bool PreDraw(ref Color lightColor) {

			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, new Vector2(7, 4), new Color(20, 60, 150), new Color(20, 150, 200), 6f, true);
			return true;
		}
	}
	public class TFTTRush2 : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 40;
		}


		public override void SetDefaults() {
			Projectile.width = 13;
			Projectile.height = 8;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = false;
			Projectile.timeLeft = 240;
			Projectile.alpha = 0;
			Projectile.damage = 15;
			Projectile.light = 0.8f;
			Projectile.friendly = false;
			Projectile.hostile = true;

		}
		private int flytime;
		private Vector2 axis;
		private Vector2 unaxis;
		public override void AI() {
			flytime++;
			if (flytime == 1) {
				axis = Projectile.velocity.RotatedBy(MathHelper.Pi / 6).SafeNormalize(Vector2.Zero);
				unaxis = axis.RotatedBy(MathHelper.PiOver2).SafeNormalize(Vector2.Zero);

			}
			if (flytime <= 160) {
				Projectile.velocity += unaxis * 0.3f + axis * 0.02f;
			}
		}
		public override bool PreDraw(ref Color lightColor) {

			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, new Vector2(7, 4), new Color(20, 60, 150), new Color(20, 150, 100), 6f, true);
			return true;
		}
	}
	public class TFTTSkillshoot : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 40;
		}
		public override void AI() {
			Projectile.velocity *= 0.99f;
		}


		public override void SetDefaults() {
			Projectile.width = 13;
			Projectile.height = 8;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = false;
			Projectile.timeLeft = 240;
			Projectile.alpha = 0;
			Projectile.damage = 15;
			Projectile.light = 0.8f;
			Projectile.friendly = false;
			Projectile.hostile = true;

		}
		private int flytime;
		private Vector2 axis;
		private Vector2 unaxis;

		public override bool PreDraw(ref Color lightColor) {

			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, new Vector2(7, 4), new Color(20, 60, 150), new Color(20, 150, 200), 6f, true);
			return true;
		}
	}

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
		private bool walk = true;
		private bool shoot;
		private bool rush;
		private bool skill;
		private bool leave;
		private int SP = 900;
		private int jumpCD;
		private int shootCD = 200;
		private int rushCD = 400;
		private int rushtime;
		private int shoottime;
		private int skilltime;
		private int walktime;
		private int Framespeed = 10;
		private int extime;
		private bool ask;
		private Vector2 rushd = new Vector2();

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
				if (NPC.frame.Y >= 26 * frameHeight || NPC.frame.Y <= 15 * frameHeight) {
					NPC.frame.Y = 16 * frameHeight;
				}

			}
			if (rush) {
				if (rushtime == 1) {
					NPC.frame.Y = 27 * frameHeight;
				}
				if (NPC.frame.Y >= 38 * frameHeight || NPC.frame.Y <= 26 * frameHeight) {
					NPC.frame.Y = 27 * frameHeight;
				}

			}
			if (skill) {
				if (skilltime == 1) {
					NPC.frame.Y = 39 * frameHeight;
				}
				if (NPC.frame.Y >= 55 * frameHeight && skilltime <= 400) {
					NPC.frame.Y = 50 * frameHeight;
				}
				if (skilltime == 401) {
					NPC.frame.Y = 55 * frameHeight;
				}
				if (NPC.frame.Y >= 64 * frameHeight && skilltime >= 400) {
					NPC.frame.Y = 1 * frameHeight;
				}
			}
		}


		private Vector2 oldpos1;
		private Vector2 oldpos2;
		private Vector2 oldpos3;
		private float texturedirection;
		public override void AI() {
			SP++;
			jumpCD++;
			if (NPC.direction > 0) {
				texturedirection = 0;
			}
			else {
				texturedirection = 1;
			}
			var newSource = NPC.GetSource_FromThis();
			NPC.TargetClosest(true);
			Player p = Main.player[NPC.target];
			int directionchoose = p.Center.X - NPC.Center.X >= 0 ? 1 : -1;
			float diffX = p.Center.X - NPC.Center.X;
			float diffY = p.Center.Y - NPC.Center.Y;
			float distance = (float)Math.Sqrt(Math.Pow(diffX / 16, 2) + Math.Pow(diffY / 16, 2));//到玩家的距离（格数）
			float acceleration = 0.1f;
			float maxSpeed = 2f;
			float angle = (float)Math.Atan((p.Center.Y - NPC.Center.Y) / (p.Center.X - NPC.Center.X));
			Music = MusicLoader.GetMusicSlot("ArknightsMod/Music/TFTT");
			if (NPC.life <= NPC.lifeMax * 0.5 && ask == true) {
				Main.maxRaining = 1.5f;
				Main.StartRain();
				Main.windSpeedTarget = 0.8f;
				Main.cloudAlpha = 0.8f;
			}
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
				if (NPC.position.Y - p.position.Y > 80 && jumpCD > 200) {
					NPC.velocity.Y = -12f;
					jumpCD = 0;
				}
				if (rushCD >= 600) {
					rush = true;
					walk = false;
					rushCD = 0;
				}
				if (shootCD >= 400 && SP <= 1800) {
					shoot = true;
					walk = false;
					shootCD = 0;
				}
				if (SP >= 2000) {
					skill = true;
					walk = false;
					SP = 0;
				}
				if (p.dead) {
					leave = true;
					walk = false;
				}
				if (distance >= 70) {
					rushCD += 6;
					maxSpeed = 4f;
					acceleration = 1;
				}
				if (NPC.life <= 0.5 * NPC.lifeMax) {
					rushCD += 1;
					shootCD += 1;
					SP += 1;
				}
				if (NPC.life <= 0.3 * NPC.lifeMax) {
					rushCD += 1;
					shootCD += 1;
					SP += 1;
				}
				if (NPC.life<=NPC.lifeMax *0.5 && ask == false) {
					NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.position.X+10, (int)NPC.position.Y, ModContent.NPCType<PocketSeaCrawler>());
					NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.position.X -10, (int)NPC.position.Y, ModContent.NPCType<PocketSeaCrawler>());
					ask = true;
					SP += 2000;
				}
			}
			if (rush) {
				walk = false;
				skill = false;
				shoot = false;
				rushtime++;
				if (Main.time %3 == 0) {
					oldpos3 = oldpos2;
					oldpos2 = oldpos1;
					oldpos1 = NPC.position;

				}
				if (rushtime == 1 && NPC.life <= 0.5 * NPC.lifeMax) {
					extime = 1;
				}
				if (rushtime == 3) {
					NPC.velocity.Y = -6f;
					rushd = (p.Center - NPC.Center).SafeNormalize(Vector2.Zero);
				}
				if (rushtime == 25) {
					NPC.velocity = 13 * rushd;
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/TFTTRush") with { Volume = 0.7f, Pitch = 0f }, NPC.Center);
					for (int i = 1; i < 4; i++) {
						Projectile.NewProjectile(newSource, NPC.Center, new Vector2(directionchoose * (8 + i * 2), 0).RotatedBy(angle + MathHelper.Pi / 6f), ModContent.ProjectileType<TFTTRush>(), 17, 0.8f, 0, 0);

					}
					for (int i = 1; i < 4; i++) {
						Projectile.NewProjectile(newSource, NPC.Center, new Vector2(directionchoose * (8 + i * 2), 0).RotatedBy(angle - MathHelper.Pi / 6f), ModContent.ProjectileType<TFTTRush2>(), 17, 0.8f, 0, 0);

					}


				}
				if (rushtime <= 90 && rushtime >= 25)
					if (NPC.collideX) {
						NPC.velocity.X *= 0.96f;
					}
				if (rushtime == 91) {
					NPC.velocity.X = 0;

				}
				if (rushtime >= 21) {
					Dust dust;
					dust = Dust.NewDustDirect(new Vector2(NPC.position.X, NPC.position.Y + 80), NPC.width, NPC.height, DustID.WhiteTorch, 0f, -7f, 50, Color.SkyBlue, 5f);
					dust.noGravity = true;
				}
				if (rushtime >= 100 && extime >= 1) {
					extime = -1;
					rushtime = 2;
				}
				if (rushtime >= 100 && extime < 1) {
					rush = false;
					walk = true;
					extime = 0;
					rushtime = 0;
				}
			}
			if (shoot) {
				walk = false;
				skill = false;
				rush = false;
				shoottime++;
				NPC.velocity.X = 0;
				if (shoottime == 50) {
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/TFTTShoot") with { Volume = 0.7f, Pitch = 0f }, NPC.Center);
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(directionchoose * 25f, 0).RotatedBy(angle), ModContent.ProjectileType<seashoot>(), 18, 0.8f, 0, NPC.whoAmI, NPC.target);
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(15f, 0).RotatedBy(-MathHelper.PiOver4), ModContent.ProjectileType<seashoot>(), 18, 0.8f, 0, NPC.whoAmI, NPC.target);
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(15f, 0).RotatedBy(-(MathHelper.Pi / 4) * 3), ModContent.ProjectileType<seashoot>(), 18, 0.8f, 0, NPC.whoAmI, NPC.target);
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(15f, 0).RotatedBy((MathHelper.Pi / 4) * 3), ModContent.ProjectileType<seashoot>(), 18, 0.8f, 0, NPC.whoAmI, NPC.target);
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(15f, 0).RotatedBy(MathHelper.Pi / 4), ModContent.ProjectileType<seashoot>(), 18, 0.8f, 0, NPC.whoAmI, NPC.target);
				}
				if (shoottime == 99 && NPC.life < 0.5 * NPC.lifeMax) {
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(15f, 0).RotatedBy(-MathHelper.PiOver4), ModContent.ProjectileType<seashoot>(), 18, 0.8f, 0, NPC.whoAmI, NPC.target);
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(15f, 0).RotatedBy(-(MathHelper.Pi / 4) * 3), ModContent.ProjectileType<seashoot>(), 18, 0.8f, 0, NPC.whoAmI, NPC.target);
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(15f, 0).RotatedBy((MathHelper.Pi / 4) * 3), ModContent.ProjectileType<seashoot>(), 18, 0.8f, 0, NPC.whoAmI, NPC.target);
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(15f, 0).RotatedBy(MathHelper.Pi / 4), ModContent.ProjectileType<seashoot>(), 18, 0.8f, 0, NPC.whoAmI, NPC.target);
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(15f, 0).RotatedBy(-MathHelper.PiOver2), ModContent.ProjectileType<seashoot>(), 18, 0.8f, 0, NPC.whoAmI, NPC.target);
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(15f, 0).RotatedBy(-(MathHelper.Pi / 2) * 3), ModContent.ProjectileType<seashoot>(), 18, 0.8f, 0, NPC.whoAmI, NPC.target);
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(15f, 0).RotatedBy((MathHelper.Pi) * 3), ModContent.ProjectileType<seashoot>(), 18, 0.8f, 0, NPC.whoAmI, NPC.target);
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(15f, 0).RotatedBy((MathHelper.Pi) * 2), ModContent.ProjectileType<seashoot>(), 18, 0.8f, 0, NPC.whoAmI, NPC.target);
				}


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
				shoot = false;
				walk = false;
				rush = false;
				dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.WaterCandle, 0f, -10f, 50, Color.Black, 1.4f);
				dust.noGravity = true;
				if(skilltime == 1) {
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/TFTTSkillS") with { Volume = 1f, Pitch = 0f }, NPC.Center);
				}
				if (skilltime == 70) {
					for (int i = 1; i < 4; i++) {
						Projectile.NewProjectile(newSource, NPC.Center - new Vector2(60, -50), new Vector2(1 * (8 + i * 2), 0).RotatedBy(-MathHelper.Pi / 6f), ModContent.ProjectileType<TFTTRush>(), 17, 0.8f, 0, 0);

					}
					for (int i = 1; i < 4; i++) {
						Projectile.NewProjectile(newSource, NPC.Center - new Vector2(-60, -50), new Vector2(1 * (8 + i * 2), 0).RotatedBy(7 * MathHelper.Pi / 6f), ModContent.ProjectileType<TFTTRush2>(), 17, 0.8f, 0, 0);

					}
				}
				if (skilltime == 100) {
					for (int i = 1; i < 4; i++) {
						Projectile.NewProjectile(newSource, NPC.Center + new Vector2(0, -40), new Vector2(1 * (11 + i * 2), 0).RotatedBy(i * MathHelper.Pi / 9f), ModContent.ProjectileType<TFTTSkillshoot>(), 17, 0.8f, 0, 0);
					}
					for (int i = 6; i < 9; i++) {
						Projectile.NewProjectile(newSource, NPC.Center + new Vector2(0, -40), new Vector2(1 * (11 + (9 - i) * 2), 0).RotatedBy(i * MathHelper.Pi / 9f), ModContent.ProjectileType<TFTTSkillshoot>(), 17, 0.8f, 0, 0);
					}
				}
				if (skilltime == 150) {

					Projectile.NewProjectile(newSource, p.Center - new Vector2(0, 800), new Vector2(21, 0).RotatedBy(MathHelper.Pi / 2f), ModContent.ProjectileType<TFTTShoot>(), 27, 0.8f, 0, 0);
				}
				if (skilltime == 200) {
					Projectile.NewProjectile(newSource, p.Center - new Vector2(0, 800), new Vector2(21, 0).RotatedBy(MathHelper.Pi / 2f), ModContent.ProjectileType<TFTTShoot>(), 27, 0.8f, 0, 0);
				}
				if (skilltime == 250) {
					Projectile.NewProjectile(newSource, p.Center - new Vector2(0, 800), new Vector2(21, 0).RotatedBy(MathHelper.Pi / 2f), ModContent.ProjectileType<TFTTShoot>(), 27, 0.8f, 0, 0);
				}
				if (skilltime == 300) {
					Projectile.NewProjectile(newSource, p.Center - new Vector2(0, 800), new Vector2(21, 0).RotatedBy(MathHelper.Pi / 2f), ModContent.ProjectileType<TFTTShoot>(), 27, 0.8f, 0, 0);
				}
				if (skilltime == 350) {
					Projectile.NewProjectile(newSource, p.Center - new Vector2(0, 800), new Vector2(21, 0).RotatedBy(MathHelper.Pi / 2f), ModContent.ProjectileType<TFTTShoot>(), 27, 0.8f, 0, 0);
				}
				if (skilltime == 400) {
					Projectile.NewProjectile(newSource, p.Center - new Vector2(0, 800), new Vector2(21, 0).RotatedBy(MathHelper.Pi / 2f), ModContent.ProjectileType<TFTTShoot>(), 27, 0.8f, 0, 0);
				}

				if (skilltime == 500) {
					walk = true;
					skill = false;
					skilltime = 0;
					NPC.dontTakeDamage = false;
				}
			}

			if (leave) {
				NPC.noTileCollide = true;
				NPC.velocity.Y *= 100f;

			}



		}
		private int Immuntime;
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor) {
			Texture2D Immuntexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/immortal2").Value;
			
			if (skilltime<70 && skill) {
				Immuntime = (int)((skilltime % 150) / 10);
				spriteBatch.Draw(Immuntexture, NPC.Center + new Vector2(0, 10) - Main.screenPosition, new Rectangle(0, Immuntime * 61, 64, 61), Color.White, 0, new Vector2(32, 30), new Vector2(3, 1.5f), 0, 0);
			}
			if (skilltime>=70 && skilltime < 80) {
				Immuntime = (int)((skilltime % 150) / 10);
				spriteBatch.Draw(Immuntexture, NPC.Center + new Vector2(0, -30) - Main.screenPosition, new Rectangle(0, Immuntime * 61, 64, 61), Color.White, 0, new Vector2(32, 30), new Vector2(3, 1.5f), 0, 0);
			}
			if (skilltime>=80 && skilltime<410) {
				
				Immuntime = (int)((skilltime % 150) / 10);
				spriteBatch.Draw(Immuntexture,NPC.Center+new Vector2(0,-60)-Main.screenPosition,new Rectangle(0,Immuntime*61,64,61),Color.White,0,new Vector2(32,30),new Vector2(3,1.5f),0,0);
			}
			if (skilltime >= 420 && skill) {
				Immuntime = (int)((skilltime % 150) / 10);
				spriteBatch.Draw(Immuntexture, NPC.Center + new Vector2(0, 10) - Main.screenPosition, new Rectangle(0, Immuntime * 61, 64, 61), Color.White, 0, new Vector2(32, 30), new Vector2(3, 1.5f), 0, 0);
			}
			if (skilltime >= 410 && skilltime < 420) {
				Immuntime = (int)((skilltime % 150) / 10);
				spriteBatch.Draw(Immuntexture, NPC.Center + new Vector2(0, -30) - Main.screenPosition, new Rectangle(0, Immuntime * 61, 64, 61), Color.White, 0, new Vector2(32, 30), new Vector2(3, 1.5f), 0, 0);
			}
			
		}

		//public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor) {
		//Texture2D texturerush = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/Seamonster/TheFirstToTalk").Value;
		//if (rush && texturedirection == 0) {
		//spriteBatch.Draw(texturerush, oldpos2 - Main.screenPosition + new Vector2(30,30), new Rectangle(0, NPC.frame.Y, 109, 99), lightColor, 0, new Vector2(55, 50), new Vector2(3, 2f), SpriteEffects.FlipHorizontally, 0);
		//}
		//if (rush && texturedirection == 1) {
		//spriteBatch.Draw(texturerush, oldpos2 - Main.screenPosition + new Vector2(30, 30), new Rectangle(0, NPC.frame.Y, 109, 99), lightColor, 0, new Vector2(55, 50), new Vector2(3, 2f), 0, 0);
		//}
		//return true;
		//}
		public override void OnKill() {			
			var entitySource = NPC.GetSource_Death();
			Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-1, 1), Main.rand.Next(-12, 12)), Mod.Find<ModGore>("TFTTGore1").Type,2);			
			Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("TFTTGore2").Type,2);
			Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("TFTTGore3").Type,2);
		}
	}
		public class seashoot : ModProjectile
		{

			public override void SetStaticDefaults() {
				Main.projFrames[Projectile.type] = 1;
				ProjectileID.Sets.TrailingMode[Type] = 2;
				ProjectileID.Sets.TrailCacheLength[Type] = 40;
			}


			public override void SetDefaults() {
				Projectile.width = 13;
				Projectile.height = 8;
				Projectile.aiStyle = 0;
				Projectile.penetrate = -1;
				Projectile.tileCollide = false;
				Projectile.ignoreWater = false;
				Projectile.timeLeft = 400;
				Projectile.alpha = 0;
				Projectile.damage = 15;
				Projectile.light = 0.8f;
				Projectile.friendly = false;
				Projectile.hostile = true;
				Projectile.scale = 1;
			}
			private int flytime;
			private Vector2 shootd;
			public override void AI() {
				NPC npc = Main.npc[(int)Projectile.ai[0]];
				Player p = Main.player[(int)Projectile.ai[1]];
				flytime++;
				if (flytime <= 20) {
					Projectile.velocity *= 0.98f;
				}
				if (flytime == 20) {
					shootd = (p.position - Projectile.position).SafeNormalize(Vector2.Zero);
					Projectile.velocity *= 0.001f;
				}
				if (flytime == 60) {
					Projectile.velocity = 10 * shootd;
				}
				if (flytime <= 300 && flytime >= 60) {
					Projectile.velocity *= 1.02f;
				}
				if (flytime >= 300) {
					Projectile.timeLeft = 1;
				}
			}

		public override bool PreDraw(ref Color lightColor) {

				Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
				TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, new Vector2(15, 15), new Color(0, 70, 50), new Color(0, 50, 40), 20f, true);
				return true;
			}
		}



	
	public class TFTTShoot : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 4;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 40;
			Projectile.spriteDirection = Projectile.direction;
			Projectile.rotation = Projectile.velocity.ToRotation();

		}

		public override void SetDefaults() {

			Projectile.width = 52;
			Projectile.height = 24;
			Projectile.aiStyle = 0;
			Projectile.scale = 2f;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = false;
			Projectile.timeLeft = 640;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = true;
			
		}
		public override void AI() {
			Projectile.frameCounter++;
			NPC npc = Main.npc[(int)Projectile.ai[0]];
			Player p = Main.player[(int)Projectile.ai[1]];
			if (Projectile.position.Y >= p.position.Y - 20) {
				Projectile.tileCollide = true;
			}
			if (Projectile.frameCounter > 5) {
				Projectile.frame += 1;
				Projectile.frameCounter = 0;
			}
			if (Projectile.frame == 4) {
				Projectile.frame = 2;
			}
			Projectile.rotation = Projectile.velocity.ToRotation();
			if (Projectile.timeLeft <= 15) {
				Projectile.velocity *= 0.01f;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			var newSource = Projectile.GetSource_FromThis();
			Projectile.tileCollide = false;
			SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/TFTTSkillb") with { Volume = 1f, Pitch = 0f }, Projectile.Center);
			Projectile.hide = true;
			Projectile.velocity = new Vector2(0, 0);
			Projectile.scale = 10f;
			Projectile.timeLeft = 1;
			Dust dust;
			dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch, 0f, -2f, 500, Color.SkyBlue, 5f);
			Projectile.NewProjectile(newSource, Projectile.Center+new Vector2(20,64), new Vector2(0, 0), ModContent.ProjectileType<splitwater2>(), 0, 0);
			return false;

		}
		public override bool PreDraw(ref Color lightColor) {
			SpriteBatch spriteBatch = Main.spriteBatch;
			Texture2D texture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/explode").Value;
			if (Projectile.timeLeft <= 20) {
				spriteBatch.Draw(texture,
				Projectile.Center - Main.screenPosition + new Vector2(50, 26), null, lightColor, 0, texture.Size(), 3, SpriteEffects.None, 0f);
			}

			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/WindTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, new Vector2(50, 26), new Color(20, 60, 255), new Color(80, 80, 255), 10f, true);

			return true;
		}
	
	}
	public class splitwater2 : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 4;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 40;
		}
		public override void SetDefaults() {

			Projectile.width = 100;
			Projectile.height = 100;
			Projectile.aiStyle = 0;
			Projectile.scale = 1.5f;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = false;
			Projectile.timeLeft = 28;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = false;
		}

			
		
		public override void AI() {
			Projectile.frameCounter++;
			if (Projectile.frameCounter > 7) {
				Projectile.frame += 1;
				Projectile.frameCounter = 0;
			}
			
		}
	}
}