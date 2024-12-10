using Terraria;
using Terraria.Localization;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using Microsoft.Xna.Framework;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework.Graphics;
using System.Security.Cryptography.X509Certificates;
using System.Net.Http.Headers;
using Terraria.GameContent.Biomes.Desert;
using System;
using Mono.Cecil;
using System.Reflection.Metadata;
using Terraria.GameContent;

namespace ArknightsMod.Content.NPCs.Enemy.Seamonster
{
	public class PrimalSeaPiercer:ModNPC
	{
		public override void SetDefaults() {
			NPC.width = 50;
			NPC.height = 94;
			NPC.damage = 30;
			NPC.defense = 15;
			NPC.lifeMax = 550;
			NPC.HitSound = SoundID.NPCHit2;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 18000;
			NPC.knockBackResist = 0.2f;
			NPC.aiStyle = -1; // Fighter AI, important to choose the aiStyle that matches the NPCID that we want to mimic. // Use vanilla zombie's type when executing AI code. (This also means it will try to despawn during daytime)
			AnimationType = -1;
			NPC.npcSlots = 4;
			Main.npcFrameCount[Type] = 13;
			NPC.friendly = false;
			NPC.noGravity = false;
			if (Main.expertMode) {
				NPC.lifeMax = (int)(NPC.lifeMax * 0.8);
				NPC.damage = (int)(NPC.damage * 0.8);
			}

		}
		private bool approach=true;
		private bool attack=false;
		private bool leave=false;
		private int attacktime=0;
		private int leavetime=0;
		private int approachtime=0;
		private float jumpCD=0;
		private float distance;
		private float diffX;
		private float diffY;
		private int directionchoose;
		private float angle;
		public override void FindFrame(int frameHeight) {
			NPC.TargetClosest(true);
			NPC.spriteDirection = -NPC.direction;
			Player p = Main.player[NPC.target];
			int Startframe = 0;
			int Endframe = 8;
			int Framespeed = 6;
			NPC.frameCounter++;
			if (NPC.frameCounter >= Framespeed) {
				NPC.frame.Y += frameHeight;
				NPC.frameCounter = 0;
			}
			if (approach == true || leave == true) {
				if (NPC.frame.Y >= 5 * frameHeight) {
					NPC.frame.Y = 0;
				}
			}
			if (attacktime == 1) {
				NPC.frame.Y = 6 * frameHeight;

			}
			if (attack == true) {
				if (NPC.frame.Y >= 12* frameHeight) {
					NPC.frame.Y = 12 * frameHeight;
				}
			}
		}
		public override void AI() {
			NPC.TargetClosest(true);
			var newSource = NPC.GetSource_FromThis();
			Player p = Main.player[NPC.target];
			float acceleration = 0.03f;
			float maxSpeed = 1.0f;
			distance = (float)Math.Sqrt(Math.Pow(diffX / 16, 2) + Math.Pow(diffY / 16, 2));
			diffX = p.Center.X - NPC.Center.X;
			diffY = p.Center.Y - NPC.Center.Y;
			directionchoose = p.Center.X - NPC.Center.X >= 0 ? 1 : -1;
			angle = (float)Math.Atan(((p.Center.Y -Math.Sqrt(Math.Pow(diffX / 4,2)))- NPC.Center.Y) / (p.Center.X - NPC.Center.X));
			if (approach == true) {
				leavetime = 0;
				attacktime = 0;
				jumpCD++;
				if (NPC.Center.X > (p.Center.X + 50)) {
					NPC.velocity.X -= acceleration;
					if (NPC.velocity.X > 0)
						NPC.velocity.X -= acceleration;
					if (NPC.velocity.X < -maxSpeed)
						NPC.velocity.X = -maxSpeed;
				}
				if (NPC.Center.X <= (p.Center.X - 50)) {
					NPC.velocity.X += acceleration;
					if (NPC.velocity.X < 0)
						NPC.velocity.X += acceleration;
					if (NPC.velocity.X > maxSpeed)
						NPC.velocity.X = maxSpeed;
				}
				if (NPC.collideX == true && jumpCD>=100 ) {
					NPC.velocity.Y = -8f;
					jumpCD = 0;
				}
				if(NPC.Center.Y > (p.Center.Y -10)&& jumpCD >= 100){
					NPC.velocity.Y = -8f;
					jumpCD = 0;
				}
				if (distance <= 20) {
					attack = true;
					approach = false;
				}

			}
			if (attack == true) {
				attacktime++;
				NPC.velocity.X = 0;
			}
			if (attacktime >= 150) {
				leave = true;
				attack = false;
				attacktime = 0;
			}
			if (leavetime>=200 && distance <= 25) {
				approach = false;
				attack = true;
				leave = false;
				attacktime++;
				leavetime = 0;
			}
			if (attacktime == 10) {
				Projectile.NewProjectile(newSource, NPC.Center, new Vector2(directionchoose * 12f, 0).RotatedBy(angle), ModContent.ProjectileType<PSPShoot>(), 25, 0.8f, 0, NPC.whoAmI);
			}
			if (leave == true) {
				approach = false;
				attacktime = 0;
				leavetime++;
				jumpCD++;

				if (NPC.Center.X > (p.Center.X + 10)) {
					NPC.velocity.X += acceleration;
					if (NPC.velocity.X < 0)
						NPC.velocity.X += acceleration;
					if (NPC.velocity.X > maxSpeed)
						NPC.velocity.X = maxSpeed;
					
				}
				if (NPC.Center.X <= (p.Center.X - 10)) {
					NPC.velocity.X -= acceleration;
					if (NPC.velocity.X > 0)
						NPC.velocity.X -= acceleration;
					if (NPC.velocity.X < -maxSpeed)
						NPC.velocity.X = -maxSpeed;
				}
				if (NPC.collideX == true && jumpCD >= 100) {
					NPC.velocity.Y = -8f;
					jumpCD = 0;
				}
				if (NPC.Center.Y > (p.Center.Y - 10) && jumpCD >= 100) {
					NPC.velocity.Y = -8f;
					jumpCD = 0;
				}
				if (leavetime >= 300 || distance>=36) {
					approach = true;
					leave = false;
					leavetime = 0;
				}
			}

		}
		public override void OnKill() {
			int Gore1 = Mod.Find<ModGore>("PSP1").Type;
			var entitySource = NPC.GetSource_Death();
			for (int i = 0; i < 2; i++) {
				Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("PSP2").Type);
			}
			Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("PSP3").Type);
			Gore.NewGore(entitySource, NPC.position, new Vector2(Main.rand.Next(-5, 4), Main.rand.Next(-5, 4)), Mod.Find<ModGore>("PSP3").Type);
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			LeadingConditionRule notExpertRule = new LeadingConditionRule(new Conditions.NotExpert());
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CorruptedRecord>(), 1, 2, 5));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TransmutedSalt>(), 3, 1, 3));

		}
		public override bool? CanFallThroughPlatforms() {
			Player player = Main.player[NPC.target];
			return (player.position.Y + player.height) - (NPC.position.Y + NPC.height) > 0;
		}

	}
	public class PSPShoot : ModProjectile
	{

		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 1;
		}
		public override void SetDefaults() {
			Projectile.width = 36;
			Projectile.height = 20;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = false;
			Projectile.timeLeft = 240;
			Projectile.alpha = 0;
			Projectile.damage = 70;
			Projectile.light = 0.1f;
			Projectile.friendly = false;
			Projectile.hostile = true;
			if (Main.expertMode == true) {
				Projectile.damage = (int)(Projectile.damage *0.8f);
			}
		}
		//public override bool PreDraw(ref Color lightColor) {
		//	Color A = new Color(255, 255, 255);
		//	Color B = new Color(235, 235, 235);
		//	Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/FlameTrail").Value;
		//	TrailProjectileMaker.ProjectileDrawTail(Projectile, trailtexture, new Vector2(0, 0), A, B, 15f, true);
		//	return true;
		//}

		public override void PostDraw(Color lightColor) {
			//Texture2D lightsTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/Seamonster/FloatingSeaDrifterShoot").Value;
			//Main.EntitySpriteDraw(lightsTexture, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, lightsTexture.Width, lightsTexture.Height), Color.White, Projectile.rotation, new Vector2(lightsTexture.Width / 2, lightsTexture.Height / 2), 1f, SpriteEffects.None, 0);
		}
		private float flytime = 0;
		private bool back = false;
		private float distance2;
		private float diffX2;
		private float diffY2;
		public override void AI() {
			NPC npc = Main.npc[(int)Projectile.ai[0]];
			distance2 = (float)Math.Sqrt(Math.Pow(diffX2 / 16, 2) + Math.Pow(diffY2 / 16, 2));
			diffX2 = npc.Center.X - Projectile.Center.X;
			diffY2 = npc.Center.Y - Projectile.Center.Y;
			if (back == false) {
				flytime++;
				Projectile.frameCounter++;
				Projectile.velocity = (Projectile.velocity + new Vector2(0, 0.16f));
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.Pi / 6;
				float frameHeight;
				Dust dust;
				Vector2 position = Projectile.Center + new Vector2(0, 3);
				dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.WhiteTorch, 0.1f, 0f, 50, default(Color), 2f);
				dust.noGravity = true;
			}
			if (flytime >= 30) {
				flytime = 0;
				back = true;

			}
			if (back == true) {
				Projectile.tileCollide = false;
				Projectile.velocity = Vector2.Normalize(npc.position - Projectile.position+new Vector2(0,10))*30f;
				if (distance2 <= 3) {
					Projectile.timeLeft = 1;
				}
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			Main.instance.LoadProjectile(Projectile.type);
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

			// Redraw the projectile with the color not influenced by light
			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
			for (int k = 0; k < Projectile.oldPos.Length; k++) {
				Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
			}

			return true;
		}
		public override bool OnTileCollide(Vector2 oldVelocity) {
			
			back = true;
			Projectile.velocity = -Projectile.oldVelocity;
			return false;

		}
		public override void OnKill(int timeLeft) {
			
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			
		}
	}
}
