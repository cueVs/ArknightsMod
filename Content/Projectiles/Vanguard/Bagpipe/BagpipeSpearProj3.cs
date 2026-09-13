using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Color = Microsoft.Xna.Framework.Color;

namespace ArknightsMod.Content.Projectiles.Vanguard.Bagpipe
{
	public class BagpipeSpearProj3 : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Projectiles/Vanguard/Bagpipe/BagpipeProj";
		public override void SetDefaults() {
			Projectile.extraUpdates = 1;
			Projectile.width = 40;// 弹幕判定体积的宽(碰撞箱)
			Projectile.height = 40;//弹幕判定体积的高
			Projectile.scale = 1f;//放大弹幕
			Projectile.friendly = false;//是否对敌对NPC造成伤害
			Projectile.DamageType = DamageClass.Melee;//伤害类型
			Projectile.ignoreWater = true;//弹幕在水里的时候会不会减速 
			Projectile.tileCollide = false;//弹幕会不会穿墙
			Projectile.timeLeft = 30;//消散时间60=1秒
			Projectile.penetrate = -1;//弹幕打中几个怪物之后会消失
			Projectile.alpha = 255;//弹幕的透明度
			Projectile.light = 0.5f;//弹幕光照的强度
			Projectile.usesLocalNPCImmunity = true;//NPC是不是按照弹幕ID来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则八个都能击中，不骗伤，原版夜明弹的反骗伤就是如此）
			Projectile.localNPCHitCooldown = 20;//上一个设定为true则被调用，NPC按照弹幕ID来获取多少无敌帧
			Projectile.usesIDStaticNPCImmunity = false;//NPC是不是按照弹幕类型来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则只能击中一次，其余的会穿透，原版用它来控制喽啰的输出上限）
			Projectile.idStaticNPCHitCooldown = 60;//上一个设定为true则被调用，NPC按照弹幕类型来获取多少无敌帧
		}
		public override void OnSpawn(IEntitySource source) {
			Projectile.ai[0] = Projectile.timeLeft;
			Projectile.rotation = Projectile.velocity.ToRotation() + 3.14f / 2f;
		}
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			player.heldProj = -1;
			if (Projectile.ai[1] > 0) {
				Projectile.friendly = true;
				//Player player = Main.player[Projectile.owner];
				Projectile.Center += player.velocity / 2f;
				if (Main.rand.NextBool(8)) {
					//for (int i = 0; i < 2; i++)
					{
						var v = Dust.NewDustDirect(player.Center + Projectile.velocity * 2, 0, 0, 10, 0f, 0f, 0, new Color(255, 220, 30, 255), 1 + Main.rand.Next(1, 8) / 10f);
						v.noGravity = true;
						v.velocity = -Projectile.velocity.RotatedBy(Main.rand.Next(-31, 32) / 60f) * (2.5f + Main.rand.Next(1, 6) / 10f) / 1.5f; //
					}
				}
			}
			base.AI();
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,//这里是用来改弹幕图层的
		List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overWiresUI.Add(index);
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];

			Texture2D 贴图 = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
			int d = (int)(200f / Projectile.ai[0] * Projectile.timeLeft);
			int d2 = (int)(d * .75f);

			Main.spriteBatch.Draw(贴图, Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f,
			null, new Color(d2 + 75, d, d2 / 4, d2 / 4 + 180), Projectile.rotation, 贴图.Size() / 2f,
			new Vector2(1.5f / Projectile.ai[0] * Projectile.timeLeft, 2.5f) / .75f
			, SpriteEffects.None, 0);//绘制

			Main.spriteBatch.Draw(贴图, Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f,
			null, new Color(d + 55, d, d / 2, 10), Projectile.rotation, 贴图.Size() / 2f,
			new Vector2(1.5f / Projectile.ai[0] * Projectile.timeLeft, 2.5f) / 1f
			, SpriteEffects.None, 0);//绘制

			var blendStatef2 = new BlendState()//配置反色混合状态
			{
				AlphaBlendFunction = BlendState.Additive.AlphaBlendFunction,
				AlphaDestinationBlend = BlendState.Additive.AlphaDestinationBlend,
				AlphaSourceBlend = BlendState.Additive.AlphaSourceBlend,
				ColorBlendFunction = BlendFunction.ReverseSubtract,
				ColorDestinationBlend = BlendState.Additive.ColorDestinationBlend,
				ColorSourceBlend = BlendState.Additive.ColorSourceBlend,
				ColorWriteChannels = ColorWriteChannels.All,
				ColorWriteChannels1 = ColorWriteChannels.All,
				ColorWriteChannels2 = ColorWriteChannels.All,
				ColorWriteChannels3 = ColorWriteChannels.All,
				BlendFactor = Color.White,
				MultiSampleMask = -1
			};
			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, blendStatef2, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			Main.spriteBatch.Draw(贴图, Projectile.Center - Main.screenPosition + Projectile.velocity.SafeNormalize(Vector2.Zero) * 6f,
			null, new Color(d + 5, d + 25, d + 55, 180), Projectile.rotation, 贴图.Size() / 2f,
			new Vector2(1.5f / Projectile.ai[0] * Projectile.timeLeft, 2.5f) / 1.75f
			, SpriteEffects.None, 0);//绘制

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);


			return true;
		}
	}
}