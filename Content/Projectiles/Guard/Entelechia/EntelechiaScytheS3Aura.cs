using System;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Entelechia
{
	// 隐德来希·三技能「灵与欲的惜别」血雾光环：
	//   技能激活期间跟随玩家、在其脚下渲染淡红/深红的柔边血雾（顶点 alpha 渐隐 → 边界模糊，
	//   半径噪声随时间波动 → 纹理运动随机），并不断喷出向上飘散的红色气体粒子。
	//   纯视觉、无伤害；技能结束后淡出消失。
	public class EntelechiaScytheS3Aura : ModProjectile
	{
		private static readonly Color LightRed = new(200, 60, 64);  // 淡红
		private static readonly Color DeepRed  = new(70, 8, 12);    // 深红

		private static BasicEffect _basic;

		public override string Texture => ArknightsMod.noTexture;

		private Player Owner => Main.player[Projectile.owner];
		private float Opacity { get => Projectile.localAI[0]; set => Projectile.localAI[0] = value; }
		private float Anim    { get => Projectile.localAI[1]; set => Projectile.localAI[1] = value; }

		public override void Unload() {
			Main.QueueMainThreadAction(() => { _basic?.Dispose(); _basic = null; });
		}

		public override void SetDefaults() {
			Projectile.width = Projectile.height = 32;
			Projectile.friendly    = false;
			Projectile.hostile     = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate   = -1;
			Projectile.timeLeft    = 2;
		}

		public override void AI() {
			Player p = Owner;
			if (p == null || !p.active || p.dead) {
				Projectile.Kill();
				return;
			}

			var mp = p.GetModPlayer<WeaponPlayer>();
			bool active = mp.Skill == 2 && mp.SkillActive;

			Projectile.Center = p.Bottom + new Vector2(0f, -6f);
			Anim += 1f;

			if (active) {
				Projectile.timeLeft = 2;
				Opacity = MathHelper.Clamp(Opacity + 0.06f, 0f, 1f);
			}
			else {
				Opacity = MathHelper.Clamp(Opacity - 0.04f, 0f, 1f);
				if (Opacity <= 0.001f) {
					Projectile.Kill();
					return;
				}
			}

			// 红色气体：从脚下向上飘散
			if (!Main.dedServ && Opacity > 0.05f && !Main.gamePaused) {
				int n = Main.rand.NextBool() ? 2 : 1;
				for (int i = 0; i < n; i++) {
					Vector2 pos = p.Bottom + new Vector2(Main.rand.NextFloat(-22f, 22f), Main.rand.NextFloat(-4f, 6f));
					Vector2 vel = new(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2.4f, -0.8f));
					int type = Main.rand.NextBool(3) ? DustID.RedTorch : DustID.Blood;
					int idx = Dust.NewDust(pos, 0, 0, type, vel.X, vel.Y, 100, default, 1f);
					Dust d = Main.dust[idx];
					d.noGravity = true;
					d.scale = Main.rand.NextFloat(0.7f, 1.4f) * MathHelper.Clamp(Opacity, 0.4f, 1f);
					d.fadeIn = Main.rand.NextFloat(0.8f, 1.5f);
					if (type == DustID.RedTorch)
						d.color = new Color(210, 50, 50, 180);
				}
			}

			Lighting.AddLight(p.Bottom, 0.35f * Opacity, 0.05f * Opacity, 0.06f * Opacity);
		}

		// 一团柔边血雾（三角扇）：中心实、边缘 alpha→0（边界模糊），半径随角度与时间做噪声波动
		private void DrawCloud(GraphicsDevice device, Vector2 center, float rx, float ry, Color color,
			float alpha, float phase, float spin) {
			const int N = 40;
			float time = Anim * 0.03f;
			var verts = new VertexPositionColor[N * 3];

			Color cCore = color; cCore.A = (byte)(alpha * 255f);
			Color cRim  = color; cRim.A  = 0;

			int vi = 0;
			for (int i = 0; i < N; i++) {
				float a0 = MathHelper.TwoPi * i / N + spin;
				float a1 = MathHelper.TwoPi * (i + 1) / N + spin;

				float w0 = 1f + 0.22f * (float)Math.Sin(a0 * 3f + time + phase) + 0.14f * (float)Math.Sin(a0 * 5f - time * 1.7f);
				float w1 = 1f + 0.22f * (float)Math.Sin(a1 * 3f + time + phase) + 0.14f * (float)Math.Sin(a1 * 5f - time * 1.7f);

				Vector2 p0 = center + new Vector2((float)Math.Cos(a0) * rx * w0, (float)Math.Sin(a0) * ry * w0);
				Vector2 p1 = center + new Vector2((float)Math.Cos(a1) * rx * w1, (float)Math.Sin(a1) * ry * w1);

				verts[vi++] = new VertexPositionColor(new Vector3(center, 0f), cCore);
				verts[vi++] = new VertexPositionColor(new Vector3(p0, 0f), cRim);
				verts[vi++] = new VertexPositionColor(new Vector3(p1, 0f), cRim);
			}

			foreach (EffectPass pass in _basic.CurrentTechnique.Passes) {
				pass.Apply();
				device.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, N);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ || Opacity <= 0.01f)
				return false;

			if (_basic == null || _basic.IsDisposed) {
				_basic = new BasicEffect(Main.instance.GraphicsDevice) {
					VertexColorEnabled = true,
					TextureEnabled = false,
					View = Matrix.Identity,
				};
			}

			GraphicsDevice device = Main.instance.GraphicsDevice;
			Vector2 center = Owner.Bottom + new Vector2(0f, -4f);

			Main.spriteBatch.End();
			BlendState oldBlend = device.BlendState;
			RasterizerState oldRaster = device.RasterizerState;
			DepthStencilState oldDepth = device.DepthStencilState;

			device.BlendState        = BlendState.Additive;
			device.RasterizerState   = RasterizerState.CullNone;
			device.DepthStencilState = DepthStencilState.None;
			_basic.Projection = Matrix.CreateOrthographicOffCenter(0f, Main.screenWidth, Main.screenHeight, 0f, 0f, 1f);
			_basic.World = Matrix.CreateTranslation(new Vector3(-Main.screenPosition.X, -Main.screenPosition.Y, 0f)) * Main.GameViewMatrix.ZoomMatrix;

			// 淡红外层（大、扁、淡） + 深红内核（小、略浓）；两层不同自转/相位 → 纹理运动随机
			DrawCloud(device, center, 78f, 34f, LightRed, 0.32f * Opacity, 0.0f,  Anim * 0.004f);
			DrawCloud(device, center, 64f, 28f, LightRed, 0.28f * Opacity, 2.1f, -Anim * 0.006f);
			DrawCloud(device, center, 46f, 22f, DeepRed,  0.55f * Opacity, 1.0f,  Anim * 0.008f);

			device.BlendState        = oldBlend;
			device.RasterizerState   = oldRaster;
			device.DepthStencilState = oldDepth;
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}
