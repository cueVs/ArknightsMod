using ArknightsMod.Content;
using ArknightsMod.Content.Projectiles.Guard.Saki;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ArknightsMod.Content.Items.Weapons.Guard.Oblivionis
{
	public class OblivionisSword : ModItem
	{
		public override void SetDefaults() {
			Item.damage = 145;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 44;
			Item.useTime = 18;
			Item.useAnimation = 18;
			Item.useStyle = 5;
			Item.knockBack = 5f;
			Item.shootSpeed = 6f;
			Item.shoot = ProjectileType<SakiSwordWhite>();
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.autoReuse = true;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Cyan;
		}
		public override bool CanUseItem(Player player) {
			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				skills++;
				if (skills > 4)
					skills = 0;
			}
			return player.ownedProjectileCounts[ProjectileType<SakiSwordWhite>()] <= 0 && player.ownedProjectileCounts[ProjectileType<SakiSwordBlack>()] <= 0;
		}
		public int skills = 0;
		public override bool AltFunctionUse(Player player) => false;

		public int swordDir = 1;

		public int timer;
		public override void HoldItem(Player Player) {
			if (skills == 0 && Player.ownedProjectileCounts[ProjectileType<SakiSwordWhite>()] <= 0 && Player.ownedProjectileCounts[ProjectileType<SakiSwordBlack>()] <= 0) {
				timer++;
				if (timer > 60) {
					timer = 0;
					Vector2 velocity = Main.MouseWorld - Player.Center;
					velocity.Normalize();
					velocity *= 4;
					Projectile.NewProjectile(null, Player.Center, velocity,
						ProjectileType<SakiNoteIdle>(), Item.damage, Item.knockBack, Player.whoAmI);
				}
			}
			if (skills == 3) {
				Player.GetAttackSpeed(DamageClass.Melee) += 0.40f;
			}
		}

		public int useTimes = 0;
		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			if (!ArknightsKeybinds.SkillActivatePressed(player)) {
				useTimes++;
				if (useTimes > 8) {
					useTimes = 1;
				}
				swordDir++;
				if (swordDir > 2) {
					swordDir = 1;
				}
				if (skills == 0) {
					Projectile.NewProjectile(source, position, velocity,
						ProjectileType<SakiNoteCommon>(), damage, knockback, player.whoAmI);
					Projectile.NewProjectile(source, position, velocity,
					ProjectileType<SakiSwordWhite>(), damage, knockback, player.whoAmI, swordDir == 1 ? 1 : -1);
				}
				else if (skills == 1) {
					if (useTimes % 4 == 0) {
						for (int i = -4; i < 4; i++) {
							float angle = i / (MathHelper.PiOver4 * 10);
							int p = Projectile.NewProjectile(source, position + new Vector2(20, 0).RotatedBy(angle), velocity.RotatedBy(angle / 3),
							ProjectileType<SakiNote1>(), (int)(damage * (0.6f + (i + 4) * 0.05f)), knockback, player.whoAmI);
							Main.projectile[p].scale = 0.6f + (i + 4) * 0.1f;
							var note = (SakiNote1)Main.projectile[p].ModProjectile;
							int count = (int)(40 * (0.44f + (i + 4) * 0.07f));
							note.oldpos = new Vector2[count];
							note.oldrot = new float[count];
						}
					}
					else {
						Projectile.NewProjectile(source, position, velocity,
							ProjectileType<SakiNoteCommon>(), damage, knockback, player.whoAmI);
					}
					Projectile.NewProjectile(source, position, velocity,
					ProjectileType<SakiSwordWhite>(), damage, knockback, player.whoAmI, swordDir == 1 ? 1 : -1);
				}
				else if (skills == 2) {
					damage = (int)(damage * 1.1f);
					int p = Projectile.NewProjectile(source, position, velocity * 1.3f,
							ProjectileType<SakiNote2Dark>(), damage, knockback, player.whoAmI);
					Projectile.NewProjectile(source, position, velocity,
					ProjectileType<SakiSwordBlack>(), damage, knockback, player.whoAmI, swordDir == 1 ? 1 : -1);
					(Main.projectile[p].ModProjectile as SakiNote2Dark).maxSpeed = 8.2f;
				}
				else if (skills == 3) {
					Projectile.NewProjectile(source, position, velocity * 0.6f,
							ProjectileType<SakiNote2Bright>(), damage, knockback, player.whoAmI);
					int p = Projectile.NewProjectile(source, position, velocity,
					ProjectileType<SakiSwordWhite>(), damage, knockback, player.whoAmI, swordDir == 1 ? 1 : -1);
					var sword = Main.projectile[p].ModProjectile as SakiSwordWhite;
					sword.outroSpeed = 45;
				}
				else if (skills == 4) {
					damage = (int)(damage * 2.2f);
					int p1 = Projectile.NewProjectile(source, position, velocity * 0.6f,
							ProjectileType<SakiNote2Bright>(), damage, knockback, player.whoAmI);
					(Main.projectile[p1].ModProjectile as SakiNote2Bright).collideMax = 1;

					int p2 = Projectile.NewProjectile(source, position, velocity * 0.6f,
							ProjectileType<SakiNote2Bright>(), damage, knockback, player.whoAmI);
					(Main.projectile[p2].ModProjectile as SakiNote2Bright).collideMax = 1;

					int p3 = Projectile.NewProjectile(source, position, velocity * 0.6f,
							ProjectileType<SakiNote2Dark>(), damage, knockback, player.whoAmI);
					(Main.projectile[p3].ModProjectile as SakiNote2Dark).collideMax = 1;

					int p4 = Projectile.NewProjectile(source, position, velocity * 0.6f,
							ProjectileType<SakiNote2Dark>(), damage, knockback, player.whoAmI);
					(Main.projectile[p4].ModProjectile as SakiNote2Dark).collideMax = 1;


					Projectile.NewProjectile(source, position, velocity,
					ProjectileType<SakiSwordWhite>(), damage, knockback, player.whoAmI, swordDir == 1 ? 1 : -1);
					Projectile.NewProjectile(source, position, velocity,
					ProjectileType<SakiSwordBlack>(), damage, knockback, player.whoAmI, swordDir == 1 ? -1 : 1);
				}
				return false;
			}
			return true;
		}
	}
}