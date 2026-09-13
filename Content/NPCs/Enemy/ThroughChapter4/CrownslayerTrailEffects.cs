using System;
using System.Collections.Generic;
using ArknightsMod.Common.VisualEffects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	internal static class CrownslayerTrailEffects
	{
		private const float MaxContinuousNpcTrailSegment = 72f;
		private const float MaxContinuousNpcTrailSegmentSq = MaxContinuousNpcTrailSegment * MaxContinuousNpcTrailSegment;

		// 保留带状拖尾绘制代码，二阶段游戏内不显示
		private const bool DrawBossRibbonBand = false;

		private const float TrailFadeGhostLife = 22f;

		private sealed class BossTrailFadeGhost
		{
			public List<Vector2> Points;
			public float Life;
		}

		private static readonly List<BossTrailFadeGhost> TrailFadeGhosts = new();
		private static readonly Dictionary<int, List<Vector2>> LastTrailPointsByNpc = new();
		private static readonly Dictionary<int, bool> WasDrawingTrailByNpc = new();

		private static void UpdateTrailFadeGhosts()
		{
			for (int i = TrailFadeGhosts.Count - 1; i >= 0; i--) {
				TrailFadeGhosts[i].Life -= 1f;
				if (TrailFadeGhosts[i].Life <= 0f)
					TrailFadeGhosts.RemoveAt(i);
			}

			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || npc.type != ModContent.NPCType<Crownslayer>()) {
					LastTrailPointsByNpc.Remove(i);
					WasDrawingTrailByNpc.Remove(i);
				}
			}
		}

		private static bool TryCommitTrailFadeGhost(NPC npc, List<Vector2> trailPoints)
		{
			if (trailPoints == null || trailPoints.Count < 3)
				return false;

			// 避免同一位置连续提交多个几乎相同的幽灵拖尾
			if (TrailFadeGhosts.Count > 0) {
				BossTrailFadeGhost last = TrailFadeGhosts[TrailFadeGhosts.Count - 1];
				if (last.Life > TrailFadeGhostLife - 2f
					&& last.Points.Count == trailPoints.Count
					&& Vector2.DistanceSquared(last.Points[0], trailPoints[0]) < 4f) {
					return false;
				}
			}

			TrailFadeGhosts.Add(new BossTrailFadeGhost {
				Points = new List<Vector2>(trailPoints),
				Life = TrailFadeGhostLife
			});
			return true;
		}

		private static void DrawTrailFadeGhosts(SpriteBatch spriteBatch, float time)
		{
			if (TrailFadeGhosts.Count == 0)
				return;

			BeginNonPremultiplied(spriteBatch);
			foreach (BossTrailFadeGhost ghost in TrailFadeGhosts) {
				float alpha = MathHelper.Clamp(ghost.Life / TrailFadeGhostLife, 0f, 1f);
				alpha *= alpha;
				DrawBossHelixTrail(ghost.Points, time, alpha);
			}
			EndNonPremultiplied(spriteBatch);
		}

		public static void DrawBossDashTrail(SpriteBatch spriteBatch, NPC npc, Crownslayer.NPCState currentAnimation, Crownslayer.AIState currentAIState)
		{
			UpdateTrailFadeGhosts();

			bool isPhase2 = npc.life <= npc.lifeMax / 2;
			bool shouldDraw = ShouldDrawBossRibbonTrail(npc, currentAnimation, currentAIState);
			WasDrawingTrailByNpc.TryGetValue(npc.whoAmI, out bool wasDrawing);

			bool jumpedThisFrame = false;
			if (isPhase2 && npc.oldPos != null && npc.oldPos.Length > 0 && npc.oldPos[0] != Vector2.Zero) {
				float jumpSq = Vector2.DistanceSquared(npc.Center, npc.oldPos[0] + npc.Size * 0.5f);
				jumpedThisFrame = jumpSq > MaxContinuousNpcTrailSegmentSq * 2f;
			}

			// 拖尾中断：瞬移跳变，或从“正在绘制”进入隐身/减速等不可绘制状态
			bool trailInterrupted = isPhase2 && (jumpedThisFrame || (wasDrawing && !shouldDraw));
			if (trailInterrupted && LastTrailPointsByNpc.TryGetValue(npc.whoAmI, out List<Vector2> cachedInterrupt)) {
				TryCommitTrailFadeGhost(npc, cachedInterrupt);
				LastTrailPointsByNpc.Remove(npc.whoAmI);
				WasDrawingTrailByNpc[npc.whoAmI] = false;
			}

			// 幽灵必须在同一帧立刻绘制，避免“先消失一帧再出现”
			DrawTrailFadeGhosts(spriteBatch, Main.GlobalTimeWrappedHourly);

			if (!shouldDraw || jumpedThisFrame)
				return;

			Texture2D trailTexture = TextureAssets.MagicPixel.Value;
			List<Vector2> trailPoints = BuildNpcTrailPoints(npc, new Vector2(0f, npc.height * 0.08f));
			if (trailPoints.Count < 3)
				return;

			LastTrailPointsByNpc[npc.whoAmI] = new List<Vector2>(trailPoints);
			WasDrawingTrailByNpc[npc.whoAmI] = true;

			Color rimHead = new Color(255, 132, 42);
			Color rimTail = new Color(255, 132, 42, 0);
			Color bodyHead = new Color(138, 22, 38);
			Color bodyTail = new Color(138, 22, 38, 0);

			BeginNonPremultiplied(spriteBatch);
			if (DrawBossRibbonBand) {
				DrawRibbonTaperFade(trailPoints, 10.5f, 0.11f, rimHead, rimTail, trailTexture);
				DrawRibbonTaperFade(trailPoints, 6f, 0f, bodyHead, bodyTail, trailTexture);
			}
			DrawBossHelixTrail(trailPoints, Main.GlobalTimeWrappedHourly);
			EndNonPremultiplied(spriteBatch);
		}

		// 冲刺路径上的本体残影（先于拖尾与默认绘制）。
		public static void DrawBossDashAfterimages(SpriteBatch spriteBatch, NPC npc, Color drawColor, Crownslayer.NPCState currentAnimation, Crownslayer.AIState currentAIState)
		{
			if (Main.dedServ || !ShouldDrawBossAfterimages(npc, currentAnimation, currentAIState))
				return;

			if (npc.oldPos == null || npc.oldPos.Length < 2)
				return;

			Texture2D texture = TextureAssets.Npc[npc.type].Value;
			Rectangle frame = npc.frame;
			Vector2 origin = frame.Size() * 0.5f;
			SpriteEffects effects = npc.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

			List<int> validAfterimageIndices = new();
			Vector2 previousCenter = npc.Center;
			for (int k = 0; k < npc.oldPos.Length; k++) {
				if (npc.oldPos[k] == Vector2.Zero)
					break;

				Vector2 oldCenter = npc.oldPos[k] + npc.Size * 0.5f;
				if (Vector2.DistanceSquared(previousCenter, oldCenter) > MaxContinuousNpcTrailSegmentSq)
					break;

				if (k > 0)
					validAfterimageIndices.Add(k);

				previousCenter = oldCenter;
			}

			for (int index = validAfterimageIndices.Count - 1; index >= 0; index--) {
				int k = validAfterimageIndices[index];

				float along = 1f - k / (float)(npc.oldPos.Length - 1);
				float fade = along * along * 0.62f;
				if (fade < 0.04f)
					continue;

				Color tint = Color.Lerp(new Color(200, 72, 88), new Color(120, 18, 32), 1f - along);
				Color c = drawColor.MultiplyRGBA(tint);
				c.A = (byte)MathHelper.Clamp((int)(200f * fade), 0, 200);

				Vector2 pos = npc.oldPos[k] + npc.Size * 0.5f - Main.screenPosition;
				spriteBatch.Draw(texture, pos, frame, c, npc.rotation, origin, npc.scale, effects, 0f);
			}
		}

		public static void DrawBossOrbitLines(SpriteBatch spriteBatch, NPC npc, float intensity)
		{
			if (Main.dedServ)
				return;

			Texture2D glow = TextureAssets.Extra[98].Value;
			intensity = MathHelper.Clamp(intensity, 0f, 1f);
			float time = Main.GlobalTimeWrappedHourly;

			BeginAdditive(spriteBatch);

			const int Rings = 3;
			const int Dots = 64;

			for (int slot = 0; slot < Rings; slot++) {
				float seed = slot * 47.31f;
				float majorR = 42f + slot * 15f;
				float minorR = majorR * (0.44f + slot * 0.06f);

				float rotDir = (slot % 2 == 0) ? 1f : -1f;
				float rotSpeed = rotDir * (1.35f + slot * 0.45f);
				float baseAngle = time * rotSpeed + seed * 2.09f;

				float tiltBase = seed * 1.618f + MathHelper.PiOver4 * slot;
				float tiltWobble = (float)Math.Sin(time * (0.26f + slot * 0.09f) + seed)
					* MathHelper.ToRadians(20f);
				float tiltAngle = tiltBase + tiltWobble;

				// 弧更长、更亮，整体偏红
				float arcSpan = MathHelper.ToRadians(158f + slot * 16f);
				float alpha = MathHelper.Lerp(0.72f, 1.35f, intensity);
				float pulse = 0.82f + 0.18f * (float)Math.Sin(time * 6.5f + slot * 1.7f);

				for (int i = 0; i < Dots; i++) {
					float t = i / (float)(Dots - 1);
					float angle = baseAngle - t * arcSpan;

					Vector2 local = new Vector2(
						(float)Math.Cos(angle) * majorR,
						(float)Math.Sin(angle) * minorR
					).RotatedBy(tiltAngle);

					Vector2 screenPos = npc.Center + local - Main.screenPosition;

					float headness = 1f - t;
					float dotAlpha = headness * headness * alpha * pulse;
					if (dotAlpha < 0.02f)
						continue;

					float outerScale = (0.26f - slot * 0.025f) * (0.65f + headness * 0.45f);
					Main.EntitySpriteDraw(glow, screenPos, null,
						new Color(255, 48, 48) * dotAlpha * 0.62f,
						0f, glow.Size() * 0.5f, outerScale, SpriteEffects.None, 0);

					float innerScale = outerScale * 0.52f;
					Main.EntitySpriteDraw(glow, screenPos, null,
						new Color(220, 12, 28) * dotAlpha * 0.95f,
						0f, glow.Size() * 0.5f, innerScale, SpriteEffects.None, 0);

					Main.EntitySpriteDraw(glow, screenPos, null,
						new Color(255, 120, 120) * dotAlpha * 0.28f,
						0f, glow.Size() * 0.5f, outerScale * 1.55f, SpriteEffects.None, 0);
				}

				float headAngle = baseAngle;
				Vector2 headLocal = new Vector2(
					(float)Math.Cos(headAngle) * majorR,
					(float)Math.Sin(headAngle) * minorR
				).RotatedBy(tiltAngle);
				Vector2 headScreen = npc.Center + headLocal - Main.screenPosition;
				float ha = alpha * pulse * (0.85f + 0.15f * (float)Math.Sin(time * 8f + slot));

				Main.EntitySpriteDraw(glow, headScreen, null,
					new Color(255, 90, 90) * ha * 0.85f, 0f,
					glow.Size() * 0.5f, 0.40f, SpriteEffects.None, 0);
				Main.EntitySpriteDraw(glow, headScreen, null,
					new Color(255, 210, 210) * ha * 0.35f, 0f,
					glow.Size() * 0.5f, 0.16f, SpriteEffects.None, 0);
			}

			DrawBossDiamondSigil(npc, glow, intensity);
			EndAdditive(spriteBatch);
		}

		public static bool DrawGravityDaggerTrail(Projectile projectile)
		{
			if (Main.dedServ)
				return true;

			Texture2D texture = ModContent.Request<Texture2D>("ArknightsMod/Content/NPCs/Enemy/ThroughChapter4/GravityDagger_Barrage").Value;
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Vector2 drawOrigin = texture.Size() / 2f;
			Vector2 tailLocalOffset = new Vector2(-texture.Width * 0.42f, 0f);

			List<Vector2> spine = BuildProjectileSpine(projectile, tailLocalOffset);
			if (spine.Count >= 3) {
				// 非加法混合 + 顶点 Alpha 淡出：实色条带，避免半透明发灰
				BeginNonPremultiplied(Main.spriteBatch);
				DrawRibbonTaperFade(spine, 5f, 0.12f, new Color(255, 132, 44), new Color(255, 132, 44, 0), pixel);
				DrawRibbonTaperFade(spine, 3.2f, 0f, new Color(172, 22, 40), new Color(172, 22, 40, 0), pixel);
				EndNonPremultiplied(Main.spriteBatch);
			}

			Main.EntitySpriteDraw(
				texture,
				projectile.Center - Main.screenPosition,
				null,
				Color.White * projectile.Opacity,
				projectile.rotation,
				drawOrigin,
				projectile.scale,
				SpriteEffects.None,
				0f
			);

			return false;
		}

		private static bool IsCrownslayerDashRibbonSkill(Crownslayer.AIState currentAIState)
		{
			return currentAIState == Crownslayer.AIState.Skill_1
				|| currentAIState == Crownslayer.AIState.Skill_2
				|| currentAIState == Crownslayer.AIState.Skill_3
				|| currentAIState == Crownslayer.AIState.Skill_5;
		}

		// 二阶段（≤50% 血，与身边光点环绕同条件）：带状冲刺拖尾。
		private static bool ShouldDrawBossRibbonTrail(NPC npc, Crownslayer.NPCState currentAnimation, Crownslayer.AIState currentAIState)
		{
			if (npc.alpha > 190 || currentAnimation == Crownslayer.NPCState.Blank)
				return false;

			if (npc.life > npc.lifeMax / 2)
				return false;

			if (!IsCrownslayerDashRibbonSkill(currentAIState))
				return false;

			if (npc.velocity.LengthSquared() < 36f)
				return false;

			return true;
		}

		// 一/二阶段冲刺位移：残影（一阶段无带状拖尾，用残影+烟雾）。
		private static bool ShouldDrawBossAfterimages(NPC npc, Crownslayer.NPCState currentAnimation, Crownslayer.AIState currentAIState)
		{
			if (npc.alpha > 190 || currentAnimation == Crownslayer.NPCState.Blank)
				return false;

			if (npc.life <= npc.lifeMax / 2)
				return false;

			if (!IsCrownslayerDashRibbonSkill(currentAIState))
				return false;

			if (npc.velocity.LengthSquared() < 36f)
				return false;

			return true;
		}

		private static List<Vector2> BuildProjectileSpine(Projectile projectile, Vector2 localOffset)
		{
			List<Vector2> points = new();
			points.Add(projectile.Center + localOffset.RotatedBy(projectile.rotation));

			for (int i = 0; i < projectile.oldPos.Length; i++) {
				if (projectile.oldPos[i] == Vector2.Zero)
					break;

				float rotation = projectile.oldRot[i];
				if (rotation == 0f && projectile.velocity.LengthSquared() > 0.01f)
					rotation = projectile.velocity.ToRotation();

				points.Add(projectile.oldPos[i] + projectile.Size * 0.5f + localOffset.RotatedBy(rotation));
			}

			return points;
		}

		private static List<Vector2> BuildNpcTrailPoints(NPC npc, Vector2 localOffset)
		{
			List<Vector2> points = new();
			Vector2 previousCenter = npc.Center;
			points.Add(previousCenter + localOffset);
			for (int i = 0; i < npc.oldPos.Length; i++) {
				if (npc.oldPos[i] == Vector2.Zero)
					break;

				Vector2 oldCenter = npc.oldPos[i] + npc.Size * 0.5f;
				if (Vector2.DistanceSquared(previousCenter, oldCenter) > MaxContinuousNpcTrailSegmentSq)
					break;

				points.Add(oldCenter + localOffset);
				previousCenter = oldCenter;
			}
			return points;
		}

		private static Vector2 SampleNpcHistoryCenter(NPC npc, float factor)
		{
			if (npc.oldPos == null || npc.oldPos.Length == 0)
				return npc.Center;

			float scaled = factor * (npc.oldPos.Length - 1);
			int index = (int)scaled;
			float lerp = scaled - index;

			Vector2 current = index <= 0 || npc.oldPos[index - 1] == Vector2.Zero
				? npc.Center
				: npc.oldPos[index - 1] + npc.Size * 0.5f;
			Vector2 previous = index >= npc.oldPos.Length || npc.oldPos[index] == Vector2.Zero
				? current
				: npc.oldPos[index] + npc.Size * 0.5f;

			return Vector2.Lerp(current, previous, lerp);
		}

		private static void DrawBossDiamondSigil(NPC npc, Texture2D glowTexture, float intensity)
		{
			float time = Main.GlobalTimeWrappedHourly;
			Vector2 center = npc.Center - Main.screenPosition + new Vector2(0f, -2f);
			float baseRotation = time * 0.75f;
			float pulse = 0.88f + 0.12f * (float)Math.Sin(time * 4.2f);
			Color outer = new Color(220, 16, 32) * (0.22f + intensity * 0.24f);
			Color inner = new Color(255, 72, 72) * (0.14f + intensity * 0.18f);

			for (int i = 0; i < 4; i++) {
				float rot = baseRotation + MathHelper.PiOver2 * i;
				Vector2 offset = rot.ToRotationVector2() * 26f;
				Main.EntitySpriteDraw(
					glowTexture,
					center + offset,
					null,
					outer,
					rot + MathHelper.PiOver4,
					glowTexture.Size() / 2f,
					new Vector2(0.22f, 0.08f) * pulse,
					SpriteEffects.None,
					0f
				);
			}

			Main.EntitySpriteDraw(
				glowTexture,
				center,
				null,
				inner,
				baseRotation + MathHelper.PiOver4,
				glowTexture.Size() / 2f,
				new Vector2(0.16f, 0.16f) * pulse,
				SpriteEffects.None,
				0f
			);
		}

		public static void DrawScreenFogOverlay(SpriteBatch spriteBatch)
		{
			if (Main.gameMenu || Main.netMode == NetmodeID.Server)
				return;

			UpdateFogOverlaySmoothing();

			if (SmoothedFogIntensity <= 0.008f)
				return;

			DrawGraveyardStyleRedFog(spriteBatch, SmoothedFogIntensity);
		}

		private static float SmoothedFogIntensity;
		private const float FogFadeInSpeed = 0.028f;
		private const float FogFadeOutSpeed = 0.022f;

		// 雾气贴图：Content/NPCs/Enemy/ThroughChapter4/CrownslayerFog_1.png ~ _4.png
		private const int CrownslayerFogTextureCount = 4;
		private static readonly string[] CrownslayerFogTexturePaths = {
			"ArknightsMod/Content/NPCs/Enemy/ThroughChapter4/CrownslayerFog_1",
			"ArknightsMod/Content/NPCs/Enemy/ThroughChapter4/CrownslayerFog_2",
			"ArknightsMod/Content/NPCs/Enemy/ThroughChapter4/CrownslayerFog_3",
			"ArknightsMod/Content/NPCs/Enemy/ThroughChapter4/CrownslayerFog_4",
		};

		private static Texture2D[] _crownslayerFogTextures;
		private static bool _crownslayerFogTexturesReady;

		private readonly struct ScreenFogSpriteSlot
		{
			public readonly int TextureIndex;
			public readonly float AnchorX;
			public readonly float AnchorY;
			public readonly float Scale;
			public readonly float MoveRangeX;
			public readonly float MoveRangeY;
			public readonly float MoveSpeed;
			public readonly float Alpha;
			public readonly float Phase;

			public ScreenFogSpriteSlot(
				int textureIndex, float anchorX, float anchorY,
				float scale, float moveRangeX, float moveRangeY,
				float moveSpeed, float alpha, float phase)
			{
				TextureIndex = textureIndex;
				AnchorX = anchorX;
				AnchorY = anchorY;
				Scale = scale;
				MoveRangeX = moveRangeX;
				MoveRangeY = moveRangeY;
				MoveSpeed = moveSpeed;
				Alpha = alpha;
				Phase = phase;
			}
		}

		// 左上 + 下方：大量重叠锚点，尺寸/漂移/透明度各不相同
		private static readonly ScreenFogSpriteSlot[] ScreenFogSpriteSlots = BuildScreenFogSpriteSlots();

		private static ScreenFogSpriteSlot[] BuildScreenFogSpriteSlots()
		{
			var slots = new List<ScreenFogSpriteSlot>(40);

			for (int i = 0; i < 20; i++) {
				int tex = i % CrownslayerFogTextureCount;
				int gx = i % 5;
				int gy = i / 5;
				float anchorX = 0.01f + gx * 0.052f;
				float anchorY = 0.02f + gy * 0.038f;
				float scale = 2.4f + (i % 4) * 1.15f + tex * 0.4f;
				float alpha = 0.48f + (i % 5) * 0.11f;
				float moveX = 10f + (i % 6) * 9f;
				float moveY = 6f + (i % 4) * 7f;
				float speed = 0.18f + (i % 7) * 0.06f;
				slots.Add(new ScreenFogSpriteSlot(tex, anchorX, anchorY, scale, moveX, moveY, speed, alpha, i * 0.71f));
			}

			for (int i = 0; i < 20; i++) {
				int tex = (i + 2) % CrownslayerFogTextureCount;
				int gx = i % 5;
				int gy = i / 5;
				float anchorX = 0.00f + gx * 0.056f;
				float anchorY = 0.66f + gy * 0.068f;
				float scale = 2.6f + (i % 3) * 1.25f + tex * 0.35f;
				float alpha = 0.52f + (i % 4) * 0.10f;
				float moveX = 14f + (i % 5) * 10f;
				float moveY = 8f + (i % 3) * 8f;
				float speed = 0.20f + (i % 6) * 0.065f;
				slots.Add(new ScreenFogSpriteSlot(tex, anchorX, anchorY, scale, moveX, moveY, speed, alpha, 11.3f + i * 0.67f));
			}

			return slots.ToArray();
		}

		// 自定义雾贴图区域可见度（不跟景深中心衰减绑死）。
		private static float GetFogSpriteZoneVisibility(float normX, float normY)
		{
			if (normX < 0.50f && normY < 0.40f)
				return MathHelper.Clamp(0.78f + (0.40f - normY) * 0.55f, 0.78f, 1f);

			if (normY > 0.56f && normX < 0.55f)
				return MathHelper.Clamp(0.75f + MathHelper.SmoothStep(0.56f, 1f, normY) * 0.25f, 0.75f, 1f);

			return 0.5f;
		}

		private static void EnsureCrownslayerFogTextures()
		{
			if (_crownslayerFogTexturesReady)
				return;

			_crownslayerFogTexturesReady = true;
			_crownslayerFogTextures = new Texture2D[CrownslayerFogTextureCount];
			for (int i = 0; i < CrownslayerFogTextureCount; i++) {
				string path = CrownslayerFogTexturePaths[i];
				if (!ModContent.HasAsset(path))
					continue;

				_crownslayerFogTextures[i] = ModContent.Request<Texture2D>(path).Value;
			}
		}

		// 每帧平滑雾强：进入二阶段淡入，Boss 死亡/离场后淡出。
		public static void UpdateFogOverlaySmoothing()
		{
			float target = GetFogOverlayTargetIntensity();
			float speed = target > SmoothedFogIntensity ? FogFadeInSpeed : FogFadeOutSpeed;
			SmoothedFogIntensity = MathHelper.Lerp(SmoothedFogIntensity, target, speed);

			if (SmoothedFogIntensity < 0.001f && target <= 0f)
				SmoothedFogIntensity = 0f;
		}

		private static float GetFogOverlayTargetIntensity()
		{
			float maxIntensity = 0f;
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || npc.type != ModContent.NPCType<Crownslayer>())
					continue;

				if (npc.ModNPC is Crownslayer crownslayer)
					maxIntensity = Math.Max(maxIntensity, crownslayer.grayScaleIntensity);
			}

			return MathHelper.Clamp(maxIntensity, 0f, 1f);
		}

		// 景深：中心战斗区清晰，越靠屏幕边缘（尤其上下）雾越浓。
		private static float GetDepthFogFactor(float normalizedY, float normalizedX = 0.5f)
		{
			const float focusY = 0.40f;
			const float focusX = 0.50f;
			float dy = Math.Abs(normalizedY - focusY) / 0.50f;
			float dx = Math.Abs(normalizedX - focusX) / 0.55f;
			float dist = (float)Math.Sqrt(dx * dx + dy * dy);
			return MathHelper.Clamp((dist - 0.12f) / 0.78f, 0f, 1f);
		}

		// 上下缘雾气积压权重（仿墓地贴地/天际雾层）。
		private static float GetEdgePoolFactor(float normalizedY)
		{
			if (normalizedY >= 0.58f)
				return MathHelper.SmoothStep(0.58f, 1f, normalizedY);
			if (normalizedY <= 0.28f)
				return MathHelper.SmoothStep(0.28f, 0f, normalizedY) * 0.75f;
			return 0f;
		}

		private static void DrawFogAccumulationBank(
			SpriteBatch spriteBatch, Texture2D pixel, int w, int h, float intensity,
			float yStartRatio, float yEndRatio, bool isBottom, Color nearColor, Color farColor)
		{
			const int bands = 12;
			for (int i = 0; i < bands; i++) {
				float t = i / (float)(bands - 1);
				float yRatio = MathHelper.Lerp(yStartRatio, yEndRatio, t);
				int bandTop = (int)(h * yRatio);
				int bandH = (int)Math.Max(6f, h * (yEndRatio - yStartRatio) / bands * 1.15f);

				float normY = yRatio;
				float depth = GetDepthFogFactor(normY);
				float edge = GetEdgePoolFactor(normY);
				float bankStrength = isBottom
					? MathHelper.Lerp(0.08f, 0.42f, t * t)
					: MathHelper.Lerp(0.35f, 0.06f, t);

				float alpha = bankStrength * intensity * MathHelper.Lerp(0.25f, 1f, depth + edge * 0.65f);
				if (alpha < 0.003f)
					continue;

				Color bandColor = Color.Lerp(nearColor, farColor, t) * alpha;
				spriteBatch.Draw(pixel, new Rectangle(0, bandTop, w, bandH), bandColor);
			}
		}

		// 仿原版墓地雾气：上下积压 + 景深（中心淡、边缘浓），红色调。
		private static void DrawGraveyardStyleRedFog(SpriteBatch spriteBatch, float intensity)
		{
			int w = Main.screenWidth;
			int h = Main.screenHeight;
			float time = Main.GlobalTimeWrappedHourly;
			float wind = Main.windSpeedCurrent;
			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Texture2D soft = TextureAssets.Extra[98].Value;

			intensity = MathHelper.Clamp(intensity, 0f, 1f);

			// 1. 仅边缘区域的轻微色偏（非全屏 wash，中心几乎不受影响）
			const int washBands = 6;
			for (int i = 0; i < washBands; i++) {
				float t = i / (float)(washBands - 1);
				float yRatio = MathHelper.Lerp(0f, 1f, t);
				float depth = GetDepthFogFactor(yRatio);
				float edge = GetEdgePoolFactor(yRatio);
				float washAlpha = (0.02f + intensity * 0.07f) * MathHelper.Lerp(depth, 1f, edge * 0.5f);
				if (washAlpha < 0.004f)
					continue;

				int bandTop = (int)(h * t);
				int bandH = (int)Math.Max(4f, h / washBands);
				spriteBatch.Draw(pixel, new Rectangle(0, bandTop, w, bandH),
					new Color(42, 8, 12) * washAlpha);
			}

			// 2. 屏幕下方雾气积压（主雾层，最重）
			DrawFogAccumulationBank(spriteBatch, pixel, w, h, intensity,
				0.52f, 1.02f, isBottom: true,
				new Color(72, 14, 18), new Color(118, 26, 32));

			// 3. 屏幕上方雾气积压（较轻，仿天际灰雾）
			DrawFogAccumulationBank(spriteBatch, pixel, w, h, intensity * 0.72f,
				-0.02f, 0.30f, isBottom: false,
				new Color(52, 10, 14), new Color(88, 20, 24));

			// 4. 云絮：主要分布在上下缘，中心区域稀疏
			int cloudCount = TextureAssets.Cloud.Length;
			if (cloudCount > 0) {
				for (int i = 0; i < 18; i++) {
					float layer = i / 17f;
					bool topBand = i % 3 == 0;
					float yBase = topBand
						? MathHelper.Lerp(0.04f, 0.26f, layer)
						: MathHelper.Lerp(0.62f, 0.94f, layer);

					float normY = yBase;
					float depth = GetDepthFogFactor(normY);
					float edge = GetEdgePoolFactor(normY);
					float zoneAlpha = MathHelper.Lerp(depth, 1f, edge * 0.8f);
					if (zoneAlpha < 0.08f)
						continue;

					float drift = time * (5f + layer * 9f) + wind * (35f + layer * 28f) + i * 1.21f;
					float x = WrapScreenPosition(w * (0.04f + layer * 0.92f) + drift * 16f, w, 540f);
					float y = h * yBase + (float)Math.Sin(time * (0.32f + layer * 0.18f) + i) * (6f + layer * 12f);

					float scaleX = MathHelper.Lerp(5f, 12f, layer) + intensity * 3.5f;
					float scaleY = MathHelper.Lerp(0.4f, 1f, layer) + intensity * 0.3f;
					float alpha = MathHelper.Lerp(0.04f, 0.14f, layer) * intensity * zoneAlpha;

					Color wispColor = Color.Lerp(new Color(100, 28, 32), new Color(190, 55, 58), layer) * alpha;
					Texture2D cloudTex = TextureAssets.Cloud[i % cloudCount].Value;
					Vector2 origin = cloudTex.Size() * 0.5f;
					float rot = (float)Math.Sin(time * 0.16f + i) * 0.05f;

					spriteBatch.Draw(cloudTex, new Vector2(x, y), null, wispColor, rot, origin,
						new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
				}
			}

			// 5. 上下缘软光斑（加厚积压感）
			for (int i = 0; i < 10; i++) {
				bool top = i < 4;
				float layer = top ? i / 3f : (i - 4) / 5f;
				float yBase = top ? MathHelper.Lerp(0.02f, 0.22f, layer) : MathHelper.Lerp(0.68f, 0.98f, layer);
				float normY = yBase;
				float zoneAlpha = MathHelper.Lerp(GetDepthFogFactor(normY), 1f, GetEdgePoolFactor(normY) * 0.7f);
				if (zoneAlpha < 0.1f)
					continue;

				float drift = time * (3.5f + layer * 5f) - wind * 22f + i * 1.9f;
				float x = WrapScreenPosition(w * (0.08f + layer * 0.84f) + drift * 20f, w, 500f);
				float y = h * yBase + (float)Math.Cos(time * 0.24f + i) * 8f;

				float scale = MathHelper.Lerp(2.5f, 6f, layer) + intensity * 2f;
				float alpha = MathHelper.Lerp(0.03f, 0.09f, layer) * intensity * zoneAlpha;
				Color softColor = Color.Lerp(new Color(130, 26, 30), new Color(210, 65, 68), layer) * alpha;

				spriteBatch.Draw(soft, new Vector2(x, y), null, softColor, 0f, soft.Size() * 0.5f,
					scale, SpriteEffects.None, 0f);
			}

			DrawCrownslayerFogSprites(spriteBatch, intensity, w, h, time);
		}

		private static void DrawCrownslayerFogSprites(
			SpriteBatch spriteBatch, float intensity, int w, int h, float time)
		{
			EnsureCrownslayerFogTextures();

			bool anyTexture = false;
			for (int t = 0; t < CrownslayerFogTextureCount; t++) {
				if (_crownslayerFogTextures[t] != null) {
					anyTexture = true;
					break;
				}
			}

			if (!anyTexture)
				return;

			float screenRef = Math.Max(w, h) / 1080f;
			float intensityBoost = MathHelper.Clamp(Math.Max(intensity, 0.55f), 0.55f, 1f);

			for (int i = 0; i < ScreenFogSpriteSlots.Length; i++) {
				ScreenFogSpriteSlot slot = ScreenFogSpriteSlots[i];
				Texture2D tex = _crownslayerFogTextures[slot.TextureIndex];
				if (tex == null)
					continue;

				float zoneVis = GetFogSpriteZoneVisibility(slot.AnchorX, slot.AnchorY);
				float driftX = (float)Math.Sin(time * slot.MoveSpeed + slot.Phase) * slot.MoveRangeX;
				float driftY = (float)Math.Cos(time * slot.MoveSpeed * 0.74f + slot.Phase * 1.17f) * slot.MoveRangeY;
				Vector2 basePos = new Vector2(w * slot.AnchorX + driftX, h * slot.AnchorY + driftY);
				float scale = slot.Scale * screenRef * (1.25f + intensityBoost * 0.95f);
				float alpha = MathHelper.Clamp(slot.Alpha * intensityBoost * zoneVis, 0.28f, 0.92f);

				Color tint = Color.Lerp(Color.White, new Color(255, 210, 212), slot.TextureIndex / 3f);

				// 双 pass：同位置略偏移叠两层，增强重叠区域的厚度
				for (int pass = 0; pass < 2; pass++) {
					Vector2 offset = pass == 0
						? Vector2.Zero
						: new Vector2(18f + (i % 3) * 6f, 12f + (i % 4) * 5f);
					float passAlpha = pass == 0 ? alpha : alpha * 0.62f;
					Color drawColor = tint * passAlpha;

					spriteBatch.Draw(tex, basePos + offset, null, drawColor, 0f, tex.Size() * 0.5f,
						scale * (pass == 0 ? 1f : 1.08f), SpriteEffects.None, 0f);
				}
			}
		}

		private static float WrapScreenPosition(float position, int screenSize, float padding)
		{
			float span = screenSize + padding * 2f;
			while (position < -padding)
				position += span;
			while (position > screenSize + padding)
				position -= span;
			return position;
		}

		private static void DrawBossHelixTrail(List<Vector2> trailPoints, float time, float alphaScale = 1f)
		{
			if (trailPoints.Count < 4)
				return;

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			int totalPts = trailPoints.Count;
			const int Samples = 36;
			const float Freq = 4.2f;
			const float Amp = 12f;
			float speed = time * 14f;
			alphaScale = MathHelper.Clamp(alphaScale, 0f, 1f);

			for (int strand = 0; strand < 2; strand++) {
				float phaseShift = strand * MathHelper.Pi;
				var helixPts = new List<Vector2>(Samples);

				for (int i = 0; i < Samples; i++) {
					float t = i / (float)(Samples - 1);
					float rawIdx = t * (totalPts - 1);
					int lo = (int)rawIdx;
					int hi = Math.Min(lo + 1, totalPts - 1);
					float frac = rawIdx - lo;
					Vector2 basePos = Vector2.Lerp(trailPoints[lo], trailPoints[hi], frac);

					Vector2 tangent;
					if (lo < totalPts - 1)
						tangent = trailPoints[hi] - trailPoints[lo];
					else
						tangent = trailPoints[lo] - trailPoints[lo - 1];
					if (tangent.LengthSquared() < 0.001f)
						tangent = Vector2.UnitX;
					tangent = tangent.SafeNormalize(Vector2.UnitX);
					Vector2 normal = new Vector2(-tangent.Y, tangent.X);

					float tailFade = (1f - t) * (1f - t);
					float sine = (float)Math.Sin(t * Freq * MathHelper.TwoPi - speed + phaseShift);
					helixPts.Add(basePos + normal * sine * Amp * tailFade);
				}

				Color headCol = strand == 0
					? new Color(255, 110, 70)
					: new Color(220, 28, 48);
				headCol *= alphaScale;
				Color tailCol = new Color(headCol.R, headCol.G, headCol.B, 0);

				DrawRibbonTaperFade(helixPts, 2.6f, 0f, headCol, tailCol, pixel);
			}
		}

		private static void DrawRibbonTaperFade(List<Vector2> points, float headWidth, float tailWidth, Color headColor, Color tailColor, Texture2D texture)
		{
			if (points.Count < 3 || texture == null)
				return;

			List<TrailMaker.CustomVertexInfo> bars = new();
			for (int i = 1; i < points.Count; i++) {
				Vector2 current = points[i];
				Vector2 previous = points[i - 1];
				Vector2 segment = previous - current;
				if (segment.LengthSquared() < 0.001f)
					continue;

				Vector2 normal = Vector2.Normalize(new Vector2(-segment.Y, segment.X));
				float factor = i / (float)(points.Count - 1);
				float width = MathHelper.Lerp(headWidth, tailWidth, factor);
				float fade = 1f - factor;
				fade *= fade;

				Color color = Color.Lerp(headColor, tailColor, factor);
				// 仅用 Alpha 做尾部消失；不再整体压暗 RGB（否则会与贴图暗部相乘形成大块黑色）
				color.A = (byte)MathHelper.Clamp((int)(255f * fade), 0, 255);

				bars.Add(new TrailMaker.CustomVertexInfo(current + normal * width - Main.screenPosition, color, new Vector3(factor, 1f, MathHelper.Lerp(1f, 0.08f, factor))));
				bars.Add(new TrailMaker.CustomVertexInfo(current - normal * width - Main.screenPosition, color, new Vector3(factor, 0f, MathHelper.Lerp(1f, 0.08f, factor))));
			}

			if (bars.Count < 4)
				return;

			List<TrailMaker.CustomVertexInfo> vertices = new();
			for (int i = 0; i < bars.Count - 2; i += 2) {
				vertices.Add(bars[i]);
				vertices.Add(bars[i + 2]);
				vertices.Add(bars[i + 1]);
				vertices.Add(bars[i + 1]);
				vertices.Add(bars[i + 2]);
				vertices.Add(bars[i + 3]);
			}

			Main.graphics.GraphicsDevice.Textures[0] = texture;
			Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3);
		}

		private static void DrawRibbon(List<Vector2> points, float headWidth, float tailWidth, Color headColor, Color tailColor, Texture2D texture = null)
		{
			if (points.Count < 3)
				return;

			List<TrailMaker.CustomVertexInfo> bars = new();
			for (int i = 1; i < points.Count; i++) {
				Vector2 current = points[i];
				Vector2 previous = points[i - 1];
				Vector2 segment = previous - current;
				if (segment.LengthSquared() < 0.001f)
					continue;

				Vector2 normal = Vector2.Normalize(new Vector2(-segment.Y, segment.X));
				float factor = i / (float)(points.Count - 1);
				float width = MathHelper.Lerp(headWidth, tailWidth, factor);
				Color color = Color.Lerp(headColor, tailColor, factor);
				color.A = 0;

				bars.Add(new TrailMaker.CustomVertexInfo(current + normal * width - Main.screenPosition, color, new Vector3(factor, 1f, MathHelper.Lerp(1f, 0.1f, factor))));
				bars.Add(new TrailMaker.CustomVertexInfo(current - normal * width - Main.screenPosition, color, new Vector3(factor, 0f, MathHelper.Lerp(1f, 0.1f, factor))));
			}

			if (bars.Count < 4)
				return;

			List<TrailMaker.CustomVertexInfo> vertices = new();
			for (int i = 0; i < bars.Count - 2; i += 2) {
				vertices.Add(bars[i]);
				vertices.Add(bars[i + 2]);
				vertices.Add(bars[i + 1]);
				vertices.Add(bars[i + 1]);
				vertices.Add(bars[i + 2]);
				vertices.Add(bars[i + 3]);
			}

			Main.graphics.GraphicsDevice.Textures[0] = texture ?? ModContent.Request<Texture2D>("ArknightsMod/Common/VisualEffects/LineTrail").Value;
			Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, vertices.ToArray(), 0, vertices.Count / 3);
		}

		private static void BeginAdditive(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.Additive,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				Main.Rasterizer,
				null,
				Main.GameViewMatrix.TransformationMatrix);
		}

		private static void BeginNonPremultiplied(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.NonPremultiplied,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				Main.Rasterizer,
				null,
				Main.GameViewMatrix.TransformationMatrix);
		}

		private static void EndAdditive(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				Main.Rasterizer,
				null,
				Main.GameViewMatrix.TransformationMatrix);
		}

		private static void EndNonPremultiplied(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			spriteBatch.Begin(
				SpriteSortMode.Deferred,
				BlendState.AlphaBlend,
				Main.DefaultSamplerState,
				DepthStencilState.None,
				Main.Rasterizer,
				null,
				Main.GameViewMatrix.TransformationMatrix);
		}
	}

	public class CrownslayerFogOverlaySystem : ModSystem
	{
		public override void PostUpdateEverything()
		{
			CrownslayerTrailEffects.UpdateFogOverlaySmoothing();
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
			if (mouseTextIndex == -1)
				return;

			layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer(
				"ArknightsMod:CrownslayerFogOverlay",
				delegate {
					CrownslayerTrailEffects.DrawScreenFogOverlay(Main.spriteBatch);
					return true;
				},
				InterfaceScaleType.UI));
		}
	}
}
