using ArknightsMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Common.GlobalNPCs
{
	// 霜晶树的被动效果：持有者满血击杀任意敌对NPC加估价，跟红雾特效无关，普通的僵尸史莱姆都算。
	public class FrostCrystalTreeGlobalNPC : GlobalNPC
	{
		public override void OnKill(NPC npc) {
			if (npc.friendly) return;

			int killer = npc.lastInteraction;
			if (killer < 0 || killer >= Main.maxPlayers) return;

			Player player = Main.player[killer];
			if (!player.active) return;

			var frostPlayer = player.GetModPlayer<FrostCrystalTreePlayer>();
			if (!frostPlayer.HasFrostCrystalTree || frostPlayer.Broken) return;
			if (player.statLife != player.statLifeMax2) return;

			frostPlayer.RegisterKill(npc.boss);
		}
	}
}
