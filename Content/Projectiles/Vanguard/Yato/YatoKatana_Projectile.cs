using System;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using ArknightsMod.Content.Items.Weapons.Vanguard.Yato;
using Terraria.ID;



namespace ArknightsMod.Content.Projectiles.Vanguard.Yato
{
	public class YatoKatana_Projectile : ModProjectile
	{
		Player player => Main.player[Projectile.owner];
		Item item => player.HeldItem;
		int attackTime = 0;

		public override void SetStaticDefaults() {
			ProjectileID.Sets.TrailingMode[Type] = 2;
			ProjectileID.Sets.TrailCacheLength[Type] = 15;
		}
		public override void SetDefaults() {
			Projectile.width = 38;
			Projectile.height = 40;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 60;
			Projectile.hide = false;
		}
		public override bool ShouldUpdatePosition() => false;
		public override bool PreDraw(ref Color lightColor) {
			Texture2D tex = TextureAssets.Projectile[Projectile.type].Value;

			// 左下角原点
			Vector2 origin = new Vector2(0, tex.Height);

			Main.EntitySpriteDraw(
				tex,
				Projectile.Center - Main.screenPosition,
				null,
				lightColor,
				Projectile.rotation,
				origin,
				Projectile.scale,
				SpriteEffects.None,
				0
			);
			return false;
		}

		// 挥刀特效，目前特效位置不太对
		// public override void PostDraw(Color lightColor)
		// {
		// 	var vertices1 = new List<VertexData>();

		// 	float timeOffset = Main.GlobalTimeWrappedHourly % 1f;

		// 	for (int i = 0; i < 15; i += 1)
		// 	{
		// 		if (Projectile.oldPos[i] != Vector2.Zero)
		// 		{
		// 			float uvX = i / 14f;
		// 			float progress = i / 4f;

		// 			float dynamicUvX = (uvX - timeOffset * 2) % 1f;

		// 			var color = Color.Lerp(new Color(255, 237, 0), new Color(9, 161, 130), progress);

		// 			// 透明度渐变
		// 			float alphaFactor = 1f;
		// 			if (i < 3)
		// 			{
		// 				alphaFactor = i / 2f;
		// 			}
		// 			else
		// 			{
		// 				alphaFactor = 1 - i / 15f;
		// 			}
		// 			color *= alphaFactor;

		// 			vertices1.Add(new VertexData(
		// 				Projectile.oldPos[i] + Projectile.Size.RotatedBy(Projectile.oldRot[i] - 1.83f)*0.7f - Main.screenPosition + new Vector2(64, -0).RotatedBy(Projectile.oldRot[i] - 0.8f),
		// 				new Vector3(dynamicUvX, 0, 1),
		// 				color
		// 			));

		// 			vertices1.Add(new VertexData(
		// 				Projectile.oldPos[i] + Projectile.Size.RotatedBy(Projectile.oldRot[i] -1.83f)*0.7f - Main.screenPosition + new Vector2(64, 0).RotatedBy(Projectile.oldRot[i] - MathHelper.Pi -0.8f),
		// 				new Vector3(dynamicUvX, 1, 1),
		// 				color
		// 			));
		// 		}
		// 	}

		// 	for (int i = 0; i < 2; i++)
		// 	{
		// 		Main.spriteBatch.End();
		// 		Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.AnisotropicWrap,
		// 			DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		// 		Main.graphics.GraphicsDevice.Textures[0] = TextureAssets.Extra[196].Value;
		// 		if (vertices1.Count >= 5)
		// 		{
		// 			Main.graphics.GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices1.ToArray(), 0, vertices1.Count - 2);
		// 		}
		// 		Main.spriteBatch.End();
		// 		Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp,
		// 			DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		// 	}
		// }
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
			List<int> behindProjectiles, List<int> overPlayers,
			List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		public override void AI() {
			if (!player.active || player.dead || item.type != ModContent.ItemType<YatoKatana>()) {
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, 0);
				Projectile.Kill();
				return;
			}
			float t = Math.Clamp(Projectile.ai[0] / item.useTime, 0, 1);
			Projectile.ai[0] += 1;
			float rotation = 0f;
			float startRotation = -1.95f;
			float startRotation_opposite = (float)Math.PI / 9f;

			if (t <= 0.05f) {
				if (player.controlUseItem) {
					attackTime = item.useTime;
					Projectile.friendly = true;
				}
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.direction *1.05f);
				Projectile.rotation = player.direction == 1 ? startRotation : startRotation_opposite;
			}
			if (attackTime > 0) {
				if (t <= 0.2f) {
					// 取刀
					Projectile.position = player.Center + new Vector2(player.direction == 1 ? 0 : -40, 10) + new Vector2(0, -10 - 10f * (t / 0.2f));
					player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.direction * MathHelper.Lerp(-(float)Math.PI*7/18, -(float)Math.PI*10/18,t / 0.2f));
					Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite) - player.direction * MathHelper.Lerp(0, 0.52f, t / 0.2f);
				}
				else if (t <= 0.5f) {
					// 小小先快后慢一下
					float u = (t - 0.2f) / 0.3f;
					t = 0.2f + 0.3f * (1 - (1 - u) * (1 - u));
					// 挥刀
					// 这个函数是人写出来的呀，，好吧确实是给三个点让ai写的,由(0,-20)到(-70,-40)
					Projectile.position = (player.direction == 1 ? player.Center : player.Center + new Vector2(-40, 0)) + new Vector2(
						(1.25f - 30 * (t - 0.25f) * (t - 0.25f) / 0.06f) * player.direction,
						(-11666.67f * (t - 0.4f) * (t - 0.4f) * (t - 0.4f) - 2833.33f * (t - 0.4f) * (t - 0.4f))
					);
					player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.direction * MathHelper.Lerp(-(float)Math.PI*10/18, (float)Math.PI * 11 / 18,(t-0.2f) / 0.3f));
					Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite) + player.direction * MathHelper.Lerp(-0.52f, 6.28f, (t - 0.2f) / 0.3f);
				}
				else if (t <= 0.6f) {
					// 停刀
					Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite) + player.direction * 6.28f;
					player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, player.direction * (float)Math.PI * 11 / 18);					
					Projectile.position = player.Center + new Vector2(player.direction == 1 ? 0 : -40, 10) + new Vector2(
						-30 * player.direction,
						-50
					);
				}
				else if (t <= 0.8f) {
					// 收刀1
					float nt = (t - 0.6f) / 0.2f;   // 0~1
					nt = (MathF.Pow(10, nt) - 1) / 9f;
					Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite) - player.direction * MathHelper.Lerp(-6.28f, -4f, nt);
					Projectile.position = player.Center + new Vector2(player.direction == 1 ? 0 : -40, 10) + new Vector2((-40 * (0.8f - t) / 0.2f +10) * player.direction, -2479f * (t - 0.73f) * (t - 0.73f) - 10f);
					player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,  player.direction * MathHelper.Lerp((float)Math.PI * 11 / 18,-(float)Math.PI/6, nt));					

				}
				else if (t <= 0.85f) {
					// 收刀2
					Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite) - player.direction * MathHelper.Lerp(-4f, 0.52f, (t - 0.8f) / 0.05f);
					Projectile.position = player.Center + new Vector2(player.direction == 1 ? 0 : -40, 10) + new Vector2(10, -30);
					player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full,  player.direction * MathHelper.Lerp(-(float)Math.PI/6,-(float)Math.PI*10/18, (t - 0.8f) / 0.05f));					

				}
				else {
					// 入鞘
					Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite) - player.direction * (float)Math.PI / 6f;
					Projectile.position = player.Center + new Vector2(player.direction == 1 ? 0 : -40, 10) + new Vector2((10 * (1 - t) / 0.15f) * player.direction, -20 - 10 * (1 - t) / 0.15f);
					player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.ThreeQuarters,  player.direction *-(float)Math.PI*10/18);					

				}

			}
			else {

				Projectile.rotation = player.direction == 1 ? startRotation : startRotation_opposite;
				Projectile.position = player.Center +new Vector2(player.direction == 1 ? 0 : -40, 10) +new Vector2(0,-10);
				if (player.controlUseItem) {
					attackTime = item.useTime;
				}
				//重新开始普攻流程
				Projectile.ai[0] = 0;
			}
			if (t == 1) {
				Projectile.ai[0] = 0;
			}
			if (player.controlUseItem || player.controlUseTile) {
				Projectile.timeLeft = item.useTime;
			}


			attackTime--;
			if (attackTime<=0){
				Projectile.friendly = false;
			}

		}

	}

	public class YatoKatanaSheath_Projectile : ModProjectile
	{
		Player player => Main.player[Projectile.owner];
		Item item => player.HeldItem;
		int attackTime = 0;
		public override void SetDefaults() {
			Projectile.width = 38;
			Projectile.height = 40;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.timeLeft = 60;
			Projectile.hide = false;
		}
		public override bool ShouldUpdatePosition() => false;

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,
			List<int> behindProjectiles, List<int> overPlayers,
			List<int> overWiresUI) {
			overPlayers.Add(index);
		}
		public override void AI() {
			if (!player.active || player.dead || item.type != ModContent.ItemType<YatoKatana>()) {
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.None, 0);
				Projectile.Kill();
				return;
			}
			float t = Math.Clamp(Projectile.ai[0] / item.useTime, 0, 1);
			Projectile.ai[0] += 1;
			float startRotation = (float)Math.PI * 5 / 12f;
			float startRotation_opposite = (float)Math.PI * 13 / 12f;

			if (t <= 0.05f) {
				if (player.controlUseItem) {
					attackTime = item.useTime;
				}
				player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, -0.52f);
				Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite);
			}
			if (attackTime > 0) {
				if (t < 0.2f) {
					Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite) - player.direction * MathHelper.Lerp(0, (float)Math.PI / 4f, (t) / 0.2f);
					Projectile.position = player.Center + new Vector2(player.direction == 1 ? 0 : 30, player.direction == 1 ? 10 : 0) + new Vector2(-32, -20 + 8 * (t) / 0.2f);
				}
				else if (t < 0.5f) {
					Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite) - player.direction * MathHelper.Lerp((float)Math.PI / 4f, -(float)Math.PI / 6f, (t - 0.2f) / 0.3f);
					Projectile.position = player.Center + new Vector2(player.direction == 1 ? 0 : 30, player.direction == 1 ? 10 : 0) + new Vector2(-32, -12);
				}
				else if (t < 0.75f) {
					Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite) + player.direction * (float)Math.PI / 6f;
					Projectile.position = player.Center + new Vector2(player.direction == 1 ? 0 : 30, player.direction == 1 ? 10 : 0) + new Vector2(-32, -12);
				}
				else if (t < 0.85f) {
					Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite) - player.direction * MathHelper.Lerp(-(float)Math.PI / 6f, (float)Math.PI / 6f, (t - 0.75f) / 0.1f);
					Projectile.position = player.Center + new Vector2(player.direction == 1 ? 0 : 30, player.direction == 1 ? 10 : 0) + new Vector2(-32, -12);
				}
				else {
					//pi/4也不对，pi/6也不对
					Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite) - player.direction * (float)Math.PI / 5f;
					Projectile.position = player.Center + new Vector2(player.direction == 1 ? 0 : 30, player.direction == 1 ? 10 : 0) + new Vector2(-32, -12);
				}
			}
			else {
				// 不是很理解为什么position.y要再减10才流畅一点。。
				Projectile.rotation = (player.direction == 1 ? startRotation : startRotation_opposite);
				Projectile.position = player.Center + new Vector2(player.direction == 1 ? 0 : 30, player.direction == 1 ? 10 : 0) + new Vector2(-32, -22);
				if (player.controlUseItem) {
					attackTime = item.useTime;
				}
				//重新开始普攻流程
				Projectile.ai[0] = 0;
			}
			if (t == 1) {
				Projectile.ai[0] = 0;
			}


			if (player.controlUseItem || player.controlUseTile) {
				Projectile.timeLeft = item.useTime;
			}
			attackTime--;
		}

	}
}
