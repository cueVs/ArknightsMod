using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.W
{
	internal class WSetPlayer : ArknightsArmorPlayer
	{
		public bool WHelmetActive;
		public bool WSetActive;

		public override void ResetEffects() {
			WHelmetActive = false;
			WSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 WHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			WHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<WHead>();
			WSetActive = WHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<WBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<WLegs>();
		}

		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
			if (WHelmetActive && OperatorStunNPC.HasStun(target) && IsPhysicalItem(item))
				modifiers.SourceDamage *= 1.18f;

			if (WSetActive && OperatorStunNPC.HasStun(target) && item.DamageType.CountsAsClass(DamageClass.Ranged))
				modifiers.SourceDamage *= 1.3f;
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			if (WHelmetActive && OperatorStunNPC.HasStun(target) && IsPhysicalProjectile(proj))
				modifiers.SourceDamage *= 1.18f;

			if (WSetActive && OperatorStunNPC.HasStun(target) && proj.DamageType.CountsAsClass(DamageClass.Ranged))
				modifiers.SourceDamage *= 1.3f;
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (!WSetActive)
				return;

			if (Main.rand.NextFloat() < 0.17f)
				modifiers.Cancel();
		}

		public override void PostUpdate() {
			if (WSetActive)
				Player.aggro -= 750;
		}

		private static bool IsPhysicalItem(Item item) {
			return item.DamageType.CountsAsClass(DamageClass.Melee)
				|| item.DamageType.CountsAsClass(DamageClass.Ranged)
				|| item.DamageType.CountsAsClass(DamageClass.Summon);
		}

		private static bool IsPhysicalProjectile(Projectile projectile) {
			return projectile.DamageType.CountsAsClass(DamageClass.Melee)
				|| projectile.DamageType.CountsAsClass(DamageClass.Ranged)
				|| projectile.DamageType.CountsAsClass(DamageClass.Summon);
		}
	}
}
