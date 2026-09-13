namespace ArknightsMod.Content.Items.BattleRecords
{
	internal class StrategicBattleRecord : BattleRecordBase
	{
		public override int Experience => 2000;

		public override void SetDefaults() {
			base.SetDefaults();
			Item.value = Terraria.Item.sellPrice(0, 0, 10, 0);
		}
	}
}