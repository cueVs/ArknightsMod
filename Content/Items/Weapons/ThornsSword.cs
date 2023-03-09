using ArknightsMod.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.Audio;

namespace ArknightsMod.Content.Items.Weapons
{
	public class ThornsSword : ModItem
	{

		public override void SetStaticDefaults()
		{
			// DisplayName.SetDefault("Yato's Katana");
			// Tooltip.SetDefault("Yato has joined the team.");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.damage = 70;
			Item.knockBack = 6.5f;
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
			Item.UseSound = new SoundStyle("ArknightsMod/Sounds/ThornsSword")
			{
				Volume = 0.2f,
				MaxInstances = 3, //This dicatates how many instances of a sound can be playing at the same time. The default is 1. Adjust this to allow overlapping sounds.
			};
		}
		public override void ModifyHitNPC(Player player, NPC target, ref int damage, ref float knockBack, ref bool crit)
		{
			target.AddBuff(BuffID.Poisoned, 180);
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Material.OrirockConcentration>(4);
			recipe.AddIngredient<Material.KetonColloid>(4);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}
}