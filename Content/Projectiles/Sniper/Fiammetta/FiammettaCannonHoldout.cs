using System;
using ArknightsMod.Common.Particle;
using ArknightsMod.Content.Items.Weapons.Sniper.Fiammetta;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.Fiammetta
{
	/// <summary>
	/// 手炮举枪、后坐和开火动作。普通炮击/S3 把炮口抬向天空；S2 按原作改为正面直射。
	/// 物品只负责输入，真正的炮弹必须从这个炮口位置生成，避免玩家手上不动、炮弹凭空出现。
	/// </summary>
	public sealed class FiammettaCannonHoldout : ModProjectile
	{
		private const int FireFrame = 16;
		// 54 / 1.2 = 45，必须早于下一次 50 帧炮击结束，否则 S3 会隔次漏发。
		private const int OneShotLifetime = 45;
		private const int FireInterval = FiammettaHandCannon.AttackIntervalTicks;
		private const int RecoilLifetime = 18;
		private const float MuzzleReach = 52f;
		private bool fired;

		private Player Owner => Main.player[Projectile.owner];
		private Vector2 Target => new(Projectile.ai[0], Projectile.ai[1]);
		private int Mode => Math.Clamp((int)Projectile.ai[2], 0, 3);
		private int CycleAge => (int)Projectile.localAI[0];
		private int RecoilAge => (int)Projectile.localAI[1];
		private bool IsContinuousFire => Mode is 0 or 1;

		public override string Texture =>
			"ArknightsMod/Content/Items/Weapons/Sniper/Fiammetta/FiammettaHandCannon";

		public override void SetDefaults()
		{
			Projectile.width = 54;
			Projectile.height = 28;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = OneShotLifetime;
			Projectile.netImportant = true;
		}

		public override bool ShouldUpdatePosition() => false;
		public override bool? CanDamage() => false;

		public override void AI()
		{
			if (!Owner.active || Owner.dead || Owner.HeldItem.ModItem is not FiammettaHandCannon)
			{
				Projectile.Kill();
				return;
			}
			// 普攻和 S1 按住时保持同一把炮；S2/S3 仍然采用一次性举炮。
			if (IsContinuousFire)
			{
				if (!Owner.channel)
				{
					Projectile.Kill();
					return;
				}
				Projectile.timeLeft = 2;
				Owner.itemTime = Owner.itemAnimation = 2;
			}

			UpdateLiveAimTarget();
			Vector2 targetDirection = (Target - Owner.MountedCenter)
				.SafeNormalize(new Vector2(Owner.direction, 0f));
			Vector2 aimDirection;
			if (Mode == 2)
			{
				// “你须愧悔”是沿正面射出的灼痕弹，不借普通迫击炮曲线偷换动作。
				aimDirection = targetDirection;
			}
			else
			{
				// SHPC 迫击炮的举枪质感：炮口始终指向高空，鼠标只改变天空锚点的横向位置。
				Vector2 skyAnchor = new(MathHelper.Lerp(Target.X, Owner.Center.X, 0.55f),
					Owner.Center.Y - 500f * Owner.gravDir);
				aimDirection = (skyAnchor - Owner.Center)
					.SafeNormalize(-Vector2.UnitY * Owner.gravDir);
			}

			float recoil = RecoilAge <= 0
				? 0f
				: MathF.Sin(MathHelper.Pi * MathHelper.Clamp(RecoilAge / (float)RecoilLifetime, 0f, 1f)) * 9f;
			Projectile.velocity = aimDirection;
			Projectile.rotation = aimDirection.ToRotation();
			Projectile.Center = Owner.MountedCenter + aimDirection * (18f - recoil);
			Projectile.spriteDirection = aimDirection.X < 0f ? -1 : 1;

			Owner.ChangeDir(Target.X < Owner.Center.X ? -1 : 1);
			Owner.heldProj = Projectile.whoAmI;
			// 一次性举炮不覆盖物品冷却；heldProj 与复合手臂已经足够维持持枪姿势。
			float armRotation = Projectile.rotation - MathHelper.PiOver2;
			Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, armRotation);
			Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Quarter,
				armRotation - 0.12f * Owner.direction);

			if (!fired && CycleAge >= FireFrame)
			{
				Fire(aimDirection, targetDirection);
				fired = true;
				Projectile.localAI[1] = 1f;
				if (IsContinuousFire)
				{
					Projectile.localAI[0] = FireFrame - FireInterval;
					fired = false;
				}
			}

			EmitChargeMotes(aimDirection);
			Projectile.localAI[0]++;
			if (RecoilAge > 0 && RecoilAge < RecoilLifetime)
				Projectile.localAI[1]++;
		}

		private void UpdateLiveAimTarget()
		{
			// S3 必须守住开启时选定的固定落点；S2 仅在发射前锁定，普攻/S1 则全程跟随鼠标。
			if (Projectile.owner != Main.myPlayer || Mode == 3 || (Mode == 2 && CycleAge > FireFrame))
				return;

			float maximumRange = Mode switch
			{
				1 => FiammettaHandCannon.ProvocateMaximumRange,
				2 => FiammettaHandCannon.PaeniteteMaximumRange,
				_ => FiammettaHandCannon.NormalMaximumRange
			};
			Vector2 target = FiammettaHandCannon.ClampTarget(Owner, Main.MouseWorld, maximumRange);
			if (Vector2.DistanceSquared(target, Target) < 1f)
				return;

			Projectile.ai[0] = target.X;
			Projectile.ai[1] = target.Y;
			if (CycleAge % 4 == 0 || CycleAge == FireFrame)
				Projectile.netUpdate = true;
		}

		private Vector2 MuzzlePosition(Vector2 direction) => Projectile.Center + direction * MuzzleReach;

		private void Fire(Vector2 holdDirection, Vector2 targetDirection)
		{
			Vector2 muzzle = MuzzlePosition(holdDirection);
			if (Projectile.owner == Main.myPlayer)
			{
				int projectileType;
				int damage = Projectile.damage;
				Vector2 velocity;
				if (Mode == 2)
				{
					projectileType = ModContent.ProjectileType<FiammettaScorchingShell>();
					damage = Math.Max(1, (int)(damage * 3.5f));
					velocity = targetDirection * 18f;
				}
				else
				{
					projectileType = ModContent.ProjectileType<FiammettaMortarShell>();
					if (Mode == 3)
						damage = Math.Max(1, (int)(damage * 1.05f));
					// Blissful Bombardier 的迫击炮离膛速度是其 24 速的 90%，即 21.6。
					velocity = holdDirection.RotatedByRandom(MathHelper.ToRadians(3f)) * 21.6f;
				}

				int index = Projectile.NewProjectile(Projectile.GetSource_FromThis(), muzzle, velocity,
					projectileType, damage, Projectile.knockBack, Projectile.owner,
					Target.X, Target.Y, Mode);
				if (Main.projectile.IndexInRange(index))
					Main.projectile[index].CritChance = Projectile.CritChance;
				if (Mode is 0 or 1)
					Owner.GetModPlayer<WeaponPlayer>().OffensiveRecovery();
			}

			SoundEngine.PlaySound(SoundID.Item61 with
			{
				Volume = Mode == 2 ? 0.82f : 0.72f,
				Pitch = Mode == 2 ? 0.08f : -0.28f,
				PitchVariance = 0.08f,
				MaxInstances = 4
			}, muzzle);
			SpawnMuzzleBurst(muzzle, Mode == 2 ? targetDirection : holdDirection);
		}

		private void EmitChargeMotes(Vector2 direction)
		{
			if (Main.dedServ || CycleAge < 0 || CycleAge >= FireFrame || !Main.rand.NextBool(2))
				return;

			Vector2 muzzle = MuzzlePosition(direction);
			Color color = Color.Lerp(new Color(230, 38, 20), new Color(255, 211, 105), CycleAge / (float)FireFrame);
			var particle = new DefaultParticle(muzzle + Main.rand.NextVector2Circular(13f, 13f),
				(muzzle - (muzzle + Main.rand.NextVector2Circular(13f, 13f))).SafeNormalize(Vector2.Zero) * 1.6f,
				Main.rand.Next(10, 17), Main.rand.NextFloat(0.22f, 0.42f), color, true)
			{
				Deformation = new Vector2(0.3f, 1.8f)
			};
			particle.Spawn();
		}

		private static void SpawnMuzzleBurst(Vector2 muzzle, Vector2 direction)
		{
			if (Main.dedServ)
				return;

			for (int i = 0; i < 12; i++)
			{
				Vector2 velocity = direction.RotatedByRandom(0.46f) * Main.rand.NextFloat(3f, 9f);
				Color color = Color.Lerp(new Color(235, 34, 18), new Color(255, 226, 130), Main.rand.NextFloat());
				var particle = new DefaultParticle(muzzle, velocity, Main.rand.Next(12, 22),
					Main.rand.NextFloat(0.30f, 0.62f), color, true)
				{
					Deformation = new Vector2(0.28f, Main.rand.NextFloat(1.4f, 2.4f))
				};
				particle.Spawn();
			}

			for (int i = 0; i < 10; i++)
			{
				Dust dust = Dust.NewDustPerfect(muzzle, i % 3 == 0 ? DustID.GoldFlame : DustID.Torch,
					direction.RotatedByRandom(0.62f) * Main.rand.NextFloat(2.5f, 8.5f),
					30, new Color(255, 110, 40), Main.rand.NextFloat(0.9f, 1.35f));
				dust.noGravity = true;
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
			Vector2 drawPosition = Projectile.Center - Main.screenPosition;
			Vector2 origin = new(texture.Width * 0.24f, texture.Height * 0.5f);
			SpriteEffects effects = Projectile.spriteDirection < 0 ? SpriteEffects.FlipVertically : SpriteEffects.None;
			float flash = RecoilAge <= 0 ? 0f : 1f - MathHelper.Clamp(RecoilAge / (float)RecoilLifetime, 0f, 1f);

			FiammettaVisuals.BeginAdditive();
			Color outline = Color.Lerp(new Color(215, 25, 16), new Color(255, 221, 124), flash);
			for (int i = 0; i < 8; i++)
			{
				Vector2 offset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * (2.2f + flash * 3.5f);
				Main.spriteBatch.Draw(texture, drawPosition + offset, null, outline * (0.34f + flash * 0.25f),
					Projectile.rotation, origin, Projectile.scale, effects, 0f);
			}

			Vector2 muzzle = MuzzlePosition(Projectile.velocity) - Main.screenPosition;
			Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
			Main.spriteBatch.Draw(glow, muzzle, null, new Color(255, 78, 28) * (0.35f + flash * 0.65f),
				0f, glow.Size() * 0.5f, new Vector2(0.17f + flash * 0.25f, 0.08f + flash * 0.12f),
				SpriteEffects.None, 0f);
			FiammettaVisuals.EndAdditive();

			Main.spriteBatch.Draw(texture, drawPosition, null, Projectile.GetAlpha(lightColor),
				Projectile.rotation, origin, Projectile.scale, effects, 0f);
			return false;
		}
	}

	/// <summary>
	/// S3“你须偿还”的永久控制器：记录一次固定落点、持续显示法阵，并按节拍自动举炮发射。
	/// 再次按技能键只关闭它，不重新扣技力。
	/// </summary>
	public sealed class FiammettaReponiteController : ModProjectile
	{
		private const int BarrageInterval = FiammettaHandCannon.AttackIntervalTicks;

		private Player Owner => Main.player[Projectile.owner];
		private Vector2 Target => new(Projectile.ai[0], Projectile.ai[1]);

		public override string Texture => "Terraria/Images/MagicPixel";

		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.friendly = false;
			Projectile.hostile = false;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 2;
			Projectile.netImportant = true;
		}

		public override bool ShouldUpdatePosition() => false;
		public override bool? CanDamage() => false;

		public override void AI()
		{
			WeaponPlayer weaponPlayer = Owner.GetModPlayer<WeaponPlayer>();
			if (!Owner.active || Owner.dead || Owner.HeldItem.ModItem is not FiammettaHandCannon ||
				weaponPlayer.Skill != 2 || !weaponPlayer.SkillActive)
			{
				Projectile.Kill();
				return;
			}

			Projectile.Center = Target;
			Projectile.timeLeft = 2;
			Lighting.AddLight(Target, new Vector3(0.72f, 0.13f, 0.035f) * 0.55f);

			int timer = (int)Projectile.localAI[0]++;
			if (timer % BarrageInterval == 0 && Projectile.owner == Main.myPlayer &&
				Owner.ownedProjectileCounts[ModContent.ProjectileType<FiammettaCannonHoldout>()] < 1)
			{
				FiammettaHandCannon.SpawnHoldout(Projectile.GetSource_FromThis(), Owner, Target,
					Owner.GetWeaponDamage(Owner.HeldItem), Owner.GetWeaponKnockback(Owner.HeldItem), 3);
			}

			if (!Main.dedServ && timer % 9 == 0)
			{
				float angle = timer * 0.09f + Main.rand.NextFloat(-0.18f, 0.18f);
				Vector2 position = Target + angle.ToRotationVector2() * Main.rand.NextFloat(46f, 82f);
				var mote = new DefaultParticle(position, (Target - position).SafeNormalize(Vector2.Zero) * 1.7f,
					18, Main.rand.NextFloat(0.24f, 0.44f), new Color(255, 116, 48), true)
				{
					Deformation = new Vector2(0.28f, 1.7f)
				};
				mote.Spawn();
			}
		}

		public override bool PreDraw(ref Color lightColor)
		{
			float time = Main.GlobalTimeWrappedHourly;
			float pulse = 0.78f + MathF.Sin(time * 4.2f) * 0.12f;
			FiammettaVisuals.DrawImpactSigil(Target, 184f * pulse, 0.62f,
				time * 0.42f, new Color(235, 40, 20));
			FiammettaVisuals.DrawImpactSigil(Target, 126f, 0.38f,
				-time * 0.68f, new Color(255, 205, 102));
			return false;
		}
	}
}
