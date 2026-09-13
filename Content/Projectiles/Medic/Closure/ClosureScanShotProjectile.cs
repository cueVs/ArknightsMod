using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Closure
{
	// 可露希尔·扫描枪普通攻击「能量镖」：尖头长条的粉紫色直线弹幕。
	// 弹幕本体形状/配色/中线白线/向后拖尾/头部外发光全部由 ClosureScanShot.fx 程序化计算，
	// C# 端负责直线飞行（无重力）、尾部掉落的暗紫粒子、以及贴在弹幕最上层绘制的白色十字星点缀。
	public class ClosureScanShotProjectile : ModProjectile
	{
		private const int FadeInTicks  = 3;
		private const int FadeOutTicks = 8;
		private const int LifeTicks    = 150;

		// 绘制包围盒：沿飞行方向拉长的矩形（shader 内按此比例设计）。
		// 命中判定与本尺寸完全无关（见下方 HitHalfLen/HitRadius），可以放心按视觉效果调整大小。
		private static readonly Vector2 DrawSize = new(140f, 42f);

		// shader 内镖体的归一化尖端/尾端位置（对应 .fx 中的 tipX/tailX），用于十字星沿中线分布
		private const float BodyExtentFrac = 0.84f;

		private static Asset<Effect> _effect;
		private static BasicEffect _starBasic;

		private Vector2 _lastLitCenter;
		private bool _hasLastLitCenter;
		private bool _burstSpawned; // 防止命中 NPC 后紧接着 penetrate 耗尽触发 OnKill 时重复生成命中特效

		// 白色十字星：直接在本弹幕的 PreDraw 里、画完弹幕本体之后再画一层，保证一定叠在弹幕上方，
		// 不再依赖单独生成的 EntelechiaCrossStar 弹幕（那种做法的绘制顺序取决于弹幕数组下标，
		// 经常先于本体绘制、被本体覆盖，看起来就像"十字星盖不住弹幕"）。
		private struct MiniStar { public float AlongT; public float Life; public float MaxLife; public float Size; }
		private readonly List<MiniStar> _stars = new();

		// 伤害判定区域的半长/半宽（世界像素）：贴着弹幕中心的小胶囊体，与 DrawSize 视觉尺寸无关。
		private const float HitHalfLen = 18f;
		private const float HitRadius  = 15f;

		public override string Texture => ArknightsMod.noTexture;

		public override void Load() {
			if (Main.dedServ)
				return;
			try {
				_effect = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/ClosureScanShot",
					AssetRequestMode.ImmediateLoad);
			}
			catch {
				_effect = null;
			}
		}

		public override void Unload() {
			_effect = null;
			Main.QueueMainThreadAction(() => {
				_starBasic?.Dispose();
				_starBasic = null;
			});
		}

		public override void SetDefaults() {
			Projectile.width  = 22;
			Projectile.height = 12;
			Projectile.friendly    = true;
			Projectile.hostile     = false;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.penetrate   = 1;
			Projectile.DamageType  = DamageClass.Ranged;
			Projectile.timeLeft    = LifeTicks;
			Projectile.extraUpdates = 1;   // 提升直线飞行的顺滑度与速度感
		}

		// 命中判定：以「本 tick 实际扫过的路径」为线段（上一位置 → 当前 Center）采样，
		// 而非只看当前瞬间的一个点，避免高速飞行时跳过目标本体（隧穿漏判）。
		// prevCenter 用 Center - velocity 直接反推本 tick 移动前的位置，不依赖任何跨帧缓存。
		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			Vector2 dir = Projectile.velocity.SafeNormalize(Vector2.UnitX);
			Vector2 prevCenter = Projectile.Center - Projectile.velocity;

			Vector2 a = prevCenter      - dir * HitHalfLen;
			Vector2 b = Projectile.Center + dir * HitHalfLen;

			// 沿"上一位置到当前位置"的整条扫过路径采样，杜绝高速移动下的隧穿漏判
			const int samples = 6;
			for (int i = 0; i <= samples; i++) {
				Vector2 pt = Vector2.Lerp(a, b, i / (float)samples);
				float cx = MathHelper.Clamp(pt.X, targetHitbox.Left, targetHitbox.Right);
				float cy = MathHelper.Clamp(pt.Y, targetHitbox.Top, targetHitbox.Bottom);
				if (Vector2.DistanceSquared(pt, new Vector2(cx, cy)) <= HitRadius * HitRadius)
					return true;
			}
			return false;
		}

		public override void AI() {
			// 直线飞行：不施加重力、不衰减速度
			Projectile.rotation = Projectile.velocity.ToRotation();

			Vector2 dir  = Projectile.velocity.SafeNormalize(Vector2.UnitX);
			Vector2 perp = new(-dir.Y, dir.X);

			// 尾部「掉落脱离」的暗紫粒子：从尾段生成，向后+向下缓慢飘离
			if (Main.rand.NextBool(2)) {
				Vector2 dropPos = Projectile.Center
					- dir * Main.rand.NextFloat(20f, 95f)
					+ perp * Main.rand.NextFloat(-10f, 10f);
				Dust d = Dust.NewDustPerfect(dropPos, Main.rand.NextBool(3) ? DustID.RedTorch : DustID.PinkTorch,
					-dir * Main.rand.NextFloat(0.4f, 1.4f) + new Vector2(0f, Main.rand.NextFloat(0.3f, 0.9f)),
					0, default, Main.rand.NextFloat(0.7f, 1.2f));
				d.noGravity = true;
				d.fadeIn    = 0.4f;
			}

			// 白色十字星：沿中线（贯穿弹幕的白线）随机位置生成，随弹幕一起往前飞；
			// 越靠近尖端（前）越大，越靠近尾端（后）越小。维护在本地列表里，PreDraw 时画在
			// 弹幕本体之后，保证视觉上一定叠加在弹幕贴图之上。
			for (int i = _stars.Count - 1; i >= 0; i--) {
				var s = _stars[i];
				s.Life -= 1f;
				if (s.Life <= 0f) {
					_stars.RemoveAt(i);
					continue;
				}
				_stars[i] = s;
			}
			if (Main.rand.NextBool(5) && _stars.Count < 6) {
				float alongT = Main.rand.NextFloat(); // 0=尾端 1=尖端
				_stars.Add(new MiniStar {
					AlongT  = alongT,
					Life    = 18f,
					MaxLife = 18f,
					Size    = MathHelper.Lerp(0.16f, 0.62f, alongT),
				});
			}

			// 沿本次位移路径插值补光，避免高速飞行时（extraUpdates 导致单帧位移大于光照采样格）
			// 灯光引擎按格更新出现"只在经过的几个点留下光斑"的断续现象，改为连续覆盖整条轨迹。
			if (!_hasLastLitCenter) {
				_lastLitCenter = Projectile.Center;
				_hasLastLitCenter = true;
			}
			float travelDist = Vector2.Distance(_lastLitCenter, Projectile.Center);
			int steps = System.Math.Max(1, (int)(travelDist / 8f));
			for (int i = 0; i <= steps; i++) {
				Vector2 litPos = Vector2.Lerp(_lastLitCenter, Projectile.Center, i / (float)steps);
				Lighting.AddLight(litPos, 0.55f, 0.16f, 0.20f);
			}
			_lastLitCenter = Projectile.Center;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			_burstSpawned = true;
			SpawnBurst(target.Center);
			if (Projectile.owner == Main.myPlayer) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
					ModContent.ProjectileType<ClosureHitBurstProjectile>(), 0, 0, Projectile.owner,
					Main.rand.Next(1, int.MaxValue));
			}
		}

		public override void OnKill(int timeLeft) {
			// timeLeft > 0 说明是被方块/边界提前打断（而非飞行时间自然耗尽），此时才需要命中特效；
			// 命中 NPC 时 OnHitNPC 已经生成过一次，避免 penetrate 耗尽紧接着触发 OnKill 时重复生成
			if (timeLeft <= 0 || _burstSpawned)
				return;

			SpawnBurst(Projectile.Center);
			if (Projectile.owner == Main.myPlayer) {
				Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
					ModContent.ProjectileType<ClosureHitBurstProjectile>(), 0, 0, Projectile.owner,
					Main.rand.Next(1, int.MaxValue));
			}
		}

		private static void SpawnBurst(Vector2 pos) {
			for (int i = 0; i < 8; i++) {
				Dust d = Dust.NewDustPerfect(pos, DustID.PinkTorch,
					Main.rand.NextVector2Circular(3.5f, 3.5f), 0, default, Main.rand.NextFloat(0.9f, 1.5f));
				d.noGravity = true;
			}
			for (int i = 0; i < 3; i++) {
				Dust d = Dust.NewDustPerfect(pos, DustID.RedTorch,
					Main.rand.NextVector2Circular(2f, 2f), 0, default, Main.rand.NextFloat(0.7f, 1.1f));
				d.noGravity = true;
			}
		}

		private float GetProgress() {
			int age = LifeTicks - Projectile.timeLeft;
			float fadeIn  = MathHelper.Clamp(age / (float)FadeInTicks, 0f, 1f);
			float fadeOut = MathHelper.Clamp(Projectile.timeLeft / (float)FadeOutTicks, 0f, 1f);
			return MathHelper.Min(fadeIn, fadeOut);
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			Effect eff = _effect?.Value;
			if (eff == null)
				return false;

			float progress = GetProgress();
			if (progress <= 0.001f)
				return false;

			// GameViewMatrix.TransformationMatrix 本身只是像素空间内的缩放+平移（SpriteBatch 内部
			// 会再乘自己的正交投影），并不是可以直接当作 MatrixTransform 使用的裁剪空间投影矩阵；
			// 必须先应用缩放+平移，再手动接上正交投影，两者相乘才是完整、正确的变换。
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1f, 1f);
			eff.Parameters["MatrixTransform"]?.SetValue(Main.GameViewMatrix.TransformationMatrix * projection);
			eff.Parameters["uProgress"]?.SetValue(progress);
			eff.Parameters["uIntensity"]?.SetValue(Main.dayTime ? 1.25f : 0.95f);
			eff.Parameters["uTime"]?.SetValue((float)Main.timeForVisualEffects * 0.05f);
			eff.Parameters["uSeed"]?.SetValue(Projectile.whoAmI * 0.731f % 10f);

			// 沿飞行方向拉长的四边形（+x = 飞行方向），手动顶点 + TriangleStrip
			float halfX = DrawSize.X * 0.5f * Projectile.scale;
			float halfY = DrawSize.Y * 0.5f * Projectile.scale;
			Vector2 center = Projectile.Center - Main.screenPosition;
			float rot = Projectile.rotation;

			Vector2 tl = center + new Vector2(-halfX, -halfY).RotatedBy(rot);
			Vector2 tr = center + new Vector2( halfX, -halfY).RotatedBy(rot);
			Vector2 bl = center + new Vector2(-halfX,  halfY).RotatedBy(rot);
			Vector2 br = center + new Vector2( halfX,  halfY).RotatedBy(rot);

			var verts = new VertexPositionTexture[4];
			verts[0] = new VertexPositionTexture(new Vector3(tl, 0f), new Vector2(0f, 0f));
			verts[1] = new VertexPositionTexture(new Vector3(tr, 0f), new Vector2(1f, 0f));
			verts[2] = new VertexPositionTexture(new Vector3(bl, 0f), new Vector2(0f, 1f));
			verts[3] = new VertexPositionTexture(new Vector3(br, 0f), new Vector2(1f, 1f));

			GraphicsDevice device = Main.instance.GraphicsDevice;

			Main.spriteBatch.End();

			device.BlendState        = BlendState.AlphaBlend; // 预乘 Alpha：夜间辉光 + 白天可见
			device.RasterizerState   = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.None;

			foreach (EffectPass pass in eff.CurrentTechnique.Passes) {
				pass.Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, 2);
			}

			// 十字星画在弹幕本体之后（同一个 End/Begin 区块内），保证视觉上一定叠在弹幕贴图上层
			DrawStars(device, center, Vector2.UnitX.RotatedBy(rot));

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None,
				RasterizerState.CullNone, null,
				Main.GameViewMatrix.TransformationMatrix);

			return false;
		}

		private static void AddTri(List<VertexPositionColor> v, Vector2 a, Vector2 b, Vector2 c, Color col) {
			v.Add(new VertexPositionColor(new Vector3(a, 0f), col));
			v.Add(new VertexPositionColor(new Vector3(b, 0f), col));
			v.Add(new VertexPositionColor(new Vector3(c, 0f), col));
		}

		private void DrawStars(GraphicsDevice device, Vector2 screenCenter, Vector2 dir) {
			if (_stars.Count == 0)
				return;

			if (_starBasic == null || _starBasic.IsDisposed) {
				_starBasic = new BasicEffect(device) {
					VertexColorEnabled = true,
					World = Matrix.Identity,
					View  = Matrix.Identity,
				};
			}

			float extent = DrawSize.X * 0.5f * BodyExtentFrac * Projectile.scale;
			var verts = new List<VertexPositionColor>(_stars.Count * 12);

			foreach (var s in _stars) {
				float lifeT = s.Life / s.MaxLife;
				float alpha = MathHelper.Clamp(lifeT * 2f, 0f, 1f) * MathHelper.Clamp((1f - lifeT) * 4f, 0f, 1f);
				if (alpha <= 0.01f)
					continue;

				Vector2 p = screenCenter + dir * MathHelper.Lerp(-extent, extent, s.AlongT);
				float size = 5f * s.Size * Projectile.scale;
				Color col = Color.White * alpha;

				Vector2 e0 = p + new Vector2(-size, 0f);
				Vector2 e1 = p + new Vector2(0f, -size);
				Vector2 e2 = p + new Vector2(size, 0f);
				Vector2 e3 = p + new Vector2(0f, size);
				AddTri(verts, e0, e1, e2, col);
				AddTri(verts, e0, e2, e3, col);
			}

			if (verts.Count < 3)
				return;

			device.BlendState = BlendState.Additive;
			// 同弹幕本体一样，必须叠加 GameViewMatrix（游戏内缩放）再接正交投影，否则缩放不为
			// 默认值时十字星会和弹幕本体错位。
			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1f, 1f);
			_starBasic.Projection = Main.GameViewMatrix.TransformationMatrix * projection;

			var arr = verts.ToArray();
			foreach (EffectPass pass in _starBasic.CurrentTechnique.Passes) {
				pass.Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleList, arr, 0, arr.Length / 3);
			}
		}
	}
}
