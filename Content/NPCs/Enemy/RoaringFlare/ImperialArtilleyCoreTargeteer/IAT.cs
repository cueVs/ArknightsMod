using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Localization;    
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Audio;
using System;
using ArknightsMod.Common.Players;
using Terraria.GameContent.Bestiary;
using ArknightsMod.Content.Items.Material;
using Terraria.GameContent.ItemDropRules;
using log4net.Util;

namespace ArknightsMod.Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer
{
	public class IAT : ModNPC
	{
        public override void SetStaticDefaults()
		{
			Main.npcFrameCount[NPC.type] = 1;//贴图帧数
		}

		public override void SetDefaults()
		{
			NPC.lifeMax = 8000;
			NPC.damage = 40;
			NPC.defense = 80;
			NPC.knockBackResist = 0f;//击退抗性，0f为最高，1f为最低
			NPC.width = 164;
			NPC.height = 70;
			NPC.noGravity = true;//无引力
			NPC.noTileCollide = true;//不与物块相撞
            NPC.lavaImmune = true;//免疫岩浆
			NPC.aiStyle = -1;
			NPC.HitSound = SoundID.NPCHit4;//金属声
			NPC.DeathSound = SoundID.NPCDeath14;//爆炸声
			NPCID.Sets.ImmuneToAllBuffs[Type] = true;//免疫所有debuff
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) => spawnInfo.Player.ZoneSnow && spawnInfo.Player.ZoneOverworldHeight && Main.hardMode && Main.raining && !Main.dayTime && !NPC.AnyNPCs(ModContent.NPCType<IAT>()) && !NPC.AnyNPCs(ModContent.NPCType<IACT>()) ? 0.05f : 0f;

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Events.Rain,
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Visuals.Blizzard,

				new FlavorTextBestiaryInfoElement(Language.GetTextValue("Mods.ArknightsMod.Bestiary.IAT")),
			});
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<IncandescentAlloy>(), 3, 3, 5));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<CrystallineComponent>(), 3, 3, 5));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<IntegratedDevice>(), 3, 3, 5));
		}

		//NPC专家模式|大师模式血量倍率（普通模式血量*倍率*2|血量*倍率*3）
		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
		{
			NPC.lifeMax = (int)(NPC.lifeMax * 0.75f * balance);//8000|12000|18000
			NPC.damage = (int)(NPC.damage * 0.8f);//40|64|96
		}

		private float expertHealthFrac = Main.expertMode ? 0.9f : 0.75f;//进入第二阶段生命值百分比（12000/16000；21600/24000）
		
		//视觉效果部分
		#region 
		private int OAOScaleY;
		private int IAORScaleX;
		private int LightScale2;
		private float OAOScale2Enter;
		private float OAOScale2;
		private float OAOScale2Zero;
		private float IACTLightScale2Enter;
		private float IACTLightScale1;
		private float IACTLightScale2;
		private float IACTLightScale1Zero;
		private float IACTLightScale2Zero;
		private float IAFRScale0Enter;
		private float IAFRScale2Enter;
		private float IAFRScale10Enter;
		private float IAFRScale0;
		private float IAFRScale2;
		private float IAFRScale4;
		private float IAFRScale6;
		private float IAFRScale8;
		private float IAFRScale10;
		private float IAFRScale0Zero;
		private float IAFRScale2Zero;
		private float IAFRScale4Zero;
		private float IAFRScale6Zero;
		private float IAFRScale8Zero;
		private float IAFRScale10Zero;
		private int stage2to3timer1;
		private int stage2to3timer2;
		private int stage2to3timer3;
		private int stage2to3timer4;
		private int bartimer;
		private float barscale;
		private int crashtimer;
		private float bardistance;
		private float stage1healthscale;
		private float atkscale;

		//NPC贴图黑暗高亮发光效果+光环效果

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)//贴图，位置，大小区域，光亮颜色，转动角度，中心点坐标，缩放倍率，特殊效果(翻转)，图层
		{
			//鉴于layerdepth是个废物，越靠下出现的实际上越在上层
			//光环部分，参数见ai
			#region
			if (Main.gamePaused != true) {
				OAOScaleY++;
				IAORScaleX++;
				LightScale2++;
			}
			if (OAOScaleY >= 120) {
				OAOScaleY = 0;
			}//周期2秒
			if (IAORScaleX >= 240) {
				IAORScaleX = 0;
			}//周期4秒
			if (LightScale2 >= 180) {
				LightScale2 = 0;
			}//周期3秒

			Texture2D OutringTexture2 = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/OAF").Value;
			Texture2D InringTexture2 = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/IAFR").Value;
			Texture2D waveTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/IACTLightwave").Value;
			if (IATcrashed != true) {
				if (npctimer <= 20)//刚进入二阶段的第1/3秒，内圈的2、8应用于此处
				{
					IAFRScale2Enter = (float)(0.95 * Math.Sin(Math.PI * IAORScaleX / 40f));//引入过程，实现平滑过渡
					Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale2Enter, SpriteEffects.None, 0);
				}
				else//确实进入了二阶段（1/3秒后）
				{
					IAFRScale2 = (float)(0.475f * Math.Sin(Math.PI * IAORScaleX / 120f + Math.PI / 3f) + 0.475f);//多内环缩放,周期4秒，在0~0.95倍之间变化
					IAFRScale8 = (float)(0.475f * Math.Sin(Math.PI * IAORScaleX / 120f + 4 * Math.PI / 3f) + 0.475f);//多内环缩放,周期4秒，在0~0.95倍之间变化
					Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale2, SpriteEffects.None, 0);
					Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale8, SpriteEffects.None, 0);
				}

				if (npctimer <= 60)//刚进入二阶段的第一秒，光环从0增大,内圈的0、6、10应用于此处
				{
					OAOScale2Enter = (float)(Math.Sin(Math.PI * OAOScaleY / 120f));//引入过程，实现平滑过渡
					IAFRScale0Enter = (float)(0.95 * Math.Sin(Math.PI * IAORScaleX / 120f));
					IAFRScale10Enter = (float)(IAORScaleX / 84.21f);
					Main.EntitySpriteDraw(OutringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, OutringTexture2.Width, OutringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(OutringTexture2.Width / 2, OutringTexture2.Height / 2), OAOScale2Enter, SpriteEffects.None, 0);
					Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale0Enter, SpriteEffects.None, 0);
					Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale10Enter, SpriteEffects.None, 0);
				}
				else//确实进入了二阶段（1秒后）
				{
					OAOScale2 = (float)(0.0625f * Math.Sin(Math.PI * OAOScaleY / 60f - Math.PI / 2f) + 0.9375f);//外环缩放,周期2秒，在1~0.875倍之间变化
					IAFRScale0 = (float)(0.475f * Math.Sin(Math.PI * IAORScaleX / 120f) + 0.475f);//多内环缩放,周期4秒，在0~0.95倍之间变化
					IAFRScale6 = (float)(0.475f * Math.Sin(Math.PI * IAORScaleX / 120f + Math.PI) + 0.475f);//多内环缩放,周期4秒，在0~0.95倍之间变化
					IAFRScale10 = (float)(0.475f * Math.Sin(Math.PI * IAORScaleX / 120f + 5 * Math.PI / 3f) + 0.475f);//多内环缩放,周期4秒，在0~0.95倍之间变化
					Main.EntitySpriteDraw(OutringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, OutringTexture2.Width, OutringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(OutringTexture2.Width / 2, OutringTexture2.Height / 2), OAOScale2, SpriteEffects.None, 0);
					Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale0, SpriteEffects.None, 0);
					Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale6, SpriteEffects.None, 0);
					Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale10, SpriteEffects.None, 0);
				}

				IACTLightScale1 = (float)(0.5f * Math.Sin(Math.PI * LightScale2 / 90f - Math.PI / 2f) + 0.5f);//灯光晕影大小变化,相位0（180）
				if (npctimer <= 90)//刚进入二阶段的第1.5秒，探照灯从0增大
				{
					IACTLightScale2Enter = 0;//引入过程，实现平滑过渡
					Main.EntitySpriteDraw(waveTexture, NPC.Center - Main.screenPosition + new Vector2(0, 25), new Rectangle(0, 0, waveTexture.Width, waveTexture.Height), Color.Red, NPC.rotation, new Vector2(waveTexture.Width / 2, 0), IACTLightScale1, SpriteEffects.None, 0);
					Main.EntitySpriteDraw(waveTexture, NPC.Center - Main.screenPosition + new Vector2(0, 25), new Rectangle(0, 0, waveTexture.Width, waveTexture.Height), Color.Red, NPC.rotation, new Vector2(waveTexture.Width / 2, 0), IACTLightScale2Enter, SpriteEffects.None, 0);
				}
				else//确实进入了二阶段（1.5秒后）
				{
					IACTLightScale2 = (float)(0.5f * Math.Sin(Math.PI * LightScale2 / 90f + Math.PI / 2f) + 0.5f);//灯光晕影大小变化,相位90
					Main.EntitySpriteDraw(waveTexture, NPC.Center - Main.screenPosition + new Vector2(0, 25), new Rectangle(0, 0, waveTexture.Width, waveTexture.Height), Color.Red, NPC.rotation, new Vector2(waveTexture.Width / 2, 0), IACTLightScale1, SpriteEffects.None, 0);
					Main.EntitySpriteDraw(waveTexture, NPC.Center - Main.screenPosition + new Vector2(0, 25), new Rectangle(0, 0, waveTexture.Width, waveTexture.Height), Color.Red, NPC.rotation, new Vector2(waveTexture.Width / 2, 0), IACTLightScale2, SpriteEffects.None, 0);
				}

				if (npctimer >= 100)//确实进入了二阶段（5/3秒后）
				{
					IAFRScale4 = (float)(0.475f * Math.Sin(Math.PI * IAORScaleX / 120f + 2 * Math.PI / 3f) + 0.475f);//多内环缩放,周期4秒，在0~0.95倍之间变化
					Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale4, SpriteEffects.None, 0);
				}

			}
			else {
				if (Main.gamePaused != true) {
					stage2to3timer1++;
					stage2to3timer2++;
					stage2to3timer3++;
					stage2to3timer4++;
				}
				if (stage2to3timer3 >= 60) {
					stage2to3timer1 = 60;
					stage2to3timer2 = 60;
					stage2to3timer3 = 60;
					stage2to3timer4 = 60;
				}

				OAOScale2Zero = OAOScale2 - OAOScale2 * stage2to3timer1 / 60;
				IAFRScale0Zero = IAFRScale0 - IAFRScale0 * stage2to3timer1 / 60;
				IAFRScale6Zero = IAFRScale6 - IAFRScale6 * stage2to3timer1 / 60;
				IAFRScale10Zero = IAFRScale10 - IAFRScale10 * stage2to3timer1 / 60;
				IAFRScale2Zero = IAFRScale2 - IAFRScale2 * stage2to3timer3 / 60;
				IAFRScale8Zero = IAFRScale8 - IAFRScale8 * stage2to3timer3 / 60;
				IACTLightScale1Zero = IACTLightScale1 - IACTLightScale1 * stage2to3timer2 / 60;
				IACTLightScale2Zero = IACTLightScale2 - IACTLightScale2 * stage2to3timer2 / 60;
				IAFRScale4Zero = IAFRScale4 - IAFRScale4 * stage2to3timer4 / 60;

				Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale2Zero, SpriteEffects.None, 0);
				Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale8Zero, SpriteEffects.None, 0);
				Main.EntitySpriteDraw(OutringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, OutringTexture2.Width, OutringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(OutringTexture2.Width / 2, OutringTexture2.Height / 2), OAOScale2Zero, SpriteEffects.None, 0);
				Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale0Zero, SpriteEffects.None, 0);
				Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale6Zero, SpriteEffects.None, 0);
				Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale10Zero, SpriteEffects.None, 0);
				Main.EntitySpriteDraw(waveTexture, NPC.Center - Main.screenPosition + new Vector2(0, 25), new Rectangle(0, 0, waveTexture.Width, waveTexture.Height), Color.Red, NPC.rotation, new Vector2(waveTexture.Width / 2, 0), IACTLightScale1Zero, SpriteEffects.None, 0);
				Main.EntitySpriteDraw(waveTexture, NPC.Center - Main.screenPosition + new Vector2(0, 25), new Rectangle(0, 0, waveTexture.Width, waveTexture.Height), Color.Red, NPC.rotation, new Vector2(waveTexture.Width / 2, 0), IACTLightScale2Zero, SpriteEffects.None, 0);
				Main.EntitySpriteDraw(InringTexture2, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, InringTexture2.Width, InringTexture2.Height), Color.White, (float)(0 * NPC.rotation), new Vector2(InringTexture2.Width / 2, InringTexture2.Height / 2), IAFRScale4Zero, SpriteEffects.None, 0);

			}
			#endregion

			Texture2D healthTexture1 = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/IACTHB2").Value;//血条
			Main.EntitySpriteDraw(healthTexture1, NPC.Center - Main.screenPosition + new Vector2(0, 1 + bardistance), new Rectangle(0, 0, (int)(healthTexture1.Width * stage1healthscale), healthTexture1.Height), Color.White, 0, new Vector2(healthTexture1.Width / 2, healthTexture1.Height / 2), barscale, SpriteEffects.None, 0);

			Texture2D atkTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/IACTTM").Value;//攻击冷却条
			Main.EntitySpriteDraw(atkTexture, NPC.Center - Main.screenPosition + new Vector2(0, 8 + bardistance), new Rectangle(0, 0, (int)(atkTexture.Width * atkscale), atkTexture.Height), Color.White, 0, new Vector2(atkTexture.Width / 2, atkTexture.Height / 2), barscale, SpriteEffects.None, 0);

			Texture2D barTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/IACTBar").Value;//属性条
			Main.EntitySpriteDraw(barTexture, NPC.Center - Main.screenPosition + new Vector2(0, 3 + bardistance), new Rectangle(0, 0, barTexture.Width, barTexture.Height), Color.White, 0, new Vector2(barTexture.Width / 2, barTexture.Height / 2), barscale, SpriteEffects.None, 0);

			if (IATcrashed != true)//灯光，坠毁锁血后高亮消失
			{
				Texture2D lightsTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/RoaringFlare/ImperialArtilleyCoreTargeteer/IACTLights").Value;//闪灯部分
				if (lightsOn == true) {
					Main.EntitySpriteDraw(lightsTexture, NPC.Center - Main.screenPosition + new Vector2(0, 3), new Rectangle(0, 0, lightsTexture.Width, lightsTexture.Height), Color.White, NPC.rotation, new Vector2(lightsTexture.Width / 2, lightsTexture.Height / 2), 1f, SpriteEffects.None, 0);
				}
			}
		}

		private float MISSChance = 0f;
		private int hpdecreasecooldown;

		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position)//隐藏下方血条
		{
			return false;
		}
		public override bool? CanBeHitByItem(Player player, Item item)//无敌帧
		{
			return null;
		}

		public override bool? CanBeHitByProjectile(Projectile Projectile)//不被敌方弹幕和无来源弹幕攻击&闪避
        {
			hpdecreasecooldown++;

            if (Projectile.hostile == true)
			{
				return false;
			}
			else if(Projectile.friendly == true)
			{
				MISSChance = Main.rand.NextFloat(1);
				if (hpdecreasecooldown % 5 == 0)
				{
					if (MISSChance > 0.9f)
					{
						//CombatText.NewText(new Rectangle((int)NPC.Center.X, (int)NPC.Center.Y, 10, 10), new Color(250,180,0) , "MISS", false, false);//好像会一直弹
						MISSChance = 0f;
						return false;
					}
					else
					{
						return true;
					}
				}
				else
					return false;
			}
			else
			{
				return false;
			}	
        }

		public override void HitEffect(NPC.HitInfo hit)//击中效果
		{
			Dust.NewDust(NPC.Center, 10, 10, DustID.Fireworks, hit.HitDirection , 0, -1, new Color(255,255,255), 1f);
		}
		#endregion

		//AI
		#region
		private float npctimer;
		private bool lightsOn = false;//灯光
		private int normalatkcooldown = 120;//平A冷却
		private float aimheight;
		private float aimX;
		private float basedistance;//转阶段速度
		private int truestage1to2;//进入第二阶段开始检测器（即一转二开始检测器）
		private int truestage2;//进入第二阶段锁血解除检测器（即一转二结束检测器）
		private int timer2;//固定判定点发射计时器
		private float stage;//弹出文本计次器兼阶段标记
		private int deathtimer;//伪死亡计时
		private int deathcheck;//checkdead触发检测
		private float downAcceleration;//死亡坠落加速度
		private float timer1to2;//一转二开始计时
		private float targetX;//转换周期时的瞄准点坐标
		private float targetY;//转换周期时的瞄准点坐标
		private float vx;//横向速度
		private float vy;//纵向速度
		private float stage1to2r;//一阶段转二阶段绕圈半径
		private float stage1to2atkspeed;//一阶段转二阶段绕圈攻击间隔
		private float stage1to2locktime;//一阶段转二阶段锁血时间
		private bool IATcrashed = false;//是否坠毁？
		private float movetimer;
		private float diffX;
		private float diffY;
		private bool stg1to2movementsafe = false;
		private float stg1to2safetimer = -1f;

        public override void AI()
		{
			//动态转角
			NPC.rotation = 0.01f*NPC.velocity.X;
			npctimer++;
			movetimer++;
			timer2++;

			//攻击机制
			if (Main.masterMode) {
				if (stage == 1) {
					normalatkcooldown = 180;
				}
				else if (stage == 2) {
					normalatkcooldown = 120;
				}
			}
			else if (Main.expertMode) {
				if (stage == 1) {
					normalatkcooldown = 210;
				}
				else if (stage == 2) {
					normalatkcooldown = 150;
				}
			}
			else {
				if (stage == 1) {
					normalatkcooldown = 240;
				}
				else if (stage == 2) {
					normalatkcooldown = 180;
				}
			}
			//绘制参数
			if (Main.gamePaused != true) {
				bartimer++;
			}
			//属性框的大小和位置
			if (NPC.life > 1) {
				barscale = bartimer / 60f;
				bardistance = bartimer;
			}
			else {
				if (Main.gamePaused != true) {
					crashtimer++;
				}
				barscale = 1 - crashtimer / 60f;
				bardistance = 60 - crashtimer;
			}

			if (barscale > 1f) {
				barscale = 1f;
			}
			else if (barscale < 0f) {
				barscale = 0f;
			}

			if (bardistance > 60f) {
				bardistance = 60f;
			}
			else if (bardistance < 0f) {
				bardistance = 0f;
			}

			stage1healthscale = (float)NPC.life / NPC.lifeMax;

			atkscale = timer2 / (float)normalatkcooldown;
			if (atkscale > 1f) {
				atkscale = 1f;
			}

			var newSource = NPC.GetSource_FromThis();
			Lighting.AddLight((int)((NPC.position.X + (NPC.width / 2)) / 16f), (int)((NPC.position.Y + (NPC.height / 2)) / 16f), 0.5f, 0f, 0.25f);//发光

			Player Player = Main.player[NPC.target];//仇恨判定和死亡判定
			if (!Player.active || Player.dead)
			{
				NPC.TargetClosest(false);
				Player = Main.player[NPC.target];
				if (Player.dead)
				{
					if (NPC.timeLeft > 15)
					{
						NPC.timeLeft = 15;
					}
					if (NPC.velocity.Y > -8)
					{
						NPC.velocity.Y -= 0.5f;
					}
					return;
				}
			}

			if (IATcrashed != true)//拖尾特效，坠毁时就没有了
			{
				Dust taildust2 = Dust.NewDustPerfect(NPC.Center + new Vector2(0, 12), 20, new Vector2(0f, 0f), 0, new Color(255, 255, 255), 1.4f);//蓝色
				taildust2.noGravity = true;
			}

			//阶段判定区
			if(stage == 0)
			{
				Main.NewText(Language.GetTextValue("Mods.ArknightsMod.StatusMessage.IACT.Summon2"), 240, 0, 0);//出场提示语
				stage = 1;//出场时文本计数=1，也即第一阶段
			}
			if(stage == 1 && NPC.life < NPC.lifeMax * expertHealthFrac)//第二阶段提示语（12000/16000；21600/24000；32400/36000）
			{
				Main.NewText(Language.GetTextValue("Mods.ArknightsMod.StatusMessage.IACT.Stage1to2"), 240, 0, 0);
				truestage1to2 = 1;//开始一转二
				stage = 1.5f;//文本计数=1.5，也即一转二阶段
			}


			if(NPC.life <= NPC.lifeMax * expertHealthFrac)//两个阶段更改伤害值和护盾
			{
				NPC.defense = 15;
				NPC.damage = 72;
			}
			else//一阶段
			{
				NPC.defense = 30;
				NPC.damage = 48;
			}

			//移动锚点
			if (Main.masterMode) {
				aimX = 300 * (float)Math.Sin(Math.PI * movetimer / 180);
				aimheight = 300;
			}
			else if (Main.expertMode) {
				aimX = 240 * (float)Math.Sin(Math.PI * movetimer / 240);
				aimheight = 330;
			}
            else {
				aimX = 180 * (float)Math.Sin(Math.PI * movetimer / 360);
				aimheight = 360;
            }

			//主体AI
			if(NPC.life > 1 && NPC.dontTakeDamage != true)//移动方式
			{
				if(truestage2 == 1 && timer2 >= normalatkcooldown)
				{
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Assets/Sound/ImperialArtilleyCoreTargeteer/AlertPro") with { Volume = 1f, Pitch = 0f }, NPC.Center);
					Projectile.NewProjectile(newSource, Player.Center.X, Player.Center.Y, 0, 0, ModContent.ProjectileType<ExplodeAim>(), NPC.damage, 0f, 0, 0);
					timer2 = 0;
				}
				if (timer2 >= normalatkcooldown && stage != 2)
				{
					timer2 = 0;
					SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Assets/Sound/ImperialArtilleyCoreTargeteer/AlertPro") with { Volume = 1f, Pitch = 0f }, NPC.Center);
					Projectile.NewProjectile(newSource, Player.Center.X, Player.Center.Y, 0, 0, ModContent.ProjectileType<ExplodeAimSmall>(), NPC.damage, 0f, 0, 0);
				}

				//灯光判定
				if (timer2 >= 0 && timer2 <= 10) {
					lightsOn = true;
				}
				else if (timer2 >= 20 && timer2 <= 30) {
					lightsOn = true;
				}
				else if (timer2 >= 40 && timer2 <= 50) {
					lightsOn = true;
				}
				else {
					lightsOn = false;
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
				basedistance = 2 * (float)Math.Sqrt(Math.Pow(diffX/16, 2) + Math.Pow(diffY/16, 2))+ 20f;//到玩家的距离（格数）+基准速度20f
				if(basedistance > 40f) {
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

				if (Math.Sqrt(Math.Pow(diffX, 2) + Math.Pow(diffY, 2)) >= 60 * 16)//距离远于60格
				{
					ax = 0.6f;
					ax = 0.4f;
                    if (Main.masterMode)
                    {
                        if (stage == 2)
                        {
                            vx = 16f;
                            vy = 16f;
                        }
                        if (stage == 1)
                        {
                            vx = 12f;
                            vy = 12f;
                        }
                    }
                    else if (Main.expertMode)
                    {
                        if (stage == 2)
                        {
                            vx = 14f;
                            vy = 14f;
                        }
                        if (stage == 1)
                        {
                            vx = 12f;
                            vy = 10f;
                        }
                    }
                    else
                    {
                        if (stage == 2)
                        {
                            vx = 12f;
                            vy = 12f;
                        }
                        if (stage == 1)
                        {
                            vx = 10f;
                            vy = 8f;
                        }
                    }
                }
                else//基础移动速度设置
				{
					ax = 0.3f;
					ax = 0.2f;
					if (Main.masterMode)
					{
						if (stage == 2)
						{
							vx = 8f;
							vy = 8f;
						}
						if (stage == 1)
						{
							vx = 6f;
							vy = 6f;
						}
					}
					else if (Main.expertMode)
					{
						if (stage == 2)
						{
							vx = 7f;
							vy = 7f;
						}
						if (stage == 1)
						{
							vx = 6f;
							vy = 5f;
						}
					}
					else
					{
						if (stage == 2)
						{
							vx = 6f;
							vy = 6f;
						}
						if (stage == 1)
						{
							vx = 5f;
							vy = 4f;
						}
					}
				}
			}

			if(truestage1to2 == 1)//一二阶段转阶段锁血以及攻击
			{
				NPC.life = (int)(NPC.lifeMax * expertHealthFrac);//先卡在线上

				if(Main.masterMode)
				{
					stage1to2locktime = 300;
					stage1to2r = 300;
					stage1to2atkspeed = 20;
				}
				else if(Main.expertMode)
				{
					stage1to2locktime = 360;
					stage1to2r = 400;
					stage1to2atkspeed = 30;
				}
				else
				{
					stage1to2locktime = 360;
					stage1to2r = 500;
					stage1to2atkspeed = 45;
				}

			 	if(stg1to2safetimer <= stage1to2locktime - 180)
				{
					timer1to2++;
					NPC.dontTakeDamage = true;
					if (timer1to2 < 180)
					{
						NPC.life = (int)(NPC.lifeMax * expertHealthFrac - 1);
						stage = 2;
						Vector2 targetPosition = Player.Center + new Vector2(targetX, -stage1to2r);
						Vector2 targetv = basedistance * (targetPosition - NPC.Center).SafeNormalize(Vector2.One);
						NPC.velocity = Vector2.Lerp(NPC.velocity, targetv, 0.03f);
					}
					else if (timer1to2 >= 180)
					{
						if ((int)(Player.Center.X-NPC.Center.X) >= -12 && (int)(Player.Center.X - NPC.Center.X) <= 12 && (int)(Player.Center.Y - NPC.Center.Y) >= stage1to2r - 12 && (int)(Player.Center.Y - NPC.Center.Y) <= stage1to2r + 12)
						{
							stg1to2movementsafe = true;
						}

						if (stg1to2movementsafe != true)//防跳变保护机制
						{
							Vector2 targetPosition = Player.Center + new Vector2(targetX, -stage1to2r);
							Vector2 targetv = basedistance * (targetPosition - NPC.Center).SafeNormalize(Vector2.One);
							NPC.velocity = Vector2.Lerp(NPC.velocity, targetv, 0.03f);
						}
						else
						{
							stg1to2safetimer++;
						}

						if ((int)stg1to2safetimer == 0)
						{
							SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/iactstage1to2") with { Volume = 1f, Pitch = 0f }, Player.Center);
						}

						if (stg1to2safetimer >= 0)
						{
							targetX = (float)(stage1to2r * Math.Sin(2 * stg1to2safetimer * Math.PI / (stage1to2locktime - 180)));
							targetY = (float)(-stage1to2r * Math.Cos(2 * stg1to2safetimer * Math.PI / (stage1to2locktime - 180)));
							NPC.velocity = new Vector2(Player.Center.X, Player.Center.Y) + new Vector2(targetX, targetY) - NPC.Center;//期间的位置变动
							if ((int)stg1to2safetimer % stage1to2atkspeed == 0)
							{
								Projectile.NewProjectile(newSource, NPC.Center.X, NPC.Center.Y, 0, 0, ModContent.ProjectileType<ExplodeAim>(), (int)(NPC.damage), 0f, 0, 0);
							}
						}
					}
				}
				else
				{
					truestage1to2 = 2;
					truestage2 = 1;
					NPC.dontTakeDamage = false;
				}
			}

			if (NPC.life <= 1)//坠毁及其保护机制
			{
				if(deathcheck == 1)//触发checkdead之后
				{
					IATcrashed = true;
					NPC.noTileCollide = false;//与物块相撞	
					deathtimer++;
					if (stage == 2)
					{
						Main.NewText(Language.GetTextValue("Mods.ArknightsMod.StatusMessage.IACT.End"), 240, 0, 0);
						stage += 1;
					}
					if (deathtimer >= 180)//下坠3秒后爆炸并触发爆炸粒子
					{
						deathcheck = 2;
						Projectile.NewProjectile(newSource,NPC.Center.X, NPC.Center.Y,0,0,ModContent.ProjectileType<Deathdust>(),0,0f,0,0);//爆炸粒子
					}
					NPC.velocity.X = NPC.velocity.X * (360 - deathtimer)/360;
					downAcceleration = deathtimer * 0.01f;
					if(downAcceleration > 0.5f)
					{
						downAcceleration = 0.5f;
					}
					NPC.velocity.Y += downAcceleration;
					Vector2 dustPos = NPC.Center + new Vector2(Main.rand.NextFloat(12), 0).RotatedByRandom(MathHelper.TwoPi);
					for(int i = 0 ; i < 2 ; i++)
					{
						Dust dust8 = Dust.NewDustPerfect(dustPos, 219, Velocity: Vector2.Zero, Scale: 1.35f);
						dust8.velocity = (3*dustPos - 3*NPC.Center);
					}
				}
				if(deathcheck == 2)//赐死
				{
					NPC.dontTakeDamage = false;
					NPC.life = 0;
					NPC.checkDead();
				}
			}
		}

		public override bool CheckDead()//锁血及锁血解除
		{
			Player Player = Main.player[Main.myPlayer];
			if(deathcheck == 2)//死亡
			{
				if (!Main.dedServ)//Gore
                {
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, new Vector2((float)Main.rand.Next(-5, 5) * 0.6f, (float)Main.rand.Next(-40, -20) * 0.6f), Mod.Find<ModGore>("IACT Gore 1").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, new Vector2((float)Main.rand.Next(-30, 31) * 0.6f, (float)Main.rand.Next(-30, 31) * 0.6f), Mod.Find<ModGore>("IACT Gore 2").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, new Vector2((float)Main.rand.Next(-30, 31) * 0.6f, (float)Main.rand.Next(-30, 31) * 0.6f), Mod.Find<ModGore>("IACT Gore 3").Type, 1f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, new Vector2((float)Main.rand.Next(-30, 31) * 0.6f, (float)Main.rand.Next(-30, 31) * 0.6f), Mod.Find<ModGore>("IACT Gore 4").Type, 1f);
                }
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Sounds/IACTboom") with { Volume = 2f, Pitch = 0f }, Player.Center);//死亡音效
				Main.NewText(Language.GetTextValue("Mods.ArknightsMod.StatusMessage.IACT.Complete"), 138, 0, 18);
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

	public class ExplodeAimSmall : ModProjectile
	{
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Projectile.width = 110;
			Projectile.height = 110;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 75;
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

		public override void AI()
		{
			var newSource = Projectile.GetSource_FromThis();
			timer++;
			if (missled != true) {
				randomx = Main.rand.NextFloat(-300, 300);

				Projectile.NewProjectile(newSource, Projectile.Center.X + randomx, Projectile.Center.Y - 1800, -randomx / 60, 0, ModContent.ProjectileType<missle>(), 10, 0f, 0, 0);
				missled = true;
			}

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

			if (timer == 10) {
				Projectile.NewProjectile(newSource, Projectile.Center.X-30, Projectile.Center.Y-30, 0, 0, ModContent.ProjectileType<HitboxRedCornerSmall1>(), 0, 0f, 0, 0);
				Projectile.NewProjectile(newSource, Projectile.Center.X+30, Projectile.Center.Y-30, 0, 0, ModContent.ProjectileType<HitboxRedCornerSmall2>(), 0, 0f, 0, 0);
				Projectile.NewProjectile(newSource, Projectile.Center.X-30, Projectile.Center.Y+30, 0, 0, ModContent.ProjectileType<HitboxRedCornerSmall3>(), 0, 0f, 0, 0);
				Projectile.NewProjectile(newSource, Projectile.Center.X+30, Projectile.Center.Y+30, 0, 0, ModContent.ProjectileType<HitboxRedCornerSmall4>(), 0, 0f, 0, 0);
			}

			if (timer == 60) {
				SoundEngine.PlaySound(new SoundStyle("ArknightsMod/Assets/Sound/ImperialArtilleyCoreTargeteer/Explode") with { Volume = 1f, Pitch = 0f }, Projectile.Center);
				Projectile.NewProjectile(newSource, Projectile.Center.X, Projectile.Center.Y, 0, 0, ModContent.ProjectileType<ExplodeAreaSmall>(), 10, 0f, 0, 0);
			}
		}
	}
	public class HitboxRedCornerSmall1 : ModProjectile
	{
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 80;
			Projectile.alpha = 10;
			Projectile.damage = 0;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.scale = 1f;
		}

		private float timer;

		public override void AI() {
			timer++;

			if (timer >= 0 && timer <= 10) {
				Projectile.scale = (float)Math.Sin(Math.PI * timer / 20f);
			}
			else if (timer > 10 && timer <= 50) {
				Projectile.scale = 1f;
			}
			else if (timer > 50 && timer <= 65) {
				Projectile.scale = (float)Math.Cos(Math.PI * (timer - 50) / 30f);
			}

			if (timer >= 0 && timer <= 20) {
				Projectile.alpha = (int)(120 * Math.Cos(Math.PI * timer / 20f) + 120);
			}
			else if (timer > 20 && timer <= 50) {
				Projectile.alpha = 0;
			}
			else if (timer > 50 && timer <= 80) {
				Projectile.alpha = (int)(-120 * Math.Cos(Math.PI * (timer - 50) / 30) + 120);
			}

			if (timer <= 20) {
				Projectile.velocity = new Vector2(-5.5f, -5.5f);
			}
			else if (timer > 20 && timer <= 50) {
				Projectile.velocity = Vector2.Zero;
			}
			else if (timer > 50) {
				Projectile.velocity = new Vector2(7.333f, 7.333f);
			}
		}
	}

	public class HitboxRedCornerSmall2 : ModProjectile
	{
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 80;
			Projectile.alpha = 10;
			Projectile.damage = 0;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.scale = 1f;
		}

		private float timer;

		public override void AI() {
			timer++;

			if (timer >= 0 && timer <= 10) {
				Projectile.scale = (float)Math.Sin(Math.PI * timer / 20f);
			}
			else if (timer > 10 && timer <= 50) {
				Projectile.scale = 1f;
			}
			else if (timer > 50 && timer <= 65) {
				Projectile.scale = (float)Math.Cos(Math.PI * (timer - 50) / 30f);
			}

			if (timer >= 0 && timer <= 20) {
				Projectile.alpha = (int)(120 * Math.Cos(Math.PI * timer / 20f) + 120);
			}
			else if (timer > 20 && timer <= 50) {
				Projectile.alpha = 0;
			}
			else if (timer > 50 && timer <= 80) {
				Projectile.alpha = (int)(-120 * Math.Cos(Math.PI * (timer - 50) / 30) + 120);
			}

			if (timer <= 20) {
				Projectile.velocity = new Vector2(5.5f, -5.5f);
			}
			else if (timer > 20 && timer <= 50) {
				Projectile.velocity = Vector2.Zero;
			}
			else if (timer > 50) {
				Projectile.velocity = new Vector2(-7.333f, 7.333f);
			}
		}
	}

	public class HitboxRedCornerSmall3 : ModProjectile
	{
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 80;
			Projectile.alpha = 10;
			Projectile.damage = 0;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.scale = 1f;
		}

		private float timer;

		public override void AI() {
			timer++;

			if (timer >= 0 && timer <= 10) {
				Projectile.scale = (float)Math.Sin(Math.PI * timer / 20f);
			}
			else if (timer > 10 && timer <= 50) {
				Projectile.scale = 1f;
			}
			else if (timer > 50 && timer <= 65) {
				Projectile.scale = (float)Math.Cos(Math.PI * (timer - 50) / 30f);
			}

			if (timer >= 0 && timer <= 20) {
				Projectile.alpha = (int)(120 * Math.Cos(Math.PI * timer / 20f) + 120);
			}
			else if (timer > 20 && timer <= 50) {
				Projectile.alpha = 0;
			}
			else if (timer > 50 && timer <= 80) {
				Projectile.alpha = (int)(-120 * Math.Cos(Math.PI * (timer - 50) / 30) + 120);
			}

			if (timer <= 20) {
				Projectile.velocity = new Vector2(-5.5f, 5.5f);
			}
			else if (timer > 20 && timer <= 50) {
				Projectile.velocity = Vector2.Zero;
			}
			else if (timer > 50) {
				Projectile.velocity = new Vector2(7.333f, -7.333f);
			}
		}
	}

	public class HitboxRedCornerSmall4 : ModProjectile
	{
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Projectile.width = 80;
			Projectile.height = 80;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 80;
			Projectile.alpha = 10;
			Projectile.damage = 0;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.scale = 1f;
		}

		private float timer;

		public override void AI() {
			timer++;

			if (timer >= 0 && timer <= 10) {
				Projectile.scale = (float)Math.Sin(Math.PI * timer / 20f);
			}
			else if (timer > 10 && timer <= 50) {
				Projectile.scale = 1f;
			}
			else if (timer > 50 && timer <= 65) {
				Projectile.scale = (float)Math.Cos(Math.PI * (timer - 50) / 30f);
			}

			if (timer >= 0 && timer <= 20) {
				Projectile.alpha = (int)(120 * Math.Cos(Math.PI * timer / 20f) + 120);
			}
			else if (timer > 20 && timer <= 50) {
				Projectile.alpha = 0;
			}
			else if (timer > 50 && timer <= 80) {
				Projectile.alpha = (int)(-120 * Math.Cos(Math.PI * (timer - 50) / 30f) + 120);
			}

			if (timer <= 20) {
				Projectile.velocity = new Vector2(5.5f, 5.5f);
			}
			else if (timer > 20 && timer <= 50) {
				Projectile.velocity = Vector2.Zero;
			}
			else if (timer > 50) {
				Projectile.velocity = new Vector2(-7.333f, -7.333f);
			}
		}
	}

	public class ExplodeAreaSmall : ModProjectile
	{
		public override void SetStaticDefaults() {
		}
		public override void SetDefaults() {
			Projectile.width = 360;
			Projectile.height = 360;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 5;
			Projectile.alpha = 0;
			Projectile.damage = 60;
			Projectile.light = 0.6f;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.scale = 1f;
			Projectile.hide = true;
		}

		public override void AI() {
			Vector2 dustPos = Projectile.Center + new Vector2(Main.rand.NextFloat(16), 0).RotatedByRandom(MathHelper.TwoPi);
			Dust dust = Dust.NewDustPerfect(dustPos, 55, Velocity: Vector2.Zero, Scale: 1.5f);
			dust.noGravity = true;
			dust.velocity = (4 * dustPos - 4 * Projectile.Center);
			Dust dust2 = Dust.NewDustPerfect(dustPos, 6, Velocity: Vector2.Zero, Scale: 4f);
			dust2.noGravity = true;
			dust2.velocity = (4 * dustPos - 4 * Projectile.Center);
			for (int i = 0; i < 2; i++) {
				Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Pixie, Scale: 1.5f)].noGravity = true;
			}
			for (int j = 0; j < 4; j++) {
				Main.dust[Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, Scale: 4f)].noGravity = true;
			}
		}

	}
}