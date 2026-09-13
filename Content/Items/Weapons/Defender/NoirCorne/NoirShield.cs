using ArknightsMod.Players;
using ArknightsMod.Content.Projectiles.Defender.NoirCorne;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;


namespace ArknightsMod.Content.Items.Weapons.Defender.NoirCorne
{    public class NoirShield : UpgradeWeaponBase
    {

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IronBar, 5);
            recipe.AddIngredient(ItemID.LeadBar, 5);
            recipe.AddTile(TileID.WorkBenches);
            recipe.Register();
        }
        private static SoundStyle NoSound;
        public override void Load()
        {
            NoSound = new SoundStyle("ArknightsMod/Sounds/NoSound")
            {
                Volume = 0f,
                MaxInstances = 4,
            };
        }
        public override void SetDefaults()
        {
            Item.damage = 12;
            Item.knockBack = 12;
            Item.crit = 2;
            Item.DamageType = DamageClass.Melee;
            Item.width = 78;
            Item.height = 102;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.rare = ItemRarityID.White;
			Item.value = Item.sellPrice(0, 0, 4, 0);
            // Item.shootSpeed = 1f;
            // Item.shoot = ModContent.ProjectileType<NoirShield_Projectile>();
        }

        public override bool CanUseItem(Player player) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI) {
				if (player.altFunctionUse == 2) {
					if (!modPlayer.SummonMode) {
						player.GetModPlayer<NoirDEFplayer>().hasNoirDEFplayer = true;
						return false;
					}
				}
				else {
					if (!modPlayer.SummonMode) {
                        if (player.ownedProjectileCounts[ModContent.ProjectileType<NoirShield_Projectile>()] > 0)
                            return false;
						Item.UseSound = NoSound;
						SoundEngine.PlaySound(Item.UseSound.Value, player.Center);
					}
				}
			}
			return base.CanUseItem(player);
		}

        public class NoirDEFplayer : ModPlayer
		{
			public bool hasNoirDEFplayer = false;
			public override void ResetEffects() {
				if (Main.myPlayer != Player.whoAmI)
					return; 
				bool isHoldingTargetWeapon = Player.HeldItem.type == ModContent.ItemType<NoirShield>();
				if (!isHoldingTargetWeapon) {
					Player.GetModPlayer<NoirDEFplayer>().hasNoirDEFplayer = false;
				}

			}

            public override void PostUpdate() {
                var it = Player.HeldItem;
                if (it.type == ModContent.ItemType<NoirShield>()) {
                    if (Player.ownedProjectileCounts[ModContent.ProjectileType<NoirShield_Projectile>()] == 0) {
                        Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<NoirShield_Projectile>(), it.damage, it.knockBack, Player.whoAmI);
                    }
                }

                base.PostUpdate();
            }

            public override void UpdateEquips() {
                var it = Player.HeldItem;
                if (it.type == ModContent.ItemType<NoirShield>() ) {
                    Player.statDefense += 5;
                    if (Player.controlUseTile) Player.statDefense += 10;

                }
                base.UpdateEquips();
            }
		}
    }
}