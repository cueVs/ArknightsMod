using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs.Supporter.Pramanix
{
	public class PramanixFreezeDebuff : ModBuff
	{
		public override string Texture => $"Terraria/Images/Buff_{BuffID.Frozen}";

		public override void SetStaticDefaults() {
			Main.debuff[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(NPC npc, ref int buffIndex) {
			if (!Content.Items.Weapons.Supporter.Pramanix.PramanixColdAttachment.HasKnockbackGrace(npc))
				npc.velocity = Vector2.Zero;
		}
	}
}
