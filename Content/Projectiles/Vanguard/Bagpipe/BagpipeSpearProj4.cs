using ArknightsMod.Content.Items.Weapons.Vanguard.Bagpipe;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Color = Microsoft.Xna.Framework.Color;

namespace ArknightsMod.Content.Projectiles.Vanguard.Bagpipe
{
	public class BagpipeSpearProj4 : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Projectiles/Vanguard/Bagpipe/BagpipeProj";
		public override void SetDefaults() {
			//Projectile.extraUpdates = 1;
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
		}
		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs,//这里是用来改弹幕图层的
 List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCsAndTiles.Add(index);
		public override void AI() {
			Player player = Main.player[Projectile.owner];
			Projectile.velocity = new Vector2(0);
			Projectile.Center = player.Center;
			player.heldProj = -1;
			//   if (Projectile.ai[0] > 0) Projectile.ai[0]--;
			if (!player.controlUseItem && player.HeldItem.type == ModContent.ItemType<BagpipeSpear>()) {
				Projectile.timeLeft = 2;
			}


			if (Projectile.ai[0] > 0) {
				Projectile.ai[0]--;
				Projectile.timeLeft = 2;
				player.itemTime =
				player.itemAnimation = 2;
			}
			// player.itemRotation = 3.14f * 2.125f;
			player.SetCompositeArmFront(true, (Player.CompositeArmStretchAmount)1, 3.14f * 1f);
		}
		public override void OnKill(int timeLeft) {
			Player player = Main.player[Projectile.owner];
			player.SetCompositeArmFront(true, 0, 0);
		}
		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			// float jd = player.direction == 1 ? 3.14f * 1.25f : 3.14f * .75f;
			int gg = player.direction == 1 ? 0 : 1;
			//int gg = player.direction == 1 ? 1 : 0;
			Texture2D 贴图1 = ModContent.Request<Texture2D>("ArknightsMod/Content/Projectiles/Vanguard/Bagpipe/BagpipeProj3").Value;
			float jd1 = player.direction == 1 ? 3.14f * .125f : 3.14f * 1.825f;
			if (!player.controlUseItem && player.HeldItem.type == ModContent.ItemType<BagpipeSpear>() || Projectile.ai[0] > 0)
				Main.spriteBatch.Draw(贴图1, Projectile.Center - Main.screenPosition + new Vector2(-10 * player.direction, -20)
				, null, new Color(1f, 1f, 1f), jd1, 贴图1.Size() / 2f, 1, (SpriteEffects)gg, 0);//绘制
			return false;
		}
	}
}