using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace ArknightsMod.Content.Buffs
{
	public class BagpipeSpearS2 : ModBuff
	{
		public override void SetStaticDefaults() {
			//DisplayName.SetDefault("High-Impact Assault");
			//Description.SetDefault("Increases the ATK of the next attack to 200%, and strikes an additional time");
			Main.buffNoTimeDisplay[Type] = true;
			// Main.debuff[Type] = false; // default: false
			// BuffID.Sets.NurseCannotRemoveDebuff[Type] = true; // if you set debuff[Type] as true, this line's default is false.
		}

		public override void Update(Player player, ref int buffIndex) {
			
		}
	}
}
