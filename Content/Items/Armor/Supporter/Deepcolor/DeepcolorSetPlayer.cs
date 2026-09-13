using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Supporter.Deepcolor
{
	internal class DeepcolorSetPlayer : ArknightsArmorPlayer
	{
		public bool DeepcolorHelmetActive;
		public bool DeepcolorSetActive;

		public override void ResetEffects() {
			DeepcolorHelmetActive = false;
			DeepcolorSetActive = false;
		}

		// 头盔标记的入口。旧代码是 DeepcolorHead.UpdateArmorEquip 直接写这个字段，
		// 新系统由 DeepcolorHead 的 SetProfile.OnHelmetActive 调这里（文档第 6 节写法 B）。
		public static void OnHelmetActive(Player player) {
			player.GetModPlayer<DeepcolorSetPlayer>().DeepcolorHelmetActive = true;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 DeepcolorHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			DeepcolorSetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<DeepcolorHead>()
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<DeepcolorBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<DeepcolorLegs>();
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (!DeepcolorHelmetActive)
				return;

			if (Main.rand.NextFloat() < 0.07f)
				modifiers.Cancel();
		}
	}
}
