using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
    public class TheProfoundSilence : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Purple;
			Item.value = Item.sellPrice(0, 6, 0, 0);
		}

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<TheProfoundSilencePlayer>().tranquilityCount++;
        }
    }

    public class TheProfoundSilencePlayer : ModPlayer
    {
        public int tranquilityCount = 0;

        public override void ResetEffects()
        {
            tranquilityCount = 0;
        }

        public override void UpdateDead()
        {
            tranquilityCount = 0;
        }
    }

    public class TheProfoundSilenceGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void SetDefaults(NPC npc)
        {
            if (Isenemy(npc))
            {
                ApplyHealthReduction(npc);
            }
        }

        private bool Isenemy(NPC npc)
        {
            return !npc.friendly;
        }

        private void ApplyHealthReduction(NPC npc)
        {
            int totalCount = 0;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && !player.dead)
                {
                    totalCount += player.GetModPlayer<TheProfoundSilencePlayer>().tranquilityCount;
                }
            }

            if (totalCount > 0)
            {
                float multiplier = 1f;
                for (int i = 0; i < totalCount; i++)
                {
                    multiplier *= 0.80f;
                }

                npc.lifeMax = (int)(npc.lifeMax * multiplier);
                npc.life = (int)(npc.life * multiplier);
            }
        }
    }
}