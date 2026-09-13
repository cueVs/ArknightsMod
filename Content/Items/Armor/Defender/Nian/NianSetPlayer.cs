using System;
using ArknightsMod.Content.Buffs.ArmorSets;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Defender.Nian
{
	internal class NianSetPlayer : ArknightsArmorPlayer
	{
		public bool NianHelmetActive;
		public bool NianSetActive;

		public int ShieldLayers;
		private bool spawnShieldGranted;
		private bool lastBossActive;

		public override void ResetEffects() {
			NianHelmetActive = false;
			NianSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 NianHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			NianHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<NianHead>();
			NianSetActive = NianHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<NianBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<NianLegs>();
		}

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			health = StatModifier.Default;
			mana = StatModifier.Default;

			if (ShouldReceiveNianLifeBonus(Player))
				health *= 1.16f;
		}

		public static bool ShouldReceiveNianLifeBonus(Player player) {
			if (!OperatorDefenderSetHelper.WearsFullDefenderSet(player))
				return false;

			for (int i = 0; i < Main.maxPlayers; i++) {
				Player other = Main.player[i];
				if (!other.active || other.dead)
					continue;

				NianSetPlayer nian = other.GetModPlayer<NianSetPlayer>();
				if (nian.NianHelmetActive && OperatorTeammateHelper.HasTeammates(other))
					return true;
			}

			return false;
		}

		public override void PostUpdate() {
			if (NianSetActive) {
				bool bossActive = OperatorSetBossHelper.AnyBossActive();
				if (!spawnShieldGranted || (bossActive && !lastBossActive)) {
					ShieldLayers = Math.Min(3, ShieldLayers + 3);
					spawnShieldGranted = true;
				}

				lastBossActive = bossActive;

				if (ShieldLayers > 0)
					Player.AddBuff(ModContent.BuffType<NianShieldBuff>(), 2);
			}

			if (Player.dead) {
				spawnShieldGranted = false;
				lastBossActive = false;
				ShieldLayers = 0;
			}
		}

		public override void OnRespawn() {
			if (NianSetActive) {
				ShieldLayers = Math.Min(3, ShieldLayers + 3);
				spawnShieldGranted = true;
			}
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (!NianSetActive || ShieldLayers <= 0)
				return;

			modifiers.ModifyHurtInfo += (ref Player.HurtInfo info) => {
				if (info.Damage <= 50 || ShieldLayers <= 0)
					return;

				info.Damage = (int)(info.Damage * 0.3f);
				ShieldLayers--;
			};
		}
	}
}
