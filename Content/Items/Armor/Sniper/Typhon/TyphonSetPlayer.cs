using System.Collections.Generic;
using ArknightsMod.Content.Buffs.ArmorSets;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Sniper.Typhon
{
	internal class TyphonSetPlayer : ArknightsArmorPlayer
	{
		public bool TyphonHelmetActive;
		public bool TyphonSetActive;

		private readonly HashSet<int> skillFirstHitTargets = new();
		private bool wasSkillActive;
		private int defIgnoreStacks;
		private int rangedIdleTimer;

		public override void ResetEffects() {
			TyphonHelmetActive = false;
			TyphonSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 TyphonHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			TyphonHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<TyphonHead>();
			TyphonSetActive = TyphonHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<TyphonBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<TyphonLegs>();
		}

		public override void PostUpdate() {
			bool skillActive = Player.GetModPlayer<WeaponPlayer>().SkillActive;
			if (!skillActive && wasSkillActive)
				skillFirstHitTargets.Clear();

			wasSkillActive = skillActive;

			if (TyphonSetActive) {
				rangedIdleTimer++;
				if (rangedIdleTimer >= 8 * 60) {
					rangedIdleTimer = 8 * 60;
					defIgnoreStacks = 0;
				}
			}
		}

		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
			ApplyHelmetFirstHit(item, target, ref modifiers);
			ApplySetDefIgnore(item, ref modifiers);
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			ApplyHelmetFirstHit(proj, target, ref modifiers);
			ApplySetDefIgnore(proj, ref modifiers);
		}

		private void ApplyHelmetFirstHit(Item item, NPC target, ref NPC.HitModifiers modifiers) {
			if (!TyphonHelmetActive || !Player.GetModPlayer<WeaponPlayer>().SkillActive)
				return;

			if (!item.DamageType.CountsAsClass(DamageClass.Ranged))
				return;

			if (skillFirstHitTargets.Contains(target.whoAmI))
				return;

			skillFirstHitTargets.Add(target.whoAmI);
			modifiers.SourceDamage *= 1.6f;

			if (Main.netMode != NetmodeID.MultiplayerClient)
				target.AddBuff(ModContent.BuffType<TyphonHelmetSlowDebuff>(), 3 * 60);
		}

		private void ApplyHelmetFirstHit(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			if (!TyphonHelmetActive || !Player.GetModPlayer<WeaponPlayer>().SkillActive)
				return;

			if (!proj.DamageType.CountsAsClass(DamageClass.Ranged))
				return;

			if (skillFirstHitTargets.Contains(target.whoAmI))
				return;

			skillFirstHitTargets.Add(target.whoAmI);
			modifiers.SourceDamage *= 1.6f;

			if (Main.netMode != NetmodeID.MultiplayerClient)
				target.AddBuff(ModContent.BuffType<TyphonHelmetSlowDebuff>(), 3 * 60);
		}

		private void ApplySetDefIgnore(Item item, ref NPC.HitModifiers modifiers) {
			if (!TyphonSetActive || !item.DamageType.CountsAsClass(DamageClass.Ranged))
				return;

			rangedIdleTimer = 0;
			modifiers.ScalingArmorPenetration += 0.1f * defIgnoreStacks;
			defIgnoreStacks = System.Math.Min(5, defIgnoreStacks + 1);
		}

		private void ApplySetDefIgnore(Projectile proj, ref NPC.HitModifiers modifiers) {
			if (!TyphonSetActive || !proj.DamageType.CountsAsClass(DamageClass.Ranged))
				return;

			rangedIdleTimer = 0;
			modifiers.ScalingArmorPenetration += 0.1f * defIgnoreStacks;
			defIgnoreStacks = System.Math.Min(5, defIgnoreStacks + 1);
		}
	}
}
