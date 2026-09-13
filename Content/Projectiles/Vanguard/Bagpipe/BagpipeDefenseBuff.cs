using ArknightsMod.Content.Items.Weapons.Vanguard.Bagpipe;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Vanguard.Bagpipe
{
	public class BagpipeDefenseBuff : ModBuff
	{
		public override void SetStaticDefaults() {
			Main.pvpBuff[Type] = true;
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buff) {
			player.statDefense *= 2.2f;
			if (player.HeldItem.type != ModContent.ItemType<BagpipeSpear>())
				player.ClearBuff(Type);
		}
	}
}
