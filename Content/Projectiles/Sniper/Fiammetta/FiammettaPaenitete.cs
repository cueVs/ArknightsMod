using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Fiammetta
{
	/// <summary>
	/// S2“你须愧悔”的正面燃烧弹。终点负责重爆炸，途中五枚灼痕在炮弹抵达后由近到远继续连爆。
	/// </summary>
	public sealed class FiammettaScorchingShell : ModProjectile
	{
		private const int MarkCount = 5;
		private Vector2 startPosition;
		private int duration;
		private int spawnedMarks;
		private bool initialized;
		private bool reachedTarget;

		private Vector2 Target => new(Projectile.ai[0], Projectile.ai[1]);

		public override string Texture => $"Terraria/Images/Projectile_{ProjectileID.RocketI}";

		public override void SetStaticDefaults()
		{
			ProjectileID.Sets.TrailCacheLength[Type] = 26;
			ProjectileID.Sets.TrailingMode[Type] = 2;
		}

		public override void SetDefaults()
		{
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 90;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.netImportant = true;
		}

		public override bool ShouldUpdatePosition() => false;
		public override bool? CanDamage() => false;

		public override void AI()
		{
			if (!initialized)
			{
				initialized = true;
				startPosition = Projectile.Center;
				duration = Math.Max(18, (int)MathF.Ceiling(Vector2.Distance(startPosition, Target) / 18f));
			}

			float progress = MathHelper.Clamp(Projectile.localAI[0] / duration, 0f, 1f);
			float eased = 1f - MathF.Pow(1f - progress, 2.25f);
			Vector2 previous = Projectile.Center;
			Projectile.Center = Vector2.Lerp(startPosition, Target, eased);
			Projectile.velocity = Projectile.Center - previous;
			if (Projectile.velocity.LengthSquared() > 0.01f)
				Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			while (spawnedMarks < MarkCount && progress >= (spawnedMarks + 1f) / (MarkCount + 1f))
			{
				SpawnMark(spawnedMarks);
				spawnedMarks++;
			}

			Lighting.AddLight(Projectile.Center, new Vector3(1f, 0.18f, 0.025f) * 0.85f);
			if (!Main.dedServ)
			{
				for (int i = 0; i < 2; i++)
				{
					Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f),
						i == 0 ? DustID.Torch : DustID.GoldFlame,
						-Projectile.velocity * Main.rand.NextFloat(0.07f, 0.15f) + Main.rand.NextVector2Circular(0.9f, 0.9f),
						30, new Color(255, 84, 26), Main.rand.NextFloat(1f, 1.45f));
					dust.noGravity = true;
				}
			}

			Projectile.localAI[0]++;
			if (progress >= 1f)
			{
				reachedTarget = true;
				Projectile.Center = Target;
				Projectile.Kill();
			}
		}

		private void SpawnMark(int index)
		{
			if (Projectile.owner != Main.myPlayer)
				return;

			float amount = (index + 1f) / (MarkCount + 1f);
			Vector2 position = Vector2.Lerp(startPosition, Target, amount);
			int delay = Math.Max(12, duration - (int)Projectile.localAI[0]) + 22 + index * 5;
			int damage = Math.Max(1, (int)(Projectile.damage * 0.5f));
			int mark = Projectile.NewProjectile(Projectile.GetSource_FromThis(), position, Vector2.Zero,
				ModContent.ProjectileType<FiammettaScorchMark>(), damage, Projectile.knockBack * 0.45f,
				Projectile.owner, delay, index);
			if (Main.projectile.IndexInRange(mark))
				Main.projectile[mark].CritChance = Projectile.CritChance;
		}

		public override void OnKill(int timeLeft)
		{
			if (reachedTarget && Projectile.owner == Main.myPlayer)
				FiammettaExplosion.Spawn(Projectile.GetSource_FromThis(), Target, Projectile.damage,
					Projectile.knockBack, Projectile.owner, 2, Projectile.CritChance);
		}

		public override bool PreDraw(ref Color lightColor)
		{
			FiammettaVisuals.DrawShellTrail(Projectile, new Color(255, 22, 13),
				new Color(255, 230, 138), 40f);
			FiammettaVisuals.DrawFlyingCore(Projectile.Center, Projectile.rotation, 0.82f);
			return false;
		}
	}

	/// <summary>灼痕只记录位置与倒计时；它本身没有碰撞，倒计时结束后才生成伤害爆炸。</summary>
	public sealed class FiammettaScorchMark : ModProjectile
	{
		private int Delay => Math.Max(1, (int)Projectile.ai[0]);

		public override string Texture => "Terraria/Images/MagicPixel";

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 180;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.netImportant = true;
		}

		public override bool ShouldUpdatePosition() => false;
		public override bool? CanDamage() => false;

		public override void AI()
		{
			Projectile.localAI[0]++;
			Lighting.AddLight(Projectile.Center, new Vector3(0.72f, 0.06f, 0.02f) * 0.42f);

			if (!Main.dedServ && Main.rand.NextBool(4))
			{
				float angle = Main.rand.NextFloat(MathHelper.TwoPi);
				Vector2 position = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(12f, 29f);
				Dust dust = Dust.NewDustPerfect(position, DustID.Torch,
					(Projectile.Center - position).SafeNormalize(Vector2.Zero) * 1.25f,
					70, new Color(220, 31, 13), Main.rand.NextFloat(0.65f, 1.05f));
				dust.noGravity = true;
			}

			if (Projectile.localAI[0] < Delay)
				return;

			if (Projectile.owner == Main.myPlayer)
				FiammettaExplosion.Spawn(Projectile.GetSource_FromThis(), Projectile.Center,
					Projectile.damage, Projectile.knockBack, Projectile.owner, 4, Projectile.CritChance);
			Projectile.Kill();
		}

		public override bool PreDraw(ref Color lightColor)
		{
			return false;
		}
	}
}
