using ArknightsMod.Content.Items.Armor.Caster.Amiya;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Content.Items.Material;
using ArknightsMod.Content.Items.Material.ReclamAlgor;
using ArknightsMod.Content.NPCs.Friendly;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Systems
{
	public class NPCShopSystem : ModSystem
	{
		public static List<int> ClosureTodaysRotation = [];
		public static List<int> ClosureMaterialRotation = [];

		/// <summary>
		/// 永远常驻售卖、不参与每日随机刷新的材料。碳素条是基建/摆件配方的常用材料，
		/// 放进随机池会被别的材料挤掉，专门给它留一个不刷新的固定位置。
		/// 不放进 <see cref="BuildClosureMaterialPool"/>，避免被随机抽走后又被移除。
		/// </summary>
		public static List<int> BuildClosurePinnedMaterials() => [
			ModContent.ItemType<CarbonBrick>(),
		];

		public static List<int> BuildClosureMaterialPool(bool forceAllTiers = false) {
			var pool = new List<int> {
				ModContent.ItemType<Orirock>(),
				ModContent.ItemType<OrironShard>(),
				ModContent.ItemType<SugarSubstitute>(),
				ModContent.ItemType<Polyester>(),
				ModContent.ItemType<Diketon>(),
				ModContent.ItemType<Ester>(),
				ModContent.ItemType<DamagedDevice>(),
				ModContent.ItemType<CrabClaw>(),
				ModContent.ItemType<RAWater>(),
				ModContent.ItemType<RAMeat>(),
				ModContent.ItemType<RiceGrain>(),
				ModContent.ItemType<RALegmeat>(),
			};

			if (forceAllTiers || NPC.downedBoss1) {
				pool.AddRange([
					ModContent.ItemType<Oriron>(),
					ModContent.ItemType<Sugar>(),
					ModContent.ItemType<Polyketon>(),
					ModContent.ItemType<Device>(),
					ModContent.ItemType<OrirockCube>(),
					ModContent.ItemType<OriginiumShard>(),
					ModContent.ItemType<CorruptedRecord>(),
				]);
			}

			if (forceAllTiers || NPC.downedBoss3) {
				pool.AddRange([
					ModContent.ItemType<ManganeseOre>(),
					ModContent.ItemType<Grindstone>(),
					ModContent.ItemType<LoxicKohl>(),
					ModContent.ItemType<RMA7012>(),
					ModContent.ItemType<OrirockCluster>(),
					ModContent.ItemType<SugarPack>(),
					ModContent.ItemType<PolyesterPack>(),
					ModContent.ItemType<CoagulatingGel>(),
					ModContent.ItemType<OrironCluster>(),
					ModContent.ItemType<Aketon>(),
					ModContent.ItemType<IntegratedDevice>(),
					ModContent.ItemType<IncandescentAlloy>(),
					ModContent.ItemType<CrystallineComponent>(),
					ModContent.ItemType<CompoundCuttingFluid>(),
					ModContent.ItemType<TransmutedSalt>(),
					ModContent.ItemType<SemiSyntheticSolvent>(),
					ModContent.ItemType<CoagulativeNodule>(),
					ModContent.ItemType<FuscousFiber>(),
					ModContent.ItemType<AggregateCyclicene>(),
				]);
			}

			if (forceAllTiers || (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3)) {
				pool.AddRange([
					ModContent.ItemType<ManganeseTrihydrate>(),
					ModContent.ItemType<GrindstonePentahydrate>(),
					ModContent.ItemType<WhiteHorseKohl>(),
					ModContent.ItemType<RMA7024>(),
					ModContent.ItemType<OrirockConcentration>(),
					ModContent.ItemType<SugarLump>(),
					ModContent.ItemType<PolyesterLump>(),
					ModContent.ItemType<PolymerizedGel>(),
					ModContent.ItemType<CyclicenePrefab>(),
					ModContent.ItemType<ChiralRefractor>(),
					ModContent.ItemType<OrironBlock>(),
					ModContent.ItemType<KetonColloid>(),
					ModContent.ItemType<OptimizedDevice>(),
					ModContent.ItemType<IncandescentAlloyBlock>(),
					ModContent.ItemType<CrystallineCircuit>(),
					ModContent.ItemType<CuttingFluidSolution>(),
					ModContent.ItemType<RefinedSolvent>(),
					ModContent.ItemType<TransmutedSaltAgglomerate>(),
					ModContent.ItemType<SolidifiedFiberBoard>(),
				]);
			}

			if (forceAllTiers || NPC.downedPlantBoss) {
				pool.AddRange([
					ModContent.ItemType<RephasicEnantiomer>(),
					ModContent.ItemType<PolymerizationPreparation>(),
					ModContent.ItemType<D32Steel>(),
					ModContent.ItemType<CrystallineElectronicUnit>(),
					ModContent.ItemType<BipolarNanoflake>(),
					ModContent.ItemType<NucleicCrystalSinter>(),
				]);
			}

			return pool;
		}
		// 淇濆瓨瀹屾暣鐨?Item 瀵硅薄鑰屼笉鏄彧淇濆瓨 type
		public static List<Item> CannotShopItems = [];
		public static int OldCannotShopCount;

		public override void Load() {
			On_Main.UpdateTime_StartDay += On_Main_UpdateTime_StartDay;
		}

		private void On_Main_UpdateTime_StartDay(On_Main.orig_UpdateTime_StartDay orig, ref bool stopEvents) {
			orig(ref stopEvents);
			UpdateClosureShop(Mod);
		}

		public static void UpdateClosureShop(Mod mod, bool firstTime = false) {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				int amiyaType = ModContent.ItemType<AmiyaDefault>();
				var sixStars = new List<int>();
				var others = new List<int>();

				foreach (var bag in ModContent.GetContent<ArknightsVanityBag>()) {
					if (bag.Type == amiyaType) continue;
					if (bag.Rarity == 6)
						sixStars.Add(bag.Type);
					else
						others.Add(bag.Type);
				}

				ClosureTodaysRotation = [amiyaType];
				int rand = Main.rand.Next(3, 6);
				while (sixStars.Count > rand) {
					sixStars.RemoveAt(Main.rand.Next(sixStars.Count));
				}
				ClosureTodaysRotation.AddRange(sixStars);
				rand = Main.rand.Next(3, 6);
				while (others.Count > rand) {
					others.RemoveAt(Main.rand.Next(others.Count));
				}
				ClosureTodaysRotation.AddRange(others);

				var materialPool = BuildClosureMaterialPool();
				var pinnedMaterials = BuildClosurePinnedMaterials();
				// 常驻材料（碳素条）永远排在最前面、不参与随机抽取和刷新；
				// 随机部分的抽取数量（8~12 种）保持和迁移前一致，不因为常驻项而缩水。
				ClosureMaterialRotation = [.. pinnedMaterials];
				int materialCount = Main.rand.Next(8, 13) + pinnedMaterials.Count;
				while (materialPool.Count > 0 && ClosureMaterialRotation.Count < materialCount) {
					int idx = Main.rand.Next(materialPool.Count);
					ClosureMaterialRotation.Add(materialPool[idx]);
					materialPool.RemoveAt(idx);
				}

				if (Main.dedServ) {
					SendUpdateClosureShop(mod);
				}
				else if (!firstTime && Main.LocalPlayer.talkNPC > -1) {
					var closure = Main.npc[Main.LocalPlayer.talkNPC];
					if (closure.type == ModContent.NPCType<Closure>())
						Main.playerInventory = false;
				}
			}
			else
				RequestUpdateClosureShopWhenStartDay(mod);
			if (!firstTime)
				Main.NewText(Language.GetTextValue("Mods.ArknightsMod.StatusMessage.UpdateClosureShop"), Color.Yellow);
		}

		public static void TryUpdateCannotShop(Mod mod, bool forcedUpdate = false) {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				int countBeforeSkeletron = 1 +
					(NPC.downedSlimeKing ? 1 : 0) +//鍙茶幈濮?
					(NPC.downedBoss1 ? 1 : 0) +//鍏嬬溂
					(NPC.downedBoss2 ? 1 : 0) +//閭伓boss
					(NPC.downedDeerclops ? 1 : 0);//宸ㄩ箍


				int countBetweenSkeletronAndPlantera =
					(NPC.downedBoss3 ? 1 : 0) +//楠烽珔鐜?
					(NPC.downedQueenBee ? 1 : 0) +//铚傚悗
					(Main.hardMode ? 1 : 0) +//鑲夊北
					(NPC.downedMechBoss1 ? 1 : 0) +//鏈烘1
					(NPC.downedMechBoss2 ? 1 : 0) +//鏈烘2
					(NPC.downedMechBoss3 ? 1 : 0);//鏈烘3

				int countBetweenPlanteraAndDukeFishron =
					(NPC.downedPlantBoss ? 1 : 0) +//涓栬姳
					(NPC.downedGolemBoss ? 1 : 0);//鐭冲法浜?

				int countFromFishronOnward =
					(NPC.downedFishron ? 1 : 0) +//鐚波
					(NPC.downedEmpressOfLight ? 1 : 0) +//鍏夊コ
					(NPC.downedAncientCultist ? 1 : 0) +//鏁欏緬
					(NPC.downedMoonlord ? 1 : 0);//鏈堟€?

				int cannotShopCount = countBeforeSkeletron + countBetweenSkeletronAndPlantera + countBetweenPlanteraAndDukeFishron + countFromFishronOnward;
				if (!forcedUpdate && cannotShopCount == OldCannotShopCount)
					return;

				var tempShop = new CannotShop();
				if (countBeforeSkeletron > 0)
					tempShop.AddPoolFromNameSpace("Rogue.Rarity_l1", countBeforeSkeletron, "ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l1", mod);
				if (countBetweenSkeletronAndPlantera > 0)
					tempShop.AddPoolFromNameSpace("Rogue.Rarity_l2", countBetweenSkeletronAndPlantera, "ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l2", mod);
				if (countBetweenPlanteraAndDukeFishron > 0)
					tempShop.AddPoolFromNameSpace("Rogue.Rarity_l3", countBetweenPlanteraAndDukeFishron, "ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3", mod);
				if (countFromFishronOnward > 0)
					tempShop.AddPoolFromNameSpace("Rogue.Rarity_l4", countFromFishronOnward, "ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4", mod);

				// 淇濆瓨瀹屾暣鐨?Item 瀵硅薄
				CannotShopItems.Clear();
				CannotShopItems.AddRange(tempShop.GenerateNewInventoryList());
				foreach (var item in CannotShopItems)
					MakeCannotPriceEven(item);

				if (Main.dedServ)
					SendUpdateCannotShop(mod);

				OldCannotShopCount = cannotShopCount;
			}
			else
				RequestUpdateCannotShop(mod, forcedUpdate);
		}

		// 坎诺特的商品只能呈现偶数定价：OriginiumIngotCurrency 把 item.value/50000 取整作为显示价格（见 GetItemExpectedPrice），
		// 这里用同样的换算把价格向上调整为偶数后再换算回 item.value，使商店里显示的价格永远是偶数。
		public static void MakeCannotPriceEven(Item item) {
			long price = item.value / 50000;
			if (price == 0 && item.value > 0)
				price = 1;
			if (price % 2 != 0)
				price += 1;
			item.value = (int)(price * 50000);
		}

		public static void RequestUpdateClosureShopWhenStartDay(Mod mod) {
			var packet = mod.GetPacket();
			packet.Write((short)ArknightsMod.ArkMessageID.RequestUpdateClosureShopWhenStartDay);
			packet.Send(255);
		}

		public static void RequestUpdateCannotShop(Mod mod, bool forcedUpdate) {
			var packet = mod.GetPacket();
			packet.Write((short)ArknightsMod.ArkMessageID.RequestUpdateCannotShop);
			packet.Write(forcedUpdate);
			packet.Send(255);
		}

		public static void SendUpdateClosureShop(Mod mod) {
			var packet = mod.GetPacket();
			packet.Write((short)ArknightsMod.ArkMessageID.UpdateClosureShopWhenStartDay);
			packet.Write(ClosureTodaysRotation.Count);
			for (int i = 0; i < ClosureTodaysRotation.Count; i++) {
				packet.Write(ClosureTodaysRotation[i]);
			}
			packet.Write(ClosureMaterialRotation.Count);
			for (int i = 0; i < ClosureMaterialRotation.Count; i++) {
				packet.Write(ClosureMaterialRotation[i]);
			}
			packet.Send();
		}

		// 淇敼锛氬悓姝ヨ嚜瀹氫箟璐у竵淇℃伅
		public static void SendUpdateCannotShop(Mod mod) {
			var packet = mod.GetPacket();
			packet.Write((short)ArknightsMod.ArkMessageID.UpdateCannotShop);
			packet.Write(CannotShopItems.Count);
			for (int i = 0; i < CannotShopItems.Count; i++) {
				packet.Write(CannotShopItems[i].type);
				packet.Write(CannotShopItems[i].shopSpecialCurrency);
			}
			packet.Send();
		}

		public static void ReadUpdateClosureShop(BinaryReader reader) {
			ClosureTodaysRotation = [];
			ClosureMaterialRotation = [];
			try {
				int count = reader.ReadInt32();
				for (int i = 0; i < count; i++) {
					ClosureTodaysRotation.Add(reader.ReadInt32());
				}
				int materialCount = reader.ReadInt32();
				for (int i = 0; i < materialCount; i++) {
					ClosureMaterialRotation.Add(reader.ReadInt32());
				}
			}
			catch {
				ClosureTodaysRotation = [ModContent.ItemType<AmiyaDefault>()];
				ClosureMaterialRotation = [];
			}
		}

		// 璇诲彇骞舵仮澶嶈嚜瀹氫箟璐у竵淇℃伅
		public static void ReadUpdateCannotShop(BinaryReader reader) {
			CannotShopItems = [];
			try {
				int count = reader.ReadInt32();
				for (int i = 0; i < count; i++) {
					int type = reader.ReadInt32();
					int currency = reader.ReadInt32();
					var item = new Item(type) {
						shopSpecialCurrency = currency
					};
					MakeCannotPriceEven(item);
					CannotShopItems.Add(item);
				}
			}
			catch {
				CannotShopItems = [];
			}
		}
	}
}