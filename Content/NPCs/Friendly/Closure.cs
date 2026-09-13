using ArknightsMod.Content.Items;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.DisplayForUI;
using ArknightsMod.Content.Items.Gacha;
using ArknightsMod.Content.Items.Material;
using ArknightsMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Personalities;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.Utilities;

namespace ArknightsMod.Content.NPCs.Friendly
{
	[AutoloadHead]
	public class Closure : ModNPC
	{
		public static string[] ShopName => ["Shop", "Shop2"];

		public static int ButtonCount;

		private static string closureShop1FullName;
		private static string closureShop2FullName;

		public override void SetStaticDefaults() {
			Main.npcFrameCount[NPC.type] = 22;
			NPCID.Sets.ExtraFramesCount[NPC.type] = 6;
			NPCID.Sets.AttackFrameCount[NPC.type] = 1;
			// 手持扫描枪射击，射程比原来的近战挥砍远得多，探测范围也相应放大
			NPCID.Sets.DangerDetectRange[NPC.type] = 500;
			// AttackType: 0=投掷 1=射击 2=魔法 3=近战挥砍。改为 1，让她端枪平射而不是挥手打人。
			NPCID.Sets.AttackType[NPC.type] = 1;
			NPCID.Sets.AttackTime[NPC.type] = 18;
			NPCID.Sets.AttackAverageChance[NPC.type] = 10;
			NPCID.Sets.HatOffsetY[NPC.type] = 4;

			NPC.Happiness
				.SetBiomeAffection<ForestBiome>(AffectionLevel.Like)
				.SetBiomeAffection<SnowBiome>(AffectionLevel.Dislike)
				.SetNPCAffection(NPCID.Mechanic, AffectionLevel.Love)
				.SetNPCAffection(NPCID.Cyborg, AffectionLevel.Like)
				.SetNPCAffection(NPCID.Merchant, AffectionLevel.Dislike)
				.SetNPCAffection(NPCID.Angler, AffectionLevel.Hate)
			;
		}

		public override List<string> SetNPCNameList() {
			return [Language.GetTextValue($"Mods.ArknightsMod.NPCs.{GetType().Name}.DisplayName")];
		}

		public override void SetDefaults() {
			NPC.townNPC = true;
			NPC.friendly = true;
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = NPCAIStyleID.Passive;
			NPC.damage = 90;
			NPC.defense = 15;
			NPC.lifeMax = 1000;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0.5f;
			AnimationType = NPCID.Guide;
		}

		public override bool CanTownNPCSpawn(int numTownNPCs) {
			for (int i = 0; i < Main.maxNPCs; i++) {
				if (Main.npc[i].active && Main.npc[i].type == Type) {
					return false;
				}
			}
			if (ClosureWorldSpawnSystem.ClosureTownUnlocked) {
				return true;
			}
			foreach (Player _ in Main.ActivePlayers) {
				return true;
			}
			return false;
		}

		public override bool CanGoToStatue(bool toQueenStatue) => true;

		public int HelpCount = -1;
		public bool Helping;

		public override string GetChat() {
			WeightedRandom<string> chat = new();
			chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue1"));
			chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue2"));
			chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue3"));
			chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue4"));
			chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue5"));
			chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue6"));
			chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue7"));
			chat.Add(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.Dialogue8"));
			return chat;
		}

		public override void SetChatButtons(ref string button, ref string button2) {
			string Text = ButtonCount switch {
				1 => Language.GetTextValue("LegacyInterface.28"),
				2 => this.GetLocalizedValue("Buttons.Shop2"),
				3 => this.GetLocalizedValue("Buttons.Annihilation"),
				_ => this.GetLocalizedValue("Buttons.Help"),
			};
			button = Text;
			button2 = this.GetLocalizedValue("Buttons.Switch");
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shop) {
			if (firstButton) {
				if (Helping) {
					HelpCount++;
					HelpCount %= 5;
				}
				else
					HelpCount = 0;
				Helping = false;
				switch (ButtonCount) {
					case 0:
						var chat = Language.GetText($"Mods.ArknightsMod.Dialogue.Closure.Help{HelpCount + 1}");
						switch (HelpCount) {
							case 0:
								chat = chat.WithFormatArgs($"[i:{ModContent.ItemType<_3DPrintingProcessingStation>()}]");
								break;
							case 1:
								chat = chat.WithFormatArgs($"[i:{ModContent.ItemType<OrironShard>()}]");
								break;
							case 2:
								chat = chat.WithFormatArgs($"[i:{ModContent.ItemType<Drone>()}]");
								break;
							case 4:
								chat = chat.WithFormatArgs($"[i:{ModContent.ItemType<Orundum>()}]", $"[i:{ModContent.ItemType<OrirockCube>()}]", $"[i:{ModContent.ItemType<OriginiumShard>()}]");
								break;
						}
						Main.npcChatText = chat.Value;
						Helping = true;
						break;
					case 1:
					case 2:
						shop = ShopName[ButtonCount - 1];
						break;
					case 3:
						AO();
						break;
				}
				return;
			}
			else {
				ButtonCount++;
				ButtonCount %= 4;
			}
		}

		public void AO() {
			var System = Main.LocalPlayer.GetModPlayer<AOSystem>();
			if (System.QuestType == 1 && System.QuestNum != System.CountQuest) {
				System.QuestType = 0;
			}
			if (!System.AOStatus) {
				if (System.QuestType == 0) {
					Main.npcChatText = System.GetCurrentQuest().ToString();
					Main.npcChatCornerItem = System.GetCurrentQuest().QuestItem;
					System.AOStatus = true;
				}
				else {
					System.QuestNum = Main.rand.Next(System.CountQuest);
					Main.npcChatText = System.GetCurrentQuest().ToString();
					Main.npcChatCornerItem = System.GetCurrentQuest().QuestItem;
					System.AOStatus = true;
				}
			}
			else {
				if (System.CheckQuest()) {
					Main.npcChatText = System.GetCurrentQuest().THX();
					Main.npcChatCornerItem = 0;
					System.SpawnReward(NPC);
					System.AOStatus = false;
					System.QuestNum++;
					if (System.QuestNum == System.CountQuest)
						System.QuestType = 1;
					return;
				}
				else {
					Main.npcChatText = System.GetCurrentQuest().ToString();
					Main.npcChatCornerItem = System.GetCurrentQuest().QuestItem;
				}
			}
		}

		public class AOSystem : ModPlayer
		{
			public static List<Quest> Quests = [];
			public int QuestNum = 0;
			public int CountQuest;
			public bool AOStatus = false;
			public int QuestType = 0;

			public override void Initialize() {
				Quests.Clear();
				Quests.Add(new Quest(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AO", "Green Slimes"), ItemID.GreenSlimeBanner, 1, Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AOThanks")));
				Quests.Add(new Quest(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AO", "Blue Slimes"), ItemID.SlimeBanner, 1, Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AOThanks")));
				Quests.Add(new Quest(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AO", Language.GetText("Mods.ArknightsMod.NPCs.OriginiumSlug.DisplayName")), ModContent.ItemType<Items.Placeable.Banners.OriginiumSlugBanner>(), 1, Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AOThanks")));
				Quests.Add(new Quest(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AO", Language.GetText("Mods.ArknightsMod.NPCs.OriginiumSlugAlpha.DisplayName")), ModContent.ItemType<Items.Placeable.Banners.OriginiumSlugAlphaBanner>(), 1, Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AOThanks")));
				Quests.Add(new Quest(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AO", Language.GetText("Mods.ArknightsMod.NPCs.OriginiumSlugBeta.DisplayName")), ModContent.ItemType<Items.Placeable.Banners.OriginiumSlugBetaBanner>(), 1, Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AOThanks")));
				Quests.Add(new Quest(Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AO", Language.GetText("Mods.ArknightsMod.NPCs.AcidOgSlug.DisplayName")), ModContent.ItemType<Items.Placeable.Banners.AcidOgSlugBanner>(), 1, Language.GetTextValue("Mods.ArknightsMod.Dialogue.Closure.AOThanks")));

				CountQuest = Quests.Count;
			}

			public Quest GetCurrentQuest() {
				try {
					return Quests[QuestNum];
				}
				catch {
					QuestNum = 0;
					return Quests[QuestNum];
				}
			}

			public int Current {
				get => QuestNum;
				set => QuestNum = value;
			}

			public bool CheckQuest() {
				try {
					var quest = Quests[QuestNum];
					foreach (var item in Player.inventory) {
						if (item.type == quest.QuestItem) {
							if (Player.CountItem(quest.QuestItem, quest.ItemAmount) >= quest.ItemAmount) {
								item.stack -= quest.ItemAmount;
								if (item.stack <= 0)
									item.SetDefaults();
								return true;
							}
						}
					}
					return false;
				}
				catch { return false; }
			}

			public void SpawnReward(NPC npc) {
				int reward = Item.NewItem(npc.GetSource_Loot(), Player.getRect(), ModContent.ItemType<Orundum>(), 50);
				if (Main.netMode == NetmodeID.MultiplayerClient && reward >= 0)
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, reward, 0f, 0f, 0f, 0);
				return;
			}

			public static int StartQuest() {
				return 0;
			}

			public override void SaveData(TagCompound tag) {
				tag.Add("QuestNum", QuestNum);
				tag.Add("QuestType", QuestType);
				tag.Add("AOStatus", AOStatus);
			}

			public override void LoadData(TagCompound tag) {

				QuestNum = tag.GetInt("QuestNum");
				QuestType = tag.GetInt("QuestType");
				AOStatus = tag.GetBool("AOStatus");

			}
		}

		public class Quest(string questMessage, int itemID, int itemAmount, string thxMessage = null)
		{
			public string QuestMessage = questMessage;
			public int ItemAmount = itemAmount;
			public int QuestItem = itemID;
			public string ThxMessage = thxMessage;
			public double Weight;

			public override string ToString() {
				return Language.GetTextValue(QuestMessage, Main.LocalPlayer.name);
			}

			public string THX() {
				return Language.GetTextValue(ThxMessage);
			}
		}

		// tModLoader 的商店展示槽位是有限的（40 格）。这两个商店的实际内容完全由
		// ModifyActiveShop 每天重新决定（含材料商店里永远常驻的碳素条），下面注册的静态列表
		// 只是给「哪里能买到」这类外部工具查询用的样本，本身并不会被玩家直接看到。
		// ⚠ 这里注册的条目没有挂任何 Condition，运行时全部算"生效中"——一旦数量超过槽位
		//   上限，tModLoader 每次开店都会弹出"物品太多，塞不进商店里 :("的警告。材料池
		//   （forceAllTiers=true 时 60+ 种）和全部时装袋（当前 50+ 个）都远超 40，
		//   所以必须截断，不能把 GetContent 的结果直接全塞进去。
		private const int MaxStaticShopSampleCount = 30;

		public override void AddShops() {
			var npcShop = new NPCShop(Type, ShopName[0])
				.Add(new Item(ModContent.ItemType<Items.Placeable.Furniture.DareUsa>()) {
					shopCustomPrice = 30,
					shopSpecialCurrency = ArknightsMod.OrundumCurrencyId
				});
			foreach (int materialType in NPCShopSystem.BuildClosurePinnedMaterials()) {
				if (materialType == ModContent.ItemType<CarbonBrick>()) {
					npcShop.Add(new Item(materialType));
				}
				else {
					npcShop.Add(new Item(materialType) {
						shopCustomPrice = 10,
						shopSpecialCurrency = ArknightsMod.OrundumCurrencyId
					});
				}
			}
			int materialSampleCount = 0;
			foreach (int materialType in NPCShopSystem.BuildClosureMaterialPool(true)) {
				if (materialSampleCount >= MaxStaticShopSampleCount)
					break;
				if (materialType == ModContent.ItemType<CarbonBrick>()) {
					npcShop.Add(new Item(materialType));
				}
				else {
					npcShop.Add(new Item(materialType) {
						shopCustomPrice = 10,
						shopSpecialCurrency = ArknightsMod.OrundumCurrencyId
					});
				}
				materialSampleCount++;
			}
			npcShop.Register();

			npcShop = new NPCShop(Type, ShopName[1]);
			int bagSampleCount = 0;
			foreach (var bag in ModContent.GetContent<ArknightsVanityBag>()) {
				if (bagSampleCount >= MaxStaticShopSampleCount)
					break;
				npcShop.Add(new Item(bag.Type) {
					shopCustomPrice = 10,
					shopSpecialCurrency = ArknightsMod.OrundumCurrencyId
				});
				bagSampleCount++;
			}
			npcShop.Register();
		}

		public override void OnSpawn(IEntitySource source) {
			if (source is EntitySource_WorldGen || source is EntitySource_SpawnNPC) {
				if (!ClosureWorldSpawnSystem.ClosureTownUnlocked) {
					ClosureWorldSpawnSystem.ClosureTownUnlocked = true;
					if (Main.netMode == NetmodeID.Server) {
						NetMessage.SendData(MessageID.WorldData);
					}
				}
			}
			NPCShopSystem.UpdateClosureShop(Mod, true);
		}

		public override void ModifyActiveShop(string shopName, Item[] items) {
			closureShop1FullName ??= NPCShopDatabase.GetShopName(ModContent.NPCType<Closure>(), ShopName[0]);
			closureShop2FullName ??= NPCShopDatabase.GetShopName(ModContent.NPCType<Closure>(), ShopName[1]);

			if (shopName == closureShop1FullName) {
				if (NPCShopSystem.ClosureMaterialRotation.Count == 0)
					NPCShopSystem.UpdateClosureShop(Mod, true);
				Array.Fill(items, null);

				items[0] = new Item(ModContent.ItemType<Items.Placeable.Furniture.DareUsa>()) {
					shopCustomPrice = 30,
					shopSpecialCurrency = ArknightsMod.OrundumCurrencyId
				};

				var materialRotation = NPCShopSystem.ClosureMaterialRotation;
				for (int j = 0; j < materialRotation.Count && j + 1 < items.Length; j++) {
					if (materialRotation[j] == ModContent.ItemType<CarbonBrick>()) {
						items[j + 1] = new Item(materialRotation[j]);
					}
					else {
						items[j + 1] = new Item(materialRotation[j]) {
							shopCustomPrice = 10,
							shopSpecialCurrency = ArknightsMod.OrundumCurrencyId
						};
					}
				}
				return;
			}

			if (shopName != closureShop2FullName)
				return;

			if (NPCShopSystem.ClosureTodaysRotation.Count == 0)
				NPCShopSystem.UpdateClosureShop(Mod, true);
			Array.Fill(items, null);

			items[0] = new Item(ModContent.ItemType<DoctorArchiveBag>()) {
				shopCustomPrice = 100,
				shopSpecialCurrency = ArknightsMod.OrundumCurrencyId
			};

			var rotation = NPCShopSystem.ClosureTodaysRotation;
			for (int j = 0; j < rotation.Count && j + 1 < items.Length; j++) {
				items[j + 1] = new Item(rotation[j]) {
					shopCustomPrice = 10,
					shopSpecialCurrency = ArknightsMod.OrundumCurrencyId
				};
			}
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
			damage = 30;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
			cooldown = 30;
			randExtraCooldown = 30;
		}

		// 主动攻击改为使用「工程部特制扫描枪」，但只会武器的普通攻击——
		// 发射和玩家左键相同的能量镖弹幕（ClosureScanShotProjectile），不涉及武器的任何技能：
		// 技能依赖 WeaponPlayer 上的技力/充能状态，那是玩家专属的系统，NPC 身上并不存在，
		// 所以这里只复用普攻弹幕，不去碰技能逻辑。
		public override void TownNPCAttackProj(ref int projType, ref int attackDelay) {
			projType = ModContent.ProjectileType<Projectiles.Medic.Closure.ClosureScanShotProjectile>();
			attackDelay = 1;
		}

		// 弹幕本身直线飞行、不受重力影响（见 ClosureScanShotProjectile.AI），所以重力补正给 0；
		// 速度对齐武器的 Item.shootSpeed=14，保证 NPC 打出来的手感和玩家左键一致。
		public override void TownNPCAttackProjSpeed(ref float multiplier, ref float gravityCorrection, ref float randomOffset) {
			multiplier = 14f;
			gravityCorrection = 0f;
			randomOffset = 0.1f;
		}

		// 攻击时在手上绘制扫描枪的物品贴图（AttackType=1 走这个 hook，原来的近战 TownNPCAttackSwing 不再适用）。
		//
		// horizontalHoldoutOffset 会被原版直接拿去当绘制原点用（Main.cs：origin = (-offset, 高度/2)），
		// 数值越大枪越往前伸、离身体越远。原版自己算这个值的公式是
		//     DrawPlayerItemPos(1f, 物品).X - 4
		// 而 DrawPlayerItemPos 对没有自定义 HoldoutOffset 的物品固定返回 X=10，也就是默认 6。
		// 原版给个别 NPC 把枪往回收，用的也是加大这个减数的办法（num10 = 16/18/28）。
		// 这里沿用同一套算法，只把减数提出来方便调：数值越大枪贴得越近。
		private const int GunPullback = 6;

		public override void DrawTownAttackGun(ref Texture2D item, ref Rectangle itemFrame, ref float scale, ref int horizontalHoldoutOffset) {
			int itemType = ModContent.ItemType<Items.Weapons.Medic.Closure.ClosureScanGun>();
			Main.instance.LoadItem(itemType);
			item = TextureAssets.Item[itemType].Value;
			itemFrame = item.Frame();
			scale = 1f;
			horizontalHoldoutOffset = (int)Main.DrawPlayerItemPos(1f, itemType).X - GunPullback;
		}
	}
}
