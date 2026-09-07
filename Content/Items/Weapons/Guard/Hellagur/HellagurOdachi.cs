using System;
using ArknightsMod.Content;
using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Content.Projectiles.BasePROJ;
using ArknightsMod.Content.Projectiles.Guard.Hellagur;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons.Guard.Hellagur
{
	/// <summary>赫拉格的大太刀：低血提速、母刀吸血，并以月相区分三种技能。</summary>
	public class HellagurOdachi : ExpansionWeaponBase
	{
		// 原面板 152 降低 40%，四舍五入为 91；三个精英档位保持统一基准。
		protected override int[] EliteDamage => [91, 91, 91];

		private static SoundStyle SkillActiveSfx;

		// 三张素材均已在导入时旋转 180°；只启用第 3 张，第 1、2 张保留备用。
		internal const string WeaponTexturePath = "ArknightsMod/Content/Items/Weapons/Guard/Hellagur/HellagurOdachiVariant3";
		public override string Texture => WeaponTexturePath;

		public override void Load() {
			SkillActiveSfx = new SoundStyle("ArknightsMod/Sounds/SkillActive1") {
				Volume = 0.52f,
				MaxInstances = 2
			};
		}

		public override void Unload() => SkillActiveSfx = default;

		public override void SetDefaults() {
			Item.damage = EliteDamage[0];
			Item.DamageType = DamageClass.Melee;
			Item.width = 58;
			Item.height = 60;
			// 真正的攻击间隔由手持弹幕动作长度控制；这里压低以免物品动画成为连斩瓶颈。
			Item.useTime = 2;
			Item.useAnimation = 2;
			Item.knockBack = 6.5f;
			Item.value = Item.sellPrice(silver: 55);
			Item.rare = ItemRarityID.LightRed;
			Item.autoReuse = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.noMelee = true;
			Item.noUseGraphic = true;
			Item.shoot = ModContent.ProjectileType<HellagurOdachiSwing>();
			Item.shootSpeed = 1f;
			Item.crit = 6;
		}

		public override bool AltFunctionUse(Player player) => false;

		public override bool CanUseItem(Player player) {
			if (ArknightsKeybinds.SkillActivatePressed(player)) {
				if (HellagurSkillBridge.TryActivateSelected(player) && !Main.dedServ)
					SoundEngine.PlaySound(SkillActiveSfx, player.Center);
				return false;
			}

			return player.ownedProjectileCounts[Item.shoot] < 1 && base.CanUseItem(player);
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source,
			Vector2 position, Vector2 velocity, int type, int damage, float knockback) {
			int skillMode = HellagurSkillBridge.GetAttackMode(player);
			int action = skillMode switch {
				HellagurSkillBridge.Skill1 => HellagurOdachiSwing.ActionSkill1,
				HellagurSkillBridge.Skill2 => BaseHeldMeleeSupport.NextCombo(player, 2),
				HellagurSkillBridge.Skill3 => HellagurOdachiSwing.ActionSkill3,
				_ => BaseHeldMeleeSupport.NextCombo(player, 3)
			};

			HellagurCombatPlayer combatPlayer = player.GetModPlayer<HellagurCombatPlayer>();
			float skillDamageScale = skillMode switch {
				HellagurSkillBridge.Skill1 => 1.75f,
				HellagurSkillBridge.Skill2 => 1.8f,
				HellagurSkillBridge.Skill3 => 2f,
				_ => 1f
			};
			int projectileDamage = Math.Max(1, (int)Math.Round(damage * skillDamageScale));

			Vector2 aim = velocity.SafeNormalize(new Vector2(player.direction, 0f));
			float lowHealthSpeed = HellagurSkillBridge.GetLowHealthActionSpeed(player);

			int projectileIndex = Projectile.NewProjectile(source, player.MountedCenter, aim,
				ModContent.ProjectileType<HellagurOdachiSwing>(), projectileDamage, knockback,
				player.whoAmI, action, skillMode, lowHealthSpeed);
			if (projectileIndex < 0 || projectileIndex >= Main.maxProjectiles)
				return false;

			combatPlayer.MarkSwingStarted();
			if (skillMode == HellagurSkillBridge.Skill1)
				combatPlayer.ConsumeSkill1();

			if (!Main.dedServ) {
				SoundStyle swingSound = skillMode == HellagurSkillBridge.Normal
					? SoundID.Item1 with { Volume = 0.38f, Pitch = -0.24f }
					: SoundID.Item71 with { Volume = 0.48f, Pitch = skillMode == 3 ? -0.38f : -0.2f };
				SoundEngine.PlaySound(swingSound, player.Center);
				if (skillMode == HellagurSkillBridge.Skill1)
					SoundEngine.PlaySound(SkillActiveSfx with { Volume = 0.38f }, player.Center);
			}
			return false;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.BrokenHeroSword)
				.AddIngredient(ItemID.ChlorophyteBar, 18)
				.AddIngredient(ItemID.SoulofNight, 12)
				.AddTile(TileID.MythrilAnvil)
				.Register();
		}

	}
}
