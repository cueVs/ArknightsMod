using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Vanguard.Bagpipe
{
	internal class BagpipeSetPlayer : ArknightsArmorPlayer
	{
		public bool BagpipeHelmetActive;
		public bool BagpipeSetActive;

		private bool spawnSpGranted;

		public override void ResetEffects() {
			BagpipeHelmetActive = false;
			BagpipeSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 BagpipeHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			BagpipeHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<BagpipeHead>();
			BagpipeSetActive = BagpipeHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<BagpipeBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<BagpipeLegs>();
		}

		public override void PostUpdate() {
			if (BagpipeHelmetActive && !Player.dead && !spawnSpGranted) {
				GrantVanguardSp();
				spawnSpGranted = true;
			}

			if (Player.dead)
				spawnSpGranted = false;
		}

		public override void OnRespawn() {
			if (BagpipeHelmetActive)
				GrantVanguardSp();
		}

		private static void GrantVanguardSp() {
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];
				if (!player.active || player.dead)
					continue;

				if (OperatorVanguardSetHelper.WearsFullVanguardSet(player))
					OperatorSPHelper.TryGainSP(player, 8);
			}
		}

		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
			if (BagpipeSetActive && item.DamageType.CountsAsClass(DamageClass.Melee) && Main.rand.NextFloat() < 0.2f)
				modifiers.SourceDamage *= 1.5f;
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			if (BagpipeSetActive && proj.DamageType.CountsAsClass(DamageClass.Melee) && Main.rand.NextFloat() < 0.2f)
				modifiers.SourceDamage *= 1.5f;
		}
	}
}
