using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Systems.Gameplay.Skill
{
	public class ArknightWeaponItem : GlobalItem
	{
		public override void HoldItem(Item item, Player player) {
			if (player.whoAmI != Main.myPlayer || item.ModItem is not UpgradeWeaponBase)
				return;
			var mp = player.GetModPlayer<WeaponPlayer>();
			var skill = mp.CurrentSkill;

			if (skill == null) {
				//Main.NewText($"[{GetType()}] 错误: 当前技能数据mp.CurrentSkill为null", Color.Red);
				return;
			}

			mp.TryAutoCharge();
			if (skill.AutoUpdateActive)
				mp.UpdateActiveSkill();

		}
	}
}
