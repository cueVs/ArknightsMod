using ArknightsMod.Content.Items;
using ArknightsMod.Content.Players;
using ArknightsMod.Content.NPCs.Enemy.Chapter6;
using ArknightsMod.Content.NPCs.Enemy.ThroughChapter4;
using ArknightsMod.Content.NPCs.Enemy.TillChapter7;
using ArknightsMod.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.NPCs.Friendly
{
	[AutoloadHead]
	public class Cannot : ModNPC
	{
		private const float MaxReinforcementRequestDistancePixels = 160f * 16f;
		private const float MaxCannotInteractionDistancePixels = 20f * 16f;

		// 修改：保存完整的 Item 对象而不是只保存 type
		public readonly static List<Item> shopItems = [];

		// A static instance of the declarative shop, defining all the items which can be brought. Used to create a new inventory when the NPC spawns
		public static CannotShop Shop;

		public const string ShopName = "Shop";

		public int TouchCount = 0;

		public static bool Isnpcexist {
			get {
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC SeekForNPCs = Main.npc[i];
					if (SeekForNPCs.active && Array.Exists(Eliteslist, x => x == SeekForNPCs.type)) {
						return true;
					}
				}
				return false;
			}
		}

		public int summoncd = 0;

		public bool Runaway;

		static int[] Eliteslist => [
			ModContent.NPCType<ShieldGuard>(),
			ModContent.NPCType<IceCleaver>(),
			ModContent.NPCType<Seniorcaster>(),
			ModContent.NPCType<InsaneZombieL>(),
			ModContent.NPCType<Oneiros>()
			];

		public override void SetStaticDefaults() {
			Main.npcFrameCount[Type] = 23;
			NPCID.Sets.ExtraFramesCount[Type] = NPCID.Sets.ExtraFramesCount[NPCID.OldMan];
			NPCID.Sets.AttackFrameCount[Type] = NPCID.Sets.ExtraFramesCount[NPCID.OldMan];
			NPCID.Sets.DangerDetectRange[Type] = NPCID.Sets.ExtraFramesCount[NPCID.OldMan];
			NPCID.Sets.ActsLikeTownNPC[Type] = true;
			NPCID.Sets.AttackType[Type] = NPCID.Sets.ExtraFramesCount[NPCID.OldMan];
			NPCID.Sets.AttackTime[Type] = NPCID.Sets.ExtraFramesCount[NPCID.OldMan];
			NPCID.Sets.AttackAverageChance[Type] = NPCID.Sets.ExtraFramesCount[NPCID.OldMan];
			NPCID.Sets.HatOffsetY[Type] = NPCID.Sets.ExtraFramesCount[NPCID.OldMan];
			NPCID.Sets.NoTownNPCHappiness[Type] = true;
		}

		public override List<string> SetNPCNameList() {
			return [];
		}

		public override void SetDefaults() {
			NPC.townNPC = true;
			NPC.friendly = false;
			NPC.dontTakeDamage = false;
			NPC.chaseable = false;
			NPC.dontTakeDamageFromHostiles = true;
			NPC.width = 18;
			NPC.height = 40;
			NPC.aiStyle = NPCAIStyleID.Passive;
			NPC.damage = 0;
			NPC.defense = 99;
			NPC.lifeMax = 1000;
			NPC.npcSlots = 7f;
			NPC.HitSound = SoundID.NPCHit1;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.knockBackResist = 0f;
			NPC.rarity = 1;
			AnimationType = NPCID.OldMan;
		}

		public override void OnHitByItem(Player player, Item item, NPC.HitInfo hit, int damageDone) {
			if (summoncd <= 0) {
				TrySpawnReinforcements(player);
				summoncd = 600;
			}
		}

		public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone) {
			if (!projectile.TryGetOwner(out var owner))
				return;

			if (summoncd <= 0) {
				TrySpawnReinforcements(owner);
				summoncd = 600;
			}
		}

		public override bool? CanBeHitByItem(Player player, Item item) {
			if (Isnpcexist)
				return false;
			if (!player.GetModPlayer<CannotAggroPlayer>().CanDamageCannotForCurrentLife())
				return false;
			return base.CanBeHitByItem(player, item);
		}

		public override bool? CanBeHitByProjectile(Projectile projectile) {
			if (Isnpcexist)
				return false;
			if (!projectile.TryGetOwner(out Player owner) || !owner.GetModPlayer<CannotAggroPlayer>().CanDamageCannotForCurrentLife())
				return false;
			return base.CanBeHitByProjectile(projectile);
		}

		public override bool CanBeHitByNPC(NPC attacker) {
			return false;
		}

		public override string GetChat() {
			int rand = Main.rand.Next(1, 9);
			return Language.GetTextValue($"Mods.ArknightsMod.Dialogue.Cannot.Dialogue{rand}");
		}

		public override void SetChatButtons(ref string button, ref string button2) {
			button = this.GetLocalizedValue("Buttons.Shop");
			button2 = this.GetLocalizedValue("Buttons.Touch");

		}

		public void TrySpawnReinforcements(Player target) {
			int x = 0;
			int y = 0;
			bool canSpawn = false;
			int sWidth = 1920;
			int sHeight = 1080;
			int spawnSpaceX = 3;
			int spawnSpaceY = 3;
			int spawnRangeX = (int)(sWidth / 16 * 0.7 / 2);
			int spawnRangeY = (int)(sHeight / 16 * 0.7 / 2);
			int safeRangeX = (int)(sWidth / 16 * 0.52 / 2);
			int safeRangeY = (int)(sHeight / 16 * 0.52 / 2);
			if (target.inventory[target.selectedItem].type == ItemID.SniperRifle || target.inventory[target.selectedItem].type == ItemID.Binoculars || target.scope) {
				float num11 = 1.5f;
				if (target.inventory[target.selectedItem].type == ItemID.SniperRifle && target.scope)
					num11 = 1.25f;
				else if (target.inventory[target.selectedItem].type == ItemID.SniperRifle)
					num11 = 1.5f;
				else if (target.inventory[target.selectedItem].type == ItemID.Binoculars)
					num11 = 1.5f;
				else if (target.scope)
					num11 = 2f;

				spawnRangeX += (int)(sWidth / 16 * 0.5 / (double)num11);
				spawnRangeY += (int)(sHeight / 16 * 0.5 / (double)num11);
				safeRangeX += (int)(sWidth / 16 * 0.5 / (double)num11);
				safeRangeY += (int)(sHeight / 16 * 0.5 / (double)num11);
			}

			NPCLoader.EditSpawnRange(target, ref spawnRangeX, ref spawnRangeY, ref safeRangeX, ref safeRangeY);

			int maxLeft = (int)(target.position.X / 16f) - spawnRangeX;
			int maxRight = (int)(target.position.X / 16f) + spawnRangeX;
			int maxTop = (int)(target.position.Y / 16f) - spawnRangeY;
			int maxBottom = (int)(target.position.Y / 16f) + spawnRangeY;
			int minLeft = (int)(target.position.X / 16f) - safeRangeX;
			int minRight = (int)(target.position.X / 16f) + safeRangeX;
			int minTop = (int)(target.position.Y / 16f) - safeRangeY;
			int minBottom = (int)(target.position.Y / 16f) + safeRangeY;
			if (maxLeft < 0)
				maxLeft = 0;

			if (maxRight > Main.maxTilesX)
				maxRight = Main.maxTilesX;

			if (maxTop < 0)
				maxTop = 0;

			if (maxBottom > Main.maxTilesY)
				maxBottom = Main.maxTilesY;

			for (int m = 0; m < 50; m++) {
				int randX = Main.rand.Next(maxLeft, maxRight);
				int randY = Main.rand.Next(maxTop, maxBottom);
				if (!Main.tile[randX, randY].HasUnactuatedTile || !Main.tileSolid[Main.tile[randX, randY].TileType]) {
					for (int n = randY; n < Main.maxTilesY && n < maxBottom; n++) {
						if (Main.tile[randX, n].HasUnactuatedTile && Main.tileSolid[Main.tile[randX, n].TileType]) {
							if (randX < minLeft || randX > minRight || n < minTop || n > minBottom) {
								x = randX;
								y = n;
								canSpawn = true;
							}

							break;
						}
					}

					if (canSpawn) {
						int left = x - spawnSpaceX / 2;
						int right = x + spawnSpaceX / 2;
						int top = y - spawnSpaceY;
						int bottom = y;
						if (left < 0)
							canSpawn = false;

						if (right > Main.maxTilesX)
							canSpawn = false;

						if (top < 0)
							canSpawn = false;

						if (bottom > Main.maxTilesY)
							canSpawn = false;

						if (canSpawn) {
							for (int spaceX = left; spaceX < right; spaceX++) {
								for (int spaceY = top; spaceY < bottom; spaceY++) {
									if (Main.tile[spaceX, spaceY].HasUnactuatedTile && Main.tileSolid[Main.tile[spaceX, spaceY].TileType]) {
										canSpawn = false;
										break;
									}
									if (Main.tile[spaceX, spaceY].LiquidType == LiquidID.Lava) {
										canSpawn = false;
										break;
									}
								}
							}
						}

						if (x >= minLeft && x <= minRight) {
							canSpawn = false;
							break;
						}
					}
				}

				if (canSpawn)
					break;
			}

			if (canSpawn) {
				if (Main.netMode == NetmodeID.MultiplayerClient)
					SendSpawnReinforcements(Mod, NPC.whoAmI, target.whoAmI, x, y);
				else
					SpawnReinforcements(NPC.whoAmI, target.whoAmI, x, y);
			}
		}

		public static void SendSpawnReinforcements(Mod mod, int whoAmI, int target, int x, int y) {
			var packet = mod.GetPacket();
			packet.Write((short)ArknightsMod.ArkMessageID.SpawnReinforcements);
			packet.Write(whoAmI);
			packet.Write(target);
			packet.Write(x);
			packet.Write(y);
			packet.Send();
		}

		public static void ReadSpawnReinforcements(BinaryReader reader, int senderWhoAmI) {
			int sourceNpcIndex = reader.ReadInt32();
			int requestedTarget = reader.ReadInt32();
			int requestedX = reader.ReadInt32();
			int requestedY = reader.ReadInt32();

			if (Main.netMode != NetmodeID.Server || (uint)senderWhoAmI >= Main.maxPlayers)
				return;
			if ((uint)sourceNpcIndex >= Main.maxNPCs || requestedTarget != senderWhoAmI)
				return;
			if (!WorldGen.InWorld(requestedX, requestedY, 10))
				return;

			Player player = Main.player[senderWhoAmI];
			NPC sourceNpc = Main.npc[sourceNpcIndex];
			if (!player.active || player.dead || !sourceNpc.active || sourceNpc.type != ModContent.NPCType<Cannot>() || sourceNpc.ModNPC is not Cannot cannot)
				return;

			Vector2 requestedSpawnPosition = new(requestedX * 16f + 8f, requestedY * 16f);
			if (Vector2.DistanceSquared(player.Center, requestedSpawnPosition) > MaxReinforcementRequestDistancePixels * MaxReinforcementRequestDistancePixels)
				return;
			if (Vector2.DistanceSquared(player.Center, sourceNpc.Center) > MaxCannotInteractionDistancePixels * MaxCannotInteractionDistancePixels)
				return;
			if (cannot.summoncd > 0 || Isnpcexist)
				return;

			// 客户端给出的落点只用于拒绝明显伪造的包；真正的安全落点必须由服务器重新计算。
			cannot.TrySpawnReinforcements(player);
			cannot.summoncd = 600;
		}

		public static void SpawnReinforcements(int whoAmI, int target, int x, int y) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;
			if ((uint)whoAmI >= Main.maxNPCs || (uint)target >= Main.maxPlayers || !WorldGen.InWorld(x, y, 10))
				return;

			NPC npc = Main.npc[whoAmI];
			if (!npc.active || npc.type != ModContent.NPCType<Cannot>() || !Main.player[target].active)
				return;

			int type = Main.rand.Next(Eliteslist);
			NPC.NewNPC(npc.GetSource_FromThis(), x * 16 + 8, y * 16, type, Target: target);
		}

		public bool anyPlayerNearby = false;

		public override void AI() {
			if (summoncd > 0) {
				summoncd--;
			}

			if (TouchCount >= 5 && !Isnpcexist) {
				DoRunaway();
				return;
			}
		}

		public void Despawn() {
			NPC.active = false;
			NPC.netUpdate = true;
		}

		public void DoRunaway() {
			Runaway = true;
			var hit = new NPC.HitInfo() { InstantKill = true };
			NPC.StrikeNPC(hit);
			if (Main.netMode != NetmodeID.SinglePlayer)
				NetMessage.SendStrikeNPC(NPC, hit);
		}

		public override LocalizedText DeathMessage => Language.GetText("Mods.ArknightsMod.NPCs.Cannot.DeathMessage.Runaway");

		public override bool ModifyDeathMessage(ref NetworkText customText, ref Color color) {
			if (Runaway) {
				color = Color.LightBlue;
				return base.ModifyDeathMessage(ref customText, ref color);
			}
			return false;
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			// 被玩家击杀（非逃走）：掉落更多源石锭（5~8）
			npcLoot.Add(ItemDropRule.ByCondition(new CannotDead(), ModContent.ItemType<OriginiumIngot>(), 1, 5, 8));
			// 逃走掉落的“当前售卖藏品”改为在 OnKill 中确定性掉落 —— 逃走是自伤 InstantKill，
			// 常规掉落规则(NPCLoot)在该路径下不一定会解析，导致藏品掉不出来。
		}

		public override void OnChatButtonClicked(bool firstButton, ref string shop) {
			if (firstButton) {
				shop = ShopName;
				return;
			}
			else {
				if (Isnpcexist) {
					Main.npcChatText = Language.GetTextValue("Mods.ArknightsMod.Dialogue.Cannot.Touchcd");
				}
				else {
					TouchCount++;
					Main.LocalPlayer.GetModPlayer<CannotAggroPlayer>().AcknowledgeCannotTouchGoodsDialogue();
					TrySpawnReinforcements(Main.LocalPlayer);
					if (TouchCount < 5)
						Main.npcChatText = Language.GetTextValue($"Mods.ArknightsMod.Dialogue.Cannot.Touch{TouchCount}");
				}
			}
		}

		public override void AddShops() {
			Shop = new CannotShop();
			Shop.Register();
		}

		public override void OnSpawn(IEntitySource source) {
			NPCShopSystem.TryUpdateCannotShop(Mod, true);
		}

		public override void ModifyActiveShop(string shopName, Item[] items) {
			NPCShopSystem.TryUpdateCannotShop(Mod);
			Array.Fill(items, null);
			// 直接克隆完整的 Item 对象,保留 shopSpecialCurrency 等所有属性
			Item[] shopItems = [.. NPCShopSystem.CannotShopItems.Select(i => i.Clone())];
			for (int i = 0; i < items.Length && i < shopItems.Length; i++) {
				items[i] = shopItems[i]?.Clone();
			}
		}

		public override bool CanChat() {
			return true;
		}

		public override void TownNPCAttackStrength(ref int damage, ref float knockback) {
			damage = 30;
			knockback = 4f;
		}

		public override void TownNPCAttackCooldown(ref int cooldown, ref int randExtraCooldown) {
			cooldown = 30;
			randExtraCooldown = 30;
		}

		public static int RespawnCooldown {
			get => CannotSpawnHelper.RespawnCooldown;
			set => CannotSpawnHelper.RespawnCooldown = value;
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo) {
			foreach (var npc in Main.ActiveNPCs) {
				if (npc.type == Type)
					return base.SpawnChance(spawnInfo);
			}
			// 死亡后的重生冷却：RespawnCooldown 在 CheckDead() 中被设为 7200，
			// 此前一直只在 CannotSpawnHelper 里递减，但没有任何地方真正用它阻止重新生成，
			// 导致坎诺特死亡后下一刻就可能被重新刷出来。
			if (RespawnCooldown > 0)
				return 0f;
			if (!spawnInfo.Invasion && !spawnInfo.Sky && (NPC.downedBoss1 || NPC.downedBoss2 || NPC.downedBoss3))
				return 0.2f;
			return base.SpawnChance(spawnInfo);
		}

		public override bool CheckDead() {
			RespawnCooldown = 7200;
			ModContent.GetInstance<CannotLifeGateSystem>().OnCannotDied();
			return base.CheckDead();
		}

		public override void OnKill() {
			// 逃走时确定性掉落一件“当前正在售卖的藏品”（服务器/单机权威，Item.NewItem 自动同步）
			if (!Runaway || Main.netMode == NetmodeID.MultiplayerClient)
				return;

			var shopItems = NPCShopSystem.CannotShopItems;
			if (shopItems.Count == 0) {
				// 兜底：若尚未生成商店内容，强制刷新一次
				NPCShopSystem.TryUpdateCannotShop(Mod, true);
				shopItems = NPCShopSystem.CannotShopItems;
			}
			if (shopItems.Count > 0) {
				int type = shopItems[Main.rand.Next(shopItems.Count)].type;
				Item.NewItem(NPC.GetSource_Death(), NPC.getRect(), type, 1);
			}
		}
	}

	public class CannotShop() : AbstractNPCShop(ModContent.NPCType<Cannot>())
	{
		public new record Entry(Item Item, List<Condition> Conditions) : AbstractNPCShop.Entry
		{
			IEnumerable<Condition> AbstractNPCShop.Entry.Conditions => Conditions;

			public bool Disabled { get; private set; }

			public Entry Disable() {
				Disabled = true;
				return this;
			}

			public bool ConditionsMet() => Conditions.All(c => c.IsMet());
		}

		public record Pool(string Name, int Slots, List<Entry> Entries)
		{
			public Pool Add(Item item, params Condition[] conditions) {
				Entries.Add(new Entry(item, conditions.ToList()));
				return this;
			}

			public Pool Add<T>(params Condition[] conditions) where T : ModItem => Add(ModContent.ItemType<T>(), conditions);
			public Pool Add(int item, params Condition[] conditions) => Add(ContentSamples.ItemsByType[item], conditions);

			// Picks a number of items (up to Slots) from the entries list, provided conditions are met.
			public IEnumerable<Item> PickItems() {
				// This is not a fast way to pick items without replacement, but it's certainly easy. Be careful not to do this many many times per frame, or on huge lists of items.
				var list = Entries.Where(e => !e.Disabled && e.ConditionsMet()).ToList();
				for (int i = 0; i < Slots; i++) {
					if (list.Count == 0)
						break;

					int k = Main.rand.Next(list.Count);
					yield return list[k].Item;

					// remove the entry from the list so it can't be selected again this pick
					list.RemoveAt(k);
				}
			}
		}

		public List<Pool> Pools { get; } = [];

		public override IEnumerable<Entry> ActiveEntries => Pools.SelectMany(p => p.Entries).Where(e => !e.Disabled);

		public Pool AddPool(string name, int slots) {
			var pool = new Pool(name, slots, []);
			Pools.Add(pool);
			return pool;
		}

		public Pool AddPoolFromNameSpace(string name, int slots, string fromNamespace, Mod mod) {
			var pool = new Pool(name, slots, []);

			var items = mod.GetContent<ModItem>()
				.Where(item => item.GetType().Namespace == fromNamespace)
				.ToList();

			if (items.Count == 0) {
				Main.NewText($"[CannotShop] 警告：命名空间 '{fromNamespace}' 中未找到任何 ModItem！", Color.OrangeRed);
			}

			foreach (var modItem in items) {
				var shopItem = new Item(modItem.Type) {
					shopSpecialCurrency = ArknightsMod.OriginiumIngotCurrencyId
				};
				pool.Add(shopItem); // 调用 Add(Item, Condition[])
			}

			Pools.Add(pool);
			return pool;
		}

		// Some methods to add a pool with a single item
		public void Add(Item item, params Condition[] conditions) => AddPool(item.ModItem?.FullName ?? $"Terraria/{item.type}", slots: 1).Add(item, conditions);
		public void Add<T>(params Condition[] conditions) where T : ModItem => Add(ModContent.ItemType<T>(), conditions);
		public void Add(int item, params Condition[] conditions) => Add(ContentSamples.ItemsByType[item], conditions);

		// Here is where we actually 'roll' the contents of the shop
		public List<Item> GenerateNewInventoryList() {
			var items = new List<Item>();
			foreach (var pool in Pools) {
				items.AddRange(pool.PickItems());
			}
			return items;
		}

		public override void FillShop(ICollection<Item> items, NPC npc) {
			// use the items which were selected when the NPC spawned.
			foreach (var item in Cannot.shopItems) {
				// make sure to add a clone of the item, in case any ModifyActiveShop hooks adjust the item when the shop is opened
				items.Add(item.Clone());
			}
		}

		public override void FillShop(Item[] items, NPC npc, out bool overflow) {
			overflow = false;
			int i = 0;
			// use the items which were selected when the NPC spawned.
			foreach (var item in Cannot.shopItems) {

				if (i == items.Length - 1) {
					// leave the last slot empty for selling
					overflow = true;
					return;
				}

				// make sure to add a clone of the item, in case any ModifyActiveShop hooks adjust the item when the shop is opened
				items[i++] = item.Clone();
			}
		}
	}

	internal class Cannot_DorpCollection : IItemDropRule
	{
		readonly CannotRunaway condition = new();

		public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; } = [];

		public bool CanDrop(DropAttemptInfo info) => condition.CanDrop(info);

		public void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo) {
			// 逃走时随机掉落一件“当前正在售卖的藏品”（即当前坎诺特商店内容）
			var shopItems = NPCShopSystem.CannotShopItems;
			float perItemRate = shopItems.Count > 0 ? ratesInfo.parentDroprateChance / shopItems.Count : 0f;
			for (int i = 0; i < shopItems.Count; i++)
				drops.Add(new DropRateInfo(shopItems[i].type, 1, 1, perItemRate, ratesInfo.conditions));
			Chains.ReportDroprates(ChainedRules, 1, drops, ratesInfo);
		}

		public ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info) {
			var shopItems = NPCShopSystem.CannotShopItems;
			if (shopItems.Count > 0) {
				int type = shopItems[info.rng.Next(shopItems.Count)].type;
				CommonCode.DropItem(info, type, 1);
			}
			ItemDropAttemptResult result = default;
			result.State = ItemDropAttemptResultState.Success;
			return result;
		}
	}

	internal class CannotRunaway : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info) {
			if (info.npc.ModNPC is Cannot cannot)
				return cannot.Runaway;
			return false;
		}

		public bool CanShowItemDropInUI() => true;

		public string GetConditionDescription() => Language.GetTextValue("Mods.ArknightsMod.ItemDropRuleCondition.CannotRunaway");
	}

	internal class CannotDead : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info) {
			if (info.npc.ModNPC is Cannot cannot)
				return !cannot.Runaway;
			return false;
		}

		public bool CanShowItemDropInUI() => true;

		public string GetConditionDescription() => Language.GetTextValue("Mods.ArknightsMod.ItemDropRuleCondition.CannotDead");
	}

	internal class CannotSpawnHelper : ModSystem
	{
		public static int RespawnCooldown;

		public override void PreUpdateNPCs() {
			if (RespawnCooldown > 0)
				RespawnCooldown--;
		}
	}
}
