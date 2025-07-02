using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
			var mp = player.GetModPlayer<AcidOgSlugDebuffPlayer>();

			player.statDefense -= mp.stackCount * 2;
			mp.Debuff = true;
		}

		public override bool ReApply(Player player, int time, int buffIndex) {
			ref int count = ref player.GetModPlayer<AcidOgSlugDebuffPlayer>().stackCount;
			count = Math.Max(++count, 5);

			/*player.GetModPlayer<AcidOgSlugDebuffPlayer>().stackCount += 1;
			if(player.GetModPlayer<AcidOgSlugDebuffPlayer>().stackCount > 5) {
				player.GetModPlayer<AcidOgSlugDebuffPlayer>().stackCount = 5;
			}*/
			player.statDefense -= count * 2;
			return false;
		}
	}


	public class AcidOgSlugDebuffPlayer : ModPlayer
	{
		public int stackCount;
		public bool Debuff;

		public override void ResetEffects() {
			if (!Debuff) {
				stackCount = 1;
			}
			Debuff = false;
		}

	}
}
