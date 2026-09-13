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
	public class BagpipeSpearProj2 : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Projectiles/Vanguard/Bagpipe/BagpipeProj2";
		public override void SetDefaults() {
			Projectile.CloneDefaults(49);
			AIType = 49;
			Projectile.width = 110;// 弹幕判定体积的宽(碰撞箱)
			Projectile.height = 110;//弹幕判定体积的高
			Projectile.DamageType = DamageClass.Melee;//伤害类型
			Projectile.scale = 1f;
			Projectile.usesLocalNPCImmunity = true;//NPC是不是按照弹幕ID来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则八个都能击中，不骗伤，原版夜明弹的反骗伤就是如此）
			Projectile.localNPCHitCooldown = 15;//上一个设定为true则被调用，NPC按照弹幕ID来获取多少无敌帧
			Projectile.usesIDStaticNPCImmunity = false;//NPC是不是按照弹幕类型来获取无敌帧？（如果设定为true，玩家发射8个该弹幕同时击中敌人，则只能击中一次，其余的会穿透，原版用它来控制喽啰的输出上限）
			Projectile.idStaticNPCHitCooldown = 60;//上一个设定为true则被调用，NPC按照弹幕类型来获取多少无敌帧
		}
		bool jjj = false;
		int max = 0;
		int hh = 20;
		int hhmax = 20;
		Vector2 cs = new Vector2(0);
		public override void OnKill(int timeLeft) {
			Player player = Main.player[Projectile.owner];
			if (Projectile.ai[2] == 3)
				Projectile.NewProjectile(player.GetSource_Death(), player.Center, new Vector2(0, 0),
				 ModContent.ProjectileType<BagpipeSpearProj4>(),
				 0, 0, Main.myPlayer, 5);
			base.OnKill(timeLeft);
		}
		public override void OnSpawn(IEntitySource source) {
			Player player = Main.player[Projectile.owner];
			max = player.itemTime;
			cs = Projectile.velocity.SafeNormalize(Vector2.Zero);

		}
		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (jjj == false) {
				jjj = true;
				Player player = Main.player[Projectile.owner];
				Projectile.NewProjectile(player.GetSource_Death(), Projectile.Center + cs * 22 + new Vector2(0, 5)
				  , cs, ModContent.ProjectileType<BagpipeSpearProj3>(), 0, 0, Main.myPlayer, 0, 0);
			}

		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,//这里是用来改弹幕图层的
		List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => overWiresUI.Add(index);

		public override void AI() {
			if (Projectile.ai[2] == 1 || Projectile.ai[2] == 4)
				Projectile.Center -= Projectile.velocity * 3.7f;
			if (Projectile.ai[2] == 3)
				Projectile.Center += Projectile.velocity * 7.5f;
			Player player = Main.player[Projectile.owner];
			player.heldProj = -1;
			if (hh > 0)
				hh--;
			if (Projectile.ai[2] == 4) {
				if (player.itemTime < 3 || player.itemAnimation < 3)
					Projectile.active = false;
			}
			//if (jjj == false && Projectile.ai[0] >= player.itemTime / 3.5f && Projectile.ai[1] == 1)
			//{
			//    jjj = true;
			//    //Projectile.NewProjectile(player.GetSource_Death(), Projectile.Center+ Projectile.velocity.SafeNormalize(Vector2.Zero)*22
			//    //  , Projectile.velocity.SafeNormalize(Vector2.Zero), ModContent.ProjectileType<fddcm3>(), 0, 0, Main.myPlayer);
			//}
		}
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			float jd = player.direction == 1 ? 3.14f * 1.25f : 3.14f * .75f;
			int gg = player.direction == 1 ? 1 : 0;
			Texture2D 贴图1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Vanguard/Bagpipe/BagpipeProj2").Value;
			float jd1 = Projectile.velocity.ToRotation() + (player.direction == 1 ? 3.14f * 2.25f : 3.14f * .75f);

			Main.spriteBatch.Draw(贴图1, Projectile.Center - Main.screenPosition - Vector2.Normalize(Projectile.velocity) * 55f + new Vector2(0, 5)
				, null, new Color(1f, 1f, 1f), jd1, 贴图1.Size() / 2f, 1, (SpriteEffects)gg, 0);//绘制
			if (Projectile.ai[0] >= player.itemTime / Projectile.ai[1]) {

				Texture2D 贴图 = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
				int d = (int)(200f / hhmax * hh);
				int d2 = (int)(d * .75f);
				Main.spriteBatch.Draw(贴图, Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f + new Vector2(0, 5),
			null, new Color(d2 + 75, d, d2 / 4, d2 / 4 + 180), Projectile.rotation + jd, 贴图.Size() / 2f,
			new Vector2(1.5f / hhmax * hh, 2.5f) / .75f
			, SpriteEffects.None, 0);//绘制

				Main.spriteBatch.Draw(贴图, Projectile.Center - Main.screenPosition - Projectile.velocity.SafeNormalize(Vector2.Zero) * 2f + new Vector2(0, 5),
			  null, new Color(d + 55, d, d / 2, 10), Projectile.rotation + jd, 贴图.Size() / 2f,
			  new Vector2(1.5f / hhmax * hh, 2.5f) / 1.5f
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

				Main.spriteBatch.Draw(贴图, Projectile.Center - Main.screenPosition + Projectile.velocity.SafeNormalize(Vector2.Zero) * 6f + new Vector2(0, 5),
				null, new Color(d + 5, d + 25, d + 55, 180), Projectile.rotation + jd, 贴图.Size() / 2f,
				new Vector2(1.5f / hhmax * hh, 2.5f) / 2f
				, SpriteEffects.None, 0);//绘制

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

			}
			return false;
		}
	}
}