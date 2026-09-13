using System;
using System.Collections.Generic;
using ArknightsMod.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Wisadel
{
	public class WisdelHitNormal : ModProjectile
	{
		public override string Texture => ArknightsMod.noTexture;
		public override void SetDefaults()
		{
			Projectile.width = 32;
			Projectile.height = 32;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 60;
			Projectile.scale = 1f;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 20;
			Projectile.noEnchantmentVisuals = true;
		}
		public int timer;
		
		public override void AI()
		{
			timer++;

			if (Projectile.ai[0] == 666 && timer == 15)
			{
				WisdelShotNormal.Aftershock(Projectile, Main.player[Projectile.owner], Projectile.damage);
			}
		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) {

			overPlayers.Add(index);
		}
		public override bool ShouldUpdatePosition()
		{
			return false;
		}

		
		public override bool? CanDamage() => false;
		
		public override bool PreDraw(ref Color lightColor)
		{
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.Default,
				Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

			Texture2D tex1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Guard/Saki/Extra_98").Value;
			// TOTEST 这里真的是Saki吗
			Texture2D bloomTexture = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Sniper/Wisadel/star_02").Value;
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;
			Vector2 origin = tex1.Size() * 0.5f;

			float sca = EaseFunction.Ease(timer, 40, 60, 1f, 0f);
			sca = Math.Clamp(sca, 0f, 1f);
			Vector2 scale = new Vector2(0.4f, 2f) * sca;



			float opacity = EaseFunction.QuadraticEase(Projectile.timeLeft, 35, 60f, 0f, 0.75f, true, 0.8f);
			opacity = Math.Clamp(opacity, 0f, 1f);
			float bloomScale = EaseFunction.QuadraticEase(Projectile.timeLeft, 20f, 60f, 0f, 0.8f, true, 0.8f);
			Vector2 bloomOrigin = bloomTexture.Size() * 0.5f;
			float bloomOpacity = EaseFunction.QuadraticEase(Projectile.timeLeft, 20f, 60f, 0f, 1f, true, 0.8f);

			var color = new Color(249, 90, 100);

			Main.spriteBatch.Draw(tex1, drawPosition, null,
				color * opacity, Projectile.rotation, origin, scale, 0, 0f);
			Main.spriteBatch.Draw(tex1, drawPosition, null,
				color * opacity, Projectile.rotation + MathHelper.PiOver2, origin, scale, 0, 0f);


			Main.spriteBatch.Draw(bloomTexture, drawPosition, null,
				color * bloomOpacity, Projectile.rotation + MathHelper.PiOver2, bloomOrigin, bloomScale, 0, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.Default,
				Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);

			return false;
		}
	}
}
