using ArknightsMod.Content.Items.Material.ReclamAlgor;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using static Terraria.ModLoader.ModContent;
namespace ArknightsMod.Content.NPCs.Enemy.ReclamationAlgorithm
{
	// Party Zombie is a pretty basic clone of a vanilla NPC. To learn how to further adapt vanilla NPC behaviors, see https://github.com/tModLoader/tModLoader/wiki/Advanced-Vanilla-Code-Adaption#example-npc-npc-clone-with-modified-projectile-hoplite
	public class LeporibeastShorthorn : ModNPC
	{
		private int status;
		private float preposition;
		private int direction;
		private bool run = false;
		private bool walk = true;
		private float jumpCD;
		private float runtime;
		private float acceleration = 0.2f;
		private float maxSpeed = 4f;
		private int DefaultFrame = 0;
		private int frameNumber = 21;//总帧数
		private int walkframe = 1;//移动开始帧
		private int walkendframe = 9;//移动结束帧
		private int runframe = 10;//逃跑开始
		private int runendframe = 20;//逃跑结束
		private int FrameSpeed = 6;//帧速率
		private int framecounter = 0;//帧计数器
		private const float runrange = 150;
		public override bool IsLoadingEnabled(Mod mod) {
			return ModContent.GetInstance<MonsterConfig>().EnableOriginiumSlug;
		}

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 21;

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}

		public override void SetDefaults() {
			NPC.width = 40;
			NPC.height = 30;
			NPC.damage = 0;
			NPC.defense = 30;
			NPC.lifeMax = 750;

			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;

			NPC.value = 10000f;
			NPC.knockBackResist = 0.5f;
			NPC.scale = 0.9f;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {

			npcLoot.Add(ItemDropRule.Common(ItemType<RAMeat>(), ModContent.GetInstance<Dropconfig>().DropLS, 1, 2));

		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			if (spawnInfo.Player.ZoneForest&&!spawnInfo.Player.ZoneCorrupt&& !spawnInfo.Player.ZoneHallow && !spawnInfo.Player.ZoneCrimson) {
				return SpawnCondition.OverworldDay.Chance * 0.1f;
			}
			if (spawnInfo.Player.ZoneDesert&&!spawnInfo.Player.ZoneCorrupt&& !spawnInfo.Player.ZoneHallow && !spawnInfo.Player.ZoneCrimson) {
				return SpawnCondition.OverworldDay.Chance * 0.1f; 
			}
			return 0f;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.DayTime,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
				new FlavorTextBestiaryInfoElement("野外常见的草食性野兽，分布范围非常广。有小小的尖角，但并不具备攻击性，受到惊吓时会奋力加速跑跳，以此躲避攻击。肉质非常鲜嫩。"),
			});
		}

		public override void FindFrame(int frameHeight) {
			framecounter++;
			NPC.spriteDirection = NPC.direction;
			if (framecounter >= FrameSpeed) {
				NPC.frame.Y += frameHeight;
				framecounter = 0;
			}
			if (walk & NPC.velocity.X != 0) {
				if (NPC.frame.Y < walkframe * frameHeight || NPC.frame.Y > walkendframe * frameHeight) {
					NPC.frame.Y = walkframe * frameHeight;
				}
			}
			else if (run) {
				if (NPC.frame.Y < runframe * frameHeight || NPC.frame.Y > runendframe * frameHeight) {
					NPC.frame.Y = runframe * frameHeight;
					FrameSpeed = 4;
				}
			}
			else {
				if (NPC.frame.Y < 0 * frameHeight || NPC.frame.Y > DefaultFrame * frameHeight) {
					NPC.frame.Y = 0 * frameHeight;
				}
			}
		}

		public override void AI() {
			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead || !Main.player[NPC.target].active) {
				NPC.TargetClosest();
			}
			if (NPC.life < NPC.lifeMax * 1f) {
				walk = false;
				run = true;
			}
			Player Player = Main.player[NPC.target];
			float distance = Vector2.Distance(NPC.Center, Player.Center);
			if (distance <= runrange) {
				walk = false;
				run = true;
			}
			jumpCD++;
			if (walk) {
				if (NPC.ai[3] % 180 == 0) {
					NPC.ai[3] = 0;
					status = Main.rand.Next(6);
					if (status == 1 || status == 3) {
						NPC.direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
					}
					if (status == 4|| status ==5) {
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
						NPC.velocity.X = 0.9f * NPC.direction;
						break;
					case 4:
						NPC.velocity.X = 0.7f * NPC.direction;
						break;
					case 5:
						NPC.velocity.X = 0f * NPC.direction;
						break;
				}
				if (NPC.collideX) {
					NPC.velocity.Y = 1.2f * NPC.directionY;
				}
				NPC.ai[3]++;
			}
			if (run) {
				NPC.direction = (Main.player[NPC.target].Center.X < NPC.Center.X).ToDirectionInt();
				NPC.velocity.X = 5f * NPC.direction;
				if (NPC.collideX == true && jumpCD >= 60) {
					NPC.velocity.Y = -8f;
					jumpCD = 0;
				}
			}
		}
		public override void HitEffect(NPC.HitInfo hit) {
			// Spawn confetti when this zombie is hit.

			for (int i = 0; i < 10; i++) {
				int dustType = DustID.RedMoss;
				var dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, dustType);

				dust.velocity.X += Main.rand.NextFloat(-0.05f, 0.05f);
				dust.velocity.Y += Main.rand.NextFloat(-0.05f, 0.05f);

				dust.scale *= 1f + Main.rand.NextFloat(-0.03f, 0.03f);
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			NPC.spriteDirection = NPC.direction;
			// 动态计算原点（水平居中，底部对齐碰撞箱）
			Vector2 origin1 = new Vector2(NPC.frame.Width / 2f, NPC.frame.Height - 14);
			Vector2 origin2 = new Vector2(NPC.frame.Width / 2f, NPC.frame.Height - 14);

			if (NPC.spriteDirection > 0) {
				spriteBatch.Draw(
				texture,
				NPC.Center - screenPos + new Vector2(0, +1f), // 整体下移4像素
				NPC.frame,
				drawColor,
				NPC.rotation,
				origin1,
				NPC.scale,
				NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
				0f
				);

			}
			if (NPC.spriteDirection < 0) {
				spriteBatch.Draw(
				texture,
				NPC.Center - screenPos + new Vector2(0, +1f), // 整体下移4像素
				NPC.frame,
				drawColor,
				NPC.rotation,
				origin2,
				NPC.scale,
				NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
				0f
				);

			}
			return false;
		}
	}
}
