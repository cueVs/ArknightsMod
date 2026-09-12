using ArknightsMod.Systems.Gameplay.Damage;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	public class Caster : ModNPC
	{
		// AI状态枚举
		private enum AIState
		{
			Spectate,   // 旁观模式
			Skirmish,   // 游击模式
			Assault,    // 进击模式
			Flee        // 逃跑模式
		}

		// 动画帧
		// 在类顶部重新定义帧枚举
		private enum Frame
		{
			Walk1 = 0,  // 第0帧
			Walk2,      // 第1帧
			Walk3,      // 第2帧
			Walk4,      // 第3帧
			Walk5,      // 第4帧
			Walk6,      // 第5帧
			Walk7,      // 第6帧 (行走帧结束)
			Attack1,    // 第7帧 (攻击开始)
			Attack2,    // 第8帧
			Attack3,    // 第9帧
			Attack4,    // 第10帧
			Attack5,    // 第11帧
			Attack6,    // 第12帧
			Attack7,    // 第13帧 (发射射弹)
			Attack8,    // 第14帧
			Attack9,    // 第15帧
			Attack10,   // 第16帧
			Attack11,   // 第17帧
			Attack12,   // 第18帧
			Attack13,   // 第19帧
			Attack14    // 第20帧 (攻击结束)
		}


		// NPC属性
		private AIState CurrentAIState {
			get => (AIState)NPC.ai[0];
			set => NPC.ai[0] = (float)value;
		}

		private float Anger {
			get => NPC.ai[1];
			set => NPC.ai[1] = value;
		}

		private float Courage {
			get => NPC.ai[2];
			set => NPC.ai[2] = value;
		}

		private float AttackCooldown {
			get => NPC.ai[3];
			set => NPC.ai[3] = value;
		}

		private float InitialAnger { get; set; }
		private float InitialCourage { get; set; }
		private float Range { get; set; }

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 21; // 总共有19帧
		}

		public override void SetDefaults() {
			NPC.width = 30;
			NPC.height = 40;
			NPC.lifeMax = 80;
			NPC.damage = 10;
			NPC.defense = 5;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 100f;
			NPC.knockBackResist = 0.5f;
			NPC.noGravity = false;
			NPC.noTileCollide = false;
			// 初始化AI状态
			CurrentAIState = AIState.Spectate;

			// 随机初始值
			InitialAnger = Main.rand.Next(300, 501);
			InitialCourage = Main.rand.Next(-100, 351);
			Anger = InitialAnger;
			Courage = InitialCourage;

			// 随机射程
			Range = Main.rand.Next(285, 316);

			var genreNPC = NPC.GetGlobalNPC<DamageCategoryNPC>();
			genreNPC.artsResistance = 0.2f;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[] {
				BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
				new FlavorTextBestiaryInfoElement("一个会施法的整合运动成员，根据情绪状态采取不同战术。"),
			});
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			return SpawnCondition.OverworldNightMonster.Chance * 0.1f;
		}


		// 行走动画 (1-7帧)
		public override void FindFrame(int frameHeight) {
			NPC.TargetClosest(true);
			NPC.frameCounter++;

			// 计算当前帧索引
			int currentFrame = NPC.frame.Y / frameHeight;

			// 攻击动画优先处理 (7-20帧)
			if (AttackCooldown > 0) {
				// 确保从攻击第一帧开始
				if (currentFrame < (int)Frame.Attack1 || currentFrame > (int)Frame.Attack14) {
					NPC.frame.Y = (int)Frame.Attack1 * frameHeight;
					currentFrame = (int)Frame.Attack1;
				}

				// 每7帧更新一次动画
				if (NPC.frameCounter >= 7) {
					NPC.frameCounter = 0;
					currentFrame++;
					NPC.frame.Y = currentFrame * frameHeight;

					// 在第14帧(攻击序列第7帧)发射射弹
					if (currentFrame == (int)Frame.Attack7) {
						ShootProjectile();
					}

					// 攻击动画结束
					if (currentFrame > (int)Frame.Attack14) {
						NPC.frame.Y = (int)Frame.Walk1 * frameHeight;
						AttackCooldown = 0; // 重置攻击状态
					}
				}
				return;
			}

			// 移动状态处理 (0-6帧)
			if (Math.Abs(NPC.velocity.X) > 0.1f || Math.Abs(NPC.velocity.Y) > 0.1f) {
				// 每7帧更新一次动画
				if (NPC.frameCounter >= 7) {
					NPC.frameCounter = 0;
					currentFrame++;

					// 循环行走动画 (0-6帧)
					if (currentFrame > (int)Frame.Walk7) {
						currentFrame = (int)Frame.Walk1;
					}

					NPC.frame.Y = currentFrame * frameHeight;
				}
				return;
			}

			// 静止状态处理 (第0帧)
			NPC.frame.Y = (int)Frame.Walk1 * frameHeight;
			NPC.frameCounter = 0;
		}
		private void ShootProjectile() {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y,
				ModContent.NPCType<CasterShoot>(), Target: NPC.target);
		}

		public override void AI() {
			// 更新目标
			NPC.TargetClosest(true);
			Player player = Main.player[NPC.target];

			// 如果目标无效或死亡
			if (!player.active || player.dead) {
				Courage += 300;
				Anger = InitialAnger;
				NPC.velocity.X = 0;
				return;
			}

			// 更新愤怒和勇气值
			UpdateEmotions(player);

			// 检查并更新AI状态
			UpdateAIState();

			// 根据当前状态执行行为
			ExecuteBehavior(player);

			// 更新攻击冷却
			if (AttackCooldown > 0) {
				AttackCooldown--;
			}

			// 设置朝向
			if (NPC.velocity.X > 0) {
				NPC.spriteDirection = -1;
			}
			else if (NPC.velocity.X < 0) {
				NPC.spriteDirection = 1;
			}
			if (NPC.velocity.X == 0) {
				if (NPC.position.X- player.position.X < 0) {
					NPC.spriteDirection = -1; // 面向左侧
				}
				else {
					NPC.spriteDirection = 1; // 面向右侧
				}
			}
		}

		private void UpdateEmotions(Player player) {
			// 检查是否有其他整合运动成员被杀死
			// 这里简化处理，实际需要更复杂的检测

			// 自身被伤害时愤怒增加 (在OnHit方法中处理)

			// 当与目标之间有其他敌怪时，勇气值每帧加1.5
			if (HasOtherEnemiesBetween(player)) {
				Courage += 1.5f;
			}

			// 当世界存在boss活跃时，勇气固定为1000
			if (NPC.AnyNPCs(NPCID.EyeofCthulhu) || NPC.AnyNPCs(NPCID.EaterofWorldsHead) ||
				NPC.AnyNPCs(NPCID.SkeletronHead) || NPC.AnyNPCs(NPCID.WallofFlesh)) {
				Courage = 1000;
			}

			// 自身血量小于一半时，每帧加2愤怒和1勇气
			if (NPC.life < NPC.lifeMax / 2) {
				Anger += 2;
				Courage += 1;
			}

			// 当与目标X轴距离小于100时，每帧减2勇气
			if (Math.Abs(NPC.Center.X - player.Center.X) < 100) {
				Courage -= 2;
			}

			// 限制值范围
			Anger = MathHelper.Clamp(Anger, 0, 1000);
			Courage = MathHelper.Clamp(Courage, -1000, 1000);
		}

		private bool HasOtherEnemiesBetween(Player player) {
			// 简化实现，实际需要更精确的检测
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC other = Main.npc[i];
				if (other.active && other != NPC && other.friendly == false && other.life > 0) {
					if (Collision.CanHitLine(NPC.position, NPC.width, NPC.height, other.position, other.width, other.height) &&
						Collision.CanHitLine(other.position, other.width, other.height, player.position, player.width, player.height)) {
						return true;
					}
				}
			}
			return false;
		}

		private void UpdateAIState() {
			// 逃跑模式优先级最高
			if (Courage < 0 && Anger < 400) {
				CurrentAIState = AIState.Flee;
				return;
			}

			// 进击模式
			if (Anger > 800 || Courage > 800) {
				CurrentAIState = AIState.Assault;
				return;
			}

			// 游击模式
			if (Anger + Courage > 1000) {
				CurrentAIState = AIState.Skirmish;
				return;
			}

			// 旁观模式
			if (Anger < 500 && Courage < 500) {
				CurrentAIState = AIState.Spectate;
				return;
			}
		}

		private void ExecuteBehavior(Player player) {
			float distanceToPlayer = Vector2.Distance(NPC.Center, player.Center);

			switch (CurrentAIState) {
				case AIState.Spectate:
					// 尝试与玩家保持(50+射程值)的距离
					float desiredDistance = 50 + Range;

					if (distanceToPlayer < desiredDistance) {
						// 远离玩家
						NPC.velocity.X = NPC.direction * -2f; // 与僵尸相似的速度
					}
					else if (distanceToPlayer > desiredDistance + 50) {
						// 接近玩家
						NPC.velocity.X = NPC.direction * 2f;
					}
					else {
						NPC.velocity.X = 0;
					}
					break;

				case AIState.Skirmish:
					// 尝试与玩家保持射程值的距离
					if (distanceToPlayer < Range - 50) {
						// 远离玩家
						NPC.velocity.X = NPC.direction * -2f;
					}
					else if (distanceToPlayer > Range + 50) {
						// 接近玩家
						NPC.velocity.X = NPC.direction * 2f;
					}
					else {
						NPC.velocity.X = 0;
					}

					// 每4秒发射一次射弹
					if (AttackCooldown <= 0) {
						AttackCooldown = 4 * 60; // 4秒
						NPC.frame.Y = (int)Frame.Attack1 * NPC.height;
						NPC.frameCounter = 0;
						NPC.velocity.X = 0;
					}
					break;

				case AIState.Assault:
					// 当距离大于射程时接近玩家
					if (distanceToPlayer > Range) {
						NPC.velocity.X = NPC.direction * 2f;
					}
					else {
						NPC.velocity.X = 0;

						// 每2秒发射一次射弹
						if (AttackCooldown <= 0) {
							AttackCooldown = 2.5f * 60; // 2秒
							NPC.frame.Y = (int)Frame.Attack1 * NPC.height;
							NPC.frameCounter = 0;
						}
					}
					break;

				case AIState.Flee:
					// 持续向远离玩家的方向移动
					NPC.velocity.X = NPC.direction * -4f; // 逃跑时速度稍快
					break;
			}

			// 简单的地面检测和重力
			if (NPC.collideY) {
				NPC.velocity.Y = 0;
			}
			else {
				NPC.velocity.Y += 0.5f;
				if (NPC.velocity.Y > 10f) {
					NPC.velocity.Y = 10f;
				}
			}
		}

		public override void ModifyIncomingHit(ref NPC.HitModifiers modifiers) {
			// 当受到任何伤害时增加愤怒值
			Anger += 150;
			Anger = MathHelper.Clamp(Anger, 0, 1000);

			// 可以在这里修改伤害参数
			// modifiers.SourceDamage *= 0.5f; // 例如减半伤害
		}
	}
	public class CasterShoot : ModNPC {
		private static BasicEffect trailEffect;

		public override string Texture => "ArknightsMod/Content/NPCs/Enemy/ThroughChapter4/explode";

		public override void SetStaticDefaults() {
			NPCID.Sets.ProjectileNPC[Type] = true;
			NPCID.Sets.TrailCacheLength[Type] = 11;
			NPCID.Sets.TrailingMode[Type] = 0;
		}

		public override void SetDefaults() {
			NPC.width = 10;
			NPC.height = 10;
			NPC.friendly = false;
			NPC.lifeMax = 1;
			NPC.HideStrikeDamage = true;
			NPC.timeLeft = 200;
			NPC.scale = 1f;
			NPC.damage = 10;
			NPC.aiStyle = -1;
			NPC.noTileCollide = true;
		}

		private const float MoveSpeed = 12f;
		private Vector2 speedPos;

		public override void OnSpawn(IEntitySource source) {
			Player target = Main.player[NPC.target];
			speedPos = (target.MountedCenter - NPC.Center).SafeNormalize(Vector2.UnitY) * MoveSpeed;
		}

		public override void AI() {
			NPC.velocity = speedPos;
			Lighting.AddLight(NPC.Center, 0.04f, 0.22f, 0.65f);
		}

		private static Color TrailColor(float progress, Color head, Color middle, Color tail, float opacity) {
			Color color = progress < 0.45f
				? Color.Lerp(head, middle, progress / 0.45f)
				: Color.Lerp(middle, tail, (progress - 0.45f) / 0.55f);
			float fade = 1f - MathHelper.SmoothStep(0.58f, 1f, progress);
			return color * (opacity * fade);
		}

		private static void DrawTrailLayer(GraphicsDevice device, BasicEffect effect, List<Vector2> points,
			Texture2D texture, float headWidth, Color head, Color middle, Color tail, float opacity) {
			if (points.Count < 2)
				return;

			int count = points.Count;
			var left = new Vector2[count];
			var right = new Vector2[count];
			var colors = new Color[count];

			for (int i = 0; i < count; i++) {
				Vector2 tangent;
				if (i == 0)
					tangent = points[0] - points[1];
				else if (i == count - 1)
					tangent = points[i - 1] - points[i];
				else
					tangent = points[i - 1] - points[i + 1];

				tangent = tangent.SafeNormalize(Vector2.UnitX);
				Vector2 normal = new Vector2(-tangent.Y, tangent.X);
				float progress = i / (float)(count - 1);
				float width = MathHelper.Lerp(headWidth, 0.2f, progress);
				left[i] = points[i] - normal * width;
				right[i] = points[i] + normal * width;
				colors[i] = TrailColor(progress, head, middle, tail, opacity);
			}

			var vertices = new VertexPositionColorTexture[(count - 1) * 6];
			int vertexIndex = 0;
			for (int i = 0; i < count - 1; i++) {
				float progress = i / (float)(count - 1);
				float nextProgress = (i + 1) / (float)(count - 1);
				vertices[vertexIndex++] = new VertexPositionColorTexture(new Vector3(left[i], 0f), colors[i], new Vector2(progress, 0f));
				vertices[vertexIndex++] = new VertexPositionColorTexture(new Vector3(right[i], 0f), colors[i], new Vector2(progress, 1f));
				vertices[vertexIndex++] = new VertexPositionColorTexture(new Vector3(left[i + 1], 0f), colors[i + 1], new Vector2(nextProgress, 0f));
				vertices[vertexIndex++] = new VertexPositionColorTexture(new Vector3(right[i], 0f), colors[i], new Vector2(progress, 1f));
				vertices[vertexIndex++] = new VertexPositionColorTexture(new Vector3(right[i + 1], 0f), colors[i + 1], new Vector2(nextProgress, 1f));
				vertices[vertexIndex++] = new VertexPositionColorTexture(new Vector3(left[i + 1], 0f), colors[i + 1], new Vector2(nextProgress, 0f));
			}

			effect.TextureEnabled = true;
			effect.Texture = texture;
			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
			}
		}

		private static void DrawTriangleLayer(GraphicsDevice device, BasicEffect effect, List<Vector2> points,
			float headWidth, Color head, Color middle, Color tail, float opacity) {
			if (points.Count < 2)
				return;

			int count = points.Count;
			var left = new Vector2[count];
			var right = new Vector2[count];
			var colors = new Color[count];
			for (int i = 0; i < count; i++) {
				Vector2 tangent;
				if (i == 0)
					tangent = points[0] - points[1];
				else if (i == count - 1)
					tangent = points[i - 1] - points[i];
				else
					tangent = points[i - 1] - points[i + 1];

				tangent = tangent.SafeNormalize(Vector2.UnitX);
				Vector2 normal = new Vector2(-tangent.Y, tangent.X);
				float progress = i / (float)(count - 1);
				float width = MathHelper.Lerp(headWidth, 0f, progress);
				left[i] = points[i] - normal * width;
				right[i] = points[i] + normal * width;
				colors[i] = TrailColor(progress, head, middle, tail, opacity);
			}

			var vertices = new VertexPositionColor[(count - 1) * 6];
			int vertexIndex = 0;
			for (int i = 0; i < count - 1; i++) {
				vertices[vertexIndex++] = new VertexPositionColor(new Vector3(left[i], 0f), colors[i]);
				vertices[vertexIndex++] = new VertexPositionColor(new Vector3(right[i], 0f), colors[i]);
				vertices[vertexIndex++] = new VertexPositionColor(new Vector3(left[i + 1], 0f), colors[i + 1]);
				vertices[vertexIndex++] = new VertexPositionColor(new Vector3(right[i], 0f), colors[i]);
				vertices[vertexIndex++] = new VertexPositionColor(new Vector3(right[i + 1], 0f), colors[i + 1]);
				vertices[vertexIndex++] = new VertexPositionColor(new Vector3(left[i + 1], 0f), colors[i + 1]);
			}

			effect.TextureEnabled = false;
			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices, 0, vertices.Length / 3);
			}
		}

		private static void AddEllipse(List<VertexPositionColor> vertices, Vector2 center, Vector2 direction,
			float length, float width, Color centerColor, Color edgeColor, int segments) {
			direction = direction.SafeNormalize(Vector2.UnitX);
			Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
			for (int i = 0; i < segments; i++) {
				float angle = MathHelper.TwoPi * i / segments;
				float nextAngle = MathHelper.TwoPi * (i + 1) / segments;
				Vector2 edge = direction * MathF.Cos(angle) * length + normal * MathF.Sin(angle) * width;
				Vector2 nextEdge = direction * MathF.Cos(nextAngle) * length + normal * MathF.Sin(nextAngle) * width;
				vertices.Add(new VertexPositionColor(new Vector3(center, 0f), centerColor));
				vertices.Add(new VertexPositionColor(new Vector3(center + edge, 0f), edgeColor));
				vertices.Add(new VertexPositionColor(new Vector3(center + nextEdge, 0f), edgeColor));
			}
		}

		private static void DrawOrb(GraphicsDevice device, BasicEffect effect, Vector2 center, Vector2 direction) {
			float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.06f;
			var vertices = new List<VertexPositionColor>(144);
			Vector2 normal = direction.RotatedBy(MathHelper.PiOver2);
			AddEllipse(vertices, center - direction * 1.4f, direction, 9.5f * pulse, 6.5f * pulse,
				new Color(30, 102, 255, 100), new Color(8, 22, 110, 0), 20);
			AddEllipse(vertices, center, direction, 6f * pulse, 4f * pulse,
				new Color(178, 239, 255), new Color(22, 72, 235, 210), 20);
			Vector2 highlight = center + direction * 1.5f - normal * 0.8f;
			AddEllipse(vertices, highlight, direction, 2.5f * pulse, 1.6f * pulse,
				new Color(245, 253, 255), new Color(80, 192, 255, 0), 16);

			effect.TextureEnabled = false;
			foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
				pass.Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3);
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			if (Main.dedServ)
				return false;

			if (trailEffect == null || trailEffect.IsDisposed) {
				trailEffect = new BasicEffect(Main.instance.GraphicsDevice) {
					VertexColorEnabled = true,
					World = Matrix.Identity,
					View = Matrix.Identity
				};
			}

			var points = new List<Vector2>(NPC.oldPos.Length + 1) {
				NPC.Center - screenPos
			};
			Vector2 normal = new Vector2(-NPC.velocity.Y, NPC.velocity.X).SafeNormalize(Vector2.UnitY);
			float waveTime = Main.GlobalTimeWrappedHourly * 9f;
			for (int i = 0; i < NPC.oldPos.Length; i++) {
				if (NPC.oldPos[i] == Vector2.Zero)
					break;
				Vector2 point = NPC.oldPos[i] + NPC.Size * 0.5f - screenPos;
				float progress = (i + 1f) / NPC.oldPos.Length;
				point += normal * MathF.Sin(waveTime - i * 0.78f) * (1.4f * progress);
				if (Vector2.DistanceSquared(points[^1], point) > 1f)
					points.Add(point);
			}

			GraphicsDevice device = Main.instance.GraphicsDevice;
			spriteBatch.End();
			device.BlendState = BlendState.Additive;
			device.RasterizerState = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.None;
			device.SamplerStates[0] = SamplerState.LinearClamp;

			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1f, 1f);
			trailEffect.Projection = Main.GameViewMatrix.TransformationMatrix * projection;
			Texture2D flameTrail = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/FlameTrail").Value;
			Texture2D lineTrail = ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;

			DrawTriangleLayer(device, trailEffect, points, 9f,
				new Color(86, 184, 255), new Color(42, 106, 232), new Color(24, 34, 128), 0.3f);
			DrawTrailLayer(device, trailEffect, points, flameTrail, 15f,
				new Color(54, 132, 255), new Color(25, 74, 215), new Color(28, 18, 112), 0.38f);
			DrawTrailLayer(device, trailEffect, points, lineTrail, 9f,
				new Color(188, 241, 255), new Color(48, 166, 255), new Color(38, 48, 176), 0.72f);
			DrawTrailLayer(device, trailEffect, points, lineTrail, 3f,
				new Color(247, 253, 255), new Color(108, 220, 255), new Color(42, 92, 220), 0.9f);
			DrawOrb(device, trailEffect, NPC.Center - screenPos, NPC.velocity.SafeNormalize(Vector2.UnitX));

			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
				DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
			return false;
		}
		public override void OnHitPlayer(Player target, Player.HurtInfo info) {
			target.immuneTime = 0;
		}
		public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers) {
			target.GetModPlayer<global::ArknightsMod.Players.ImmunePlayer>().ImmuneMultiplier = 0.6f; // 免疫倍数
			modifiers.ArmorPenetration += 9999f;
			if (Main.expertMode)
				modifiers.FinalDamage *= 0.9f; // 专家模式伤害 ×1.5
			if (Main.masterMode)
				modifiers.FinalDamage *= 0.85f;   // 大师模式伤害 ×2
		}



	}
}
