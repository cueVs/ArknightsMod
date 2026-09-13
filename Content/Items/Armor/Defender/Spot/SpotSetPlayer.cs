using ArknightsMod.Content.Buffs.ArmorSets;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Defender.Spot
{
	internal class SpotSetPlayer : ArknightsArmorPlayer
	{
		public bool SpotHelmetActive;
		public bool SpotSetActive;

		public override void ResetEffects() {
			SpotHelmetActive = false;
			SpotSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 SpotHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			SpotHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<SpotHead>();
			SpotSetActive = SpotHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<SpotBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<SpotLegs>();
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (!Player.HasBuff<SpotHealDodgeBuff>())
				return;

			if (Main.rand.NextFloat() < 0.17f)
				modifiers.Cancel();
		}
	}
}
