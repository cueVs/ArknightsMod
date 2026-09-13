using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Goldenglow
{
	// 澄闪魔法弹：玩家法杖与信标共用同一弹幕，金色发光小球
	public class GoldenglowBolt : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Items/Weapons/Caster/Goldenglow/GoldenglowWand";

		public override void SetDefaults() {
			Projectile.width = 10;
			Projectile.height = 10;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.timeLeft = 90;
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.light = 0.5f;
		}

		public override void AI() {
			// 金色粒子尾迹
			if (Main.rand.NextBool(2)) {
				int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
					DustID.GoldFlame, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 150, default, 0.9f);
				Main.dust[dust].noGravity = true;
			}
		}

		public override void OnKill(int timeLeft) {
			for (int i = 0; i < 6; i++) {
				int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
					DustID.GoldFlame, 0f, 0f, 100, default, 1.2f);
				Main.dust[dust].noGravity = true;
				Main.dust[dust].velocity *= 1.5f;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			// 用法杖贴图的同时叠加发光，用简单缩放模拟光球外观
			Texture2D tex = ModContent.Request<Texture2D>(
				"ArknightsMod/Content/Items/Weapons/Caster/Goldenglow/GoldenglowWand").Value;
			Vector2 origin = tex.Size() / 2f;
			Vector2 pos = Projectile.Center - Main.screenPosition;
			// 用白色叠加让弹幕看起来像光球
			Main.spriteBatch.Draw(tex, pos, null, new Color(255, 230, 120, 200),
				Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);
			return false;
		}
	}
}
