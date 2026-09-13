using System.IO;
using ArknightsMod.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Content.Tiles.Natural
{
	// 记录“种下去的这一株恋家果”当前的估价：种下时从物品的当前估价起步，
	// 之后每过一个游戏日（NaturalGrowthSystem.GameDayCounter 变化）恢复2点，封顶32；采下来时把这个值带进新掉落的物品里。
	public class HomesickFruitTileEntity : ModTileEntity
	{
		public const int MaxValue = 32;

		public int Value = MaxValue;
		private int lastDayCounter = -1;

		public override bool IsTileValidForEntity(int x, int y) {
			Tile tile = Main.tile[x, y];
			return tile.HasTile && tile.TileType == ModContent.TileType<HomesickFruit>();
		}

		public override void Update() {
			if (Main.netMode == NetmodeID.MultiplayerClient) return;

			int currentDay = NaturalGrowthSystem.GameDayCounter;
			if (lastDayCounter < 0) {
				lastDayCounter = currentDay;
				return;
			}
			if (currentDay != lastDayCounter) {
				lastDayCounter = currentDay;
				Value = System.Math.Min(MaxValue, Value + 2);
			}
		}

		public override void SaveData(TagCompound tag) {
			tag["value"] = Value;
			tag["lastDay"] = lastDayCounter;
		}

		public override void LoadData(TagCompound tag) {
			Value = tag.GetInt("value");
			lastDayCounter = tag.GetInt("lastDay");
		}

		public override void NetSend(BinaryWriter writer) {
			writer.Write(Value);
		}

		public override void NetReceive(BinaryReader reader) {
			Value = reader.ReadInt32();
		}
	}
}
