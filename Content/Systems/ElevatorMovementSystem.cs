using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content.Tiles;

namespace ArknightsMod.Content.Systems
{
	// 兜底：若 WorldGen 未调用 TE.Update，仍在本帧推进电梯移动逻辑。
	public class ElevatorMovementSystem : ModSystem
	{
		public override void PostUpdateWorld()
		{
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			foreach (var kv in TileEntity.ByID)
			{
				if (kv.Value is not TEElevator te)
					continue;
				if (te.TargetFloorBottomY >= 0 || te.IsMoving)
					te.ProcessSimulation();
			}
		}
	}
}
