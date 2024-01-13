using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using System.Collections.Generic;
using System.Security.Principal;

namespace ArknightsMod.Content.Buffs
{
	public class AcidOgSlugDebuff : ModBuff
	{
		public override void SetStaticDefaults() {
			//DisplayName.SetDefault("Swift Strike Gamma");
			//Description.SetDefault("ATK +45%; ASPD +45");
			Main.buffNoTimeDisplay[Type] = false;
			Main.buffNoSave[Type] = true;
			Main.debuff[Type] = true; // default: false
			BuffID.Sets.NurseCannotRemoveDebuff[Type] = false; // if you set debuff[Type] as true, this line's default is false.
		}

		public override void Update(Player player, ref int buffIndex) {
			player.statDefense -= player.GetModPlayer<AcidOgSlugDebuffPlayer>().stackCount * 2;
			player.GetModPlayer<AcidOgSlugDebuffPlayer>().Debuff = true;
		}

		public override bool ReApply(Player player, int time, int buffIndex) {
			player.GetModPlayer<AcidOgSlugDebuffPlayer>().stackCount += 1;
			if(player.GetModPlayer<AcidOgSlugDebuffPlayer>().stackCount > 5) {
				player.GetModPlayer<AcidOgSlugDebuffPlayer>().stackCount = 5;
			}
			player.statDefense -= player.GetModPlayer<AcidOgSlugDebuffPlayer>().stackCount * 2;
			return false;
		}
	}


	public class AcidOgSlugDebuffPlayer : ModPlayer
	{
		public int stackCount;
		public bool Debuff;

		public override void ResetEffects() {
			if(!Debuff) {
				stackCount = 1;
			}
			Debuff = false;
		}

	}
}
