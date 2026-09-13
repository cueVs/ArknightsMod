namespace ArknightsMod.Content.Items.BattleRecords
{
	internal class TacticalBattleRecord : BattleRecordBase
	{
		public override int Experience => 1000;

		public override void SetDefaults() {
			base.SetDefaults();
			Item.value = Terraria.Item.sellPrice(0, 0, 5, 0);
		}
	}
}