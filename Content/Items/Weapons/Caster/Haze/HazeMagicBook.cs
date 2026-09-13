using ArknightsMod.Content.Projectiles.Caster.Haze;
using ArknightsMod.Content.Items.Material;
using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Players;
using ArknightsMod.Content;

using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;


namespace ArknightsMod.Content.Items.Weapons.Caster.Haze
{
	public class HazeMagicBook : UpgradeWeaponBase
	{
		private float Skill = 0;
		private static int BaseUseTime ; 
		private static SoundStyle SkillActiveSound;

		public override void Load()
        {
            SkillActiveSound = new SoundStyle("ArknightsMod/Sounds/SkillActive1")
            {
                Volume = 0.4f,
                MaxInstances = 4,
            };
        }

		public override void SetDefaults() {
			Item.damage = 50;
			Item.mana = 6;
			Item.knockBack = 3;
			Item.useAnimation = 48;
			Item.useTime = 48;
			Item.shootSpeed = 8;
			BaseUseTime = Item.useTime;

			Item.shoot = ModContent.ProjectileType<Haze_Magicball>();
			Item.DamageType = DamageClass.Magic;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.UseSound = SoundID.Item9;
			Item.rare = ItemRarityID.LightPurple;
			//Item.value = Item.sellPrice(silver: 5);

			Item.noMelee = true;
		}
    
		public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient<OrirockCluster>(5);
            recipe.AddIngredient<RMA7024>(3);
            recipe.AddTile(ModContent.TileType<FactoryTile>());
            recipe.Register();
        }

        public override bool AltFunctionUse(Player player) => false;

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            var modPlayer = player.GetModPlayer<WeaponPlayer>();
            if (modPlayer.Skill == 0 && modPlayer.SkillActive)
            {
                Projectile.NewProjectile(
                    source, position, velocity,
                    ModContent.ProjectileType<Haze_Magicball>(),
                    damage, knockback, player.whoAmI,
                    0f, 0f, 2f);
                return false;
            }
            else if (modPlayer.Skill == 1 && modPlayer.SkillActive)
            {
                Projectile.NewProjectile(
                    source, position, velocity,
                    ModContent.ProjectileType<Haze_Crimsonball>(),
                    damage, knockback, player.whoAmI,
                    0f, 0f, 2f);
                return false;
            }
			else{
				Projectile.NewProjectile(
					source, position, velocity,
					ModContent.ProjectileType<Haze_Magicball>(),
					damage, knockback, player.whoAmI,
					0f, 0f, 0f);
				return false;
			}
            
		}

		public override bool CanUseItem(Player player)
        {
            if (Main.myPlayer != player.whoAmI)
                return base.CanUseItem(player);

            var modPlayer = player.GetModPlayer<WeaponPlayer>();

            Item.useTime = BaseUseTime;
            Item.useAnimation = BaseUseTime;
            Item.UseSound = SoundID.Item9;

            if (ArknightsKeybinds.SkillActivatePressed(player))
            {
                if (modPlayer.Skill == 0 && modPlayer.StockCount > 0 && !modPlayer.SkillActive)
                {
                    modPlayer.SkillActive = true;
                    modPlayer.SkillTimer = 0;
                    modPlayer.DelStockCount();
                    SoundEngine.PlaySound(SkillActiveSound, player.Center);
                    return false;
                }
                if (modPlayer.Skill == 1 && modPlayer.StockCount > 0 && !modPlayer.SkillActive)
                {
                    modPlayer.SkillActive = true;
                    modPlayer.SkillTimer = 0;
                    modPlayer.DelStockCount();
                    SoundEngine.PlaySound(SkillActiveSound, player.Center);
                    return false;
                }
                return false;
            }

            if (modPlayer.Skill == 1 && modPlayer.SkillActive)
            {
                Item.useTime = 30;
                Item.useAnimation = Item.useTime;
            }

            return base.CanUseItem(player);
        }

        public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
        {
            if (Main.myPlayer != player.whoAmI) return;
            var modPlayer = player.GetModPlayer<WeaponPlayer>();
            if (modPlayer.Skill == 0 && modPlayer.SkillActive) damage *= 1.8f;
            if (modPlayer.Skill == 1 && modPlayer.SkillActive) damage *= 1.6f;
        }
	}

    public class HazeState : ModPlayer
    {
        private bool _wasS2Active;
        private bool _pendingHeal;

        public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
        {
            health = StatModifier.Default;
            mana = StatModifier.Default;
            if (Player.HeldItem.ModItem is not HazeMagicBook) return;
            var wp = Player.GetModPlayer<WeaponPlayer>();
            if (wp.Skill == 1 && wp.SkillActive)
                health *= 0.25f;
        }

        public override void PostUpdate()
        {
            bool nowActive = false;
            if (Player.HeldItem.ModItem is HazeMagicBook)
            {
                var wp = Player.GetModPlayer<WeaponPlayer>();
                nowActive = wp.Skill == 1 && wp.SkillActive;
            }

            // 上一帧打的标，本帧执行
            if (_pendingHeal)
            {
                int newLife = Player.statLife * 4;
                Player.statLife = newLife > Player.statLifeMax2 ? Player.statLifeMax2 : newLife;
                _pendingHeal = false;
            }

            // 下降沿仅打标，不立即 heal, 否则在技能结束时无法恢复正常比例的血量
            if (_wasS2Active && !nowActive)
                _pendingHeal = true;

            _wasS2Active = nowActive;
        }
    }
}
