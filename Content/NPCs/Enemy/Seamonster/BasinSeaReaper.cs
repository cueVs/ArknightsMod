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
using ArknightsMod;
using MonoMod.Core.Platforms;
using ArknightsMod.Common.Damageclasses;



namespace ArknightsMod.Content.NPCs.Enemy.Seamonster
{
	public class BasinSeaReaper : ModNPC
	{
		public override void SetDefaults() {
			NPC.width = 80;
			NPC.height = 118;
			NPC.damage = 50;
			NPC.defense = 80;
			NPC.lifeMax = 1000;
			NPC.HitSound = SoundID.NPCHit2;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 32000;
			NPC.knockBackResist = 0.3f;
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
		private bool sleep = true;
		private float wondertime;
		private float jumpCD;
		private int status;
		private int direction;
		private float blooding;
		private float acceleration = 0.2f;
		private float maxSpeed = 4f;
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
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/BSReaper") with { Volume = 0.9f, Pitch = 0f }, NPC.Center);
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
				NPC.damage = 0;
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
				NPC.damage = 50;
				if (blooding == 5 && NPC.life >= 20) {
					NPC.life -= 6;
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
					NPC.velocity.Y = -10f;
					jumpCD = 0;
				}
				if (NPC.Center.Y > (Main.player[NPC.target].Center.Y - 10) && jumpCD >= 100) {
					NPC.velocity.Y = -10f;
					jumpCD = 0;
				}
				if (distance <= 15) {
					//Player.CurrentSan -= 2;
					
				}

			}
		}
		public override bool? CanFallThroughPlatforms() {
			Player player = Main.player[NPC.target];
			return (player.position.Y + player.height) - (NPC.position.Y + NPC.height) > 0;
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
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CorruptedRecord>(), 1, 4, 6));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CoagulatingGel>(), 2, 1, 4));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<TransmutedSalt>(), 3, 1, 2));

		}
		private int wakeframe;
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color lightColor) {
			wakeframe = ((int)waketime % 30)/10;
			Texture2D texture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/voice").Value;
			if (awake) {
				spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, new Rectangle(0, wakeframe * 314, 316, 314), lightColor, 0, new Vector2(158, 157), new Vector2(1, 1f), 0, 0);
			}
			return true;
		}
		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			if (SpellDamageConfig.SpellProjectiles.Contains(projectile.type)) {
				// 法术伤害无视护甲
				modifiers.ScalingArmorPenetration += 1f;
				// 0.95倍伤害减免
				modifiers.FinalDamage *= 0.25f;

				for (int i = 0; i < 3; i++) {
					Dust.NewDust(NPC.position, NPC.width, NPC.height,
						DustID.Shadowflame, 0, 0, 150, Color.LightBlue, 0.7f);
				}
			}
		}
	}
}