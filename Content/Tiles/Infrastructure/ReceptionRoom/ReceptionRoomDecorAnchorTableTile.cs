using Terraria.ID;

namespace ArknightsMod.Content.Tiles.Infrastructure.ReceptionRoom
{
	// ReceptionRoomDecorAnchorTile 的"算作桌子"变体——接待室装饰家具系统里，
	// 真正摆出来的贴图都是自绘的（DrawAllDecor），世界里落地的只有这个不可见的
	// 锚点 tile，原版住房检测只认 tile 类型，没法按格子里具体摆了什么家具动态
	// 判断，所以只能把"算桌子"的家具（电脑桌/办公桌/花瓶桌）对应的锚点换成
	// 这个专门标了 CountsAsTable 的子类，其余逻辑完全照抄父类。
	public class ReceptionRoomDecorAnchorTableTile : ReceptionRoomDecorAnchorTile
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();
			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);
		}
	}
}
