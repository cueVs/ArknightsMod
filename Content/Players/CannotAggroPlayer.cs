using ArknightsMod.Systems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Content.Players
{
	public class CannotAggroPlayer : ModPlayer
	{
		public int CannotLifeAck = -1;

		public bool CanDamageCannotForCurrentLife() {
			return CannotLifeAck == ModContent.GetInstance<CannotLifeGateSystem>().LifeToken;
		}

		public void AcknowledgeCannotTouchGoodsDialogue() {
			var sys = ModContent.GetInstance<CannotLifeGateSystem>();
			CannotLifeAck = sys.LifeToken;
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				var packet = Mod.GetPacket();
				packet.Write((short)ArknightsMod.ArkMessageID.CannotAggroAck);
				packet.Send();
			}
		}

		public static void ServerApplyAck(int playerWhoAmI) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			Player plr = Main.player[playerWhoAmI];
			if (!plr.active || !plr.TryGetModPlayer(out CannotAggroPlayer aggro))
				return;
			aggro.CannotLifeAck = ModContent.GetInstance<CannotLifeGateSystem>().LifeToken;
		}

		public override void SaveData(TagCompound tag) {
			tag["CannotLifeAck"] = CannotLifeAck;
		}

		public override void LoadData(TagCompound tag) {
			CannotLifeAck = tag.ContainsKey("CannotLifeAck") ? tag.GetInt("CannotLifeAck") : -1;
		}
	}
}
