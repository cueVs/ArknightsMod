using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Content.BossBars;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

namespace ArknightsMod.Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer
{
	[AutoloadBossHead]
	public class AACT : ModNPC
	{
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 1;//贴图帧数
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			NPCID.Sets.BossBestiaryPriority.Add(Type);
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers() {
				CustomTexturePath = "ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/AACT",
				PortraitScale = 1f,
				PortraitPositionYOverride = 0f,
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetDefaults() {
			NPC.lifeMax = 16000;
			NPC.damage = 40;
			NPC.defense = 120;
			NPC.boss = true;//BOSS
			NPC.knockBackResist = 0f;//击退抗性，0f为最高，1f为最低
			NPC.scale = 0f;
			NPC.width = 180;
			NPC.height = 80;
			NPC.noGravity = true;//无引力
			NPC.noTileCollide = true;//不与物块相撞
			NPC.lavaImmune = true;//免疫岩浆
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit4;//金属声
			NPC.DeathSound = SoundID.NPCDeath14;//爆炸声
			NPCID.Sets.ImmuneToAllBuffs[Type] = true;//免疫所有debuff
			NPC.ai[2] = 2;//存储二阶段的伤害值
			NPC.npcSlots = 10f;
		}

		//public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.Player.ZoneSnow && spawnInfo.Player.ZoneOverworldHeight && NPC.downedPlantBoss == true && Main.raining && !Main.dayTime && !NPC.AnyNPCs(ModContent.NPCType<IAT>()) && !NPC.AnyNPCs(ModContent.NPCType<IACT>()) ? 0.01f : 0f;
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.ArknightsMod.Bestiary.AACT")),
			});
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<IncandescentAlloyBlock>(), 2, 5, 8));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CrystallineCircuit>(), 2, 5, 8));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<OptimizedDevice>(), 2, 5, 8));
		}

		//NPC专家模式|大师模式血量倍率（普通模式血量*倍率*2|血量*倍率*3）
		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
			NPC.lifeMax = (int)(NPC.lifeMax * 0.75f * balance);//16000|24000|36000
			NPC.damage = (int)(NPC.damage * 0.8f);//40|64|96
		}

		private float expertHealthFrac = 0.9f;//进入第二阶段生命值百分比（90%）

		#region 视觉效果部分+盾闪避
		private int crashtimer;
		private float atkscale;
		private float MISSChance = 0f;
		private bool nodamage = false;
		private float nodamagetimer = 0;
		private float randtplength {
			get {
				if (Main.masterMode) {
					return Main.rand.NextFloat(288f, 384f);
				}
				else {
					return Main.expertMode ? Main.rand.NextFloat(320f, 432f) : Main.rand.NextFloat(352f, 480f);
				}
			}
		}
		private float spinrotation;
		private float originscale = 1f;

		//NPC贴图黑暗高亮发光效果+光环效果
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)//贴图，位置，大小区域，光亮颜色，转动角度，中心点坐标，缩放倍率，特殊效果(翻转)，图层
		{
			//鉴于layerdepth是个废物，越靠下出现的实际上越在上层
			Texture2D AACTitself = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/AACT").Value;

			if (AACTcrashed != true) {
				//残影
				for (int i = oldPos.Length - 1; i > 0; i--) {
					if (oldPos[i] != Vector2.Zero && i % 3 == 0) {//1%3是为了增大残影绘制间隔；Color.White * 1 * (1 - m*f * i)是残影淡化； (oldPos[i - 1] - oldPos[i]).ToRotation() + (float)(0.5 * MathHelper.Pi)是速度变化角度（基准为0.5pi，前面随速度变化不需要），1 * (1 - n*f * i)是大小逐级递减（也不需要）
						Main.EntitySpriteDraw(AACTitself, oldPos[i] - Main.screenPosition, null, Color.White * (1 - 0.025f * i), 0/*旋转*/, AACTitself.Size() * 0.5f, 1 * (1 - 0.00f * i) * originscale, SpriteEffects.None, 0);
					}
				}
			}
			//基准绘制
			Main.EntitySpriteDraw(AACTitself, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, AACTitself.Width, AACTitself.Height), Color.White, NPC.rotation, new Vector2(AACTitself.Width / 2, AACTitself.Height / 2), originscale, SpriteEffects.None, 0);
			//光环部分，参数见ai
			if (AACTcrashed != true) {
				if (nodamage == true && AACTstage < 3)//次数盾
			{
					Texture2D Glow1 = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/AACTGlow").Value;
					if (nodamagetimer <= 20) {
						Main.EntitySpriteDraw(Glow1, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, Glow1.Width, Glow1.Height), Color.White, NPC.rotation, new Vector2(Glow1.Width / 2, Glow1.Height / 2), originscale, SpriteEffects.None, 0);
					}
					else if (nodamagetimer > 30 && nodamagetimer <= 50) {
						Main.EntitySpriteDraw(Glow1, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, Glow1.Width, Glow1.Height), Color.White, NPC.rotation, new Vector2(Glow1.Width / 2, Glow1.Height / 2), originscale, SpriteEffects.None, 0);
					}
					else if (nodamagetimer > 60 && nodamagetimer <= 80) {
						Main.EntitySpriteDraw(Glow1, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, Glow1.Width, Glow1.Height), Color.White, NPC.rotation, new Vector2(Glow1.Width / 2, Glow1.Height / 2), originscale, SpriteEffects.None, 0);
					}
				}
				else {
					Texture2D Glow2 = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/AACTGlow2").Value;
					Main.EntitySpriteDraw(Glow2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, Glow2.Width, Glow2.Height), Color.White, NPC.rotation, new Vector2(Glow2.Width / 2, Glow2.Height / 2), originscale, SpriteEffects.None, 0);
				}
			}

			if (AACTstage >= 2)//二阶段以及之后出现坍缩眼
			{
				Texture2D Eye1 = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/AACTEye").Value;
				Texture2D Eye2 = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/AACTEye2").Value;
				Main.EntitySpriteDraw(Eye1, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, Eye1.Width, Eye1.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(Eye1.Width / 2, Eye1.Height / 2), originscale, SpriteEffects.None, 0);
				Main.EntitySpriteDraw(Eye2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, Eye2.Width, Eye2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(Eye2.Width / 2, Eye2.Height / 2), originscale, SpriteEffects.None, 0);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			spinrotation = (float)Math.PI * movetimer / 60;
			Texture2D ringdot = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/AARD").Value;
			//Texture2D shadespin = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/AACTshadespin").Value;
			Texture2D shadering = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/AACTshadering").Value;
			Texture2D ringline = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/AARL").Value;
			Main.EntitySpriteDraw(ringdot, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, ringdot.Width, ringdot.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(ringdot.Width / 2, ringdot.Height / 2), 1f, SpriteEffects.None, 0);

			List<Vertex> shadespin = new List<Vertex>();
			float FRota = 0 * (float)Math.PI * movetimer / 60;
			float halftextwidth = 360;
			float yscale = 1.333f;
			Vector2 OCenter = NPC.Center - Main.screenPosition;
			Vector2 originvector = new Vector2(-halftextwidth, -halftextwidth).RotatedBy(spinrotation);
			shadespin.Add(new Vertex(OCenter + new Vector2(originvector.X / 1, originvector.Y / yscale).RotatedBy(FRota), new Vector3(0, 0, 1), Color.White));
			originvector = new Vector2(halftextwidth, -halftextwidth).RotatedBy(spinrotation);
			shadespin.Add(new Vertex(OCenter + new Vector2(originvector.X / 1, originvector.Y / yscale).RotatedBy(FRota), new Vector3(1, 0, 1), Color.White));
			originvector = new Vector2(-halftextwidth, halftextwidth).RotatedBy(spinrotation);
			shadespin.Add(new Vertex(OCenter + new Vector2(originvector.X / 1, originvector.Y / yscale).RotatedBy(FRota), new Vector3(0, 1, 1), Color.White));
			originvector = new Vector2(halftextwidth, halftextwidth).RotatedBy(spinrotation);
			shadespin.Add(new Vertex(OCenter + new Vector2(originvector.X / 1, originvector.Y / yscale).RotatedBy(FRota), new Vector3(1, 1, 1), Color.White));

			SpriteBatch shadespindraw = Main.spriteBatch;
			shadespindraw.End();
			shadespindraw.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
			Main.graphics.GraphicsDevice.Textures[0] = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/AACTshadespin").Value;
			Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, shadespin.ToArray(), 0, shadespin.Count - 2);
			shadespindraw.End();
			shadespindraw.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			//OCenter指绘制位置
			//FRota是这个椭圆的压缩朝向
			//Projectile.localAI[0]是旋转角度，可以放在ai或其他位置++

			//Main.EntitySpriteDraw(shadespin, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, shadespin.Width, shadespin.Height), Color.White, spinrotation, new Vector2(shadespin.Width / 2, shadespin.Height / 2), 1f, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(shadering, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, shadering.Width, shadering.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(shadering.Width / 2, shadering.Height / 2), 1f, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(ringline, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, ringline.Width, ringline.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(ringline.Width / 2, ringline.Height / 2), 1f, SpriteEffects.None, 0);
			return true;
		}

		//隐藏下方血条
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			return false;
		}
		//无敌帧
		public override bool? CanBeHitByItem(Player player, Item item) {
			return (AACTstage == 3 && stage3truehealth > 1) || CantBeChoose == true ? false : null;
		}
		//不被敌方弹幕和无来源弹幕攻击
		public override bool? CanBeHitByProjectile(Projectile Projectile) {
			if (Projectile.hostile == true) {
				return false;
			}
			else if (Projectile.friendly == true) {
				return (AACTstage == 3 && stage3truehealth > 1) || CantBeChoose == true ? false : null;
			}
			else {
				return false;
			}
		}
		//时间盾闪避近战+传送
		public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) {
			Player Player = Main.player[Main.myPlayer];

			MISSChance = Main.rand.NextFloat(1);
			if (MISSChance > 0.9f && nodamage == false && AACTstage == 2 && zoomtimer == 0) {
				NPC.dontTakeDamage = true;
				nodamage = true;
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/AACTShield") with { Volume = 1f, Pitch = 0f }, Player.Center);
				//CombatText.NewText(new Rectangle((int)NPC.Center.X, (int)NPC.Center.Y, 10, 10), new Color(200,0,0) , "SHIELD", false, false);
			}
		}
		//时间盾闪避远程+传送
		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			Player Player = Main.player[Main.myPlayer];

			MISSChance = Main.rand.NextFloat(1);
			if (MISSChance > 0.9f && nodamage == false && AACTstage == 2 && zoomtimer == 0) {
				NPC.dontTakeDamage = true;
				nodamage = true;
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/AACTShield") with { Volume = 1f, Pitch = 0f }, Player.Center);
				//CombatText.NewText(new Rectangle((int)NPC.Center.X, (int)NPC.Center.Y, 10, 10), new Color(200,0, 0), "SHIELD", false, false);
			}
		}

		public static int HeadSlot = -1;
		public override void Load() {
			string texture = BossHeadTexture;
			HeadSlot = Mod.AddBossHeadTexture(texture, -1);
		}

		public override void BossHeadSlot(ref int index)//击落后不显示图标和特殊血条
		{
			int slot = HeadSlot;
			index = slot;
		}
		public override void HitEffect(NPC.HitInfo hit)//击中效果
		{
			hitdusttimer++;
			if (stage3dusttimer >= 11) {
				stage3dusttimer = 0;
			}
			if (stage3dusttimer <= 2) {
				Dust.NewDust(NPC.Center, 10, 10, DustID.PurificationPowder, hit.HitDirection, 0, -1, new Color(255, 255, 255), 1f);
			}
			else if (stage3dusttimer > 2 && stage3dusttimer <= 5) {
				Dust.NewDust(NPC.Center, 10, 10, DustID.VilePowder, hit.HitDirection, 0, -1, new Color(255, 255, 255), 1f);
			}
			else if (stage3dusttimer > 5 && stage3dusttimer <= 8) {
				Dust.NewDust(NPC.Center, 10, 10, DustID.Firework_Blue, hit.HitDirection, 0, -1, new Color(255, 255, 255), 1f);
			}
			else if (stage3dusttimer > 8) {
				Dust.NewDust(NPC.Center, 10, 10, DustID.Firework_Pink, hit.HitDirection, 0, -1, new Color(255, 255, 255), 1f);
			}
		}
		#endregion

		#region AI
		#region 变量
		private bool ontransform = false;//转阶段判定器
		private int normalatkcooldown = 120;//平A冷却
		private float aimX;//锚点（x）
		private float aimheight;//锚点（y）
		private float basedistance;//转阶段速度
		private int truestage1to2;//进入第二阶段开始检测器（即一转二开始检测器）
		private int truestage2;//进入第二阶段锁血解除检测器（即一转二结束检测器）
		private int truestage2to3;//二转三开始检测器
		private int timer1;//玩家位置发射计时器
		private int timer2;//固定判定点发射计时器
		private int stage2timer;//第二阶段开始计时
		private float stage3timer = 0;
		private float AACTstage;//弹出文本计次器兼阶段标记
		private int deathtimer;//伪死亡计时
		private int deathcheck;//checkdead触发检测
		private float prefire;//预判速度倍率
		private int playerprefire = 120;//预判攻击间隔
		private float downAcceleration;//死亡坠落加速度
		private float timer1to2;//一转二开始计时
		private float timer2to3;//二转三开始计时
		private float targetX;//转换周期时的瞄准点坐标
		private float targetY;//转换周期时的瞄准点坐标
		private float vx;//横向速度
		private float vy;//纵向速度
		private float stage1to2r;//一阶段转二阶段绕圈半径
		private float stage1to2atkspeed;//一阶段转二阶段绕圈攻击间隔
		private float stage1to2locktime;//一阶段转二阶段锁血时间
		private float stage2to3r;
		private float stage2to3atkspeed;
		private float stage2to3locktime;
		private bool AACTcrashed = false;//是否坠毁？
		private float endtimer;//坠毁计时
		private float movetimer;
		private float endexplodespeed;
		private float diffX;
		private float diffY;
		private bool stg1to2movementsafe = false;
		private float stg1to2safetimer = -1f;
		private bool stg2to3movementsafe = false;
		private float stg2to3safetimer = -1f;
		private bool stgendmovementsafe = false;
		private float stgendsafetimer = -1f;
		private float taildustX;
		private float taildustY;
		private float redlightscale;
		private float bluelightscale;
		private float lighttimer;
		private float stage3dusttimer;
		private float hitdusttimer;
		private bool teleportscalezoom = false;
		private float zoomtimer = 0f;
		private Vector2[] oldPos = new Vector2[25]; //残影个数
		private int frametime = 0;
		private float zoomscale;
		private float zoomframe;
		private bool oppositecolor = false;
		private float oppositetimer = 0;
		private int stage3truehealth = 100;
		private bool isthNPCsummoned = false;
		private bool CantBeChoose = false;
		private float redcolor;
		private float STG2RingRad;
		private float RingThick;
		private float RingLight;
		private float distance;
		private float firstChangeSTGLoop;
		private float secondChangeSTGLoop;
		private float lastChangeSTGLoop;
		private bool InitializedSTGChange1;
		private bool InitializedSTGChange2;
		private int stage3atkloop;
		private int AACTStage3timer = 1;
		private int truestage3;
		private float killtimer;
		private int random1in3;
		private bool isPosLocked = false;
		private Vector2 OldCenter;
		private int stg2musictimer;
		#endregion

		public static int SubNPCType() {
			return ModContent.NPCType<AACTStage3TrueHealth>();
		}
		public static int SubProjType() {
			return ModContent.ProjectileType<CollapsedExplodeAim>();
		}
		public static int MinionType() {
			return ModContent.NPCType<IACTStage3target>();
		}
		public static int EndShowType() {
			return ModContent.ProjectileType<AACTEndShow>();
		}
		//public int CollapsedProjHitTime = 0;

		public override void AI() {
			var newSource = NPC.GetSource_FromThis();
			//AACTPos = NPC.Center;
			Player Player = Main.player[NPC.target];//仇恨判定和死亡判定
			if (!Player.active || Player.dead) {
				if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["LightRing"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene["LightRing"].Deactivate();
				}
				killtimer++;
				if (killtimer <= 60) {
					if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBFence"].IsActive()) {
						Terraria.Graphics.Effects.Filters.Scene.Activate("AACTSTG3RBFence", Player.Center).GetShader().UseIntensity(Math.Max(1f - killtimer / 60, 0f));
					}
					if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBNoise"].IsActive()) {
						Terraria.Graphics.Effects.Filters.Scene.Activate("AACTSTG3RBNoise", Player.Center).GetShader().UseIntensity(Math.Max(1f - killtimer / 60, 0f));
					}
				}
				else {
					if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBFence"].IsActive()) {
						Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBFence"].Deactivate();
					}
					if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBNoise"].IsActive()) {
						Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBNoise"].Deactivate();
					}
				}
				NPC.TargetClosest(false);
				Player = Main.player[NPC.target];
				if (Player.dead) {
					if (NPC.timeLeft > 15) {
						NPC.timeLeft = 15;
					}
					if (NPC.velocity.Y > -8) {
						NPC.velocity.Y -= 0.5f;
					}
					return;
				}
			}

			//if (AACTstage < 2)//自定义血条
			//{
			//	NPC.BossBar = ModContent.GetInstance<AACTBossBarMT>();//一阶段
			//}
			//else if (AACTstage >= 2 && AACTstage < 3) {
			//	NPC.BossBar = ModContent.GetInstance<AACTBossBarEX>();//二阶段
			//}
			//else if (AACTstage == 3) {
			//	NPC.BossBar = ModContent.GetInstance<AACTBossBarNM>();//三阶段
			//}

			NPC.ai[0] = AACTstage;
			NPC.ai[1] = AACTcrashed ? 0 : 1;
			//NPC.ai[2] = NPC.Center.X;
			//NPC.ai[3] = NPC.Center.Y;

			NPC.BossBar = ModContent.GetInstance<AACTBossBar>();

			if (!Main.dedServ) {
				if (AACTstage <= 1) {
					Music = MusicLoader.GetMusicSlot(Mod, "Music/IACTBoss3");//音乐三
				}
				else if (timer1to2 > 0 && timer1to2 <= 120) {
					Music = MusicLoader.GetMusicSlot(Mod, "Assets/Sound/none");//一转二阶段
				}
				else if (AACTstage >= 2) {
					stg2musictimer++;
					if (stg2musictimer <= 1340) {
						Music = MusicLoader.GetMusicSlot(Mod, "Assets/OriginalMusic/AACTintro");
					}
					else {
						Music = MusicLoader.GetMusicSlot(Mod, "Assets/OriginalMusic/AACTloop");
					}
				}
			}

			//动态转角
			NPC.rotation = 0.01f * NPC.velocity.X;

			movetimer++;
			timer1++;
			timer2++;
			lighttimer++;
			frametime++;

			if (frametime % 1 == 0)//每几帧记录一次位置，这个数填5、10、32等一些特殊值好像就不会有残影了，不知道为什么，而且填完也不会导致残影间距改变
			{
				for (int i = oldPos.Length - 1; i > 0; i--) {
					oldPos[i] = oldPos[i - 1];
				}
				oldPos[0] = NPC.Center;
			}

			#region 二阶段特殊机制效果
			//二阶段限制框，ucolor是光环颜色，uintensity是半径，uopacity是粗细，uprogress是亮度渐变
			RingLight = 0.2f * (float)Math.Sin(movetimer * Math.PI / 180) + 0.8f;
			if (AACTstage == 2) {
				stage2timer++;

				if (stage2timer <= 180) {
					STG2RingRad = -512 * (float)Math.Cos(stage2timer * Math.PI / 180) + 512;
					RingThick = -3 * (float)Math.Cos(stage2timer * Math.PI / 180) + 3;
				}
				else { //传送机制写在这里
					STG2RingRad = 1024;
					RingThick = 6;
					if (distance > 66.67f) {
						Player.Center = NPC.Center + new Vector2(0, randtplength).RotateRandom(Math.PI * 2);//传送
						SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/AACTTeleport") with { Volume = 1f, Pitch = 0f }, Player.Center);
					}
				}

				redcolor = (float)(20 * Math.Sin(stage2timer * Math.PI / 180) + 220) / 255;

				if (Main.netMode != NetmodeID.Server && !Terraria.Graphics.Effects.Filters.Scene["LightRing"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene.Activate("LightRing", NPC.Center).GetShader().UseColor(redcolor, 0, 0).UseTargetPosition(NPC.Center + new Vector2(0, 3)).UseIntensity(STG2RingRad).UseOpacity(RingThick).UseProgress(RingLight);
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/AACTShield") with { Volume = 1f, Pitch = 0f }, Player.Center);
				}
				if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["LightRing"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene.Activate("LightRing", NPC.Center).GetShader().UseColor(redcolor, 0, 0).UseTargetPosition(NPC.Center + new Vector2(0, 3)).UseIntensity(STG2RingRad).UseOpacity(RingThick).UseProgress(RingLight);
				}
			}
			else {
				STG2RingRad = float.Lerp(STG2RingRad, 0, 0.01f);
				RingThick = float.Lerp(RingThick, 0, 0.001f);
				if (timer2to3 >= 120) {
					if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["LightRing"].IsActive()) {
						Terraria.Graphics.Effects.Filters.Scene["LightRing"].Deactivate();
					}
				}
			}

			//时间盾闪避持续以及恢复和传送（也包括三阶段的周期传送）
			if (nodamage == true) {
				nodamagetimer++;
				if (nodamagetimer >= 90) {
					NPC.dontTakeDamage = false;
					nodamage = false;
				}
			}
			else {
				nodamagetimer = 0;
				isPosLocked = false;
				if (endtimer == 0) {
					OldCenter = Vector2.Zero;
				}
			}

			if (nodamage == true && NPC.dontTakeDamage == true) {
				teleportscalezoom = true;
				if (nodamagetimer >= 88 && nodamagetimer < 90) {
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/AACTTeleport") with { Volume = 1f, Pitch = 0f }, Player.Center);
					if (AACTstage == 2) {
						NPC.Center = Player.Center + new Vector2(0, randtplength).RotateRandom(Math.PI * 2);//传送
					}
					else if (AACTstage == 3) {
						random1in3 = Main.rand.Next(4);
						//NPC传送到玩家对称点
						if (random1in3 == 0) {
							NPC.Center = 2 * Player.Center - NPC.Center;
						}
						//玩家传送到玩家对称点
						else if (random1in3 == 1) {
							Player.Center = 2 * Player.Center - NPC.Center;
						}
						//NPC传送到玩家对称点，玩家传送到NPC原先位置
						else if (random1in3 == 2) {
							if (!isPosLocked) {
								OldCenter = NPC.Center;
								isPosLocked = true;
							}
							NPC.Center = 2 * Player.Center - NPC.Center;
							Player.Center = OldCenter;
						}
						//玩家传送到NPC对称点，NPC传送到玩家原先位置
						else {
							if (!isPosLocked) {
								OldCenter = Player.Center;
								isPosLocked = true;
							}
							Player.Center = 2 * Player.Center - NPC.Center;
							NPC.Center = OldCenter;
						}
					}
				}
			}
			if (teleportscalezoom == true) {
				zoomtimer++;
				if (zoomtimer >= 30) {//60帧，从30到90，对应0到60
					originscale = (float)Math.Cos(Math.PI * (zoomtimer - 30) / 120);
					zoomframe = 536f * (float)Math.Sin(Math.PI * (zoomtimer - 30) / 120);//0到最大，同步游戏缩放
					zoomscale = 0.5f - (zoomtimer - 30) / 120;//0.5到趋近0
					if (Main.netMode != NetmodeID.Server && !Terraria.Graphics.Effects.Filters.Scene["AACTTP"].IsActive()) {
						Terraria.Graphics.Effects.Filters.Scene.Activate("AACTTP", NPC.Center).GetShader().UseColor(zoomframe, zoomscale, 0).UseTargetPosition(NPC.Center + new Vector2(0, 3) + NPC.velocity * 0.5f);//将zoomframe和zoomscale作为ucolor.x和y
					}
					if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTTP"].IsActive()) {
						Terraria.Graphics.Effects.Filters.Scene.Activate("AACTTP", NPC.Center).GetShader().UseColor(zoomframe, zoomscale, 0).UseTargetPosition(NPC.Center + new Vector2(0, 3) + NPC.velocity * 0.5f);
					}
				}
				else {
					originscale = 1f;
					zoomframe = 0;
					zoomscale = 0.5f;
					if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTTP"].IsActive()) {
						Terraria.Graphics.Effects.Filters.Scene["AACTTP"].Deactivate();
					}
				}
				if (zoomtimer >= 90) {
					teleportscalezoom = false;
				}
			}
			else {
				zoomtimer--;
				if (zoomtimer >= 45) {//45帧,从90到45，对应45到0
					originscale = (float)Math.Cos(Math.PI * (zoomtimer - 45) / 90);
					zoomframe = 0.28f * (float)Math.Sin(Math.PI * (zoomtimer - 45) / 90);//0.28到0
					zoomscale = 0.5f - (zoomtimer - 45) / 90;//趋近0到0.5
					if (Main.netMode != NetmodeID.Server && !Terraria.Graphics.Effects.Filters.Scene["AACTTP"].IsActive()) {
						Terraria.Graphics.Effects.Filters.Scene.Activate("AACTTP", NPC.Center).GetShader().UseColor(zoomframe, zoomscale, 0).UseTargetPosition(NPC.Center + new Vector2(0, 3) + NPC.velocity * 0.5f);//将zoomframe和zoomscale作为ucolor.x和y
					}
					if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTTP"].IsActive()) {
						Terraria.Graphics.Effects.Filters.Scene.Activate("AACTTP", NPC.Center).GetShader().UseColor(zoomframe, zoomscale, 0).UseTargetPosition(NPC.Center + new Vector2(0, 3) + NPC.velocity * 0.5f);
					}
				}
				else {
					originscale = 1f;
					zoomscale = 0.5f;
					if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTTP"].IsActive()) {
						Terraria.Graphics.Effects.Filters.Scene["AACTTP"].Deactivate();
					}
				}
				//保护机制
				if (zoomtimer < 0) {
					zoomtimer = 0;
				}
			}

			//反色&变色
			if (oppositecolor == true) {
				oppositetimer++;
				if (Main.netMode != NetmodeID.Server && !Terraria.Graphics.Effects.Filters.Scene["AACTOC"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene.Activate("AACTOC", NPC.Center).GetShader().UseIntensity(0.5f - oppositetimer / 120);
				}
				if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTOC"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene.Activate("AACTOC", NPC.Center).GetShader().UseIntensity(0.5f - oppositetimer / 120);
				}
				if (Main.netMode != NetmodeID.Server && !Terraria.Graphics.Effects.Filters.Scene["AACTOC2"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene.Activate("AACTOC2", NPC.Center).GetShader().UseIntensity(oppositetimer);
				}
				if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTOC2"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene.Activate("AACTOC2", NPC.Center).GetShader().UseIntensity(oppositetimer);
				}
				if (oppositetimer >= 30) {
					if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTOC2"].IsActive()) {
						Terraria.Graphics.Effects.Filters.Scene["AACTOC2"].Deactivate();
					}
				}
				if (oppositetimer >= 60) {
					oppositecolor = false;
				}
			}
			else {
				if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTOC"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene["AACTOC"].Deactivate();
				}
			}
			#endregion

			#region 基本属性机制

			//攻击机制
			if (Main.masterMode) {
				if (AACTstage == 1) {
					normalatkcooldown = 150;
				}
				else if (AACTstage == 2) {
					normalatkcooldown = 150;
				}
				else if (AACTstage == 3) {
					normalatkcooldown = 120;
				}
				playerprefire = 120;//攻击频率
				prefire = 72;//72倍预判单位，等于准星
			}
			else if (Main.expertMode) {
				if (AACTstage == 1) {
					normalatkcooldown = 180;
				}
				else if (AACTstage == 2) {
					normalatkcooldown = 150;
				}
				else if (AACTstage == 3) {
					normalatkcooldown = 150;
				}
				playerprefire = 150;
				prefire = 60;
			}
			else {
				if (AACTstage == 1) {
					normalatkcooldown = 210;
				}
				else if (AACTstage == 2) {
					normalatkcooldown = 180;
				}
				else if (AACTstage == 3) {
					normalatkcooldown = 150;
				}
				playerprefire = 180;
				prefire = 54;
			}

			if (AACTstage != 2) {
				atkscale = timer2 / (float)normalatkcooldown;
				if (atkscale > 1f) {
					atkscale = 1f;
				}
			}
			else {
				atkscale = timer1 / (float)playerprefire;
				if (atkscale > 1f) {
					atkscale = 1f;
				}
			}

			if (lighttimer >= 240) {
				lighttimer = 0;
			}

			if (lighttimer <= 60) {
				redlightscale = lighttimer / 60;
				bluelightscale = 1f;
			}
			if (lighttimer > 60 && lighttimer <= 120) {
				redlightscale = 1f;
				bluelightscale = 1f - (lighttimer - 60) / 60;
			}
			if (lighttimer > 120 && lighttimer <= 180) {
				redlightscale = 1f;
				bluelightscale = (lighttimer - 120) / 60;
			}
			if (lighttimer > 180 && lighttimer <= 240) {
				redlightscale = 1f - (lighttimer - 180) / 60;
				bluelightscale = 1f;
			}

			if (AACTcrashed != true)//拖尾特效，坠毁时就没有了
			{
				taildustX = 4f * (float)Math.Sin(Math.PI * movetimer / 30);
				taildustY = -4f * (float)Math.Cos(Math.PI * movetimer / 30);
				Lighting.AddLight(NPC.Center, redlightscale, 0f, bluelightscale);//发光
				if (ontransform != true)//常态
				{
					//if (AACTstage == 1) {
					//	Dust taildust = Dust.NewDustPerfect(NPC.Center + new Vector2(2 * taildustX, 2 * taildustY), 20, new Vector2(NPC.velocity.X, NPC.velocity.Y), 0, new Color(255, 255, 255), 1.5f);//蓝色
					//	taildust.noGravity = true;
					//	Dust taildust2 = Dust.NewDustPerfect(NPC.Center + new Vector2(-2 * taildustX, -2 * taildustY), 21, new Vector2(NPC.velocity.X, NPC.velocity.Y), 0, new Color(255, 255, 255), 1.5f);//紫色
					//	taildust2.noGravity = true;
					//}
					/*else*/
					if (AACTstage >= 2) {
						Dust taildust3 = Dust.NewDustPerfect(NPC.Center + new Vector2(taildustX, taildustY), 20, new Vector2(NPC.velocity.X, NPC.velocity.Y), 0, new Color(255, 255, 255), 1.2f);//蓝色
						taildust3.noGravity = true;
						Dust taildust4 = Dust.NewDustPerfect(NPC.Center + new Vector2(-taildustX, -taildustY), 21, new Vector2(NPC.velocity.X, NPC.velocity.Y), 0, new Color(255, 255, 255), 1.2f);//紫色
						taildust4.noGravity = true;
					}
					if (AACTstage >= 3) {
						stage3dusttimer++;
						Vector2 dustPos = NPC.Center + new Vector2(Main.rand.NextFloat(12), 0).RotatedByRandom(MathHelper.TwoPi);
						if (stage3dusttimer >= 11) {
							stage3dusttimer = 0;
						}
						if (stage3dusttimer <= 2) {
							Dust dust1 = Dust.NewDustPerfect(dustPos, 20, Velocity: Vector2.Zero, Scale: 1f);
							dust1.velocity = 2 * dustPos - 2 * NPC.Center;
						}
						else if (stage3dusttimer > 2 && stage3dusttimer <= 5) {
							Dust dust2 = Dust.NewDustPerfect(dustPos, 21, Velocity: Vector2.Zero, Scale: 1f);
							dust2.velocity = 2 * dustPos - 2 * NPC.Center;
						}
						else if (stage3dusttimer > 5 && stage3dusttimer <= 8) {
							Dust dust3 = Dust.NewDustPerfect(dustPos, 132, Velocity: Vector2.Zero, Scale: 1f);
							dust3.velocity = 2 * dustPos - 2 * NPC.Center;
						}
						else if (stage3dusttimer > 8) {
							Dust dust4 = Dust.NewDustPerfect(dustPos, 134, Velocity: Vector2.Zero, Scale: 1f);
							dust4.velocity = 2 * dustPos - 2 * NPC.Center;
						}
					}
				}
				else//转阶段,叠加后是紫色
				{
					if (stgendsafetimer < 0) {
						Dust traildust = Dust.NewDustPerfect(NPC.Center + new Vector2(0, 3), 132, new Vector2(0f, 0f), 0, new Color(255, 255, 255), 1.5f);//蓝色
						traildust.noGravity = true;
						Dust traildust2 = Dust.NewDustPerfect(NPC.Center + new Vector2(0, 3), 134, new Vector2(0f, 0f), 0, new Color(255, 255, 255), 1.5f);//梅红色
						traildust2.noGravity = true;
					}
				}
			}

			//阶段判定区
			if (AACTstage == 0) {
				Main.NewText(Language.GetTextValue("甯濆浗鐐伀涓灑鍏堝厗鑰�"), 240, 0, 0);//出场提示语
				AACTstage = 1;//出场时文本计数=1，也即第一阶段
			}
			if (AACTstage == 1 && NPC.life < NPC.lifeMax * expertHealthFrac)//第二阶段提示语（12000/16000；21600/24000；32400/36000）
			{
				Main.NewText(Language.GetTextValue("閭瓟鐐鍥犳褰撲换浣曚竴姩璐熻矗鏃讹紝"), 240, 0, 0);
				truestage1to2 = 1;//开始一转二
				AACTstage = 1.5f;//文本计数=1.5，也即一转二阶段
			}
			if (AACTstage == 2 && NPC.life < NPC.lifeMax * expertHealthFrac / 2)//第三阶段提示语（6000/16000；10800/24000；16200/36000）
			{
				Main.NewText(Language.GetTextValue("閭瓟绔嬭冻钀ㄧ背鐨勬槦闂ㄤ镜铓鍒堕€寰佹湇鍏ㄤ笘鐣�"), 240, 0, 0);
				truestage2to3 = 1;//开始二转三
				AACTstage = 2.5f;//文本计数=2.5，也即二转三阶段
			}

			if (NPC.life <= NPC.lifeMax * expertHealthFrac)//三个阶段更改伤害值和护盾
			{
				if (NPC.life <= NPC.lifeMax * expertHealthFrac / 2)//三阶段
				{
					NPC.defense = 30;
					NPC.damage = 128;
				}
				else//二阶段
				{
					NPC.defense = 40;
					NPC.damage = 96;
				}
			}
			else//一阶段
			{
				NPC.defense = 50;
				NPC.damage = 72;
			}

			aimX = 0;
			aimheight = 0;

			//主体AI
			if (NPC.life > 1 && (NPC.dontTakeDamage != true || nodamage == true || AACTstage == 3)) {
				//2阶段之后的玩家判定区
				if ((truestage2 == 1 || truestage3 == 1) && timer1 >= playerprefire) {
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Assets/Sound/ImperialArtilleyCoreTargeteer/AlertPro") with { Volume = 1f, Pitch = 0f }, NPC.Center);
					Projectile.NewProjectile(newSource, Player.Center.X + prefire * Player.velocity.X, Player.Center.Y + prefire * Player.velocity.Y, 0, 0, ModContent.ProjectileType<CollapsedHitboxblueCore>(), 0, 0, 0, 0, 0);
					timer1 = 0;
				}

				//固定判定区，数秒一个
				if (timer2 >= normalatkcooldown && AACTstage <= 2) {
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Assets/Sound/ImperialArtilleyCoreTargeteer/Alert") with { Volume = 1f, Pitch = 0f }, NPC.Center);
					Projectile.NewProjectile(newSource, Player.Center.X + Main.rand.NextFloat(-128, 128), Player.Center.Y + Main.rand.NextFloat(-128, 128), 0, 0, ModContent.ProjectileType<ExplodeAim>(), NPC.damage, 0f, 0, 0);
					timer2 = 0;
				}

				//移动方式
				Vector2 velDiff = NPC.velocity - Player.velocity;
				float ax = 0.3f;
				float ay = 0.2f;
				int haltDirectionX = velDiff.X > 0 ? 1 : -1;
				int haltDirectionY = velDiff.Y > 0 ? 1 : -1;
				float haltPointX = NPC.Center.X + haltDirectionX * (velDiff.X * velDiff.X) / (2 * ax) + aimX;
				float haltPointY = NPC.Center.Y + haltDirectionY * (velDiff.Y * velDiff.Y) / (2 * ay) + aimheight;
				diffX = Player.Center.X - NPC.Center.X;
				diffY = Player.Center.Y - NPC.Center.Y;
				distance = (float)Math.Sqrt(Math.Pow(diffX / 16, 2) + Math.Pow(diffY / 16, 2));
				basedistance = 2 * distance + 20f;//2倍到玩家的距离（格数）+基准速度20f
				if (basedistance > 40f) {
					basedistance = 40f;
				}
				if (basedistance < 20f) {
					basedistance = 20f;
				}
				if (Player.Center.X > haltPointX) {
					NPC.velocity.X += ax;
				}
				else {
					NPC.velocity.X -= ax;
				}
				NPC.velocity.X = Math.Min(vx, Math.Max(-vx, NPC.velocity.X));

				if (Player.Center.Y > haltPointY) {
					NPC.velocity.Y += ay;
				}
				else {
					NPC.velocity.Y -= ay;
				}
				NPC.velocity.Y = Math.Min(vy, Math.Max(-vy, NPC.velocity.Y));

				if (Math.Sqrt(Math.Pow(diffX, 2) + Math.Pow(diffY, 2)) >= 96 * 16)//距离远于96格
				{
					ax = 0.6f;
					ay = 0.4f;
					if (Main.masterMode) {
						if (AACTstage == 3) {
							vx = 20f;
							vy = 16f;
						}
						if (AACTstage == 2) {
							vx = 16f;
							vy = 14.4f;
						}
						if (AACTstage == 1) {
							vx = 14.4f;
							vy = 12f;
						}
					}
					else if (Main.expertMode) {
						if (AACTstage == 3) {
							vx = 16f;
							vy = 14.4f;
						}
						if (AACTstage == 2) {
							vx = 15f;
							vy = 13.5f;
						}
						if (AACTstage == 1) {
							vx = 12f;
							vy = 10f;
						}
					}
					else {
						if (AACTstage == 3) {
							vx = 15f;
							vy = 13.5f;
						}
						if (AACTstage == 2) {
							vx = 14.4f;
							vy = 12f;
						}
						if (AACTstage == 1) {
							vx = 12f;
							vy = 10f;
						}
					}
				}
				else//基础移动速度设置
				{
					ax = 0.3f;
					ay = 0.2f;
					if (Main.masterMode) {
						if (AACTstage == 3) {
							vx = 7.2f;
							vy = 7.2f;
						}
						if (AACTstage == 2) {
							vx = 6.4f;
							vy = 6.4f;
						}
						if (AACTstage == 1) {
							vx = 5.6f;
							vy = 5.6f;
						}
					}
					else if (Main.expertMode) {
						if (AACTstage == 3) {
							vx = 6.4f;
							vy = 6.4f;
						}
						if (AACTstage == 2) {
							vx = 6f;
							vy = 6f;
						}
						if (AACTstage == 1) {
							vx = 5.2f;
							vy = 5.2f;
						}
					}
					else {
						if (AACTstage == 3) {
							vx = 6f;
							vy = 6f;
						}
						if (AACTstage == 2) {
							vx = 5.6f;
							vy = 5.6f;
						}
						if (AACTstage == 1) {
							vx = 5f;
							vy = 5f;
						}
					}
				}
			}
			#endregion

			#region 转阶段
			if (truestage1to2 == 1)//一二阶段转阶段锁血以及攻击
			{
				if ((int)timer1to2 == 30) {
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Assets/Sound/ImperialArtilleyCoreTargeteer/AACTstart") with { Volume = 0.8f, Pitch = 0f }, Player.Center);
				}

				if (timer1to2 >= 120) {
					ontransform = true;
				}

				NPC.life = (int)(NPC.lifeMax * expertHealthFrac);//先卡在线上

				if (InitializedSTGChange1 == false) {
					if (Main.masterMode) {
						stage1to2locktime = Main.rand.Next(420, 451);
						stage1to2r = Main.rand.Next(270, 331);
						stage1to2atkspeed = Main.rand.Next(16, 25);
						firstChangeSTGLoop = Main.rand.Next(240, 361);
					}
					else if (Main.expertMode) {
						stage1to2locktime = Main.rand.Next(420, 481);
						stage1to2r = Main.rand.Next(360, 421);
						stage1to2atkspeed = Main.rand.Next(24, 33);
						firstChangeSTGLoop = Main.rand.Next(150, 241);
					}
					else {
						stage1to2locktime = Main.rand.Next(450, 511);
						stage1to2r = Main.rand.Next(480, 511);
						stage1to2atkspeed = Main.rand.Next(36, 46);
						firstChangeSTGLoop = Main.rand.Next(60, 151);
					}
					InitializedSTGChange1 = true;
				}

				if (stg1to2safetimer <= stage1to2locktime - 300 + firstChangeSTGLoop) {
					timer1to2++;
					//NPC.dontTakeDamage = true;
					CantBeChoose = true;
					if (timer1to2 < 120) {
						NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.05f);
						if ((int)timer1to2 == 115) {
							Projectile.NewProjectile(newSource, NPC.Center.X, NPC.Center.Y, 0, 0, ModContent.ProjectileType<IACTScreenWave>(), 0, 0f, 0, 0);
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/AACTTeleport") with { Volume = 1f, Pitch = 0f }, Player.Center);
							DeathTP();
						}
						if ((int)timer1to2 == 119) {
							DeathTP();
							oppositecolor = true;
						}
					}
					else if (timer1to2 >= 120 && timer1to2 < 180) {
						//NPC.life = (int)(NPC.lifeMax * expertHealthFrac);
						AACTstage = 2;
						if (timer1to2 == 150 || timer1to2 == 179) {
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/AACTTeleport") with { Volume = 1f, Pitch = 0f }, Player.Center);
							DeathTP();
						}
					}
					else if (timer1to2 >= 180 && timer1to2 < 300) {
						Vector2 targetPosition = Player.Center + new Vector2(targetX, -stage1to2r);
						Vector2 targetv = basedistance * (targetPosition - NPC.Center).SafeNormalize(Vector2.One);
						NPC.velocity = Vector2.Lerp(NPC.velocity, 3 * targetv, 0.03f);
					}
					else if (timer1to2 >= 300) {
						if (-12 <= diffX && diffX <= 12 && stage1to2r - 12 <= diffY && diffY <= stage1to2r + 12) {
							stg1to2movementsafe = true;
						}

						if (stg1to2movementsafe != true)//防跳变保护机制
						{
							Vector2 targetPosition = Player.Center + new Vector2(targetX, -stage1to2r);
							Vector2 targetv = basedistance * (targetPosition - NPC.Center).SafeNormalize(Vector2.One);
							NPC.velocity = Vector2.Lerp(NPC.velocity, 3 * targetv, 0.03f);
						}
						else {
							stg1to2safetimer++;
						}

						if ((int)stg1to2safetimer == 0) {
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/iactstage1to2") with { Volume = 1f, Pitch = 0f }, Player.Center);
						}

						if (stg1to2safetimer >= 0) {
							targetX = (float)(stage1to2r * Math.Sin(2 * stg1to2safetimer * Math.PI / (stage1to2locktime - 300)));
							targetY = (float)(-stage1to2r * Math.Cos(2 * stg1to2safetimer * Math.PI / (stage1to2locktime - 300)));
							NPC.velocity = new Vector2(Player.Center.X, Player.Center.Y) + new Vector2(targetX, targetY) - NPC.Center;//期间的位置变动
							if ((int)stg1to2safetimer % stage1to2atkspeed == 0) {
								Projectile.NewProjectile(newSource, NPC.Center.X, NPC.Center.Y, 0, 0, ModContent.ProjectileType<ExplodeAimPro>(), NPC.damage, 0f, 0, 0);
								resetChangeSTG();
							}
						}
					}
				}
				else {
					Main.NewText(Language.GetTextValue("鏃犲灎鍥炶崱鍏嬮浄鏉撅紝"), 240, 0, 0);
					truestage1to2 = 2;
					truestage2 = 1;
					//NPC.dontTakeDamage = false;
					CantBeChoose = false;
					ontransform = false;
					targetX = 0;
				}
			}

			//二三阶段转阶段锁血以及攻击
			if (truestage2to3 == 1) {
				if (timer2to3 >= 240) {
					ontransform = true;
				}

				NPC.life = (int)(NPC.lifeMax * expertHealthFrac / 2 * Math.Max(2 - timer2to3 / 240, 1));//变动，lifemax*exp/2是三阶段血量

				if (InitializedSTGChange2 == false) {
					if (Main.masterMode) {
						stage2to3locktime = Main.rand.Next(360, 373);
						stage2to3r = Main.rand.Next(240, 331);
						stage2to3atkspeed = Main.rand.Next(16, 25);
						secondChangeSTGLoop = Main.rand.Next(360, 481);
					}
					else if (Main.expertMode) {
						stage2to3locktime = Main.rand.Next(330, 361);
						stage2to3r = Main.rand.Next(330, 421);
						stage2to3atkspeed = Main.rand.Next(24, 33);
						secondChangeSTGLoop = Main.rand.Next(240, 361);
					}
					else {
						stage2to3locktime = Main.rand.Next(300, 331);
						stage2to3r = Main.rand.Next(390, 481);
						stage2to3atkspeed = Main.rand.Next(32, 43);
						secondChangeSTGLoop = Main.rand.Next(120, 241);
					}
					InitializedSTGChange2 = true;
				}

				if (stg2to3safetimer <= stage2to3locktime - 480 + secondChangeSTGLoop) {
					timer2to3++;
					//NPC.dontTakeDamage = true;
					CantBeChoose = true;
					if (timer2to3 < 240) {
						NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.05f);
						if ((int)timer2to3 == 235) {
							Projectile.NewProjectile(newSource, NPC.Center.X, NPC.Center.Y, 0, 0, ModContent.ProjectileType<IACTScreenWave>(), 0, 0f, 0, 0);
						}
						if ((int)timer2to3 == 238) {
							Player.GetModPlayer<ShakeEffectPlayer>().screenShakeOnlyOnY = true;//纵向震动
							Player.GetModPlayer<ShakeEffectPlayer>().screenShakeTime = 30;//屏幕抖动
						}
					}
					else if (timer2to3 >= 240 && timer2to3 < 360) {
						if ((int)timer2to3 == 240) {
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Assets/Sound/ImperialArtilleyCoreTargeteer/IACTStageChangeTremor") with { Volume = 1f, Pitch = 0f }, Player.Center);
							AACTstage = 3;
						}
						if (timer2to3 == 240 || timer2to3 == 270 || timer2to3 == 300 || timer2to3 == 330 || timer2to3 == 359) {
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/AACTTeleport") with { Volume = 1f, Pitch = 0f }, Player.Center);
							DeathTP();
						}
						//NPC.life = (int)(NPC.lifeMax * expertHealthFrac / 2);
					}
					else if (timer2to3 >= 360 && timer2to3 < 480) {
						Vector2 targetPosition = Player.Center + new Vector2(targetX, -stage2to3r);
						Vector2 targetv = basedistance * (targetPosition - NPC.Center).SafeNormalize(Vector2.One);
						NPC.velocity = Vector2.Lerp(NPC.velocity, 3 * targetv, 0.03f);
					}
					else if (timer2to3 >= 480) {
						if ((int)(Player.Center.X - NPC.Center.X) >= -12 && (int)(Player.Center.X - NPC.Center.X) <= 12 && (int)(Player.Center.Y - NPC.Center.Y) >= stage2to3r - 12 && (int)(Player.Center.Y - NPC.Center.Y) <= stage2to3r + 12) {
							stg2to3movementsafe = true;
						}

						if (stg2to3movementsafe != true)//防跳变保护机制
						{
							Vector2 targetPosition = Player.Center + new Vector2(targetX, -stage2to3r);
							Vector2 targetv = basedistance * (targetPosition - NPC.Center).SafeNormalize(Vector2.One);
							NPC.velocity = Vector2.Lerp(NPC.velocity, 3 * targetv, 0.03f);
						}
						else {
							stg2to3safetimer++;
						}
						if ((int)stg2to3safetimer == 0) {
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/iactstage1to2") with { Volume = 1f, Pitch = 0f }, Player.Center);
						}
						if ((int)stg2to3safetimer >= 0) {
							targetX = (float)(stage2to3r * Math.Sin(2 * stg2to3safetimer * Math.PI / (stage2to3locktime - 480)));
							targetY = (float)(-stage2to3r * Math.Cos(2 * stg2to3safetimer * Math.PI / (stage2to3locktime - 480)));
							NPC.velocity = new Vector2(Player.Center.X, Player.Center.Y) + new Vector2(targetX, targetY) - NPC.Center;//期间的位置变动
							if ((int)stg2to3safetimer % stage2to3atkspeed == 0) {
								Projectile.NewProjectile(newSource, NPC.Center.X, NPC.Center.Y, 0, 0, ModContent.ProjectileType<ExplodeAimPro>(), NPC.damage, 0f, 0, 0);
								resetChangeSTG2();
							}
						}
					}
				}
				else {
					Main.NewText(Language.GetTextValue("濡傛€濈淮鑸贩娌岀殑鏈兘銆�"), 240, 0, 0);
					truestage2to3 = 2;
					truestage3 = 1;
					//NPC.dontTakeDamage = false;
					CantBeChoose = false;
					ontransform = false;
					targetX = 0;
				}
			}
			#endregion

			if (AACTstage == 3)//第三阶段
			{
				stage3timer++;
				//屏幕效果
				float fenceintensity = Math.Min(stage3timer / 60, 0.999f);
				if (Main.netMode != NetmodeID.Server && !Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBFence"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene.Activate("AACTSTG3RBFence", Player.Center).GetShader().UseIntensity(fenceintensity);
				}
				if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBFence"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene.Activate("AACTSTG3RBFence", Player.Center).GetShader().UseIntensity(fenceintensity);
				}
				float noiseintensity = Math.Min(Math.Max(1.5f - stage3truehealth / 15, 0.25f), 0.999f);
				if (Main.netMode != NetmodeID.Server && !Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBNoise"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene.Activate("AACTSTG3RBNoise", Player.Center).GetShader().UseIntensity(noiseintensity);
				}
				if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBNoise"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene.Activate("AACTSTG3RBNoise", Player.Center).GetShader().UseIntensity(noiseintensity);
				}
				//假血量机制
				if (isthNPCsummoned == false) {
					var entitySource = NPC.GetSource_FromAI();
					NPC SubNPC = NPC.NewNPCDirect(entitySource, (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<AACTStage3TrueHealth>(), NPC.whoAmI);
					AACTStage3TrueHealth SubTH = (AACTStage3TrueHealth)SubNPC.ModNPC;
					SubTH.Host = NPC.whoAmI;
					//NPC.NewNPC(entitySource, (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<AACTStage3TrueHealth>(),NPC.whoAmI);
					//stage3truehealth = 100;
					stage3truehealth = SubNPC.life;
					//NPC.life = (int)(NPC.lifeMax * expertHealthFrac / 4);
					isthNPCsummoned = true;
				}
				else {
					for (int i = 0; i < Main.maxNPCs; i++) {
						NPC SeekForNPCs = Main.npc[i];
						if (SeekForNPCs.active && SeekForNPCs.type == SubNPCType() && SeekForNPCs.ModNPC is AACTStage3TrueHealth SubTH) {
							if (SubTH.Host == NPC.whoAmI) {
								stage3truehealth = SeekForNPCs.life;
							}
						}
					}
					NPC.dontTakeDamage = true;
				}

				if (stage3truehealth <= 1) {
					NPC.dontTakeDamage = false;
					NPC.life = 1;
				}
				//三阶段运行体系
				if (Main.masterMode) {
					stage3atkloop = 450;
				}
				else if (Main.expertMode) {
					stage3atkloop = 540;
				}
				else {
					stage3atkloop = 630;
				}

				NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.01f);
				if (truestage3 == 1)//开启定点系统
				{
					NPC minionNPC = Main.npc[NPC.NewNPC(newSource, (int)(2 * Player.Center.X - NPC.Center.X), (int)(2 * Player.Center.Y - NPC.Center.Y), ModContent.NPCType<IACTStage3target>(), NPC.whoAmI)];
					if (minionNPC.ModNPC is IACTStage3target minion) {
						minion.ParentIndex = NPC.whoAmI;
					}
					truestage3 = 2;
				}
				if (stage3timer - stage2to3locktime >= 0 && truestage3 == 2) {
					AACTStage3timer++;
					if ((AACTStage3timer + 180) % stage3atkloop == 0) {
						nodamage = true;
						NPC.dontTakeDamage = true;
					}
				}
			}

			if (NPC.life <= 1)//坠毁及其保护机制
			{
				if (deathcheck == 1)//触发checkdead之后
				{
					if (stgendsafetimer < 1500) {
						endtimer++;

						if (endtimer <= 180) {
							if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBFence"].IsActive()) {
								Terraria.Graphics.Effects.Filters.Scene.Activate("AACTSTG3RBFence", Player.Center).GetShader().UseIntensity(Math.Max(1 - endtimer / 60, 0));
							}
							if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBNoise"].IsActive()) {
								Terraria.Graphics.Effects.Filters.Scene.Activate("AACTSTG3RBNoise", Player.Center).GetShader().UseIntensity(Math.Max(1 - endtimer / 60, 0));
							}
						}
						else {
							if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBFence"].IsActive()) {
								Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBFence"].Deactivate();
							}
							if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBNoise"].IsActive()) {
								Terraria.Graphics.Effects.Filters.Scene["AACTSTG3RBNoise"].Deactivate();
							}
						}

						if (endtimer < 180)//先照例悬停
						{
							NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.05f);
							OldCenter = Player.Center;
						}
						else if (endtimer >= 180 && endtimer < 360) {
							if (AACTstage == 3) {
								Main.NewText(Language.GetTextValue("涔岃惃鏂渶娣遍們鐨勭瀵嗕笉瀹逛粬浜虹鎺紝"), 240, 0, 0);
								OldCenter = Player.Center;
								ontransform = true;
								AACTstage += 1;
							}
							Vector2 targetPosition = OldCenter + new Vector2(targetX, -480);
							Vector2 targetv = basedistance * (targetPosition - NPC.Center).SafeNormalize(Vector2.One);
							NPC.velocity = Vector2.Lerp(NPC.velocity, 2 * targetv, 0.03f);
						}
						else if (endtimer >= 360) {
							if ((int)(OldCenter.X - NPC.Center.X) >= -16 && (int)(OldCenter.X - NPC.Center.X) <= 16 && (int)(OldCenter.Y - NPC.Center.Y) >= 468 && (int)(OldCenter.Y - NPC.Center.Y) <= 492) {
								stgendmovementsafe = true;
							}

							if (stgendmovementsafe != true)//防跳变保护机制
							{
								Vector2 targetPosition = OldCenter + new Vector2(targetX, -480);
								Vector2 targetv = basedistance * (targetPosition - NPC.Center).SafeNormalize(Vector2.One);
								NPC.velocity = Vector2.Lerp(NPC.velocity, 2 * targetv, 0.05f);
							}
							else {
								stgendsafetimer++;
							}
							if ((int)stgendsafetimer == 0) {
								Projectile.NewProjectile(newSource, NPC.Center.X, NPC.Center.Y, 0, 0, ModContent.ProjectileType<AACTEndShow>(), 0, 0f, 0, 0);
								//Projectile Endshow = Projectile.NewProjectileDirect(newSource, NPC.Center, Vector2.Zero, ModContent.ProjectileType<AACTEndSHow>(), 0, 0, NPC.whoAmI);
								//AACTEndSHow SubEnd = (AACTEndSHow)Endshow.ModProjectile;
								//SubEnd.ParentIndex = NPC.whoAmI;
								//SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/iactstage1to2") with { Volume = 1f, Pitch = 0f }, Player.Center);
							}
							if ((int)stgendsafetimer >= 0) {
								float t = 1 - stgendsafetimer / 2500;
								targetX = (float)(480 * (t - t * stgendsafetimer / 1500) * Math.Sin(2 * stgendsafetimer * Math.PI / 180 / (1 - stgendsafetimer / 1800)));
								targetY = (float)(-480 * (t - t * stgendsafetimer / 1500) * Math.Cos(2 * stgendsafetimer * Math.PI / 180 / (1 - stgendsafetimer / 1800)));
								NPC.velocity = new Vector2(OldCenter.X, OldCenter.Y) + new Vector2(targetX, targetY) - NPC.Center;//期间的位置变动
								endexplodespeed = (int)Main.rand.NextFloat(0, 60);
								if ((int)endexplodespeed >= 57) {
									Projectile.NewProjectile(newSource, NPC.Center.X, NPC.Center.Y, 0, 0, ModContent.ProjectileType<ExplodeAimPro>(), NPC.damage, 0f, 0, 0);
								}
							}
						}
					}
					else {
						AACTcrashed = true;
						deathtimer++;
						if (deathtimer <= 1) {
							Projectile.NewProjectile(newSource, NPC.Center.X, NPC.Center.Y, 0, 0, ModContent.ProjectileType<IACTScreenWave>(), 0, 0f, 0, 0);
						}
						if (deathtimer <= 360) {
							NPC.velocity.X = NPC.velocity.X * (360 - deathtimer) / 360;
							NPC.velocity.Y = NPC.velocity.Y * (360 - deathtimer) / 360;
						}
						else {
							NPC.velocity.X = 0;
							NPC.velocity.Y = 0;
						}
						if (deathtimer == 168) {
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Assets/Sound/ImperialArtilleyCoreTargeteer/AACTendteleport") with { Volume = 1f, Pitch = 0f }, Player.Center);
						}

						if (deathtimer == 170 || deathtimer == 230 || deathtimer == 260 || deathtimer == 275 || deathtimer == 285 || deathtimer == 295 || deathtimer == 305 || deathtimer == 310 || deathtimer == 315 || deathtimer == 320) {
							DeathTP();
						}

						if (deathtimer == 320) {
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Assets/Sound/ImperialArtilleyCoreTargeteer/AACTendboom") with { Volume = 1f, Pitch = 0f }, Player.Center);
						}

						if (deathtimer >= 380) {
							deathcheck = 2;
							Projectile.NewProjectile(newSource, NPC.Center.X, NPC.Center.Y, 0, 0, ModContent.ProjectileType<IACTScreenWave>(), 0, 0f, 0, 0);
							Projectile.NewProjectile(newSource, NPC.Center.X, NPC.Center.Y, 0, 0, ModContent.ProjectileType<AACTDeathdust>(), 0, 0f, 0, 0);//爆炸粒子
						}

						Vector2 dustPos = NPC.Center + new Vector2(Main.rand.NextFloat(12), 0).RotatedByRandom(MathHelper.TwoPi);
						for (int i = 0; i < 2; i++) {
							Dust deathdust1 = Dust.NewDustPerfect(dustPos, 20, Velocity: Vector2.Zero, Scale: 1.25f);
							deathdust1.velocity = 4 * dustPos - 4 * NPC.Center;
							Dust deathdust2 = Dust.NewDustPerfect(dustPos, 21, Velocity: Vector2.Zero, Scale: 1.25f);
							deathdust2.velocity = 4 * dustPos - 4 * NPC.Center;
							Dust deathdust3 = Dust.NewDustPerfect(dustPos, 132, Velocity: Vector2.Zero, Scale: 1.5f);
							deathdust3.velocity = 3 * dustPos - 3 * NPC.Center;
							Dust deathdust4 = Dust.NewDustPerfect(dustPos, 134, Velocity: Vector2.Zero, Scale: 1.5f);
							deathdust4.velocity = 3 * dustPos - 3 * NPC.Center;
						}
					}
				}
				if (deathcheck == 2)//赐死
				{
					NPC.dontTakeDamage = false;
					NPC.life = 0;
					NPC.checkDead();
				}
			}
		}

		private void DeathTP()//传送
		{
			Player Player = Main.player[NPC.target];
			NPC.velocity = new Vector2(NPC.Center.X, NPC.Center.Y) + new Vector2(-80 + Main.rand.NextFloat(0, 160), -80 + Main.rand.NextFloat(0, 160)) - NPC.Center;//传送
		}

		private void resetChangeSTG() {//刷新投弹属性
			Player Player = Main.player[NPC.target];
			if (Main.masterMode) {
				stage1to2r = Main.rand.Next(240, 421);
				stage1to2atkspeed = Main.rand.Next(16, 25);
			}
			else if (Main.expertMode) {
				stage1to2r = Main.rand.Next(360, 481);
				stage1to2atkspeed = Main.rand.Next(24, 33);
			}
			else {
				stage1to2r = Main.rand.Next(450, 511);
				stage1to2atkspeed = Main.rand.Next(36, 46);
			}
			SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/AACTTeleport") with { Volume = 1f, Pitch = 0f }, Player.Center);
			return;
		}

		private void resetChangeSTG2() {//刷新投弹属性
			Player Player = Main.player[NPC.target];
			if (Main.masterMode) {
				stage2to3r = Main.rand.Next(240, 331);
				stage2to3atkspeed = Main.rand.Next(16, 25);
			}
			else if (Main.expertMode) {
				stage2to3r = Main.rand.Next(330, 421);
				stage2to3atkspeed = Main.rand.Next(24, 33);
			}
			else {
				stage2to3r = Main.rand.Next(390, 481);
				stage2to3atkspeed = Main.rand.Next(32, 43);
			}
			SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/AACTTeleport") with { Volume = 1f, Pitch = 0f }, Player.Center);
			return;
		}

		public override bool CheckDead()//锁血及锁血解除
		{
			Player Player = Main.player[Main.myPlayer];
			if (deathcheck == 2)//死亡
			{
				Main.NewText(Language.GetTextValue("瀹冨氨鐞嗗簲琚姽娑堛€�"), 138, 0, 18);
				return true;
			}
			else//锁血
			{
				NPC.active = true;
				NPC.dontTakeDamage = true;
				NPC.life = 1;
				deathcheck = 1;
				return false;
			}
		}
		#endregion
	}

	public class AACTIntro : ModNPC
	{
		public override string Texture => ArknightsMod.noTexture;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 1;//贴图帧数
		}

		public override void SetDefaults() {
			NPC.lifeMax = 1;
			NPC.damage = 0;
			NPC.defense = 0;
			NPC.knockBackResist = 0f;//击退抗性，0f为最高，1f为最低
			NPC.width = 1;
			NPC.height = 1;
			NPC.noGravity = true;//无引力
			NPC.noTileCollide = true;//不与物块相撞
			NPC.lavaImmune = true;//免疫岩浆
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit4;//金属声
			NPC.DeathSound = SoundID.NPCDeath14;//爆炸声
			NPCID.Sets.ImmuneToAllBuffs[Type] = true;//免疫所有debuff
			NPC.friendly = true;
			NPC.dontTakeDamageFromHostiles = true;
			NPC.dontTakeDamage = true;
		}

		private float timer = 0;
		public override void AI() {
			Player Player = Main.player[Main.myPlayer];
			NPC.Center = Player.Center;
			timer++;
			if (timer == 330) {
				Main.NewText(Language.GetTextValue("钀ㄧ背鍥犻潪鍐板師鐩樿笧鐨勯偑榄�"), 138, 0, 18);
				NPC.NewNPC(Terraria.Entity.GetSource_TownSpawn(), (int)Player.Center.X, (int)Player.Center.Y - 1200, NPCType<AACT>());
				NPC.life = 0;
			}
		}
	}

	public class AACTDeathdust : ModProjectile//死亡粒子效果触发器
	{
		public override string Texture => ArknightsMod.noTexture;
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 60;
			Projectile.alpha = 0;
			Projectile.damage = 0;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = false;
		}

		public override void AI() {
			Player Player = Main.player[Main.myPlayer];
			Player.GetModPlayer<ShakeEffectPlayer>().screenShakeOnlyOnY = true;//只在纵向震动
			Player.GetModPlayer<ShakeEffectPlayer>().screenShakeTime = 30;
			Vector2 dustPos = Projectile.Center + new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(Math.PI);
			for (int i = 0; i < 5; i++) {
				Dust deathdust1 = Dust.NewDustPerfect(dustPos, 20, Velocity: Vector2.Zero, Scale: 2f);
				deathdust1.velocity = 5 * dustPos - 5 * Projectile.Center;
				Dust deathdust2 = Dust.NewDustPerfect(dustPos, 21, Velocity: Vector2.Zero, Scale: 2f);
				deathdust2.velocity = 5 * dustPos - 5 * Projectile.Center;
				Dust deathdust3 = Dust.NewDustPerfect(dustPos, 132, Velocity: Vector2.Zero, Scale: 2.5f);
				deathdust3.velocity = 4 * dustPos - 4 * Projectile.Center;
				Dust deathdust4 = Dust.NewDustPerfect(dustPos, 134, Velocity: Vector2.Zero, Scale: 2.5f);
				deathdust4.velocity = 4 * dustPos - 4 * Projectile.Center;
			}
		}
	}

	public class CollapsedHitboxblueCore : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 8;
		}
		public override void SetDefaults() {
			Projectile.width = 110;
			Projectile.height = 110;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 120;
			Projectile.alpha = 50;
			Projectile.damage = 0;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = false;
		}

		private float click = 0;
		private float prefire;
		private int randt = 1;
		private int frametimer;
		private int skilltimer = 0;

		public override void AI()//ai
		{
			var newSource = Projectile.GetSource_FromThis();
			Player Player = Main.player[Main.myPlayer];
			click++;
			skilltimer++;
			frametimer++;
			if (frametimer > 3) {
				randt = Main.rand.Next(2, 31);
				frametimer = 0;
			}
			Projectile.frame += (int)Main.time % randt == 0 ? Main.rand.Next(1, 9) : 0;
			Projectile.frame %= 8;

			if (Main.masterMode) {
				prefire = 48;
			}
			else if (Main.expertMode) {
				prefire = 42;
			}
			else {
				prefire = 36;
			}

			if (Projectile.scale > 1f) {
				Projectile.scale = 1f;
			}
			if (Projectile.scale < 0f) {
				Projectile.scale = 0f;
			}
			if (Projectile.alpha < 0) {
				Projectile.alpha = 0;
			}
			if (Projectile.alpha > 255) {
				Projectile.alpha = 255;
			}

			Projectile.Center = Player.Center + prefire * Player.velocity + new Vector2(Main.rand.NextFloat(-64, 64), Main.rand.NextFloat(-64, 64));
			Projectile.scale = 3.3f / (click / 24 + 0.5f) / (click / 24 - 5.5f) + 1.4f;
			Projectile.alpha = (int)(0.1 * click * click - 12 * click + 255);

			if (skilltimer == 120) {
				if (Player.velocity.X * Player.velocity.X + Player.velocity.Y * Player.velocity.Y >= 100) {
					Projectile.NewProjectile(newSource, Projectile.Center.X + 0.25f * prefire * Player.velocity.X, Projectile.Center.Y + 0.25f * prefire * Player.velocity.Y, 0, 0, ModContent.ProjectileType<CollapsedExplodeAim>(), 0, 0, 0, 0);
				}
				else {
					Projectile.NewProjectile(newSource, Projectile.Center.X, Projectile.Center.Y, 0, 0, ModContent.ProjectileType<CollapsedExplodeAim>(), 0, 0, 0, 0);
				}
				skilltimer = 0;
			}

		}
	}

	public class CollapsedExplodeAim : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 8;
		}
		public override void SetDefaults() {
			Projectile.width = 110;
			Projectile.height = 110;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 85;
			Projectile.alpha = 10;
			Projectile.damage = 0;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.scale = 1f;
		}

		private float randomx;
		private bool missled = false;
		private float timer;
		private int randt = 1;
		private int frametimer;
		private Vector2 randvelocity;
		private float randspeed;
		private float randrotation;
		private bool summonedframes = false;
		private float[,] VelocityABCDxy = new float[4, 2] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };//存储四个顶点的发射速度
		private float[] MoveLengthABCD = new float[4] { 0, 0, 0, 0 };//存储四个弹幕的位移
		private float[,] RectangleABCDxy = new float[4, 2] { { 0, 0 }, { 0, 0 }, { 0, 0 }, { 0, 0 } };//存储四个顶点的位置坐标
		private static Vector2 fixedBCDxy = new Vector2(192, 192);
		private float r;
		private float g;
		private float b;
		private float a;

		private static float CalcPos(float velocity, float time) {
			return velocity * time * (1 - time / 120);
		}

		public override void AI() {
			var newSource = Projectile.GetSource_FromThis();
			Player player = Main.player[Main.myPlayer];
			frametimer++;
			if (frametimer > 3) {
				randt = Main.rand.Next(2, 31);
				frametimer = 0;
			}
			Projectile.frame += (int)Main.time % randt == 0 ? Main.rand.Next(1, 9) : 0;
			Projectile.frame %= 8;

			timer++;

			if (timer >= 0 && timer <= 10) {
				Projectile.scale = (float)Math.Sin(Math.PI * timer / 20f);
			}
			else if (timer > 10 && timer <= 60) {
				Projectile.scale = 1f;
			}
			else if (timer > 60 && timer <= 75) {
				Projectile.scale = (float)Math.Cos(Math.PI * timer / 30f);
			}

			if (timer >= 0 && timer <= 10) {
				Projectile.alpha = (int)(120 * Math.Cos(Math.PI * timer / 10f) + 120);
			}
			else if (timer > 10 && timer <= 30) {
				Projectile.alpha = 0;
			}
			else if (timer > 30 && timer <= 60) {
				Projectile.alpha = (int)(-120 * Math.Cos(Math.PI * timer / 5f) + 120);
			}
			else if (timer > 60 && timer <= 75) {
				Projectile.alpha = (int)(-120 * Math.Cos(Math.PI * timer / 15f) + 120);
			}

			if (timer == 10) {// >= 10
							  //if(summonedframes == false) {
				for (int i = 0; i < 4; i++) {
					if (Main.masterMode) {
						randspeed = Main.rand.NextFloat(9.6f, 10.8f);
					}
					else if (Main.expertMode) {
						randspeed = Main.rand.NextFloat(7.5f, 10f);
					}
					else {
						randspeed = Main.rand.NextFloat(6f, 8f);
					}
					randrotation = i * MathHelper.PiOver2 + Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4);
					randvelocity = new Vector2(randspeed, 0).RotatedBy(randrotation);
					VelocityABCDxy[i, 0] = randspeed;//存储每个顶点的发射速度向量模长
					VelocityABCDxy[i, 1] = randrotation;//存储每个顶点的发射速度角度
					Projectile.NewProjectile(newSource, Projectile.Center, randvelocity, ModContent.ProjectileType<CollapsedFrame>(), 0, 0f, -1, i, randvelocity.X, randvelocity.Y);
				}
				//summonedframes = true;
				//}
			}

			if (timer > 10) {
				for (int i = 0; i < 4; i++) {
					MoveLengthABCD[i] = CalcPos(VelocityABCDxy[i, 0], timer - 10);//每个弹幕的运动长度（不是顶点）
					RectangleABCDxy[i, 0] = Projectile.Center.X + new Vector2(MoveLengthABCD[i], 0).RotatedBy(VelocityABCDxy[i, 1]).X + new Vector2(-18, 0).RotatedBy(VelocityABCDxy[i, 1]).X;//每个顶点的X坐标
					RectangleABCDxy[i, 1] = Projectile.Center.Y + new Vector2(MoveLengthABCD[i], 0).RotatedBy(VelocityABCDxy[i, 1]).Y + new Vector2(-18, 0).RotatedBy(VelocityABCDxy[i, 1]).Y;//每个顶点的Y坐标
				}
				//颜色修改
				if (timer <= 30) {
					r = 0;
					g = (30 - timer) / 20;
					b = 1;
					a = (timer - 10) / 40;
				}
				else if (timer <= 50) {
					r = 0.75f * (timer - 30) / 20;
					g = 0;
					b = 1;
					a = 0.2f * (float)Math.Cos(Math.PI * timer / 5f) + 0.3f;
				}
				else if (timer <= 70) {
					r = 0.75f + 0.25f * (timer - 50) / 20;
					g = (timer - 50) / 20;
					b = 1;
					a = 0.5f;
				}
				else {
					a = 0.5f * (85 - timer) / 15;
				}
				//喜闻乐见的傻der时刻
				if (Main.netMode != NetmodeID.Server && !Terraria.Graphics.Effects.Filters.Scene["CollapsedExplosionPart1"].IsActive()) {
					ArknightsMod.CollapsedExplosionPart1.Parameters["uImageSize1"].SetValue(new Vector2(RectangleABCDxy[1, 0], RectangleABCDxy[1, 1]) - fixedBCDxy);
					ArknightsMod.CollapsedExplosionPart1.Parameters["uImageSize2"].SetValue(new Vector2(RectangleABCDxy[2, 0], RectangleABCDxy[2, 1]) - fixedBCDxy);
					ArknightsMod.CollapsedExplosionPart1.Parameters["uImageSize3"].SetValue(new Vector2(RectangleABCDxy[3, 0], RectangleABCDxy[3, 1]) - fixedBCDxy);
					Terraria.Graphics.Effects.Filters.Scene.Activate("CollapsedExplosionPart1", Projectile.Center).GetShader().UseColor(r, g, b).UseTargetPosition(new Vector2(RectangleABCDxy[0, 0], RectangleABCDxy[0, 1])).UseIntensity(36 * Projectile.scale).UseOpacity(a);
				}
				if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["CollapsedExplosionPart1"].IsActive()) {
					ArknightsMod.CollapsedExplosionPart1.Parameters["uImageSize1"].SetValue(new Vector2(RectangleABCDxy[1, 0], RectangleABCDxy[1, 1]) - fixedBCDxy);
					ArknightsMod.CollapsedExplosionPart1.Parameters["uImageSize2"].SetValue(new Vector2(RectangleABCDxy[2, 0], RectangleABCDxy[2, 1]) - fixedBCDxy);
					ArknightsMod.CollapsedExplosionPart1.Parameters["uImageSize3"].SetValue(new Vector2(RectangleABCDxy[3, 0], RectangleABCDxy[3, 1]) - fixedBCDxy);
					Terraria.Graphics.Effects.Filters.Scene.Activate("CollapsedExplosionPart1", Projectile.Center).GetShader().UseColor(r, g, b).UseTargetPosition(new Vector2(RectangleABCDxy[0, 0], RectangleABCDxy[0, 1])).UseIntensity(36 * Projectile.scale).UseOpacity(a);
				}
				if (Main.netMode != NetmodeID.Server && !Terraria.Graphics.Effects.Filters.Scene["CollapsedExplosionPart2"].IsActive()) {
					ArknightsMod.CollapsedExplosionPart2.Parameters["uImageSize1"].SetValue(new Vector2(RectangleABCDxy[1, 0], RectangleABCDxy[1, 1]) - fixedBCDxy);
					ArknightsMod.CollapsedExplosionPart2.Parameters["uImageSize2"].SetValue(new Vector2(RectangleABCDxy[2, 0], RectangleABCDxy[2, 1]) - fixedBCDxy);
					ArknightsMod.CollapsedExplosionPart2.Parameters["uImageSize3"].SetValue(new Vector2(RectangleABCDxy[3, 0], RectangleABCDxy[3, 1]) - fixedBCDxy);
					Terraria.Graphics.Effects.Filters.Scene.Activate("CollapsedExplosionPart2", Projectile.Center).GetShader().UseColor(r, g, b).UseTargetPosition(new Vector2(RectangleABCDxy[0, 0], RectangleABCDxy[0, 1])).UseIntensity(36 * Projectile.scale).UseOpacity(a);
				}
				if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["CollapsedExplosionPart2"].IsActive()) {
					ArknightsMod.CollapsedExplosionPart2.Parameters["uImageSize1"].SetValue(new Vector2(RectangleABCDxy[1, 0], RectangleABCDxy[1, 1]) - fixedBCDxy);
					ArknightsMod.CollapsedExplosionPart2.Parameters["uImageSize2"].SetValue(new Vector2(RectangleABCDxy[2, 0], RectangleABCDxy[2, 1]) - fixedBCDxy);
					ArknightsMod.CollapsedExplosionPart2.Parameters["uImageSize3"].SetValue(new Vector2(RectangleABCDxy[3, 0], RectangleABCDxy[3, 1]) - fixedBCDxy);
					Terraria.Graphics.Effects.Filters.Scene.Activate("CollapsedExplosionPart2", Projectile.Center).GetShader().UseColor(r, g, b).UseTargetPosition(new Vector2(RectangleABCDxy[0, 0], RectangleABCDxy[0, 1])).UseIntensity(36 * Projectile.scale).UseOpacity(a);
				}
			}

			if (timer == 60) {
				Projectile.NewProjectile(newSource, Projectile.Center.X, Projectile.Center.Y, 0, 0, ModContent.ProjectileType<ExplodeWave>(), 10, 0f, 0, 0);
			}

			if (timer == 70) {
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Assets/Sound/ImperialArtilleyCoreTargeteer/IACTStageChangeTremor") with { Volume = 2f, Pitch = 0f }, player.Center);
				//player.Hurt(PlayerDeathReason.ByCustomReason(player.name + "坍缩了。"), Main.rand.Next(60, 120),0);
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC SeekForNPCs = Main.npc[i];
					if (SeekForNPCs.active && SeekForNPCs.type == ModContent.NPCType<AACT>()) {
						if (player.Center.X < RectangleABCDxy[0, 0] && player.Center.X > RectangleABCDxy[2, 0] && player.Center.Y < RectangleABCDxy[1, 1] && player.Center.Y > RectangleABCDxy[3, 1]) {
							player.Heal((int)-SeekForNPCs.ai[2]);
							SeekForNPCs.ai[2] = Math.Max(SeekForNPCs.ai[2] * Main.rand.NextFloat(2f, 4f), 2f);
							if (player.statLife <= 0) {
								player.KillMe(PlayerDeathReason.ByCustomReason(player.name + Language.GetTextValue("Mods.ArknightsMod.StatusMessage.AACT.Collapsed")), 99999, 0);
							}
						}
						else {
							SeekForNPCs.ai[2] = Math.Max(SeekForNPCs.ai[2] * Main.rand.NextFloat(0.5f, 1f), 2f);
						}
					}
				}
			}
		}

		public override void OnKill(int timeLeft) {
			if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["CollapsedExplosionPart1"].IsActive()) {
				Terraria.Graphics.Effects.Filters.Scene["CollapsedExplosionPart1"].Deactivate();
			}
			if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["CollapsedExplosionPart2"].IsActive()) {
				Terraria.Graphics.Effects.Filters.Scene["CollapsedExplosionPart2"].Deactivate();
			}
		}
	}

	public class CollapsedFrame : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 8;
		}
		public override void SetDefaults() {
			Projectile.width = 64;
			Projectile.height = 72;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 75;
			Projectile.alpha = 255;
			Projectile.damage = 0;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = false;
		}

		private int timer;
		private int randt = 1;
		private float timer2;
		//private float randstop;

		public override void PostDraw(Color lightColor) {
			Texture2D Colline = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/CollapsedLine").Value;
			Main.EntitySpriteDraw(Colline, Projectile.Center - Main.screenPosition + new Vector2(0, 32 * Projectile.scale).RotatedBy(Projectile.rotation), new Rectangle(0, 0, Colline.Width, Colline.Height), Color.White, (Projectile.ai[0] - 1) * MathHelper.PiOver2, new Vector2(Colline.Width / 2, Colline.Height / 2), Projectile.scale, SpriteEffects.None, 0);
		}

		public override void AI() {
			//for (int i = 0; i < 1; i++) {
			//	if (Main.masterMode) {
			//		randstop = Main.rand.NextFloat(0.0375f, 0.05f);
			//	}
			//	else if (Main.expertMode) {
			//		randstop = Main.rand.NextFloat(0.05f, 0.075f);
			//	}
			//	else {
			//		randstop = Main.rand.NextFloat(0.0625f, 0.0875f);
			//	}
			//}
			timer++;
			timer2++;
			if (timer > 3) {
				randt = Main.rand.Next(2, 31);
				timer = 0;
			}

			if (timer2 <= 15) {
				Projectile.alpha = (int)(255 - timer2 * 17);
				Projectile.scale = timer2 / 15;
			}
			else if (timer2 <= 60) {
				Projectile.alpha = 0;
				Projectile.scale = 1f;
			}
			else {
				Projectile.alpha = (int)(timer2 - 60) * 17;
				Projectile.scale = 5 - timer2 / 15;
			}

			Projectile.frame += (int)Main.time % randt == 0 ? Main.rand.Next(1, 9) : 0;
			Projectile.frame %= 8;
			if (Projectile.timeLeft >= 15) {
				Projectile.velocity = new Vector2(Projectile.ai[1], Projectile.ai[2]) * (Projectile.timeLeft - 14) / 60;//一秒内减速到0
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}
			else {
				Projectile.velocity = new Vector2(Projectile.ai[1], Projectile.ai[2]) * 0.01f;
			}
			//Projectile.velocity = Vector2.Lerp(Projectile.velocity,Vector2.Zero,randstop);
		}
	}

	public class AACTStage3TrueHealth : ModNPC
	{
		public override string Texture => ArknightsMod.noTexture;
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			NPC.width = 1;
			NPC.height = 1;
			NPC.aiStyle = 0;
			NPC.defense = 2147483647;
			NPC.alpha = 0;
			NPC.damage = 0;
			NPC.friendly = false;
			NPC.lifeMax = AACTstg3TH;
			NPC.HitSound = SoundID.NPCHit4;//金属声
			NPC.DeathSound = SoundID.NPCDeath14;//爆炸声
			NPC.knockBackResist = 0f;//击退抗性，0f为最高，1f为最低
			NPC.noGravity = true;
		}

		public int AACTstg3TH {
			get {
				if (!ishealthget) {
					ishealthget = true;
					lasthealth = Main.masterMode ? Main.rand.Next(16, 21) : (Main.expertMode ? Main.rand.Next(12, 17) : Main.rand.Next(8, 13));
					usehealth = lasthealth;
				}
				return lasthealth;
			}
		}

		public int AACTDeathTH {
			get {
				return usehealth;
			}
		}

		private bool ishealthget = false;
		private int lasthealth = 100;
		private int usehealth = 100;
		private int cooldown = 600;
		private bool iscooldownsoundplayed = false;
		private int timer;
		private int randt = 1;
		private int deathtimer;

		//隐藏下方血条
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			return false;
		}
		//无敌帧
		public override bool? CanBeHitByItem(Player player, Item item) {
			return cooldown <= 0 ? null : false;
		}
		//无敌帧
		public override bool? CanBeHitByProjectile(Projectile Projectile) {
			if (Projectile.hostile == true) {
				return false;
			}
			else if (Projectile.friendly == true) {
				return cooldown <= 0 ? null : false;
			}
			else {
				return false;
			}
		}
		//血量修改
		public override void ModifyHitByItem(Player player, Item item, ref NPC.HitModifiers modifiers) {
			Player Player = Main.player[Main.myPlayer];
			SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Assets/Sound/ImperialArtilleyCoreTargeteer/Shield") with { Volume = 1f, Pitch = 0f }, Player.Center);
			iscooldownsoundplayed = true;
			cooldown = Main.rand.Next(60, 121);
			usehealth -= 1;
			NPC.life = usehealth;
		}
		//血量修改
		public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers) {
			Player Player = Main.player[Main.myPlayer];
			SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Assets/Sound/ImperialArtilleyCoreTargeteer/AACTSTG3HIT") with { Volume = 1f, Pitch = 0f }, Player.Center);
			iscooldownsoundplayed = true;
			cooldown = Main.rand.Next(60, 121);
			usehealth -= 1;
			NPC.life = usehealth;
		}
		public int Host {
			get => (int)NPC.ai[0] - 1;
			set => NPC.ai[0] = value + 1;
		}

		public bool IsAACTActive => Host > -1;

		public static int HostType() {
			return ModContent.NPCType<AACT>();
		}

		public override void AI() {
			Player Player = Main.player[Main.myPlayer];
			//var AACT = ModContent.GetModNPC(ModContent.NPCType<AACT>());
			NPC HostNPC = Main.npc[Host];
			NPC.Center = HostNPC.Center;
			if (cooldown <= 0) {
				NPC.dontTakeDamage = false;
				if (iscooldownsoundplayed == true) {
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/AACTTeleport") with { Volume = 1f, Pitch = 0f }, Player.Center);
					iscooldownsoundplayed = false;
				}
			}
			else {
				cooldown--;
				NPC.dontTakeDamage = true;
			}

			timer++;
			if (timer > 3) {
				randt = Main.rand.Next(2, 31);
				timer = 0;
			}

			if (NPC.life <= 1) {
				deathtimer++;
				NPC.checkDead();
			}
		}

		public override bool CheckDead()//锁血及锁血解除
		{
			if (deathtimer > 15) {
				return true;
			}
			else {
				NPC.life = 1;
				return false;
			}
		}
	}

	public class IACTStage3target : ModNPC
	{
		public override string Texture => ArknightsMod.noTexture;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 1;//贴图帧数
		}

		public override void SetDefaults() {
			NPC.lifeMax = 1;
			NPC.damage = 0;
			NPC.defense = 0;
			NPC.knockBackResist = 0f;//击退抗性，0f为最高，1f为最低
			NPC.width = 1;
			NPC.height = 1;
			NPC.noGravity = true;//无引力
			NPC.noTileCollide = true;//不与物块相撞
			NPC.lavaImmune = true;//免疫岩浆
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit4;//金属声
			NPC.DeathSound = SoundID.NPCDeath14;//爆炸声
		}

		public int ParentIndex {
			get => (int)NPC.ai[0] - 1;
			set => NPC.ai[0] = value + 1;
		}

		public static int BodyType() {
			return ModContent.NPCType<AACT>();
		}

		private int timer;
		private int stage3atkloop;

		public override void AI() {
			var newSource = NPC.GetSource_FromThis();

			if (Main.masterMode) {
				stage3atkloop = 450;
			}
			else if (Main.expertMode) {
				stage3atkloop = 540;
			}
			else {
				stage3atkloop = 630;
			}
			if (timer % stage3atkloop == 0) {
				Projectile.NewProjectile(newSource, NPC.Center.X, NPC.Center.Y, 0, 0, ModContent.ProjectileType<ISTG3InCore>(), 0, 0f, 0, NPC.whoAmI);
				Projectile.NewProjectile(newSource, NPC.Center.X, NPC.Center.Y, 0, 0, ModContent.ProjectileType<ISTG3InRing>(), 0, 0f, 0, NPC.whoAmI);
				Projectile.NewProjectile(newSource, NPC.Center.X, NPC.Center.Y, 0, 0, ModContent.ProjectileType<ISTG3OutPrism>(), 0, 0f, 0, NPC.whoAmI);
				Projectile.NewProjectile(newSource, NPC.Center.X, NPC.Center.Y, 0, 0, ModContent.ProjectileType<ISTG3OutRing>(), 0, 0f, 0, NPC.whoAmI);
			}
			timer++;
			Player Player = Main.player[Main.myPlayer];
			NPC parentNPC = Main.npc[ParentIndex];
			NPC.dontTakeDamage = true;
			NPC.Center = 2 * Player.Center - parentNPC.Center;
			if (parentNPC.life == 1 || parentNPC.active == false) {
				die();
			}
		}

		private int killtimer;

		private void die() {
			killtimer++;
			NPC.alpha = 4 * killtimer;
			if (NPC.alpha > 255) {
				NPC.alpha = 255;
			}
			NPC.scale = 1 - killtimer / 60;
			if (NPC.scale < 0) {
				NPC.scale = 0;
			}
			if (killtimer >= 120) {
				NPC.dontTakeDamage = false;
				NPC.life = 0;
			}
		}
	}

	public class ISTG3InCore : ModProjectile
	{
		private const string ChainTextPath = "ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/ISTG3InCore";

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			if (Main.masterMode) {
				Projectile.timeLeft = 360;
			}
			else if (Main.expertMode) {
				Projectile.timeLeft = 450;
			}
			else {
				Projectile.timeLeft = 540;
			}
			Projectile.alpha = 10;
			Projectile.damage = 0;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.scale = 1f;
		}

		private int timer;

		public int ParentIndex {
			get => (int)Projectile.ai[0] - 1;
			set => Projectile.ai[0] = value + 1;
		}

		public override void AI() {
			timer++;
			Player Player = Main.player[Main.myPlayer];
			NPC parentNPC = Main.npc[ParentIndex];
			Projectile.Center = 2 * Player.Center - parentNPC.Center;
			if (Main.masterMode) {
				Projectile.alpha = (int)(Math.Pow(timer, 2) / 60 - timer * 6 + 255);
				if ((timer >= 0 && timer <= 60) || (timer >= 300 && timer <= 360)) {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120);
				}
				else {
					Projectile.scale = 0.1f * (float)Math.Sin(timer * Math.PI / 60 - Math.PI / 2) + 0.9f;
				}
			}
			else if (Main.expertMode) {
				Projectile.alpha = (int)(Math.Pow(timer, 2) / 75 - timer * 6 + 255);
				if (timer >= 0 && timer <= 60) {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120);
				}
				else if (timer >= 60 && timer <= 390) {
					Projectile.scale = 0.1f * (float)Math.Sin(timer * Math.PI / 70 - 5 * Math.PI / 14) + 0.9f;
				}
				else {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120 + Math.PI / 2);
				}
			}
			else {
				Projectile.alpha = (int)(Math.Pow(timer, 2) / 90 - timer * 6 + 255);
				if (timer >= 0 && timer <= 60) {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120);
				}
				else if (timer >= 60 && timer <= 480) {
					Projectile.scale = 0.1f * (float)Math.Sin(timer * Math.PI / 70 - 5 * Math.PI / 14) + 0.9f;
				}
				else {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120 - 3 * Math.PI / 4);
				}
			}
			if (Projectile.alpha < 0) {
				Projectile.alpha = 0;
			}
		}
	}

	public class ISTG3InRing : ModProjectile
	{
		private const string ChainTextPath = "ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/ISTG3InRing";

		public override void SetDefaults() {
			Projectile.width = 52;
			Projectile.height = 52;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			if (Main.masterMode) {
				Projectile.timeLeft = 360;
			}
			else if (Main.expertMode) {
				Projectile.timeLeft = 450;
			}
			else {
				Projectile.timeLeft = 540;
			}
			Projectile.alpha = 10;
			Projectile.damage = 0;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.scale = 1f;
		}

		private int timer;

		public int ParentIndex {
			get => (int)Projectile.ai[0] - 1;
			set => Projectile.ai[0] = value + 1;
		}

		public override void AI() {
			timer++;
			Player Player = Main.player[Main.myPlayer];
			NPC parentNPC = Main.npc[ParentIndex];
			Projectile.Center = 2 * Player.Center - parentNPC.Center;
			Projectile.rotation = -0.05f * timer;
			if (Main.masterMode) {
				Projectile.alpha = (int)(Math.Pow(timer, 2) / 60 - timer * 6 + 255);
				if ((timer >= 0 && timer <= 60) || (timer >= 300 && timer <= 360)) {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120);
				}
				else {
					Projectile.scale = 0.1f * (float)Math.Sin(timer * Math.PI / 60 - Math.PI / 2) + 0.9f;
				}
			}
			else if (Main.expertMode) {
				Projectile.alpha = (int)(Math.Pow(timer, 2) / 75 - timer * 6 + 255);
				if (timer >= 0 && timer <= 60) {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120);
				}
				else if (timer >= 60 && timer <= 390) {
					Projectile.scale = 0.1f * (float)Math.Sin(timer * Math.PI / 70 - 5 * Math.PI / 14) + 1f;
				}
				else {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120 + Math.PI / 2);
				}
			}
			else {
				Projectile.alpha = (int)(Math.Pow(timer, 2) / 90 - timer * 6 + 255);
				if (timer >= 0 && timer <= 60) {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120);
				}
				else if (timer >= 60 && timer <= 480) {
					Projectile.scale = 0.1f * (float)Math.Sin(timer * Math.PI / 70 - 5 * Math.PI / 14) + 0.9f;
				}
				else {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120 - 3 * Math.PI / 4);
				}
			}
			if (Projectile.alpha < 0) {
				Projectile.alpha = 0;
			}
		}
	}

	public class ISTG3OutRing : ModProjectile
	{
		private const string ChainTextPath = "ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/ISTG3OutRing";

		public override void SetDefaults() {
			Projectile.width = 166;
			Projectile.height = 166;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			if (Main.masterMode) {
				Projectile.timeLeft = 360;
			}
			else if (Main.expertMode) {
				Projectile.timeLeft = 450;
			}
			else {
				Projectile.timeLeft = 540;
			}
			Projectile.alpha = 10;
			Projectile.damage = 0;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.scale = 1f;
		}

		private int timer;

		public int ParentIndex {
			get => (int)Projectile.ai[0] - 1;
			set => Projectile.ai[0] = value + 1;
		}

		public override void AI() {
			timer++;
			Player Player = Main.player[Main.myPlayer];
			NPC parentNPC = Main.npc[ParentIndex];

			Projectile.Center = 2 * Player.Center - parentNPC.Center;
			Projectile.rotation = 0.05f * timer;
			if (Main.masterMode) {
				Projectile.alpha = (int)(Math.Pow(timer, 2) / 60 - timer * 6 + 255);
				if ((timer >= 0 && timer <= 60) || (timer >= 300 && timer <= 360)) {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120);
				}
				else {
					Projectile.scale = 1f;
				}
			}
			else if (Main.expertMode) {
				Projectile.alpha = (int)(Math.Pow(timer, 2) / 75 - timer * 6 + 255);
				if (timer >= 0 && timer <= 60) {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120);
				}
				else if (timer >= 60 && timer <= 390) {
					Projectile.scale = 1f;
				}
				else {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120 + Math.PI / 2);
				}
			}
			else {
				Projectile.alpha = (int)(Math.Pow(timer, 2) / 90 - timer * 6 + 255);
				if (timer >= 0 && timer <= 60) {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120);
				}
				else if (timer >= 60 && timer <= 480) {
					Projectile.scale = 1f;
				}
				else {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120 - 3 * Math.PI / 4);
				}
			}
			if (Projectile.alpha < 0) {
				Projectile.alpha = 0;
			}
		}
	}

	public class ISTG3OutPrism : ModProjectile
	{
		private const string ChainTextPath = "ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/ISTG3OutPrism";

		public override void SetDefaults() {
			Projectile.width = 132;
			Projectile.height = 132;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			if (Main.masterMode) {
				Projectile.timeLeft = 360;
			}
			else if (Main.expertMode) {
				Projectile.timeLeft = 450;
			}
			else {
				Projectile.timeLeft = 540;
			}
			Projectile.alpha = 10;
			Projectile.damage = 0;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.scale = 1f;
		}

		private int timer;

		public int ParentIndex {
			get => (int)Projectile.ai[0] - 1;
			set => Projectile.ai[0] = value + 1;
		}

		public override void AI() {
			timer++;
			Player Player = Main.player[Main.myPlayer];
			NPC parentNPC = Main.npc[ParentIndex];
			Projectile.Center = 2 * Player.Center - parentNPC.Center;
			Projectile.rotation = 0.05f * timer;
			if (Main.masterMode) {
				Projectile.alpha = (int)(Math.Pow(timer, 2) / 60 - timer * 6 + 255);
				if ((timer >= 0 && timer <= 60) || (timer >= 300 && timer <= 360)) {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120) + 0.2f;
				}
				else {
					Projectile.scale = 0.1f * (float)Math.Sin(timer * Math.PI / 60 - Math.PI / 2) + 1.1f;
				}
			}
			else if (Main.expertMode) {
				Projectile.alpha = (int)(Math.Pow(timer, 2) / 75 - timer * 6 + 255);
				if (timer >= 0 && timer <= 60) {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120) + 0.2f;
				}
				else if (timer >= 60 && timer <= 390) {
					Projectile.scale = 0.1f * (float)Math.Sin(timer * Math.PI / 70 - 5 * Math.PI / 14) + 1.1f;
				}
				else {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120 + Math.PI / 2) + 0.2f;
				}
			}
			else {
				Projectile.alpha = (int)(Math.Pow(timer, 2) / 90 - timer * 6 + 255);
				if (timer >= 0 && timer <= 60) {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120) + 0.2f;
				}
				else if (timer >= 60 && timer <= 480) {
					Projectile.scale = 0.1f * (float)Math.Sin(timer * Math.PI / 70 - 5 * Math.PI / 14) + 1.1f;
				}
				else {
					Projectile.scale = (float)Math.Sin(timer * Math.PI / 120 - 3 * Math.PI / 4) + 0.2f;
				}
			}
			if (Projectile.alpha < 0) {
				Projectile.alpha = 0;
			}
		}
	}

	public class AACTEndShow : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;
		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 600;
		}

		public override void SetDefaults() {
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 3200;
			Projectile.alpha = 0;
			Projectile.damage = 0;
			Projectile.light = 0f;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.scale = 1f;
		}

		public int ParentIndex {
			get => (int)Projectile.ai[0] - 1;
			set => Projectile.ai[0] = value + 1;
		}

		public static int HostType() {
			return ModContent.NPCType<AACT>();
		}

		private float timer;
		private int colortimer;
		private int r;
		private int g;

		public override void AI() {
			timer++;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC SeekForNPCs = Main.npc[i];
				if (SeekForNPCs.active && SeekForNPCs.type == ModContent.NPCType<AACT>()) {
					Projectile.Center = SeekForNPCs.Center;
					colortimer += Math.Min(Math.Max(1, (int)(16 * timer / Projectile.timeLeft)), Projectile.timeLeft / 10);
				}
			}

			if (colortimer < 240) {
				r = 0;
				g = 240 - colortimer;
			}
			else if (colortimer < 440) {
				r = colortimer - 240;
				g = 0;
			}
			else if (colortimer < 640) {
				r = 640 - colortimer;
				g = 0;
			}
			else if (colortimer < 880) {
				r = 0;
				g = colortimer - 640;
			}
			else {
				colortimer = 0;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, Vector2.Zero, new Color(r, g, 240), new Color(0, 0, 0), 25f * Math.Min(timer / 600, 1), true);
			return true;
		}
	}
}