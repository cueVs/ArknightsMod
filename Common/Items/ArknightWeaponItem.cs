using ArknightsMod.Common.Players;
using ArknightsMod.Content.Items.Weapons;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Common.Items
{
	public class ArknightWeaponItem : GlobalItem
	{
		public override void HoldItem(Item item, Player player) {
			if (player.whoAmI != Main.myPlayer || item.ModItem is not ArknightsWeapon)
				return;
			var mp = player.GetModPlayer<WeaponPlayer>();
			var skill = mp.CurrentSkill;
			mp.TryAutoCharge();
			if (skill.AutoUpdateActive)
				mp.UpdateActiveSkill();

		}
	}
}
