namespace ArknightsMod.Content.Items.BattleRecords
{
	internal class DrillBattleRecord : BattleRecordBase
	{
		public override int Experience => 200;

		public override void SetDefaults() {
			base.SetDefaults();
			Item.value = Terraria.Item.sellPrice(0, 0, 1, 0);
		}
	}
}