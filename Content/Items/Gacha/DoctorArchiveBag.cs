using System;
using System.Collections.Generic;
using ArknightsMod.Content.Items.Armor;
using ArknightsMod.Players;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Gacha
{
	public class DoctorArchiveBag : ModItem
	{
		public override void SetStaticDefaults()
		{
			Item.ResearchUnlockCount = 1;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
			tooltips.Add(new TooltipLine(Mod, "DoctorArchivePool", "可抽出常驻及限定干员时装"));

			try
			{
				var player = Main.LocalPlayer;
				if (player is null)
					return;

				var gacha = player.GetModPlayer<DoctorArchiveGachaPlayer>();
				int remaining = Utils.Clamp(90 - gacha.PullsSinceLastSixStar, 1, 90);
				string text = $"距离6星保底：还差 {remaining} 抽";
				tooltips.Add(new TooltipLine(Mod, "DoctorArchivePity", text));
			}
			catch
			{
			}
		}

		public override void SetDefaults()
		{
			Item.maxStack = 9999;
			Item.consumable = true;
			Item.width = 30;
			Item.height = 56;
			Item.rare = ItemRarityID.White;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.UseSound = SoundID.Item4;
		}

		private static Dictionary<int, List<int>> _rarityBags;

		private static Dictionary<int, List<int>> GetRarityBags()
		{
			if (_rarityBags != null)
				return _rarityBags;

			_rarityBags = new();

			foreach (var bag in ModContent.GetContent<ArknightsVanityBag>())
			{
				if (bag.ObtainType == ArknightsVanityBag.ObtainTypes.NoGacha)
					continue;
				int r = bag.Rarity;
				if (!_rarityBags.TryGetValue(r, out var list))
				{
					list = new();
					_rarityBags[r] = list;
				}
				list.Add(bag.Type);
			}

			return _rarityBags;
		}

		private static bool IsShiftDown()
		{
			var state = Main.keyState;
			return state.IsKeyDown(Keys.LeftShift) || state.IsKeyDown(Keys.RightShift);
		}

		public override bool CanRightClick()
		{
			if (Item.stack <= 0)
				return false;

			bool shift = IsShiftDown();

			if (shift)
				return Item.stack >= 10;

			return true;
		}

		public override void RightClick(Player player)
		{
			if (player.whoAmI != Main.myPlayer)
				return;

			bool shift = IsShiftDown();
			int pulls = shift ? 10 : 1;

			int originalStack = Item.stack + 1;
			if (originalStack < pulls)
				return;

			int extraToConsume = pulls - 1;
			if (extraToConsume > 0)
			{
				if (Item.stack < extraToConsume)
					return;
				Item.stack -= extraToConsume;
				if (Item.stack <= 0)
					Item.TurnToAir();
			}

			DoPulls(player, pulls);
		}

		private void DoPulls(Player player, int pulls)
		{
			var gacha = player.GetModPlayer<DoctorArchiveGachaPlayer>();
			if (pulls >= 10)
				gacha.BeginTenPull();

			for (int i = 0; i < pulls; i++)
			{
				int stars = RollStars(gacha, pulls >= 10);
				int bagType = RollBagType(stars);
				if (bagType > 0)
					GiveToInventoryOrDrop(player, player.GetSource_OpenItem(Type), bagType, 1);
				gacha.RegisterPull(stars);
			}
		}

		private static int RollBagType(int stars)
		{
			// 从全部可抽袋子中均匀随机，保证每抽必出且各干员概率相等。
			// 星级保底计数依然由 RollStars 和 RegisterPull 维护，不影响这里的选袋逻辑。
			var allBags = new List<int>();
			foreach (var list in GetRarityBags().Values)
				allBags.AddRange(list);
			return allBags.Count > 0 ? allBags[Main.rand.Next(allBags.Count)] : 0;
		}

		private int RollStars(DoctorArchiveGachaPlayer gacha, bool isTenPull)
		{
			if (gacha.PullsSinceLastSixStar >= 89)
				return 6;

			bool forceAtLeastFiveStar =
				isTenPull &&
				gacha.PullsInCurrentTenPull == 9 &&
				!gacha.TenPullHadAtLeastFiveStar;

			if (forceAtLeastFiveStar)
			{
				int highRoll = Main.rand.Next(20);
				return highRoll < 4 ? 6 : 5;
			}

			int roll = Main.rand.Next(100);
			if (roll < 4)
				return 6;
			if (roll < 20)
				return 5;
			if (roll < 60)
				return 4;
			return 3;
		}

		private static void GiveToInventoryOrDrop(Player player, IEntitySource source, int itemType, int stack, int? overrideRarity = null)
		{
			if (stack <= 0)
				return;

			Item item = new(itemType, stack);
			if (overrideRarity.HasValue)
				item.rare = overrideRarity.Value;

			int remaining = item.stack;

			for (int i = 0; i < player.inventory.Length; i++)
			{
				if (remaining <= 0)
					break;

				Item inv = player.inventory[i];
				if (inv is null || inv.IsAir)
					continue;
				if (inv.type != itemType)
					continue;
				if (inv.stack >= inv.maxStack)
					continue;

				int move = Math.Min(remaining, inv.maxStack - inv.stack);
				inv.stack += move;
				remaining -= move;
			}

			for (int i = 0; i < player.inventory.Length; i++)
			{
				if (remaining <= 0)
					break;

				Item inv = player.inventory[i];
				if (inv is not null && !inv.IsAir)
					continue;

				int give = Math.Min(remaining, item.maxStack);
				Item newItem = new(itemType, give);
				if (overrideRarity.HasValue)
					newItem.rare = overrideRarity.Value;
				player.inventory[i] = newItem;
				remaining -= give;
			}

			if (remaining > 0)
			{
				Item.NewItem(source, player.getRect(), itemType, remaining);
			}
		}
	}
}
