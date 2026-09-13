using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Hongdou
{
	public class HongdouSpearStab : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Items/Weapons/Guard/Hongdou/HongdouLance_protile";

		protected virtual float HoldoutRangeMin => 20f;
		protected virtual float HoldoutRangeMax => 52f;

		public override void SetDefaults() {
			Projectile.CloneDefaults(ProjectileID.Spear);
			Projectile.width = 70;
			Projectile.height = 70;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
		}

		public override bool PreAI() {
			Player player = Main.player[Projectile.owner];
			int duration = player.itemAnimationMax;

			player.heldProj = Projectile.whoAmI;

			if (Projectile.timeLeft > duration)
				Projectile.timeLeft = duration;

			Projectile.velocity = Vector2.Normalize(Projectile.velocity);

			float returnDuration = duration * 0.8f;
			float progress;

			if (Projectile.timeLeft < returnDuration)
				progress = Projectile.timeLeft / returnDuration;
			else
				progress = 1 - (Projectile.timeLeft - returnDuration) / (duration - returnDuration);

			Projectile.Center = player.MountedCenter + Vector2.SmoothStep(
				Projectile.velocity * HoldoutRangeMin,
				Projectile.velocity * HoldoutRangeMax,
				progress);

			// rotation 仅供原版碰撞箱参考，实际绘制由 PreDraw 覆盖
			Projectile.rotation = Projectile.velocity.ToRotation();

			return false;
		}

		public override bool PreDraw(ref Color lightColor) {
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			float ang = Projectile.velocity.ToRotation();
			// 以贴图中心为原点，使碰撞箱与贴图视觉位置一致
			Vector2 origin = tex.Size() / 2f;
			Vector2 drawPos = Projectile.Center - Main.screenPosition;
			Main.spriteBatch.Draw(tex, drawPos, null, lightColor, ang, origin, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
