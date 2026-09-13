using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Weapons.Specialist.Gravel;
using ArknightsMod.Players;

namespace ArknightsMod.Content.Projectiles.Specialist.Gravel
{
	// S2 鼠群护盾视觉效果：边缘亮、内部暗、带扭曲动画，亮度随护盾剩余量变化
	public class GravelShieldVfx : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Items/Weapons/Specialist/Gravel/GravelDualBlades";

		private const float BaseRadius = 72f;

		public override void SetDefaults() {
			Projectile.width = 1;
			Projectile.height = 1;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.timeLeft = int.MaxValue;
			Projectile.hide = true;
			Projectile.alpha = 255;
		}

		public override void AI() {
			Player player = Main.player[Projectile.owner];
			if (!player.active || player.dead) { Projectile.Kill(); return; }

			var mp = player.GetModPlayer<WeaponPlayer>();
			if (!mp.SkillActive || mp.Skill != 1) { Projectile.Kill(); return; }

			Projectile.Center = player.Center;
		}

		public override bool PreDraw(ref Color lightColor) {
			Player player = Main.player[Projectile.owner];
			var sp = player.GetModPlayer<GravelShieldPlayer>();
			float strength = sp.ShieldRatio; // 0~1，护盾耗尽时淡出
			if (strength <= 0f) return false;

			Texture2D pixel = TextureAssets.MagicPixel.Value;
			Rectangle src = new Rectangle(0, 0, 1, 1);
			Vector2 center = player.Center - Main.screenPosition;
			float time = (float)Main.timeForVisualEffects * 0.04f;

			// 内填充圈（5 层）：边缘越近越亮
			for (int ring = 1; ring <= 5; ring++) {
				float t = ring / 6f;
				float radius = BaseRadius * t;
				float alpha = MathHelper.Lerp(0.03f, 0.18f, t) * strength;
				int segs = Math.Max(20, (int)(radius * 1.8f));

				for (int i = 0; i < segs; i++) {
					float angle = i / (float)segs * MathHelper.TwoPi;
					float wave = (float)Math.Sin(angle * 5f + time * 2f + ring * 1.3f) * 0.08f
					           + (float)Math.Sin(angle * 9f + time * 3.5f) * 0.04f;
					float r = radius * (1f + wave);
					Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * r;
					float flicker = 0.65f + (float)Math.Sin(angle * 13f + time * 5f + ring) * 0.35f;
					Main.spriteBatch.Draw(pixel, pos, src, Color.White * (alpha * flicker),
						0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
				}
			}

			// 外缘亮环
			int edgeSegs = 108;
			for (int i = 0; i < edgeSegs; i++) {
				float angle = i / (float)edgeSegs * MathHelper.TwoPi;
				float wave = (float)Math.Sin(angle * 7f + time * 2.5f) * 0.07f
				           + (float)Math.Sin(angle * 13f + time * 4.1f) * 0.04f;
				float r = BaseRadius * (1f + wave);
				Vector2 pos = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * r;
				float flicker = 0.45f + (float)Math.Sin(angle * 19f + time * 6.3f) * 0.55f;
				Main.spriteBatch.Draw(pixel, pos, src, Color.White * (0.8f * strength * flicker),
					0f, Vector2.Zero, 5f, SpriteEffects.None, 0f);
			}

			return false;
		}

		public override bool ShouldUpdatePosition() => false;
	}
}
