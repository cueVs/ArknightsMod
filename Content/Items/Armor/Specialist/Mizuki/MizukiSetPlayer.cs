using System.Collections.Generic;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Specialist.Mizuki
{
	internal class MizukiSetPlayer : ArknightsArmorPlayer
	{
		public bool MizukiHelmetActive;
		public bool MizukiSetActive;

		private readonly List<int> swingHits = [];
		private int lastItemAnimation;

		public override void ResetEffects() {
			MizukiHelmetActive = false;
			MizukiSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 MizukiHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			MizukiHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<MizukiHead>();
			MizukiSetActive = MizukiHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<MizukiBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<MizukiLegs>();
		}

		public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
			if (MizukiHelmetActive && HasLowHpEnemyNearby())
				damage *= 1.2f;
		}

		public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) {
			TrackSwingHit(target, item, damageDone);
		}

		public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) {
			TrackSwingHit(target, proj, damageDone);
		}

		public override void PostUpdate() {
			if (Player.itemAnimation > 0 && lastItemAnimation == 0)
				swingHits.Clear();

			if (lastItemAnimation > 0 && Player.itemAnimation == 0)
				ApplyLowestHpBonus();

			lastItemAnimation = Player.itemAnimation;
		}

		private bool HasLowHpEnemyNearby() {
			for (int i = 0; i < Main.maxNPCs; i++) {
				NPC npc = Main.npc[i];
				if (!npc.active || npc.friendly || npc.lifeMax <= 5 || npc.dontTakeDamage)
					continue;

				if (npc.Distance(Player.Center) > 900f)
					continue;

				if (npc.life < npc.lifeMax * 0.5f)
					return true;
			}

			return false;
		}

		private void TrackSwingHit(NPC target, Item item, int damageDone) {
			if (!MizukiSetActive || damageDone <= 0)
				return;

			if (!swingHits.Contains(target.whoAmI))
				swingHits.Add(target.whoAmI);
		}

		private void TrackSwingHit(NPC target, Projectile proj, int damageDone) {
			if (!MizukiSetActive || damageDone <= 0)
				return;

			if (!swingHits.Contains(target.whoAmI))
				swingHits.Add(target.whoAmI);
		}

		private void ApplyLowestHpBonus() {
			if (!MizukiSetActive || swingHits.Count == 0 || Main.netMode == NetmodeID.MultiplayerClient)
				return;

			NPC lowest = null;
			for (int i = 0; i < swingHits.Count; i++) {
				int id = swingHits[i];
				if (id < 0 || id >= Main.maxNPCs)
					continue;

				NPC npc = Main.npc[id];
				if (!npc.active)
					continue;

				if (lowest == null || npc.life < lowest.life)
					lowest = npc;
			}

			if (lowest == null)
				return;

			int bonus = (int)(Player.GetTotalDamage(DamageClass.Generic).ApplyTo(1f) * 0.5f);
			if (bonus > 0)
				lowest.SimpleStrikeNPC(bonus, 0, false, 0, DamageClass.Magic);

			swingHits.Clear();
		}
	}
}
