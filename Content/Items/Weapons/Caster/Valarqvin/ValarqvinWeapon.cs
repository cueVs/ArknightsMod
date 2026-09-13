using ArknightsMod.Content.ElementalImpairment.Effect;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.Sniper.KroosAlter;
using ArknightsMod.Content.Projectiles.Caster.Valarqvin;
using ArknightsMod.Players;
using ArknightsMod.Systems.Gameplay.Skill;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Caster.Valarqvin
{
	public class ValarqvinWeapon : UpgradeWeaponBase
	{
		public const int BaseDamage = 65;
		public const int BaseUseTime = 48;
		public const float BaseKnockback = 2f;
		public const float BaseShootSpeed = 13;

		private static SoundStyle SkillActiveSound;
		private static SoundStyle AttackSound;
		private static SoundStyle SkillAttackSound; 

		public override void Load() {
			SkillActiveSound = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.5f,
				MaxInstances = 3,
			};
			AttackSound = new SoundStyle("ArknightsMod/Assets/Sound/Valarqvin/Valarqvin") {
				Volume = 0.7f,
				MaxInstances = 5,
			};
			SkillAttackSound = new SoundStyle("ArknightsMod/Assets/Sound/Valarqvin/Valarqvin_1") {
				Volume = 0.7f,
				MaxInstances = 5,
			};
		}

		public override void HoldItem(Player player) {
			var modPlayer = player.GetModPlayer<WeaponPlayer>();
			if (modPlayer.IconName != Name)
				modPlayer.IconName = Name;
		}

		public override void SetDefaults() {
			Item.damage = BaseDamage;
			Item.DamageType = DamageClass.Ranged;
			Item.width = 40;
			Item.height = 32;
			Item.useTime = BaseUseTime;
			Item.useAnimation = BaseUseTime;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = BaseKnockback;
			Item.shootSpeed = BaseShootSpeed;
			Item.shoot = ModContent.ProjectileType<ValarqvinProj>();
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.value = Item.buyPrice(gold: 1);
			Item.rare = ItemRarityID.Blue;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player) {
			if (Main.myPlayer != player.whoAmI)
				return base.CanUseItem(player);

			var modPlayer = player.GetModPlayer<WeaponPlayer>();


			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				if (modPlayer.Skill == 1 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
					modPlayer.SkillActive = true;
					modPlayer.SkillTimer = 0;
					modPlayer.DelStockCount();
					SoundEngine.PlaySound(SkillActiveSound, player.Center);
				}
				return false;
			}


			if (modPlayer.Skill == 0 && modPlayer.StockCount > 0 && !modPlayer.SkillActive) {
				modPlayer.SkillActive = true;
				modPlayer.SkillTimer = 0;
				modPlayer.DelStockCount();
			}

			if (modPlayer.CurrentSkill?.ChargeType == SkillChargeType.Attack && !modPlayer.SkillActive)
				modPlayer.OffensiveRecovery();

			if (modPlayer.Skill == 1 && modPlayer.SkillActive) {
				Item.useTime = (int)Math.Max(1, BaseUseTime / 1.5f); 
				Item.useAnimation = Item.useTime;
				SoundEngine.PlaySound(SkillAttackSound, player.Center); 
			}
			else {
				Item.useTime = BaseUseTime;
				Item.useAnimation = BaseUseTime;
				SoundEngine.PlaySound(AttackSound, player.Center); 
			}

			return base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
						   Vector2 position, Vector2 velocity,
						   int type, int damage, float knockback) {
			var wp = player.GetModPlayer<WeaponPlayer>();
			int arrowType = ModContent.ProjectileType<ValarqvinProj>();
			int arrowType2 = ModContent.ProjectileType<ValarqvinProj_2>();

		
			if (wp.Skill == 0 && wp.SkillActive) {
				int enhancedDamage = (int)(damage * 1.3f); 
				Projectile.NewProjectile(source, position, velocity, arrowType, enhancedDamage, knockback,
					player.whoAmI, ai0: 1f);
				wp.SkillActive = false;
				return false;
			}

			
			if (wp.Skill == 1 && wp.SkillActive) {
				Projectile.NewProjectile(source, position, velocity, arrowType2, damage, knockback, player.whoAmI);

				var mousePos = Main.MouseWorld;
				NPC first = null, second = null;
				float firstDistSq = float.MaxValue, secondDistSq = float.MaxValue;

				foreach (NPC npc in Main.ActiveNPCs) {
					if (!npc.CanBeChasedBy(player) || npc.friendly)
						continue;
					float distSq = Vector2.DistanceSquared(npc.Center, mousePos);
					if (distSq < firstDistSq) {
						secondDistSq = firstDistSq;
						second = first;
						firstDistSq = distSq;
						first = npc;
					}
					else if (distSq < secondDistSq) {
						secondDistSq = distSq;
						second = npc;
					}
				}

				if (second != null) {
					Vector2 aimVel = (second.Center - position).SafeNormalize(Vector2.Zero) * velocity.Length();
					Projectile.NewProjectile(source, position, aimVel, arrowType2, damage, knockback, player.whoAmI);
				}
				return false;
			}

		
			Projectile.NewProjectile(source, position, velocity, arrowType, damage, knockback, player.whoAmI);
			return false;
		}

		public override Vector2? HoldoutOffset() => new Vector2(-4, 0);
	}

	public class ValarqvinPlayer : ModPlayer
	{
		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			if (Player.HeldItem?.ModItem is not ValarqvinWeapon)
				return;

			var wp = Player.GetModPlayer<WeaponPlayer>();
			float necroRatio = 0.15f; 

			if (proj.ai[0] == 1f) {
				necroRatio += 0.80f; 
			}

			else if (wp.Skill == 1 && wp.SkillActive) {
				necroRatio += 0.35f; 
			}

			int value = (int)(damageDone * necroRatio);
			if (value <= 0)
				return;

			var container = target.GetGlobalNPC<AfflictionGlobalNPC>().Container;
			container?.AddAfflictionValue<NecrosisImpairment>(value);
		}
	}
}