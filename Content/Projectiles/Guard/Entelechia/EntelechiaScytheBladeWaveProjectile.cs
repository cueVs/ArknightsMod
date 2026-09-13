using System;
using ArknightsMod.Assets.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Entelechia
{
	// 隐德来希·普通攻击：朝光标方向甩出的月牙形「刀光」。
	// 形状、颜色渐变、边缘锯齿抖动全部由 EntelechiaBladeWave.fx 程序化计算（XNA HiDef 格式，EasyXnb 编译）。
	// C# 端负责飞行轨迹、生命周期淡入淡出、命中判定与命中粒子反馈。
	// 调用方式：SpriteBatch 绑定 Effect，以 MagicPixel（1x1 白色纹理）撑开包围盒执行绘制，
	// 纹理本身内容无关紧要，真正的形状/颜色完全由 shader 程序化计算输出。
	public class EntelechiaScytheBladeWaveProjectile : ModProjectile
	{
		private const int FadeInTicks   = 4;
		private const int FadeOutTicks  = 16;   // 末端更长的淡出消失
		private const int BaseLifeTicks = 100;  // 基础存活（配合初速决定飞行距离）

		// ai[1] = 寿命/距离倍率（0 视作 1）；ai[0] = 扁平度，垂直收缩刀光使其更“扁”（0 视作 1）
		private int LifeMax => System.Math.Max(8, (int)(BaseLifeTicks * (Projectile.ai[1] > 0f ? Projectile.ai[1] : 1f)));
		private float Flatten => Projectile.ai[0] > 0f ? Projectile.ai[0] : 1f;

		// 必须是正方形，shader 中的环形 SDF 才能保持圆形（非正方形 UV 会变椭圆）
		// 包围盒需给发光和拖尾在 UV [-1,1] 边界内完全淡出留够空间
		// 整体放大：刀光在屏幕上更大，命中范围（Colliding）同步按此尺寸计算
		private static readonly Vector2 DrawSize = new(280f, 280f);

		// 命中判定用：月牙外半径、张开角度，与近身核心半径
		private const float HitOuterFrac = 0.82f; // 略大于 shader outerR，覆盖发光边缘
		private const float HitArcAngle  = 1.49f; // = PI - arcOpen(1.65)，月牙单侧张开角
		private const float HitNearFrac  = 0.45f; // 近身核心：此半径内不受弧段角度限制，直接命中（避免近距离打空）

		private static Asset<Effect> _effect;
		private static Asset<Effect> _distortEffect;

		public override string Texture => ArknightsMod.noTexture;

		public override void Load() {
			if (Main.dedServ)
				return;
			try {
				_effect = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/EntelechiaBladeWave",
					AssetRequestMode.ImmediateLoad);
			}
			catch {
				_effect = null;
			}
			try {
				_distortEffect = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/EntelechiaBladeWaveDistort",
					AssetRequestMode.ImmediateLoad);
			}
			catch {
				_distortEffect = null;
			}
		}

		public override void Unload() {
			_effect = null;
			_distortEffect = null;
		}

		public override void SetDefaults() {
			Projectile.width  = 84;
			Projectile.height = 56;
			Projectile.friendly          = true;
			Projectile.hostile           = false;
			Projectile.tileCollide       = false;  // 穿过方块
			Projectile.ignoreWater       = true;
			Projectile.penetrate         = -1;     // 无限穿透：穿过任何目标，命中后继续飞行
			Projectile.DamageType        = DamageClass.Melee;
			Projectile.timeLeft          = BaseLifeTicks; // OnSpawn 内按 lifeMul 覆盖
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown  = 12;
			Projectile.extraUpdates      = 1;
		}

		// 命中判定与刀光视觉一致：以月牙环形弧段（旋转后的扇环）作为伤害范围，
		// 而非默认的矩形包围盒。穿过任何目标和方块、命中后不消失。
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			float half  = DrawSize.X * 0.5f * Projectile.scale;
			float outer = half * HitOuterFrac;
			float near  = half * HitNearFrac;

			Vector2 c = Projectile.Center;
			// 目标矩形上距离刀光中心最近的点
			Vector2 closest = new(
				MathHelper.Clamp(c.X, targetHitbox.Left, targetHitbox.Right),
				MathHelper.Clamp(c.Y, targetHitbox.Top, targetHitbox.Bottom));

			float d = Vector2.Distance(c, closest);
			if (d > outer)
				return false;

			// 近身核心：不留中空死区，近距离目标直接命中
			if (d <= near)
				return true;

			// 其余范围需落在月牙张开的弧段内（相对飞行方向 ±HitArcAngle）
			float ang = MathHelper.WrapAngle((closest - c).ToRotation() - Projectile.rotation);
			if (System.Math.Abs(ang) > HitArcAngle)
				return false;

			return true;
		}

		public override void OnSpawn(Terraria.DataStructures.IEntitySource source) {
			// 按 lifeMul（ai[1]）确定实际存活 → 决定飞行距离
			Projectile.timeLeft = LifeMax;

			// S3 刀光（ai[2]==2）：单次伤害判定 —— 命中冷却拉长到超过存活，每个目标只命中一次
			if (Projectile.ai[2] == 2f)
				Projectile.localNPCHitCooldown = 100000;

			// 发射瞬间：红色粒子爆发 + 一颗较大的白色十字星点缀
			for (int i = 0; i < 10; i++) {
				Dust d = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(14f, 14f),
					DustID.RedTorch, Main.rand.NextVector2Circular(3f, 3f) - Projectile.velocity * 0.05f,
					0, default, Main.rand.NextFloat(1.0f, 1.6f));
				d.noGravity = true;
			}
			if (Projectile.owner == Main.myPlayer) {
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
					ModContent.ProjectileType<EntelechiaCrossStar>(), 0, 0, Projectile.owner, 1.3f);
			}
		}

		public override void AI() {
			// 速度随生命递减：发射时最快，越临近消失越慢（缓出曲线）
			if (Projectile.localAI[0] <= 0f)
				Projectile.localAI[0] = Projectile.velocity.Length(); // 记录初速
			float initSpeed = Projectile.localAI[0];
			if (initSpeed > 0.01f) {
				float lifeT = 1f - Projectile.timeLeft / (float)LifeMax; // 0→1
				float speedMul = MathHelper.Clamp((float)System.Math.Pow(1f - lifeT, 0.7), 0.06f, 1f);
				Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * (initSpeed * speedMul);
			}

			Projectile.rotation = Projectile.velocity.ToRotation();

			Vector2 dir  = Projectile.velocity.SafeNormalize(Vector2.UnitX);
			Vector2 perp = new(-dir.Y, dir.X);

			// 拖尾轨迹处的红色粒子：在月牙凹侧（飞行反方向）沿轨迹散布
			for (int i = 0; i < 2; i++) {
				Vector2 trailPos = Projectile.Center
					- dir * Main.rand.NextFloat(0f, 36f)
					+ perp * Main.rand.NextFloat(-26f, 26f);
				Dust d = Dust.NewDustPerfect(trailPos, DustID.RedTorch,
					-dir * Main.rand.NextFloat(0.5f, 1.6f) + perp * Main.rand.NextFloat(-0.6f, 0.6f),
					0, default, Main.rand.NextFloat(0.9f, 1.4f));
				d.noGravity = true;
				d.fadeIn    = 0.5f;
			}

			// 沿轨迹偶发白色四角十字星（大幅降低出现频率）
			if (Projectile.owner == Main.myPlayer && Main.rand.NextBool(28)) {
				Vector2 starPos = Projectile.Center
					- dir * Main.rand.NextFloat(0f, 30f)
					+ perp * Main.rand.NextFloat(-22f, 22f);
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), starPos, Vector2.Zero,
					ModContent.ProjectileType<EntelechiaCrossStar>(), 0, 0, Projectile.owner,
					Main.rand.NextFloat(0.5f, 0.9f));
			}

			Lighting.AddLight(Projectile.Center, 0.6f, 0.15f, 0.1f);
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			// 红色粒子
			for (int i = 0; i < 10; i++) {
				Dust d = Dust.NewDustPerfect(target.Center, DustID.RedTorch,
					Main.rand.NextVector2Circular(5f, 5f), 0, default, Main.rand.NextFloat(1.0f, 1.6f));
				d.noGravity = true;
			}
			// S1 强化刀光（ai[2]==1）：对目标额外造成一次伤害（一次命中 = 两次伤害）
			if (Projectile.ai[2] == 1f)
				target.SimpleStrikeNPC(damageDone, hit.HitDirection, false, 0f, Projectile.DamageType);

			// 命中爆开特效（白→红→深红 辐射状角 + 白色辐射线）
			if (Projectile.owner == Main.myPlayer) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
					ModContent.ProjectileType<EntelechiaHitBurst>(), 0, 0, Projectile.owner,
					Main.rand.Next(1, int.MaxValue));
			}
		}

		// 白色核心：先出现、最后消失；红色辉光/拖尾：后出现、先消失（淡入淡出错峰，互相重叠）
		private void GetProgress(out float white, out float red) {
			int age = LifeMax - Projectile.timeLeft;

			// 淡入：白色先涨（前 60% 内涨满），红色延后起步（40% 才开始）
			float wi = MathHelper.Clamp(age / (FadeInTicks * 0.6f), 0f, 1f);
			float ri = MathHelper.Clamp((age - FadeInTicks * 0.45f) / (FadeInTicks * 0.7f), 0f, 1f);

			// 淡出：红色先退（剩余 45% 前就退完），白色拖到最后才退
			float wo = MathHelper.Clamp(Projectile.timeLeft / (FadeOutTicks * 0.55f), 0f, 1f);
			float ro = MathHelper.Clamp((Projectile.timeLeft - FadeOutTicks * 0.45f) / (FadeOutTicks * 0.6f), 0f, 1f);

			white = MathHelper.Min(wi, wo);
			red   = MathHelper.Min(ri, ro);
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			Effect eff = _effect?.Value;
			if (eff == null)
				return false;

			GetProgress(out float whiteProgress, out float redProgress);
			if (whiteProgress <= 0.001f && redProgress <= 0.001f)
				return false;

			// ── Ortho 投影矩阵（屏幕空间 → 裁剪空间），传给 shader 的 VS ─────
			Matrix matrix = Matrix.CreateOrthographicOffCenter(
				0, Main.screenWidth, Main.screenHeight, 0, -1f, 1f);
			eff.Parameters["MatrixTransform"]?.SetValue(matrix);
			eff.Parameters["uColor"]?.SetValue(new Vector3(0.95f, 0.10f, 0.08f));  // 外发光：红
			eff.Parameters["uSecondaryColor"]?.SetValue(new Vector3(1.0f, 1.0f, 1.0f)); // 核心：白
			eff.Parameters["uProgress"]?.SetValue(whiteProgress);
			eff.Parameters["uRedProgress"]?.SetValue(redProgress);
			// 白天背景明亮，整体提高强度让本色更易达到不透明、显示更明显
			float intensity = Main.dayTime ? 1.35f : 0.85f;
			eff.Parameters["uIntensity"]?.SetValue(intensity);
			eff.Parameters["uTime"]?.SetValue((float)Main.timeForVisualEffects * 0.05f);

			// ── 手动构造正方形四边形（旋转后顶点坐标 + 0→1 UV）──────────────
			// 必须手动 End/Begin 以结束当前 SpriteBatch 批次，再直接调用 DrawUserPrimitives
			float half  = DrawSize.X * 0.5f * Projectile.scale;
			float halfX = half;                 // 沿飞行方向（不收缩）
			float halfY = half * Flatten;       // 垂直方向按扁平度收缩 → 刀光更“扁”
			Vector2 center = Projectile.Center - Main.screenPosition;
			float rot = Projectile.rotation;

			Vector2 tl = center + new Vector2(-halfX, -halfY).RotatedBy(rot);
			Vector2 tr = center + new Vector2( halfX, -halfY).RotatedBy(rot);
			Vector2 bl = center + new Vector2(-halfX,  halfY).RotatedBy(rot);
			Vector2 br = center + new Vector2( halfX,  halfY).RotatedBy(rot);

			// TriangleStrip 顺序：TL → TR → BL → BR（2 个三角形）
			var verts = new VertexPositionTexture[4];
			verts[0] = new VertexPositionTexture(new Vector3(tl, 0f), new Vector2(0f, 0f));
			verts[1] = new VertexPositionTexture(new Vector3(tr, 0f), new Vector2(1f, 0f));
			verts[2] = new VertexPositionTexture(new Vector3(bl, 0f), new Vector2(0f, 1f));
			verts[3] = new VertexPositionTexture(new Vector3(br, 0f), new Vector2(1f, 1f));

			GraphicsDevice device = Main.instance.GraphicsDevice;

			// 结束当前 deferred 批次（进入 PreDraw 时它处于 active）
			Main.spriteBatch.End();

			// ── 拖尾屏幕折射扭曲（在画刀光本体之前，扭曲其后方画面）──
			DrawDistortion(redProgress);

			device.BlendState        = BlendState.AlphaBlend; // 预乘 Alpha：兼顾夜间辉光与白天可见
			device.RasterizerState   = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.None;

			foreach (EffectPass pass in eff.CurrentTechnique.Passes) {
				pass.Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, 2);
			}

			// 恢复 SpriteBatch
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None,
				RasterizerState.CullNone, null,
				Main.GameViewMatrix.TransformationMatrix);

			return false;
		}

		// 复用 SchwarzDistortionSystem 的抓屏快照，把刀光拖尾区域后方的画面做折射扭曲。
		// 进入时假定 Main.spriteBatch 已 End；本方法自行管理 capture/immediate 批次，结束时保持 End。
		private void DrawDistortion(float progress) {
			Effect deff = _distortEffect?.Value;
			if (deff == null)
				return;

			var gd = Main.instance?.GraphicsDevice;
			if (gd == null)
				return;

			var screenRT = Main.screenTarget;
			if (screenRT == null || screenRT.IsDisposed)
				return;

			int sw = screenRT.Width;
			int sh = screenRT.Height;

			// 按需创建/重建快照与独立 capture batch（与 Schwarz 共用）
			if (SchwarzDistortionSystem.ScreenSnapshot == null ||
				SchwarzDistortionSystem.ScreenSnapshot.IsDisposed ||
				SchwarzDistortionSystem.ScreenSnapshot.Width  != sw ||
				SchwarzDistortionSystem.ScreenSnapshot.Height != sh) {
				SchwarzDistortionSystem.ScreenSnapshot?.Dispose();
				SchwarzDistortionSystem.ScreenSnapshot = new RenderTarget2D(
					gd, sw, sh, false, screenRT.Format, DepthFormat.None);
			}
			if (SchwarzDistortionSystem.CaptureBatch == null ||
				SchwarzDistortionSystem.CaptureBatch.IsDisposed) {
				SchwarzDistortionSystem.CaptureBatch = new SpriteBatch(gd);
			}

			var snap = SchwarzDistortionSystem.ScreenSnapshot;
			var cap  = SchwarzDistortionSystem.CaptureBatch;
			var snapSz = new Vector2(sw, sh);

			// 步骤1：抓屏到 snap；步骤2：重绑 screenTarget 并整面填回（避免 DiscardContents）
			gd.SetRenderTarget(snap);
			cap.Begin(SpriteSortMode.Deferred, BlendState.Opaque,
				SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
			cap.Draw(screenRT, Vector2.Zero, Color.White);
			cap.End();

			gd.SetRenderTarget(screenRT);
			cap.Begin(SpriteSortMode.Deferred, BlendState.Opaque,
				SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Matrix.Identity);
			cap.Draw(snap, Vector2.Zero, Color.White);
			cap.End();

			// 与可视 shader 完全一致的原始屏幕相对坐标（不经 GameViewMatrix），
			// 否则扭曲区域会与实际刀光错位（出现"扭曲在刀光前方"的现象）
			float half = DrawSize.X * 0.5f * Projectile.scale;
			float rot  = Projectile.rotation;
			Vector2 scrC = Projectile.Center - Main.screenPosition;
			Vector2 uDir = new((float)Math.Cos(rot), (float)Math.Sin(rot)); // 本地 +x 屏幕方向
			float halfPx = half;

			// 旋转包围盒 → 屏幕对齐 bbox（含 padding 防采样越界）
			Vector2[] corners = {
				scrC + new Vector2(-half, -half).RotatedBy(rot),
				scrC + new Vector2( half, -half).RotatedBy(rot),
				scrC + new Vector2(-half,  half).RotatedBy(rot),
				scrC + new Vector2( half,  half).RotatedBy(rot),
			};
			float pad = 16f;
			float minX = sw, minY = sh, maxX = 0f, maxY = 0f;
			foreach (var v in corners) {
				minX = MathF.Min(minX, v.X); minY = MathF.Min(minY, v.Y);
				maxX = MathF.Max(maxX, v.X); maxY = MathF.Max(maxY, v.Y);
			}
			minX = MathHelper.Clamp(minX - pad, 0f, sw);
			minY = MathHelper.Clamp(minY - pad, 0f, sh);
			maxX = MathHelper.Clamp(maxX + pad, 0f, sw);
			maxY = MathHelper.Clamp(maxY + pad, 0f, sh);
			if (maxX <= minX || maxY <= minY)
				return;

			var srcRect = new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));

			deff.Parameters["uRes"]?.SetValue(snapSz);
			deff.Parameters["uCenter"]?.SetValue(scrC / snapSz);
			deff.Parameters["uDir"]?.SetValue(uDir);
			deff.Parameters["uHalfPx"]?.SetValue(halfPx);
			deff.Parameters["uIntensity"]?.SetValue(0.11f * progress);
			deff.Parameters["uTime"]?.SetValue((float)Main.timeForVisualEffects * 0.05f);

			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
				SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, deff, Matrix.Identity);
			deff.CurrentTechnique.Passes[0].Apply();
			Main.spriteBatch.Draw(snap, new Vector2(minX, minY), srcRect, Color.White);
			Main.spriteBatch.End();
		}
	}
}
