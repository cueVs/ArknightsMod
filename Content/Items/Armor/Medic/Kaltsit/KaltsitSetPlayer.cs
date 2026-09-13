using ArknightsMod.Content.Buffs.Medic.Kaltsit;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Projectiles.Medic.Kaltsit;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Medic.Kaltsit
{
	internal class KaltsitSetPlayer : ArknightsArmorPlayer
	{
		public bool KaltsitHelmetActive;
		public bool KaltsitSetActive;

		public override void ResetEffects() {
			KaltsitHelmetActive = false;
			KaltsitSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 KaltsitHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		//
		// 套装效果——召唤 M3(Mon3tr)：AddBuff 只管 UI 图标的显示/续期，真正"没有就补生成"
		// 的逻辑在 Mon3tr.EnsureFor 里（原因见 Mon3trBuff 顶部注释：不能指望 ModBuff.Update
		// 自己生成实体）。M3 的消失走 Mon3tr.AI() 里的 HasBuff<Mon3trBuff> 检查，不在这里处理。
		public override void PostUpdateEquips() {
			KaltsitHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<KaltsitHead>();
			KaltsitSetActive = KaltsitHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<KaltsitBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<KaltsitLegs>();

			if (KaltsitSetActive) {
				Player.AddBuff(ModContent.BuffType<Mon3trBuff>(), 2);
				Mon3tr.EnsureFor(Player);
			}
		}
	}
}
