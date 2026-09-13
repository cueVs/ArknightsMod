using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Players;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Developer
{
	// 本地调试开关：设为 true 后才能在游戏中生成/保留该饰品（默认关闭，无正常获取途径）
	internal static class OriginiumCoreDevAccess
	{
		internal const bool Enabled = false;
	}

	public class OriginiumCore : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 0;
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 30;
			Item.accessory = true;
			Item.rare = ItemRarityID.Purple;
			Item.value = 0;
		}

		public override void OnCreated(ItemCreationContext context)
		{
			if (OriginiumCoreDevAccess.Enabled)
				return;
			if (context is InitializationItemCreationContext) // 加载阶段不能销毁模板物品
				return;
			Item.TurnToAir();
		}

		public override bool CanResearch() => false;

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetModPlayer<OriginiumCorePlayer>().Equipped = true;
		}
	}

	public class OriginiumCorePlayer : ModPlayer
	{
		public bool Equipped;

		public override void ResetEffects()
		{
			Equipped = false;
		}

		public override void PostUpdate()
		{
			if (!Equipped || Player.whoAmI != Main.myPlayer || Main.gameMenu || Player.dead)
				return;

			bool fPressed = Main.keyState.IsKeyDown(Keys.F) && !Main.oldKeyState.IsKeyDown(Keys.F);
			if (!fPressed || Player.HeldItem.ModItem is not UpgradeWeaponBase)
				return;

			Player.GetModPlayer<WeaponPlayer>().DevFillSkillCharge();
		}
	}
}
