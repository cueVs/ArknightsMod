namespace ArknightsMod.Content.Items.BattleRecords
{
	internal class FrontlineBattleRecord : BattleRecordBase
	{
		public override int Experience => 400;

		public override void SetDefaults() {
			base.SetDefaults();
			Item.value = Terraria.Item.sellPrice(0, 0, 2, 0);
		}
	}
}