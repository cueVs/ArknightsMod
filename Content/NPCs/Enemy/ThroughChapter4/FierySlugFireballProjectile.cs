using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ArknightsMod.Content.NPCs.Enemy.ThroughChapter4
{
	/// <summary>
	/// 炽焰源石虫喷出的火球抛物线射弹
	/// </summary>
	public class FierySlugFireballProjectile : ModProjectile
	{
		public override string Texture => "ArknightsMod/Content/Projectiles/FierySlugFireballProjectile";
		private const float Gravity = 0.22f;

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 240;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.alpha = 20;
		}

		public override void AI()
		{
			Projectile.velocity.Y += Gravity;
			Projectile.rotation = Projectile.velocity.ToRotation();

			if (Main.rand.NextBool(2))
			{
				int d = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height,
					DustID.Torch, 0f, 0f, 120, default, 1.1f);
				Main.dust[d].noGravity = true;
				Main.dust[d].velocity *= 0.4f;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
			SpawnExplosionDust();
			return true;
		}

		public override void OnHitPlayer(Player target, Player.HurtInfo info)
		{
			SpawnExplosionDust();

			// 10% 几率施加灼烧buff，持续3秒；若已有灼烧则叠加持续时间
			if (Main.rand.Next(10) == 0)
			{
				const int addDuration = 3 * 60; // 3秒
				target.buffImmune[BuffID.OnFire] = false;
				int existingTime = 0;
				for (int i = 0; i < Player.MaxBuffs; i++)
				{
					if (target.buffType[i] == BuffID.OnFire)
					{
						existingTime = target.buffTime[i];
						break;
					}
				}
				target.AddBuff(BuffID.OnFire, existingTime + addDuration);
			}
		}

		private void SpawnExplosionDust()
		{
			for (int i = 0; i < 16; i++)
			{
				Vector2 spd = Main.rand.NextVector2Circular(2.6f, 2.6f);
				int d = Dust.NewDust(Projectile.Center, 2, 2, DustID.FlameBurst, spd.X, spd.Y, 80, default, 1.15f);
				Main.dust[d].noGravity = true;
			}
			for (int i = 0; i < 8; i++)
			{
				Vector2 spd = Main.rand.NextVector2Circular(1.6f, 1.6f);
				Dust.NewDust(Projectile.Center, 2, 2, DustID.Smoke, spd.X, spd.Y, 120, default, 0.9f);
			}
		}
	}
}
