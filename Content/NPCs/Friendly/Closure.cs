using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;
using Terraria.GameContent.Personalities;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Content.NPCs.Friendly
{
    [AutoloadHead]
    class Closure : ModNPC
    {

        public override void SetStaticDefaults()
        {
            // DisplayName.SetDefault("Engineer");
            Main.npcFrameCount[NPC.type] = 22;
            NPCID.Sets.ExtraFramesCount[NPC.type] = 6;
            NPCID.Sets.AttackFrameCount[NPC.type] = 1;
            NPCID.Sets.DangerDetectRange[NPC.type] = 40;
            NPCID.Sets.AttackType[NPC.type] = 3;
            NPCID.Sets.AttackTime[NPC.type] = 18;
            NPCID.Sets.AttackAverageChance[NPC.type] = 10;
            NPCID.Sets.HatOffsetY[NPC.type] = 4;

            // Set Example Person's biome and neighbor preferences with the NPCHappiness hook. You can add happiness text and remarks with localization (See an example in ExampleMod/Localization/en-US.lang).
            // NOTE: The following code uses chaining - a style that works due to the fact that the SetXAffection methods return the same NPCHappiness instance they're called on.
            NPC.Happiness
                .SetBiomeAffection<ForestBiome>(AffectionLevel.Like) // Example Person prefers the forest.
                .SetBiomeAffection<SnowBiome>(AffectionLevel.Dislike) // Example Person dislikes the snow.
                // .SetBiomeAffection<ExampleSurfaceBiome>(AffectionLevel.Love) // Example Person likes the Example Surface Biome
                .SetNPCAffection(NPCID.Dryad, AffectionLevel.Love) // Loves living near the dryad.
                .SetNPCAffection(NPCID.Guide, AffectionLevel.Like) // Likes living near the guide.
                .SetNPCAffection(NPCID.Merchant, AffectionLevel.Dislike) // Dislikes living near the merchant.
                .SetNPCAffection(NPCID.Demolitionist, AffectionLevel.Hate) // Hates living near the demolitionist.
            ; // < Mind the semicolon!
        }

        public override List<string> SetNPCNameList() {
            return new List<string> { Language.GetTextValue("Mods.ArknightsMod.NameList.Closure") };
        }

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = 7;
            NPC.damage = 90;
            NPC.defense = 15;
            NPC.lifeMax = 1000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            AnimationType = NPCID.Guide;
        }

        public override bool CanTownNPCSpawn(int numTownNPCs, int money)
        {
            foreach (Player player in Main.player)
            {
                if (!player.active)
                {
                    continue;
                }
                if (player.statDefense > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public override bool CanGoToStatue(bool toQueenStatue)
        {
            return true;
        }


        public override string GetChat()
        {
            WeightedRandom<string> chat = new();
            chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue1"));
            chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue2"));
            chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue3"));
            chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue4"));
            chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue5"));
            chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue6"));
            chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue7"));
            chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue8"));
            //if (LanternNight.LanternsUp)
            //{
            //    chat.Add("You have done well, indeed you have. You've a strong arm, strong faith, and most importantly, a strong heart.", 1.5);
            //}
            return chat;
        }
        public override void SetChatButtons(ref string button, ref string button2)
        {
            button = Language.GetTextValue("LegacyInterface.28");
            button2 = Language.GetTextValue("Mods.ArknightsMod.ButtonName.button2");
        }

        public override void OnChatButtonClicked(bool firstButton, ref bool shop)
        {
            if (firstButton)
            {
                shop = true;
                return;
            }
            else
            {
                AO();
            }
        }

        public void AO()
        {
            var System = Main.player[Main.myPlayer].GetModPlayer<AOSystem>();
            // AOStatus: false=最初の受注時, true=クエスト中
            // QuestType: 0:pre/unfinAll 1:pre/fin (2:HM/unfin 3:HM/fin)
            if (!System.AOStatus)
            {
                if (System.QuestType == 0)
                {
                    Main.npcChatText = System.GetCurrentQuest().ToString();
                    Main.npcChatCornerItem = System.GetCurrentQuest().QuestItem;
                    System.AOStatus = true;
                }
                else
                {
                    System.QuestNum = Main.rand.Next(System.CountQuest);
                    Main.npcChatText = System.GetCurrentQuest().ToString();
                    Main.npcChatCornerItem = System.GetCurrentQuest().QuestItem;
                    System.AOStatus = true;
                }
            }
            else
            {
                if (System.CheckQuest())
                {
                    Main.npcChatText = System.GetCurrentQuest().Finish();
                    Main.npcChatCornerItem = 0;
                    System.SpawnReward(NPC);
                    System.QuestNum++;
                    if (System.QuestNum == System.CountQuest)
                        System.QuestType = 1;
                    return;
                }
                else
                {
                    Main.npcChatText = System.GetCurrentQuest().ToString();
                    Main.npcChatCornerItem = System.GetCurrentQuest().QuestItem;
                }
            }
        }

        public class AOSystem : ModPlayer
        {
            public static List<Quest> Quests = new();
            public int QuestNum = 0;
            public int CountQuest;
            public bool AOStatus = false;
            public int QuestType = 0;

            public override void Initialize()
            {
                Quests.Clear();
                Quests.Add(new Quest(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AO", "Green Slimes"), ItemID.GreenSlimeBanner, 1, Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AOThanks")));
                Quests.Add(new Quest(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AO", "Blue Slimes"), ItemID.SlimeBanner, 1, Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AOThanks")));
                CountQuest = Quests.Count;
            }

            public Quest GetCurrentQuest()
            {
                return Quests[QuestNum];
            }

            public int Current
            {
                get { return QuestNum; }
                set { QuestNum = value; }
            }

            public override void clientClone(ModPlayer clientClone)
            {
                AOSystem clone = clientClone as AOSystem;
                clone.QuestNum = QuestNum;
                clone.QuestType = QuestType;
                clone.AOStatus = AOStatus;
            }

            public override void SendClientChanges(ModPlayer clientPlayer)
            {
                AOSystem clone = clientPlayer as AOSystem;
                if (clone.QuestNum != QuestNum || clone.QuestType != QuestType || clone.AOStatus != AOStatus)
                {
                    ModPacket packet = Mod.GetPacket();
                    packet.Write((byte)4);
                    packet.Write(Player.whoAmI);
                    packet.Write(QuestNum);
                    packet.Write(QuestType);
                    packet.Write(AOStatus);
                    packet.Send();
                }
            }

            public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
            {
                ModPacket packet = Mod.GetPacket();
                packet.Write((byte)4);
                packet.Write(Player.whoAmI);
                packet.Write(QuestNum);
                packet.Write(QuestType);
                packet.Write(AOStatus);
                packet.Send(toWho, fromWho);
            }

            public bool CheckQuest()
            {                
                var quest = Quests[QuestNum];               
                foreach (var item in Player.inventory)
                {
                    if (item.type == quest.QuestItem)
                    {
                        if (Player.CountItem(quest.QuestItem, quest.ItemAmount) >= quest.ItemAmount)
                        {                            
                            item.stack -= quest.ItemAmount;                         
                            if (item.stack <= 0)
                                item.SetDefaults();
                            return true;
                        }
                    }
                }
                return false;
            }

            public void SpawnReward(NPC npc)
            { 
                int reward = Item.NewItem(npc.GetSource_Loot(), Player.getRect(), ModContent.ItemType<Items.Orundum>(), 50);
                if (Main.netMode == NetmodeID.MultiplayerClient && reward >= 0)
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, reward, 0f, 0f, 0f, 0);
                return;
            }

            public static int StartQuest()
            {
                return 0;
            }

            public override void SaveData(TagCompound tag)
            {
                tag.Add("QuestNum", QuestNum);
                tag.Add("QuestType", QuestType);
                tag.Add("AOStatus", AOStatus);
            }

            public override void LoadData(TagCompound tag)
            {

                QuestNum = tag.GetInt("QuestNum");
                QuestType = tag.GetInt("QuestType");
                AOStatus = tag.GetBool("AOStatus");

            }
        }

        public class Quest
        {
            public string QuestMessage;
            public int ItemAmount;
            public int QuestItem;
            public string FinMessage;
            public double Weight;

            public Quest(string questMessage, int itemID, int itemAmount, string finMessage = null)
            {
                QuestMessage = questMessage;
                QuestItem = itemID;
                ItemAmount = itemAmount;
                FinMessage = finMessage;
            }

            public override string ToString()
            {
                return Language.GetTextValue(QuestMessage, Main.LocalPlayer.name);
            }

            public string Finish()
            {
                return Language.GetTextValue(FinMessage);
            }
        }

        public override void SetupShop(Chest shop, ref int nextSlot)
        {
            shop.item[nextSlot].SetDefaults(ModContent.ItemType<Items.Material.Polyketon>());
            shop.item[nextSlot].shopCustomPrice = 1;
            shop.item[nextSlot].shopSpecialCurrency = ArknightsMod.OrundumCurrencyId;
            nextSlot++;
            shop.item[nextSlot].SetDefaults(ModContent.ItemType<Items.Material.LoxicKohl>());
            shop.item[nextSlot].shopCustomPrice = 1;
            shop.item[nextSlot].shopSpecialCurrency = ArknightsMod.OrundumCurrencyId;
            nextSlot++;
            shop.item[nextSlot].SetDefaults(ModContent.ItemType<Items.Placeable.Furniture.DareUsa>());
            shop.item[nextSlot].shopCustomPrice = 1;
            shop.item[nextSlot].shopSpecialCurrency = ArknightsMod.OrundumCurrencyId;
            nextSlot++;
        }

        public override void TownNPCAttackStrength(ref int damage, ref float knockback)
        {
            damage = 30;
            knockback = 4f;
        }

        public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown)
        {
            cooldown = 30;
            randExtraCooldown = 30;
        }
    
    }
}
