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
using Mono.Cecil;
using System;
using Terraria.Audio;
using ArknightsMod.Common.VisualEffects;



namespace ArknightsMod.Content.NPCs.Enemy.Seamonster
{
	public class PocketSeaCrawler : ModNPC
	{
		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = NPC.direction;
			Player p = Main.player[NPC.target];
			int Startframe = 1;
			int Endframe = 4;
			int Framespeed = 5;
			if (NPC.ai[3] < 300 || NPC.frame.Y > 5 * frameHeight) {
				NPC.frameCounter++;
			}
			
			
			if (NPC.frameCounter > Framespeed) {
				NPC.frame.Y += frameHeight;
				NPC.frameCounter = 0;
			}
			if (NPC.frame.Y >= 4 * frameHeight) {
				NPC.frame.Y = frameHeight;
			}
		}
		public override void SetDefaults() {
			NPC.width = 60;
			NPC.height = 60;
			NPC.damage = 25;
			NPC.defense = 0;
			NPC.lifeMax = 1000;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 4800;
			NPC.knockBackResist = 0.1f;
			NPC.aiStyle = 0; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic. // Use vanilla zombie's type when executing AI code. (This also means it will try to despawn during daytime)
			AnimationType = -1; // Use vanilla zombie's type when executing animation code. Important to also match Main.npcFrameCount[NPC.type] in SetStaticDefaults.
								// Makes kills of this NPC go towards dropping the banner it's associated with.
								//new int[1] { ModContent.GetInstance<ExampleSurfaceBiome>().Type }; // Associates this NPC with the ExampleSurfaceBiome in Bestiary
			NPC.npcSlots = 3;
			Main.npcFrameCount[Type] = 5;
			NPC.friendly = false;
			NPC.noGravity = false;
			if (Main.expertMode) {
				NPC.lifeMax = (int)(NPC.lifeMax * 0.8);
				NPC.damage = (int)(NPC.damage * 0.8);
			}

		}
		private bool onshoot= false;
		private bool shoot1 = false;
		private bool shoot2 = false;
		private bool shoot3 = false;
		private bool shoot4 = false;
		private bool shoot5 = false;
		private bool shoot6 = false;
		private int shootime;
		private int directionchoose;
		private float angle;

		public override void AI() {

			NPC.ai[3]++;
			var newSource = NPC.GetSource_FromThis();
			
			NPC.TargetClosest(true);
			Player p = Main.player[NPC.target];
			directionchoose = p.Center.X - NPC.Center.X >= 0 ? 1 : -1;
			angle = (float)Math.Atan((p.Center.Y - NPC.Center.Y) / (p.Center.X - NPC.Center.X));
			if (NPC.life <= 0.85*NPC.lifeMax & shoot1==false) {
				onshoot = true;
				shoot1 = true;
			}
			if (NPC.life <= 0.7*NPC.lifeMax & shoot2 == false) {
				onshoot = true;
				shoot2 = true;
			}
			if (NPC.life <= 0.55 * NPC.lifeMax & shoot3 == false) {
				onshoot = true;
				shoot3 = true;
			}
			if (NPC.life <= 0.4 * NPC.lifeMax & shoot4 == false) {
				onshoot = true;
				shoot4 = true;
			}
			if (NPC.life <= 0.25 * NPC.lifeMax & shoot5 == false) {
				onshoot = true;
				shoot5 = true;
			}
			if (NPC.life <= 0.1 * NPC.lifeMax & shoot6 == false) {
				onshoot = true;
				shoot6 = true;
			}
			if (onshoot == true) {
				shootime++;
				if (shootime >= 30) {
					shootime = 0;
					onshoot = false;
				}
			}

			if (NPC.ai[3] <= 300) {
				if (NPC.position.X - p.position.X > 30) {
					NPC.velocity.X = -0.6f;



					

				}
				if (NPC.position.X - p.position.X <= -30) {
					NPC.velocity.X = 0.6f;


					

				}
				if (NPC.collideX) {
					NPC.velocity.Y = -0.6f;
				}

			}
			if (NPC.ai[3] < 400 && NPC.ai[3] > 300) {

				NPC.velocity.X = 0;

			}
			if (NPC.ai[3] >= 400) {
				NPC.ai[3] = 0;
				
			}
			if (shootime == 1) {
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/PSCrawler") with { Volume = 0.7f, Pitch = 0f }, NPC.Center);
			}
			if (shootime == 20) {
				if (Main.expertMode) {
					for (int i = -6; i < 6; i++) {
						Projectile.NewProjectile(newSource, NPC.Center, new Vector2(directionchoose * 5f, 0).RotatedBy(angle + i * MathHelper.Pi / 6f), ModContent.ProjectileType<PocketSeaCrawlerShoot2>(), 17, 0.8f, 0, 0);
					}
				}
				for (int i = -8; i < 8; i++) {
					Projectile.NewProjectile(newSource, NPC.Center, new Vector2(directionchoose * 5f, 0).RotatedBy(angle+i*MathHelper.Pi / 8f), ModContent.ProjectileType<PocketSeaCrawlerShoot>(), 17, 0.8f, 0, 0);
					
				}
			}

			int direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
			NPC.direction = direction;
		}
		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.OverworldDayRain.Chance * 0.6f;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			
			NPC.ai[3] = 200;

		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CorruptedRecord>(), 1, 2, 5));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CoagulatingGel>(), 3, 1, 3));

		}
		public override void HitEffect(NPC.HitInfo hit) {
			for (int i = 0; i < 10; i++) {
				int dustType = DustID.BlueMoss;
				var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);
				dust.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
				dust.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);
				dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.03f);
			}
		}
		public override void OnKill() {
			int Gore1 = Mod.Find<ModGore>("PSCrawler1").Type;
			var entitySource = NPC.GetSource_Death();
			for (int i = 0; i < 3; i++) {
				Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("PSCrawler2").Type);
				Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("PSCrawler3").Type);
			}
			
			Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("PSCrawler4").Type);
		}
		public override bool? CanFallThroughPlatforms() {
			Player player = Main.player[NPC.target];
			return (player.position.Y + player.height) - (NPC.position.Y + NPC.height) > 0;
		}
	}
	public class PocketSeaCrawlerShoot : ModProjectile
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
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.timeLeft = 240;
			Projectile.alpha = 0;
			Projectile.damage = 15;
			Projectile.light = 0.8f;
			Projectile.friendly = false;
			Projectile.hostile = true;
			if (Main.expertMode==true) {
				Projectile.damage = 12;
			}
		}
		//public override bool PreDraw(ref Color lightColor) {
		//	Color A = new Color(255, 255, 255);
		//	Color B = new Color(235, 235, 235);
		//	Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/FlameTrail").Value;
		//	TrailProjectileMaker.ProjectileDrawTail(Projectile, trailtexture, new Vector2(0, 0), A, B, 15f, true);
		//	return true;
		//}

		public override bool PreDraw(ref Color lightColor) {

			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, new Vector2(7, 4), new Color(20, 60, 255), new Color(80, 80, 255), 6f, true);
			return true;
		}

		public override void AI() {
			;
			Projectile.frameCounter++;
			Projectile.velocity = (Projectile.velocity+ new Vector2(0, 0.04f)) * 1.05f;
			Projectile.rotation = Projectile.velocity.ToRotation();
			float frameHeight;
			Dust dust;
			Vector2 position = Projectile.Center + new Vector2(0, 3);
			dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.WaterCandle, 0.1f, 0f, 50, default(Color), 2f);
			dust.noGravity = true;

			
		}
		
	
	}
	public class PocketSeaCrawlerShoot2 : ModProjectile
	{

		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 1;
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 40;
		}
		public override void SetDefaults() {
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.timeLeft = 240;
			Projectile.alpha = 0;
			Projectile.damage = 15;
			Projectile.light = 0.8f;
			Projectile.friendly = false;
			Projectile.hostile = true;
			if (Main.expertMode == true) {
				Projectile.damage = 12;
			}
		}
		//public override bool PreDraw(ref Color lightColor) {
		//	Color A = new Color(255, 255, 255);
		//	Color B = new Color(235, 235, 235);
		//	Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/FlameTrail").Value;
		//	TrailProjectileMaker.ProjectileDrawTail(Projectile, trailtexture, new Vector2(0, 0), A, B, 15f, true);
		//	return true;
		//}

		public override bool PreDraw(ref Color lightColor) {

			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, new Vector2(7, 4), new Color(20, 60, 255), new Color(80, 80, 255), 6f, true);
			return true;
		}
		public override void AI() {
			;
			Projectile.frameCounter++;
			Projectile.velocity = (Projectile.velocity + new Vector2(0, 0.06f)) * 0.995f;
			Projectile.rotation = Projectile.velocity.ToRotation();
			float frameHeight;
			Dust dust;
			Vector2 position = Projectile.Center + new Vector2(0, 3);
			dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.WaterCandle, 0.1f, 0f, 50, default(Color), 1.5f);
			dust.noGravity = true;


		}
	
	}
}


