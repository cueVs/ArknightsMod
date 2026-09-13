
using ArknightsMod.Systems.Gameplay.Damage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content.NPCs.Enemy.Evolution.Evoution_skill;

namespace ArknightsMod.Content.NPCs.Enemy.Evolution
{
	public class Evolution: ModNPC {
		//計時器方面的東西之後改到ai的第二位置 目前放置如下NPC.ai = [stage,stagetime]
		//交給明天有空的我


		//转阶段参数
		private float Stage = 1;
		private int Stage1Time = 0;
		private int Stage2Time = 0;
		private int Stage3Time = 0;
		private int Stage1MaxTime = 60; // 一分钟进化
		private int Stage2MaxTime = 60; // 一分钟进化
		private int Stage2ImmuneTime = 10; // 10秒免疫伤害
		private int Stage3ImmuneTime = 10; // 10秒免疫伤害
		//数值参数
		private int defense = 50;
		private int SpellResist = 50; // 法术抗性(填明日方舟里的法抗）
		private int Health = 5000;
		private int AttackDamage1 = 0;//接触伤害
		private int AttackDamage2 = 60;//射弹伤害
		private int AttackDamage3 = 80;//真实伤害
		//动画参数
		private int frameNumber = 70; //一共多少帧
		private int Stage1Frame = 10; //第一阶段最后一帧(-1)
		private int Change12Frame = 19; //一转二阶段最后一帧(-1)
		private int Stage2Frame = 29; //第二阶段最后一帧(-1)
		private int Change23Frame = 46; //二转三阶段最后一帧(-1)
		private int Stage3Frame = 56;//三阶段最后一帧(-1)
		private int frameSpeed = 7; //帧速率
		private int framecounter = 0; //帧计数器
		private int fadeTimer = 60;
		private int LeftShield = 0;
		private int RightShield = 0;
		private int AllShield = 0;

		// 範圍圈的攻擊
		private int roundDamge_S1 = 100; // 階段一的全圖攻擊傷害
		private int roundDamge_S2 = 200; // 階段一的全圖攻擊傷害
		private int roundDamge_S3 = 400; // 階段一的全圖攻擊傷害
		private const float Radius = 600f;// 像素
		private const float Thickness = 24f;


		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("不应行于这片大地的狂人，不应存于彼端世界的奇物，二者的结合诞生出了这一超越生物常理的“进化”之节点。它的存在本质与泰拉的一切相悖，它不该活着，它不能活着。"),
			});
		}
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = frameNumber;
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = 1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}
		public override void SetDefaults() {
			NPC.lifeMax = Health;
			NPC.defense = defense;
			NPC.damage = AttackDamage1;
			NPC.scale = 2f;
			NPC.boss = true;
			NPC.knockBackResist = 0;
			// #1 这个写法相对与泰拉瑞亚/tml而言是错误的：它并不等价于NPC.ai[0] = Stage;，而是等价于NPC.ai = new float[1] {Stage};，
			// 也就是给NPC.ai这个字段本身赋值，而NPC.ai这个字段本身不应当被赋值
			// #2 也不要在这个函数里面给NPC.ai[]赋值或判断，本函数仅作为新生成NPC初始化的一部分，
			// 本函数结束后结束后NPC.ai[]会被NPC.NewNPC的参数float ai0/1/2/3 分别赋值
			NPC.ai[0] = Stage;
			Music = MusicLoader.GetMusicSlot("ArknightsMod/Sounds/Music/Evolution");
			NPC.width = 55;
			NPC.height = 100;
			fadeTimer = 60; // 持续60帧
			NPC.color = Color.Black; // 初始为纯黑
			NPC.alpha = 240;
			var genreNPC = NPC.GetGlobalNPC<DamageCategoryNPC>();
			genreNPC.artsResistance = 0.25f;
			NPC.value=100000;
		}

		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
			//if (Main.expertMode || Main.masterMode) {
			//	NPC.lifeMax = (int)(NPC.lifeMax * 0.75);
			//	NPC.damage = (int)(NPC.damage * 0.75);
			//}
			NPC.lifeMax = (int)(NPC.lifeMax * 0.75 * balance * bossAdjustment);
			NPC.damage = (int)(NPC.damage * 0.75 * bossAdjustment);
		}

		public override void FindFrame(int frameHeight) {
			framecounter++;

			switch (NPC.ai[0]) {
				case 1:
					if (framecounter >= frameSpeed) {
						NPC.frame.Y += frameHeight;
						framecounter = 0;
					}
					if (NPC.frame.Y >= Stage1Frame * frameHeight) {
						NPC.frame.Y = 0;
					}
					break;
				case 2:
					if (framecounter >= frameSpeed) {
						NPC.frame.Y += frameHeight;
						framecounter = 0;
					}
					if (NPC.frame.Y <= Stage1Frame * frameHeight) {
						NPC.frame.Y = (Stage1Frame + 1) * frameHeight;
					}
					if (NPC.frame.Y >= Stage2Frame * frameHeight) {
						NPC.frame.Y = (Change12Frame + 1) * frameHeight;
					}
					break;
				case 3:
					if (framecounter >= frameSpeed) {
						NPC.frame.Y += frameHeight;
						framecounter = 0;
					}
					if (NPC.frame.Y <= Stage2Frame * frameHeight) {
						NPC.frame.Y = (Stage2Frame + 1) * frameHeight;
					}
					if (NPC.frame.Y >= Stage3Frame * frameHeight) {
						NPC.frame.Y = (Change23Frame + 1) * frameHeight;
					}
					break;
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D Shield =ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/EvolutionShield").Value;
			Texture2D AllShieldText = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/Evolutionshield3").Value;
			NPC.spriteDirection = NPC.direction;
			if (fadeTimer > 0) {
				fadeTimer--;
			}
			Color drawcolor = Color.Lerp(new Color(0,0,0,65), new Color(255,255,255,255), 1f - fadeTimer / 60f);
			Color LeftShieldcolor = Color.Lerp(new Color(255, 120, 0, 128), new Color(255, 120, 0, 10), 1f - LeftShield / 30f);
			Color RightShieldcolor = Color.Lerp(new Color(255, 120, 0, 200), new Color(255, 120, 0, 70), 1f - RightShield / 30f);
			Color AllShieldColor = Color.Lerp(new Color(255, 255, 255, 200), new Color(0, 0,0, 0), 1f - AllShield / 60f);
			// 动态计算原点（水平居中，底部对齐碰撞箱）
			Vector2 origin1 = new Vector2(110, 110);
			Vector2 origin2 = new Vector2(85, 110);

			if (NPC.spriteDirection > 0) {
				spriteBatch.Draw(
				texture,
				NPC.Center - screenPos + new Vector2(0, 4f), // 整体下移4像素
				NPC.frame,
				drawcolor,
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
				NPC.Center - screenPos + new Vector2(0, 4f), // 整体下移4像素
				NPC.frame,
				drawcolor,
				NPC.rotation,
				origin2,
				NPC.scale,
				NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally,
				0f
				);

			}
			if (LeftShield > 0) {
				spriteBatch.Draw(
				Shield,
				NPC.Center - screenPos + new Vector2(-130, -130f),
				new Rectangle(0,0,Shield.Width, Shield.Height),
				LeftShieldcolor,
				0,
				new Vector2(0, 0),
				1f,
				0f,
				0f
				);
			}
			if (RightShield > 0) {
				spriteBatch.Draw(
				Shield,
				NPC.Center - screenPos + new Vector2(-130, -130f),
				new Rectangle(0, 0, Shield.Width, Shield.Height),
				LeftShieldcolor,
				0,
				new Vector2(0, 0),
				1f,
				SpriteEffects.FlipHorizontally,
				0
				);
			}
			if (AllShield > 0) {
					spriteBatch.Draw(
					AllShieldText,
					NPC.Center - screenPos + new Vector2(-130, -130f),
					new Rectangle(0, 0, AllShieldText.Width, AllShieldText.Height),
					AllShieldColor,
					0,
					new Vector2(0, 0),
					1f,
					0,
					0
					);
			}
			return false;
		}

		public override void OnSpawn(IEntitySource source) {
			// 这几项可移动到SetDefaults()
			//fadeTimer = 60; // 持续60帧
			//NPC.color = Color.Black; // 初始为纯黑
			//NPC.alpha = 240;
			NPC.ai[0] = Stage;
		}
		public override void AI() {
			//动画相关
			if (LeftShield >= 0) {
				LeftShield--;
			}
			if (RightShield >= 0) {
				RightShield--;
			}
			if (AllShield >= 0) {
				AllShield--;
			}
			//控制转阶段
			//預計要改的地方先改參數讓ERROR消失
			if (NPC.ai[0]==1) {
				Stage1Time++;
				if (NPC.life <= NPC.lifeMax * 0.6f || Stage1Time > Stage1MaxTime * 60) {
					NPC.ai[0] = 2;
				}

			}
			if (NPC.ai[0]==2) {
				Stage2Time++;
				if (NPC.life <= NPC.lifeMax * 0.2f || Stage2Time > Stage2MaxTime * 60) {
					NPC.ai[0] = 3;
				}
			}
			if (NPC.ai[0]==3) {
				NPC.width = 170;
				NPC.height = 200;
			}
			NPC.TargetClosest(true);

		}
		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {

			var genreNPC = NPC.GetGlobalNPC<DamageCategoryNPC>();

			if (NPC.ai[0]==1 && (projectile.velocity.X >= NPC.velocity.X || projectile.position.X <= NPC.position.X)) {
				genreNPC.artsResistance = 0.8f;
				RightShield = 30;
			}
			if (NPC.ai[0]==2 && (projectile.velocity.X <= NPC.velocity.X || projectile.position.X >= NPC.position.X)) {
				genreNPC.artsResistance = 0.8f;
				LeftShield = 30;
			}
			if (NPC.ai[0]==3) {
				genreNPC.artsResistance = 0.99f;
				AllShield = 60;
			}

		}
		public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) {
			if (NPC.ai[0]==1 && player.position.X <= NPC.position.X) {
				modifiers.FinalDamage *= 0.2f;
				RightShield = 30;
			}
			if (NPC.ai[0]==2 && player.position.X >= NPC.position.X) {
				modifiers.FinalDamage *= 0.2f;
				LeftShield = 30;
			}
			if (NPC.ai[0]==3) {
				modifiers.FinalDamage *= 0.01f;
				AllShield= 60;
			}
		}
	}
}

