using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Players;
using ArknightsMod.Systems.Gameplay.Skill;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP2;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP1;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.SP3;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.EffectImage;
using ArknightsMod.Content.Projectiles.Supporter.Tragodia.NormalAttack;
using ArknightsMod.Content.ElementalImpairment.Effect;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Supporter.Tragodia
{
	public class Tragodia : ExpansionWeaponBase
	{
		protected override int[] EliteDamage => [64, 82, 98];

		public const int BaseUseTime = 48;
		public const int S2UseTime = 31;
		public const float BaseKnockback = 3f;
		public const float BaseShootSpeed = 1f;

		private static SoundStyle SkillActiveSound;
		private static SoundStyle Attack;
		private static SoundStyle Hit;
		private static SoundStyle SP1;
		private static SoundStyle SP1_Hit;
		private static SoundStyle SP2Attack;
		private static SoundStyle SP3;
		private static SoundStyle SP3Attack;

		private int spDelayTimer = 0;
		private NPC spTarget = null;
		private int spDamage = 0;
		private int instinctCallCooldown = 0;
		private const int InstinctCallCooldownTime = 600;

		public override void Load() {
			SkillActiveSound = new SoundStyle("ArknightsMod/Sounds/SkillActive1") { Volume = 0.5f, MaxInstances = 3 };
			Attack = new SoundStyle("ArknightsMod/Assets/Sound/Tragodia/Attack") { Volume = 1.5f, MaxInstances = 5 };
			Hit = new SoundStyle("ArknightsMod/Assets/Sound/Tragodia/Hit") { Volume = 1.5f, MaxInstances = 5 };
			SP1 = new SoundStyle("ArknightsMod/Assets/Sound/Tragodia/SP1") { Volume = 1.5f, MaxInstances = 3 };
			SP1_Hit = new SoundStyle("ArknightsMod/Assets/Sound/Tragodia/SP1_Hit") { Volume = 1.5f, MaxInstances = 5 };
			SP2Attack = new SoundStyle("ArknightsMod/Assets/Sound/Tragodia/SP2Attack") { Volume = 1.5f, MaxInstances = 5 };
			SP3 = new SoundStyle("ArknightsMod/Assets/Sound/Tragodia/SP3") { Volume = 1.5f, MaxInstances = 3 };
			SP3Attack = new SoundStyle("ArknightsMod/Assets/Sound/Tragodia/SP3Attack") { Volume = 1.5f, MaxInstances = 5 };
		}

		public override void HoldItem(Player player) {
			base.HoldItem(player);
			var modPlayer = player.GetModPlayer<WeaponPlayer>();
			if (modPlayer.IconName != Name)
				modPlayer.IconName = Name;

			if (spDelayTimer > 0) {
				spDelayTimer--;
				if (spDelayTimer == 0 && spTarget != null && spTarget.active) {
					SP1Extract.SpawnPair(player, spTarget, spDamage);
					spTarget = null;
				}
			}

			if (instinctCallCooldown > 0)
				instinctCallCooldown--;

			if (modPlayer.Skill == 1 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
				modPlayer.SkillActive = true;
				modPlayer.SkillTimer = 0;
				modPlayer.DelStockCount();
				if (modPlayer.CurrentSkill != null)
					modPlayer.CurrentSkill.AutoUpdateActive = false;
				SoundEngine.PlaySound(SkillActiveSound, player.Center);
			}
		}

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Ranged;
			Item.width = 40;
			Item.height = 32;
			Item.useTime = BaseUseTime;
			Item.useAnimation = BaseUseTime;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = BaseKnockback;
			Item.shootSpeed = BaseShootSpeed;
			Item.shoot = ModContent.ProjectileType<TragodiaWaveSpawner>();
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.value = Item.buyPrice(gold: 5);
			Item.rare = ItemRarityID.LightRed;
			Item.mana = 10;
		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool CanUseItem(Player player) {
			if (Main.myPlayer != player.whoAmI)
				return base.CanUseItem(player);

			var modPlayer = player.GetModPlayer<WeaponPlayer>();

			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				if (modPlayer.Skill == 2 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
					modPlayer.SkillActive = true;
					modPlayer.SkillTimer = 0;
					modPlayer.DelStockCount();
					SoundEngine.PlaySound(SP3, player.Center);
					return false;
				}
				return false;
			}

			if (player.altFunctionUse == 2) {
				if (modPlayer.Skill == 1 && modPlayer.SkillActive) {
					return true;
				}
				return false;
			}

			if (player.statMana < Item.mana)
				return false;

			if (modPlayer.Skill == 0) {
				if (modPlayer.StockCount == 0) {
					modPlayer.OffensiveRecovery();
				}
				else if (modPlayer.StockCount > 0) {
					modPlayer.SkillActive = true;
					modPlayer.SkillTimer = 0;
					modPlayer.DelStockCount();
				}
			}

			if (modPlayer.Skill == 1 && modPlayer.SkillActive) {
				Item.useTime = S2UseTime;
				Item.useAnimation = S2UseTime;
			}
			else {
				Item.useTime = BaseUseTime;
				Item.useAnimation = BaseUseTime;
			}

			return base.CanUseItem(player);
		}

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			if (Main.myPlayer != player.whoAmI)
				return;
			var modPlayer = player.GetModPlayer<WeaponPlayer>();
			if (modPlayer.Skill == 0 && modPlayer.SkillActive)
				damage *= 1.6f;
			if (modPlayer.Skill == 2 && modPlayer.SkillActive)
				damage *= 2.25f;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			var modPlayer = player.GetModPlayer<WeaponPlayer>();

			if (player.altFunctionUse == 2) {
				if (modPlayer.Skill == 1 && modPlayer.SkillActive) {
					bool detonated = false;
					foreach (Projectile proj in Main.ActiveProjectiles) {
						if (proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<InstinctCall>() && proj.ModProjectile is InstinctCall call) {
							call.ForceExplode();
							detonated = true;
						}
					}

					if (!detonated && instinctCallCooldown <= 0) {
						Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero,
							ModContent.ProjectileType<InstinctCall>(), 0, 0, player.whoAmI);
						instinctCallCooldown = InstinctCallCooldownTime;
					}
				}
				return false;
			}

			if (player.statMana >= Item.mana)
				player.statMana -= Item.mana;

			int baseProjectile = ModContent.ProjectileType<TragodiaWaveSpawner>();
			SoundEngine.PlaySound(Attack, player.Center);

			if (modPlayer.Skill == 0 && modPlayer.SkillActive) {
				ShootSkill1(player, source, damage, knockback, baseProjectile);
				return false;
			}
			if (modPlayer.Skill == 1 && modPlayer.SkillActive) {
				ShootSkill2(player, source, damage, knockback, baseProjectile);
				return false;
			}
			if (modPlayer.Skill == 2 && modPlayer.SkillActive) {
				ShootSkill3(player, source, damage, knockback, baseProjectile);
				return false;
			}

			ShootNormal(source, damage, knockback, baseProjectile);
			return false;
		}

		public override Vector2? HoldoutOffset() => new Vector2(-4, 0);

		private bool ShootSkill1(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback, int baseProjectile) {
			SoundEngine.PlaySound(SP1, player.Center);

			NPC target = FindClosestEnemy(player, Main.MouseWorld, 800f);
			if (target != null) {
				BindingEffect.Apply(target, duration: 180, slowAmount: 0.98f, freezeAI: true);
				spDelayTimer = 12;
				spTarget = target;
				spDamage = damage;
			}

			Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero,
				baseProjectile, damage, knockback, player.whoAmI, 1f, 0f, 0f);
			return false;
		}

		private bool ShootSkill2(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback, int baseProjectile) {
			Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero,
				baseProjectile, damage, knockback, player.whoAmI, 0f, 1f, 0f);
			return false;
		}

		private bool ShootSkill3(Player player, EntitySource_ItemUse_WithAmmo source, int damage, float knockback, int baseProjectile) {
			Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero,
				baseProjectile, damage, knockback, player.whoAmI, 0f, 0f, 1f);
			return false;
		}

		private void ShootNormal(EntitySource_ItemUse_WithAmmo source, int damage, float knockback, int baseProjectile) {
			Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero,
				baseProjectile, damage, knockback, Main.myPlayer, 0f, 0f, 0f);
		}

		private static NPC FindClosestEnemy(Player player, Vector2 position, float maxRange) {
			NPC closest = null;
			float closestDist = maxRange;
			foreach (NPC npc in Main.ActiveNPCs) {
				if (!npc.CanBeChasedBy(player) || npc.friendly)
					continue;
				float dist = Vector2.Distance(npc.Center, position);
				if (dist < closestDist) {
					closestDist = dist;
					closest = npc;
				}
			}
			return closest;
		}

		public class TragodiaWaveSpawner : ModProjectile
		{
			private NPC targetNPC;
			private bool hasAttached;
			private int attachTimer;
			private const int WaitFrames = 12;

			private static SoundStyle AttackSound;
			private static SoundStyle HitSound;
			private static SoundStyle SP1HitSound;
			private static SoundStyle SP2AttackSound;
			private static SoundStyle SP3AttackSound;

			public override void Load() {
				AttackSound = new SoundStyle("ArknightsMod/Assets/Sound/Tragodia/Attack") { Volume = 1.5f, MaxInstances = 5 };
				HitSound = new SoundStyle("ArknightsMod/Assets/Sound/Tragodia/Hit") { Volume = 1.5f, MaxInstances = 5 };
				SP1HitSound = new SoundStyle("ArknightsMod/Assets/Sound/Tragodia/SP1_Hit") { Volume = 1.5f, MaxInstances = 5 };
				SP2AttackSound = new SoundStyle("ArknightsMod/Assets/Sound/Tragodia/SP2Attack") { Volume = 1.5f, MaxInstances = 5 };
				SP3AttackSound = new SoundStyle("ArknightsMod/Assets/Sound/Tragodia/SP3Attack") { Volume = 1.5f, MaxInstances = 5 };
			}

			public override void SetDefaults() {
				Projectile.width = 1;
				Projectile.height = 1;
				Projectile.friendly = false;
				Projectile.hostile = false;
				Projectile.DamageType = DamageClass.Magic;
				Projectile.penetrate = -1;
				Projectile.tileCollide = false;
				Projectile.timeLeft = 60;
				Projectile.alpha = 255;
				Projectile.ignoreWater = true;
			}

			public override void AI() {
				if (!hasAttached) {
					hasAttached = true;
					AttachToTarget();
				}

				if (targetNPC != null && targetNPC.active)
					Projectile.Center = targetNPC.Center + new Vector2(0, 20f);

				attachTimer++;
				if (attachTimer >= WaitFrames) {
					Explode();
					Projectile.Kill();
				}
			}

			private void AttachToTarget() {
				Player player = Main.player[Projectile.owner];
				if (player == null || !player.active)
					return;

				targetNPC = FindClosestEnemy();

				SoundEngine.PlaySound(AttackSound, Projectile.Center);

				if (targetNPC != null) {
					Projectile.Center = targetNPC.Center + new Vector2(0, 20f);
					int dustType = ModContent.ProjectileType<Pre_Attack>();
					Projectile dustProj = Projectile.NewProjectileDirect(
						Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
						dustType, 0, 0, Projectile.owner);
					if (dustProj.ModProjectile is Pre_Attack windDust) {
						windDust.targetNPCIndex = targetNPC.whoAmI;
						windDust.offsetY = 20f;
					}
				}
				else {
					Projectile.Center = Main.MouseWorld;
					int dustType = ModContent.ProjectileType<Pre_Attack>();
					Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero,
						dustType, 0, 0, Projectile.owner);
				}
			}

			private NPC FindClosestEnemy() {
				Player player = Main.player[Projectile.owner];
				if (player == null || !player.active)
					return null;

				NPC closest = null;
				float closestDist = 800f;
				foreach (NPC npc in Main.ActiveNPCs) {
					if (!npc.CanBeChasedBy(player) || npc.friendly)
						continue;
					float dist = Vector2.Distance(Main.MouseWorld, npc.Center);
					if (dist < closestDist) {
						closestDist = dist;
						closest = npc;
					}
				}
				return closest;
			}

			private void Explode() {
				bool isSP1 = Projectile.ai.Length > 0 && Projectile.ai[0] == 1f;
				bool isSP2 = Projectile.ai.Length > 1 && Projectile.ai[1] == 1f;
				bool isSP3 = Projectile.ai.Length > 2 && Projectile.ai[2] == 1f;

				if (isSP1) {
					SoundEngine.PlaySound(SP1HitSound, Projectile.Center);
				}
				else if (isSP2) {
					SoundEngine.PlaySound(SP2AttackSound, Projectile.Center);
				}
				else if (isSP3) {
					SoundEngine.PlaySound(SP3AttackSound, Projectile.Center);
				}
				else {
					SoundEngine.PlaySound(HitSound, Projectile.Center);
				}

				int attackType;
				float effectScale = 1f;
				float waveAmplitude = 1f;

				if (isSP1) {
					attackType = ModContent.ProjectileType<SP1TragodiaNormalAttack>();
				}
				else if (isSP2) {
					attackType = ModContent.ProjectileType<SP2TragodiaNormalAttack>();
					effectScale = 1.1f;
					waveAmplitude = 1.5f;
				}
				else if (isSP3) {
					attackType = ModContent.ProjectileType<SP3TragodiaNormalAttack>();
					effectScale = 1.1f;
					waveAmplitude = 2.0f;
				}
				else {
					attackType = ModContent.ProjectileType<TragodiaNormalAttack>();
				}

				var source = Projectile.GetSource_FromThis();
				var center = Projectile.Center;
				var owner = Projectile.owner;

				Projectile.NewProjectile(source, center, Vector2.Zero,
					attackType, Projectile.damage, Projectile.knockBack, owner);

				if (isSP3) {
					Projectile.NewProjectile(source, center, Vector2.Zero,
						ModContent.ProjectileType<SP3Wave>(), 0, 0, owner, effectScale, waveAmplitude);
					for (int i = 0; i < 3; i++) {
						Projectile.NewProjectile(source, center, Vector2.Zero,
							ModContent.ProjectileType<SP3ExtraVisual>(), 0, 0, owner,
							effectScale, waveAmplitude, i);
					}
				}
				else if (isSP2) {
					Projectile.NewProjectile(source, center, Vector2.Zero,
						ModContent.ProjectileType<SP2Wave>(), 0, 0, owner, effectScale, waveAmplitude);
					Projectile.NewProjectile(source, center, Vector2.Zero,
						ModContent.ProjectileType<SP2ExtraVisual>(), 0, 0, owner);
				}
				else {
					Projectile.NewProjectile(source, center, Vector2.Zero,
						ModContent.ProjectileType<WaveProjectile>(), 0, 0, owner, effectScale, waveAmplitude);
				}

				Projectile.NewProjectile(source, center, Vector2.Zero,
					ModContent.ProjectileType<Light>(), 0, 0, owner, effectScale, 0f);
				Projectile.NewProjectile(source, center, Vector2.Zero,
					ModContent.ProjectileType<RibbonProjectile>(), 0, 0, owner, effectScale, 0f);
			}
		}
	}
}