using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs
{
	public class BagpipeSpearS1 : ModBuff
	{
		public override void SetStaticDefaults() {
			//DisplayName.SetDefault("Swift Strike Gamma");
			//Description.SetDefault("ATK +45%; ASPD +45");
			Main.buffNoTimeDisplay[Type] = true;
			// Main.debuff[Type] = false; // default: false
			// BuffID.Sets.NurseCannotRemoveDebuff[Type] = true; // if you set debuff[Type] as true, this line's default is false.
		}

		public override void Update(Player player, ref int buffIndex) {

		}
	}
}
