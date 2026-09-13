using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Buffs.ArmorSets;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Caster.Indigo
{
	internal class IndigoSetPlayer : ArknightsArmorPlayer
	{
		public bool IndigoHelmetActive;
		public bool IndigoSetActive;

		public override void ResetEffects() {
			IndigoHelmetActive = false;
			IndigoSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 IndigoHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			IndigoHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<IndigoHead>();
			IndigoSetActive = IndigoHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<IndigoBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<IndigoLegs>();
		}

		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
			if (IndigoHelmetActive && item.DamageType.CountsAsClass(DamageClass.Magic) && HasBindOrStun(target))
				modifiers.SourceDamage *= 1.3f;

			if (IndigoSetActive && item.DamageType.CountsAsClass(DamageClass.Magic) && Main.rand.NextFloat() < 0.18f)
				TryBind(target);
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			if (IndigoHelmetActive && proj.DamageType.CountsAsClass(DamageClass.Magic) && HasBindOrStun(target))
				modifiers.SourceDamage *= 1.3f;

			if (IndigoSetActive && proj.DamageType.CountsAsClass(DamageClass.Magic) && Main.rand.NextFloat() < 0.18f)
				TryBind(target);
		}

		private static bool HasBindOrStun(NPC target) {
			return target.HasBuff(ModContent.BuffType<IndigoBindDebuff>()) || OperatorStunNPC.HasStun(target);
		}

		private static void TryBind(NPC target) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (target.friendly || target.lifeMax <= 5 || target.dontTakeDamage)
				return;

			target.AddBuff(ModContent.BuffType<IndigoBindDebuff>(), 4 * 60);
		}
	}
}
