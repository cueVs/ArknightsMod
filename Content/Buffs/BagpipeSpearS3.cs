using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System;

namespace ArknightsMod.Content.Buffs
{
	public class BagpipeSpearS3 : ModBuff
	{
		public override void SetStaticDefaults() {
			//DisplayName.SetDefault("Locked Breech Burst");
			//Description.SetDefault("ATK Interval increases, Block +1, ATK and DEF +120%, and attacks hit 3 times in a row");
			Main.buffNoTimeDisplay[Type] = true;
			// Main.debuff[Type] = false; // default: false
			// BuffID.Sets.NurseCannotRemoveDebuff[Type] = true; // if you set debuff[Type] as true, this line's default is false.
		}

		public override void Update(Player player, ref int buffIndex) {
			var modPlayer = Main.LocalPlayer.GetModPlayer<Common.Players.WeaponPlayer>();
			if (modPlayer.SkillActive) {
				player.statDefense = (int)Math.Round(player.statDefense * 2.2);
			}
			else {
				player.DelBuff(buffIndex);
				buffIndex--;
			}
		}
	}
}
