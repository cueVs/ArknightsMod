using ArknightsMod.Common.UI.BattleRecord;
using ArknightsMod.Content.Items;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Common.Players
{
	internal class UpgradePlayer : ModPlayer
	{
		private UpgradeItemBase upgradeItem;

		public override void SaveData(TagCompound tag) {
			base.SaveData(tag);
			if (UpgradeUIState.Instance.UpgradeItem != null) {
				tag["UpgradeUIItem"] = ItemIO.Save(UpgradeUIState.Instance.UpgradeItem.Item);
				UpgradeUIState.Instance.UpgradeItem = null;
			}
		}

		public override void LoadData(TagCompound tag) {
			base.LoadData(tag);
			if (tag.TryGet("UpgradeUIItem", out TagCompound tc))
				upgradeItem = ItemIO.Load(tc).ModItem as UpgradeItemBase;
			else
				upgradeItem = null;
		}

		public override void OnEnterWorld() {
			base.OnEnterWorld();
			UpgradeUIState.Instance.UpgradeItem = upgradeItem;
		}
	}
}