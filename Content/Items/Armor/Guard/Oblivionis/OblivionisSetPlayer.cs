using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Systems.Gameplay.Damage;
using ArknightsMod.Systems.Gameplay.OperatorTags;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Oblivionis
{
	internal class OblivionisSetPlayer : ArknightsArmorPlayer
	{
		public bool OblivionisHelmetActive;
		public bool OblivionisSetActive;
		public int Fever;

		public override void ResetEffects() {
			OblivionisHelmetActive = false;
			OblivionisSetActive = false;
		}

		// NeoArmor Reforge：套装件是独立 ItemID，穿上它本身就代表"已经是套装形态"，
		// 不需要再查 hasUpgraded；player.setBonus 文本交给 OblivionisHead 的
		// SetProfile.SetBonusKey 统一设置，这里不再重复设置一遍。
		public override void PostUpdateEquips() {
			OblivionisHelmetActive = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<OblivionisHead>();
			OblivionisSetActive = OblivionisHelmetActive
				&& Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<OblivionisBody>()
				&& Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<OblivionisLegs>();
		}

		public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers) {
			ApplyHelmetPenetration(modifiers);
			TryAddFeverOnHit();
			TryReduceArtsResistance(target);
		}

		public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers) {
			ApplyHelmetPenetration(modifiers);
			TryAddFeverOnHit();
			TryReduceArtsResistance(target);
		}

		private void ApplyHelmetPenetration(NPC.HitModifiers modifiers) {
			if (!OblivionisHelmetActive || !OperatorTagHelper.PlayerHasFaction(Player, OperatorFaction.AveMujica))
				return;

			int noteLayers = System.Math.Min(10, OperatorTagHelper.CountNotesOnField(Player.whoAmI));
			if (noteLayers <= 0)
				return;

			modifiers.ScalingArmorPenetration += 0.03f * noteLayers;
		}

		private void TryReduceArtsResistance(NPC target) {
			if (!OblivionisHelmetActive || !OperatorTagHelper.PlayerHasFaction(Player, OperatorFaction.AveMujica))
				return;

			int noteLayers = System.Math.Min(10, OperatorTagHelper.CountNotesOnField(Player.whoAmI));
			if (noteLayers <= 0)
				return;

			DamageCategoryNPC cat = target.GetGlobalNPC<DamageCategoryNPC>();
			cat.artsResistance = System.Math.Max(0f, cat.artsResistance - 0.02f * noteLayers);
		}

		private void TryAddFeverOnHit() {
			if (!OblivionisSetActive || Main.netMode == NetmodeID.MultiplayerClient)
				return;

			Fever += 3;
		}

		public static bool IsAnyOblivionisSetActive() {
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player player = Main.player[i];
				if (player.active && !player.dead && player.GetModPlayer<OblivionisSetPlayer>().OblivionisSetActive)
					return true;
			}

			return false;
		}
	}

	internal class OblivionisAllyAttackSpeedPlayer : ModPlayer
	{
		public override float UseSpeedMultiplier(Item item) {
			return OblivionisSetPlayer.IsAnyOblivionisSetActive() ? 1.15f : 1f;
		}
	}
}
