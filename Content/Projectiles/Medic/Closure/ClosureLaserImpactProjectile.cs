using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Medic.Closure
{
	// 可露希尔·二技能激光命中特效：小型爆炸——内部纯白核心，边缘不规则地向四周炸开红色外发光尖角
	// （而不是规则的正圆）。比扫描枪弹头命中特效（ClosureHitBurstProjectile）更小、更简洁，
	// 专用于激光命中瞬间的反馈。ai[0] 作为随机种子，保证同一个实例的不规则形状每帧draw都固定不变；
	// ai[1] = 被命中目标的 NPC 下标，效果全程贴着目标当前位置显示（而非停在命中瞬间的世界坐标，
	// 避免目标快速移动时特效明显跟不上、看起来有延迟）。
	// 每个实例的存活时长随机（在攻击频率附近浮动），且淡出曲线延后到接近生命末尾才完全消失——
	// 保证连续攻击时，下一个特效出现前，上一个还没完全淡完，视觉上连贯不断档。
	public class ClosureLaserImpactProjectile : ModProjectile
	{
		private int _age;
		private int _lifeMax = 18;

		private static readonly Color CoreCol = new(255, 255, 255);
		private static readonly Color GlowCol = new(235, 60, 60);

		private static BasicEffect _basic;

		public override string Texture => ArknightsMod.noTexture;

		public override void Unload() {
			Main.QueueMainThreadAction(() => {
				_basic?.Dispose();
				_basic = null;
			});
		}

		public override void SetDefaults() {
			Projectile.width = Projectile.height = 8;
			Projectile.friendly    = false;
			Projectile.hostile     = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate   = -1;
			Projectile.timeLeft    = 99999; // 实际存活时长由 _lifeMax（随机、手动倒计时）控制
		}

		public override void OnSpawn(IEntitySource source) {
			if (Projectile.ai[0] <= 0f)
				Projectile.ai[0] = Main.rand.Next(1, int.MaxValue);
			_lifeMax = Main.rand.Next(16, 23); // 随攻击频率附近随机浮动，避免每次时长完全一致
			Projectile.velocity = Vector2.Zero;
		}

		public override void AI() {
			Projectile.velocity = Vector2.Zero;
			_age++;

			int idx = (int)Projectile.ai[1];
			if (idx >= 0 && idx < Main.maxNPCs && Main.npc[idx].active)
				Projectile.Center = Main.npc[idx].Center; // 持续贴着目标当前位置，不滞后

			if (_age >= _lifeMax)
				Projectile.Kill();

			Lighting.AddLight(Projectile.Center, 0.5f, 0.2f, 0.2f);
		}

		private static void AddTri(List<VertexPositionColor> v, Vector2 a, Vector2 b, Vector2 c,
			Color ca, Color cb, Color cc) {
			v.Add(new VertexPositionColor(new Vector3(a, 0f), ca));
			v.Add(new VertexPositionColor(new Vector3(b, 0f), cb));
			v.Add(new VertexPositionColor(new Vector3(c, 0f), cc));
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			if (_basic == null || _basic.IsDisposed) {
				_basic = new BasicEffect(Main.instance.GraphicsDevice) {
					VertexColorEnabled = true,
					World = Matrix.Identity,
					View  = Matrix.Identity,
				};
			}

			float t = _age / (float)_lifeMax; // 0→1
			float grow = 1f - (float)Math.Pow(1f - MathHelper.Clamp(t / 0.30f, 0f, 1f), 3f); // 快速弹出的缓出曲线
			// 淡出推迟到接近生命末尾才开始、且持续到底：保证下一次命中的新特效出现之前，
			// 这一个还没完全消失，连续攻击时不会出现"先消失、再重新蹦出"的断档感。
			float fade = 1f - MathHelper.Clamp((t - 0.55f) / 0.45f, 0f, 1f);
			if (fade <= 0.01f)
				return false;

			var rng = new Random((int)Projectile.ai[0]);
			Vector2 pos = Projectile.Center - Main.screenPosition;

			var verts = new List<VertexPositionColor>(160);

			// ── 边缘：不规则向四周炸开的红色尖角（而非正圆）——先画宽而淡的多层辉光，
			//      再画偏白的核心尖角，层层叠加形成"由内向外柔和模糊"的爆炸感 ──
			int spikeCount = 9 + rng.Next(5); // 9~13
			var spikes = new (float ang, float len, float halfW)[spikeCount];
			for (int i = 0; i < spikeCount; i++) {
				spikes[i] = (
					(float)(rng.NextDouble() * MathHelper.TwoPi),
					MathHelper.Lerp(7f, 18f, (float)rng.NextDouble()) * grow,
					MathHelper.Lerp(1.6f, 4.2f, (float)rng.NextDouble()));
			}

			Color glowOuter = GlowCol * (fade * 0.35f);
			Color glowMid   = GlowCol * (fade * 0.65f);
			Color transparentOuter = glowOuter; transparentOuter.A = 0;
			Color transparentMid   = glowMid;   transparentMid.A   = 0;
			Color spikeCore = Color.Lerp(CoreCol, GlowCol, 0.25f) * fade;
			Color transparentCore = spikeCore; transparentCore.A = 0;

			foreach (var s in spikes) {
				Vector2 dir  = s.ang.ToRotationVector2();
				Vector2 perp = new(-dir.Y, dir.X);
				Vector2 baseC = pos + dir * (1.2f * grow);
				Vector2 oL = baseC + perp * (s.halfW * 2.6f);
				Vector2 oR = baseC - perp * (s.halfW * 2.6f);
				Vector2 oApex = pos + dir * (s.len * 1.5f);
				AddTri(verts, oL, oR, oApex, glowOuter, glowOuter, transparentOuter);
			}
			foreach (var s in spikes) {
				Vector2 dir  = s.ang.ToRotationVector2();
				Vector2 perp = new(-dir.Y, dir.X);
				Vector2 baseC = pos + dir * (1.4f * grow);
				Vector2 mL = baseC + perp * (s.halfW * 1.7f);
				Vector2 mR = baseC - perp * (s.halfW * 1.7f);
				Vector2 mApex = pos + dir * (s.len * 1.2f);
				AddTri(verts, mL, mR, mApex, glowMid, glowMid, transparentMid);
			}
			foreach (var s in spikes) {
				Vector2 dir  = s.ang.ToRotationVector2();
				Vector2 perp = new(-dir.Y, dir.X);
				Vector2 baseC = pos + dir * (1.6f * grow);
				Vector2 bL = baseC + perp * s.halfW;
				Vector2 bR = baseC - perp * s.halfW;
				Vector2 apex = pos + dir * s.len;
				AddTri(verts, bL, bR, apex, spikeCore, spikeCore, transparentCore);
			}

			// ── 中心：纯白核心团块，全程叠在最上层，保证爆炸中心足够"白" ──
			{
				const int seg = 10;
				float coreR = MathHelper.Lerp(1.2f, 5f, grow) * fade;
				Color core = CoreCol * fade;
				for (int i = 0; i < seg; i++) {
					float a0 = MathHelper.TwoPi * i / seg;
					float a1 = MathHelper.TwoPi * (i + 1) / seg;
					Vector2 p0 = pos + a0.ToRotationVector2() * coreR;
					Vector2 p1 = pos + a1.ToRotationVector2() * coreR;
					AddTri(verts, pos, p0, p1, core, core, core);
				}
			}

			if (verts.Count < 3)
				return false;

			GraphicsDevice device = Main.instance.GraphicsDevice;
			Main.spriteBatch.End();

			device.BlendState        = BlendState.NonPremultiplied;
			device.RasterizerState   = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.None;

			Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1f, 1f);
			_basic.Projection = Main.GameViewMatrix.TransformationMatrix * projection;

			var arr = verts.ToArray();
			foreach (EffectPass pass in _basic.CurrentTechnique.Passes) {
				pass.Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleList, arr, 0, arr.Length / 3);
			}

			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}
