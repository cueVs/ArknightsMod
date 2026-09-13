using ArknightsMod.Content.Items.Material;
using ArknightsMod.Content.Projectiles.Defender.Durnar;
using ArknightsMod.Content.Tiles.Infrastructure;
using ArknightsMod.Players;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content;

namespace ArknightsMod.Content.Items.Weapons.Defender.Durnar
{
    public class DN_Weapon : UpgradeWeaponBase
    {
        public override void AddRecipes()
		{
			CreateRecipe()
				.AddIngredient(ModContent.ItemType<OrirockCluster>(), 19)
				.AddIngredient(ModContent.ItemType<RMA7012>(), 3)
				.AddTile(ModContent.TileType<FactoryTile>())
				.Register();
		}
		private static SoundStyle SkillActive3;
		private static SoundStyle NoSound;
		public override void Load()
		{
			SkillActive3 = new SoundStyle("ArknightsMod/Sounds/SkillActive3")
			{
				Volume = 0.4f,
				MaxInstances = 4,
			};
			NoSound = new SoundStyle("ArknightsMod/Sounds/NoSound")
			{
				Volume = 0f,
				MaxInstances = 4,
			};
		}

		public override void SetDefaults()
		{
			Item.damage = 23; // �����˺�
			Item.knockBack = 7;
			Item.crit = 2; // ������
			Item.DamageType = DamageClass.Melee; // �˺�����
			Item.width = 48; // ��Ʒ����
			Item.height = 60; // ��Ʒ�߶�
			Item.useTime = 25; // ʹ��ʱ��
			Item.useAnimation = 25; // ʹ�ö���ʱ��
			Item.autoReuse = true; // �Զ�ʹ��
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.useStyle = ItemUseStyleID.HiddenAnimation;
		}
        public override bool AltFunctionUse(Player player) => false;
		public override bool CanUseItem(Player player)
		{
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (Main.myPlayer == player.whoAmI)
			{
				if (ArknightsKeybinds.SkillActivatePressed(player))
				{
					if (!modPlayer.SummonMode&&modPlayer.StockCount > 0 )
					{
						// S1
						if (modPlayer.Skill == 0 && !modPlayer.SkillActive)
						{
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;

							modPlayer.DelStockCount();

							SoundEngine.PlaySound(SkillActive3, player.Center);
						}
						else if(modPlayer.Skill == 1 && !modPlayer.SkillActive)
						{
							modPlayer.SkillActive = true;
							modPlayer.SkillTimer = 0;
							var DN_player = player.GetModPlayer<DNProj_Player>();
							DN_player.ShieldAttackMode = true;
							modPlayer.DelStockCount();

							SoundEngine.PlaySound(SkillActive3, player.Center);
						}
						return false;
					}
				}
			}
			return base.CanUseItem(player);
		}

		public override void HoldItem(Player player) => base.HoldItem(player);
		public override void ModifyWeaponDamage(Player player, ref StatModifier damage)
		{
			var modPlayer = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if(modPlayer.SkillActive)
				damage *= 1.8f;
		}
    }
}