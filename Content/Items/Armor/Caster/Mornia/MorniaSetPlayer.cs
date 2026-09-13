using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Armor;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Caster.Mornia
{
	// NeoArmor Reforge：头盔/套装效果不再靠 UpdateEquip 里的一堆 if(hasUpgraded) 判断，
	// 而是 MorniaHead 的 SetProfile.OnHelmetActive / OnFullSetActive 在实际装备时直接调用
	// 下面那两个静态方法——这个类只负责"记住这一帧是否生效"和真正的玩法效果，
	// 检测逻辑完全交给 NeoArmorReforgeSetPiece。
	internal class MorniaSetPlayer : ModPlayer
	{
		public bool HelmetActive;
		public bool SetActive;

		public override void ResetEffects() {
			HelmetActive = false;
			SetActive = false;
		}

		public static void OnHelmetActive(Player player) {
			player.GetModPlayer<MorniaSetPlayer>().HelmetActive = true;
		}

		// 套装效果前半段「+8% 魔法暴击」就地生效。
		// OnFullSetActive 每帧都会被调用（见 NeoArmorReforgeSetPiece.UpdateEquip），这里
		// 直接 += 不会累积——暴击率每帧会被原版重算回基础值，和原版盔甲的写法一致。
		public static void OnFullSetActive(Player player) {
			player.GetModPlayer<MorniaSetPlayer>().SetActive = true;
			player.GetCritChance(DamageClass.Magic) += 8f;
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (HelmetActive && item.DamageType.CountsAsClass(DamageClass.Magic))
				damage *= 1.06f;
		}

		// 套装效果后半段「奥术弹幕自动追踪最近的敌人」实现在
		// Common/GlobalProjectiles/MorniaArcaneHomingGlobalProj.cs——追踪要每帧改弹幕自己的
		// velocity，挂在弹幕上比在这里遍历 Main.projectile 更直接。它读的就是上面的 SetActive。
	}
}
