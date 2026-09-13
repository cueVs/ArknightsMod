using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;


namespace ArknightsMod.Content.NPCs.Enemy.Chapter6
{
	public class IceCleaver:ModNPC
	{
		private int fadeTimer;
		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 21;
			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers() { // Influences how the NPC looks in the Bestiary
				Velocity = -1f // Draws the NPC in the bestiary as if its walking +1 tiles in the x direction
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
		}
		public override void SetDefaults() {
			NPC.width = 35;
			NPC.height = 60;
			NPC.damage = 38;
			NPC.defense = 50;
			NPC.lifeMax = 1600;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath3;
			NPC.value = 100f;
			NPC.knockBackResist = 0.20f;
			NPC.aiStyle = -1;
			NPC.scale = 1f;
		}
		public override void OnSpawn(IEntitySource source) {
			fadeTimer = 60; // 持续60帧
			NPC.color = Color.Black; // 初始为纯黑
			NPC.alpha = 240;
		}
		public override void FindFrame(int frameHeight) {

			attackframeY = 10 * frameHeight;
			NPC.TargetClosest(true);
			framecounter++;
			

			if (attack == true && (NPC.frame.Y > attackframeY)) {
				NPC.frame.Y = 0;
			}

			if (walk == true && (NPC.frame.Y <= attackframeY || NPC.frame.Y > (20 * frameHeight))) {
				NPC.frame.Y = attackframeY;
				
			}
			//26.6.10 改
			//速度为0时播放 前摇第一帧
			if (framecounter >= Framespeed) {
				NPC.frame.Y += frameHeight;
				framecounter = 0;
			}
			else if (!attack) {
				if (NPC.velocity.X == 0) {
					NPC.frame.Y = 0;
					//NPC.frame.Y = 9 * frameHeight;
					framecounter--;
				}
			}
		}
		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			NPC.spriteDirection = -NPC.direction;
			if (fadeTimer > 0) {
				fadeTimer--;
			}
			Color drawcolor = Color.Lerp(new Color(0, 0, 0, 65), new Color(255, 255, 255, 255), 1f - fadeTimer / 60f);
			// 动态计算原点（水平居中，底部对齐碰撞箱）
			Vector2 origin1 = new Vector2(NPC.frame.Width *2/ 3f, NPC.frame.Height - 55);
			Vector2 origin2 = new Vector2(NPC.frame.Width / 3f, NPC.frame.Height - 55);

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
			return false;
		}

		private int AttackCD = 0;
		private bool attack;
		private bool walk = true;
		private int Framespeed = 7;
		private int framecounter;
		private int attackframeY;
		private float maxspeed = 1.2f;
		private int jumpCD = 0;

		/// <summary>
		/// 陆地npc跳跃 下平台 逻辑 //BY KZ
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="width">npc宽</param>
		/// <param name="height">npc高</param>
		/// <param name="jumpHeight">跳跃高度</param>
		/// <param name="GetOffPlatform">能否下平台</param>
		public static void LandNPCMovementLogic(NPC npc, float width = 0, float height = 0, float jumpHeight = 8, bool GetOffPlatform = true, bool IfJump = true) {
			Player player = Main.player[npc.target];
			int wi = width == 0 ? npc.width / 16 : (int)(width / 16);
			int he = height == 0 ? npc.height / 16 : (int)(height / 16);
			int tileX = (int)(npc.position.X / 16f);
			int tileY = (int)((npc.position.Y + 4) / 16f);
			if (player != null) {
				if (GetOffPlatform) {
					if (player.Center.Y - 3 > npc.Center.Y && TileID.Sets.Platforms[Main.tile[tileX, tileY + 3].TileType]) {
						npc.Center += new Vector2(0, 1);
					}
				}
				if (IfJump) {
					int di = npc.direction == -1 ? -1 : wi + 1;
					//Main.NewText(npc.velocity.Y);
					if (npc.velocity.Y == 0) {
						float Jump = 0;

						if (Math.Abs(player.Center.X - npc.Center.X) < 10) {
							if (player.Center.Y + player.height < npc.Center.Y && npc.Center.Y - player.Center.Y - player.height < jumpHeight * 16)
								Jump += he;
						}
						else {
							if (npc.velocity.X != 0) {
								for (int i = 0; i <= jumpHeight; i++) {
									Tile t = Framing.GetTileSafely(tileX + di, tileY + he - i);
									//跳跃通过 墙
									if (Main.tileSolid[t.TileType] && t.HasTile && !TileID.Sets.Platforms[t.TileType] && Main.tileSolid[t.TileType]) {
										Jump++;
										//Main.NewText(t);
									}
									if (Jump == jumpHeight + 1)
										Jump = 0;
								}
							}
							/*
							if (Jump > 0) {
								float CanUpJump = 0;
								float OldJump = Jump;
								for (int i = -he - 3; i < 0; i++) {
									Tile t = Framing.GetTileSafely(tileX + di, tileY + i);
									//Main.NewText(t + " " + Framing.GetTileSafely((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16));
									if ((Math.Abs(player.Center.X - npc.Center.X) < 50 && player.Center.Y < npc.Center.Y - 16 * 6) || (Main.tileSolid[t.TileType] && t.HasTile)) {
										Jump += -i * 0.9f;
										CanUpJump = 0;
										continue;
									}
									else {
										CanUpJump++;
									}
									if (CanUpJump >= he) {
										Jump = OldJump;
										break;
									}
								}
							}
							*/

							//跳跃通过 沟
							if (player.Center.Y < npc.Center.Y) {
								bool[] CanAdd = { false, false };
								for (int jj = 0; jj < 2; jj++)
									for (int i = he + 1; i < he + 4; i++) {
										//Dust.NewDustPerfect(new Point(tileX + di - jj * npc.direction, tileY + i).ToWorldCoordinates(), 6).noGravity = true;

										if (!Framing.GetTileSafely(tileX + di - jj * npc.direction, tileY + i).HasTile) {
											CanAdd[jj] = true;
											//Main.NewText(jj + " " + i + " " + Framing.GetTileSafely(tileX + di - jj * npc.direction, tileY + i));
										}
										else {
											CanAdd[jj] = false;
											break;
										}
									}
								if (CanAdd[0] && CanAdd[1])
									Jump += 4;
							}

							//跳跃通过岩浆
							if (npc.direction > 0) {
								for (int i = di + npc.direction; i > 0; i--) {
									Tile T = Framing.GetTileSafely(tileX + di + i, tileY + he);
									//Main.NewText(T + " " + Framing.GetTileSafely((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16));
									if (T.LiquidAmount > 0) {
										if (T.LiquidType == LiquidID.Lava) {


											Jump += 4;
											npc.velocity.X += npc.direction;
										}
									}
								}
							}
							else {
								for (int i = di + npc.direction; i <= 0; i++) {
									Tile T = Framing.GetTileSafely(tileX + i, tileY + he);
									//Main.NewText(T + " " + Framing.GetTileSafely((int)Main.MouseWorld.X / 16, (int)Main.MouseWorld.Y / 16));
									if (T.LiquidAmount > 0) {
										if (T.LiquidType == LiquidID.Lava) {

											Jump += 4;
											npc.velocity.X += npc.direction;
										}
									}
								}
							}
						}
						if (Jump > 0) {
							float result = (float)(4 * Math.Sqrt(Jump));
							result = Math.Clamp(result, 0, (float)(4 * Math.Sqrt(jumpHeight)));
							if (npc.wet)
								result *= 1.215f;
							npc.velocity.Y -= result;
							//npc.velocity.Y -= 10;
						}
					}
				}
			}
		}

		public override void AI() {
			Player p = Main.player[NPC.target];
			if (walk == true) {
				NPC.spriteDirection = -NPC.direction;
				AttackCD++;
				/*if (NPC.position.X - p.position.X < -100 || (NPC.position.X - p.position.X < 100 && NPC.position.X - p.position.X > 0)) {
					if (NPC.velocity.X < maxspeed) {
						NPC.velocity.X += 0.4f;
					}
					if (NPC.velocity.X >= maxspeed) {
						NPC.velocity.X = maxspeed;
					}
				}

				if (NPC.position.X - p.position.X > 100 || (NPC.position.X - p.position.X > -100 && NPC.position.X - p.position.X < 0)) {
					if (NPC.velocity.X > -maxspeed) {
						NPC.velocity.X -= 0.4f;
					}
					if (NPC.velocity.X <= -maxspeed) {
						NPC.velocity.X = -maxspeed;
					}
				}*/

				//26.6.10 改
				//优化移动逻辑
				NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, NPC.direction * maxspeed, 0.1f);


				/*
				if (Math.Abs(NPC.velocity.X) <= 0.5f) {
					jumpCD++;
				}
				if (jumpCD >= 180) {
					jumpCD = 0;
					NPC.velocity.Y = -7.2f;
				}
				*/


				//26.6.10 改
				//防止神秘太空步
				if (Math.Abs(NPC.position.X - p.position.X) <= 100 && Math.Abs(NPC.position.Y - p.position.Y) <= 100) {

					NPC.velocity.X = 0;
					if (AttackCD >= 100 && !attack) {
						walk = false;
						attack = true;
						AttackCD = 0;
					}
				}
				if(Math.Abs(NPC.position.X - p.position.X) >= 120) {
					AttackCD = 114514;
				}

				//26.6.10 改
				//跳+下平台
				LandNPCMovementLogic(NPC, NPC.width, NPC.height, 8);

			}
			if (attack == true) {
				NPC.velocity.X = 0;
				AttackCD++;
				NPC.damage = 76;
				if (AttackCD == 25) {
					if (NPC.spriteDirection < 0) {
						Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X + 60, NPC.Center.Y + 40), new Vector2(0, 0), ModContent.ProjectileType<Icebreak>(), 38, 0.8f);

					}
					if (NPC.spriteDirection > 0) {
						Projectile.NewProjectile(NPC.GetSource_FromThis(), new Vector2(NPC.Center.X - 60, NPC.Center.Y + 40), new Vector2(0, 0), ModContent.ProjectileType<Icebreak>(), 38, 0.8f);

					}
				}
				
				if (AttackCD > 70) {
					attack = false;
					walk = true;
					AttackCD = 0;
					NPC.damage = 38;
				}
			}
		}

		public override bool? CanFallThroughPlatforms() {
			Player player = Main.player[NPC.target];
			return (player.position.Y + player.height) - (NPC.position.Y + NPC.height) > 0;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			Player p = Main.player[NPC.target];
			if (p.frozen == true) {
				modifiers.SourceDamage *= 3;

			}
			if (Main.expertMode)
				modifiers.SourceDamage *= 1.5f; // 专家模式伤害 ×1.5
			if (Main.masterMode)
				modifiers.SourceDamage *= 2f;   // 大师模式伤害 ×2
		}
		public override void ModifyNPCLoot(NPCLoot npcLoot) {

			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Device>(), 8, 1, 1));
			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Polyketon>(), 8, 1, 1));

		}
	}

	public class Icebreak : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		public override void SetDefaults() {
			Projectile.width = 180;
			Projectile.height = 180;
			Projectile.damage = 76;
			Projectile.penetrate = 9999;
			Projectile.tileCollide = false;
			Projectile.hostile = true;
		}
		private int flytime;
		public override void AI() { 
			flytime++;
			if (flytime >= 50) {
				Projectile.timeLeft = 1;
			}
			Dust dust;
			dust = Dust.NewDustDirect(Projectile.position,135, 135, DustID.Ice, 0, 0, 0, default, 1);
			dust.noGravity = true;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			if (target.frozen == true) {
				modifiers.SourceDamage *= 3;
			}
			if (Main.expertMode)
				modifiers.SourceDamage *= 0.75f; // 专家模式伤害 ×1.5
			else if (Main.masterMode)
				modifiers.SourceDamage *= 0.7f;   // 大师模式伤害 ×2
		}
	}
}
