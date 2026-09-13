using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Rogue.FireworksHand
{
	public class FireworksHandProjGlow : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Projectiles/Sniper/Wisadel/star_02";

		private const int MAX_LIFE = 15;              // 总生命周期30帧
		private const int EXPAND_FRAMES = 2;          // 放大只占前2帧（极短）
		private const float PULSE_FREQUENCY = 2.5f;   // 脉冲频率

		private int currentLife;
		private float baseScale;

		public override void SetDefaults() {
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.penetrate = -1;
			Projectile.timeLeft = MAX_LIFE;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.friendly = false;
			Projectile.hostile = false;
		}

		public override void AI() {
			currentLife++;

			if (currentLife == 1) {
				baseScale = (Projectile.ai[0] > 0.1f ? Projectile.ai[0] : 1.2f) * 0.4f;
			}

			Projectile.rotation += 0.02f;
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D starTex = ModContent.Request<Texture2D>(Texture).Value;
			if (starTex == null || starTex.IsDisposed)
				return false;

			Vector2 center = Projectile.Center - Main.screenPosition;
			float progress = (float)currentLife / MAX_LIFE;

			// 亮度曲线（峰值在 60% 生命周期处）
			float glowIntensity;
			float glowPeakTime = 0.6f;
			if (progress < glowPeakTime) {
				float t = progress / glowPeakTime;
				glowIntensity = t * t * 1.3f;
			}
			else {
				float t = (progress - glowPeakTime) / (1f - glowPeakTime);
				glowIntensity = 1.3f * (1f - t * t);
			}
			glowIntensity = MathHelper.Clamp(glowIntensity, 0f, 1.55f);

			// 脉冲效果
			float pulse = 1f + (float)System.Math.Sin(progress * MathHelper.TwoPi * PULSE_FREQUENCY) * 0.1f * (1f - progress * 0.5f);

			// ===== 尺寸动画：极速放大 + 缓慢缩小 =====
			float sizeScale;
			if (currentLife <= EXPAND_FRAMES) {
				// 放大阶段：前 EXPAND_FRAMES 帧内从 0.5 放大到 1.8
				float t = (float)currentLife / EXPAND_FRAMES;
				sizeScale = MathHelper.SmoothStep(0.5f, 1.8f, t);
			}
			else {
				// 缩小阶段：从最大尺寸 1.8 线性缩小到 0
				float shrinkProgress = (float)(currentLife - EXPAND_FRAMES) / (MAX_LIFE - EXPAND_FRAMES);
				sizeScale = MathHelper.SmoothStep(3f, 0f, shrinkProgress);
			}

			float finalScale = baseScale * sizeScale * pulse;
			Color glowColor = new Color(255, 220, 80) * glowIntensity;

			// 切换至 Additive 混合模式
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive,
				SamplerState.LinearClamp, DepthStencilState.None,
				RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

			// 外光晕（大而淡）
			float blurScale = finalScale * 2.8f;
			float blurAlpha = 0.35f * glowIntensity;
			Main.spriteBatch.Draw(starTex, center, null, glowColor * blurAlpha,
				Projectile.rotation, starTex.Size() / 2, blurScale, SpriteEffects.None, 0f);

			// 主光晕（三层渐变，边缘柔和）
			float mainScale = finalScale;
			// 外层
			Main.spriteBatch.Draw(starTex, center, null, glowColor * (0.3f * glowIntensity),
				Projectile.rotation + 0.1f, starTex.Size() / 2, mainScale * 1.1f, SpriteEffects.None, 0f);
			// 中层
			Main.spriteBatch.Draw(starTex, center, null, glowColor * (0.45f * glowIntensity),
				Projectile.rotation + 0.15f, starTex.Size() / 2, mainScale * 0.9f, SpriteEffects.None, 0f);
			// 内层
			Main.spriteBatch.Draw(starTex, center, null, glowColor * (0.55f * glowIntensity),
				Projectile.rotation + 0.2f, starTex.Size() / 2, mainScale * 0.7f, SpriteEffects.None, 0f);

			// 核心高亮
			float coreScale = finalScale * 2.2f;
			float coreIntensity = glowIntensity * 0.45f;
			Color coreColor = new Color(255, 255, 180) * coreIntensity;
			Main.spriteBatch.Draw(starTex, center, null, coreColor,
				Projectile.rotation, starTex.Size() / 2, coreScale, SpriteEffects.None, 0f);

			// 星芒
			if (glowIntensity > 0.25f) {
				float rayIntensity = glowIntensity * 0.5f;
				float rayScale = finalScale * 3.2f;
				Color rayColor = new Color(255, 200, 100) * rayIntensity;
				Main.spriteBatch.Draw(starTex, center, null, rayColor,
					Projectile.rotation + MathHelper.PiOver4, starTex.Size() / 2, rayScale, SpriteEffects.None, 0f);
			}

			// 恢复默认混合模式
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None,
				Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}