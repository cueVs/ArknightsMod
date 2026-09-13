using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l1
{
    public class NamelessPoetryCollection : ModItem
    {
      //减少全部怪物5%生命

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(0, 2, 0, 0);
		}

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<NamelessPoetryCollectionPlayer>().tranquilityCount++;
        }
    }

    public class NamelessPoetryCollectionPlayer : ModPlayer
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

    public class NamelessPoetryCollectionGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public override void SetDefaults(NPC npc)
        {
            if (!IsBoss(npc))
            {
                ApplyHealthReduction(npc);
            }
        }

        private bool IsBoss(NPC npc)
        {
            return npc.boss ||
                   NPCID.Sets.ShouldBeCountedAsBoss[npc.type] ||
                   npc.friendly;
        }

        private void ApplyHealthReduction(NPC npc)
        {
            int totalCount = 0;

            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && !player.dead)
                {
                    totalCount += player.GetModPlayer<NamelessPoetryCollectionPlayer>().tranquilityCount;
                }
            }

            if (totalCount > 0)
            {
                float multiplier = 1f;
                for (int i = 0; i < totalCount; i++)
                {
                    multiplier *= 0.95f;
                }

                npc.lifeMax = (int)(npc.lifeMax * multiplier);
                npc.life = (int)(npc.life * multiplier);
            }
        }
    }
}