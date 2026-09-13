using System;
using ArknightsMod.Content.Buffs.ArmorSets;
using ArknightsMod.Content.Items.Armor.Defender.Saria;
using ArknightsMod.Content.Items.Armor.Defender.Spot;
using ArknightsMod.Content.Items.Armor.Guard.Matoimaru;
using ArknightsMod.Content.Items.Armor.Medic.Ansel;
using ArknightsMod.Content.Items.Armor.Medic.Hibiscus;
using ArknightsMod.Content.Items.Armor.Supporter.CivilightEterna;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor
{
	// 技能治疗友方时由各技能实现处调用。
	internal static class OperatorSkillHealHelper
	{
		public static int ApplyHeal(Player target, int baseAmount, Player healer = null) {
			if (baseAmount <= 0)
				return 0;

			float multiplier = 1f;
			MatoimaruSetPlayer matoi = target.GetModPlayer<MatoimaruSetPlayer>();
			if (matoi.MatoimaruHelmetActive)
				multiplier = 1.25f;

			if (healer != null) {
				SpotSetPlayer spot = healer.GetModPlayer<SpotSetPlayer>();
				if (spot.SpotHelmetActive)
					multiplier *= 1.04f;

				AnselSetPlayer ansel = healer.GetModPlayer<AnselSetPlayer>();
				if (ansel.AnselHelmetActive)
					multiplier *= 1.04f;

				HibiscusSetPlayer hibiscus = healer.GetModPlayer<HibiscusSetPlayer>();
				if (hibiscus.HibiscusHelmetActive)
					multiplier *= 1.04f;
			}

			int finalAmount = (int)(baseAmount * multiplier);
			target.Heal(finalAmount);

			CivilightEternaSetPlayer civilight = healer?.GetModPlayer<CivilightEternaSetPlayer>();
			if (civilight != null && civilight.CivilightEternaSetActive
				&& target.HasBuff(ModContent.BuffType<CivilightEternaHealBoostBuff>())) {
				int bonus = (int)(finalAmount * 0.5f);
				if (bonus > 0) {
					target.Heal(bonus);
					target.HealEffect(bonus);
					finalAmount += bonus;
				}
			}

			if (healer != null) {
				SariaSetPlayer saria = healer.GetModPlayer<SariaSetPlayer>();
				if (saria.SariaHelmetActive)
					OperatorSPHelper.TryGainSP(target, 2);

				SpotSetPlayer spotSet = healer.GetModPlayer<SpotSetPlayer>();
				if (spotSet.SpotSetActive && healer.whoAmI != target.whoAmI)
					target.AddBuff(ModContent.BuffType<SpotHealDodgeBuff>(), SpotHealDodgeBuff.DurationTicks);

				AnselSetPlayer anselSet = healer.GetModPlayer<AnselSetPlayer>();
				if (anselSet.AnselSetActive && healer.whoAmI != target.whoAmI)
					TryAnselChainHeal(healer, target, finalAmount);
			}

			return finalAmount;
		}

		private static void TryAnselChainHeal(Player healer, Player primaryTarget, int healAmount) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (Main.rand.NextFloat() >= 0.07f)
				return;

			Player chainTarget = FindRandomNearbyAlly(healer, primaryTarget);
			if (chainTarget == null)
				return;

			chainTarget.Heal(Math.Max(1, healAmount / 2));
			chainTarget.HealEffect(Math.Max(1, healAmount / 2));
		}

		private static Player FindRandomNearbyAlly(Player healer, Player exclude) {
			const float maxRange = 800f;
			Player chosen = null;
			int count = 0;

			for (int i = 0; i < Main.maxPlayers; i++) {
				Player other = Main.player[i];
				if (!other.active || other.dead || other.whoAmI == healer.whoAmI || other.whoAmI == exclude.whoAmI)
					continue;

				if (Vector2.Distance(healer.Center, other.Center) > maxRange)
					continue;

				count++;
				if (Main.rand.Next(count) == 0)
					chosen = other;
			}

			return chosen;
		}
	}
}
