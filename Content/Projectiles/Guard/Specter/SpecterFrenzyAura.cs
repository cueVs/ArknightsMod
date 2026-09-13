using ArknightsMod.Content.Items.Weapons.Guard.Specter;
using ArknightsMod.Content.Projectiles.BasePROJ;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Specter
{
	/// <summary>「肉斩骨断」的深海/血色双层光环；无碰撞、无伤害。</summary>
	public sealed class SpecterFrenzyAura : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Textures/circle_03";

		public override void SetDefaults() {
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
			Projectile.netImportant = true;
			Projectile.timeLeft = 2;
		}

		public override bool ShouldUpdatePosition() => false;

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			if (!owner.active || owner.dead) {
				Projectile.Kill();
				return;
			}

			Projectile.Center = owner.Center;
			Projectile.rotation += 0.025f;
			Projectile.localAI[0]++;

			if (owner.whoAmI == Main.myPlayer && Projectile.ai[0] == 0f
				&& !owner.GetModPlayer<SpecterBoneSawPlayer>().Skill2Active) {
				Projectile.ai[0] = 1f;
				Projectile.localAI[1] = 0f;
				Projectile.netUpdate = true;
			}

			if (Projectile.ai[0] > 0f) {
				Projectile.localAI[1]++;
				if (Projectile.localAI[1] >= 20f) {
					Projectile.Kill();
					return;
				}
			}

			Projectile.timeLeft = 2;
			if (!Main.dedServ && Projectile.ai[0] == 0f && (int)Projectile.localAI[0] % 5 == 0)
				SpecterVisuals.SpawnFrenzyMotes(owner.Center, 3);
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			float fadeIn = MathHelper.Clamp(Projectile.localAI[0] / 12f, 0f, 1f);
			float fadeOut = Projectile.ai[0] > 0f
				? 1f - MathHelper.Clamp(Projectile.localAI[1] / 20f, 0f, 1f)
				: 1f;
			BaseHeldMeleeSupport.BeginAdditive(Main.spriteBatch);
			SpecterVisuals.DrawFrenzyAura(Projectile.Center, Projectile.rotation, fadeIn * fadeOut);
			BaseHeldMeleeSupport.EndAdditive(Main.spriteBatch);
			return false;
		}
	}
}
