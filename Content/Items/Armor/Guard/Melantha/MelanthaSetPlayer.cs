using ArknightsMod.Content.Buffs.Guard.Melantha;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ArknightsMod.Content.Items.Armor.Guard.Melantha
{
	internal class MelanthaSetPlayer : ArknightsArmorPlayer
	{
		public bool MelanthaHelmetActive;
		public bool MelanthaSetActive;

		public int FearlessStacks;
		public int FearlessDecayTimer;

		public override void ResetEffects() {
			MelanthaHelmetActive = false;
			MelanthaSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 MelanthaHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			MelanthaHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<MelanthaHead>();
			MelanthaSetActive = MelanthaHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<MelanthaBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<MelanthaLegs>();
		}

		public override void PostUpdate() {
			if (FearlessDecayTimer > 0) {
				FearlessDecayTimer--;
				if (FearlessDecayTimer <= 0)
					FearlessStacks = 0;
			}
		}

		public override void UpdateLifeRegen() {
			if (FearlessStacks > 0)
				Player.lifeRegen += 3 * FearlessStacks;
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (!MelanthaHelmetActive)
				return;

			if (item.DamageType.CountsAsClass(DamageClass.Melee))
				damage *= 1.04f;
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			TryAddFearlessStack(item, target, damageDone);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			TryAddFearlessStack(proj, target, damageDone);
		}

		private void TryAddFearlessStack(Item item, NPC target, int damageDone) {
			if (!MelanthaSetActive || damageDone <= 0)
				return;

			if (!item.DamageType.CountsAsClass(DamageClass.Melee))
				return;

			if (target.friendly || target.lifeMax <= 5 || target.dontTakeDamage || target.immortal)
				return;

			AddFearlessStack();
		}

		private void TryAddFearlessStack(Projectile proj, NPC target, int damageDone) {
			if (!MelanthaSetActive || damageDone <= 0)
				return;

			if (!proj.DamageType.CountsAsClass(DamageClass.Melee))
				return;

			if (target.friendly || target.lifeMax <= 5 || target.dontTakeDamage || target.immortal)
				return;

			AddFearlessStack();
		}

		private void AddFearlessStack() {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			FearlessStacks++;
			FearlessDecayTimer = 4 * 60;
			Player.AddBuff(BuffType<FearlessDebuff>(), FearlessDecayTimer);
		}
	}
}
