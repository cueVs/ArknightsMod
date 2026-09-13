using Terraria.ID;

namespace ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom
{
	// 同 ReceptionRoomDecorAnchorTableTile 的说明，这个是"算作椅子"的变体
	// （办公椅/办公躺椅对应的锚点会换成这个）。
	public class ReceptionRoomDecorAnchorChairTile : ReceptionRoomDecorAnchorTile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsChair);
		}
	}
}
