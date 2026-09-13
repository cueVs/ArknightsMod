using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Systems.Commands
{
	// 调试指令 /plant：立即让玩家周围符合条件的地表格子长出已注册的自然植物（目前只有血蕈），
	// 不用干等被动生长的低概率判定，方便测试。
	public class PlantCommand : ModCommand
	{
		private const int Radius = 30;

		public override string Command => "plant";
		public override CommandType Type => CommandType.Chat;
		public override string Usage => "/plant";
		public override string Description => "让周围符合条件的地表格子立刻长出自然植物（调试用）";

		public override void Action(CommandCaller caller, string input, string[] args) {
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				caller.Reply("/plant 只能在单人游戏或服务器端使用，联机客户端调用会导致不同步。", Color.OrangeRed);
				return;
			}

			int planted = NaturalGrowthSystem.GrowAllNear(caller.Player, Radius);
			caller.Reply(planted > 0 ? $"已生长 {planted} 株植物。" : "周围没有找到符合条件的草皮/苔藓。", Color.LightGreen);
		}
	}
}
