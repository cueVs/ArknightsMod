using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Provence
{
	internal class ProvenceSetPlayer : ArknightsArmorPlayer
	{
		public bool ProvenceHelmetActive;
		public bool ProvenceSetActive;

		private const float CloseRange = 300f;

		public override void ResetEffects() {
			ProvenceHelmetActive = false;
			ProvenceSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 ProvenceHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			ProvenceHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<ProvenceHead>();
			ProvenceSetActive = ProvenceHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<ProvenceBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<ProvenceLegs>();
		}

		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
			TryCloseRangeCrit(item, target, ref modifiers);
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			TryCloseRangeCrit(proj, target, ref modifiers);
		}

		private void TryCloseRangeCrit(Item item, NPC target, ref NPC.HitModifiers modifiers) {
			if (!ProvenceHelmetActive || !item.DamageType.CountsAsClass(DamageClass.Ranged))
				return;

			if (Vector2.Distance(Player.Center, target.Center) > CloseRange)
				return;

			float chance = ProvenceSetActive ? 0.5f : 0.2f;
			if (Main.rand.NextFloat() < chance)
				modifiers.SourceDamage *= 1.8f;
		}

		private void TryCloseRangeCrit(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			if (!ProvenceHelmetActive || !proj.DamageType.CountsAsClass(DamageClass.Ranged))
				return;

			if (Vector2.Distance(Player.Center, target.Center) > CloseRange)
				return;

			float chance = ProvenceSetActive ? 0.5f : 0.2f;
			if (Main.rand.NextFloat() < chance)
				modifiers.SourceDamage *= 1.8f;
		}
	}
}
