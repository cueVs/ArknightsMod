using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Common.VisualEffects;

namespace ArknightsMod.Content.Projectiles.Medic.ReedFlameShadow
{
	// ============================================================
	//  焰影苇草 法杖普攻弹幕 —— 彗星型火流
	//
	//  整条弹幕是**一个连续的顶点条带**（不是"弹头 sprite + 拖尾"两件东西）：
	//    · 鼻端最宽最亮（着色器修圆头 + 白热芯 + 黄色外发光边缘）
	//    · 沿长度渐变 白 → 黄 → 褐（不经过红色）
	//    · 尾部低频摆动，振幅随进度平方增长，头部保持稳定
	//    · 尾端噪声溶解成火舌，中前段恒定不透明（不闪）
	//
	//  ⚠ 为什么自己维护 _history 而不用 Projectile.oldPos：
	//    oldPos 里相邻项可能重合或极近（弹幕减速时、或 TrailingMode 的记录时机），
	//    相减得到零/近零向量，Normalize() 后方向乱跳 → 法线乱翻 → 三角带自交成
	//    蝴蝶结，整条外形扭曲乱掉。自己按"走够最小间距才记一个点"来采样，
	//    从源头保证不会出现退化线段。改动这里时不要改回 oldPos。
	// ============================================================
	public class ReedFlameShadowFlame : ModProjectile
	{
		private const string NoiseTexPath = "ArknightsMod/Content/Projectiles/Rogue/FireworksHand/NoiseTexture";

		// ── 视觉微调入口 ──────────────────────────────────
		private const int   HistoryLength   = 26;    // 记录多少个路径点（越大拖尾越长）
		private const float MinPointDist    = 7f;    // 走够这么多像素才记一个新点（去重，保证线段不退化）
		private const int   SubDivide       = 3;     // 每段细分几份（曲线更顺、摆动更细腻）

		// 宽度拆成两个分量叠加：细长的身体 + 头部的光团。
		// 这样身体可以很细（"一条"），同时头部仍有一个明显更粗的发光头。
		private const float BodyWidth       = 9f;    // 身体半宽（像素）—— 调"细不细"看这个
		private const float HeadExtraWidth  = 8f;    // 鼻端额外加宽（头部光团大小）
		private const float HeadBulgeLength = 0.16f; // 头部光团沿长度延伸多远（0~1）
		private const float TaperPower      = 0.62f; // 身体收窄曲线；<1 前段更饱满
		private const float TailMinWidth    = 0.22f; // 尾端保留的最小宽度比例

		private const float WobbleAmplitude = 3.2f;  // 尾部摆动振幅（像素）
		private const float WobbleSpeed     = 2.6f;
		private const float WobbleStart     = 0.34f; // 这之前完全不摆，保证头部形状明确

		private const float FlowScrollSpeed = 0.85f; // 噪声滚动速度（太快会显得躁动）
		private const float DissolveAmount  = 0.55f; // 尾端溶解强度
		private const float DissolveStart   = 0.55f; // 从哪开始允许溶解（之前恒不透明）
		private const float NoiseScaleX     = 2.0f;
		private const float NoiseScaleY     = 1.0f;
		private const float TrailIntensity  = 1.2f;
		private const float HeadRound       = 0.09f;
		private const float PixelWarp       = 0.10f;
		private const float TailBrightness  = 0.26f; // 尾部亮度下限，防止后半段整个消失

		// 发光集中度：头部爆亮、身体只是"一条"，不整条泛光
		private const float GlowLength      = 0.20f; // 头部发光沿长度延伸多远
		private const float HeadHeat        = 2.1f;  // 鼻端亮度倍率
		private const float BodyHeat        = 0.62f; // 身体/尾部亮度倍率

		// 配色：白 → 黄 → 褐，不含红
		private static readonly Vector3 HeadColor = new(1.00f, 0.96f, 0.86f);
		private static readonly Vector3 MidColor  = new(1.00f, 0.76f, 0.22f);
		private static readonly Vector3 TailColor = new(0.45f, 0.22f, 0.07f);
		private static readonly Vector3 EdgeTint  = new(1.00f, 0.68f, 0.24f);

		// 自己维护的路径历史（[0] = 最新 = 弹头）
		private readonly List<Vector2> _history = new();

		private Vector2 flowOffset;
		private Vector2 noiseSeed;
		private float wobblePhase;

		public override string Texture => "ArknightsMod/Content/Items/Weapons/Medic/ReedFlameShadow/ReedFlameShadowStaff";

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 3;
			Projectile.timeLeft = 240;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 12;
			Projectile.light = 0.7f;
		}

		public override void OnSpawn(Terraria.DataStructures.IEntitySource source) {
			noiseSeed = new Vector2(Main.rand.NextFloat(), Main.rand.NextFloat());
			wobblePhase = Main.rand.NextFloat(MathHelper.TwoPi);
			_history.Clear();
			_history.Add(Projectile.Center);
		}

		public override void AI() {
			Projectile.rotation = Projectile.velocity.ToRotation();

			RecordPath();

			flowOffset.X -= FlowScrollSpeed * 0.016f;
			flowOffset.Y += 0.02f * 0.016f;
			wobblePhase += WobbleSpeed * 0.016f;

			if (Projectile.velocity.Length() < 15f)
				Projectile.velocity *= 1.012f;
			Projectile.velocity.Y += 0.035f;

			SpawnEmbers();
			Lighting.AddLight(Projectile.Center, 1.0f, 0.62f, 0.18f);
		}

		// 只有走够 MinPointDist 才记新点：从源头杜绝重合点导致的退化线段
		private void RecordPath() {
			Vector2 now = Projectile.Center;
			if (_history.Count == 0) {
				_history.Add(now);
				return;
			}

			if (Vector2.DistanceSquared(now, _history[0]) >= MinPointDist * MinPointDist) {
				_history.Insert(0, now);
				if (_history.Count > HistoryLength)
					_history.RemoveAt(_history.Count - 1);
			}
			else {
				// 没走够距离时只更新最新点，保持弹头贴合当前位置
				_history[0] = now;
			}
		}

		private void SpawnEmbers() {
			if (Main.dedServ)
				return;

			if (Main.rand.NextBool(2)) {
				Dust d = Dust.NewDustDirect(
					Projectile.position, Projectile.width, Projectile.height,
					DustID.GoldFlame, 0f, 0f, 100,
					default, Main.rand.NextFloat(0.8f, 1.4f));
				d.noGravity = true;
				d.velocity = -Projectile.velocity * Main.rand.NextFloat(0.02f, 0.08f)
					+ Main.rand.NextVector2Circular(1.0f, 1.0f);
				d.fadeIn = 0.8f;
			}

			if (Main.rand.NextBool(6)) {
				Dust e = Dust.NewDustDirect(
					Projectile.position, Projectile.width, Projectile.height,
					DustID.Torch, 0f, 0f, 160,
					default, Main.rand.NextFloat(0.6f, 1.0f));
				e.noGravity = true;
				e.velocity = -Projectile.velocity * 0.06f + Main.rand.NextVector2Circular(0.7f, 0.7f);
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.OnFire, 240);
			FlameBurst(14);
		}

		public override void OnKill(int timeLeft) {
			FlameBurst(20);
			SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.35f, Pitch = 0.45f }, Projectile.Center);
		}

		private void FlameBurst(int count) {
			if (Main.dedServ)
				return;

			for (int i = 0; i < count; i++) {
				Dust d = Dust.NewDustDirect(
					Projectile.position, Projectile.width, Projectile.height,
					Main.rand.NextBool(3) ? DustID.Torch : DustID.GoldFlame,
					0f, 0f, 100, default, Main.rand.NextFloat(1.0f, 1.7f));
				d.noGravity = true;
				d.velocity = Main.rand.NextVector2Circular(4.5f, 4.5f);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			DrawFlameBody();
			return false;
		}

		// 两层不同频率的正弦叠加 = 有机摆动（层数比之前少，形状更明确不躁）
		private float Wobble(float along) {
			return MathF.Sin(along * 5.4f - wobblePhase) * 0.72f
				 + MathF.Sin(along * 11.2f - wobblePhase * 1.6f) * 0.28f;
		}

		// ── 整条火流：一个连续顶点条带 ─────────────────────
		private void DrawFlameBody() {
			Effect fx = ArknightsMod.ReedFlameTrail?.Value;
			Texture2D noiseTex = ModContent.Request<Texture2D>(NoiseTexPath).Value;
			if (fx == null || noiseTex == null || _history.Count < 2)
				return;

			// 1) 细分成更密的采样点
			var pts = new List<Vector2>((_history.Count - 1) * SubDivide + 1);
			for (int i = 0; i < _history.Count - 1; i++) {
				for (int s = 0; s < SubDivide; s++) {
					float t = s / (float)SubDivide;
					pts.Add(Vector2.Lerp(_history[i], _history[i + 1], t));
				}
			}
			pts.Add(_history[^1]);
			if (pts.Count < 3)
				return;

			// 2) 生成条带顶点
			var bars = new List<Vertex>(pts.Count * 2);
			int last = pts.Count - 1;
			Vector2 prevNormal = Vector2.Zero;

			for (int i = 0; i <= last; i++) {
				float along = i / (float)last;   // 0 弹头 → 1 尾端

				// 切线：用前后各一点的中心差分，比只看相邻两点稳得多
				int a = Math.Max(i - 1, 0);
				int b = Math.Min(i + 1, last);
				Vector2 tangent = pts[a] - pts[b];

				if (tangent.LengthSquared() < 0.0001f) {
					// 极端退化：沿用上一段的法线，绝不让 Normalize 产生乱跳方向
					if (prevNormal == Vector2.Zero)
						continue;
					AddPair(bars, pts[i], prevNormal, along);
					continue;
				}

				tangent.Normalize();
				Vector2 normal = new(-tangent.Y, tangent.X);

				// 防止法线突然翻面导致三角带自交成蝴蝶结
				if (prevNormal != Vector2.Zero && Vector2.Dot(normal, prevNormal) < 0f)
					normal = -normal;
				prevNormal = normal;

				AddPair(bars, pts[i], normal, along);
			}

			if (bars.Count < 3)
				return;

			// 3) 绘制
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
				SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone);

			Matrix projection = Matrix.CreateOrthographicOffCenter(
				0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
			Matrix model = Matrix.CreateTranslation(
					new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0f))
				* Main.GameViewMatrix.ZoomMatrix;

			fx.Parameters["uTransform"]?.SetValue(model * projection);
			fx.Parameters["tex0"]?.SetValue(noiseTex);
			fx.Parameters["uFlowOffset"]?.SetValue(flowOffset + noiseSeed);
			fx.Parameters["uNoiseScale"]?.SetValue(new Vector2(NoiseScaleX, NoiseScaleY));
			fx.Parameters["uDissolveAmount"]?.SetValue(DissolveAmount);
			fx.Parameters["uDissolveStart"]?.SetValue(DissolveStart);
			fx.Parameters["uIntensity"]?.SetValue(TrailIntensity);
			fx.Parameters["uHeadRound"]?.SetValue(HeadRound);
			fx.Parameters["uPixelWarp"]?.SetValue(PixelWarp);
			fx.Parameters["uTailBrightness"]?.SetValue(TailBrightness);
			fx.Parameters["uGlowLength"]?.SetValue(GlowLength);
			fx.Parameters["uHeadHeat"]?.SetValue(HeadHeat);
			fx.Parameters["uBodyHeat"]?.SetValue(BodyHeat);
			fx.Parameters["uHeadColor"]?.SetValue(HeadColor);
			fx.Parameters["uMidColor"]?.SetValue(MidColor);
			fx.Parameters["uTailColor"]?.SetValue(TailColor);
			fx.Parameters["uEdgeTint"]?.SetValue(EdgeTint);
			fx.CurrentTechnique.Passes["FlameTrail"].Apply();

			Main.graphics.GraphicsDevice.DrawUserPrimitives(
				PrimitiveType.TriangleStrip, bars.ToArray(), 0, bars.Count - 2);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}

		// 在给定路径点处，按彗星宽度剖面 + 尾部摆动，生成条带两侧的一对顶点
		private void AddPair(List<Vertex> bars, Vector2 point, Vector2 normal, float along) {
			// 身体：细长、沿长度收窄
			float bodyTaper = MathF.Pow(1f - along, TaperPower);
			bodyTaper = MathHelper.Lerp(TailMinWidth, 1f, bodyTaper);
			// 头部光团：只在最前面一小段鼓起来，二次衰减收得紧
			float headBulge = MathHelper.Clamp(1f - along / HeadBulgeLength, 0f, 1f);
			headBulge *= headBulge;
			float w = BodyWidth * bodyTaper + HeadExtraWidth * headBulge;

			float wobbleWeight = MathHelper.Clamp((along - WobbleStart) / (1f - WobbleStart), 0f, 1f);
			float amp = WobbleAmplitude * wobbleWeight * wobbleWeight;
			Vector2 center = point + normal * Wobble(along) * amp;

			// 注意：TexCoord.z 这里传 1，不再传 taper——宽度衰减已经体现在几何上，
			// 着色器里不能再乘一遍，否则尾部会过早塌成看不见
			bars.Add(new Vertex(center + normal * w, new Vector3(along, 1f, 1f), Color.White));
			bars.Add(new Vertex(center - normal * w, new Vector3(along, 0f, 1f), Color.White));
		}
	}
}
