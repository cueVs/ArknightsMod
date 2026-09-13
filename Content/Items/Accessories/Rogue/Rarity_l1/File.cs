using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l1
{
    public class File : ModItem
    {
        //6%

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Green;
			Item.value = Item.sellPrice(0, 2, 0, 0);
		}

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<RosemaryPlayer>().rosemaryCount++;
        }
    }

    public class RosemaryPlayer : ModPlayer
    {
        public int rosemaryCount = 0;

        public override void ResetEffects()
        {
            rosemaryCount = 0; // 每帧重置计数
        }

        public override void UpdateDead()
        {
            rosemaryCount = 0; // 死亡时禁用效果
        }
    }

    public class RosemaryGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // 在NPC生成时应用防御减少
        public override void SetDefaults(NPC npc)
        {
            if (!IsBoss(npc))
            {
                ApplyDefenseReduction(npc);
            }
        }

        // 在NPC生成后也应用（确保兼容性）
        public override void OnSpawn(NPC npc, Terraria.DataStructures.IEntitySource source)
        {
            if (!IsBoss(npc))
            {
                ApplyDefenseReduction(npc);
            }
        }

        private bool IsBoss(NPC npc)
        {
            // 排除Boss和友好NPC
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
                    totalCount += player.GetModPlayer<RosemaryPlayer>().rosemaryCount;
                }
            }

            // 乘算叠加 (1 * 0.7^n)
            if (totalCount > 0)
            {
                float multiplier = 1f;
                for (int i = 0; i < totalCount; i++)
                {
                    multiplier *= 0.96f; // 每次叠加减少30%
                }

                // 应用防御修改
                int originalDefense = npc.defense;
                int modifiedDefense = (int)(originalDefense * multiplier);

                // 确保防御不会变成负值
                npc.defense = modifiedDefense > 0 ? modifiedDefense : 0;

                // 可选：存储原始防御值用于显示或其他效果
                if (npc.TryGetGlobalNPC<RosemaryNPCDefenseData>(out var data))
                {
                    data.originalDefense = originalDefense;
                }
            }
        }
    }

    // 用于存储NPC原始防御数据的类
    public class RosemaryNPCDefenseData : GlobalNPC
    {
        public int originalDefense;

        public override bool InstancePerEntity => true;
    }
}