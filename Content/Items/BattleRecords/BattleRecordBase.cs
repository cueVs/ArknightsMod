using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.BattleRecords
{
	internal abstract class BattleRecordBase : ModItem
	{
		public abstract int Experience { get; }

		public override void SetDefaults() {
			base.SetDefaults();
			Item.width = 38;
			Item.height = 28;
			Item.maxStack = 9999;
		}
	}
}