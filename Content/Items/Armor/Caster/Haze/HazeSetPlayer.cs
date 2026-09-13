using ArknightsMod.Content.Buffs.ArmorSets;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Caster.Haze
{
	internal class HazeSetPlayer : ArknightsArmorPlayer
	{
		public bool HazeHelmetActive;
		public bool HazeSetActive;

		public override void ResetEffects() {
			HazeHelmetActive = false;
			HazeSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 HazeHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			HazeHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<HazeHead>();
			HazeSetActive = HazeHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<HazeBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<HazeLegs>();

			if (HazeSetActive) {
				int critBonus = !Main.dayTime ? 12 : 6;
				Player.GetCritChance(DamageClass.Magic) += critBonus;
			}
		}

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			health = StatModifier.Default;
			mana = StatModifier.Default;

			if (HazeSetActive)
				mana.Base += !Main.dayTime ? 100 : 50;
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			TryApplyFragile(item, target, damageDone);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			TryApplyFragile(proj, target, damageDone);
		}

		private void TryApplyFragile(Item item, NPC target, int damageDone) {
			if (!HazeHelmetActive || damageDone <= 0 || !item.DamageType.CountsAsClass(DamageClass.Magic))
				return;

			ApplyFragile(target);
		}

		private void TryApplyFragile(Projectile proj, NPC target, int damageDone) {
			if (!HazeHelmetActive || damageDone <= 0 || !proj.DamageType.CountsAsClass(DamageClass.Magic))
				return;

			ApplyFragile(target);
		}

		private static void ApplyFragile(NPC target) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (target.friendly || target.lifeMax <= 5 || target.dontTakeDamage)
				return;

			target.AddBuff(ModContent.BuffType<HazeMagicFragileDebuff>(), 60);
		}
	}
}
