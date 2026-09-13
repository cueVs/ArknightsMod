using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Defender.Saria
{
	internal class SariaSetPlayer : ArknightsArmorPlayer
	{
		public bool SariaHelmetActive;
		public bool SariaSetActive;

		public int GuardStacks;
		private int stackTimer;

		public override void ResetEffects() {
			SariaHelmetActive = false;
			SariaSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 SariaHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			SariaHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<SariaHead>();
			SariaSetActive = SariaHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<SariaBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<SariaLegs>();

			if (SariaSetActive && GuardStacks > 0) {
				Player.GetDamage(DamageClass.Generic) += 0.05f * GuardStacks;
				extraDefenseBonus += 0.04f * GuardStacks;
			}
		}

		public override void PostUpdate() {
			if (!SariaSetActive) {
				GuardStacks = 0;
				stackTimer = 0;
				return;
			}

			if (!OperatorSetBossHelper.AnyBossActive()) {
				GuardStacks = 0;
				stackTimer = 0;
				return;
			}

			stackTimer++;
			if (stackTimer >= 10 * 60 && GuardStacks < 5) {
				GuardStacks++;
				stackTimer = 0;
			}
		}

		public override void UpdateDead() {
			GuardStacks = 0;
			stackTimer = 0;
		}
	}
}
