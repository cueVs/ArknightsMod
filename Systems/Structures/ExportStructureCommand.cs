using System;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace ArknightsMod.Systems.Structures
{
	// 聊天指令 /exp：把「开发测试扳手」当前定格的选区导出成 .akstruct 文件。
	// CommandType.Chat —— 只在客户端本地聊天栏识别，不会被当成发给服务器的消息，
	// 单人/联机都能用，且不会被其他玩家看到你打了这行字。
	public class ExportStructureCommand : ModCommand
	{
		public override string Command => "exp";
		public override CommandType Type => CommandType.Chat;
		public override string Usage => "/exp";
		public override string Description => "把开发测试扳手当前定格的选区导出为 .akstruct 结构文件";

		public override void Action(CommandCaller caller, string input, string[] args) {
			var wrenchPlayer = caller.Player.GetModPlayer<DevWrenchPlayer>();

			if (!wrenchPlayer.HasSelection) {
				caller.Reply("没有可导出的选区：先手持开发测试扳手，按住左键拖出一个框选范围。", Color.OrangeRed);
				return;
			}

			var (start, end) = wrenchPlayer.FinalizedSelection.Value;

			try {
				string dir = AkStructureFormat.GetDefaultDirectory();
				string fileName = $"structure_{DateTime.Now:yyyyMMdd_HHmmss}{AkStructureFormat.FileExtension}";
				string path = Path.Combine(dir, fileName);

				AkStructureFormat.Save(start, end, path);

				int width = Math.Abs(end.X - start.X) + 1;
				int height = Math.Abs(end.Y - start.Y) + 1;
				caller.Reply($"已导出 {width}x{height} 的结构：{path}", Color.LightGreen);
			}
			catch (Exception ex) {
				// 导出是开发者本地操作，直接把异常信息回显给聊天栏即可，
				// 不需要额外包装成"友好提示"——看到具体报错反而更方便排查。
				caller.Reply($"导出失败：{ex.Message}", Color.OrangeRed);
			}
		}
	}
}
