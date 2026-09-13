using System;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Systems.Gameplay.OperatorTags;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Ulpianus
{
	internal class UlpianusSetPlayer : ArknightsArmorPlayer
	{
		public bool UlpianusHelmetActive;
		public bool UlpianusSetActive;
		public int KillStacks;

		private int lifeBonus;
		private float attackBonus;

		public override void ResetEffects() {
			UlpianusHelmetActive = false;
			UlpianusSetActive = false;
			lifeBonus = 0;
			attackBonus = 0f;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 UlpianusHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			UlpianusHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<UlpianusHead>();
			UlpianusSetActive = UlpianusHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<UlpianusBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<UlpianusLegs>();

			if (UlpianusHelmetActive) {
				lifeBonus = KillStacks * 12;
				attackBonus = KillStacks * 4f;
			}
			else if (OperatorTagHelper.PlayerHasFaction(Player, OperatorFaction.AbyssalHunter)
				&& OperatorTagHelper.AnyPlayerWithHelmet<UlpianusHead>(out Player ulpianus)) {
				int stacks = ulpianus.GetModPlayer<UlpianusSetPlayer>().KillStacks;
				lifeBonus = (int)(stacks * 12 * 0.5f);
				attackBonus = stacks * 4f * 0.5f;
			}
		}

		public override void ModifyMaxStats(out StatModifier health, out StatModifier mana) {
			health = StatModifier.Default;
			mana = StatModifier.Default;
			if (lifeBonus > 0)
				health.Base += lifeBonus;
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (attackBonus > 0f)
				damage.Flat += attackBonus;
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			TryAddKillStack(target);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			TryAddKillStack(target);
		}

		private void TryAddKillStack(NPC target) {
			if (!UlpianusHelmetActive || KillStacks >= 9 || Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (target.life > 0 || target.friendly || target.lifeMax <= 5)
				return;

			KillStacks++;
		}

		public override void ModifyHurt(ref Player.HurtModifiers modifiers) {
			if (!UlpianusSetActive)
				return;

			modifiers.ModifyHurtInfo += (ref Player.HurtInfo info) => {
				if (info.Damage <= 0)
					return;

				float lifeRatio = Player.statLife / (float)Math.Max(1, Player.statLifeMax2);
				int heal = lifeRatio < 0.5f ? 16 : 12;
				if (HoldsSignatureAnchor())
					heal *= 2;

				Player.Heal(heal);
				Player.HealEffect(heal, false);
			};
		}

		private static bool HoldsSignatureAnchor() {
			// 专武「沉向深渊的锚」尚未实装
			return false;
		}
	}
}
