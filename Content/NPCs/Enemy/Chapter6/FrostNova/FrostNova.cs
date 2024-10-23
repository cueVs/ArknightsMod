using System;
using System.Collections.Generic;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ArknightsMod.Content.BossBars;
using ArknightsMod.Content.Items.Material;
using ArknightsMod.Common.VisualEffects;
using ArknightsMod.Content.NPCs.Enemy.RoaringFlare.ImperialArtilleyCoreTargeteer;


namespace ArknightsMod.Content.NPCs.Enemy.Chapter6.FrostNova
{
	[AutoloadBossHead]
	public class FrostNova : ModNPC {
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
			NPC.lifeMax = 200;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.DoubleJump;
			NPC.knockBackResist = 0f;
			NPC.lavaImmune = true;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			NPC.value = Item.buyPrice(gold: 5);
			NPC.SpawnWithHigherTime(30);
			NPC.boss = true;
			NPC.npcSlots = 10f;
			NPC.aiStyle = -1;
			NPC.BossBar = ModContent.GetInstance<FNBossBar>();
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
		}
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
			Main.EntitySpriteDraw(BarTexBot, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 + (Main.screenHeight / 2 - 108) / Main.GameZoomTarget), new Rectangle(0, 0, (int)(BarTexBot.Width), BarTexBot.Height), Color.White * (Bartimer / 180), 0, new Vector2(BarTexBot.Width / 2, BarTexBot.Height / 2), 1 / Main.GameZoomTarget, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(BarTexMed2, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 + (Main.screenHeight / 2 - 108) / Main.GameZoomTarget), new Rectangle(0, 0, (int)(ActualHealthBarLength * 4), BarTexMed.Height), Color.White * (Bartimer / 120), 0, new Vector2(BarTexMed.Width / 2, BarTexMed.Height / 2), 1 / Main.GameZoomTarget, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(BarTexMed, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 + (Main.screenHeight / 2 - 108) / Main.GameZoomTarget), new Rectangle(0, 0, (int)(TargetHealthBarLength), BarTexMed.Height), Color.White * (Bartimer / 120), 0, new Vector2(BarTexMed.Width / 2, BarTexMed.Height / 2), 1 / Main.GameZoomTarget, SpriteEffects.None, 0);
			Main.EntitySpriteDraw(BarTexTop, new Vector2(Main.screenWidth / 2, Main.screenHeight / 2 + (Main.screenHeight / 2 - 108) / Main.GameZoomTarget), new Rectangle(0, 0, (int)(BarTexTop.Width), BarTexTop.Height), Color.White * (Bartimer / 30), 0, new Vector2(BarTexTop.Width / 2, BarTexTop.Height / 2), 1 / Main.GameZoomTarget, SpriteEffects.None, 0);
		}

		//public override void BossLoot(ref string name, ref int potionType) {
		//}

		//public override void HitEffect(NPC.HitInfo hit) {
		//}

		//public override bool CanHitPlayer(Player target, ref int cooldownSlot) {
		//	return null;
		//}

		private float BirthTimer;
		private float ReBirthTimer;
		private float escapetimer;
		private float DeathTimer;
		private float JumpTimer = 0;
		private float StateTimer;
		private float FNStage;

		private float FNShaderRingTimer;
		private float FNRevivalShaderRingTimer;
		private float FNShaderRingIntensity;
		private float FNShaderRingOpacity;
		private float FNShaderRingProgress;
		private float FNShaderRingColorR;
		private float FNShaderRingColorG;
		private float FNShaderRingColorB;
		private float FNTPOpacity = 1;

		private bool FNBirth = false;
		private bool FNBirthEnd = false;
		private bool FNRevival = false;
		private bool FNRevivalStart = false;
		private bool FNRevivalEnd = false;
		private bool FNEnd = false;
		private bool FNDeathStart = false;

		private bool isstuck = false;
		private int jumpstage;
		private int jumptimes;
		private float statplrposX;
		private float statplrposY;

		public override void AI() {
			Player player = Main.player[NPC.target];

			//霜星的具体阶段比较复杂，有两个标识种类，阶段表示法和状态表示法
			//生成时候是0阶段，FNBirth和FNBirthEnd不生效，然后FNBirth的判定是生成动画播完，FNBirthEnd的判定是机制出完（BirthTimer 达到4秒），生成结束后进入1阶段
			//一阶段没血后锁血进入1.5阶段，同时播放复活动画，FNRevivalStart启用，复活回满血持续10秒，这之间动画播完后结束后FNRevivalEnd生效，回到走路动画
			//复活阶段结束后FNRevival启用，FNRevivalStart和FNRevivalEnd被禁用，进入2阶段
			//二阶段起始前四秒（ReBirthTimer达到四秒）会召唤龙卷风，四秒后进入正常AI，n秒后解除无敌
			//二阶段没血后锁血进入2.5阶段（还没写），后面还没写

			#region 音乐
			if (!Main.dedServ) {
				if (FNStage <= 1) {
					Music = MusicLoader.GetMusicSlot(Mod, "Music/FrostnovaStage1");//一阶段
				}
				else if (FNStage == 1.5f || FNStage == 2) {
					Music = MusicLoader.GetMusicSlot(Mod, "Music/FrostnovaStage2");//二阶段
				}
				else if (FNStage >= 3) {
					Music = MusicLoader.GetMusicSlot(Mod, "Music/FrostnovaDeath");//死亡
				}
			}
			#endregion

			#region 移动方式
			if (!FNRevivalStart && FNBirthEnd && FNStage != 3) {
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
						jumptimes = 0;
					}
				}
				else {//没有撞墙
					isstuck = false;
					jumptimes = 0;
					jumpstage = 0;
					if (player.Center.X > haltPointX) {
						NPC.velocity.X += ax;
					}
					else {
						NPC.velocity.X -= ax;
					}
					NPC.velocity.X = Math.Min(vx, Math.Max(-vx, NPC.velocity.X));
				}
			}
			#endregion

			#region 限制阈效果,progress为半径，intensity为不透明度，time为形变，opacity为粗细
			if (FNStage == 1) {
				FNShaderRingTimer++;
				//半径、颜色等与阶段有关
				FNShaderRingProgress = Math.Min(FNShaderRingTimer / 60, 0.75f);
				//浅蓝白色
				FNShaderRingColorR = 200 / 255;
				FNShaderRingColorG = (17.5f * (float)Math.Sin(FNShaderRingTimer * Math.PI / 180) + 237.5f) / 255;
				FNShaderRingColorB = 1;
				//不透明度
				if (FNShaderRingTimer <= 30) {
					FNShaderRingIntensity = Math.Min(FNShaderRingTimer / 30, 1);
				}
				else {
					FNShaderRingIntensity = 0.125f * (float)Math.Sin(FNShaderRingTimer * Math.PI / 240) + 0.875f;
				}
			}
			else if (FNStage == 1.5f) {
				//透明化
				FNShaderRingIntensity -= 0.01667f;
				if (FNShaderRingIntensity <= 0) {
					FNShaderRingIntensity = 0;
				}
				//颜色锁定
				FNShaderRingColorR = 200 / 255;
				FNShaderRingColorG = (17.5f * (float)Math.Sin(FNShaderRingTimer * Math.PI / 180) + 237.5f) / 255;
				FNShaderRingColorB = 1;
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
			}
			if (!FNDeathStart) {
				//粗细只和是否死亡有关，与阶段无关
				FNShaderRingOpacity = Math.Min(FNShaderRingTimer / 60, 1);
				//形变和其他因素无关
				if (Main.netMode != NetmodeID.Server && !Terraria.Graphics.Effects.Filters.Scene["FNTwistedRing"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene.Activate("FNTwistedRing", NPC.Center).GetShader().UseColor(FNShaderRingColorR, FNShaderRingColorG, FNShaderRingColorB).UseTargetPosition(NPC.Center + new Vector2(0, 3)).UseIntensity(FNShaderRingIntensity * FNTPOpacity).UseOpacity(FNShaderRingOpacity).UseProgress(FNShaderRingProgress * FNTPOpacity);
					ArknightsMod.FNTwistedRing.Parameters["uTime"].SetValue((float)Main.time / 64);
					SoundEngine.PlaySound(SoundID.NPCHit5);
				}
				if (Main.netMode != NetmodeID.Server && Terraria.Graphics.Effects.Filters.Scene["FNTwistedRing"].IsActive()) {
					Terraria.Graphics.Effects.Filters.Scene.Activate("FNTwistedRing", NPC.Center).GetShader().UseColor(FNShaderRingColorR, FNShaderRingColorG, FNShaderRingColorB).UseTargetPosition(NPC.Center + new Vector2(0, 3)).UseIntensity(FNShaderRingIntensity * FNTPOpacity).UseOpacity(FNShaderRingOpacity).UseProgress(FNShaderRingProgress * FNTPOpacity);
					ArknightsMod.FNTwistedRing.Parameters["uTime"].SetValue((float)Main.time / 64);
				}
			}
			else if (FNDeathStart || escapetimer != 0) {
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
			if (!player.active || player.dead) {
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
				return;
			}
			#endregion

			# region 生成（0阶段）
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
					//NPC.lifeMax = 200;
				}
			}
			#endregion

			# region 复活（1.5阶段）
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
					}
				}
			}
			#endregion

			#region 复活后召唤龙卷风
			if (FNStage == 2f) {
				ReBirthTimer++;
				if (ReBirthTimer < 300) {
					NPC.dontTakeDamage = true;
					NPC.velocity.X = 0;
					if (ReBirthTimer == 30) {
						for (int i = -1; i < 3; i +=2 ) {
							Projectile.NewProjectile(null, NPC.Center - new Vector2(0, NPC.height / 1.5f), Vector2.Zero, ProjectileType<BlizzardStormStarter>(), 0, 0f, -1, i);
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
					//FNStage = 2;
					NPC.damage = 24;
					NPC.dontTakeDamage = false;
					//NPC.lifeMax = 200;
				}
			}
			#endregion

			#region 死亡
			if (FNDeathStart == true) {
				DeathTimer++;
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
					FNDeathStart = true;
					FNEnd = true;
					NPC.life = 0;
					NPC.checkDead();
				}
			}
			#endregion
		}

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

		//帧动画，霜星一共76帧，1~5为走路，6~16为普攻，17~34为坠冰技能，35~55为重生（35~39为死亡），56~76为吟唱
		public override void FindFrame(int frameHeight) {
			int startFrame;
			int finalFrame;
			int frameSpeed;
			NPC.spriteDirection = NPC.direction;
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
			//走路
			if (FNStage == 1 || (FNStage == 2 && ReBirthTimer > 300) || FNRevivalEnd) {
				startFrame = 0;
				finalFrame = 4;
				frameSpeed = 5;
				//if (State == ActionState.FastWalk) {
				//	frameSpeed = 4;
				//}
				NPC.frameCounter += 0.5f;
				if (NPC.frameCounter > frameSpeed) {
					NPC.frameCounter = 0;
					NPC.frame.Y += frameHeight;

					if (NPC.frame.Y > finalFrame * frameHeight) {
						NPC.frame.Y = startFrame * frameHeight;
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
					if(NPC.frameCounter == 144){
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
		}
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

		public override bool PreDraw(ref Color lightColor) {
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
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

			return false;
		}
	}
	public class FrostNovaRevival : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 12;
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

			if (++Projectile.frameCounter >= 15) {
				Projectile.frameCounter = 0;
				if (Projectile.frame < 12) {
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

		public override bool PreDraw(ref Color lightColor) {
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
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

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

		public override bool PreDraw(ref Color lightColor) {
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
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
			return false;
		}
	}
	public class FrostNovaYellowAura : ModProjectile
	{
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 15;
			//DrawOriginOffsetY = -40;
		}
		public override void SetDefaults() {
			Projectile.width = 62;
			Projectile.height = 64;
			Projectile.aiStyle = 0;
			Projectile.timeLeft = 25 * 60;
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
			Projectile.ai[1]++;
			Projectile.spriteDirection = Projectile.direction;

			if (++Projectile.frameCounter >= 15) {
				Projectile.frameCounter = 0;
				if (Projectile.frame < 15) {
					Projectile.frame++;
				}
				else {
					Projectile.frame = 0;
				}
			}

			if (Projectile.ai[1] > 20 * 60) {
				Projectile.Kill();
			}
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
			Projectile.damage = 100;
			Projectile.light = 1f;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.scale = 1f;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			var newSource = Projectile.GetSource_FromThis();
			Projectile.NewProjectile(newSource, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BlizzardStorm>(), 0, 0f, 0);
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
			target.AddBuff(BuffID.Frozen, 180);
		}

		public override void AI() {
			timer++;
			r = 235 + 20 * (int)MathF.Sin(timer * MathHelper.Pi / 180);
			g = 245 + 10 * (int)MathF.Sin(timer * MathHelper.Pi / 180);

			if (Main.rand.NextFloat() < 0.25f) {
				Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Bosses.FrostNovaDeathDust>(), Vector2.Zero, 120, new Color(255, 255, 255), 0.5f);
			}

			float uptimer = 60;
			float spintimer = 495;
			float lerptimer = 60;

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
				Projectile.velocity.Y += 0.03f;
				Projectile.velocity.X = float.Lerp(Projectile.velocity.X, Projectile.ai[0] * 8f * (60 / (timer - spintimer)), 0.025f);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/WindTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, Vector2.Zero, new Color(r, g, 255), new Color(0, 0, 0), 15f * Math.Min(timer / 60, 1), true);
			return true;
		}
	}
	//玩家超过出龙卷风区域时龙卷风消失？？//不会写
	public class BlizzardStorm : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		public override void SetDefaults() {
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.aiStyle = 0;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = 6000;
			Projectile.alpha = 0;
			Projectile.damage = 0;
			Projectile.light = 0f;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.scale = 1f;
		}

		private int trailnum = Main.rand.Next(8, 13);//随机8-12条轨迹

		public static int HostNPCType() {
			return ModContent.NPCType<FrostNova>();
		}

		private float timer;

		public override void AI() {
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

			var newSource = Projectile.GetSource_FromThis();
			if ((int)timer % 30 == 0) {
				for (int i = 0; i < trailnum; i++) {//生成轨迹，ai0为相位，2iπ/个数
					Projectile.NewProjectile(newSource, Projectile.Center, Vector2.Zero, ModContent.ProjectileType<Blizzard>(), 0, 0f, 0, 2 * i * MathHelper.Pi / trailnum);
				}
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
			Projectile.timeLeft = 900;
			Projectile.alpha = 0;
			Projectile.damage = 0;
			Projectile.light = 1f;
			Projectile.friendly = false;
			Projectile.hostile = false;
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
			target.AddBuff(BuffID.Frozen, 120);
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

		public override bool PreDraw(ref Color lightColor) {
			Texture2D trailtexture = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/WindTrail").Value;
			TrailMaker.ProjectileDrawTailByConstWidth(Projectile, trailtexture, Vector2.Zero, new Color((int)(r * colorstate), (int)(g * colorstate), (int)(255 * colorstate)), new Color(0, 0, 0), 15f, true);
			return true;
		}
	}
}