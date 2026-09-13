using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Caster.Lava
{
	internal class LavaSetPlayer : ArknightsArmorPlayer
	{
		public bool LavaHelmetActive;
		public bool LavaSetActive;
		private bool spawnSpGranted;

		public override void ResetEffects() {
			LavaHelmetActive = false;
			LavaSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 LavaHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			LavaHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<LavaHead>();
			LavaSetActive = LavaHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<LavaBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<LavaLegs>();
		}

		public override void PostUpdate() {
			if (LavaSetActive && Player.GetModPlayer<WeaponPlayer>().SkillActive)
				Player.AddBuff(BuffID.Inferno, 2);

			if (!Player.dead && LavaHelmetActive && !spawnSpGranted) {
				OperatorSPHelper.TryGainSP(Player, 15);
				spawnSpGranted = true;
			}

			if (Player.dead)
				spawnSpGranted = false;
		}

		public override void OnRespawn() {
			if (LavaHelmetActive)
				OperatorSPHelper.TryGainSP(Player, 15);
		}
	}
}
