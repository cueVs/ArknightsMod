using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
    public class RosmontissEmbrace : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.accessory = true;
            Item.rare = ItemRarityID.Master;
			Item.value = Item.sellPrice(0, 6, 0, 0);
		}

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RosmontissEmbracePlayer>().tranquilityCount++;
        }
    }

    public class RosmontissEmbracePlayer : ModPlayer
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

    public class RosmontissEmbraceGlobalNPC : GlobalNPC
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
                    totalCount += player.GetModPlayer<RosmontissEmbracePlayer>().tranquilityCount;
                }
            }

            if (totalCount > 0)
            {
                float multiplier = 1f;
                for (int i = 0; i < totalCount; i++)
                {
                    multiplier *= 0.70f;
                }

                npc.defDefense = (int)(npc.defDefense * multiplier);
				npc.defense = (int)(npc.defense * multiplier);
			}
        }
    }
}