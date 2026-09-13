using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Localization;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4
{
    public class ChitinousRipper : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.accessory = true;
            Item.rare = ItemRarityID.Master;
			Item.value = Item.sellPrice(0, 16, 0, 0);
		}

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<ChitinousRipperPlayer>().hasChitinousBlade = true;
        }
    }

    public class ChitinousRipperPlayer : ModPlayer
    {
        public bool hasChitinousBlade;
        public int selectedMinionIndex = -1;
        public int selectedPlayerIndex = -1;
        public int selectedNPCIndex = -1;
        public int selectedPlayerForDamageIndex = -1;

        // 用于跟踪是否处于Boss战
        private bool wasInBossFight;
        private bool hasDisplayedMessages;

        // 添加标志来跟踪是否已应用效果
        private bool effectsApplied = false;

        public override void ResetEffects()
        {
            hasChitinousBlade = false;
        }

        public override void PreUpdate()
        {
            if (!hasChitinousBlade)
            {
                // 清除效果
                ClearEffects();
                wasInBossFight = false;
                hasDisplayedMessages = false;
                effectsApplied = false;
                return;
            }

            bool isBossFight = IsBossActive();

            if (isBossFight)
            {
                if (!wasInBossFight)
                {
                    // Boss战刚开始，选择目标
                    SelectTargets();
                    hasDisplayedMessages = false;
                    effectsApplied = false;
                }

                // 应用效果（只应用一次）
                if (!effectsApplied)
                {
                    ApplyEffects();
                    effectsApplied = true;
                }

                // 显示增益对象信息（只显示一次）
                if (!hasDisplayedMessages)
                {
                    DisplayGainMessages();
                    hasDisplayedMessages = true;
                }
            }
            else if (wasInBossFight)
            {
                // Boss战结束，清除效果
                ClearEffects();
                hasDisplayedMessages = false;
                effectsApplied = false;
            }

            wasInBossFight = isBossFight;
        }

        private bool IsBossActive()
        {
            for (int i = 0; i < Main.npc.Length; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.boss)
                {
                    return true;
                }
            }
            return false;
        }

        private void SelectTargets()
        {
            // 重置选择
            selectedMinionIndex = -1;
            selectedPlayerIndex = -1;
            selectedNPCIndex = -1;
            selectedPlayerForDamageIndex = -1;

            // 获取玩家位置
            Vector2 playerCenter = Player.Center;
            float range = 500f; // 500像素范围
            float rangeSquared = range * range;

            // 选择攻击加成目标 (召唤物或玩家)
            List<int> validMinions = [];
            List<int> validPlayersForDamage = [];

            // 检查玩家自己的召唤物
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == Player.whoAmI && proj.minion &&
                    Vector2.DistanceSquared(playerCenter, proj.Center) <= rangeSquared)
                {
                    validMinions.Add(i);
                }
            }

            // 检查其他玩家的召唤物
            for (int p = 0; p < Main.maxPlayers; p++)
            {
                if (p == Player.whoAmI || !Main.player[p].active) continue;

                Player otherPlayer = Main.player[p];
                if (Vector2.DistanceSquared(playerCenter, otherPlayer.Center) <= rangeSquared)
                {
                    for (int i = 0; i < Main.maxProjectiles; i++)
                    {
                        Projectile proj = Main.projectile[i];
                        if (proj.active && proj.owner == p && proj.minion &&
                            Vector2.DistanceSquared(playerCenter, proj.Center) <= rangeSquared)
                        {
                            validMinions.Add(i);
                        }
                    }

                    // 添加玩家到有效玩家列表
                    validPlayersForDamage.Add(p);
                }
            }

            // 添加自己到有效玩家列表
            validPlayersForDamage.Add(Player.whoAmI);

            // 随机选择一个召唤物或玩家进行攻击加成
            if (validMinions.Count > 0 && Main.rand.NextBool(2)) // 50%几率选择召唤物
            {
                selectedMinionIndex = validMinions[Main.rand.Next(validMinions.Count)];
            }
            else if (validPlayersForDamage.Count > 0)
            {
                selectedPlayerForDamageIndex = validPlayersForDamage[Main.rand.Next(validPlayersForDamage.Count)];
            }

            // 选择生命加成目标
            List<int> validNPCs = [];
            List<int> validPlayersForLife = [];

            // 检查友好NPC
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.friendly && npc.lifeMax > 5 && // 有血量的友好NPC
                    Vector2.DistanceSquared(playerCenter, npc.Center) <= rangeSquared)
                {
                    validNPCs.Add(i);
                }
            }

            // 检查玩家
            for (int p = 0; p < Main.maxPlayers; p++)
            {
                if (Main.player[p].active &&
                    Vector2.DistanceSquared(playerCenter, Main.player[p].Center) <= rangeSquared)
                {
                    validPlayersForLife.Add(p);
                }
            }

            // 随机选择一个玩家或NPC进行生命加成
            if (validNPCs.Count > 0 && Main.rand.NextBool(2)) // 50%几率选择NPC
            {
                selectedNPCIndex = validNPCs[Main.rand.Next(validNPCs.Count)];
            }
            else if (validPlayersForLife.Count > 0)
            {
                selectedPlayerIndex = validPlayersForLife[Main.rand.Next(validPlayersForLife.Count)];
            }
        }

        private void DisplayGainMessages()
        {
            string yourself = Language.GetTextValue("Mods.ArknightsMod.Default.you");
            string player = Language.GetTextValue("Mods.ArknightsMod.Default.player");
            string buff = Language.GetTextValue("Mods.ArknightsMod.Default.check");

            // 显示攻击加成目标信息
            if (selectedMinionIndex >= 0)
            {
                Projectile proj = Main.projectile[selectedMinionIndex];
                if (proj.active)
                {
                    string minionName = Lang.GetProjectileName(proj.type).Value;
                    int textIndex = CombatText.NewText(Player.getRect(), Color.Gold, $" {buff}{minionName}");
                    if (textIndex > -1)
                    {
                        Main.combatText[textIndex].lifeTime = 300; // 3秒显示时间
                    }
                }
            }
            else if (selectedPlayerForDamageIndex >= 0)
            {
                string playerName = (selectedPlayerForDamageIndex == Player.whoAmI) ? yourself : $"{player}{selectedPlayerForDamageIndex}";
                int textIndex = CombatText.NewText(Player.getRect(), Color.Gold, $" {buff}{playerName}");
                if (textIndex > -1)
                {
                    Main.combatText[textIndex].lifeTime = 300; // 3秒显示时间
                }
            }

            // 显示生命加成目标信息
            if (selectedNPCIndex >= 0)
            {
                NPC npc = Main.npc[selectedNPCIndex];
                if (npc.active)
                {
                    string npcName = Lang.GetNPCName(npc.type).Value;
                    int textIndex = CombatText.NewText(Player.getRect(), Color.Red, $" {buff}{npcName}");
                    if (textIndex > -1)
                    {
                        Main.combatText[textIndex].lifeTime = 300; // 3秒显示时间
                    }
                }
            }
            else if (selectedPlayerIndex >= 0)
            {
                string playerName = (selectedPlayerIndex == Player.whoAmI) ? yourself : $"{player}{selectedPlayerIndex}";
                int textIndex = CombatText.NewText(Player.getRect(), Color.Red, $"{buff}{playerName}");
                if (textIndex > -1)
                {
                    Main.combatText[textIndex].lifeTime = 300; // 3秒显示时间
                }
            }
        }

        private void ApplyEffects()
        {
            // 应用攻击加成 - 召唤物
            if (selectedMinionIndex >= 0 && selectedMinionIndex < Main.maxProjectiles)
            {
                Projectile proj = Main.projectile[selectedMinionIndex];
                if (proj.active)
                {
                    // 使用GetGlobalProjectile来存储和修改伤害
                    var globalProj = proj.GetGlobalProjectile<ChitinousRipperGlobalProjectile>();
                    if (!globalProj.damageModified)
                    {
                        globalProj.originalDamage = proj.damage;
                        globalProj.damageModified = true;
                    }

                    proj.damage = (int)(globalProj.originalDamage * 2f); // 100%加成
                }
                else
                {
                    // 召唤物已消失，重新选择
                    SelectTargets();
                    hasDisplayedMessages = false;
                    effectsApplied = false;
                }
            }

            // 应用攻击加成 - 玩家
            if (selectedPlayerForDamageIndex >= 0 && selectedPlayerForDamageIndex < Main.maxPlayers)
            {
                Player targetPlayer = Main.player[selectedPlayerForDamageIndex];
                if (targetPlayer.active)
                {
                    // 使用GetModPlayer来存储和修改伤害加成
                    var modPlayer = targetPlayer.GetModPlayer<ChitinousRipperTargetPlayer>();
                    if (!modPlayer.damageModified)
                    {
                        modPlayer.damageModified = true;
                    }
                }
                else
                {
                    // 玩家已离开，重新选择
                    SelectTargets();
                    hasDisplayedMessages = false;
                    effectsApplied = false;
                }
            }

            // 应用生命加成 - NPC
            if (selectedNPCIndex >= 0 && selectedNPCIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[selectedNPCIndex];
                if (npc.active && npc.friendly)
                {
                    var globalNPC = npc.GetGlobalNPC<ChitinousRipperGlobalNPC>();
                    if (!globalNPC.lifeModified)
                    {
                        globalNPC.originalLifeMax = npc.lifeMax;
                        globalNPC.originalLife = npc.life; // 保存当前生命值
                        globalNPC.lifeModified = true;
                    }

                    // 直接修改生命值
                    npc.lifeMax = globalNPC.originalLifeMax * 2; // 100%加成
                    // 按比例调整当前生命值
                    float lifeRatio = (float)globalNPC.originalLife / globalNPC.originalLifeMax;
                    npc.life = (int)(npc.lifeMax * lifeRatio);
                }
                else
                {
                    SelectTargets();
                    hasDisplayedMessages = false;
                    effectsApplied = false;
                }
            }

            // 应用生命加成 - 玩家
            if (selectedPlayerIndex >= 0 && selectedPlayerIndex < Main.maxPlayers)
            {
                Player targetPlayer = Main.player[selectedPlayerIndex];
                if (targetPlayer.active)
                {
                    var modPlayer = targetPlayer.GetModPlayer<ChitinousRipperTargetPlayer>();
                    if (!modPlayer.lifeModified)
                    {
                        modPlayer.originalLifeMax = targetPlayer.statLifeMax2;
                        modPlayer.originalLife = targetPlayer.statLife; // 保存当前生命值
                        modPlayer.lifeModified = true;

                        // 设置生命加成
                        modPlayer.lifeBoost = modPlayer.originalLifeMax;
                    }
                }
                else
                {
                    SelectTargets();
                    hasDisplayedMessages = false;
                    effectsApplied = false;
                }
            }
        }

        // 在造成伤害时实际应用加成
        public override void UpdateEquips()
        {
            if (hasChitinousBlade && IsBossActive() && selectedPlayerForDamageIndex == Player.whoAmI)
            {
                // 实际加倍伤害
                Player.GetDamage(DamageClass.Generic) += 1;
            }
        }

        private void ClearEffects()
        {
            // 恢复攻击加成 - 召唤物
            if (selectedMinionIndex >= 0 && selectedMinionIndex < Main.maxProjectiles)
            {
                Projectile proj = Main.projectile[selectedMinionIndex];
                if (proj.active)
                {
                    var globalProj = proj.GetGlobalProjectile<ChitinousRipperGlobalProjectile>();
                    if (globalProj.damageModified)
                    {
                        proj.damage = globalProj.originalDamage;
                        globalProj.damageModified = false;
                    }
                }
            }

            // 恢复攻击加成 - 玩家
            if (selectedPlayerForDamageIndex >= 0 && selectedPlayerForDamageIndex < Main.maxPlayers)
            {
                Player targetPlayer = Main.player[selectedPlayerForDamageIndex];
                if (targetPlayer.active)
                {
                    var modPlayer = targetPlayer.GetModPlayer<ChitinousRipperTargetPlayer>();
                    if (modPlayer.damageModified)
                    {
                        modPlayer.damageModified = false;
                    }
                }
            }

            // 恢复生命加成 - NPC
            if (selectedNPCIndex >= 0 && selectedNPCIndex < Main.maxNPCs)
            {
                NPC npc = Main.npc[selectedNPCIndex];
                if (npc.active && npc.friendly)
                {
                    var globalNPC = npc.GetGlobalNPC<ChitinousRipperGlobalNPC>();
                    if (globalNPC.lifeModified)
                    {
                        // 恢复原始生命值
                        npc.lifeMax = globalNPC.originalLifeMax;
                        // 按比例调整当前生命值
                        float lifeRatio = (float)npc.life / (globalNPC.originalLifeMax * 2);
                        npc.life = (int)(npc.lifeMax * lifeRatio);
                        globalNPC.lifeModified = false;
                    }
                }
            }

            // 恢复生命加成 - 玩家
            if (selectedPlayerIndex >= 0 && selectedPlayerIndex < Main.maxPlayers)
            {
                Player targetPlayer = Main.player[selectedPlayerIndex];
                if (targetPlayer.active)
                {
                    var modPlayer = targetPlayer.GetModPlayer<ChitinousRipperTargetPlayer>();
                    if (modPlayer.lifeModified)
                    {
                        modPlayer.lifeModified = false;
                        modPlayer.lifeBoost = 0;
                    }
                }
            }

            // 重置选择
            selectedMinionIndex = -1;
            selectedPlayerIndex = -1;
            selectedNPCIndex = -1;
            selectedPlayerForDamageIndex = -1;
        }
    }

    // 用于存储玩家伤害和生命值修改状态的类
    public class ChitinousRipperTargetPlayer : ModPlayer
    {
        public bool damageModified = false;
        public bool lifeModified = false;
        public int lifeBoost = 0;
        public int originalLifeMax = 100;
        public int originalLife = 100;

        public override void ResetEffects()
        {
            damageModified = false;

            // 只有在没有激活生命修改时才重置生命加成
            if (!lifeModified)
            {
                lifeBoost = 0;
            }
        }

        public override void ModifyMaxStats(out StatModifier health, out StatModifier mana)
        {
            health = StatModifier.Default;
            mana = StatModifier.Default;

            // 只有在激活生命修改时才应用生命加成
            if (lifeModified)
            {
                health.Base += lifeBoost;
            }
        }
    }

    // 用于存储投射物伤害修改状态的类
    public class ChitinousRipperGlobalProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;

        public bool damageModified = false;
        public int originalDamage = 0;

        public override void SetDefaults(Projectile projectile)
        {
            damageModified = false;
            originalDamage = 0;
        }
    }

    // 用于存储NPC生命值修改状态的类
    public class ChitinousRipperGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool lifeModified = false;
        public int originalLifeMax = 0;
        public int originalLife = 0;

        public override void SetDefaults(NPC npc)
        {
            lifeModified = false;
            originalLifeMax = 0;
            originalLife = 0;
        }
    }
}