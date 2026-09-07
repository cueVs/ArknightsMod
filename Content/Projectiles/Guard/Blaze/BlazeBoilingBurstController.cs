using System;
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
	/// S3「沸腾爆裂」：10 秒维持；前 8 秒的切割间隔从 7 帧逐步缩短至 3 帧，第 9 秒达到最大攻防并终爆。
	/// </summary>
	public class BlazeBoilingBurstController : ModProjectile
	{
		private const int BaseSecondTicks = 60;
		private const int BasePulseDamageWindow = 4;
		private const int BaseFinalDamageWindow = 5;
		private const int InitialCutInterval = 7;
		private const int FinalCutInterval = 3;
		private const float CuttingReach = 104f;
		private const float CuttingWidth = 46f;
		// 原作终爆是以自身为中心的 3×3 近身范围；在 Terraria 尺度中保留成紧凑爆区，
		// 不让高达 7.2 倍基础伤害的终爆越过一整片屏幕清场。
		private const float ExplosionRadius = 112f;

		private int age;
		private int cutCooldown;
		private int pulseDamageTicks;
		private int finalDamageTicks;
		private int pulseIndex;
		private int pulseVisualTicks;
		private int pulseVisualDuration;
		private float currentCutMultiplier = 1f;
		private float chainPhase;
		private bool finalDetonated;
		private bool selfCostPaid;
		private bool pulseImpactTriggered;
		// 方向使用原版同步的 velocity，远端也能读取持有者最新的瞄准结果。
		private Vector2 AimDirection => Projectile.velocity.SafeNormalize(new Vector2(Owner.direction, 0f));
		private Vector2 handWorld;
		private Vector2 tipWorld;
		private Vector2 EffectMuzzle => Vector2.Lerp(handWorld, tipWorld, 0.78f);
		private float sawAngle;

		private Player Owner => Main.player[Projectile.owner];
		private bool InFinalDamageWindow => finalDamageTicks > 0;
		private float TimelineScale => Projectile.ai[2] > 0f
			? MathHelper.Clamp(Projectile.ai[2], 0.1f, 4f)
			: 1f;
		private int SecondTicks => Math.Max(1, (int)Math.Round(BaseSecondTicks * TimelineScale));
		private int TotalTicks => 10 * SecondTicks;
		private int CuttingTicks => 8 * SecondTicks;
		private int DetonationTick => 9 * SecondTicks;
		private int PulseDamageWindow => Math.Max(1, (int)Math.Round(BasePulseDamageWindow * TimelineScale));
		private int FinalDamageWindow => Math.Max(1, (int)Math.Round(BaseFinalDamageWindow * TimelineScale));
		private float Ramp => MathHelper.Clamp(age / (float)DetonationTick, 0f, 1f);
		private float CuttingRamp => MathHelper.Clamp(age / (float)CuttingTicks, 0f, 1f);
		private int CurrentCutInterval => Math.Clamp(
			(int)MathF.Round(MathHelper.Lerp(InitialCutInterval, FinalCutInterval, CuttingRamp)),
			FinalCutInterval,
			InitialCutInterval);
		private float Spool => MathHelper.SmoothStep(0f, 1f,
			MathHelper.Clamp(age / Math.Max(1f, 45f * TimelineScale), 0f, 1f));

		public override string Texture => "Terraria/Images/MagicPixel";

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.ignoreWater = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = -1;
			Projectile.netImportant = true;
			Projectile.timeLeft = 2;
		}

		public override bool ShouldUpdatePosition() => false;

		public override void OnSpawn(Terraria.DataStructures.IEntitySource source) {
			Projectile.velocity = AimDirection;
			cutCooldown = InitialCutInterval;
		}

		public override void AI() {
			Player player = Owner;
			var weaponPlayer = player.GetModPlayer<WeaponPlayer>();
			if (!player.active || player.dead) {
				Projectile.Kill();
				return;
			}

			// 不允许用切武器规避末尾生命代价；取消时不触发终爆，但已释放就要付出代价。
			bool owningClientCancelled = player.whoAmI == Main.myPlayer && weaponPlayer.Skill != 2;
			if (player.HeldItem.ModItem is not BlazeGreatsword || owningClientCancelled) {
				PaySelfCost();
				Projectile.Kill();
				return;
			}

			Projectile.timeLeft = 2;
			Projectile.Center = player.MountedCenter;
			UpdateAim(player);
			if (player.whoAmI == Main.myPlayer) {
				// S3 禁用公共自动计时，由这个 10 秒控制器作为唯一时间源驱动技能条。
				weaponPlayer.SkillActive = true;
				weaponPlayer.SkillTimer = Math.Min(age, TotalTicks);
			}
			// S3启动后链条视觉与实际切割频率同时提升至三倍。
			chainPhase += (0.34f + Ramp * 0.42f) * MathHelper.Lerp(0.22f, 1f, Spool) * 3f;
			if (!Main.dedServ && age < DetonationTick && age % 5 == 0) {
				SoundEngine.PlaySound(BlazeVisuals.ButcherMotorSound with {
					Volume = 0.42f + Ramp * 0.24f,
					Pitch = MathHelper.Lerp(-0.24f, 0.62f, MathF.Pow(Ramp, 0.72f)),
					MaxInstances = 4
				}, player.Center);
			}

			float vibration = MathF.Sin(age * (0.28f + Ramp * 0.4f)) * (0.015f + Ramp * 0.055f);
			sawAngle = AimDirection.ToRotation() + vibration;
			Vector2 sawDirection = sawAngle.ToRotationVector2();
			handWorld = player.MountedCenter + sawDirection * 5f;
			tipWorld = handWorld + sawDirection * CuttingReach;
			Projectile.rotation = sawAngle;
			Projectile.spriteDirection = sawDirection.X < 0f ? -1 : 1;
			if (age < DetonationTick)
				BaseHeldMeleeSupport.HoldAndPose(player, Projectile, handWorld, sawDirection.X < 0f);

			var blazePlayer = player.GetModPlayer<BlazeGreatswordPlayer>();
			// 第九秒终爆时攻防增益一并结束；控制器仅继续存活一秒，负责技能计时和收尾视觉。
			if (age < DetonationTick)
				blazePlayer.UpdateBoilingBurst(Ramp);
			else
				blazePlayer.EndBoilingBurst();
			Lighting.AddLight(player.Center, new Vector3(0.54f + Ramp * 0.45f, 0.07f + Ramp * 0.16f,
				0.025f + Ramp * 0.05f));

			if (age == 0)
				SpawnStartEffects();

			// 切割从 7 帧一次逐步加速至 3 帧一次。使用独立倒计时而非动态取模，
			// 否则间隔变化时会出现连续触发或漏掉切割的跳拍。
			if (age > 0 && age <= CuttingTicks) {
				cutCooldown--;
				if (cutCooldown <= 0) {
					int cutInterval = CurrentCutInterval;
					pulseIndex++;
					currentCutMultiplier = 1f + 0.8f * Ramp;
					pulseDamageTicks = PulseDamageWindow;
					// 视觉脉冲必须在下一次切割前完整展开，不能沿用旧的 18 帧寿命
					// 并在 3 帧高速阶段被反复重置成一个卡住的小光圈。
					pulseVisualDuration = cutInterval;
					pulseVisualTicks = pulseVisualDuration;
					pulseImpactTriggered = false;
					Projectile.ResetLocalNPCHitImmunity();
					SpawnCutPulseEffects(cutInterval);
					cutCooldown = cutInterval;
				}
			}

			if (age == DetonationTick) {
				finalDetonated = true;
				finalDamageTicks = FinalDamageWindow;
				pulseImpactTriggered = false;
				Projectile.ResetLocalNPCHitImmunity();
				PaySelfCost();
				SpawnFinalEffects();
			}

			if (pulseDamageTicks > 0)
				pulseDamageTicks--;
			if (finalDamageTicks > 0)
				finalDamageTicks--;
			if (pulseVisualTicks > 0)
				pulseVisualTicks--;

			if (!Main.dedServ && age < DetonationTick && age % Math.Max(5, 16 - (int)(Ramp * 10f)) == 0) {
				Vector2 motePosition = Vector2.Lerp(handWorld, tipWorld, Main.rand.NextFloat(0.2f, 1f));
				BlazeVisuals.SpawnHeatMotes(motePosition, Ramp > 0.7f ? 2 : 1, Ramp, 10f + Ramp * 14f);
			}

			age++;
			if (age >= TotalTicks)
				Projectile.Kill();
		}

		private void UpdateAim(Player player) {
			if (player.whoAmI != Main.myPlayer)
				return;

			Vector2 direction = (Main.MouseWorld - player.MountedCenter).SafeNormalize(AimDirection);
			float turn = MathF.Abs(MathHelper.WrapAngle(direction.ToRotation() - AimDirection.ToRotation()));
			// 大招不再锁住启动方向；锯身、命中判定与血液喷射共同使用当前瞄准。
			Projectile.velocity = direction;
			if (age % 6 == 0 || turn > 0.18f)
				Projectile.netUpdate = true;
		}

		public override bool? CanDamage() => pulseDamageTicks > 0 || finalDamageTicks > 0 ? null : false;

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) {
			if (pulseDamageTicks <= 0 && finalDamageTicks <= 0)
				return false;

			if (!Collision.CanHit(Owner.MountedCenter, 1, 1,
				targetHitbox.TopLeft(), targetHitbox.Width, targetHitbox.Height))
				return false;

			if (InFinalDamageWindow)
				return CircleIntersectsRectangle(Owner.MountedCenter, ExplosionRadius, targetHitbox);

			float collisionPoint = 0f;
			return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(),
				handWorld, tipWorld, CuttingWidth, ref collisionPoint);
		}

		private static bool CircleIntersectsRectangle(Vector2 center, float radius, Rectangle rectangle) {
			float closestX = MathHelper.Clamp(center.X, rectangle.Left, rectangle.Right);
			float closestY = MathHelper.Clamp(center.Y, rectangle.Top, rectangle.Bottom);
			return Vector2.DistanceSquared(center, new Vector2(closestX, closestY)) <= radius * radius;
		}

		public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) {
			// 专三终爆：400% 的“此时攻击力”，此时已达到 +80%，故总倍率为 4 × 1.8 = 7.2。
			modifiers.SourceDamage *= InFinalDamageWindow ? 7.2f : currentCutMultiplier;
			modifiers.HitDirectionOverride = target.Center.X >= Owner.Center.X ? 1 : -1;
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) {
			if (damageDone > 0)
				target.AddBuff(ModContent.BuffType<WeaponBleedingDebuff>(), 180);
			bool final = InFinalDamageWindow;
			BlazeVisuals.SpawnImpactBurst(target.Center, target.Center - Owner.Center, final, final ? 1f : Ramp);
			if (!pulseImpactTriggered && !Main.dedServ) {
				pulseImpactTriggered = true;
				SoundEngine.PlaySound(SoundID.Tink with {
					Volume = final ? 0.78f : 0.5f,
					Pitch = final ? -0.3f : MathHelper.Lerp(-0.05f, 0.3f, Ramp),
					MaxInstances = 5
				}, target.Center);
				BlazeVisuals.AddShake(Owner, final ? 12 : 4, final ? 8f : 2.6f);
			}
		}

		private void SpawnStartEffects() {
			if (Main.dedServ)
				return;
			SoundEngine.PlaySound(BlazeVisuals.ButcherMotorSound with { Volume = 0.72f, Pitch = -0.28f }, Owner.Center);
			BlazeVisuals.SpawnMetalSparks(EffectMuzzle, AimDirection, 18, 0.2f, 7.5f);
			BlazeVisuals.SpawnHeatMotes(Owner.Center, 8, 0.2f, 28f);
			BlazeVisuals.AddShake(Owner, 6, 3f);
		}

		private void SpawnCutPulseEffects(int cutInterval) {
			if (Main.dedServ)
				return;

			// 伤害脉冲可以高达 3 帧一次，但大型粒子/音效/震屏保持约 12 帧一个重音；
			// 其余切割只给少量齿轨火花。命中敌人时 OnHitNPC 仍会逐刀提供完整爆发反馈。
			int accentStride = Math.Max(1, (int)MathF.Round(12f / Math.Max(1, cutInterval)));
			bool accent = pulseIndex % accentStride == 0;
			int sparkCount = accent
				? 8 + (int)MathF.Round(Ramp * 9f)
				: 2 + (int)MathF.Round(Ramp * 3f);
			int moteCount = accent
				? 3 + (int)MathF.Round(Ramp * 5f)
				: 1 + (Ramp > 0.72f ? 1 : 0);
			BlazeVisuals.SpawnMetalSparks(EffectMuzzle, AimDirection,
				sparkCount, Ramp, 7f + Ramp * 4f);
			BlazeVisuals.SpawnHeatMotes(Vector2.Lerp(handWorld, EffectMuzzle, 0.72f),
				moteCount, Ramp, accent ? 20f : 11f);

			if (accent) {
				float pitch = MathHelper.Lerp(-0.12f, 0.72f, MathF.Pow(Ramp, 0.72f));
				SoundEngine.PlaySound(BlazeVisuals.ButcherMotorSound with {
					Volume = 0.42f + Ramp * 0.18f,
					Pitch = pitch,
					MaxInstances = 4
				}, EffectMuzzle);
				BlazeVisuals.AddShake(Owner, 2 + (int)MathF.Round(Ramp * 2f), 1.4f + Ramp * 1.8f);
			}
		}

		private void SpawnFinalEffects() {
			if (Main.dedServ)
				return;

			BlazeScarletEjecta.SpawnFan(Projectile.GetSource_FromThis(), EffectMuzzle, AimDirection, 30,
				MathHelper.ToRadians(30f), 9f, 17f, Math.Max(1, (int)MathF.Round(Projectile.damage * 0.3f)),
				Projectile.knockBack * 0.22f, Projectile.owner, 1.35f);
			SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Volume = 0.9f, Pitch = -0.35f }, Owner.Center);
			SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.82f, Pitch = -0.18f }, Owner.Center);
			SoundEngine.PlaySound(SoundID.Item74 with { Volume = 0.78f, Pitch = -0.12f }, Owner.Center);
			BlazeVisuals.SpawnHeatMotes(Owner.Center, 42, 1f, ExplosionRadius * 0.72f);
			for (int i = 0; i < 54; i++) {
				Vector2 direction = (MathHelper.TwoPi * i / 54f).ToRotationVector2();
				BlazeVisuals.SpawnMetalSparks(Owner.Center + direction * Main.rand.NextFloat(18f, 62f),
					direction, 1, 1f, Main.rand.NextFloat(7f, 13f));
			}
			BlazeVisuals.AddShake(Owner, 20, 12f);
		}

		private void PaySelfCost() {
			if (selfCostPaid || Owner.dead || Owner.whoAmI != Main.myPlayer)
				return;

			selfCostPaid = true;
			int lifeLoss = Math.Max(1, (int)MathF.Ceiling(Owner.statLifeMax2 * 0.25f));
			int lifeBefore = Owner.statLife;
			Owner.statLife = Math.Max(1, lifeBefore - lifeLoss);
			int actualLoss = lifeBefore - Owner.statLife;
			if (actualLoss > 0)
				CombatText.NewText(Owner.getRect(), new Color(255, 85, 55), $"-{actualLoss}", true);
			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendData(MessageID.PlayerLifeMana, -1, -1, null, Owner.whoAmI);
		}

		public override void OnKill(int timeLeft) {
			if (!finalDetonated && age > 0)
				PaySelfCost();

			Owner.GetModPlayer<BlazeGreatswordPlayer>().EndBoilingBurst();
			if (Owner.whoAmI == Main.myPlayer) {
				var weaponPlayer = Owner.GetModPlayer<WeaponPlayer>();
				weaponPlayer.SkillActive = false;
				weaponPlayer.SkillTimer = 0;
			}
		}

		public override bool PreDraw(ref Color lightColor) {
			if (Main.dedServ)
				return false;

			Texture2D weapon = TextureAssets.Item[ModContent.ItemType<BlazeGreatsword>()].Value;
			Texture2D glow = TextureAssets.Extra[ExtrasID.ThePerfectGlow].Value;
			bool faceLeft = MathF.Cos(sawAngle) < 0f;
			int frame = BlazeChainsawSprite.FrameAt(age, 2);
			if (age < DetonationTick)
				BlazeChainsawSprite.DrawHeld(weapon, handWorld, sawAngle, faceLeft, CuttingReach, lightColor, frame);

			BaseHeldMeleeSupport.BeginAdditive(Main.spriteBatch);
			if (age < DetonationTick) {
				BlazeChainsawSprite.DrawHeld(weapon, handWorld, sawAngle, faceLeft, CuttingReach,
					new Color(235, 26, 38, 110) * MathHelper.Lerp(0.14f, 0.34f, Ramp), frame);
				BlazeVisuals.DrawBladeHeat(handWorld, tipWorld, Ramp, chainPhase,
					MathHelper.Lerp(0.55f, 0.92f, Spool), extended: true);
				BlazeVisuals.DrawScarletTears(Vector2.Lerp(handWorld, tipWorld, 0.7f), sawAngle,
					MathHelper.Lerp(0.85f, 1.55f, Ramp), MathHelper.Lerp(0.32f, 0.76f, Ramp));

				float breathing = 0.5f + MathF.Sin(age * 0.12f) * 0.5f;
				float auraRadius = 35f + Ramp * 32f + breathing * (4f + Ramp * 7f);
				BlazeVisuals.DrawRing(Owner.Center, auraRadius, 2f + Ramp * 2.5f,
					new Color(220, 12, 38), 0.16f + Ramp * 0.34f, 44, age * 0.008f);
				BlazeVisuals.DrawRing(Owner.Center, auraRadius * 0.72f, 1.5f + Ramp * 2f,
					new Color(255, 125, 30), 0.12f + Ramp * 0.3f, 40, -age * 0.011f);

				if (pulseVisualTicks > 0) {
					float pulseProgress = 1f - pulseVisualTicks
						/ (float)Math.Max(1, pulseVisualDuration);
					float fade = 1f - pulseProgress;
					BlazeVisuals.DrawRing(EffectMuzzle, MathHelper.Lerp(12f, 58f, pulseProgress),
						MathHelper.Lerp(8f, 1.5f, pulseProgress), new Color(255, 38, 49), fade * 0.78f, 34);
					BlazeVisuals.DrawRadialRays(EffectMuzzle, 6f, MathHelper.Lerp(28f, 82f, pulseProgress),
						8, MathHelper.Lerp(4f, 1f, pulseProgress), new Color(255, 175, 55), fade * 0.55f,
						age * 0.03f);
					BlazeVisuals.DrawBurstTextures(EffectMuzzle, pulseProgress, age * 0.035f, false);
				}
			}

			if (age >= DetonationTick) {
				float finalProgress = MathHelper.Clamp((age - DetonationTick) / (float)SecondTicks, 0f, 1f);
				float fade = 1f - finalProgress;
				float radius = MathHelper.Lerp(18f, ExplosionRadius * 1.15f, MathF.Sqrt(finalProgress));
				BlazeVisuals.DrawRing(Owner.Center, radius, MathHelper.Lerp(18f, 2f, finalProgress),
					new Color(232, 8, 36), fade * 0.94f, 58, age * 0.018f);
				BlazeVisuals.DrawRing(Owner.Center, radius * 0.72f, MathHelper.Lerp(12f, 2f, finalProgress),
					new Color(255, 132, 30), fade * 0.82f, 52, -age * 0.023f);
				BlazeVisuals.DrawRadialRays(Owner.Center, 14f, radius * 1.12f, 16,
					MathHelper.Lerp(7f, 1f, finalProgress), new Color(255, 230, 175), fade * 0.75f,
					age * 0.01f);
				BlazeVisuals.DrawBurstTextures(Owner.Center, finalProgress, age * 0.018f, true);

				Main.spriteBatch.Draw(glow, Owner.Center - Main.screenPosition, null,
					new Color(238, 16, 42) * (fade * 0.92f), 0f, glow.Size() * 0.5f,
					MathHelper.Lerp(1.15f, 2.7f, finalProgress), SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(glow, Owner.Center - Main.screenPosition, null,
					new Color(255, 165, 45) * (fade * 0.82f), 0f, glow.Size() * 0.5f,
					MathHelper.Lerp(0.65f, 1.55f, finalProgress), SpriteEffects.None, 0f);
			}

			BaseHeldMeleeSupport.EndAdditive(Main.spriteBatch);
			return false;
		}
	}
}
