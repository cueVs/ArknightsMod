using ArknightsMod.Content.Projectiles;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Weapons
{
	public class ThornsSword : UpgradeWeaponBase
	{
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Yato's Katana");
			// Tooltip.SetDefault("Yato has joined the team.");
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.damage = 70;
			Item.knockBack = 3.5f;
			Item.useStyle = ItemUseStyleID.Swing; // Makes the player do the proper arm motion
			Item.useAnimation = 40;
			Item.useTime = 40;
			Item.width = 60;
			Item.height = 60;
			Item.DamageType = DamageClass.MeleeNoSpeed;
			Item.channel = true; //Channel so that you can held the weapon [Important]
			Item.crit = 0; // The percent chance at hitting an enemy with a crit, plus the default amount of 4.
			Item.autoReuse = true;

			Item.rare = ItemRarityID.Yellow;
			Item.value = Item.sellPrice(0, 0, 4, 0);

			Item.shoot = ModContent.ProjectileType<ThornsSwordProjectile>(); // The projectile is what makes a shortsword work
			Item.shootSpeed = 20f; // This value bleeds into the behavior of the projectile as velocity, keep that in mind when tweaking values

			// The sound that this item plays when used. Need "using Terraria.Audio;"
			Item.UseSound = new SoundStyle("ArknightsMod/Sounds/ThornsSwordS0") {
				Volume = 0.4f,
				MaxInstances = 3, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
			};
		}

		//　OnHitNPC is used in case of the effects when the hit happening
		public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone) {
			target.AddBuff(BuffID.Poisoned, 180);
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<Material.OrirockConcentration>(4)
				.AddIngredient<Material.KetonColloid>(4)
				.AddIngredient<Material.OrironBlock>(6)
				.AddTile(TileID.Anvils)
				.Register();
		}

		protected override int GetDamage(int level) => Item.OriginalDamage + (level - 1) * 20;

		public override void ModifyWeaponDamage(Player player, ref StatModifier damage) {
			base.ModifyWeaponDamage(player, ref damage);
			damage.Base += (Level - 1) * 20;
		}

		//public override void AddRecipes() {
		//	CreateRecipe()
		//		.AddIngredient(ItemID.DirtBlock, 1)
		//		.AddTile(TileID.WorkBenches)
		//		.Register();
		//}
	}
}