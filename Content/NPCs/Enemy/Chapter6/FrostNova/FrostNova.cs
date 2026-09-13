using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Content.BossBars;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ArknightsMod.Content.NPCs.Enemy.Chapter6.FrostNova
{
	[AutoloadBossHead]
	public class FrostNova : ModNPC
	{
		#region 基本属性设置
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 76;
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.BossBestiaryPriority.Add(Type);
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
			NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				CustomTexturePath = "ArknightsMod/Content/NPCs/Enemy/Chapter6/FrostNova/FrostNova_preview",
				PortraitScale = 1f,
				PortraitPositionYOverride = 0f,
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults() {
			NPC.width = 40;
			NPC.height = 58;
			NPC.damage = 0;
			NPC.defense = 10;
			NPC.lifeMax = 20000;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.DoubleJump;
			NPC.knockBackResist = 0f;
			NPC.lavaImmune = true;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.value = Item.buyPrice(gold: 5);
			NPC.boss = true;
			NPC.npcSlots = 10f;
			NPC.aiStyle = -1;
			NPC.BossBar = ModContent.GetInstance<NoBossBar>();
			NPC.dontTakeDamage = true;
			NPC.Opacity = 0f;
		}

		//修改血量不生效，因为出场无敌
		//public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
		//	NPC.lifeMax = (int)(NPC.lifeMax * 0.75f * balance);
		//	NPC.damage = (int)(NPC.damage * 0.8f);
		//}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,

				new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.ArknightsMod.Bestiary.FrostNova"))
			});
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemType<IncandescentAlloyBlock>(), 3, 3, 5));
			npcLoot.Add(ItemDropRule.Common(ItemType<CrystallineCircuit>(), 3, 3, 5));
			npcLoot.Add(ItemDropRule.Common(ItemType<OptimizedDevice>(), 3, 3, 5));
			// 霜星掉落：12 源石锭 & 1000 合成玉
			npcLoot.Add(ItemDropRule.Common(ItemType<global::ArknightsMod.Content.Items.OriginiumIngot>(), 1, 12, 12));
			npcLoot.Add(ItemDropRule.Common(ItemType<global::ArknightsMod.Content.Items.Orundum>(), 1, 1000, 1000));
		}
		#endregion
		#region 自定义血条
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			return false;
		}

		private float Bartimer;
		private float TargetHealthBarLength;
		private float ActualHealthBarLength;
		//自制血条
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D BarTexBot = ModContent.Request<Texture2D>("ArknightsMod/Content/BossBars/FNBossBarBot").Value;
			Texture2D BarTexMed2 = ModContent.Request<Texture2D>("ArknightsMod/Content/BossBars/FNBossBarMed2").Value;
			Texture2D BarTexMed = ModContent.Request<Texture2D>("ArknightsMod/Content/BossBars/FNBossBarMed").Value;
			Texture2D BarTexTop = ModContent.Request<Texture2D>("ArknightsMod/Content/BossBars/FNBossBarTop").Value;
			if (!FNDeathStart) {
				Bartimer++;
				if (Bartimer > 120) {
					Bartimer = 120;
				}
			}
			else {
				Bartimer--;
				if (Bartimer < 0) {
					Bartimer = 0;
				}
			}
			TargetHealthBarLength = BarTexMed.Width * NPC.life / NPC.lifeMax * Bartimer / 120;
			if (ActualHealthBarLength > TargetHealthBarLength / 4) {
				ActualHealthBarLength--;
			}
			if (ActualHealthBarLength < TargetHealthBarLength / 4) {
				ActualHealthBarLength++;
			}
			float fixedscr = Main.screenWidth > 1920 ? ((Main.screenWidth - 1920) / 1920f * 2.75f + 1) * (Main.screenHeight > 1080 ? Main.screenHeight / 1080f : 1) : 1;
			Main.EntitySpriteDraw(BarTexBot, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 + (Main.screenHeight / 2 - 108 * fixedscr) / Main.GameZoomTarget), new Rectangle(0, 0, (int)(BarTexBot.Width), BarTexBot.Height), Color.White * (Bartimer / 180), 0, new Vector2(BarTexBot.Width / 2, BarTexBot.Height / 2), 1 / Main.GameZoomTarget, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(BarTexMed2, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 + (Main.screenHeight / 2 - 108 * fixedscr) / Main.GameZoomTarget), new Rectangle(0, 0, (int)(ActualHealthBarLength * 4), BarTexMed.Height), Color.White * (Bartimer / 120), 0, new Vector2(BarTexMed.Width / 2, BarTexMed.Height / 2), 1 / Main.GameZoomTarget, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(BarTexMed, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 + (Main.screenHeight / 2 - 108 * fixedscr) / Main.GameZoomTarget), new Rectangle(0, 0, (int)(TargetHealthBarLength), BarTexMed.Height), Color.White * (Bartimer / 120), 0, new Vector2(BarTexMed.Width / 2, BarTexMed.Height / 2), 1 / Main.GameZoomTarget, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(BarTexTop, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 + (Main.screenHeight / 2 - 108 * fixedscr) / Main.GameZoomTarget), new Rectangle(0, 0, (int)(BarTexTop.Width), BarTexTop.Height), Color.White * (Bartimer / 30), 0, new Vector2(BarTexTop.Width / 2, BarTexTop.Height / 2), 1 / Main.GameZoomTarget, SpriteEffects.None, 0);
		}
		#endregion
		#region 变量与自定义函数声明
		//时间轴变量
		private float BirthTimer;
		private float ReBirthTimer;
		private float escapetimer;
		private float DeathTimer;
		private float JumpTimer = 0;

		//限制阈变量
		private float FNShaderRingTimer;
		private float FNRevivalShaderRingTimer;
		private float FNShaderRingIntensity;
		private float FNShaderRingOpacity;
		private float FNShaderRingProgress;
		private float FNShaderRingColorR;
		private float FNShaderRingColorG;
		private float FNShaderRingColorB;
		private float FNTPOpacity = 1;

		//阶段变量
		private float FNStage;
		private bool FNBirth = false;
		private bool FNBirthEnd = false;
		private bool FNRevival = false;
		private bool FNRevivalStart = false;
		private bool FNRevivalEnd = false;
		private bool FNEnd = false;
		private bool FNDeathStart = false;

		//跳跃机制变量
		private bool isstuck = false;
		private bool isstand = false;
		private int jumpstage;
		private int jumptimes = -1;
		private float statplrposX;
		private float statplrposY;

		//攻击机制变量
		private float FNATKTimer;
		private bool FNOnATK;
		private bool FNATKFrameInitialized = false;
		private float FNSkillTimer;
		private bool FNOnSkill;
		private bool FNSkillFrameInitialized = false;
		private float FNSkillChoice;
		private int FNAttackTimes=0;

		//封地板机制变量
		private float[] Stage2BorderCoordX = new float[2] { 0, 0 };
		private float deltaX = 0;
		private float slice = 36f;
		private float[] IceFloorCandidate = new float[36];
		private int IndexOfFNBF;
		private int FNBFChecker;
		private int KillPlayerBecauseAllFloorAreBanned = 0;
		private int NumOfBanFloorOneTime = 0;

		//召唤雪怪小队机制变量
		int summontime = Main.masterMode ?4: Main.expertMode ? 3: 2;//召唤总波次
		int summontimes = 0;//当前召唤波次
		int summonCD = 0;
		bool isnpcdefeated;
		int isallnpcdefeated = 0;
		private int iceAltarSpawnCooldown;
		int[] subNPCType = new int[6] { ModContent.NPCType<SnowCaster>(), ModContent.NPCType<SnowSoldier>(), ModContent.NPCType<Oneiros>(), ModContent.NPCType<IceCleaver>(),ModContent.NPCType<SnowSniper>(),ModContent.NPCType<SnowHound>() };
		int SummonTypeChoice=0;
		int LeftSummonCount;
		int CurCount;

		public static int IceAltarType() {
			return ModContent.ProjectileType<BlizzardStorm>();
		}

		private static void ShuffleArray(float[] array) {
			if (array == null || array.Length <= 1)
				return;

			for (int i = array.Length - 1; i > 0; i--) {
				int j = Main.rand.Next(i + 1);
				float temp = array[i];
				array[i] = array[j];
				array[j] = temp;
			}
		}
		#endregion
		public override void AI() {
			#region 初始化与机制小作文
			Player player = Main.player[NPC.target];
			Vector2 disDiff = NPC.Center - player.Center;
			player.buffImmune[BuffID.Chilled] = false;
			player.buffImmune[BuffID.Frozen] = false;

			//霜星的具体阶段比较复杂，有两个标识种类，阶段表示法和状态表示法
			//生成时候是0阶段，FNBirth和FNBirthEnd不生效，然后FNBirth的判定是生成动画播完，FNBirthEnd的判定是机制出完（BirthTimer 达到4秒），生成结束后进入1阶段
			//一阶段没血后锁血进入1.5阶段，同时播放复活动画，FNRevivalStart启用，复活回满血持续10秒，这之间动画播完后结束后FNRevivalEnd生效，回到走路动画
			//复活阶段结束后FNRevival启用，FNRevivalStart和FNRevivalEnd被禁用，进入2阶段
			//二阶段起始前四秒（ReBirthTimer达到四秒）会召唤龙卷风，四秒后进入正常AI，n秒后解除无敌
			//二阶段没血后锁血进入2.5阶段（还没写），后面还没写

			//准确来概况详细过程是：以stage0生成，240帧内BirthTimer++，240后stage改为1，设置标志FNBirthEnd为true，这期间使用的帧图是吟唱部分（56-76）
			//stage1正常状态下是走路的动画，血条空了之后，执行checkdead函数，此时检测到还没有重生（!FNRevival），锁血无敌，阶段变为1.5
			//设置标志FNRevivalStart为true，复活阶段10秒后将stage设置为2，设置标志位FNRevival为true，FNRevivalStart和FNRevivalEnd为false，这期间使用第34-54帧
			//进入2阶段后前300帧召唤龙卷风和源石冰晶，使用吟唱动画（56-76帧），之后解除无敌
			//2阶段血条空了之后执行checkdead函数，发现此时还没有设置FNEnd为true，不执行死亡，锁血无敌，stage变为3，
			//设置标志物FNDeathStart为true，这期间使用第34-38帧，之后FNDeathStart为true时释放粒子效果，设置FNEnd为true，强制检查死亡

			//以上是绘制和阶段判断的机制，接下来是行为模式机制：
			//霜星在卡墙时会尝试跳跃，一共尝试4次，越跳越高，中间第3次跳跃不成功时会存储玩家位置，第四次跳跃若仍不成功则后退并传送
			//以下机制仅为进一步构想，实际未实现：
			//霜星检测到玩家的高度低于自己的高度+50时会每2秒尝试将自己的高度+1（尝试下平台），如果累计到一定尝试次数，第三次不成功的时候会存储玩家位置，第四次仍不成功则传送
			//检测到自己的位置低于玩家的高度+50时候，会开始计时，当计时达到6秒时会存储玩家位置，8秒时进行传送
			//平A攻击前，会提前在预设位置放下一个编号0的不可见预判弹幕，弹幕遍历从那个位置到玩家位置的十个等分点，如果过程中撞墙了则死亡，否则最终附着在玩家的center，此时霜星检测弹幕是否存活
			//如果存活，照常发射弹幕，如果不存活，再生成16个编号为1-16的不可见预判弹幕环绕玩家并在五个等分点内靠近玩家，记录存活情况，如果检测到存活的则从那个位置的那个角度进行发射
			//如果这16个弹幕仍然全部死亡，直接在玩家位置召唤弹幕

			//攻击循环机制：
			//一阶段的攻击方式是：A-A-A-（0）冰环α，上升冰环，交火攻击-A-A-A-（1）冰环β，周身冰环，倒序攻击-A-A-A-（2）冰环α-A-A-A-（3）冰环γ，周身冰环，同时攻击-构成循环
			//二阶段起始保持无敌会召唤2个源石冰晶并以其作为风暴发出点，之后召唤雪怪小队，雪怪小队死亡之前只进行平A，然后给玩家周期性上寒冷debuff，雪怪小队全部死亡后解除无敌
			//两个源石冰晶会把x轴位置传参给霜星，霜星将其存储后进行切割，判断要进行多少次封印地板，每次封印地板的落点为何，之后打乱这个坐标数组，每次往后遍历3个值直到遍历完成
			//本身不携带寒冷buff时站在地板上先赋予寒冷，随后数秒赋予冻结，被攻击也冻结，如果本身就携带寒冷的情况下站在地板上就直接赋予冻结，然后一旦赋予了冻结，进入CD读条，数秒后才能再次造成冻结
			//遍历结束后，霜星会发射一个全屏冲击波秒杀玩家来结束战斗，之后退场
			//二阶段的攻击方式是：AAA-AAA（同时召唤坠冰封印地块）-AAA-（0）两个冰环α（同时间）-AAA-AAA（同时封印）-AAA-（1）冰环β+，周身冰环，绕圈发射-AAA-AAA（同时封印）-
			//AAA-（2）两个冰环α（同时间）-AAA-AAA（同时封印）-AAA-（3）冰环γ＋，周身冰环，同时发射一层，拥有数层-构成循环

			//技能分发弹幕机制：
			//霜星在放技能的时候会把当前阶段传递给技能分发弹幕MKI的ai0，技能次数传给ai1
			//MKI无论如何都会生成两个轨迹弹幕，ai0分别设置为±1，ai1用来同步位置，ai2用于计算绘制显示
			//MKI会读取ai0和ai1的值，做出分发判断，ai0决定是否加强，ai1进行判断，若除以四余0为交火冰环，余1为螺旋冰环，余2为坠冰冰环，余3为辐向冰环，其中余数为奇数（螺旋和辐向）时
			//还会额外读取BOSS的位置进行附着

			//专家模式debuff时间为2倍，大师模式为2.5倍，则基于平A间隔：4.5s，寒冷时长：5s / 6s / 7.5s，冰冻时长：1.5s / 2s / 2.5s
			//封地板：站上去赋予永寒，3秒后冰冻4s / 6s / 7.5s，或者仁慈一点，3s / 4s / 5s

			//最后没按这个写，但是还是把这篇小作文保留下来吧（
			#endregion
			#region 音乐
			if (!Main.dedServ) {
				if (FNStage <= 1) {
					Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/FrostnovaStage1");//一阶段
				}
				else if (FNStage == 1.5f || FNStage == 2) {
					Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/FrostnovaStage2");//二阶段
				}
				else if (FNStage >= 3) {
					Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/FrostnovaDeath");//死亡
				}
			}
			#endregion
			#region 移动方式
			if (!FNRevivalStart && FNBirthEnd && FNStage != 3 && !FNOnATK && !FNOnSkill && (ReBirthTimer == 0 || ReBirthTimer >= 300)) {
				//方向
				float ax = 0.1f;
				float vx = 0.25f;
				int direction = (Main.player[NPC.target].Center.X > NPC.Center.X).ToDirectionInt();
				NPC.direction = direction;
				Vector2 velDiff = NPC.velocity - player.velocity;
				int haltDirectionX = velDiff.X > 0 ? 1 : -1;
				float haltPointX = NPC.Center.X + haltDirectionX * (velDiff.X * velDiff.X) / (2 * ax);
				//撞墙后跳跃准备，跳跃K次后记录玩家位置，K+1次后传送到该位置
				float jumpspeed = jumptimes < 3 ? 4f * (jumptimes + 1) : 0;
				//int antijump = jumptimes == 2 ? -1 : 1;
				//跳跃
				if (NPC.velocity.X == 0 && jumptimes != 4 && !(FNStage == 2 && ReBirthTimer < 240)) {//撞墙，排除二阶段起始
					isstuck = true;

					if (player.Center.X > haltPointX) {
						NPC.velocity.X += /*antijump * */ax;
					}
					else {
						NPC.velocity.X -= /*antijump * */ax;
					}
					NPC.velocity.X = Math.Min(/*antijump * */vx, Math.Max(-vx/* * antijump*/, NPC.velocity.X));
					if (NPC.velocity.Y == 0) {
						jumpstage += 1;
						if (jumpstage >= 1) {
							NPC.velocity.Y -= jumpspeed;
							jumpstage = 0;
							jumptimes += 1;
							if (jumptimes == 2) {
								statplrposX = player.Center.X;
								statplrposY = player.Center.Y;
							}
						}
					}
				}
				else if (jumptimes == 4) {//传送
					JumpTimer++;
					if (JumpTimer <= 30) {
						if (player.Center.X > haltPointX) {
							NPC.velocity.X -= 0.05f * ax;
						}
						else {
							NPC.velocity.X += 0.05f * ax;
						}
					}
					else if (JumpTimer <= 60) {
						NPC.velocity.X = float.Lerp(NPC.velocity.X, 0, 0.01f);
						NPC.Opacity = 2 - JumpTimer / 30;
						FNTPOpacity = NPC.Opacity;
						if (JumpTimer == 40) {
							for (int i = 0; i < 2; i++) {
								Projectile.NewProjectile(null, NPC.Center, new Vector2(1f * NPC.direction, 1f), ProjectileType<FrostNovaJump>(), 0, 0f, -1, 0, 40, 0);
							}
							SoundEngine.PlaySound(SoundID.NPCHit5, NPC.Center);
						}
						if (JumpTimer >= 40) {
							Dust dust = Main.dust[Dust.NewDust(NPC.position + new Vector2(0, NPC.height / 2), NPC.width, NPC.height / 2, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, 0f)];
							dust.noGravity = true;
							dust.fadeIn = 0f;
							dust.scale = 1f;
						}
					}
					else if (JumpTimer <= 90) {
						if (JumpTimer == 90) {
							NPC.Center = new Vector2(statplrposX, statplrposY - 8);
						}
					}
					else if (JumpTimer <= 135) {
						if (JumpTimer == 91) {
							for (int i = 0; i < 2; i++) {
								Projectile.NewProjectile(null, NPC.Center, new Vector2(1f * NPC.direction, 1f), ProjectileType<FrostNovaJump>(), 0, 0f, -1, 0, 40, 0);
							}
							SoundEngine.PlaySound(SoundID.NPCHit5, NPC.Center);
						}
						if (JumpTimer <= 120) {
							Dust dust = Main.dust[Dust.NewDust(NPC.position + new Vector2(0, NPC.height / 2), NPC.width, NPC.height / 2, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, 0f)];
							dust.noGravity = true;
							dust.fadeIn = 0f;
							dust.scale = 1f;
						}
						NPC.Opacity = Math.Min(JumpTimer / 45 - 2, 1);
						FNTPOpacity = NPC.Opacity;
					}
					else {
						JumpTimer = 0;
						jumptimes = -1;
					}
				}
				else {//没有撞墙
					isstuck = false;
					jumptimes = -1;
					jumpstage = 0;
					if (Math.Abs(player.Center.X - NPC.Center.X) > 16) {
						if (player.Center.X > haltPointX) {
							NPC.velocity.X += ax;
						}
						else {
							NPC.velocity.X -= ax;
						}
					}
					else {
						isstand = true;
						NPC.velocity.X = 0;
					}
					NPC.velocity.X = Math.Min(vx, Math.Max(-vx, NPC.velocity.X));
				}
			}
			#endregion
			#region 限制阈和玩家减速、拉回效果,progress为半径，intensity为不透明度，time为形变，opacity为粗细
			if (FNStage == 1) {
				FNShaderRingTimer++;
				//半径、颜色等与阶段有关
				FNShaderRingProgress = Math.Min(FNShaderRingTimer / 60, 0.75f);
				//浅蓝白色
				FNShaderRingColorR = 0.5f * 200 / 255;
				FNShaderRingColorG = 0.5f * (17.5f * (float)Math.Sin(FNShaderRingTimer * Math.PI / 180) + 237.5f) / 255;
				FNShaderRingColorB = 0.5f * 1;
				//不透明度
				if (FNShaderRingTimer <= 30) {
					FNShaderRingIntensity = Math.Min(FNShaderRingTimer / 30, 0.75f);
				}
				else {
					FNShaderRingIntensity = 0.125f * (float)Math.Sin(FNShaderRingTimer * Math.PI / 240) + 0.875f;
				}

				//环完全展开时出环给玩家上减速
				if (FNShaderRingProgress == 0.75f) {
					player.buffImmune[BuffID.Chilled] = false;
					if (disDiff.Length() >= 450f) {//半径为450像素
						player.AddBuff(BuffID.Chilled, Main.masterMode ? 10 : Main.expertMode ? 10 : 15);
					}
				}
			}
			else if (FNStage == 1.5f) {
				//透明化
				FNShaderRingIntensity -= 0.00833f;
				if (FNShaderRingIntensity <= 0) {
					FNShaderRingIntensity = 0;
				}
				//颜色锁定
				FNShaderRingColorR = 0.5f * 200 / 255;
				FNShaderRingColorG = 0.5f * (17.5f * (float)Math.Sin(FNShaderRingTimer * Math.PI / 180) + 237.5f) / 255;
				FNShaderRingColorB = 0.5f * 1;
			}
			else if (FNStage == 2) {
				FNRevivalShaderRingTimer++;
				//半径先略微减小后增加
				FNShaderRingProgress = Math.Min(0.125f * FNRevivalShaderRingTimer / 60 * FNRevivalShaderRingTimer / 60 - 0.125f * FNRevivalShaderRingTimer / 60 + 0.75f, 1);
				//透明度恢复
				if (FNRevivalShaderRingTimer <= 30) {
					FNShaderRingIntensity = Math.Min(FNRevivalShaderRingTimer / 30, 1);
				}
				else {
					FNShaderRingIntensity = 0.125f * (float)Math.Sin(FNRevivalShaderRingTimer * Math.PI / 240) + 0.875f;
				}
				//颜色
				FNShaderRingColorR = (80 * (float)Math.Sin(FNRevivalShaderRingTimer * Math.PI / 180) + 135) / 255;
				FNShaderRingColorG = (90 * (float)Math.Sin(FNRevivalShaderRingTimer * Math.PI / 180) + 145) / 255;
				FNShaderRingColorB = (100 * (float)Math.Sin(FNRevivalShaderRingTimer * Math.PI / 180) + 155) / 255;

				//环完全展开时出环给玩家上减速，并拉回玩家
				//if (FNShaderRingProgress == 1f) {
				player.buffImmune[BuffID.Chilled] = false;
				if (disDiff.Length() >= 600f * FNShaderRingProgress && disDiff.Length() <= 1500f) {//半径为600像素，不超过场地极限
					player.AddBuff(BuffID.Chilled, Main.masterMode ? 20 : Main.expertMode ? 20 : 30);
					player.velocity = disDiff * 0.01f;
				}
				//}
			}
			if (!FNDeathStart) {
				//粗细只和是否死亡有关，与阶段无关
				FNShaderRingOpacity = Math.Min(FNShaderRingTimer / 60, 1);
				//形变和其他因素无关
				if (Main.netMode != NetmodeID.Server && !Terraria.Graphics.Effects.Filters.Scene["FNTwistedRing"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene.Activate("FNTwistedRing", NPC.Center).GetShader().UseColor(FNShaderRingColorR, FNShaderRingColorG, FNShaderRingColorB).UseTargetPosition(NPC.Center + new Vector2(0, 3)).UseIntensity(FNShaderRingIntensity * FNTPOpacity).UseOpacity(FNShaderRingOpacity).UseProgress(FNShaderRingProgress * FNTPOpacity);
					ArknightsMod.FNTwistedRing.Value.Parameters["uTime"].SetValue((float)Main.time / 64);
					//SoundEngine.PlaySound(SoundID.NPCHit5);
				}
				if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["FNTwistedRing"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene.Activate("FNTwistedRing", NPC.Center).GetShader().UseColor(FNShaderRingColorR, FNShaderRingColorG, FNShaderRingColorB).UseTargetPosition(NPC.Center + new Vector2(0, 3)).UseIntensity(FNShaderRingIntensity * FNTPOpacity).UseOpacity(FNShaderRingOpacity).UseProgress(FNShaderRingProgress * FNTPOpacity);
					ArknightsMod.FNTwistedRing.Value.Parameters["uTime"].SetValue((float)Main.time / 64);
				}
			}
			else if (FNDeathStart || escapetimer != 0 || player.dead || player.active == false) {
				FNShaderRingIntensity -= 0.01667f;
				FNShaderRingOpacity -= 0.01667f;
				FNShaderRingProgress -= 0.01667f;

				if (FNShaderRingIntensity <= 0) {
					FNShaderRingIntensity = 0;
				}
				if (FNShaderRingOpacity <= 0) {
					FNShaderRingOpacity = 0;
				}
				if (FNShaderRingProgress <= 0) {
					FNShaderRingProgress = 0;
				}
				if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["FNTwistedRing"].IsActive() && FNShaderRingIntensity == 0 && FNShaderRingOpacity == 0 && FNShaderRingProgress == 0) {
					Terraria.Graphics.Effects.Filters.Scene["FNTwistedRing"].Deactivate();
				}
			}
			#endregion
			#region 玩家死亡后消失
			if (!player.active || player.dead || (player.Center - NPC.Center).Length() > Main.screenWidth) {
				NPC.TargetClosest(false);
				player = Main.player[NPC.target];
				escapetimer++;
				if (escapetimer > 60) {
					for (int i = 0; i < 3; i++) {
						float positionX = NPC.Center.X + Main.rand.NextFloat(-15, 15) * NPC.direction;
						float positionY = NPC.Center.Y + Main.rand.NextFloat(0, 5);
						Vector2 position = new Vector2(positionX, positionY);
						Projectile.NewProjectile(null, position, new Vector2(0, -1f), ProjectileType<FrostNovaJump>(), 0, 0f, -1, 0, 40, 0);
					}
					for (int i = 0; i < 3; i++) {
						float positionX = NPC.Center.X + Main.rand.NextFloat(-15, 15) * NPC.direction;
						float positionY = NPC.Center.Y + Main.rand.NextFloat(0, 5);
						Vector2 position = new Vector2(positionX, positionY);
						Projectile.NewProjectile(null, position, new Vector2(0, -1f), ProjectileType<FrostNovaJump>(), 0, 0f, -1, 0, 40, 0);
					}
					NPC.Center = Vector2.Zero;
					NPC.active = false;
				}
				if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["FNTwistedRing"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene["FNTwistedRing"].Deactivate();
				}
				return;
			}
			#endregion
			#region 生成（0阶段）
			if (!FNBirthEnd) {
				if (BirthTimer < 240) {
					BirthTimer++;
					NPC.damage = 0;
					NPC.dontTakeDamage = true;
					NPC.Opacity = BirthTimer / 120;
					NPC.velocity.X = 0;
					if (BirthTimer == 30) {
						for (int i = 0; i < 3; i++) {
							float positionX = NPC.Center.X + Main.rand.NextFloat(-15, 15) * NPC.direction;
							float positionY = NPC.Center.Y + Main.rand.NextFloat(0, 5);
							Vector2 position = new Vector2(positionX, positionY);
							Projectile.NewProjectile(null, position, new Vector2(0, -1f), ProjectileType<FrostNovaJump>(), 0, 0f, -1, 0, 40, 0);
						}
						for (int i = 0; i < 3; i++) {
							float positionX = NPC.Center.X + Main.rand.NextFloat(-15, 15) * NPC.direction;
							float positionY = NPC.Center.Y + Main.rand.NextFloat(0, 5);
							Vector2 position = new Vector2(positionX, positionY);
							Projectile.NewProjectile(null, position, new Vector2(0, -1f), ProjectileType<FrostNovaJump>(), 0, 0f, -1, 0, 40, 0);
						}
					}
					if (BirthTimer <= 60) {
						Dust dust = Main.dust[Dust.NewDust(NPC.position + new Vector2(0, NPC.height / 2), NPC.width, NPC.height / 2, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, 0f)];
						dust.noGravity = true;
						dust.fadeIn = 0f;
						dust.scale = 1f;
					}
				}
				else {
					FNStage = 1;
					NPC.damage = 12;
					NPC.dontTakeDamage = false;
					NPC.Opacity = 1;
					FNBirthEnd = true;
					//NPC.lifeMax = 20000;
				}
			}
			#endregion
			#region 封印地板数量达到上限时杀死玩家
			if (IndexOfFNBF == 36) {
				KillPlayerBecauseAllFloorAreBanned++;
				if (KillPlayerBecauseAllFloorAreBanned == 105) {
					player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromKey("Mods.ArknightsMod.StatusMessage.FN.AllFloorBanned", player.name)), 1224, 0);
				}
			}
			#endregion
			#region 普通攻击
			if ((FNStage == 1 || (FNStage == 2 && isallnpcdefeated == 2)) && !FNOnSkill) {
				FNATKTimer++;
				if (FNATKTimer >= 270) {//攻击间隔4.5s
					FNATKTimer = 0;
					FNAttackTimes++;
				}
				if (FNATKTimer >= 156) {
					FNOnATK = true;
					NPC.velocity.X = 0;
					if (FNStage == 2) {
						if (FNATKTimer == 176) {
							FNBFChecker += 1;
							SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
							Projectile.NewProjectile(null, NPC.Center + new Vector2(-80f * NPC.direction, -48f), Vector2.Zero, ProjectileType<BlackIceIntro>(), 10, 0.2f, -1, 0);
						}
						if (FNATKTimer == 186 & FNBFChecker % 3 == 2) {
							SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
							NumOfBanFloorOneTime = Main.masterMode ? Main.rand.Next(4, 7) : Main.expertMode ? Main.rand.Next(3, 6) : Main.rand.Next(2, 5);
							int NumOfFNBF = 36 - IndexOfFNBF > 3 ? NumOfBanFloorOneTime : 36 - IndexOfFNBF;
							for (int i = IndexOfFNBF; i < IndexOfFNBF + NumOfFNBF; i++) {
								Vector2 SlicePos = new Vector2(IceFloorCandidate[i], NPC.Center.Y - 120) + new Vector2(Main.rand.NextFloat(-4, 4), Main.rand.NextFloat(-16, 16));
								Projectile.NewProjectile(null, SlicePos, Vector2.Zero, ProjectileType<FNBanFloor>(), 0, 0);
							}
							IndexOfFNBF += NumOfFNBF;
						}
						if (FNATKTimer == 196) {
							SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
							Projectile.NewProjectile(null, NPC.Center + new Vector2(-48f * NPC.direction, -80f), Vector2.Zero, ProjectileType<BlackIceIntro>(), 10, 0.2f, -1, 0);
						}
					}
					if (FNATKTimer == 186) {
						SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
						Projectile.NewProjectile(null, NPC.Center + new Vector2(-64f * NPC.direction, -64f), Vector2.Zero, ProjectileType<BlackIceIntro>(), 10, 0.2f, -1, 0);
					}
				}
				else {
					FNOnATK = false;
					FNATKFrameInitialized = false;
				}
			}
			#endregion
			#region 几乎所有的技能类型（冰环α交火冰环、冰环β螺旋冰环、冰环γ坠冰冰环、冰环δ辐向冰环，以及对应的强化版）
			if ((FNStage == 1 || (FNStage == 2 && isallnpcdefeated == 2)) && !FNOnATK) {
				FNSkillTimer++;
				if (FNSkillTimer >= 1040) {
					FNSkillTimer = 0;
				}
				if (FNSkillTimer >= 480) {
					FNOnSkill = true;

					//所有冰环都会阻挡
					if (disDiff.Length() <= 60f) {//玩家距离霜星太近
						player.AddBuff(BuffID.Chilled, Main.masterMode ? 20 : Main.expertMode ? 20 : 30);
						player.velocity = -disDiff * 0.08f;
					}

					//放射粒子
					for (int i = 0; i < 2; i++) {
						Vector2 dustPos = NPC.Center + new Vector2(Main.rand.NextFloat(12), 0).RotatedByRandom(MathHelper.TwoPi);
						Dust dust = Dust.NewDustPerfect(dustPos, DustType<Dusts.Bosses.FrostNovaDeathDust>(), Velocity: Vector2.Zero, Scale: 0.5f);
						//dust.noGravity = true;
						dust.velocity = (1 * dustPos - 1 * NPC.Center);
					}

					//环状冰环的阻挡功能粒子视觉效果
					if (FNSkillTimer % 10 == 0) {
						for (int i = 0; i < 48; i++) {
							Vector2 speed = Main.rand.NextVector2CircularEdge(1f, 1f);
							Dust d = Dust.NewDustPerfect(NPC.Center + speed * 48, DustID.Snow, speed * 0.1f, Scale: 1f);
							d.noGravity = true;
						}
					}

					NPC.velocity.X = 0;
					if (FNStage == 2 && FNSkillChoice % 4 == 0) {//二阶段要发射3个alpha冰环
						if (FNSkillTimer == 484) {
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/FNIceRingStg2Start") with { Volume = 1f, Pitch = 0f }, NPC.Center);
							Projectile.NewProjectile(null, NPC.Center + new Vector2(0, -48f), Vector2.Zero, ProjectileType<FNIceRingChoice>(), 10, 0.2f, -1, FNStage, FNSkillChoice, -float.Pi / 4);
						}
						if (FNSkillTimer == 494) {
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/FNIceRingStg2Start") with { Volume = 1f, Pitch = 0f }, NPC.Center);
							Projectile.NewProjectile(null, NPC.Center + new Vector2(0, -48f), Vector2.Zero, ProjectileType<FNIceRingChoice>(), 10, 0.2f, -1, FNStage, FNSkillChoice, 0);
						}
						if (FNSkillTimer == 504) {
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/FNIceRingStg2Start") with { Volume = 1f, Pitch = 0f }, NPC.Center);
							Projectile.NewProjectile(null, NPC.Center + new Vector2(0, -48f), Vector2.Zero, ProjectileType<FNIceRingChoice>(), 10, 0.2f, -1, FNStage, FNSkillChoice, float.Pi / 4);
							FNSkillChoice += 1;
						}
					}
					else {
						if (FNSkillTimer == 504) {
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/FNIceRingStg2Start") with { Volume = 1f, Pitch = 0f }, NPC.Center);
							Projectile.NewProjectile(null, NPC.Center + new Vector2(0, -48f), Vector2.Zero, ProjectileType<FNIceRingChoice>(), 10, 0.2f, -1, FNStage, FNSkillChoice, 0);
							FNSkillChoice += 1;
						}
					}
				}
				else {
					FNOnSkill = false;
					FNSkillFrameInitialized = false;
				}
			}
			#endregion
			#region 冰晶祭坛（仅二阶段：雪怪全灭且重生计时满足后周期召唤；风絮前兆 + 场上最多 5 座）
			bool iceAltarSpawnAllowed = FNStage == 2f && isallnpcdefeated == 2 && ReBirthTimer >= 300;
			if (iceAltarSpawnAllowed) {
				if (Main.netMode != NetmodeID.MultiplayerClient) {
					const int iceAltarSpawnInterval = 900;
					iceAltarSpawnCooldown++;
					if (iceAltarSpawnCooldown >= iceAltarSpawnInterval) {
						iceAltarSpawnCooldown = 0;
						if (IceCrystalAltar.CountActiveAltars() < IceCrystalAltar.MaxConcurrentAltars) {
							float spawnX = player.Center.X + Main.rand.NextFloat(-420f, 420f);
							float spawnY = player.Center.Y + Main.rand.NextFloat(-100f, 100f);
							Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ProjectileType<IceAltarSummonWisp>(), 0, 0f, -1, spawnX, spawnY);
						}
					}
				}
			}
			#endregion
			#region 一阶段攻击计数召唤小怪
			/* 0 1雪怪术士+3雪怪士兵
			1 1雪怪术士+2霜牙
			2 2虚幻+2狙击手
			3 1破冰者+2霜牙
			4 3雪怪士兵+2雪怪狙击手   */
			if (FNAttackTimes == 3&&FNStage==1)//一阶段每三次普攻召唤一次小怪
			{
				FNAttackTimes = 0;
				if (SummonTypeChoice>4)
				{
					SummonTypeChoice = 0;
				}
				switch (SummonTypeChoice)
				{
					case 0:
						NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowCaster>());
						for(int i = 0; i < 3; i++) {
							NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowSoldier>());
						}
						break;
					case 1:
						NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowCaster>());
						for(int i = 0; i < 2; i++) {
							NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowHound>());
						}
						break;
					case 2:
						for (int i = 0; i < 2; i++) {
							NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowSniper>());
							NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<Oneiros>());
						}
						break;
					case 3:
						NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<IceCleaver>());
						for(int i = 0; i < 2; i++) {
							NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowHound>());
						}
						break;
					case 4:
						for (int i = 0; i < 3; i++) {
							NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowSoldier>());
						}
						for(int i = 0; i < 2; i++) {
							NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowSniper>());
						}
						break;


				}

				SummonTypeChoice++;
			}
			#endregion
			#region 复活（1.5阶段）
			if (FNRevivalStart) {
				if (FNStage == 1.5f) {
					NPC.velocity.X = 0f;
					DeathTimer++;
					if (DeathTimer <= 320) {
						NPC.life = 1;
					}
					else if (DeathTimer <= 500) {
						NPC.life = (int)((NPC.lifeMax - 1) * Math.Sin((DeathTimer - 320) * Math.PI / 360) + 1);
					}
					else {
						NPC.life = NPC.lifeMax;
					}
					if (DeathTimer == 325) {
						float positionX = NPC.Center.X - 4 * NPC.direction;
						float positionY = NPC.Center.Y - 7;
						Vector2 position = new Vector2(positionX, positionY);
						Projectile.NewProjectile(null, position, Vector2.Zero, ProjectileType<FrostNovaRevival>(), 0, 0f, -1, NPC.direction);
					}
					if (DeathTimer == 411) {
						float positionX = NPC.Center.X - 4 * NPC.direction;
						float positionY = NPC.Center.Y - 4;
						Vector2 position = new Vector2(positionX, positionY);
						Projectile.NewProjectile(null, position, Vector2.Zero, ProjectileType<FrostNovaWhiteRing>(), 0, 0f, -1, NPC.direction);
					}
					if (DeathTimer >= 600) {
						FNStage = 2f;
						FNRevival = true;
						FNRevivalStart = false;
						FNRevivalEnd = false;
						FNOnATK = false;
						FNOnSkill = false;
						FNSkillChoice = 0;
						FNBFChecker = 0;
					}
				}
			}
			#endregion
			#region 复活后召唤龙卷风和雪怪小队并将场地切片
			if (FNStage == 2f) {
				ReBirthTimer++;
				if (summonCD > 600 || summonCD == 0) {
					summonCD = 0;
				}
				else {
					summonCD++;
				}
				isnpcdefeated = true;
				CurCount= 0;
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC SeekForNPCs = Main.npc[i];
					if (SeekForNPCs.active && Array.Exists(subNPCType, x => x == SeekForNPCs.type)) {
						CurCount++;
						isnpcdefeated = false;//判断是否有雪怪小队存活
					}
				}
				if (LeftSummonCount==0) {
					LeftSummonCount = CurCount;
				}
				if (CurCount < LeftSummonCount) {
					Main.NewText("击败支援霜星的雪怪小队，当前雪怪小队剩余数量：" + CurCount + "，剩余召唤次数：" + (summontime - summontimes), 0, 255, 255);
					LeftSummonCount = CurCount;
				}
				if (summonCD==0&&isnpcdefeated&&summontimes<summontime)
				{
					Main.NewText("击败支援霜星的雪怪小队,当前"+(summontimes+1)+"/"+(summontime+1));
					summontimes++;
					isnpcdefeated = false;
					int Choice1,Choice2;
					Choice1= Main.rand.Next(0, 5);
					Choice2=Main.rand.Next(0, 5);
					for (int j=0; j < 2; j++) {
						int randomChoice =j==0?Choice1:Choice2;//四选二
						switch (randomChoice) {
							case 0:
								NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowCaster>());
								for (int i = 0; i < 3; i++) {
									NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowSoldier>());
								}
								break;
							case 1:
								NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowCaster>());
								for (int i = 0; i < 2; i++) {
									NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowHound>());
								}
								break;
							case 2:
								for (int i = 0; i < 2; i++) {
									NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowSniper>());
									NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<Oneiros>());
								}
								break;
							case 3:
								NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<IceCleaver>());
								for (int i = 0; i < 2; i++) {
									NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowHound>());
								}
								break;
							case 4:
								for (int i = 0; i < 3; i++) {
									NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowSoldier>());
								}
								for (int i = 0; i < 2; i++) {
									NPC.NewNPC(Terraria.Entity.GetSource_NaturalSpawn(), (int)NPC.Center.X + (Main.rand.NextBool() ? Main.screenWidth / 2 + Main.rand.Next(0, 160) : -(Main.screenWidth / 2 + Main.rand.Next(0, 160))), (int)(NPC.Center.Y - Main.rand.Next(120, 180)), NPCType<SnowSniper>());
								}
								break;


						}
					}
				}



				if (ReBirthTimer > (Main.masterMode ? 1920f : Main.expertMode ? 1080f : 480f)&&summontimes>=summontime) {
					bool isnpcexist = false;
					for (int i = 0; i < Main.maxNPCs; i++) {
						NPC SeekForNPCs = Main.npc[i];
						if (SeekForNPCs.active && Array.Exists(subNPCType, x => x == SeekForNPCs.type)) {
							isnpcexist = true;
							break;
						}
					}
					if (!isnpcexist) {
						isallnpcdefeated = 1;
					}
				}

				if (ReBirthTimer < 300) {
					NPC.dontTakeDamage = true;
					NPC.velocity.X = 0;
					if (ReBirthTimer == 30) {
						for (int i = -1; i < 3; i += 2) {
							Projectile.NewProjectile(null, NPC.Center - new Vector2(0, NPC.height / 1.5f), Vector2.Zero, ProjectileType<BlizzardStormStarter>(), 10, 0f, -1, i);
						}
					}
					if (BirthTimer <= 60) {
						Dust dust = Main.dust[Dust.NewDust(NPC.position + new Vector2(0, NPC.height / 2), NPC.width, (int)(NPC.height / 1.5f), DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, -5f)];
						dust.noGravity = true;
						dust.fadeIn = 0f;
						dust.scale = 1f;
					}
				}
				else {
					NPC.damage = 24;
					jumptimes = -1;
				}
				if (ReBirthTimer == 1) {
					Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ProjectileType<FNInvincibleEffect>(), 10, 0f, -1, 1800, 1);
					Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ProjectileType<FNInvincibleEffect>(), 10, 0f, -1, 1800, -1);
				}

				if (ReBirthTimer >= 1175 && ReBirthTimer < 1185) {//检测冰晶坐标，开始给场地切块
					for (int i = 0; i < Main.maxProjectiles; i++) {
						Projectile SeekForProjs = Main.projectile[i];
						if (SeekForProjs.active && SeekForProjs.type == IceAltarType()) {
							if (SeekForProjs.ai[0] == -1) {//左侧
								Stage2BorderCoordX[0] = SeekForProjs.Center.X;
							}
							else if (SeekForProjs.ai[0] == 1) {//右侧
								Stage2BorderCoordX[1] = SeekForProjs.Center.X;
							}
						}
					}
				}

				if (ReBirthTimer >= 1190 && ReBirthTimer < 1195) {//计算36个冰晶的横坐标并填入数组
					deltaX = Stage2BorderCoordX[1] - Stage2BorderCoordX[0];
					for (int i = 0; i < (int)slice; i++) {
						IceFloorCandidate[i] = Stage2BorderCoordX[0] + ((i + 0.5f) / slice) * deltaX;
					}
					ShuffleArray(IceFloorCandidate);//打乱冰晶坐标点位
				}

				//可视化切片效果
				//if (ReBirthTimer >= 1200) {
				//	deltaX = Stage2BorderCoordX[1] - Stage2BorderCoordX[0];

				//	for (int i = 0; i <= (int)slice; i++) {//切片可视化（用粒子效果）
				//		Dust.NewDustPerfect(new Vector2(Stage2BorderCoordX[0] + (i / slice) * deltaX, NPC.Bottom.Y), 6, new Vector2(0f, 0f), 0, new Color(255, 255, 255), 1.5f);
				//	}

				//	if (IceFloorCandidate.Length != 0) {//冰晶召唤出来的点坐标
				//		for (int i = 0; i < IceFloorCandidate.Length; i++) {
				//			Vector2 SlicePos = new Vector2(IceFloorCandidate[i], NPC.Center.Y - 120) + new Vector2(Main.rand.NextFloat(12), 0).RotatedByRandom(MathHelper.TwoPi);
				//			Dust IceCore = Dust.NewDustPerfect(SlicePos, 6, new Vector2(0f, 0f), 0, new Color(255, 255, 255), 1.5f);
				//			IceCore.noGravity = true;
				//		}
				//	}
				//}

				if (ReBirthTimer > 1800 && isallnpcdefeated == 1) {//30秒后且所有雪怪小队均被打败后解除无敌
					NPC.dontTakeDamage = false;
					isallnpcdefeated = 2;
				}
			}
			#endregion
			#region 限制阈内寒冷冲击波（第二阶段雪怪小队存活时或低血量）
			if (FNStage == 2 && (isallnpcdefeated == 0 || NPC.life < NPC.lifeMax * (Main.masterMode ? 0.5f : Main.expertMode ? 0.33f : 0.25f))) {
				player.buffImmune[BuffID.Chilled] = false;
				if ((int)ReBirthTimer % (int)(Main.masterMode ? 150 : Main.expertMode ? 180 : 240) == 0) {
					player.AddBuff(BuffID.Chilled, Main.masterMode ? 50 : Main.expertMode ? 50 : 75);
				}
				if ((int)ReBirthTimer % 60 == 0) {
					for (int i = 0; i < 6; i++) {
						Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ProjectileType<FNIceDust>(), 0, 0f, -1, i, (int)ReBirthTimer / 30);
					}
				}
			}
			#endregion
			#region 控制暴风雪
			float stg1rainpercent = 0.25f;
			float stg2rainpercent = 0.9f;

			if (!FNBirthEnd) {
				if (BirthTimer < 240) {
					if (!Main.raining) {
						Main.StartRain();
					}
					Main.cloudAlpha = stg1rainpercent * BirthTimer / 240;
					Main.maxRaining = stg1rainpercent * BirthTimer / 240;
				}
				else {
					if (!Main.raining) {
						Main.StartRain();
					}
					Main.cloudAlpha = stg1rainpercent;
					Main.maxRaining = stg1rainpercent;
				}
			}
			if (FNStage == 1) {
				if (!Main.raining) {
					Main.StartRain();
				}
				Main.cloudAlpha = stg1rainpercent;
				Main.maxRaining = stg1rainpercent;
			}
			else if(FNStage == 1.5f) {
				if (!Main.raining) {
					Main.StartRain();
				}
				Main.cloudAlpha = stg1rainpercent + (stg2rainpercent - stg1rainpercent) * DeathTimer / 600;
				Main.maxRaining = stg1rainpercent + (stg2rainpercent - stg1rainpercent) * DeathTimer / 600;
			}
			else if(FNStage == 2) {
				if (!Main.raining) {
					Main.StartRain();
				}
				Main.cloudAlpha = stg2rainpercent;
				Main.maxRaining = stg2rainpercent;
			}
			else if (FNDeathStart) {
				if (Main.raining) {
					Main.StopRain();
				}
				Main.cloudAlpha = stg2rainpercent * (1 - DeathTimer / 135);
				Main.maxRaining = stg2rainpercent * (1 - DeathTimer / 135);
			}
			#endregion
			#region 死亡
			if (FNDeathStart == true) {
				DeathTimer++;
				NPC.velocity = Vector2.Zero;
				if (DeathTimer == 130) {
					for (int i = 0; i < 3; i++) {
						float positionX = NPC.Center.X + Main.rand.NextFloat(-20, 23) * NPC.direction;
						float positionY = NPC.Center.Y + Main.rand.NextFloat(0, 45);
						Vector2 position = new Vector2(positionX, positionY);
						Projectile.NewProjectile(null, position, new Vector2(0.5f * NPC.direction, -0.3f), ProjectileType<FrostNovaSmoke>(), 0, 0f, -1, 0, 90, 0);
					}
					for (int i = 0; i < 4; i++) {
						float positionX = NPC.Center.X + Main.rand.NextFloat(-34, 16) * NPC.direction;
						float positionY = NPC.Center.Y + Main.rand.NextFloat(-10, 34);
						Vector2 position = new Vector2(positionX, positionY);
						Projectile.NewProjectile(null, position, new Vector2(2.8f * NPC.direction, -0.5f), ProjectileType<FrostNovaSmoke>(), 0, 0f, -1, 0, 80, -1);
					}
					for (int i = 0; i < 4; i++) {
						float positionX = NPC.Center.X + Main.rand.NextFloat(-18, 28) * NPC.direction;
						float positionY = NPC.Center.Y + Main.rand.NextFloat(-29, 16);
						Vector2 position = new Vector2(positionX, positionY);
						Projectile.NewProjectile(null, position, new Vector2(-3f * NPC.direction, 0.7f), ProjectileType<FrostNovaSmoke>(), 0, 0f, -1, 0, 85, 1);
					}
					for (int i = 0; i < 3; i++) {
						float positionX = NPC.Center.X + Main.rand.NextFloat(-10, 33) * NPC.direction;
						float positionY = NPC.Center.Y + Main.rand.NextFloat(10, 36);
						Vector2 position = new Vector2(positionX, positionY);
						Projectile.NewProjectile(null, position, new Vector2(-0.9f * NPC.direction, -0.4f), ProjectileType<FrostNovaSmoke>(), 0, 0f, -1, 0, 85, 1);
					}
				}

				if (DeathTimer >= 135) {
					FNDeathStart = false;
					FNEnd = true;
					NPC.life = 0;
					NPC.checkDead();
				}
			}
			#endregion
		}
		#region 死亡检查机制
		public override bool CheckDead() {
			if (!FNRevival) {
				NPC.dontTakeDamage = true;
				DeathTimer = 0;
				NPC.life = 1;
				FNStage = 1.5f;
				FNRevivalStart = true;
				return false;
			}
			else if (!FNEnd) {
				NPC.dontTakeDamage = true;
				DeathTimer = 0;
				NPC.life = 1;
				NPC.damage = 0;
				FNStage = 3;
				FNDeathStart = true;
				return false;
			}
			return true;
		}

		public override void OnKill() {
			if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["FNTwistedRing"].IsActive()) {
				Terraria.Graphics.Effects.Filters.Scene["FNTwistedRing"].Deactivate();
			}
		}
		#endregion
		#region 帧图绘制
		//帧动画，霜星一共76帧，1~5为走路，6~16为普攻，17~34为坠冰技能，35~55为重生（35~39为死亡），56~76为吟唱
		public override void FindFrame(int frameHeight) {
			int startFrame;
			int finalFrame;
			int frameSpeed;
			NPC.spriteDirection = NPC.direction;
			//走路
			if ((FNStage == 1 || (FNStage == 2 && ReBirthTimer > 300) || FNRevivalEnd) && !FNOnATK && !FNOnSkill) {
				startFrame = 0;
				finalFrame = 4;
				frameSpeed = 5;
				//if (State == ActionState.FastWalk) {
				//	frameSpeed = 4;
				//}
				NPC.frameCounter += 0.5f;
				if (NPC.frameCounter > frameSpeed) {
					NPC.frameCounter = 0;
					if (!isstand) { // 距离过近时站着不动
						NPC.frame.Y += frameHeight;
					}

					if (NPC.frame.Y > finalFrame * frameHeight) {
						NPC.frame.Y = startFrame * frameHeight;
					}
				}
			}
			//普通攻击
			if ((FNStage == 1 || (FNStage == 2 && ReBirthTimer > 300) || FNRevivalEnd) && FNOnATK && FNATKTimer >= 156) {
				startFrame = 5;
				finalFrame = 15;
				frameSpeed = 6;
				if (!FNATKFrameInitialized) {
					NPC.frame.Y = startFrame * frameHeight;
					FNATKFrameInitialized = true;
				}
				else {
					NPC.frameCounter++;
					if (NPC.frame.Y < finalFrame * frameHeight) {
						if (NPC.frameCounter > frameSpeed) {
							NPC.frameCounter = 0;
							NPC.frame.Y += frameHeight;
						}
					}
				}
			}
			//技能
			if ((FNStage == 1 || (FNStage == 2 && ReBirthTimer > 300) || FNRevivalEnd) && FNOnSkill && FNSkillTimer >= 480) {
				startFrame = 16;
				finalFrame = 33;
				frameSpeed = 12;
				if (!FNSkillFrameInitialized) {
					NPC.frame.Y = startFrame * frameHeight;
					FNSkillFrameInitialized = true;
				}
				else {
					NPC.frameCounter++;
					if (FNSkillTimer <= 804 && NPC.frame.Y > 25 * frameHeight) {
						NPC.frame.Y = 18 * frameHeight;
					}
					else if (FNSkillTimer > 804 && FNSkillTimer < 888) {
						NPC.frame.Y = 25 * frameHeight;
					}
					else if (FNSkillTimer == 888) {
						NPC.frame.Y = 26 * frameHeight;
					}
					if (NPC.frame.Y < finalFrame * frameHeight) {
						if (NPC.frameCounter > frameSpeed && NPC.frame.Y != 25 * frameHeight) {
							NPC.frameCounter = 0;
							NPC.frame.Y += frameHeight;
						}
					}
				}
			}
			//重生
			if (FNStage == 1.5f) {
				startFrame = 34;
				finalFrame = 54;
				if (NPC.frame.Y < startFrame * frameHeight) {
					NPC.frame.Y = startFrame * frameHeight;
				}
				if (NPC.frame.Y == 42 * frameHeight) { //在地上多躺一会
					frameSpeed = 180;
					if (NPC.frameCounter == 144) {
						SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/FNRevive1") with { Volume = 0.8f, Pitch = 0f }, NPC.Center);
					}
				}
				else {
					frameSpeed = 15;
				}
				if (NPC.frame.Y == 51 * frameHeight && NPC.frameCounter == 0) {
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/FNRevive2") with { Volume = 1.2f, Pitch = 0f }, NPC.Center);
				}
				NPC.frameCounter++;
				if (NPC.frameCounter > frameSpeed) {
					NPC.frameCounter = 0;
					NPC.frame.Y += frameHeight;

					if (NPC.frame.Y > finalFrame * frameHeight) {
						NPC.frame.Y = finalFrame * frameHeight;
						FNRevivalEnd = true;
						NPC.dontTakeDamage = true;
					}
				}
			}
			//死亡
			if (FNDeathStart) {
				startFrame = 34;
				finalFrame = 38;
				if (NPC.frame.Y < startFrame * frameHeight) {
					NPC.frame.Y = startFrame * frameHeight;
				}

				frameSpeed = 10;
				NPC.frameCounter += 0.5f;
				if (NPC.frameCounter > frameSpeed) {
					NPC.frameCounter = 0;
					NPC.frame.Y += frameHeight;

					if (NPC.frame.Y > finalFrame * frameHeight) {
						NPC.frame.Y = finalFrame * frameHeight;
					}
				}
			}
			//出场吟唱
			if (BirthTimer < 240 || (FNStage == 2 && ReBirthTimer < 300)) {
				startFrame = 55;
				finalFrame = 75;
				frameSpeed = 7;
				if (!FNBirth || (FNStage == 2 && ReBirthTimer < 300)) {
					NPC.frameCounter++;
					//如果超出范围就锁定第一帧
					if (NPC.frame.Y == finalFrame * frameHeight) {
						if (NPC.frameCounter > frameSpeed) {
							FNBirth = true;
						}
					}
					else {//从第56帧开始
						if (NPC.frame.Y < startFrame * frameHeight) {
							NPC.frame.Y = startFrame * frameHeight;
						}
						if (NPC.frameCounter > frameSpeed) {
							NPC.frameCounter = 0;
							NPC.frame.Y += frameHeight;
						}
					}
				}
				else {
					NPC.frameCounter = 0;
					NPC.frame.Y = 0;
				}
			}
		}
		#endregion
	}

	public class FrostNovaJump : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		public override void SetDefaults() {
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 250;
			Projectile.penetrate = -1;
			Projectile.scale = 0.01f;
			Projectile.Opacity = 0f;
			Projectile.hide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.netUpdate = true;
		}

		public override void AI() {
			Projectile.ai[0] += 1;
			if (Projectile.localAI[0] == 0f) {
				Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
				Projectile.localAI[0] = Main.rand.Next(2) * 2 - 1; // 1 or -1
				Projectile.localAI[1] = Main.rand.NextFloat(Projectile.ai[1], Projectile.ai[1] + 40);
				Projectile.localAI[2] = Main.rand.Next(1, 5);
			}
			//NPC npc = Main.npc[Projectile.owner];
			//float positionX = npc.position.X - 10;
			//float positionY = npc.position.Y;
			//if (Projectile.ai[1] > 0f) {
			//	Projectile.velocity.X = Math.Max(Projectile.velocity.X - 0.1f, 0f);
			//}
			//else {
			//	Projectile.velocity.X = Math.Min(Projectile.velocity.X + 0.1f, 0f);
			//}
			//if (Projectile.ai[2] > 0f) {
			//	Projectile.velocity.Y = Math.Max(Projectile.velocity.Y - 0.1f, 0f);
			//}
			//else {
			//	Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.1f, 0f);
			//}

			//Projectile.velocity.X = GetSPV(Projectile.ai[1], Projectile.ai[1] + 20, Projectile.velocity.X, 2);
			//Projectile.velocity.Y = GetSPV(Projectile.ai[2], Projectile.ai[2] + 20, Projectile.velocity.Y, 2);

			if (Projectile.ai[0] >= 250)
				Projectile.Kill();

			FadeInAndOut();
		}

		public void FadeInAndOut() {
			// If last less than 50 ticks — fade in, than more — fade out
			if (Projectile.ai[0] <= Projectile.localAI[1]) {
				// Fade in
				Projectile.Opacity += 0.1f;
				Projectile.scale += 0.05f;
				Projectile.rotation += 0.003f * Projectile.ai[2];
				Projectile.velocity *= 0.95f;
				// Cap
				if (Projectile.Opacity > 1f)
					Projectile.Opacity = 1f;
				if (Projectile.scale > 1f) {
					Projectile.scale = 1f;
				}
				return;
			}

			// Fade out
			Projectile.Opacity -= 0.03f;
			Projectile.velocity *= 0;
			Projectile.rotation += 0.005f * Projectile.ai[2];
			if (Projectile.Opacity < 0)
				Projectile.Opacity = 0;
			//if (Projectile.localAI[1] <= Projectile.ai[0] && Projectile.ai[0] <= (Projectile.localAI[1] + 60f) && Projectile.ai[0] % 10 == 0 && Projectile.localAI[0] == 1) {
			//	Dust dust = Main.dust[Dust.NewDust(Projectile.Left + new Vector2(-30, -25), 50, Projectile.height / 2, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, -2f)];
			//	dust.noGravity = true;
			//	dust.fadeIn = 0f;
			//	dust.scale = 1.5f;
			//}
		}

		public override bool PreDraw(ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);


			Texture2D texture = Request<Texture2D>("ArknightsMod/Assets/GrayScaleTexture/Smoke" + (int)Projectile.localAI[2], AssetRequestMode.ImmediateLoad).Value;
			float opacity = Projectile.Opacity * 0.6f;
			Color color = Color.White * opacity;
			float scale = Projectile.scale;
			Vector2 origin = texture.Size() * 0.5f;
			//float rotation = 2f * (float)Math.PI * Main.rand.NextFloat();
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, origin, scale, SpriteEffects.None, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
	public class FrostNovaRevival : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 14;
			//DrawOriginOffsetY = -40;
		}
		public override void SetDefaults() {
			Projectile.width = 196;
			Projectile.height = 86;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 250;
			Projectile.penetrate = -1;
			//Projectile.scale = 0.01f;
			//Projectile.Opacity = 0f;
			Projectile.hide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.netUpdate = true;
		}

		public override void AI() {
			Projectile.direction = (int)Projectile.ai[0];
			Projectile.spriteDirection = Projectile.direction;

			if (++Projectile.frameCounter >= 14) {
				Projectile.frameCounter = 0;
				if (Projectile.frame < 15) {
					Projectile.frame++;
				}
				else {
					Projectile.Kill();
				}
			}
		}
	}
	public class FrostNovaSmoke : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		public override void SetDefaults() {
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 250;
			Projectile.penetrate = -1;
			Projectile.scale = 0.01f;
			Projectile.Opacity = 0f;
			Projectile.hide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.netUpdate = true;
		}

		public override void AI() {
			Projectile.ai[0] += 1;
			if (Projectile.localAI[0] == 0f) {
				Projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
				Projectile.localAI[0] = Main.rand.Next(2) * 2 - 1; // 1 or -1
				Projectile.localAI[1] = Main.rand.NextFloat(Projectile.ai[1], Projectile.ai[1] + 40);
				Projectile.localAI[2] = Main.rand.Next(1, 5);
			}
			//NPC npc = Main.npc[Projectile.owner];
			//float positionX = npc.position.X - 10;
			//float positionY = npc.position.Y;
			//if (Projectile.ai[1] > 0f) {
			//	Projectile.velocity.X = Math.Max(Projectile.velocity.X - 0.1f, 0f);
			//}
			//else {
			//	Projectile.velocity.X = Math.Min(Projectile.velocity.X + 0.1f, 0f);
			//}
			//if (Projectile.ai[2] > 0f) {
			//	Projectile.velocity.Y = Math.Max(Projectile.velocity.Y - 0.1f, 0f);
			//}
			//else {
			//	Projectile.velocity.Y = Math.Min(Projectile.velocity.Y + 0.1f, 0f);
			//}

			//Projectile.velocity.X = GetSPV(Projectile.ai[1], Projectile.ai[1] + 20, Projectile.velocity.X, 2);
			//Projectile.velocity.Y = GetSPV(Projectile.ai[2], Projectile.ai[2] + 20, Projectile.velocity.Y, 2);

			if (Projectile.ai[0] >= 250)
				Projectile.Kill();

			FadeInAndOut();
		}

		public void FadeInAndOut() {
			// If last less than 50 ticks — fade in, than more — fade out
			if (Projectile.ai[0] <= Projectile.localAI[1]) {
				// Fade in
				Projectile.Opacity += 0.1f;
				Projectile.scale += 0.1f;
				Projectile.rotation += 0.003f * Projectile.ai[2];
				Projectile.velocity *= 0.95f;
				// Cap
				if (Projectile.Opacity > 1f)
					Projectile.Opacity = 1f;
				if (Projectile.scale > 1f) {
					Projectile.scale = 1f;
				}
				return;
			}

			// Fade out
			Projectile.Opacity -= 0.01f;
			Projectile.velocity *= 0;
			Projectile.rotation += 0.005f * Projectile.ai[2];
			if (Projectile.Opacity < 0)
				Projectile.Opacity = 0;
			if (Projectile.localAI[1] <= Projectile.ai[0] && Projectile.ai[0] <= (Projectile.localAI[1] + 60f) && Projectile.ai[0] % 10 == 0 && Projectile.localAI[0] == 1) {
				Dust dust = Main.dust[Dust.NewDust(Projectile.Left + new Vector2(-30, -25), 50, Projectile.height / 2, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, -2f)];
				dust.noGravity = true;
				dust.fadeIn = 0f;
				dust.scale = 1.5f;
			}
		}

		public override bool PreDraw(ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);


			Texture2D texture = Request<Texture2D>("ArknightsMod/Assets/GrayScaleTexture/Smoke" + (int)Projectile.localAI[2], AssetRequestMode.ImmediateLoad).Value;
			float opacity = Projectile.Opacity * 0.6f;
			Color color = Color.White * opacity;
			float scale = Projectile.scale;
			Vector2 origin = texture.Size() * 0.5f;
			//float rotation = 2f * (float)Math.PI * Main.rand.NextFloat();
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, origin, scale, SpriteEffects.None, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
	public class FrostNovaWhiteRing : ModProjectile
	{
		public override string Texture => "ArknightsMod/Assets/GrayScaleTexture/WhiteRing";
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 12;
			//DrawOriginOffsetY = -40;
		}
		public override void SetDefaults() {
			Projectile.width = 1000;
			Projectile.height = 1000;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 250;
			Projectile.penetrate = -1;
			Projectile.scale = 2f;
			//Projectile.Opacity = 0f;
			Projectile.hide = false;
			Projectile.hostile = false;
			Projectile.friendly = false;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.netUpdate = true;
		}

		public override void AI() {
			Projectile.ai[0]++;
			if (Projectile.ai[0] < 60) {
				Projectile.scale -= 0.06f;
				if (Projectile.scale < 0.001f) {
					Projectile.scale = 0.001f;
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			Texture2D texture = Request<Texture2D>("ArknightsMod/Assets/GrayScaleTexture/WhiteRing", AssetRequestMode.ImmediateLoad).Value;
			float opacity = Projectile.Opacity * 0.6f;
			Color color = Color.White * opacity;
			float scale = Projectile.scale;
			Vector2 origin = texture.Size() * 0.5f;
			//float rotation = 2f * (float)Math.PI * Main.rand.NextFloat();
			Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, color, Projectile.rotation, origin, scale, SpriteEffects.None, 0);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}
	}
	//往左右方向抛出的冰球，ai0分别设置为-1和1（由霜星发射时判定），同值传递给Storm
	public class BlizzardStormStarter : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 60;
		}

		public override void SetDefaults() {
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 6000;
			Projectile.alpha = 0;
			Projectile.damage = 20;
			Projectile.light = 1f;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.scale = 1f;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			var newSource = Projectile.GetSource_FromThis();
			Projectile.NewProjectile(newSource, Projectile.Center - new Vector2(0, 14), Vector2.Zero, ModContent.ProjectileType<BlizzardStorm>(), Projectile.damage, 0f, 0, Projectile.ai[0]);
			SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/FNSummonIceAltar") with { Volume = 0.8f, Pitch = 0f }, Projectile.Center);
			return true;
		}

		private float timer;
		private int r;
		private int g;
		private float move1X;
		private float move1Y;
		private float oldPosX;
		private float oldPosY;

		//碰上就冻结
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.buffImmune[BuffID.Frozen] = false;
			if (!target.HasBuff(BuffID.Frozen)) {
				target.AddBuff(BuffID.Frozen, Main.masterMode ? 60 : Main.expertMode ? 60 : 90);
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/Frozen") with { Volume = 1f, Pitch = 0f }, target.Center);
			}
		}

		public static int HostNPCType() {
			return ModContent.NPCType<FrostNova>();
		}

		public override void AI() {
			var newSource = Projectile.GetSource_FromThis();
			timer++;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC SeekForNPCs = Main.npc[i];
				if (SeekForNPCs.active && SeekForNPCs.type == HostNPCType()) {
					if (SeekForNPCs.life == 1) {
						Projectile.timeLeft = 0;
					}
					else {
						Projectile.timeLeft = 60;
					}
				}
			}
			r = 235 + 20 * (int)MathF.Sin(timer * MathHelper.Pi / 180);
			g = 245 + 10 * (int)MathF.Sin(timer * MathHelper.Pi / 180);

			if (Main.rand.NextFloat() < 0.25f) {
				Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Bosses.FrostNovaDeathDust>(), Vector2.Zero, 120, new Color(255, 255, 255), 0.5f);
			}

			float uptimer = 60;
			float spintimer = 500;
			float lerptimer = 30;

			if (timer == uptimer) {
				oldPosX = Projectile.Center.X;
				oldPosY = Projectile.Center.Y;
			}
			if (timer == uptimer + spintimer) {
				Projectile.velocity.X = Projectile.ai[0] * 8f;
				Projectile.velocity.Y = -Projectile.velocity.X;
			}

			if (timer <= uptimer) {
				Projectile.velocity = new Vector2(0, -7f * (1 - timer / uptimer));
			}
			else if (timer <= uptimer + spintimer) {
				float percentOfT = 1 - (timer - uptimer) / (2 * spintimer);
				move1X = Projectile.ai[0] * (timer - uptimer) / 5f * MathF.Sin((timer - uptimer) * MathHelper.Pi / (120 * percentOfT));
				move1Y = Projectile.ai[0] * (timer - uptimer) / 5f * MathF.Cos((timer - uptimer) * MathHelper.Pi / (120 * percentOfT));
				Projectile.Center = new Vector2(move1X + oldPosX, move1Y + oldPosY);//期间的位置变动
			}
			else if (timer <= uptimer + spintimer + lerptimer) {
				Projectile.velocity.Y = float.Lerp(Projectile.velocity.Y, 0, 0.15f);
				Projectile.velocity.X = float.Lerp(Projectile.velocity.X, Projectile.ai[0] * 4f, 0.25f);
			}
			else {
				//Projectile.velocity.Y += 0.03f;
				Projectile.velocity.Y = 0f;
				Projectile.velocity.X = float.Lerp(Projectile.velocity.X, 0, 0.00625f);
				if (Math.Abs(Projectile.velocity.X) <= 0.25f) {
					Projectile.NewProjectile(newSource, Projectile.Center - new Vector2(0, 14), Vector2.Zero, ModContent.ProjectileType<BlizzardStorm>(), Projectile.damage, 0f, 0, Projectile.ai[0]);
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/FNSummonIceAltar") with { Volume = 0.8f, Pitch = 0f }, Projectile.Center);
					Projectile.Kill();
				}
			}
		}

		public override bool PreDraw(ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/WindTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, Vector2.Zero, new Color(r, g, 255), new Color(0, 0, 0), 15f * Math.Min(timer / 60, 1), true);
			return true;
		}
	}
	//浮在空中的源石冰晶
	public class BlizzardStorm : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/NPCs/Enemy/Chapter6/FrostNova/FNIceCrystal";

		public override void SetDefaults() {
			Projectile.width = 44;
			Projectile.height = 52;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 6000;
			Projectile.alpha = 0;
			Projectile.damage = 30;
			Projectile.light = 0.2f;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.scale = 1f;
			Projectile.rotation = -23 * float.Pi / 180f;
		}

		private readonly int trailnum = Main.rand.Next(8, 13);//随机8-12条轨迹

		public static int HostNPCType() {
			return ModContent.NPCType<FrostNova>();
		}

		//碰上就冻结
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.buffImmune[BuffID.Frozen] = false;
			if (!target.HasBuff(BuffID.Frozen)) {
				target.AddBuff(BuffID.Frozen, Main.masterMode ? 60 : Main.expertMode ? 60 : 90);
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/Frozen") with { Volume = 1f, Pitch = 0f }, target.Center);
			}
		}

		private float timer;
		private float startY;

		public override void PostDraw(Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			Texture2D ICGlowT = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/Chapter6/FrostNova/ICGlow").Value;
			Texture2D ICRingT = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/Chapter6/FrostNova/ICRing").Value;
			Main.EntitySpriteDraw(ICGlowT, Projectile.Center - Main.screenPosition + new Vector2(0, 0), new Rectangle(0, 0, ICGlowT.Width, ICGlowT.Height), Color.White * Projectile.Opacity, Projectile.rotation, new Vector2(ICGlowT.Width / 2, ICGlowT.Height / 2), 1f, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(ICRingT, Projectile.Center - Main.screenPosition + new Vector2(0, 0), new Rectangle(0, 0, ICRingT.Width, ICRingT.Height), Color.White * Projectile.Opacity, 0, new Vector2(ICRingT.Width / 2, ICRingT.Height / 2), 1.2f, SpriteEffects.None, 0);

		}

		public override void OnSpawn(IEntitySource source) {
			startY = Projectile.Center.Y;
		}

		public override void AI() {
			Player Player = Main.player[Main.myPlayer];
			var newSource = Projectile.GetSource_FromThis();
			timer++;

			Projectile.Center = new Vector2(Projectile.Center.X, startY + 16 * MathF.Sin(timer * float.Pi / 180f));

			if (timer < 30) {
				for (int i = 0; i < 3; i++) {
					Dust.NewDustDirect(new Vector2(Projectile.Center.X, startY + timer * 10), 0, 0, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, 0f, 120, new Color(255, 255, 255), 0.5f);
					Dust.NewDustDirect(Projectile.Center, 0, 0, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, 0f, 120, new Color(255, 255, 255), 0.5f);
				}
				Projectile.Opacity = timer / 30f;
			}
			else if (timer == 30) {
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/SpawnDevice") with { Volume = 1f, Pitch = 0f }, Projectile.Center);
				Projectile.Opacity = 1;
			}

			if (timer >= 30) {
				if ((int)timer % 30 == 0) {
					for (int i = 0; i < trailnum; i++) {//生成轨迹，ai0为相位，2iπ/个数
						Projectile.NewProjectile(newSource, new Vector2(Projectile.Center.X, startY + 292), Vector2.Zero, ModContent.ProjectileType<Blizzard>(), Projectile.damage, 0f, 0, 2 * i * MathHelper.Pi / trailnum);
					}
				}
			}

			//生成与消失
			if (Projectile.timeLeft > 30) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC SeekForNPCs = Main.npc[i];
					if (SeekForNPCs.active && SeekForNPCs.type == HostNPCType()) {
						if (SeekForNPCs.life == 1) {
							Projectile.timeLeft = 30;
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/IceCrystal") with { Volume = 1f, Pitch = 0f }, Projectile.Center);
							for (int j = 0; j < 4; j++) {
								Dust.NewDustDirect(Projectile.Center, 0, 0, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, 0f, 120, new Color(255, 255, 255), 0.5f);
							}
						}
						else {
							Projectile.timeLeft = 60;
						}
					}
				}
			}
			else {
				for (int j = 0; j < 4; j++) {
					Dust.NewDustDirect(Projectile.Center, 0, 0, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, 0f, 120, new Color(255, 255, 255), 0.5f);
				}
				Projectile.Opacity = Projectile.timeLeft / 30f;
			}

			//超出左右龙卷风各自的范围就回弹
			if (Projectile.ai[0] == -1 ? Player.Center.X < Projectile.Center.X : Player.Center.X > Projectile.Center.X) {
				Player.velocity.X -= Projectile.ai[0] * 5f;
			}

			//口瓜！我不要看口牙！我不要看到嵌套三元运算符口牙！
			//Player.velocity.X -= Projectile.ai[0] == -1 ? Player.Center.X < Projectile.Center.X ? Projectile.ai[0] * 5f : 0 : Player.Center.X > Projectile.Center.X ? Projectile.ai[0] * 5f : 0;
		}

		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 4; i++) {
				Dust.NewDustDirect(Projectile.Center, 0, 0, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, 0f, 120, new Color(255, 255, 255), 0.5f);
			}
		}
	}
	//单个的龙卷风团
	public class Blizzard : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 60;
		}

		public override void SetDefaults() {
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 600;
			Projectile.alpha = 0;
			Projectile.damage = 10;
			Projectile.light = 1f;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.scale = 1f;
		}

		private float timer;//计时器
		private float addspeedy;//竖直方向的向上加速
		private float ScreenMedPlaceDisRate;//到屏幕中央的距离占比
		private float cosspeedy;//竖直方向的余弦运动，振幅随时间变大，与到屏幕中央的距离成正比
		private float sinspeedx;//水平方向的正弦运动，振幅随时间变大
		private float colorstate;//左右方向颜色不同
		private int r;
		private int g;

		//碰上就冻结
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.buffImmune[BuffID.Frozen] = false;
			if (!target.HasBuff(BuffID.Frozen)) {
				target.AddBuff(BuffID.Frozen, Main.masterMode ? 60 : Main.expertMode ? 60 : 90);
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/Frozen") with { Volume = 1f, Pitch = 0f }, target.Center);
			}
		}

		public override void AI() {
			Player Player = Main.player[Main.myPlayer];

			timer++;
			ScreenMedPlaceDisRate = Math.Abs(2 * (Projectile.Center.Y - Player.Center.Y) / Main.screenHeight);
			addspeedy = -timer / 90;
			cosspeedy = ScreenMedPlaceDisRate * Math.Min(timer / 240, 1) * (timer / 45) * (float)Math.Cos(timer * MathHelper.Pi / 120 + Projectile.ai[0]);
			sinspeedx = (float)((Math.Pow(1.004f, 1.6f * timer) - 1) * Math.Min(timer / 360, 1) * Math.Sin(timer * MathHelper.Pi / 120 + Projectile.ai[0]));
			Projectile.velocity = new Vector2(sinspeedx, addspeedy + cosspeedy);
			if (Projectile.velocity.X >= 0) {
				colorstate = 1;
			}
			else {
				colorstate = 0.5f;
			}

			r = 235 + 20 * (int)MathF.Sin(timer * MathHelper.Pi / 180);
			g = 245 + 10 * (int)MathF.Sin(timer * MathHelper.Pi / 180);

			if (Main.rand.NextFloat() < Math.Min(timer / 960, 0.25f)) {
				Dust.NewDustDirect(Projectile.Center, 0, 0, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, 0f, 120, new Color(255, 255, 255), 0.5f);
			}
		}

		public override bool PreDraw(ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/WindTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, Vector2.Zero, new Color((int)(r * colorstate), (int)(g * colorstate), (int)(255 * colorstate)), new Color(0, 0, 0), 35f, true);
			return true;
		}
	}
	//冰柱
	public class BlackIceProjectile : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 6;
		}
		public override void SetDefaults() {
			Projectile.width = 12;
			Projectile.height = 12;
			Projectile.aiStyle = 0;
			Projectile.damage = 10;
			Projectile.timeLeft = 65536;
			Projectile.penetrate = -1;
			Projectile.scale = 1f;
			Projectile.Opacity = 0f;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.netUpdate = true;
		}

		private float timer;
		private readonly Vector2[] oldPos = new Vector2[13]; //残影个数
		private float deltatime;
		private float offsettime;

		public override void PostDraw(Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			Texture2D Ice = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/Chapter6/FrostNova/BlackIceProjectile").Value;

			if (Projectile.frame == 5) {
				//残影
				for (int i = oldPos.Length - 1; i > 0; i--) {
					if (oldPos[i] != Vector2.Zero && i % 4 == 0) {//1%3是为了增大残影绘制间隔；Color.White * 1 * (1 - m*f * i)是残影淡化； (oldPos[i - 1] - oldPos[i]).ToRotation() + (float)(0.5 * MathHelper.Pi)是速度变化角度（基准为0.5pi，前面随速度变化不需要），1 * (1 - n*f * i)是大小逐级递减（也不需要）
						Main.EntitySpriteDraw(Ice, oldPos[i] - Main.screenPosition + new Vector2(0, 3) + new Vector2(0, (Projectile.frame * Ice.Height / 6 / 2)).RotatedBy(Projectile.rotation), new Rectangle(0, (int)(Projectile.frame * Ice.Height / 6f), Ice.Width, Ice.Height / 6), Color.White * (1 - 0.05f * i), Projectile.rotation, new Vector2(Ice.Width / 2, (Projectile.frame + 1) * (Ice.Height / 6) / 2), 1f, SpriteEffects.None, 0);
					}
				}
			}
			Main.EntitySpriteDraw(Ice, Projectile.Center - Main.screenPosition + new Vector2(0, 3) + new Vector2(0, (Projectile.frame * Ice.Height / 6 / 2)).RotatedBy(Projectile.rotation), new Rectangle(0, (int)(Projectile.frame * Ice.Height / 6f), Ice.Width, Ice.Height / 6), Color.White, Projectile.rotation, new Vector2(Ice.Width / 2, (Projectile.frame + 1) * (Ice.Height / 6) / 2), 1f, SpriteEffects.None, 0);

		}

		public override bool CanHitPlayer(Player Player) {
			return Projectile.frame == 5 && new Vector2(Player.Center.X - Projectile.Center.X, Player.Center.Y - Projectile.Center.Y).Length() <= 12f;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Projectile.Kill();
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.buffImmune[BuffID.Chilled] = false;
			target.buffImmune[BuffID.Frozen] = false;
			if (!target.HasBuff(BuffID.Chilled)) {
				target.AddBuff(BuffID.Chilled, Main.masterMode ? 180 : Main.expertMode ? 180 : 300);
			}
			else {
				target.AddBuff(BuffID.Frozen, Main.masterMode ? 60 : Main.expertMode ? 60 : 90);
			}
		}

		public override void OnKill(int timeLeft) {
			Vector2 position = Projectile.Center + new Vector2(0, -32f).RotatedBy(Projectile.rotation);
			SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/FNIceRingStg1") with { Volume = 1f, Pitch = 0f }, Projectile.Center);

			for (int i = 0; i < 5; i++) {
				Dust.NewDust(position, 16, 16, DustID.Granite, -0.5f * Projectile.velocity.X, -0.5f * Projectile.velocity.Y, 0, new Color(255, 255, 255), 1.25f);
				Dust.NewDust(position, 16, 16, DustID.PortalBolt, -0.5f * Projectile.velocity.X, -0.5f * Projectile.velocity.Y, 0, new Color(255, 255, 255), 1.25f);
			}
		}

		public override void AI() {//ai0为icenum（总数），1为numshooted（编号，0开始），2为技能种类
			timer++;
			if ((int)Projectile.ai[2] != Projectile.ai[2]) {//二阶段
				offsettime = (int)Projectile.ai[2] % 4 == 1 ? 4 : 2;
			}
			else {//一阶段
				offsettime = Projectile.ai[2] % 4 == 1 ? 8 : 4;
			}

			deltatime = (Projectile.ai[0] - Projectile.ai[1]) * offsettime;
			Projectile.rotation = (int)Projectile.ai[2] % 4 == 2 ? Projectile.ai[1] * 2 * float.Pi / Projectile.ai[0] + float.Pi : Projectile.ai[1] * 2 * float.Pi / Projectile.ai[0];
			if (Projectile.frame != 5) {
				if (Projectile.frameCounter >= 5) {
					Projectile.frameCounter = 0;
					Projectile.frame += 1;
				}
				else {
					if (Projectile.ai[0] == 0) {
						Projectile.frameCounter++;
					}
					else {

						if (timer >= deltatime) {
							Projectile.frameCounter++;
						}
					}
				}
			}
			else {
				Projectile.velocity = new Vector2(0, -Math.Min(((timer - deltatime) - 20f) / 5f, 16f)).RotatedBy(Projectile.rotation);
				if (timer % 1 == 0) {
					for (int i = oldPos.Length - 1; i > 0; i--) {
						oldPos[i] = oldPos[i - 1];
					}
					oldPos[0] = Projectile.Center;
				}

				Dust dust1;
				Vector2 position1 = Projectile.Center + new Vector2(8f * (float)Math.Sin((timer - deltatime) * Math.PI / 18f), -24f).RotatedBy(Projectile.rotation);
				dust1 = Terraria.Dust.NewDustPerfect(position1, 240, new Vector2(0, 0), 0, new Color(255, 255, 255), 1.5f);
				dust1.noGravity = true;

				Dust dust2;
				Vector2 position2 = Projectile.Center + new Vector2(-8f * (float)Math.Sin((timer - deltatime) * Math.PI / 18f), -24f).RotatedBy(Projectile.rotation);
				dust2 = Terraria.Dust.NewDustPerfect(position2, 263, new Vector2(0, 0), 0, new Color(255, 255, 255), 1.5f);
				dust2.noGravity = true;
			}
		}
	}
	//冰晶（引物）
	public class BlackIceIntro : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 15;
		}
		public override void SetDefaults() {
			Projectile.width = 46;
			Projectile.height = 36;
			Projectile.aiStyle = 0;
			Projectile.damage = 10;
			Projectile.timeLeft = 600;
			Projectile.penetrate = -1;
			Projectile.scale = 1f;
			Projectile.Opacity = 0f;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.netUpdate = true;
		}

		private float timer;
		private bool isProjShooted = false;
		private float deltatime;
		private float skilloffsetrotation;
		private Vector2 IceVelocity;

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.buffImmune[BuffID.Chilled] = false;
			target.buffImmune[BuffID.Frozen] = false;
			if (!target.HasBuff(BuffID.Chilled)) {
				target.AddBuff(BuffID.Chilled, Main.masterMode ? 180 : Main.expertMode ? 180 : 300);
			}
			else {
				target.AddBuff(BuffID.Frozen, Main.masterMode ? 60 : Main.expertMode ? 60 : 90);
			}
		}

		public override void AI() {//ai0为0时是平A召唤，否则为技能召唤
			timer++;
			deltatime = (Projectile.ai[0] - Projectile.ai[1]) * 8;//越晚越短，ai0是总数，ai1是编号
			Projectile.Opacity = Math.Min(timer / 30f, 1f);
			Player Player = Main.player[Main.myPlayer];

			if (Projectile.frame < 12) {
				Projectile.rotation = Projectile.DirectionTo(Player.Center).ToRotation();
			}
			else if (Projectile.frame == 12) {
				skilloffsetrotation = Projectile.ai[0] == 0 ? 0 : 0.075f * float.Pi * MathF.Sin(2 * Projectile.ai[1] * float.Pi / Projectile.ai[0]);
				IceVelocity = new Vector2(0, -Math.Min(((timer - deltatime) - 20f) / 5f, 16f)).RotatedBy(Projectile.rotation + skilloffsetrotation);
				if (!isProjShooted) {
					Projectile.NewProjectile(null, Projectile.Center, IceVelocity, ModContent.ProjectileType<BlackIce>(), Projectile.damage, 0.25f, -1, skilloffsetrotation);
					isProjShooted = true;
				}
			}
			//帧绘制
			if (Projectile.frameCounter >= 2) {
				Projectile.frameCounter = 0;
				Projectile.frame += 1;
				if (Projectile.frame == 14) {
					Projectile.Kill();
				}
			}
			else {
				if (Projectile.ai[0] == 0) {
					Projectile.frameCounter++;
				}
				else {

					if (timer >= deltatime) {
						Projectile.frameCounter++;
					}
				}
			}
		}
	}
	//冰晶（飞行）
	public class BlackIce : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 1;
		}
		public override void SetDefaults() {
			Projectile.width = 24;
			Projectile.height = 22;
			Projectile.aiStyle = 0;
			Projectile.damage = 10;
			Projectile.timeLeft = 65536;
			Projectile.penetrate = 1;
			Projectile.scale = 1f;
			Projectile.Opacity = 1f;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.netUpdate = true;
		}

		private float timer;
		private readonly Vector2[] oldPos = new Vector2[13]; //残影个数

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.buffImmune[BuffID.Chilled] = false;
			target.buffImmune[BuffID.Frozen] = false;
			if (!target.HasBuff(BuffID.Chilled)) {
				target.AddBuff(BuffID.Chilled, Main.masterMode ? 180 : Main.expertMode ? 180 : 300);
			}
			else {
				target.AddBuff(BuffID.Frozen, Main.masterMode ? 60 : Main.expertMode ? 60 : 90);
			}
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			Projectile.Kill();
		}

		public override void PostDraw(Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			Texture2D Ice = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/Chapter6/FrostNova/BlackIce").Value;
			for (int i = oldPos.Length - 1; i > 0; i--) {
				if (oldPos[i] != Vector2.Zero && i % 3 == 0) {//1%3是为了增大残影绘制间隔；Color.White * 1 * (1 - m*f * i)是残影淡化； (oldPos[i - 1] - oldPos[i]).ToRotation() + (float)(0.5 * MathHelper.Pi)是速度变化角度（基准为0.5pi，前面随速度变化不需要），1 * (1 - n*f * i)是大小逐级递减（也不需要）
					Main.EntitySpriteDraw(Ice, oldPos[i] - Main.screenPosition, null, Color.White * (1 - 0.05f * i), Projectile.rotation, Ice.Size() * 0.5f, 1f, SpriteEffects.None, 0);
				}
			}
		}

		public override void AI() {
			timer++;
			Player Player = Main.player[Main.myPlayer];
			if (timer <= 1f) {
				Projectile.rotation = Projectile.DirectionTo(Player.Center).ToRotation() + MathHelper.PiOver4 + Projectile.ai[0];
			}
			else {
				Projectile.velocity = new Vector2(0, -Math.Min(timer / 5f + 4f, 16f)).RotatedBy(Projectile.rotation + MathHelper.PiOver4);
				if (timer % 1 == 0) {
					for (int i = oldPos.Length - 1; i > 0; i--) {
						oldPos[i] = oldPos[i - 1];
					}
					oldPos[0] = Projectile.Center;
				}

				Dust dust1;
				Vector2 position1 = Projectile.Center + new Vector2(8f * (float)Math.Sin(timer * Math.PI / 18f), -12f).RotatedBy(Projectile.rotation + MathHelper.PiOver4);
				dust1 = Terraria.Dust.NewDustPerfect(position1, 240, new Vector2(0, 0), 0, new Color(255, 255, 255), 1f);
				dust1.noGravity = true;
				Dust dust2;
				Vector2 position2 = Projectile.Center + new Vector2(-8f * (float)Math.Sin(timer * Math.PI / 18f), -12f).RotatedBy(Projectile.rotation + MathHelper.PiOver4);
				dust2 = Terraria.Dust.NewDustPerfect(position2, 263, new Vector2(0, 0), 0, new Color(255, 255, 255), 1f);
				dust2.noGravity = true;
			}
		}

		public override void OnKill(int timeLeft) {
			Vector2 position = Projectile.Center + new Vector2(0, -12f).RotatedBy(Projectile.rotation);
			Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BlackIceExplode>(), Projectile.damage, 0.25f, -1, Projectile.ai[0]);
			for (int i = 0; i < 3; i++) {
				Dust.NewDust(position, 16, 16, DustID.Granite, -0.5f * Projectile.velocity.X, -0.5f * Projectile.velocity.Y, 0, new Color(255, 255, 255), 1f);
				Dust.NewDust(position, 16, 16, DustID.PortalBolt, -0.5f * Projectile.velocity.X, -0.5f * Projectile.velocity.Y, 0, new Color(255, 255, 255), 1f);
			}
		}
	}
	//冰晶爆炸
	public class BlackIceExplode : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 6;
		}
		public override void SetDefaults() {
			Projectile.width = 64;
			Projectile.height = 64;
			Projectile.aiStyle = 0;
			Projectile.damage = 20;
			Projectile.timeLeft = 60;
			Projectile.penetrate = -1;
			Projectile.scale = 1f;
			Projectile.Opacity = 1f;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.netUpdate = true;
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.buffImmune[BuffID.Chilled] = false;
			target.buffImmune[BuffID.Frozen] = false;
			if (!target.HasBuff(BuffID.Chilled)) {
				target.AddBuff(BuffID.Chilled, Main.masterMode ? 180 : Main.expertMode ? 180 : 300);
			}
			else {
				target.AddBuff(BuffID.Frozen, Main.masterMode ? 60 : Main.expertMode ? 60 : 90);
			}
		}

		public override void AI() {
			Projectile.frameCounter++;
			if (Projectile.frameCounter >= 6) {
				Projectile.frameCounter = 0;
				Projectile.frame += 1;
				if (Projectile.frame == 1) {
					if (Projectile.ai[0] == 0) {
						SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/FNAttack") with { Volume = 1f, Pitch = 0f }, Projectile.Center);
					}
					else {
						SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/FNIceRingStg1") with { Volume = 1f, Pitch = 0f }, Projectile.Center);
					}
				}
				if (Projectile.frame == 5) {
					Projectile.Kill();
				}
			}
		}
	}
	//技能分发弹幕，整合了8种冰环技能
	public class FNIceRingChoice : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 60;
		}

		public override void SetDefaults() {
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 420;
			Projectile.alpha = 0;
			Projectile.damage = 10;
			Projectile.light = 1f;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.scale = 1f;
		}

		private float timer;//计时器
		private int icenum;//冰凌个数
		private int icenumshooted;
		private float speedY;
		private NPC HostNPC = null;

		//碰上就冻结
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.buffImmune[BuffID.Frozen] = false;
			if (!target.HasBuff(BuffID.Frozen)) {
				target.AddBuff(BuffID.Frozen, Main.masterMode ? 100 : Main.expertMode ? 100 : 180);
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/Frozen") with { Volume = 1f, Pitch = 0f }, target.Center);
			}
		}

		public static int CHildProjType1() {
			return ModContent.ProjectileType<FNIceRingRound>();
		}

		public static int HostNPCType() {
			return ModContent.NPCType<FrostNova>();
		}

		public override void OnSpawn(IEntitySource source) {
			bool hasChild = false;
			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile ChildProj = Main.projectile[i];
				if (ChildProj.active && ChildProj.ai[1] == Projectile.whoAmI && ChildProj.type == CHildProjType1()) {
					hasChild = true;
				}
			}
			if (!hasChild) {
				Projectile.NewProjectile(source, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FNIceRingRound>(), Projectile.damage, 0f, -1, -1, Projectile.whoAmI);
				Projectile.NewProjectile(source, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<FNIceRingRound>(), Projectile.damage, 0f, -1, 1, Projectile.whoAmI);
			}
			//寻找主体
			for (int i = 0; i < Main.maxNPCs; i++) {
				HostNPC = Main.npc[i];
				if (HostNPC.active && HostNPC.type == HostNPCType()) {
					break;
				}
			}
			//修正冰晶数量
			if (Projectile.ai[1] % 4 == 0) {//交火冰环
				icenum = Main.rand.Next(8, 13);
			}
			else if (Projectile.ai[1] % 4 == 2) {//坠冰冰环
				icenum = Projectile.ai[0] == 1 ? 45 : 90;
			}
			else {//螺旋冰环和辐向冰环
				icenum = Projectile.ai[0] == 1 ? Main.rand.Next(16, 25) : Main.rand.Next(32, 49);
			}
		}

		public override void AI() {
			Player Player = Main.player[Main.myPlayer];
			var newSource = Projectile.GetSource_FromThis();
			timer++;

			if (Projectile.ai[1] % 4 == 0) {//交火冰环（α和α+）
				speedY = timer >= 240f ? 0 : (Projectile.ai[2] == 0 ? -2 : -2 * MathF.Sqrt(2)) * MathF.Sin(float.Pi * timer / 240f);//960 / π像素
				Projectile.velocity = new Vector2(0, speedY).RotatedBy(Projectile.ai[2]);
				if (timer >= 300f && (int)(timer - 300) % 8 == 0 && icenumshooted < icenum) {
					Projectile.NewProjectile(newSource, Projectile.Center + new Vector2(0, -72f).RotatedBy(icenumshooted * 2 * float.Pi / icenum) + new Vector2(1f, 14f).RotatedBy(0), Vector2.Zero, ModContent.ProjectileType<BlackIceIntro>(), Projectile.damage, 0f, -1, icenum, icenumshooted);
					SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);
					icenumshooted++;
				}
			}
			else if (Projectile.ai[1] % 4 == 2) {//坠冰冰环（γ和γ+）
				float dis = timer >= 120f ? (Projectile.Center - HostNPC.Center).Length() : 0;

				if (timer <= 120f) {
					speedY = (Projectile.ai[0] == 1 ? -5.25f : -7.25f) * MathF.Sin(float.Pi * timer / 120f);//1260(2520) / π像素
					Projectile.velocity = new Vector2(0, speedY);
				}
				else if (timer <= 300f) {
					speedY = 2f / 9f * (timer - 120f) / 60f * (timer - 120f) / 60f * float.Pi;//绕一整圈
					Projectile.Center = HostNPC.Center + new Vector2(0, -dis).RotatedBy(speedY);
				}
				else if (timer <= 390f) {
					speedY = (timer - 300) * float.Pi / 45f;
					Projectile.Center = HostNPC.Center + new Vector2(0, -dis).RotatedBy(speedY);//绕一整圈
				}
				else {
					Projectile.Center = HostNPC.Center + new Vector2(0, -dis).RotatedBy(speedY);
				}

				if (timer >= 300f && (int)(timer - 300) % (Projectile.ai[0] == 1 ? 2 : 1) == 0 && timer < 390f) {
					Projectile.NewProjectile(newSource, Projectile.Center + new Vector2(0, 32f).RotatedBy((icenumshooted) * 2 * float.Pi / icenum), Vector2.Zero, ModContent.ProjectileType<BlackIceProjectile>(), Projectile.damage, 0f, -1, icenum, icenumshooted - 1, Projectile.ai[1]);
					if ((int)(timer - 300) % 2 == 0) {
						SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);
					}
					icenumshooted++;
				}
			}
			else {//周身冰环（β和δ）,ai1（技能次数）会传给子弹幕的ai2，除以4余1的时候是不同时间发射的冰环（β），除以4余3是同时间发射的冰环（δ）
				Projectile.Center = HostNPC.Center;
				if (timer >= 300f && (int)(timer - 300) % (Projectile.ai[0] == 1 ? 4 : 2) == 0 && icenumshooted < icenum) {
					Projectile.NewProjectile(newSource, Projectile.Center + new Vector2(0, -72f).RotatedBy(icenumshooted * 2 * float.Pi / icenum) + new Vector2(1f, 0f).RotatedBy(0), Vector2.Zero, ModContent.ProjectileType<BlackIceProjectile>(), Projectile.damage, 0f, -1, icenum, icenumshooted, Projectile.ai[1] + 0.1f * (Projectile.ai[0] - 1));
					SoundEngine.PlaySound(SoundID.Item30, Projectile.Center);
					icenumshooted++;
				}
			}

			if (Main.rand.NextFloat() < Math.Min(timer / 300, 0.25f)) {
				Dust.NewDustDirect(Projectile.Center, 0, 0, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, 0f, 120, new Color(255, 255, 255), 0.5f);
			}
		}
	}
	//环绕弹幕
	public class FNIceRingRound : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 60;
		}

		public override void SetDefaults() {
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 900;
			Projectile.alpha = 0;
			Projectile.damage = 10;
			Projectile.light = 1f;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.scale = 1f;
		}

		private float timer;//计时器
		private float dis;
		private float drawopacity;
		private int r;
		private int g;
		private Projectile HostProj = null;

		//碰上就冻结
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.buffImmune[BuffID.Frozen] = false;
			if (!target.HasBuff(BuffID.Frozen)) {
				target.AddBuff(BuffID.Frozen, Main.masterMode ? 100 : Main.expertMode ? 100 : 180);
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/Frozen") with { Volume = 1f, Pitch = 0f }, target.Center);
			}
		}

		public static int HostProjType1() {
			return ModContent.ProjectileType<FNIceRingChoice>();
		}

		public override void OnSpawn(IEntitySource source) {
			for (int i = 0; i < Main.maxProjectiles; i++) {
				HostProj = Main.projectile[i];
				if (HostProj.active && Projectile.ai[1] == HostProj.whoAmI && HostProj.type == HostProjType1()) {
					break;
				}
			}
		}

		public override void AI() {
			timer++;
			Projectile.velocity = Vector2.Zero;
			dis = Math.Min(timer / 4, 36f);

			Projectile.timeLeft = HostProj.timeLeft;
			Projectile.Center = HostProj.Center + new Vector2(Projectile.ai[0] * dis, 0).RotatedBy(timer * float.Pi / 45);

			if (timer >= 240f) {
				Projectile.ai[2]--;
			}
			else if (timer <= 30f) {
				Projectile.ai[2] += 2f;
			}
			else {
				Projectile.ai[2] = 60;
			}
			drawopacity = Math.Min(Projectile.ai[2] / 60f, 1f);

			if (drawopacity == 0) {
				Projectile.Kill();
			}

			r = 235 + 20 * (int)MathF.Sin(timer * MathHelper.Pi / 180);
			g = 245 + 10 * (int)MathF.Sin(timer * MathHelper.Pi / 180);

			if (Main.rand.NextFloat() < Math.Min(timer / 960, 0.25f)) {
				Dust.NewDustDirect(Projectile.Center, 0, 0, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, 0f, 120, new Color(255, 255, 255), 0.5f);
			}
		}

		public override bool PreDraw(ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/WindTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, Vector2.Zero, new Color(r, g, 255) * drawopacity, new Color(0, 0, 0), 25f, true);
			return true;
		}
	}
	//无敌特效
	public class FNInvincibleEffect : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 60;
		}

		public override void SetDefaults() {
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 900;
			Projectile.alpha = 0;
			Projectile.damage = 0;
			Projectile.light = 1f;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.scale = 1f;
		}

		private float timer;
		private float drawopacity;
		private float dotX;
		private float dotY;
		private int r;
		private int g;
		private int b;

		private NPC HostNPC = null;

		public static int HostNPCType() {
			return ModContent.NPCType<FrostNova>();
		}

		//寻找主体
		public override void OnSpawn(IEntitySource source) {
			Projectile.timeLeft = (int)Projectile.ai[0];
			for (int i = 0; i < Main.maxNPCs; i++) {
				HostNPC = Main.npc[i];
				if (HostNPC.active && HostNPC.type == HostNPCType()) {
					break;
				}
			}
		}

		public override void AI() {
			timer++;
			Projectile.velocity = Vector2.Zero;
			dotX = 36 * MathF.Sin((timer / 30 + 0.4f + 0.6f * Projectile.ai[1]) * float.Pi);
			dotY = 36 * Projectile.ai[1] * 0.5f * MathF.Cos((timer / 30 + 0.6f * Projectile.ai[1]) * float.Pi);
			Projectile.Center = HostNPC.Center + new Vector2(dotX, dotY + 8);

			if (timer <= 60f) {
				drawopacity += 1 / 60f;
			}
			else if (Projectile.timeLeft <= 60f) {
				drawopacity -= 1 / 60f;
			}
			else {
				drawopacity = 1;
			}

			r = 235 + 20 * (int)MathF.Sin(timer * MathHelper.Pi / 180);
			g = 245 + 10 * (int)MathF.Sin(timer * MathHelper.Pi / 180);

			if (!HostNPC.active) {
				Projectile.Kill();
			}
			if (Projectile.timeLeft > 60) {
				Projectile.timeLeft = HostNPC.dontTakeDamage ? 1800 : 60;
			}
		}

		public override bool PreDraw(ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, Vector2.Zero, new Color(r, g, 0) * drawopacity, new Color(0, 0, 0), 15f, true);
			return true;
		}
	}
	//封印地板的冰柱
	public class FNBanFloor : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 1;
		}
		public override void SetDefaults() {
			Projectile.width = 14;
			Projectile.height = 70;
			Projectile.aiStyle = 0;
			Projectile.damage = 10;
			Projectile.timeLeft = 65536;
			Projectile.penetrate = 1;
			Projectile.scale = 0f;
			Projectile.Opacity = 0f;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.netUpdate = true;
		}

		private float timer;
		private readonly Vector2[] oldPos = new Vector2[13]; //残影个数
		private float GateScaleX = 0f;
		private float GateScaleY = 2f;
		private float GateOpacity = 0f;
		private float MaskOpacity = 0f;
		private float ProjOpacity = 0f;
		private float Projscale = 0f;
		private Vector2 SpawnPos;

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.buffImmune[BuffID.Frozen] = false;
			target.AddBuff(BuffID.Frozen, Main.masterMode ? 90 : Main.expertMode ? 90 : 120);
		}

		public override void OnSpawn(IEntitySource source) {
			SpawnPos = Projectile.Center;
		}

		public override bool PreDraw(ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			Texture2D Gate = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/Chapter6/FrostNova/FNBanFloorGate").Value;
			Vector2 GateScale = new Vector2(GateScaleX, GateScaleY);
			Main.EntitySpriteDraw(Gate, SpawnPos - Main.screenPosition, new Rectangle(0, 0, Gate.Width, Gate.Height), Color.White * GateOpacity, 0, new Vector2(Gate.Width / 2, Gate.Height / 2), GateScale, SpriteEffects.None, 0);
			return true;
		}

		public override void PostDraw(Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			Texture2D Ice = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/Chapter6/FrostNova/FNBanFloor").Value;
			Texture2D Mask = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/Chapter6/FrostNova/FNBanFloorMask").Value;
			Main.EntitySpriteDraw(Ice, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, Ice.Width, Ice.Height), Color.White * ProjOpacity, Projectile.rotation, new Vector2(Ice.Width / 2, Ice.Height / 2), Projscale, SpriteEffects.None, 0);

			if (timer > 60 && timer <= 90) {
				Main.EntitySpriteDraw(Mask, Projectile.Center - Main.screenPosition, new Rectangle(0, 0, Mask.Width, Mask.Height), Color.White * MaskOpacity, Projectile.rotation, new Vector2(Mask.Width / 2, Mask.Height / 2), 1f, SpriteEffects.None, 0);
			}
			else if (timer > 90) {
				for (int i = oldPos.Length - 1; i > 0; i--) {
					if (oldPos[i] != Vector2.Zero && i % 3 == 0) {//1%3是为了增大残影绘制间隔；Color.White * 1 * (1 - m*f * i)是残影淡化； (oldPos[i - 1] - oldPos[i]).ToRotation() + (float)(0.5 * MathHelper.Pi)是速度变化角度（基准为0.5pi，前面随速度变化不需要），1 * (1 - n*f * i)是大小逐级递减（也不需要）
						Main.EntitySpriteDraw(Ice, oldPos[i] - Main.screenPosition, null, Color.White * (1 - 0.05f * i), Projectile.rotation, Ice.Size() * 0.5f, 1f, SpriteEffects.None, 0);
					}
				}
			}
		}

		public override void AI() {
			timer++;
			Player Player = Main.player[Main.myPlayer];
			if (timer <= 30f) {
				GateScaleX = timer / 60f;
				GateScaleY = 2f + timer / 15f;
				GateOpacity = timer / 30f;
			}
			else if (timer <= 60f) {
				ProjOpacity = (timer - 30f) / 30f;
				Projscale = (timer - 30f) / 30f;
			}
			else if (timer <= 90f) {
				GateScaleX = 1f - (timer - 60f) / 30f;
				GateScaleY = 4f - (timer - 60f) / 15f;
				GateOpacity = 1f - (timer - 60f) / 30f;
				if (timer > 60f && timer <= 75f) {
					MaskOpacity = (timer - 75f) / 15f;
				}
				else if (timer > 75f && timer <= 90f) {
					MaskOpacity = (90f - timer) / 15f;
				}
			}
			else {
				Projectile.velocity = new Vector2(0, Math.Min((timer - 60f) / 5f + 4f, 16f));
				if (timer % 1 == 0) {
					for (int i = oldPos.Length - 1; i > 0; i--) {
						oldPos[i] = oldPos[i - 1];
					}
					oldPos[0] = Projectile.Center;
				}

			}
			if (timer == 5) {
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/FNSummonLockTileIce") with { Volume = 1f, Pitch = 0f }, Projectile.Center);
			}
			if (timer <= 60f) {
				Projectile.velocity.Y = -timer / 60f;
			}
		}

		public override void OnKill(int timeLeft) {
			Vector2 position = Projectile.Center + new Vector2(0, -12f).RotatedBy(Projectile.rotation);
			SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/FNLockTileIce") with { Volume = 1f, Pitch = 0f }, Projectile.Center);
			for (int i = 0; i < 3; i++) {
				Dust.NewDust(position, 16, 16, DustID.Granite, -0.5f * Projectile.velocity.X, -0.5f * Projectile.velocity.Y, 0, new Color(255, 255, 255), 1f);
				Dust.NewDust(position, 16, 16, DustID.PortalBolt, -0.5f * Projectile.velocity.X, -0.5f * Projectile.velocity.Y, 0, new Color(255, 255, 255), 1f);
			}
			Projectile.NewProjectile(null, Projectile.Center, Vector2.Zero, ProjectileType<BlackIceFloor>(), 20, 0f, -1, Main.rand.Next(0, 12));
		}
	}
	//被封印的地板
	public class BlackIceFloor : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 12;
		}
		public override void SetDefaults() {
			Projectile.width = 60;
			Projectile.height = 32;
			Projectile.aiStyle = 0;
			Projectile.damage = 20;
			Projectile.timeLeft = 60000;
			Projectile.penetrate = -1;
			Projectile.scale = 1f;
			Projectile.Opacity = 1f;
			Projectile.hostile = true;
			Projectile.friendly = false;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
			Projectile.netUpdate = true;
		}

		public static int HostNPCType() {
			return ModContent.NPCType<FrostNova>();
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.buffImmune[BuffID.Chilled] = false;
			target.buffImmune[BuffID.Frozen] = false;
			if (!target.HasBuff(BuffID.Chilled)) {
				target.AddBuff(BuffID.Chilled, Main.masterMode ? 180 : Main.expertMode ? 180 : 300);
			}
			else {
				target.AddBuff(BuffID.Frozen, Main.masterMode ? 90 : Main.expertMode ? 90 : 120);
			}
		}

		public override void AI() {
			Projectile.frame = (int)Projectile.ai[0];

			if (Projectile.timeLeft > 30) {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC SeekForNPCs = Main.npc[i];
					if (SeekForNPCs.active && SeekForNPCs.type == HostNPCType()) {
						if (SeekForNPCs.life == 1) {
							Projectile.timeLeft = 30;
						}
						else {
							Projectile.timeLeft = 60;
						}
					}
				}
			}
			else {

			}
		}

		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/IceCrystal") with { Volume = 1f, Pitch = 0f }, Projectile.Center);
			for (int i = 0; i < 5; i++) {
				Dust.NewDust(Projectile.Center, 16, 16, DustID.Granite, -0.5f * Projectile.velocity.X, -0.5f * Projectile.velocity.Y, 0, new Color(255, 255, 255), 1f);
				Dust.NewDust(Projectile.Center, 16, 16, DustID.PortalBolt, -0.5f * Projectile.velocity.X, -0.5f * Projectile.velocity.Y, 0, new Color(255, 255, 255), 1f);
			}
		}
	}
	//冷气粒子效果
	public class FNIceDust : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 60;
		}

		public override void SetDefaults() {
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 190;
			Projectile.alpha = 0;
			Projectile.damage = 0;
			Projectile.light = 0f;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.scale = 1f;
		}

		private float timer;
		private float drawopacity;
		private float dotX;
		private float dotY;
		private int r;
		private int g;
		private int b;

		private NPC HostNPC = null;

		public static int HostNPCType() {
			return ModContent.NPCType<FrostNova>();
		}

		//寻找主体
		public override void OnSpawn(IEntitySource source) {
			for (int i = 0; i < Main.maxNPCs; i++) {
				HostNPC = Main.npc[i];
				if (HostNPC.active && HostNPC.type == HostNPCType()) {
					break;
				}
			}
		}

		public override void AI() {
			timer++;
			float k = Projectile.ai[1] * float.Pi / 18;
			float radius = 300;
			float dX = -radius * MathF.Sin(Projectile.ai[0] * float.Pi / 3 + k) + radius * MathF.Sin(((float.Pi * timer) / 90) + Projectile.ai[0] * float.Pi / 3 + k);
			float dY = radius * MathF.Cos(Projectile.ai[0] * float.Pi / 3 + k) - radius * MathF.Cos(((float.Pi * timer) / 90) + Projectile.ai[0] * float.Pi / 3 + k);
			Projectile.Center = HostNPC.Center + new Vector2(dX, dY);

			if (timer <= 60f) {
				drawopacity += 1 / 360f;
			}
			else if (Projectile.timeLeft <= 60f) {
				drawopacity -= 1 / 360f;
			}
			else {
				drawopacity = 1 / 6f;
			}

			r = 185 + 20 * (int)MathF.Sin(timer * MathHelper.Pi / 180);
			g = 245 + 10 * (int)MathF.Sin(timer * MathHelper.Pi / 180);

			Dust.NewDustDirect(Projectile.Center, 0, 0, DustType<Dusts.Bosses.FrostNovaDeathDust>(), 0f, 0f, 120, new Color(255, 255, 255), 0.5f);
		}

		public override bool PreDraw(ref Color lightColor)/* tModPorter Replace 'Main.player[Projectile.owner]' with 'player'. */ {
			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/WindTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, Vector2.Zero, new Color(r, g, 255) * drawopacity, new Color(0, 0, 0), 100f, true);
			return true;
		}
	}
}