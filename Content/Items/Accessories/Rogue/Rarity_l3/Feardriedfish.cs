using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
    public class Feardriedfish : ModItem
    {
        

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Purple;
			Item.value = Item.sellPrice(0, 6, 0, 0);
		}

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<FeardriedfishPlayer>().FeardriedfishCount++;
        }
    }

    public class FeardriedfishPlayer : ModPlayer
    {
        public int FeardriedfishCount = 0;

        public override void ResetEffects()
        {
            FeardriedfishCount = 0; // 每帧重置计数
        }

        public override void UpdateDead()
        {
            FeardriedfishCount = 0; // 死亡时禁用效果
        }
    }

    public class FeardriedfishGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;


        public override void SetDefaults(NPC npc)
        {
            if (!IsBoss(npc))
            {
                ApplyDefenseReduction(npc);
            }
        }


        public override void OnSpawn(NPC npc, Terraria.DataStructures.IEntitySource source)
        {
            if (!IsBoss(npc))
            {
                ApplyDefenseReduction(npc);
            }
        }

        private bool IsBoss(NPC npc)
        {

            return npc.boss ||
                   NPCID.Sets.ShouldBeCountedAsBoss[npc.type] ||
                   npc.friendly ||
                   npc.townNPC;
        }

        private void ApplyDefenseReduction(NPC npc)
        {
            int totalCount = 0;

            // 统计所有存活玩家佩戴的饰品数量
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (player.active && !player.dead)
                {
                    totalCount += player.GetModPlayer<FeardriedfishPlayer>().FeardriedfishCount;
                }
            }

            //其实我也不知道一开始为什么要写这么多，这些计算无意义
            if (totalCount > 0)
            {
                float multiplier = 1f;
                for (int i = 0; i < totalCount; i++)
                {
                    multiplier *= 0.79f; 
                }


                int originalDefense = npc.defense;
                int modifiedDefense = (int)(originalDefense * multiplier);


                npc.defense = modifiedDefense > 0 ? modifiedDefense : 0;


                if (npc.TryGetGlobalNPC<FeardriedfishNPCDefenseData>(out var data))
                {
                    data.originalDefense = originalDefense;
                }
            }
        }
    }


    public class FeardriedfishNPCDefenseData : GlobalNPC
    {
        public int originalDefense;

        public override bool InstancePerEntity => true;
    }
}