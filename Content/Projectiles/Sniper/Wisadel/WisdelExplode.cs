using System;
using System.Collections.Generic;
using ArknightsMod.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Wisadel
{
	public class WisdelExplode : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;
		public override void SetDefaults() {
			Projectile.width = 48;
			Projectile.height = 48;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.MeleeNoSpeed;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 120;
			Projectile.scale = 1f;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.noEnchantmentVisuals = true;
		}
		public int timer;
		public override void AI() {
			if (Projectile.ai[0] > 0) {
				Projectile.ai[0]--;
				Projectile.timeLeft = 120;
				return;
			}
			if (Projectile.ai[1] == 0) {
				if (Main.netMode != NetmodeID.Server) {
					SoundEngine.PlaySound(Wisdel_Probe.Explode.WithVolumeScale(1.5f), Projectile.position);
				}
				Projectile.ai[1] = 1;
			}
			Player player = Main.player[Projectile.owner];
			timer++;
			if (timer == 5) {
				player.wisdel().ShakeTime = 12;
			}
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {

			overPlayers.Add(index);
		}
		public override bool ShouldUpdatePosition() {
			return false;
		}
		public override bool? CanDamage() => false;

		/// <summary>
		/// 震荡函数
		/// </summary>
		/// <param name="x"></param>
		/// <param name="xMin"></param>
		/// <param name="xMax"></param>
		/// <param name="minY1"></param>
		/// <param name="maxY"></param>
		/// <param name="minY2"></param>
		/// <param name="overshootTime">到达峰值的时间比例</param>
		/// <param name="firstReturnTime">第一次回到minY2的时间比例</param>
		/// <param name="oscillationDecay">震荡衰减因子</param>
		/// <returns></returns>
		public static float Oscillate(float x, float xMin, float xMax,
			float minY1, float maxY, float minY2,
			float overshootTime = 0.1f, float firstReturnTime = 0.3f, float oscillationDecay = 4.0f) {
			// 归一化
			float t = (x - xMin) / (xMax - xMin);
			// 参数验证
			if (t < 0)
				return minY1;
			if (t > 1)
				return minY2;

			// 从minY1快速上升到maxY
			if (t <= overshootTime) {
				// 使用指数增长快速达到峰值
				float progress = t / overshootTime;
				return minY1 + (maxY - minY1) * (float)Math.Pow(progress, 0.2f);
			}

			// 从maxY下降到minY2
			else if (t <= firstReturnTime) {
				float progress = (t - overshootTime) / (firstReturnTime - overshootTime);
				// 使用缓动函数实现下降
				float easeProgress = 1 - (float)Math.Pow(1 - progress, 2);
				return maxY + (minY2 - maxY) * easeProgress;
			}

			// 在minY2周围小幅震荡两次
			else {
				float progress = (t - firstReturnTime) / (1 - firstReturnTime);

				// 计算衰减的震荡
				float oscillation = (float)Math.Exp(-oscillationDecay * progress) *
								   (float)Math.Sin(4 * Math.PI * progress);

				// 将震荡幅度限制在(minY2 - minY1)的10%范围内
				float amplitude = 0.1f * Math.Abs(minY2 - minY1);
				return minY2 + amplitude * oscillation;
			}
		}
		public override bool PreDraw(ref Color lightColor) {
			if (Projectile.ai[0] > 0) {
				return false;
			}
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.Default,
				Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

			Texture2D texbase = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Sniper/Wisadel/WisdelExplodeBase").Value;
			Texture2D tex = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Sniper/Wisadel/WisdelExplode").Value;
			Texture2D texglow = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Sniper/Wisadel/WisdelExplode_Glow").Value;
			Texture2D hightlight = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Sniper/Wisadel/star_02").Value;
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;

			drawPosition += new Vector2(0, -40);
			Vector2 origin = Utils.Size(tex) * 0.5f;
			Vector2 Scale = new Vector2(0.9f, 1) * 1.5f;


			float bloomScale = Oscillate(timer, 0, 28, 0.5f, 0.9f, 0.6f, overshootTime: 0.1f, oscillationDecay: 6);
			bloomScale = Math.Clamp(bloomScale, 0f, 1f);
			float bloomOpacity = EaseFunction.Ease(timer, 0, 30, 0.8f, 0f);
			bloomOpacity = Math.Clamp(bloomOpacity, 0f, 1f);
			Color bloomColor = Color.Lerp(Color.White, new Color(170, 108, 210), timer / 10f);

			Main.spriteBatch.Draw(texglow, drawPosition, null,
				bloomColor * bloomOpacity, Projectile.rotation, origin, bloomScale * Scale, 0, 0f);


			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default,
				Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);


			float scale = Oscillate(timer, 0, 28, 0.75f, 1.2f, 0.6f, overshootTime: 0.1f, firstReturnTime: 0.4f, oscillationDecay: 3);
			scale = Math.Clamp(scale, 0.5f, 1.2f);
			float opacity = EaseFunction.Ease(timer, 60, 120, 1f, 0f);
			opacity = Math.Clamp(opacity, 0f, 1f);
			Color color = Color.White;

			float baseOpacity = EaseFunction.Ease(timer, 5, 50, 2f, 0f);
			float baseScale = EaseFunction.Ease(timer, 0, 5, 0, 0.6f);
			baseScale = Math.Clamp(baseScale, 0f, 0.6f);
			Main.spriteBatch.Draw(texbase, drawPosition + new Vector2(texbase.Size().Y / 10, -texbase.Size().Y / 8), null,
					color * baseOpacity, Projectile.rotation, new Vector2(texbase.Width / 2, 0), Scale * baseScale, 0, 0f);

			Main.spriteBatch.Draw(tex, drawPosition, null,
				color * opacity, Projectile.rotation, origin, scale * Scale, 0, 0f);


			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.Default,
				Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);


			drawPosition += new Vector2(0, -20);

			Vector2 hlOrigin = hightlight.Size() / 2;
			float hlScale = Oscillate(timer, 0, 28, 2f, 5f, 4f, overshootTime: 0.1f, oscillationDecay: 6);
			hlScale = Math.Clamp(hlScale, 0f, 100f);
			Color hlColor = new Color(249, 90, 100);
			float hlOpacity = EaseFunction.Ease(timer, 5, 60, 1f, 0f);
			hlOpacity = Math.Clamp(hlOpacity, 0f, 1f);

			Main.spriteBatch.Draw(hightlight, drawPosition, null,
				hlColor * hlOpacity, Projectile.rotation, hlOrigin, hlScale * Scale, 0, 0f);


			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default,
				Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);


			return false;
		}
	}
}
