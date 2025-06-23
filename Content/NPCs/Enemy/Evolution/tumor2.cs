using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs.Enemy.Evolution
{

	public class tumor2 : ModNPC
	{
		// 是否有实心方块
		private int direction = 1; // 1 for right, -1 for left
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 15;
		}

		public override void SetDefaults() {
			NPC.width = 11;
			NPC.height = 11;
			NPC.scale = 1.8f;
			NPC.damage = 22;
			NPC.defense = 0;
			NPC.lifeMax = 120;
			NPC.HitSound = SoundID.NPCHit8;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 25f;
			NPC.knockBackResist = 0.5f;
			NPC.aiStyle = NPCAIStyleID.Snail;
			NPC.noTileCollide = false;

		}

		public override void AI() {


			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
				NPC.TargetClosest(false);
			}

			if (NPC.ai[3] % 300 == 0) {
				NPC.ai[3] = 0;
				direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
				NPC.direction = direction;

			}
			NPC.velocity.X = direction * 1.8f; // Move towards the player
			if (NPC.collideX) {
				NPC.velocity.Y = 1.4f * NPC.directionY;
			}
			NPC.ai[3]++;
			base.AI();
			Vector2 slimePos = NPC.Center + Main.rand.NextVector2Circular(NPC.width, NPC.height);

			Color slimeColor = Color.Lerp(Color.DarkRed, Color.Purple, 0.3f);
			if (NPC.ai[3] % 4 == 0) {
				Dust slime = Dust.NewDustPerfect(slimePos, DustID.RedMoss);
				slime.scale = Main.rand.NextFloat(0.8f, 1.5f);
				slime.velocity = NPC.velocity * Main.rand.NextFloat(-0.8f, -0.5f);
				slime.noGravity = true;
				slime.color = slimeColor;
			}
		}
		public override void FindFrame(int frameHeight) {
			NPC.spriteDirection = -NPC.direction;

			// This NPC animates with a simple "go from start frame to final frame, and loop back to start frame" rule
			// In this case: 0-1-2-3-0-1-2-3
			int startFrame = 0;
			int finalFrame = 14;
			int frameSpeed = 7;

			if (NPC.velocity.Length() != 0) {
				NPC.frameCounter += 0.6f;
				NPC.frameCounter += NPC.velocity.Length() / 4f; // Make the counter go faster with more movement speed
			}

			if (NPC.frameCounter > frameSpeed) {
				NPC.frameCounter = 0;
				if (NPC.velocity.Length() != 0) {
					NPC.frame.Y += frameHeight;
				}

				if (NPC.frame.Y > finalFrame * frameHeight) {
					NPC.frame.Y = startFrame * frameHeight;
				}
			}
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo) {
			target.AddBuff(ModContent.BuffType<Buffs.Tumor2>(), 240); // Apply Tumor2 buff for 3 seconds
		}
		public override void OnKill() {
			for (int i = 0; i < 15; i++) {

				Vector2 slimePos = NPC.Center + Main.rand.NextVector2Circular(NPC.width, NPC.height);
				Dust slime = Dust.NewDustPerfect(slimePos, DustID.RedMoss);
				Color slimeColor = Color.Lerp(Color.DarkRed, Color.Purple, 0.3f);
				slime.scale = Main.rand.NextFloat(0.8f, 1.5f);
				slime.velocity = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
				slime.noGravity = false;
				slime.color = slimeColor;

			}
		}
	}
}
