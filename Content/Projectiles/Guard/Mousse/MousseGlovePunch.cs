using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Mousse
{
	public class MousseGlovePunch : ModProjectile
	{
		public override string Texture =>
			"ArknightsMod/Content/Items/Weapons/Guard/Mousse/MousseGlove_protile";

		private const float MaxRange = 60f;
		private const int PauseTicks = 12;
		private const float PunchSpeed = 16f;
		private const float RetractSpeed = 14f;

		private ref float Phase => ref Projectile.ai[0];
		private ref float Extension => ref Projectile.ai[1];
		private ref float PauseTimer => ref Projectile.ai[2];

		public override void SetDefaults() {
			Projectile.width = 20;
			Projectile.height = 20;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.timeLeft = 120;
			Projectile.ignoreWater = true;
		}

		public override void OnSpawn(IEntitySource source) {
			Projectile.localAI[0] = Projectile.velocity.ToRotation();
		}

		public override void AI() {
			Player owner = Main.player[Projectile.owner];
			Vector2 dir = Vector2.UnitX.RotatedBy(Projectile.localAI[0]);

			if (Phase == 0) {
				Extension += PunchSpeed;
				if (Extension >= MaxRange) {
					Extension = MaxRange;
					Projectile.friendly = false;
					Phase = 1;
					PauseTimer = PauseTicks;
				}
			} else if (Phase == 1) {
				PauseTimer--;
				if (PauseTimer <= 0)
					Phase = 2;
			} else {
				Extension -= RetractSpeed;
				if (Extension <= 0) {
					Projectile.Kill();
					return;
				}
			}

			Projectile.Center = owner.MountedCenter + dir * Extension;
			Projectile.velocity = Vector2.Zero;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			var mp = Main.player[Projectile.owner].GetModPlayer<WeaponPlayer>();
			// S1 挠伤：命中目标施加虚弱并立即结束技能（一次性），随后自动重新充能
			if (mp.SkillActive && mp.Skill == 0) {
				target.AddBuff(BuffID.Weak, 5 * 60);
				mp.SkillActive = false;
			}
		}

		public override bool ShouldUpdatePosition() => false;

		public override bool PreDraw(ref Color lightColor) {
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 origin = tex.Size() / 2f;
			Vector2 pos = Projectile.Center - Main.screenPosition;
			float rot = Projectile.localAI[0];
			Main.spriteBatch.Draw(tex, pos, null, lightColor, rot, origin, Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}
	}
}
