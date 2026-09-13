using ArknightsMod.Common.NPCDeathDebris;
using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Content.BossBars;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ArknightsMod.Content.NPCs.Enemy.OF.Pmp
{
    public class Pompeii : ModNPC
    {
        private enum AIState
        {
            Mode1Crawl,
            Mode2ShootFireballs,
            Mode3Idle,
            Mode4RainFire,
            Mode5Idle,
            ModeJumpToPlayer,
            TailKillRise,
            TailKillSpin,
            TailKillDeath
        }

        private AIState _currentState = AIState.Mode1Crawl;
        private int _modeTimer;
        private int _explosionCooldown;
        private int _tailKillDuration;
        private bool _tailKillDebrisSpawned;
        private bool _hasEnteredTailKill;
        private Vector2 _tailKillStartPos;
        private int _fireRainCounter;
        private int _currentFrame; //当前帧
        private int _frameCounter; 
        private const int FrameSpeed = 5;
        private int _frameResetFlag;
		private float escapetimer;
		private int _slugEggCooldown = 120; // 庞贝喷出炽焰源石虫卵的冷却计时
		private int _heightMismatchTimer;
		private bool _isTeleportJumping;
		private int  _jumpTimer;
		private bool _jumpPrepCompleted;
		private Vector2 _jumpTargetPos;
		private float _jumpGravity;
		private Vector2 _jumpTrailLastPos;
		private int _stg2BurstCount;
		private int _stg2BurstTimer;
		private bool _platformLockActive;
		private float _platformLockBottomY;
		private const int JumpLandingSearchWidthTiles = 24;
		private const int JumpLandingClearanceTiles = 12;
		private const int JumpLandingDropToleranceTiles = 3;
		private const int JumpLandingRiseToleranceTiles = 30;
		private const int LootOrirockCubeCount = 20;
		private const int LootPolyesterCount = 5;
		private const int HeightMismatchJumpTriggerFrames = 300;
		private const float PlatformLockReleasePlayerBelow = 72f;
		private const float PlatformLockReleasePlayerAbove = 120f;
		private const float PlatformLockSnapTolerance = 28f;
		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("巨型的野生被感染生物。在特殊的高温环境中诞生的变异生物个体。不仅它的外观可怖，它自身的高温也让任何人不敢轻易靠近。从古至今就有不少对于巨型生物的目击报告，但若是能亲眼看见这么巨大的感染生物，恐怕永远都无法忘记吧……"),
			});
		}
		public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 53;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.ShouldBeCountedAsBoss[Type] = true;
			NPCID.Sets.TrailCacheLength[Type] = 10;
			NPCID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            NPC.width = 176;
            NPC.height = 154;
            NPC.lifeMax = 4000;
            NPC.defense = 15;
            NPC.damage = 80;
            NPC.knockBackResist = 0f;
            NPC.boss = true;
            NPC.lavaImmune = false;
            NPC.noTileCollide = false;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.aiStyle = -1;
			NPC.BossBar = ModContent.GetInstance<NoBossBar>();
			Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/PmpBoss");
		}

		public override void OnSpawn(IEntitySource source)
		{
			_slugEggCooldown = Main.rand.Next(60, 121);
			escapetimer = 0f;
			_tailKillDebrisSpawned = false;
			SelectNextAttackState();
		}

		//NPC专家模式|大师模式血量倍率（普通模式血量*倍率*2|血量*倍率*3）
		public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) {
			NPC.lifeMax = (int)(NPC.lifeMax * 0.75f * balance);
			NPC.damage = (int)(NPC.damage * 0.75f);
		}

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.buffImmune[BuffID.OnFire] = false;
			if (!target.HasBuff(BuffID.OnFire)) {
				target.AddBuff(BuffID.OnFire, Main.masterMode ? 180 : Main.expertMode ? 180 : 270);
			}
		}

		public override bool? CanBeHitByItem(Player player, Item item)//无敌帧
		{
			return null;
		}

		public override bool? CanBeHitByProjectile(Projectile Projectile)//不被敌方弹幕和无来源弹幕攻击&闪避
		{
			if (Projectile.hostile == true) {
				return false;
			}
			else if (Projectile.friendly == true) {
				return null;
			}
			else {
				return false;
			}
		}

		public override bool CheckDead() //尾杀
        {
            if (!_hasEnteredTailKill)
            {
                InitTailKill();
                return false;
            }
            return true;
        }

		private bool ispmpstg2 = false;

		#region 自定义血条
		public override bool? DrawHealthBar(byte hbPosition, ref float scale, ref Vector2 position) {
			return false;
		}

		private float Bartimer;
		private float TargetHealthBarLength;
		private float ActualHealthBarLength;

		//自制血条
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (_isTeleportJumping)
			{
				Texture2D npcTex = TextureAssets.Npc[Type].Value;
				Rectangle frame = NPC.frame;
				Vector2 origin = frame.Size() / 2f;
				SpriteEffects fx = NPC.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

				for (int i = NPC.oldPos.Length - 1; i >= 1; i--)
				{
					if (NPC.oldPos[i] == Vector2.Zero)
						continue;

					float opacity = (NPC.oldPos.Length - i) / (float)NPC.oldPos.Length * 0.35f;
					Vector2 drawPos = NPC.oldPos[i] + NPC.Size / 2f - Main.screenPosition + new Vector2(0f, NPC.gfxOffY);
					Main.EntitySpriteDraw(npcTex, drawPos, frame, new Color(255, 120, 80) * opacity, NPC.rotation, origin, NPC.scale, fx, 0);
				}
			}

			//发光图层
			Texture2D PmpGlow = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/OF/Pmp/Pompeii_glow").Value;
			Main.EntitySpriteDraw(PmpGlow, NPC.Center - Main.screenPosition + new Vector2(0, 3) + new Vector2(0, (_currentFrame * PmpGlow.Height / 53 / 2)).RotatedBy(NPC.rotation), new Rectangle(0, (int)(_currentFrame * PmpGlow.Height / 53f), PmpGlow.Width, PmpGlow.Height / 53), Color.White, NPC.rotation, new Vector2(PmpGlow.Width / 2, (_currentFrame + 1) * (PmpGlow.Height / 53) / 2), 1f, NPC.direction < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0);

			Texture2D BarTexBot = ModContent.Request<Texture2D>("ArknightsMod/Content/BossBars/PmpBossBarBot").Value;
			Texture2D BarTexMed2 = ModContent.Request<Texture2D>("ArknightsMod/Content/BossBars/PmpBossBarMed2").Value;
			Texture2D BarTexMed = ModContent.Request<Texture2D>("ArknightsMod/Content/BossBars/PmpBossBarMed").Value;
			if (!_hasEnteredTailKill) {
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
			Main.EntitySpriteDraw(BarTexMed2, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 + (Main.screenHeight / 2 - 106 * fixedscr) / Main.GameZoomTarget), new Rectangle(0, 0, (int)(ActualHealthBarLength * 4), BarTexMed.Height), Color.White * (Bartimer / 120), 0, new Vector2(BarTexMed.Width / 2, BarTexMed.Height / 2), 1 / Main.GameZoomTarget, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(BarTexMed, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 + (Main.screenHeight / 2 - 106 * fixedscr) / Main.GameZoomTarget), new Rectangle(0, 0, (int)(TargetHealthBarLength), BarTexMed.Height), Color.White * (Bartimer / 120), 0, new Vector2(BarTexMed.Width / 2, BarTexMed.Height / 2), 1 / Main.GameZoomTarget, SpriteEffects.None, 0);
		}
		#endregion

		public override void AI()
        {
            NPC.TargetClosest();
            Player player = Main.player[NPC.target];
			Lighting.AddLight(NPC.Center, 10);
			#region 玩家死亡后消失
			float loseInterestDistance = Math.Max(Main.screenWidth * 1.8f, 2200f);
			if (!player.active || player.dead || Vector2.Distance(player.Center, NPC.Center) > loseInterestDistance) {
				NPC.TargetClosest(false);
				player = Main.player[NPC.target];
				escapetimer++;
				if (escapetimer > 50) {
					for (int i = 0; i < 3; i++) {
						Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Pixie, Scale: 1.5f)].noGravity = true;
					}
					for (int j = 0; j < 6; j++) {
						Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, Scale: 2.5f)].noGravity = true;
					}
				}
				if (escapetimer > 60) {
					NPC.Center = Vector2.Zero;
					NPC.active = false;
				}
				return;
			}
			#endregion

			if (_slugEggCooldown > 0) _slugEggCooldown--;
			UpdatePlatformLock(player);

			// 高度差检测：与玩家长期不在同层时触发追跳（平台锁期间加速计时，作为兜底）
			if (!_hasEnteredTailKill && _currentState != AIState.ModeJumpToPlayer)
			{
				float heightDiff = Math.Abs(player.Center.Y - NPC.Center.Y);
				if (heightDiff > 180f)
					_heightMismatchTimer += _platformLockActive ? 2 : 1;
				else
					_heightMismatchTimer = Math.Max(0, _heightMismatchTimer - 2);
				if (_heightMismatchTimer >= HeightMismatchJumpTriggerFrames)
				{
					_heightMismatchTimer = 0;
					_platformLockActive = false;
					_currentState = AIState.ModeJumpToPlayer;
					_modeTimer = 0;
					_frameResetFlag = 2;
					NPC.netUpdate = true;
				}
			}

			ExecuteStateMachine(player);
            UpdateAnimation();

			if ((float)NPC.life / (float)NPC.lifeMax <= 0.5f && !ispmpstg2) {
				Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ProjectileType<PMPSTG2Effect>(), 0, 0f, -1, 1800, 1);
				Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ProjectileType<PMPSTG2Effect>(), 0, 0f, -1, 1800, 0.5f);
				Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ProjectileType<PMPSTG2Effect>(), 0, 0f, -1, 1800, -0.5f);
				Projectile.NewProjectile(null, NPC.Center, Vector2.Zero, ProjectileType<PMPSTG2Effect>(), 0, 0f, -1, 1800, -1);
				ispmpstg2 = true;
				_stg2BurstCount = 5; // 立即触发5连快速喷卵
				_stg2BurstTimer = 0;
			}

			// 5连快速喷卵（每12帧一次）
			if (_stg2BurstCount > 0)
			{
				_stg2BurstTimer++;
				if (_stg2BurstTimer % 12 == 0)
				{
					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						Player burstTarget = Main.player[NPC.target];
						Vector2 burstTargetPoint = GetSlugEggTargetPoint(burstTarget);
						Vector2 bp1 = NPC.Center + new Vector2(55f * NPC.direction, -35f);
						Vector2 bv1 = SolveSlugEggLaunchVelocity(bp1, burstTargetPoint, false);
						Projectile.NewProjectile(NPC.GetSource_FromAI(), bp1, bv1, ModContent.ProjectileType<PmpSlugEgg>(), 0, 0f, Main.myPlayer);
						Vector2 bp2 = NPC.Center + new Vector2(-25f * NPC.direction, -70f);
						Vector2 bv2 = SolveSlugEggLaunchVelocity(bp2, burstTargetPoint + new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(-12f, 12f)), true);
						Projectile.NewProjectile(NPC.GetSource_FromAI(), bp2, bv2, ModContent.ProjectileType<PmpSlugEgg>(), 0, 0f, Main.myPlayer);
					}
					_stg2BurstCount--;
				}
			}

		}

		public override void SendExtraAI(System.IO.BinaryWriter writer)
		{
			writer.Write((byte)_currentState);
			writer.Write(_platformLockActive);
			writer.Write(_platformLockBottomY);
			writer.Write(_isTeleportJumping);
			writer.Write(_hasEnteredTailKill);
			writer.Write(_jumpTargetPos.X);
			writer.Write(_jumpTargetPos.Y);
			writer.Write(_jumpGravity);
			writer.Write(_heightMismatchTimer);
			writer.Write(ispmpstg2);
		}

		public override void ReceiveExtraAI(System.IO.BinaryReader reader)
		{
			_currentState = (AIState)reader.ReadByte();
			_platformLockActive = reader.ReadBoolean();
			_platformLockBottomY = reader.ReadSingle();
			_isTeleportJumping = reader.ReadBoolean();
			_hasEnteredTailKill = reader.ReadBoolean();
			_jumpTargetPos = new Vector2(reader.ReadSingle(), reader.ReadSingle());
			_jumpGravity = reader.ReadSingle();
			_heightMismatchTimer = reader.ReadInt32();
			ispmpstg2 = reader.ReadBoolean();
		}

        private void ExecuteStateMachine(Player player)
        {
            switch (_currentState)
            {
                case AIState.Mode1Crawl: Mode1Crawl(player); break;
                case AIState.Mode2ShootFireballs: Mode2ShootFireballs(player); break;
                case AIState.Mode3Idle: Mode3Idle(); break;
                case AIState.Mode4RainFire: Mode4RainFire(player); break;
                case AIState.Mode5Idle: Mode5Idle(); break;
                case AIState.ModeJumpToPlayer: ModeJumpToPlayer(player); break;
                case AIState.TailKillRise: HandleTailKillRise(); break;
                case AIState.TailKillSpin: HandleTailKillSpin(player); break;
                case AIState.TailKillDeath: HandleTailKillDeath(); break;
            }

            //if (!_hasEnteredTailKill)
            NPC.direction = NPC.spriteDirection = (player.Center.X > NPC.Center.X) ? -1 : 1;
        }

		#region 帧图与绘制
		private void UpdateAnimation()
        {
            if (_frameResetFlag > 0)
            {
                _currentFrame = GetStateMinFrame(_currentState);
                _frameCounter = 0;
                _frameResetFlag--;
            }

            _frameCounter++;
            if (_frameCounter < FrameSpeed) return;

            _frameCounter = 0;
            int minFrame = GetStateMinFrame(_currentState);
            int maxFrame = GetStateMaxFrame(_currentState);
            //if (_currentState == AIState.TailKillSpin) return;
            if (_currentState == AIState.TailKillDeath && _currentFrame >= maxFrame) return;
            if (_currentFrame < minFrame || _currentFrame >= maxFrame)
                _currentFrame = minFrame;
            else
                _currentFrame++;
        }

        private int GetStateMinFrame(AIState state)
        {
            return state switch
            {
                AIState.Mode1Crawl => 26,
                AIState.Mode2ShootFireballs => 1,
                AIState.Mode3Idle => 19,
                AIState.Mode4RainFire => 8,
                AIState.Mode5Idle => 19,
                AIState.ModeJumpToPlayer => 1,
                AIState.TailKillRise => 1,
                AIState.TailKillSpin => 1,
                AIState.TailKillDeath => 11,
                _ => 0
            };
        }

        private int GetStateMaxFrame(AIState state)
        {
            return state switch
            {
                AIState.Mode1Crawl => 35,
                AIState.Mode2ShootFireballs => 10,
                AIState.Mode3Idle => 25,
                AIState.Mode4RainFire => 11,
                AIState.Mode5Idle => 25,
                AIState.ModeJumpToPlayer => 10,
                AIState.TailKillRise => 10,
                AIState.TailKillSpin => 11,
                AIState.TailKillDeath => 17,
                _ => Main.npcFrameCount[Type]
            };
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frame = new Rectangle(0, _currentFrame * frameHeight, NPC.width, frameHeight);
        }
		#endregion

		private void Mode1Crawl(Player player)
        {
			float timecount = Main.masterMode ? 3f : Main.expertMode ? 4f : 5f;
			float ax = 0.06f;
			float vx = Main.masterMode ? 2.0f : Main.expertMode ? 1.8f : 1.5f;
			int direction = (player.Center.X > NPC.Center.X).ToDirectionInt();
			NPC.direction = direction;

			float dx = player.Center.X - NPC.Center.X;
			if (Math.Abs(dx) > 72f)
			{
				NPC.velocity.X += direction * ax;
			}
			else
			{
				NPC.velocity.X *= 0.92f;
			}
			NPC.velocity.X = Math.Clamp(NPC.velocity.X, -vx, vx);

			ShootSlugEggs();

			if (++_modeTimer >= 60 * timecount)
            {
				SelectNextAttackState(AIState.Mode1Crawl);
            }
        }

		private void ShootSlugEggs()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient) return;
			if (_slugEggCooldown > 0) return;

			// 蓄力阶段：停下来等待40帧后发射
			const int chargeTime = 20;
			// 用 _slugEggCooldown 的负数区间表示蓄力计时（-chargeTime ~ 0）
			// 进入蓄力：_slugEggCooldown == 0 时设为 -chargeTime
			if (_slugEggCooldown == 0)
			{
				_slugEggCooldown = -chargeTime;
				return;
			}

			// 蓄力中：强制停止移动
			if (_slugEggCooldown < 0)
			{
				NPC.velocity.X *= 0.85f;
				_slugEggCooldown++;
				if (_slugEggCooldown < 0) return; // 还在蓄力中
				// 蓄力结束，发射
			}

			// 根据玩家所在平台层高度动态调整虫卵落点
			Player eggTarget = Main.player[NPC.target];
			Vector2 targetPoint = GetSlugEggTargetPoint(eggTarget);
			Vector2 spawnPos1 = NPC.Center + new Vector2(55f * NPC.direction, -35f);
			Vector2 spawnPos2 = NPC.Center + new Vector2(-25f * NPC.direction, -70f);
			Vector2 vel1 = SolveSlugEggLaunchVelocity(spawnPos1, targetPoint, false);
			Vector2 vel2 = SolveSlugEggLaunchVelocity(spawnPos2, targetPoint + new Vector2(Main.rand.NextFloat(-20f, 20f), Main.rand.NextFloat(-12f, 12f)), true);
			Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos1, vel1, ModContent.ProjectileType<PmpSlugEgg>(), 0, 0f, Main.myPlayer);
			Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos2, vel2, ModContent.ProjectileType<PmpSlugEgg>(), 0, 0f, Main.myPlayer);

			// 重置随机CD
			_slugEggCooldown = Main.rand.Next(120, 241);
		}

		private void UpdatePlatformLock(Player player)
		{
			if (!_platformLockActive || _hasEnteredTailKill)
				return;

			// 跳跃状态交出控制权，避免平台锁覆盖跳跃期 noTileCollide / 速度设置。
			if (_currentState == AIState.ModeJumpToPlayer || _isTeleportJumping)
			{
				_platformLockActive = false;
				return;
			}

			bool playerAbove = player.Bottom.Y < _platformLockBottomY - PlatformLockReleasePlayerAbove;
			bool playerBelow = player.Bottom.Y > _platformLockBottomY + PlatformLockReleasePlayerBelow;
			if (playerAbove || playerBelow)
			{
				_platformLockActive = false;
				_heightMismatchTimer = 0;
				_currentState = AIState.ModeJumpToPlayer;
				_modeTimer = 0;
				_frameResetFlag = 2;
				NPC.netUpdate = true;
				return;
			}

			NPC.noTileCollide = false;
			float deltaBottom = _platformLockBottomY - NPC.Bottom.Y;
			if (Math.Abs(deltaBottom) <= PlatformLockSnapTolerance)
			{
				NPC.position.Y += deltaBottom;
				if (NPC.velocity.Y > 0f)
					NPC.velocity.Y = 0f;
			}
		}

		private static float GetPlayerPlatformHeight(Player player)
		{
			int centerTileX = (int)(player.Center.X / 16f);
			int sampleStartY = (int)(player.Bottom.Y / 16f);
			int maxScanTiles = 14; // 约224像素，优先捕捉玩家脚下最近平台层

			for (int y = sampleStartY; y <= sampleStartY + maxScanTiles; y++)
			{
				for (int x = centerTileX - 2; x <= centerTileX + 2; x++)
				{
					if (!WorldGen.InWorld(x, y))
						continue;

					Tile tile = Main.tile[x, y];
					if (tile == null || !tile.HasTile)
						continue;

					bool isSolidTop = Main.tileSolidTop[tile.TileType] || TileID.Sets.Platforms[tile.TileType];
					bool isSolidBlock = Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType];
					if (isSolidTop || isSolidBlock)
					{
						return y * 16f;
					}
				}
			}

			return player.Bottom.Y;
		}

		private static Vector2 GetSlugEggTargetPoint(Player player)
		{
			float platformY = GetPlayerPlatformHeight(player);
			float targetX = player.Center.X + player.velocity.X * 12f;
			float targetY = platformY - 4f;
			return new Vector2(targetX, targetY);
		}

		private Vector2 SolveSlugEggLaunchVelocity(Vector2 spawnPos, Vector2 targetPoint, bool higherArc)
		{
			float dx = targetPoint.X - spawnPos.X;
			float dy = targetPoint.Y - spawnPos.Y;
			float gravity = 0.22f;
			float preferredTime = higherArc ? 50f : 42f;
			preferredTime += MathHelper.Clamp(Math.Abs(dy) * 0.045f, 0f, higherArc ? 12f : 8f);
			float travelTime = MathHelper.Clamp(preferredTime, higherArc ? 38f : 30f, higherArc ? 68f : 56f);
			float vx = dx / travelTime;
			vx = Math.Clamp(vx, -8.2f, 8.2f);
			travelTime = Math.Max(Math.Abs(dx) / Math.Max(1.25f, Math.Abs(vx)), higherArc ? 36f : 28f);
			travelTime = Math.Min(travelTime, higherArc ? 70f : 58f);
			float vy = (dy - 0.5f * gravity * travelTime * travelTime) / travelTime;
			vy -= higherArc ? 1.35f : 0.9f;
			vy = Math.Clamp(vy, -16.8f, -6.2f);
			return new Vector2(vx, vy);
		}

		private Vector2 FindSafeJumpLandingPosition(Player player, out bool foundSafeLanding)
		{
			float playerPlatformY = GetPlayerPlatformHeight(player);
			float preferredX = player.Center.X + player.velocity.X * 18f;
			float preferredY = playerPlatformY - NPC.height - 8f;
			Vector2 fallbackTarget = new Vector2(preferredX, preferredY);
			int centerTileX = (int)(preferredX / 16f);
			int centerTileY = (int)(playerPlatformY / 16f);
			int npcCenterTileY = (int)(NPC.Bottom.Y / 16f);

			Vector2 bestCandidate = fallbackTarget;
			float bestScore = float.MaxValue;

			for (int offset = 0; offset <= JumpLandingSearchWidthTiles; offset++)
			{
				for (int side = -1; side <= 1; side += 2)
				{
					if (offset == 0 && side == 1)
						continue;

					int tileX = centerTileX + offset * side;
					if (!WorldGen.InWorld(tileX, centerTileY, 20))
						continue;

					if (!TryFindLandingSurface(tileX, centerTileY, out int surfaceY))
						continue;

					if (surfaceY > centerTileY + JumpLandingDropToleranceTiles)
						continue;
					if (surfaceY < centerTileY - JumpLandingRiseToleranceTiles)
						continue;

					if (!HasStandingRoom(tileX, surfaceY))
						continue;

					Vector2 candidate = GetLandingWorldPosition(tileX, surfaceY);
					if (candidate.Y > player.Bottom.Y + 16f)
						continue;

					float levelDeltaToPlayer = centerTileY - surfaceY;
					float climbFromNpc = npcCenterTileY - surfaceY;
					float playerHeightPenalty = Math.Abs(levelDeltaToPlayer) * 26f;
					float horizontalPenalty = Math.Abs(tileX - centerTileX) * 14f;
					float lowPlatformPenalty = surfaceY > centerTileY ? 220f + Math.Abs(surfaceY - centerTileY) * 120f : 0f;
					float climbBonus = climbFromNpc > 0f ? 260f + climbFromNpc * 18f : 0f;
					float towardPlayerBonus = Math.Max(0f, 120f - Math.Abs(candidate.X + NPC.width / 2f - player.Center.X) * 0.25f);
					float score = playerHeightPenalty + horizontalPenalty + lowPlatformPenalty - climbBonus - towardPlayerBonus;
					if (score < bestScore)
					{
						bestScore = score;
						bestCandidate = candidate;
					}
				}
			}

			foundSafeLanding = bestScore < float.MaxValue;
			return foundSafeLanding ? bestCandidate : fallbackTarget;
		}

		private bool TryFindLandingSurface(int tileX, int centerTileY, out int surfaceY)
		{
			for (int y = centerTileY - JumpLandingRiseToleranceTiles; y <= centerTileY + JumpLandingDropToleranceTiles; y++)
			{
				if (!WorldGen.InWorld(tileX, y, 10))
					continue;

				Tile tile = Framing.GetTileSafely(tileX, y);
				if (!tile.HasTile)
					continue;

				bool isSurface = Main.tileSolidTop[tile.TileType] || TileID.Sets.Platforms[tile.TileType] || (Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType]);
				if (!isSurface)
					continue;

				surfaceY = y;
				return true;
			}

			surfaceY = 0;
			return false;
		}

		private bool HasStandingRoom(int tileX, int surfaceY)
		{
			// 站立检测按实体宽度（每16像素1格）估算，并只保留少量余量，避免把可落脚平台误判为不可用。
			int halfWidthTiles = Math.Max(2, (int)Math.Ceiling(NPC.width / 16f * 0.5f) - 1);
			int left = tileX - halfWidthTiles;
			int right = tileX + halfWidthTiles;
			int solidSupportCount = 0;
			int topSupportCount = 0;
			for (int x = left; x <= right; x++)
			{
				Tile support = Framing.GetTileSafely(x, surfaceY);
				if (!support.HasTile)
					continue;

				bool isSolidTop = Main.tileSolidTop[support.TileType] || TileID.Sets.Platforms[support.TileType];
				bool isSolidBlock = Main.tileSolid[support.TileType] && !Main.tileSolidTop[support.TileType];
				if (isSolidBlock)
					solidSupportCount++;
				else if (isSolidTop)
					topSupportCount++;
			}

			int neededWidth = right - left + 1;
			bool hasEnoughSupport = solidSupportCount >= Math.Max(2, neededWidth / 2) || (solidSupportCount + topSupportCount) >= Math.Max(4, neededWidth - 1);
			if (!hasEnoughSupport)
				return false;

			int topY = surfaceY - JumpLandingClearanceTiles;
			for (int x = left; x <= right; x++)
			{
				for (int y = topY; y < surfaceY; y++)
				{
					if (!WorldGen.InWorld(x, y, 10))
						return false;

					Tile tile = Framing.GetTileSafely(x, y);
					if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
						return false;
				}
			}

			return true;
		}

		private Vector2 GetLandingWorldPosition(int tileX, int surfaceY)
		{
			float x = tileX * 16f + 8f - NPC.width / 2f;
			float y = surfaceY * 16f - NPC.height - 4f;
			return new Vector2(x, y);
		}

		private bool HasClearJumpArc(Vector2 start, Vector2 end)
		{
			float dx = end.X - start.X;
			float apexY = Math.Min(start.Y, end.Y) - 160f;
			int steps = Math.Max(10, (int)(Math.Abs(dx) / 20f));
			for (int i = 1; i <= steps; i++)
			{
				float t = i / (float)steps;
				float sampleX = MathHelper.Lerp(start.X, end.X, t);
				float lerpY = MathHelper.Lerp(start.Y, end.Y, t);
				float arcOffset = 4f * t * (1f - t) * (lerpY - apexY);
				float sampleY = lerpY - arcOffset;
				Vector2 sample = new Vector2(sampleX, sampleY);
				if (Collision.SolidCollision(sample - new Vector2(NPC.width / 2f, NPC.height / 2f), NPC.width, NPC.height))
					return false;
			}

			return true;
		}

        private void Mode2ShootFireballs(Player player)
        {
			int modetimermax = (float)NPC.life / (float)NPC.lifeMax > 0.5f ? 60 : 90;
			NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.1f);
			int shootnum = Main.masterMode ? Main.rand.Next(6, 10) : Main.expertMode ? Main.rand.Next(4, 7) : Main.rand.Next(3, 6);
            if (_modeTimer == 20)
            {
                for (int i = 0; i < shootnum; i++)
                {
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(55f * NPC.direction, -35f), Vector2.Zero, ModContent.ProjectileType<PmpFireBall>(), 30, 6f, Main.myPlayer);
                }
                SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
            }

			if ((Main.expertMode || Main.masterMode) && _modeTimer == 40) {
				for (int i = 0; i < shootnum; i++) {
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(55f * NPC.direction, -35f), Vector2.Zero, ModContent.ProjectileType<PmpFireBall>(), 40, 6f, Main.myPlayer);
				}
				SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
			}

			if (modetimermax == 90 && _modeTimer == 60) {
				for (int i = 0; i < shootnum; i++) {
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(55f * NPC.direction, -35f), Vector2.Zero, ModContent.ProjectileType<PmpFireBall>(), 40, 6f, Main.myPlayer);
				}
				SoundEngine.PlaySound(SoundID.Item20, NPC.Center);
			}

            if (++_modeTimer >= modetimermax)
            {
				SelectNextAttackState(AIState.Mode2ShootFireballs);
            }
        }

        private void Mode3Idle()
        {
			Player Player = Main.player[Main.myPlayer];
			//float timecount = Main.masterMode ? 2 : Main.expertMode ? 2.5f : 3;
			float timecount = 3;

			if (_modeTimer == 60) {
				for (int i = 0; i < 100; i++) {
					Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch);
					dust.velocity = Main.rand.NextVector2Circular(20f, 20f);
					dust.scale = 2.5f;
					dust.noGravity = true;
				}
				SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PmpExplode>(), 50, 0, Main.myPlayer, 0);
			}

			if (_modeTimer >= 90 && _modeTimer <= 105) {
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(-25f * NPC.direction, -70f), Vector2.Zero, ModContent.ProjectileType<PmpFireRain>(), 0, 0, Main.myPlayer, 0);
			}

			if (_modeTimer == 90) {
				SoundEngine.PlaySound(SoundID.Item11, NPC.Center);
			}

			if (++_modeTimer >= 60 * timecount)
            {
                _currentState = AIState.Mode4RainFire;
                _modeTimer = 0;
                _fireRainCounter = 0;
                _frameResetFlag = 2;
            }
        }

        private void Mode4RainFire(Player player)
        {
			int skilltime = Main.masterMode ? 60 : Main.expertMode ? 75 : 90;
			int modetime = Main.masterMode ? 180 : Main.expertMode ? 225 : 270;
			if (_modeTimer % 60 == 0 && _fireRainCounter < 3)
            {
                int fireballCount = Main.masterMode ? Main.rand.Next(8, 13) : Main.expertMode ? Main.rand.Next(6, 10) : Main.rand.Next(4, 7);
                for (int i = 0; i < fireballCount; i++)
                {
                    Vector2 spawnPos = new Vector2(player.Center.X + Main.rand.NextFloat(-600, 600), player.Center.Y + Main.rand.NextFloat(-120, 120) - 600);
					Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, Vector2.Zero, ModContent.ProjectileType<PmpFireRain>(), 35, 6f, Main.myPlayer, 1);
                }
                _fireRainCounter++;
                SoundEngine.PlaySound(SoundID.Item34, NPC.Center);
            }

            if (_fireRainCounter >= 3 && _modeTimer > 180)
            {
				SelectNextAttackState(AIState.Mode4RainFire);
            }
            else _modeTimer++;
        }

        private void Mode5Idle()
        {
			float timecount = Main.masterMode ? 1.5f : Main.expertMode ? 2 : 2.5f;

			if (_modeTimer == 15 && ispmpstg2) {
				for (int i = 0; i < 100; i++) {
					Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch);
					dust.velocity = Main.rand.NextVector2Circular(20f, 20f);
					dust.scale = 2.5f;
					dust.noGravity = true;
				}
				SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PmpExplode>(), 50, 0, Main.myPlayer, 0);
			}

			if (++_modeTimer >= 60 * timecount)
            {
				SelectNextAttackState(AIState.Mode5Idle);
            }
        }


		private void ModeJumpToPlayer(Player player)
		{
			_modeTimer++;

			if (_modeTimer == 1)
			{
				_jumpPrepCompleted = false;
				_isTeleportJumping = false;
				NPC.noTileCollide = false;
				_platformLockActive = false;
				NPC.netUpdate = true;
			}

			if (!_jumpPrepCompleted)
			{
				// 冲刺前摇：原地蓄力并持续喷发火焰粒子
				NPC.velocity *= 0.8f;
				for (int i = 0; i < 6; i++)
				{
					Dust prep = Dust.NewDustDirect(NPC.position + Main.rand.NextVector2Circular(NPC.width / 2f, NPC.height / 2f), 8, 8, DustID.Torch);
					prep.velocity = Main.rand.NextVector2Circular(2.8f, 2.8f);
					prep.scale = Main.rand.NextFloat(1.2f, 1.9f);
					prep.noGravity = true;
				}

				// 必须先触发原地方位火焰爆发，再允许开始冲刺
				if (_modeTimer == 15)
				{
					for (int i = 0; i < 96; i++)
					{
						float angle = MathHelper.TwoPi * i / 96f;
						Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4.5f, 9.5f);
						Dust burst = Dust.NewDustDirect(NPC.Center, 2, 2, i % 3 == 0 ? DustID.FlameBurst : DustID.Torch, burstVel.X, burstVel.Y, 80, default, Main.rand.NextFloat(1.2f, 2.3f));
						burst.noGravity = true;
					}
					SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
					if (Main.netMode != NetmodeID.MultiplayerClient)
					{
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PmpExplode>(), 50, 0, Main.myPlayer, 0);
					}
					_jumpPrepCompleted = true;
				}

				return;
			}

			// 爆发后短暂停顿，再起跳冲刺
			if (!_isTeleportJumping && _modeTimer >= 32)
			{
				BeginParabolicJump(player);
			}

			if (_isTeleportJumping)
			{
				_jumpTimer++;
				NPC.velocity.Y += _jumpGravity;
				NPC.velocity.X *= 0.994f; // 轻微减速，给玩家闪避窗口

				if (NPC.noTileCollide && _jumpTimer > 8)
				{
					// 进入下落且接近目标层时尽快恢复碰撞，避免整段跳跃都穿墙导致错过平台。
					float targetBottomY = _jumpTargetPos.Y + NPC.height;
					bool descending = NPC.velocity.Y > 0.55f;
					bool nearTargetLevel = NPC.Bottom.Y >= targetBottomY - 22f;
					bool hasSupportBelow = Collision.SolidCollision(NPC.position + new Vector2(0f, 8f), NPC.width, NPC.height);
					bool timeoutForceRecover = _jumpTimer >= 50;
					if ((descending && nearTargetLevel) || hasSupportBelow || timeoutForceRecover)
					{
						NPC.noTileCollide = false;
					}
				}

				SpawnJumpTrailDust(_jumpTrailLastPos, NPC.Center);
				_jumpTrailLastPos = NPC.Center;

				float dxToTarget = _jumpTargetPos.X - NPC.position.X;
				float dyToTarget = _jumpTargetPos.Y - NPC.position.Y;
				bool closeEnoughToLand = Math.Abs(dxToTarget) < 28f && Math.Abs(dyToTarget) < 18f;
				bool touchingGround = Collision.SolidCollision(NPC.position + new Vector2(0f, 8f), NPC.width, NPC.height);
				bool passedTargetHeight = NPC.position.Y >= _jumpTargetPos.Y - 4f;
				bool hasDugIntoGround = touchingGround && NPC.velocity.Y >= 0f;
				if ((_jumpTimer > 30 && closeEnoughToLand && touchingGround) || (_jumpTimer > 36 && passedTargetHeight && hasDugIntoGround) || _jumpTimer >= 140)
				{
					if (touchingGround || hasDugIntoGround)
					{
						for (int i = 0; i < 18 && Collision.SolidCollision(NPC.position, NPC.width, NPC.height); i++)
						{
							NPC.position.Y -= 3f;
						}
					}

					TriggerJumpLandingExplosion();
					NPC.velocity = Vector2.Zero;
					NPC.noTileCollide = false;
					StabilizeLandingAfterJump();
					_platformLockActive = true;
					_platformLockBottomY = NPC.Bottom.Y;
					_isTeleportJumping = false;
					_jumpPrepCompleted = false;
					_heightMismatchTimer = 0;
					NPC.netUpdate = true;
					SelectNextAttackState();
				}
			}
		}

		private void BeginParabolicJump(Player player)
		{
			_platformLockActive = false;
			Vector2 desiredTarget = FindSafeJumpLandingPosition(player, out bool foundSafeLanding);
			float horizontalDistance = Math.Abs(desiredTarget.X - NPC.Center.X);
			float maxPhaseDistance = 72f;
			NPC.noTileCollide = !foundSafeLanding || horizontalDistance > maxPhaseDistance || !HasClearJumpArc(NPC.Center, desiredTarget);
			_isTeleportJumping = true;
			_jumpTimer = 0;
			_jumpTrailLastPos = NPC.Center;
			_jumpTargetPos = desiredTarget;

			int travelTime = Main.masterMode ? 72 : Main.expertMode ? 80 : 92;
			float targetTopY = _jumpTargetPos.Y + NPC.height * 0.5f;
			float climbHeight = Math.Max(0f, NPC.Center.Y - targetTopY);
			float arcLift = MathHelper.Clamp(170f + horizontalDistance * 0.15f + climbHeight * 0.55f, 170f, 340f);
			float apexY = Math.Min(NPC.Center.Y, targetTopY) - arcLift;
			_jumpGravity = MathHelper.Clamp(2f * ((NPC.Center.Y - apexY) + (targetTopY - apexY)) / (travelTime * travelTime), 0.18f, 0.30f);

			float dx = _jumpTargetPos.X - NPC.Center.X;
			float vx = Math.Clamp(dx / travelTime, -7.2f, 7.2f);
			float vy = (targetTopY - NPC.Center.Y - 0.5f * _jumpGravity * travelTime * travelTime) / travelTime;
			float minVy = -MathF.Sqrt(Math.Max(0.01f, 2f * _jumpGravity * Math.Max(36f, NPC.Center.Y - apexY)));
			vy = Math.Clamp(vy, minVy, -5.8f);

			NPC.velocity = new Vector2(vx, vy);
			SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
			NPC.netUpdate = true;
		}

		private void TriggerJumpLandingExplosion()
		{
			for (int i = 0; i < 80; i++)
			{
				float angle = MathHelper.TwoPi * i / 80f;
				Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(3.5f, 8.5f);
				Dust ring = Dust.NewDustDirect(NPC.Center, 2, 2, i % 4 == 0 ? DustID.FlameBurst : DustID.Torch, vel.X, vel.Y, 80, default, Main.rand.NextFloat(1.1f, 2.1f));
				ring.noGravity = true;
			}
			SoundEngine.PlaySound(SoundID.Item14, NPC.Center);

			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PmpExplode>(), 50, 0f, Main.myPlayer, 0);
			}
		}

		private void StabilizeLandingAfterJump()
		{
			for (int i = 0; i < 12 && Collision.SolidCollision(NPC.position, NPC.width, NPC.height); i++)
			{
				NPC.position.Y -= 2f;
			}

			bool onSolidGround = Collision.SolidCollision(NPC.position + new Vector2(0f, 8f), NPC.width, NPC.height);
			if (!onSolidGround)
			{
				for (int i = 0; i < 8 && !Collision.SolidCollision(NPC.position + new Vector2(0f, 8f), NPC.width, NPC.height); i++)
				{
					NPC.position.Y += 1f;
				}
			}

			int footTileX = (int)(NPC.Center.X / 16f);
			int footTileY = (int)(NPC.Bottom.Y / 16f);
			if (!Collision.SolidCollision(NPC.position + new Vector2(0f, 8f), NPC.width, NPC.height) && TryFindLandingSurface(footTileX, footTileY, out int surfaceY))
			{
				Tile support = Framing.GetTileSafely(footTileX, surfaceY);
				if (support.HasTile && (Main.tileSolidTop[support.TileType] || TileID.Sets.Platforms[support.TileType]))
				{
					NPC.position.Y = surfaceY * 16f - NPC.height - 2f;
				}
			}

			NPC.velocity = Vector2.Zero;
		}

		private void SpawnJumpTrailDust(Vector2 from, Vector2 to)
		{
			float dist = Vector2.Distance(from, to);
			int steps = Math.Max(2, (int)(dist / 12f));
			for (int s = 0; s <= steps; s++)
			{
				Vector2 p = Vector2.Lerp(from, to, s / (float)steps);
				Dust flame = Dust.NewDustDirect(p, 2, 2, DustID.FlameBurst);
				flame.velocity = Main.rand.NextVector2Circular(1.4f, 1.4f) - NPC.velocity * 0.12f;
				flame.scale = Main.rand.NextFloat(1.0f, 1.7f);
				flame.noGravity = true;

				if (Main.rand.NextBool(2))
				{
					Dust ember = Dust.NewDustDirect(p, 2, 2, DustID.Torch);
					ember.velocity = Main.rand.NextVector2Circular(1.8f, 1.8f) - NPC.velocity * 0.08f;
					ember.scale = Main.rand.NextFloat(0.9f, 1.4f);
					ember.noGravity = true;
				}
			}
		}
		private void SelectNextAttackState(AIState? previousState = null)
		{
			bool lowHp = (float)NPC.life / NPC.lifeMax <= 0.5f;
			Player selTarget = Main.player[NPC.target];
			bool sameLevel = Math.Abs(selTarget.Center.Y - NPC.Center.Y) < 180f;
			int roll = Main.rand.Next(10);
			// 同层时：优先爬行追近+攻击，权重根据血量调整
			// 不同层时：降低爬行权重，更多释放远程技能等待穿墙
			AIState nextState;
			if (sameLevel)
			{
				nextState = (lowHp ? roll switch {
					<= 8 => AIState.Mode1Crawl,
					<= 9 => AIState.Mode2ShootFireballs,
					_ => AIState.Mode3Idle,
				} : roll switch {
					<= 6 => AIState.Mode1Crawl,
					<= 8 => AIState.Mode2ShootFireballs,
					_ => AIState.Mode3Idle,
				});
			}
			else
			{
				// 不同层：减少爬行，多放技能（天女散花能打不同层的玩家）
				nextState = roll switch {
					<= 2 => AIState.Mode1Crawl,
					<= 6 => AIState.Mode2ShootFireballs,
					_ => AIState.Mode3Idle,
				};
			}

			if (previousState.HasValue && nextState == previousState.Value)
			{
				nextState = nextState switch
				{
					AIState.Mode1Crawl => Main.rand.NextBool() ? AIState.Mode2ShootFireballs : AIState.Mode3Idle,
					AIState.Mode2ShootFireballs => Main.rand.NextBool(3) ? AIState.Mode1Crawl : AIState.Mode3Idle,
					AIState.Mode3Idle => Main.rand.NextBool(3) ? AIState.Mode1Crawl : AIState.Mode2ShootFireballs,
					_ => AIState.Mode1Crawl,
				};
			}

			_currentState = nextState;
			_modeTimer = 0;
			_fireRainCounter = 0;
			_frameResetFlag = 2;
			NPC.netUpdate = true;

			if (_currentState == AIState.Mode1Crawl && _slugEggCooldown <= 0)
			{
				_slugEggCooldown = Main.rand.Next(45, 91);
			}
		}

        private void InitTailKill()
        {
			_platformLockActive = false;
            _hasEnteredTailKill = true;
            _currentState = AIState.TailKillRise;
            _tailKillDuration = 0;
            _tailKillStartPos = NPC.Center;
            NPC.life = 1;
            NPC.dontTakeDamage = true;
            NPC.velocity = Vector2.Zero;
            _frameResetFlag = 2;

			// 死亡尾杀时清除场上所有庞贝火焰弹幕，避免死后继续发射
			if (Main.netMode != NetmodeID.MultiplayerClient)
			{
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile p = Main.projectile[i];
					if (p.active && p.hostile && (
						p.type == ModContent.ProjectileType<PmpFireBall>() ||
						p.type == ModContent.ProjectileType<PmpSlugEgg>()))
					{
						p.Kill();
					}
				}
			}
        }

        private void HandleTailKillRise()
        {
            _tailKillDuration++;
            NPC.position.Y -= 40f / 60f;

			if (_tailKillDuration >= 90 && _tailKillDuration % 48 == 0) {
				for (int i = 0; i <= Main.rand.Next(3,7); i++) {
					Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(-25f * NPC.direction, -70f), Vector2.Zero, ModContent.ProjectileType<PmpFireRain>(), 0, 0, Main.myPlayer, 0);
				}
				SoundEngine.PlaySound(SoundID.Item11, NPC.Center);
			}

			if (_tailKillDuration >= 60 * 4)
            {
                _currentState = AIState.TailKillSpin;
                _tailKillDuration = 0;
            }
        }

        private void HandleTailKillSpin(Player player)
        {
            _tailKillDuration++;
			int shootdelta = Main.masterMode ? 48 : Main.expertMode ? 60 : 75;
			int shootnum = Main.masterMode ? Main.rand.Next(6, 10) : Main.expertMode ? Main.rand.Next(4, 7) : Main.rand.Next(3, 6);
			int shoottime = Main.masterMode ? 32 : Main.expertMode ? 28 : 24;
			if (_tailKillDuration % shootdelta == 0)
            {
				if (_tailKillDuration <= (shoottime - 1) * shootdelta) {
					for (int j = 0; j <= Main.rand.Next(3, 7); j++) {
						Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + new Vector2(-25f * NPC.direction, -70f), Vector2.Zero, ModContent.ProjectileType<PmpFireRain>(), 0, 0, Main.myPlayer, 0);
					}
				}

				for (int i = 0; i < shootnum; i++)
                {
					//Vector2 spawnPos = new Vector2(NPC.Center.X + Main.rand.NextFloat(-600, 600), NPC.Center.Y + Main.rand.NextFloat(-60, 60) - 540);
					Vector2 spawnPos = new Vector2(player.Center.X + Main.rand.NextFloat(-600, 600), player.Center.Y + Main.rand.NextFloat(-120, 120) - 720);
					Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, Vector2.Zero, ModContent.ProjectileType<PmpFireRain>(), 50, 6f, Main.myPlayer, 1);
				}
                SoundEngine.PlaySound(SoundID.Item11, NPC.Center);
            }

            if (_tailKillDuration >= shoottime * shootdelta)
            {
                _currentState = AIState.TailKillDeath;
                _tailKillDuration = 0;
                _frameResetFlag = 2;
            }
        }

		private void HandleTailKillDeath()
        {
			Player Player = Main.player[Main.myPlayer];
			_tailKillDuration++;

			if (_tailKillDuration == 45) {
				Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<PmpExplode>(), 0, 0, -1);
			}

			if (_tailKillDuration == 60)
            {
                for (int i = 0; i < 100; i++)
                {
                    Dust dust = Dust.NewDustDirect(NPC.position,NPC.width,NPC.height,DustID.Torch);
                    dust.velocity = Main.rand.NextVector2Circular(20f, 20f);
                    dust.scale = 2.5f;
                    dust.noGravity = true;
                }
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);

				if ((NPC.Center - Player.Center).Length() <= 240f) {
					Player.KillMe(PlayerDeathReason.ByCustomReason(NetworkText.FromKey("Mods.ArknightsMod.StatusMessage.Pmp.GetTooClose", Player.name)), 1018, 0);
				}
			}

            if (_tailKillDuration >= 75)
            {
				if (!_tailKillDebrisSpawned)
				{
					_tailKillDebrisSpawned = true;
					int frameRow = Math.Clamp(_currentFrame, 0, Main.npcFrameCount[Type] - 1);
					NPCDebrisSystem.TrySpawnDynamicDebris(NPC, isBoss: true, frameRowIndex: frameRow);
				}
				NPC.life = 0;
                NPC.checkDead();
            }
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.Common(ItemType<OrirockCube>(), 1, LootOrirockCubeCount, LootOrirockCubeCount));
			npcLoot.Add(ItemDropRule.Common(ItemType<Polyester>(), 1, LootPolyesterCount, LootPolyesterCount));
			// 庞贝掉落：4 源石锭 & 200 合成玉 & 庞贝的熔岩核心（饰品，必掉）
			npcLoot.Add(ItemDropRule.Common(ItemType<global::ArknightsMod.Content.Items.OriginiumIngot>(), 1, 4, 4));
			npcLoot.Add(ItemDropRule.Common(ItemType<global::ArknightsMod.Content.Items.Orundum>(), 1, 200, 200));
			npcLoot.Add(ItemDropRule.Common(ItemType<global::ArknightsMod.Content.Items.Accessories.Boss.PompeiiLavaCore>(), 1, 1, 1));
		}
    }

	//火球
	public class PmpFireBall : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 15;
		}

		public override void SetDefaults() {
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 600;
			Projectile.alpha = 0;
			Projectile.damage = 30;
			//Projectile.light = 1f;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.scale = 0f;
		}

		private float timer;
		private int r = 255;
		private int g = 50;
		private int b = 0;
		private Vector2 spawnPos;
		private float targethittime;//预计撞击时间
		private float randomay; //随机纵向加速度

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.buffImmune[BuffID.OnFire] = false;
			if (!target.HasBuff(BuffID.OnFire)) {
				target.AddBuff(BuffID.OnFire, Main.masterMode ? 180 : Main.expertMode ? 180 : 270);
			}
		}

		public override void OnSpawn(IEntitySource source) {
			spawnPos = Projectile.Center;
			targethittime = Main.masterMode ? Main.rand.NextFloat(48, 72) : Main.expertMode ? Main.rand.NextFloat(60, 90) : Main.rand.NextFloat(75, 105);
			randomay = Main.rand.NextFloat(0.1f, 0.5f);
			g = 50 + Main.rand.Next(0, 81);
		}

		public override void AI() {
			Player Player = Main.player[Main.myPlayer];
			Lighting.AddLight(Projectile.Center, 10);
			Projectile.rotation = Projectile.velocity.ToRotation() + float.Pi / 2;
			Vector2 deltaPos = Player.Center - spawnPos;
			Dust dust = Main.dust[Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 0, new Color(255, 255, 255), 2f)];
			dust.noGravity = true;
			if (timer <= 30f) {
				Projectile.Opacity = timer / 30f;
				Projectile.scale = timer / 30f;
			}
			float targetvx = deltaPos.X / targethittime + Main.rand.NextFloat(-0.25f, 0.25f) * Player.velocity.X;
			float targetvy = -randomay * targethittime / 2f + randomay * timer;
			Projectile.velocity = new Vector2(targetvx, targetvy);

			timer++;
		}

		public override void PostDraw(Color lightColor) {
			Texture2D text = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/OF/Pmp/PmpFireBall").Value;
			Main.EntitySpriteDraw(text, Projectile.Center - Main.screenPosition - Projectile.velocity * 0.5f, new Rectangle(0, 0, text.Width, text.Height), new Color(r, g, b) * Projectile.Opacity, Projectile.rotation, new Vector2(text.Width / 2, text.Height / 2), 0.4f * Projectile.scale, SpriteEffects.None, 0);
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, Vector2.Zero, new Color(r, g, b), new Color(0, 0, 0), 10f, true);
			return true;
		}

		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center); //炸弹
			for (int i = 0; i < 3; i++) {
				Main.dust[Dust.NewDust(Projectile.position - new Vector2(10, 10), Projectile.width + 20, Projectile.height + 20, DustID.Pixie, Scale: 1.5f)].noGravity = true;
			}
			for (int j = 0; j < 6; j++) {
				Main.dust[Dust.NewDust(Projectile.position - new Vector2(10, 10), Projectile.width + 20, Projectile.height + 20, DustID.Torch, Scale: 2.5f)].noGravity = true;
			}
		}
	}

	//火雨
	public class PmpFireRain : ModProjectile
	{

		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults() {
			//ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 14;
		}

		public override void SetDefaults() {
			Projectile.width = 30;
			Projectile.height = 30;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 600;
			Projectile.alpha = 0;
			Projectile.damage = 30;
			//Projectile.light = 1f;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.scale = 0f;
		}

		private float timer;
		private int r = 255;
		private int g = 50;
		private int b = 0;
		private Vector2 spawnPos;
		private float vx;
		private float vy;
		private float shakeOpacity = 1;
		private float shakeT = Main.rand.NextFloat(3, 10);

		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.buffImmune[BuffID.OnFire] = false;
			if (!target.HasBuff(BuffID.OnFire)) {
				target.AddBuff(BuffID.OnFire, Main.masterMode ? 180 : Main.expertMode ? 180 : 270);
			}
		}

		public override void OnSpawn(IEntitySource source) {
			spawnPos = Projectile.Center;
			vx = Main.rand.NextFloat(-1, 1);
			vy = Main.masterMode ? Main.rand.NextFloat(10.8f, 12f) : Main.expertMode ? Main.rand.NextFloat(9.6f, 11.2f) : Main.rand.NextFloat(8.4f, 10.8f);
			g = 50 + Main.rand.Next(0, 41);
		}

		//存储历史轨迹（方括号内数目比绘制数量大一个deltaT）
		private Vector2[] beforePos = new Vector2[16];

		//修改历史轨迹
		public override void PostAI() {
			//更新缓存数组 （从最大索引开始倒序更新）
			for (int n = 15; n > 0; n--) {
				beforePos[n] = beforePos[n - 1];
			}
			beforePos[0] = Projectile.position + 3 * Projectile.velocity; //按需调整
			//曲线部分（无偏移）曲线部分点数
			for (int i = 0; i < 7; i++) {
				Projectile.oldPos[i] = beforePos[2 * i + (int)timer % 2]; //deltaT
			}
			//折线部分（有偏移）模数为deltaT | 存储轨迹最大索引 | 折线绘制上限索引 | 折线绘制起始索引+1
			if (timer % 2 == 0 && beforePos[15] != Vector2.Zero) {
				//更新
				for (int n = 13; n > 7; n--) {
					Projectile.oldPos[n] = Projectile.oldPos[n - 1];
				}
				Projectile.oldPos[7] = beforePos[15] + new Vector2(0, Main.rand.NextFloat(-24, 24)).RotatedBy(Projectile.velocity.ToRotation()); //第一个索引的引入计算，偏移量垂直于当前速度方向（懒得写旧的速度方向了）
			}
		}

		public override void AI() {
			Lighting.AddLight(Projectile.Center, 10);
			Projectile.rotation = Projectile.velocity.ToRotation() + float.Pi / 2;
			Dust dust;
			Vector2 position = Projectile.Center;
			dust = Main.dust[Dust.NewDust(Projectile.Center + new Vector2(-4, 0), Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 0, new Color(255, 255, 255), 1.5f)];
			dust.noGravity = true;

			if (timer <= 30f) {
				Projectile.Opacity = timer / 30f;
				Projectile.scale = timer / 30f;
			}
			else {
				shakeOpacity = 0.4f * MathF.Cos(float.Pi * timer / shakeT) + 0.6f;
			}

			Projectile.velocity = Projectile.ai[0] == 1 ? new Vector2(0, vy) : new Vector2(vx, -vy);

			timer++;

			if (timer > 60f) {
				Projectile.tileCollide = true;
			}
		}

		public override void PostDraw(Color lightColor) {
			Texture2D text = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/OF/Pmp/PmpFireStar").Value;
			Main.EntitySpriteDraw(text, Projectile.Center - Main.screenPosition - Projectile.velocity * 0.5f, new Rectangle(0, 0, text.Width, text.Height), new Color(r, g, b) * Projectile.Opacity * shakeOpacity, Projectile.rotation, new Vector2(text.Width / 2, text.Height / 2), 1f * new Vector2(0.25f, 1f), SpriteEffects.None, 0);
			Main.EntitySpriteDraw(text, Projectile.Center - Main.screenPosition - Projectile.velocity * 0.5f, new Rectangle(0, 0, text.Width, text.Height), new Color(r, g, b) * Projectile.Opacity * shakeOpacity, Projectile.rotation + float.Pi / 2, new Vector2(text.Width / 2, text.Height / 2), 1f * new Vector2(0.25f, 1f), SpriteEffects.None, 0);
			Main.EntitySpriteDraw(text, Projectile.Center - Main.screenPosition - Projectile.velocity * 0.5f, new Rectangle(0, 0, text.Width, text.Height), new Color(r, g + 120, 150) * Projectile.Opacity * shakeOpacity, Projectile.rotation, new Vector2(text.Width / 2, text.Height / 2), 0.6f * new Vector2(0.25f, 1f), SpriteEffects.None, 0);
			Main.EntitySpriteDraw(text, Projectile.Center - Main.screenPosition - Projectile.velocity * 0.5f, new Rectangle(0, 0, text.Width, text.Height), new Color(r, g + 120, 150) * Projectile.Opacity * shakeOpacity, Projectile.rotation + float.Pi / 2, new Vector2(text.Width / 2, text.Height / 2), 0.6f * new Vector2(0.25f, 1f), SpriteEffects.None, 0);
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, Vector2.Zero, new Color(r, g, b) * shakeOpacity, new Color(0, 0, 0), 10f, true);
			return true;
		}

		public override void OnKill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item14, Projectile.Center); //炸弹
			for (int i = 0; i < 3; i++) {
				Main.dust[Dust.NewDust(Projectile.position - new Vector2(10, 10), Projectile.width + 20, Projectile.height + 20, DustID.Pixie, Scale: 1.5f)].noGravity = true;
			}
			for (int j = 0; j < 6; j++) {
				Main.dust[Dust.NewDust(Projectile.position - new Vector2(10, 10), Projectile.width + 20, Projectile.height + 20, DustID.Torch, Scale: 2.5f)].noGravity = true;
			}
		}
	}

	//爆炸
	public class PmpExplode : ModProjectile
	{

		public override string Texture => ArknightsMod.noTexture;

		public override void SetDefaults() {
			Projectile.width = 320;
			Projectile.height = 320;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 75;
			Projectile.alpha = 0;
			Projectile.damage = 50;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.scale = 1f;
		}

		private int shadertimer = 0;//充当utime
		//private int shadercheck = 0;
		private int ammount = 100;//波纹数量
		private int scalesize = 50;//波纹大小
		private int wavevelocity;//波纹速度
		private float distortStrength = 80f;//强度

		public override void AI() {
			shadertimer++;
			if (shadertimer > 75) {
				shadertimer = 75;
			}
			wavevelocity = 10;
			if (wavevelocity < 0) {
				wavevelocity = 0;
			}

			if (shadertimer <= 60) {
				if (Projectile.ai[0] == 0) {
					Projectile.ai[0] = 1;
					if (Main.netMode != NetmodeID.Server && !Terraria.Graphics.Effects.Filters.Scene["IACTSW"].IsActive()) {
						Terraria.Graphics.Effects.Filters.Scene.Activate("IACTSW", Projectile.Center).GetShader().UseColor(ammount, scalesize, wavevelocity).UseTargetPosition(Projectile.Center);
					}
				}

				if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["IACTSW"].IsActive()) {
					float progress = (shadertimer) / 60f;
					Terraria.Graphics.Effects.Filters.Scene["IACTSW"].GetShader().UseProgress(3 * progress).UseOpacity(distortStrength * (1 - progress / 1f));
				}
			}

			if (shadertimer >= 5) {
				Projectile.damage = 0;
			}
		}

		public override void OnKill(int timeLeft) {
			if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["IACTSW"].IsActive()) {
				Terraria.Graphics.Effects.Filters.Scene["IACTSW"].Deactivate();
			}
		}
	}

	public class PMPSTG2Effect : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 10;
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
			return ModContent.NPCType<Pompeii>();
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
			Lighting.AddLight(Projectile.Center, 10);
			Projectile.velocity = Vector2.Zero;
			float offset = 0.04f * MathF.Cos(timer * float.Pi / 39f) + 0.36f;
			float delta = 0.2f * MathF.Cos(timer * float.Pi / 41f) + 0.5f;
			float maxd = 10 * MathF.Cos(timer * float.Pi / 29f);
			dotX = (70 + maxd) * MathF.Sin((timer / 10 + offset + delta * Projectile.ai[1]) * float.Pi);
			dotY = (70 - maxd) * Projectile.ai[1] * (1- offset) * MathF.Cos((timer / 9 + (1 - delta) * Projectile.ai[1]) * float.Pi);
			Projectile.Center = HostNPC.Center + new Vector2(dotX, dotY + 24) + 4 * HostNPC.velocity;

			if (timer <= 60f) {
				drawopacity += 0.75f / 60f;
			}
			else if (Projectile.timeLeft <= 60f) {
				drawopacity -= 0.75f / 60f;
			}
			else {
				drawopacity = 0.75f;
			}

			r = 235 + 20 * (int)MathF.Sin(timer * MathHelper.Pi / 180);
			g = 20 + 20 * (int)MathF.Sin(timer * MathHelper.Pi / 180);

			if (!HostNPC.active) {
				Projectile.Kill();
			}
			if (Projectile.timeLeft > 60) {
				Projectile.timeLeft = (float)HostNPC.life / (float)HostNPC.lifeMax < 0.5f ? 1800 : 60;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, Vector2.Zero, new Color(r, g, 0) * drawopacity, new Color(0, 0, 0), 15f, true);
			return true;
		}
	}
}