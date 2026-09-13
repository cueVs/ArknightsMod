using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Systems
{
	public class CannotLifeGateSystem : ModSystem
	{
		public int LifeToken { get; private set; }

		public void OnCannotDied() {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			LifeToken++;
			if (Main.netMode != NetmodeID.SinglePlayer) {
				var packet = Mod.GetPacket();
				packet.Write((short)ArknightsMod.ArkMessageID.CannotLifeTokenSync);
				packet.Write(LifeToken);
				packet.Send();
			}
		}

		public void ApplyNetworkLifeToken(int token) {
			LifeToken = token;
		}

		public override void SaveWorldData(TagCompound tag) {
			tag["CannotLifeToken"] = LifeToken;
		}

		public override void LoadWorldData(TagCompound tag) {
			LifeToken = tag.ContainsKey("CannotLifeToken") ? tag.GetInt("CannotLifeToken") : 0;
		}
	}
}
