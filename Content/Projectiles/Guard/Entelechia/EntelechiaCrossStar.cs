using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Entelechia
{
	// 隐德来希·刀光修饰用「四角十字星」——纯代码绘制（MagicPixel 拉伸的十字光芒 + 柔光核心），
	// 加性混合、生命周期内先放大后收缩并淡出。无伤害、无碰撞，仅作视觉点缀。
	public class EntelechiaCrossStar : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;

		// ai[0] = 最大尺寸；ai[1] = 旋转基准
		private int LifeMax => 26;

		public override void SetDefaults() {
			Projectile.width = Projectile.height = 8;
			Projectile.friendly    = false;
			Projectile.hostile     = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate   = -1;
			Projectile.timeLeft    = LifeMax;
			Projectile.alpha       = 0;
		}

		public override void OnSpawn(Terraria.DataStructures.IEntitySource source) {
			if (Projectile.ai[0] <= 0f)
				Projectile.ai[0] = Main.rand.NextFloat(0.6f, 1.1f); // 尺寸
			Projectile.ai[1] = Main.rand.NextFloat(MathHelper.TwoPi);
		}

		public override void AI() {
			Projectile.velocity *= 0.90f;
			Projectile.rotation = Projectile.ai[1];
			Lighting.AddLight(Projectile.Center, 0.25f, 0.25f, 0.3f);
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			var pixelRect = new Rectangle(0, 0, 1, 1);
			var origin = new Vector2(0.5f, 0.5f);

			Vector2 pos = Projectile.Center - Main.screenPosition;

			// 生命进度：先快速放大（0→0.25），再缓慢收缩淡出（0.25→1）
			float age = (LifeMax - Projectile.timeLeft) / (float)LifeMax;
			float grow = MathHelper.Clamp(age / 0.25f, 0f, 1f);
			float fade = 1f - MathHelper.Clamp((age - 0.25f) / 0.75f, 0f, 1f);
			float life = grow * fade;
			if (life <= 0.001f)
				return false;

			float size = Projectile.ai[0] * (0.6f + 0.4f * grow);
			float armLen   = 22f * size * life;   // 光芒臂长
			float armThick = 2.2f * size;          // 光芒臂粗
			float rot = Projectile.rotation;

			// Additive 混合（SourceAlpha, One）：Color 整体 *life，alpha 参与亮度
			Color armCol  = new Color(255, 255, 255) * life;
			Color coreCol = new Color(255, 240, 245) * life;

			// 切到加性混合
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive,
				Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);

			// 四角十字：水平 + 垂直两条对称光芒（各画两段实现由中心向外渐细的尖角）
			for (int seg = 0; seg < 2; seg++) {
				float t = seg == 0 ? 1f : 0.5f;
				Vector2 hScale = new(armLen * t, armThick * (seg == 0 ? 1f : 0.5f));
				Vector2 vScale = new(armThick * (seg == 0 ? 1f : 0.5f), armLen * t);
				Color c = armCol * (seg == 0 ? 1f : 0.8f);

				Main.spriteBatch.Draw(pixel, pos, pixelRect, c, rot, origin, hScale, SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(pixel, pos, pixelRect, c, rot, origin, vScale, SpriteEffects.None, 0f);
			}

			// 柔光核心
			float coreSize = 4.5f * size * life;
			Main.spriteBatch.Draw(pixel, pos, pixelRect, coreCol, rot, origin, new Vector2(coreSize), SpriteEffects.None, 0f);

			// 还原默认批次
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
				Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone,
				null, Main.GameViewMatrix.TransformationMatrix);

			return false;
		}
	}
}
