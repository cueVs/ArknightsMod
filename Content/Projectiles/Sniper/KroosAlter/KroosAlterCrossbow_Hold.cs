using ArknightsMod.Content.Items.Weapons.Sniper.KroosAlter;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Sniper.KroosAlter
{
    public class KroosAlterCrossbow_Hold : HoldProjectile
    {
		public override int BindingItemType() => ModContent.ItemType<KroosAlterCrossbow>();
		public override float Speed() => 24f;

		private int circleProj = ModContent.ProjectileType<KroostheKeenGlint_Crossbow_Circle1>();
		private int effectProj = ModContent.ProjectileType<KroosAlterCrossbow_Effect>();
		private int arrowProj = ModContent.ProjectileType<KroosAlterCrossbow_Arrow>();

		/// <summary>
		/// 获取当前武器实例（缓存以避免重复转换）
		/// </summary>
		protected KroosAlterCrossbow Crossbow {
			get {
				if (_cachedCrossbow == null && player.HeldItem.ModItem is KroosAlterCrossbow cb)
					_cachedCrossbow = cb;
				return _cachedCrossbow;
			}
		}
		private KroosAlterCrossbow _cachedCrossbow;

		public override void InitializeProjectile()
        {
            Projectile.width = 60;
            Projectile.height = 28;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.coldDamage = true; // 你确定要加这个？
        }

		public override void OnAI_Beginning() {
			// 清除缓存
			_cachedCrossbow = null;
		}
		public override void UpdateSkill_1(int baseUseTime, int baseReuseDelay) {
			// 2倍攻速
			int useTime = ApplyAttackSpeed(baseUseTime * 0.5f);

			if (Timer < useTime) {
				int interval = useTime / 2;
				// 二连发
				if (Timer % interval == 0) {

					ShootBullet(player, MathHelper.ToRadians(2));
					ShootEffect(player);
					Projectile.NewProjectile(Projectile.GetSource_FromAI(),
						Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16,
						Projectile.velocity.SafeNormalize(Vector2.Zero),
						circleProj,
						0, 0, player.whoAmI, 0, 0, CurrentSkill);

					SoundStyle SkillSound = new SoundStyle("ArknightsMod/Sounds/p_atk_krossbow_h") with {
						Volume = 0.5f
					};
					SoundEngine.PlaySound(SkillSound, Projectile.position);

					ShootTimes++;

					if (ShootTimes >= 4) {
						Timer = useTime; // 进入useDelay阶段
						ShootTimes = 0;
					}
				}
			}
			// 冷却阶段处理
			else if (Timer >= useTime + baseReuseDelay) {
				Timer = 0;
			}
		}
		public override void UpdateSkill_2(int baseUseTime, int baseReuseDelay) {
			// 2倍攻速
			int useTime = ApplyAttackSpeed(baseUseTime * 0.5f);
			// 从 ModItem 读取累计次数
			int shotsPerInterval = Crossbow?.Skill2HitCounter < 32 ? 2 : 4;

			if (Timer < useTime) {
				int interval = Math.Max(1, useTime / shotsPerInterval);

				if (Timer % interval == 0) {

					// 射弹、特效、音效...
					ShootBullet(player, MathHelper.ToRadians(2));
					ShootEffect(player);
					Projectile.NewProjectile(Projectile.GetSource_FromAI(),
						Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16,
						Projectile.velocity.SafeNormalize(Vector2.Zero),
						circleProj,
						0, 0, player.whoAmI, 0, 0, CurrentSkill);

					SoundStyle SkillSound = new("ArknightsMod/Sounds/p_atk_krossbow_n") { Volume = 0.5f };
					SoundEngine.PlaySound(SkillSound, Projectile.position);

					ShootTimes++;

					// 关键：累计攻击次数 +1（存在 ModItem 中）
					if (Crossbow != null)
						Crossbow.Skill2HitCounter++;

					if (ShootTimes >= 4) {
						Timer = useTime;
						ShootTimes = 0;
					}
				}
			}
			else if (Timer >= useTime + baseReuseDelay) {
				Timer = 0;
			}
		}
		public override void UpdateNormalAttack(int baseUseTime, int baseReuseDelay) {
			//攻速不变
			int useTime = ApplyAttackSpeed(baseUseTime);
			if (Timer >= useTime + baseReuseDelay) {
				ShootBullet(player, 0);
				ShootEffect(player);

				SoundStyle AttackSound = new SoundStyle("ArknightsMod/Sounds/p_atk_krossbow_d") with {
					Volume = 0.5f
				};
				SoundEngine.PlaySound(AttackSound, Projectile.position);

				Timer = 0;
			}
		}

		private void ShootBullet(Player player, float rot)
        {
			if (player.HasAmmo(player.inventory[player.selectedItem]))
            {
				bool canUse = player.channel && player.HasAmmo(player.inventory[player.selectedItem]) && !player.noItems && !player.CCed;
                
                player.PickAmmo(player.inventory[player.selectedItem],
                    out int weaponAmmo,
                    out float shootSpeed,
                    out int weaponDamage,
                    out float weaponKnockback,
                    out weaponAmmo,
                    canUse
                    );

				if (modPlayer.Skill == 0 && modPlayer.StockCount == 0) { 
					modPlayer.OffensiveRecovery();
				}
				Projectile.NewProjectileDirect(player.GetSource_ItemUse_WithPotentialAmmo(player.inventory[player.selectedItem], weaponAmmo),
                    player.Center,
                    Projectile.velocity.RotatedByRandom(rot),
					arrowProj, weaponDamage, weaponKnockback, player.whoAmI, 0, 0, CurrentSkill);
            }
        }

        private void ShootEffect(Player player)
        {
            for (int j = 0; j < 2; j++)
            {
                float[] rand = 
                    {
                    Main.rand.NextFloat(-45f, -15f),
                    Main.rand.NextFloat(-15f, 15f),
                    Main.rand.NextFloat(15f, 45f)
                    };

                for (int i = 0; i < 3; i++)
                {
                    float angle = rand[i];
                    float angleMagnitude = Math.Abs(angle);

                    float speedFactor = 0.5f + angleMagnitude / 45f * 0.5f;

                    Vector2 vel = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.ToRadians(angle)) * speedFactor * Main.rand.NextFloat(0.5f, 1.5f)  * 8;

                    Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromAI(),
                        Projectile.Center + Projectile.velocity.SafeNormalize(Vector2.Zero) * 16,
                        vel, effectProj, 0, 0, player.whoAmI, 0, 0, CurrentSkill);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            var origin = new Vector2(tex.Width / 2, tex.Height / 2);
            SpriteEffects effects = Projectile.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(
                tex,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor,
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0
            );
            return false;
        }
    }
}
