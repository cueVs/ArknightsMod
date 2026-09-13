using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Midnight
{
	internal class MidnightSetPlayer : ArknightsArmorPlayer
	{
		public bool MidnightHelmetActive;
		public bool MidnightSetActive;

		public override void ResetEffects() {
			MidnightHelmetActive = false;
			MidnightSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 MidnightHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			MidnightHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<MidnightHead>();
			MidnightSetActive = MidnightHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<MidnightBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<MidnightLegs>();

			if (MidnightHelmetActive)
				Player.statDefense -= 5;
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (MidnightHelmetActive && item.DamageType.CountsAsClass(DamageClass.Melee))
				damage *= 1.15f;
		}

		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
			if (MidnightSetActive && item.DamageType.CountsAsClass(DamageClass.Melee) && Main.rand.NextFloat() < 0.1f)
				modifiers.SourceDamage *= 1.5f;
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			if (MidnightSetActive && proj.DamageType.CountsAsClass(DamageClass.Melee) && Main.rand.NextFloat() < 0.1f)
				modifiers.SourceDamage *= 1.5f;
		}
	}
}
