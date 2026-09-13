using System;
using ArknightsMod.Common.GlobalNPCs;
using ArknightsMod.Content.Buffs.Supporter.Pramanix;
using ArknightsMod.Content.Projectiles.Supporter.Pramanix;
using ArknightsMod.Players;
using ArknightsMod.Systems.Gameplay.Damage;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Pramanix
{
	public class SaintBellPlayer : ModPlayer
	{
		// 二技能/三技能左键齐射共用冷却（刻）
		public const int SkillVolleyCooldown = 60;
		public const float Skill2ShockwaveMaxRadius = 280f;

		public const float Skill1DamageMult      = 5.2f;
		public const int   Skill1ColdTicks       = 210; // 3.5秒
		public const float Skill1KnockbackForce    = 52f;
		public const float Skill1KnockbackVertical = 10f;
		public const float Skill1BossKnockbackMin  = 5f;
		public const float Skill1SnowSpreadTiles = 5f;  // 积雪扩散距离（格）

		public const int Skill3LureDuration = 600;
		public const float Skill3LureOrbitRadius = 148f;
		public const float Skill3LureMinDistance = 132f;
		public const float Skill3LureMoveSpeed = 4.8f;
		public const float Skill3ArtsResistIgnore = 0.10f;
		public const float Skill3DamageBonus = 1f;
		public const float Skill3UseSpeedBonus = 0.13f;
		public const float Skill3VolleyDamageMult = 2.6f;

		public static readonly float[] Skill2RingScales = [0.42f, 0.68f, 1f];
		public static readonly float[] Skill2DamageRingScales = [0.68f, 1f];

		public int FrostTier;
		public int FrostHitBudget;
		public int FrostDomainTimer;
		public int PassiveGrowthTimer;

		public bool Skill2Active;
		public bool Skill3Active;
		public int SkillVolleyTimer;
		private float emblemAfterglow;

		public override void HideDrawLayers(PlayerDrawSet drawInfo) {
			if (drawInfo.drawPlayer.HeldItem.ModItem is SaintBell)
				PlayerDrawLayers.HeldItem.Hide();
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (item.ModItem is not SaintBell || !Skill3Active)
				return;
			damage *= 1f + Skill3DamageBonus;
		}

		public override float UseSpeedMultiplier(Item item) {
			if (item.ModItem is not SaintBell || !Skill3Active)
				return 1f;
			return 1f + Skill3UseSpeedBonus;
		}

		public override void PostUpdate() {
			if (Player.HeldItem.ModItem is not SaintBell) {
				PassiveGrowthTimer = 0;
				if (Skill2Active)
					DeactivateSkill2();
				if (Skill3Active)
					DeactivateSkill3();
				return;
			}

			var wp = Player.GetModPlayer<WeaponPlayer>();
			if (wp.Skill != 1 && Skill2Active)
				DeactivateSkill2();
			if (wp.Skill != 2 && Skill3Active)
				DeactivateSkill3();

			if (SkillVolleyTimer > 0)
				SkillVolleyTimer--;

			SyncSkillActiveState(wp);

			if (Main.netMode != NetmodeID.MultiplayerClient) {
				FrostDomainLogic.TickPassiveGrowth(Player, this);
				FrostDomainLogic.TickDomain(Player, this);
			}

			PramanixColdAttachment.TickPending();
			PramanixColdAttachment.CleanupInactive();
			UpdateEmblemAfterglow();
		}

		private void SyncSkillActiveState(WeaponPlayer wp) {
			if (Skill2Active && wp.Skill == 1 && !wp.SkillActive) {
				wp.SkillActive = true;
				wp.SkillTimer = 0;
				wp.UpdateActiveSkill2();
			}

			if (Skill3Active && wp.Skill == 2 && !wp.SkillActive)
				DeactivateSkill3();
		}

		public void SpawnSkill2Shockwave() {
			if (Player.whoAmI != Main.myPlayer)
				return;

			PramanixSoundHelper.PlayShockwaveFire(Player.Center);

			float radius = GetSkill2ShockwaveRadius();
			int damage = Math.Max(1, (int)(FrostDomainLogic.GetMagicDamage(Player) * 3.8f));
			var source = Player.GetSource_ItemUse(Player.HeldItem);
			Projectile.NewProjectile(source, Player.Center, Vector2.Zero,
				ModContent.ProjectileType<PramanixShockwaveRing>(), damage, 0f, Player.whoAmI, radius);
			SkillVolleyTimer = GetSkillVolleyCooldown();
		}

		public void SpawnSkill3Volley() {
			if (Player.whoAmI != Main.myPlayer)
				return;

			int damage = GetSkill3VolleyDamage();
			ApplySkill3AreaDamage(damage);

			var source = Player.GetSource_ItemUse(Player.HeldItem);
			Projectile.NewProjectile(source, Player.Center, Vector2.Zero,
				ModContent.ProjectileType<PramanixSkill3VolleyFx>(), 0, 0f, Player.whoAmI);
			PramanixSoundHelper.PlaySkill3Attack(Player.Center);
			SkillVolleyTimer = GetSkillVolleyCooldown();
		}

		private int GetSkillVolleyCooldown() =>
			Skill3Active ? Math.Max(1, (int)(SkillVolleyCooldown / (1f + Skill3UseSpeedBonus))) : SkillVolleyCooldown;

		public int GetSkill3VolleyDamage() =>
			Math.Max(1, (int)(FrostDomainLogic.GetMagicDamage(Player) * Skill3VolleyDamageMult));

		public float GetSkill3AttackRadius() =>
			PramanixSkill3Geometry.GetSkill3AreaRadius(Player.direction);

		private void ApplySkill3AreaDamage(int damage) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			float radius = GetSkill3AttackRadius();
			float radiusSq = radius * radius;
			Vector2 center = Player.Center;

			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.active || !npc.CanBeChasedBy(Player))
					continue;
				if (Vector2.DistanceSquared(npc.Center, center) > radiusSq)
					continue;

				ApplySkill3HitToNpc(npc, damage);
			}
		}

		public void ApplySkill3BoltHit(int pathIndex, int direction, int damage) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			Vector2 boltPos = PramanixSkill3Geometry.SamplePathWorld(pathIndex, PramanixSkill3Geometry.PathL, Player.Center, direction);
			float hitRadiusSq = PramanixSkill3Geometry.Skill3BoltHitRadius * PramanixSkill3Geometry.Skill3BoltHitRadius;

			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.active || !npc.CanBeChasedBy(Player))
					continue;
				if (Vector2.DistanceSquared(npc.Center, boltPos) > hitRadiusSq)
					continue;

				ApplySkill3HitToNpc(npc, damage);
			}
		}

		private void ApplySkill3HitToNpc(NPC npc, int damage) {
			StrikeSkill3Magic(npc, damage);
			npc.AddBuff(ModContent.BuffType<PramanixSlowDebuff>(), 24);
			PramanixHitVfx.SpawnSlowSmoke(npc);
			PramanixColdAttachment.ApplySkill2Hit(npc, Player);
		}

		public void StrikeSkill3Magic(NPC npc, int damage) {
			float savedArts = 0f;
			bool hasCat = npc.TryGetGlobalNPC(out DamageCategoryNPC cat);
			if (hasCat) {
				savedArts = cat.artsResistance;
				cat.artsResistance = Math.Max(0f, cat.artsResistance - Skill3ArtsResistIgnore);
			}

			FrostDomainLogic.StrikeMagic(Player, npc, damage);

			if (hasCat)
				cat.artsResistance = savedArts;
		}

		private void ApplySkill3InitialLure() {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			float radius = GetSkill3AttackRadius();
			Player.aggro += 1200;

			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.active || !npc.CanBeChasedBy(Player))
					continue;
				if (npc.Distance(Player.Center) > radius)
					continue;

				npc.GetGlobalNPC<PramanixSkill3LureGlobalNPC>().BeginLure(Player.whoAmI, Skill3LureDuration);
			}
		}

		private float GetSkill2ShockwaveRadius() {
			int tier = Math.Max(FrostTier, 2);
			float tierScale = 0.88f + (tier - 2) * 0.06f;
			return Skill2ShockwaveMaxRadius * tierScale;
		}

		public void ActivateSkill1() {
			if (Player.whoAmI != Main.myPlayer)
				return;

			var wp = Player.GetModPlayer<WeaponPlayer>();
			wp.DelStockCount();
			wp.SkillTimer  = 0;
			wp.SkillActive = false;

			SpawnSkill1Snowflakes();
			SpawnSkill1GroundSnow();
		}

		private void SpawnSkill1Snowflakes() {
			if (Player.whoAmI != Main.myPlayer)
				return;

			var source   = Player.GetSource_ItemUse(Player.HeldItem);
			int projType = ModContent.ProjectileType<PramanixSkill1Snowflake>();

			// 6片雪花，垂直分布形成"风墙"外观
			float[] yOffsets = [-88f, -60f, -35f, -12f, 12f, 32f];
			float baseSpeed  = 15f;

			for (int i = 0; i < 6; i++) {
				float yOff   = yOffsets[i] + Main.rand.NextFloat(-5f, 5f);
				float xOff   = Main.rand.NextFloat(-13f, 13f);
				Vector2 pos  = Player.Center + new Vector2(Player.direction * xOff, yOff);
				float speedX = baseSpeed + Main.rand.NextFloat(-0.9f, 0.9f);
				float speedY = Main.rand.NextFloat(-0.45f, 0.45f);
				Vector2 vel  = new(Player.direction * speedX, speedY);
				Projectile.NewProjectile(source, pos, vel, projType, 0, 0f, Player.whoAmI);
			}
		}

		// 积雪向前方地面扩散（最多5格）的视觉效果
		private void SpawnSkill1GroundSnow() {
			if (Player.whoAmI != Main.myPlayer || Main.netMode == NetmodeID.Server)
				return;

			Vector2 feet = Player.Bottom;
			int dir      = Player.direction;
			float maxX   = Skill1SnowSpreadTiles * 16f;

			for (int i = 0; i < 22; i++) {
				float t    = i / 21f;
				float xOff = t * maxX * dir + Main.rand.NextFloat(-7f, 7f);
				float yOff = Main.rand.NextFloat(0f, 6f);
				Vector2 pos = feet + new Vector2(xOff, yOff);

				Dust d = Dust.NewDustPerfect(pos, DustID.Snow,
					new Vector2(dir * Main.rand.NextFloat(0.4f, 2f), Main.rand.NextFloat(-1f, -0.2f)),
					115, new Color(215, 238, 255), 0.65f + Main.rand.NextFloat(0.45f));
				d.noGravity = false;
				d.fadeIn    = 0.3f;
			}
		}

		public void ActivateSkill2() {
			DeactivateSkill3();
			Skill2Active = true;
			SkillVolleyTimer = 0;

			var wp = Player.GetModPlayer<WeaponPlayer>();
			wp.SkillActive = true;
			wp.SkillTimer = 0;
			wp.UpdateActiveSkill2();

			if (FrostTier < 2)
				FrostDomainLogic.SetTier(this, 2);
		}

		public void DeactivateSkill2() {
			Skill2Active = false;

			var wp = Player.GetModPlayer<WeaponPlayer>();
			if (wp.Skill == 1) {
				wp.SkillActive = false;
				wp.SkillTimer = 0;
			}
		}

		public void ActivateSkill3() {
			DeactivateSkill2();
			Skill3Active = true;
			SkillVolleyTimer = 0;

			var wp = Player.GetModPlayer<WeaponPlayer>();
			wp.DelStockCount();
			wp.SkillActive = true;
			wp.SkillTimer = 0;

			FrostDomainLogic.SetTier(this, 3);
			ApplySkill3InitialLure();
		}

		public void DeactivateSkill3() {
			Skill3Active = false;
			emblemAfterglow = 0f;

			var wp = Player.GetModPlayer<WeaponPlayer>();
			if (wp.Skill == 2) {
				wp.SkillActive = false;
				wp.SkillTimer = 0;
			}
		}

		// 攻击时淡入，淡出晚于弹幕，峰值不超过 EmblemMaxDisplayAlpha
		public float GetEmblemDisplayAlpha() =>
			emblemAfterglow * PramanixSkill3Geometry.EmblemMaxDisplayAlpha;

		private void UpdateEmblemAfterglow() {
			if (!Skill3Active) {
				emblemAfterglow = 0f;
				return;
			}

			float volleyFactor = GetActiveVolleyEmblemFactor();
			if (volleyFactor > 0f) {
				emblemAfterglow = volleyFactor;
				return;
			}

			if (emblemAfterglow > 0f)
				emblemAfterglow = Math.Max(0f, emblemAfterglow - 1f / PramanixSkill3Geometry.EmblemLingerTicks);
		}

		private float GetActiveVolleyEmblemFactor() {
			int volleyType = ModContent.ProjectileType<PramanixSkill3VolleyFx>();
			float factor = 0f;

			for (int i = 0; i < Main.maxProjectiles; i++) {
				Projectile proj = Main.projectile[i];
				if (!proj.active || proj.type != volleyType || proj.owner != Player.whoAmI)
					continue;
				if (proj.ModProjectile is not PramanixSkill3VolleyFx volley)
					continue;

				factor = Math.Max(factor, GetVolleyEmblemFactor(volley, proj));
			}

			return factor;
		}

		private static float GetVolleyEmblemFactor(PramanixSkill3VolleyFx volley, Projectile proj) {
			var state = volley.GetVisualState();
			if (state.SpawningSparks)
				return EaseIn(MathHelper.Clamp(state.TMax / PramanixSkill3Geometry.PathL, 0f, 1f));

			int fadeElapsed = PramanixSkill3VolleyFx.FadeTicks - proj.timeLeft;
			if (fadeElapsed < PramanixSkill3Geometry.EmblemFadeDelayTicks)
				return 1f;

			float t = (fadeElapsed - PramanixSkill3Geometry.EmblemFadeDelayTicks) / (float)PramanixSkill3Geometry.EmblemFadeTicks;
			return 1f - MathHelper.Clamp(t, 0f, 1f);
		}

		private static float EaseIn(float t) => t * t;
	}
}
