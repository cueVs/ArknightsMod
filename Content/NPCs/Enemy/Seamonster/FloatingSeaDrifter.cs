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
using System;
using ArknightsMod.Content.NPCs.Enemy.ThroughChapter4;
using System.Reflection.Metadata;
using Humanizer;
using Terraria.Audio;
using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Common.Damageclasses;



namespace ArknightsMod.Content.NPCs.Enemy.Seamonster
{ 
	public class FloatingSeaDrifter:ModNPC
	{
		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;
			Player p = Main.player[NPC.target];
			int Startframe = 0;
			int Endframe = 8;
			int Framespeed = 5;
			
				NPC.frameCounter++;
			
			if (NPC.frame.Y == Endframe * frameHeight && NPC.frame.Y < (Endframe + 1) * frameHeight) {
				NPC.frame.Y = Startframe * frameHeight;
			}
			if (NPC.frame.Y >= 14 * frameHeight) {
				NPC.frame.Y = 1 * frameHeight;
			}
			if (NPC.frameCounter > Framespeed) {
				NPC.frame.Y += frameHeight;
				NPC.frameCounter = 0;
			}
			if (inattack == true && attacktime == 1) {
				if (NPC.frame.Y < (Endframe + 1) * frameHeight) {
					NPC.frame.Y = (Endframe + 1) * frameHeight;

				}

			}

		}

		public override void SetDefaults()
		{
			NPC.width = 84;
			NPC.height = 56;
			NPC.damage = 22;
			NPC.defense = 20;
			NPC.lifeMax = 140;
			NPC.HitSound = SoundID.NPCHit2;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 1800;
			NPC.knockBackResist = 0.7f;
			NPC.aiStyle = 0; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic. // Use vanilla zombie's type when executing AI code. (This also means it will try to despawn during daytime)
			AnimationType = -1; // Use vanilla zombie's type when executing animation code. Important to also match Main.npcFrameCount[NPC.type] in SetStaticDefaults.
								// Makes kills of this NPC go towards dropping the banner it's associated with.
								//new int[1] { ModContent.GetInstance<ExampleSurfaceBiome>().Type }; // Associates this NPC with the ExampleSurfaceBiome in Bestiary
			NPC.npcSlots = 2;
			Main.npcFrameCount[Type] = 15;
			NPC.friendly = false;
			NPC.noGravity = true;
			if (Main.expertMode){
				NPC.damage = 19;
			}
		}
		private bool inattack = false;
		private float flytime;
		private float attacktime;
		private float distance;
		private float diffX;
		private float diffY;
		private float ax;
		private float vx;
		private int directionchoose;
		private float angle;
		private int moving=0;
		public override void AI() {
			moving++;
			if (moving >= 600) {
				moving = 0;
			}
			var newSource = NPC.GetSource_FromThis();
			NPC.ai[3]++;
			NPC.TargetClosest(true);
			Player Player = Main.player[NPC.target];
			directionchoose = Player.Center.X - NPC.Center.X >= 0 ? 1 : -1;
			diffX = Player.Center.X - NPC.Center.X;
			diffY = Player.Center.Y - NPC.Center.Y;
			ax = 0.3f;
			distance = (float)Math.Sqrt(Math.Pow(diffX / 16, 2) + Math.Pow(diffY / 16, 2));//到玩家的距离（格数）
			float acceleration = 0.02f;
			float maxSpeed = 2f;

			angle = (float)Math.Atan((Player.Center.Y - NPC.Center.Y) / (Player.Center.X - NPC.Center.X));
			if (NPC.ai[3] >= 1200) {
				NPC.ai[3] = 0;
			}
			if (inattack == false) {
				flytime++;
				attacktime = 0;
			}
			if (inattack == true) {
				attacktime++;
				flytime = 0;
			}
			if (flytime >= 300 && distance < 30) {
				inattack = true;
				NPC.velocity.Y = 0;
				NPC.velocity.X = 0;
			}
			if (attacktime >= 40) {
				inattack = false;
			}
			if (moving < 400) {
				if (inattack == false && diffY > 200) {
					NPC.velocity.Y += acceleration;
					if (NPC.velocity.Y < 0)
						NPC.velocity.Y += acceleration;
					if (NPC.velocity.Y > maxSpeed)
						NPC.velocity.Y = maxSpeed;

				}
				if (inattack == false && diffY <= 200) {
					NPC.velocity.Y -= acceleration;
					if (NPC.velocity.Y > 0)
						NPC.velocity.Y -= acceleration;
					if (NPC.velocity.Y < -maxSpeed)
						NPC.velocity.Y = -maxSpeed;


				}
				if (inattack == false && diffX > 100) {
					NPC.velocity.X = 1.0f;
				}
				if (inattack == false && diffX <= -100) {
					NPC.velocity.X = -1.0f;
				}
			}
			if (moving > 400) {
				if (inattack == false && diffY > 300) {
					NPC.velocity.Y += acceleration;
					if (NPC.velocity.Y < 0)
						NPC.velocity.Y += acceleration;
					if (NPC.velocity.Y > maxSpeed)
						NPC.velocity.Y = maxSpeed;

				}
				if (inattack == false && diffY <= 300) {
					NPC.velocity.Y -= acceleration;
					if (NPC.velocity.Y > 0)
						NPC.velocity.Y -= acceleration;
					if (NPC.velocity.Y < -maxSpeed)
						NPC.velocity.Y = -maxSpeed;


				}
				if (inattack == false && diffX > 0) {
					NPC.velocity.X = -0.6f;
				}
				if (inattack == false && diffX <= 0) {
					NPC.velocity.X = 0.6f;
				}
			}
			if (attacktime == 3) {
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/FSDrifter") with { Volume = 0.5f, Pitch = 0f }, NPC.Center);
			}

			if (attacktime == 20) {
				Projectile.NewProjectile(newSource, NPC.Center, new Vector2(directionchoose * 8f, 0).RotatedBy(angle), ModContent.ProjectileType<FloatingSeaDrifterShoot>(), 12, 0.8f, 0, 0);
			}
		}

		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (SpellDamageConfig.SpellProjectiles.Contains(projectile.type)) {
				// 法术伤害无视护甲
				modifiers.ScalingArmorPenetration += 1f;
				// 伤害减免
				modifiers.FinalDamage *= 0.8f;

			}
		}

		
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.OverworldDayRain.Chance * 0.8f;
		}
		public override void OnKill() {
			int Gore1 = Mod.Find<ModGore>("FloatingSeaDrifter1").Type;
			var entitySource = NPC.GetSource_Death();
			for (int i = 0; i < 2; i++) {
				Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("FloatingSeaDrifter2").Type);
			}
			Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("FloatingSeaDrifter3").Type);
			Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("FloatingSeaDrifter3").Type);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CorruptedRecord>(), 1, 1, 3));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CoagulatingGel>(), 8, 1, 2));

		}
	}	
	public class FloatingSeaDrifterShoot : ModProjectile
	{
		
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 4;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 40;
		}
		public override void SetDefaults() {
			Projectile.width = 26;
			Projectile.height = 12;
			Projectile.aiStyle = 0;
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.timeLeft = 640;
			Projectile.alpha = 0;
			Projectile.damage = 11;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = true;
			if (Main.expertMode) {
				Projectile.damage = 9;
			}
		}


		public override bool PreDraw(ref Color lightColor) {

			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, new Vector2(13, 6), new Color(20, 60, 255), new Color(80, 80, 255), 10f, true);
			return true;
		}

		public override void AI() {
			Projectile.frameCounter++;
			Projectile.velocity = Vector2.Lerp(Projectile.velocity, 0.800f * Projectile.velocity, 0.01f)+new Vector2(0,0.05f);
			Projectile.rotation = Projectile.velocity.ToRotation();
			float frameHeight;
			Dust dust;
			Vector2 position = Projectile.Center + new Vector2(0, 3);
			dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.WaterCandle, 0f, 0f , 50, default(Color),3f);
			dust.noGravity = true;
			
			if (Projectile.frameCounter > 5) {
				Projectile.frame += 1;
				Projectile.frameCounter = 0;
			}
			if (Projectile.frame == 4) {
				Projectile.frame = 2;
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.AddBuff(31, 3 * 60);
		}
	}
}
