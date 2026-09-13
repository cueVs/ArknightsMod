using System;
using System.Collections.Generic;
using System.IO;
using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Items.Weapons.Guard.Blaze;
using ArknightsMod.Content.Projectiles.BasePROJ;
using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Guard.Blaze
{
	/// <summary>
	/// 煌的链锯持有体。它是一台持续工作的近战机器，不再借用大剑挥砍模板：
	/// 玩家按住攻击维持转动，锯身自由跟随鼠标，并保留马达震动、滚动锯齿和短周期群攻。
	/// </summary>
	public sealed class BlazeChainsawHoldout : ModProjectile
	{
		private const float NormalReach = 86f;
		private const float ExtendedReach = 104f;
		private const float SawWidth = 38f;
		private const int BiteCycleTicks = 7;

		private readonly HashSet<int> cycleTargets = [];
		private int age;
		private float aimAngle;
		private float spool;
		private float chainPhase;
		private bool initialized;
		private bool creditedThisCycle;
		private bool powerStrikeCycle;
		private bool ejectaSpawnedThisCycle;
		private Vector2 handWorld;
		private Vector2 drawHandWorld;
		private Vector2 tipWorld;
		private Vector2 EffectMuzzle => Vector2.Lerp(handWorld, tipWorld, 0.82f);

		private Player Owner => Main.player[Projectile.owner];
		private int SelectedSkill => (int)Projectile.ai[0];
		private bool Extended => Projectile.ai[1] > 0.5f;
		private float Reach => Extended ? ExtendedReach : NormalReach;

		public override string Texture => BlazeChainsawSprite.TexturePath;

		public override void SetDefaults() {
			Projectile.width = 18;
			Projectile.height = 18;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.aiStyle = -1;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.netImportant = true;
			Projectile.timeLeft = 2;
		}

		public override bool ShouldUpdatePosition() => false;

		public override void OnSpawn(Terraria.DataStructures.IEntitySource source) {
			aimAngle = Projectile.velocity.SafeNormalize(Vector2.UnitX).ToRotation();
			initialized = true;
		}

		public override void AI() {
			Player player = Owner;
			if (!player.active || player.dead || player.noItems || player.CCed
				|| player.HeldItem.ModItem is not BlazeGreatsword) {
				Projectile.Kill();
				return;
			}

			if (player.whoAmI == Main.myPlayer && !player.channel) {
				Projectile.Kill();
				return;
			}

			Projectile.timeLeft = 2;
			if (!initialized) {
				aimAngle = Projectile.velocity.SafeNormalize(new Vector2(player.direction, 0f)).ToRotation();
				initialized = true;
			}

			UpdateAim(player);
			UpdateExtendedState(player);
			spool = MathHelper.Lerp(spool, 1f, Extended ? 0.16f : 0.12f);
			chainPhase += MathHelper.Lerp(0.16f, Extended ? 0.86f : 0.72f, spool);

			Vector2 direction = aimAngle.ToRotationVector2();
			Vector2 normal = new(-direction.Y, direction.X);
			float vibration = MathF.Sin(age * (1.52f + spool * 0.34f)) * (0.45f + spool * 1.15f);
			// 手部锚点保持稳定。马达只让锯身沿自身轴线前后振动，不能把玩家手臂左右来回拖动。
			handWorld = player.MountedCenter + direction * 4f;
			drawHandWorld = handWorld + direction * vibration;
			tipWorld = drawHandWorld + direction * Reach;

			Projectile.Center = (handWorld + tipWorld) * 0.5f;
			Projectile.velocity = direction;
			Projectile.rotation = aimAngle;
			Projectile.spriteDirection = direction.X < 0f ? -1 : 1;
			player.ChangeDir(direction.X < 0f ? -1 : 1);
			player.itemTime = 2;
			player.itemAnimation = 2;
			BaseHeldMeleeSupport.HoldAndPose(player, Projectile, handWorld, direction.X < 0f);

			UpdateBiteCycle();
			UpdateMotorEffects(direction, normal);
			Lighting.AddLight(Vector2.Lerp(handWorld, tipWorld, 0.68f),
				new Vector3(0.62f + spool * 0.32f, 0.07f + spool * 0.06f, 0.035f));
			age++;
		}

		private void UpdateExtendedState(Player player) {
			if (player.whoAmI != Main.myPlayer)
				return;

			bool shouldExtend = player.GetModPlayer<WeaponPlayer>().Skill == 1
				&& player.GetModPlayer<BlazeGreatswordPlayer>().ExtensionDeployed;
			float state = shouldExtend ? 1f : 0f;
			if (Projectile.ai[1] == state)
				return;

			Projectile.ai[1] = state;
			Projectile.netUpdate = true;
		}

		private void UpdateAim(Player player) {
			if (player.whoAmI != Main.myPlayer)
				return;

			Vector2 toMouse = Main.MouseWorld - player.MountedCenter;
			if (toMouse.LengthSquared() < 16f)
				toMouse = new Vector2(player.direction, 0f);
			float targetAngle = toMouse.ToRotation();
			float difference = MathHelper.WrapAngle(targetAngle - aimAngle);
			float distance = MathF.Abs(difference);

			// 普攻与延长模式都直接跟随鼠标，不再限制转向速度或添加追赶惯性。
			aimAngle = targetAngle;

			if (age % 6 == 0 || distance > 0.18f)
				Projectile.netUpdate = true;
		}

		private void UpdateBiteCycle() {
			if (age % BiteCycleTicks != 0)
				return;

			cycleTargets.Clear();
			creditedThisCycle = false;
			powerStrikeCycle = false;
			ejectaSpawnedThisCycle = false;
			Projectile.ResetLocalNPCHitImmunity();

			if (Extended && spool >= 0.5f && age > 0) {
				ejectaSpawnedThisCycle = true;
				BlazeScarletEjecta.SpawnCone(Projectile.GetSource_FromThis(), EffectMuzzle,
					aimAngle.ToRotationVector2(), 1, MathHelper.ToRadians(16f), 7.5f, 11f,
					Math.Max(1, (int)MathF.Round(Projectile.damage * 0.18f)),
					Projectile.knockBack * 0.18f, Projectile.owner, 0.9f);
			}
		}

		private void UpdateMotorEffects(Vector2 direction, Vector2 normal) {
			if (Main.dedServ)
				return;

			if (age % 6 == 0) {
				SoundEngine.PlaySound(BlazeVisuals.ButcherMotorSound with {
					Volume = 0.32f + spool * 0.2f,
					Pitch = MathHelper.Lerp(-0.22f, Extended ? 0.28f : 0.12f, spool),
					MaxInstances = 4
				}, handWorld);
			}

			if (age % 2 == 0 && spool > 0.35f) {
				Vector2 position = Vector2.Lerp(handWorld, tipWorld, Main.rand.NextFloat(0.35f, 1f));
				Vector2 tangent = normal * (Main.rand.NextBool() ? 1f : -1f);
				BlazeVisuals.SpawnMetalSparks(position, tangent + direction * 0.25f, Extended ? 2 : 1,
					spool, 4.5f + spool * 2f);
			}

			if (age % 5 == 0)
				BlazeVisuals.SpawnHeatMotes(Vector2.Lerp(handWorld, tipWorld, Main.rand.NextFloat(0.45f, 0.95f)),
					Extended ? 2 : 1, spool, 9f);
		}

		public override bool? CanDamage() => spool >= 0.32f ? null : false;

		public override bool? CanHitNPC(NPC target) {
			if (cycleTargets.Contains(target.whoAmI))
				return false;
			return null;
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (spool < 0.32f)
				return false;
			if (!Collision.CanHit(Owner.MountedCenter, 1, 1,
				targetHitbox.TopLeft(), targetHitbox.Width, targetHitbox.Height))
				return false;

			Vector2 direction = aimAngle.ToRotationVector2();
			float collisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
				handWorld + direction * 18f, tipWorld, SawWidth, ref collisionPoint);
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			if (SelectedSkill == 0 && !powerStrikeCycle && !creditedThisCycle) {
				var weaponPlayer = Owner.GetModPlayer<WeaponPlayer>();
				powerStrikeCycle = Owner.GetModPlayer<BlazeGreatswordPlayer>().TryConsumePowerStrike(weaponPlayer);
			}
			if (powerStrikeCycle)
				modifiers.SourceDamage *= BlazeGreatsword.PowerStrikeDamageMultiplier;
			modifiers.HitDirectionOverride = target.Center.X >= Owner.Center.X ? 1 : -1;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (damageDone > 0)
				target.AddBuff(ModContent.BuffType<WeaponBleedingDebuff>(), 180);
			cycleTargets.Add(target.whoAmI);
			if (SelectedSkill == 0 && !powerStrikeCycle && !creditedThisCycle) {
				creditedThisCycle = true;
				Owner.GetModPlayer<BlazeGreatswordPlayer>().RegisterPowerStrikeAttack();
			}

			Vector2 direction = aimAngle.ToRotationVector2();
			if (!Extended && !ejectaSpawnedThisCycle) {
				ejectaSpawnedThisCycle = true;
				BlazeScarletEjecta.SpawnCone(Projectile.GetSource_OnHit(target), EffectMuzzle,
					direction, 1, MathHelper.ToRadians(21f), 8f, 12.5f,
					Math.Max(1, (int)MathF.Round(Projectile.damage * 0.24f)),
					Projectile.knockBack * 0.22f, Projectile.owner, powerStrikeCycle ? 1.25f : 1f);
			}
			BlazeVisuals.SpawnImpactBurst(target.Center, direction, powerStrikeCycle, spool);
			if (!Main.dedServ) {
				SoundEngine.PlaySound(SoundID.NPCHit18 with {
					Volume = powerStrikeCycle ? 0.78f : 0.48f,
					Pitch = powerStrikeCycle ? -0.18f : 0.08f,
					MaxInstances = 6
				}, target.Center);
				BlazeVisuals.AddImpactShake(Owner, powerStrikeCycle ? 7 : 3, powerStrikeCycle ? 5.5f : 2.2f);
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			Texture2D weapon = TextureAssets.Item[ModContent.ItemType<BlazeGreatsword>()].Value;
			bool faceLeft = MathF.Cos(aimAngle) < 0f;
			int frame = BlazeChainsawSprite.FrameAt(age, Extended ? 3 : 4);
			if (Extended)
				DrawExtensionOutline(weapon, faceLeft, frame);
			BlazeChainsawSprite.DrawHeld(weapon, drawHandWorld, aimAngle, faceLeft, Reach, lightColor, frame);

			BaseHeldMeleeSupport.BeginAdditive(Main.spriteBatch);
			BlazeChainsawSprite.DrawHeld(weapon, drawHandWorld, aimAngle, faceLeft, Reach,
				new Color(214, 24, 34, 95) * (0.18f + spool * 0.12f), frame);
			BlazeVisuals.DrawBladeHeat(handWorld, tipWorld, spool, chainPhase,
				opacity: 0.55f + spool * 0.25f, extended: Extended);
			BaseHeldMeleeSupport.EndAdditive(Main.spriteBatch);
			return false;
		}

		private void DrawExtensionOutline(Texture2D weapon, bool faceLeft, int frame) {
			BaseHeldMeleeSupport.BeginAdditive(Main.spriteBatch);
			float pulse = 0.9f + MathF.Sin((float)Main.GlobalTimeWrappedHourly * 7f) * 0.1f;
			Color outline = new Color(244, 12, 35, 105) * (0.72f * pulse);
			for (int i = 0; i < 8; i++) {
				Vector2 offset = (MathHelper.TwoPi * i / 8f).ToRotationVector2() * 2.4f;
				BlazeChainsawSprite.DrawHeld(weapon, drawHandWorld + offset, aimAngle, faceLeft,
					Reach, outline, frame);
			}
			BaseHeldMeleeSupport.EndAdditive(Main.spriteBatch);
		}

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write(aimAngle);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			aimAngle = reader.ReadSingle();
			initialized = true;
		}
	}
}
