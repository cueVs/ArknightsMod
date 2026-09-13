using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor
{
	public abstract class ArknightsVanityBag : ModItem
	{
		public enum ObtainTypes {
			/// <summary>常驻干员（明日方舟）</summary>
			Default,
			/// <summary>限定干员（明日方舟，周年庆典）</summary>
			Limited_Celebration,
			/// <summary>限定干员（明日方舟，夏季庆典）</summary>
			Limited_Carnival,
			/// <summary>限定干员（明日方舟，春节庆典）</summary>
			Limited_Festival,
			/// <summary>限定干员（明日方舟，联动）</summary>
			Limited_CrossOver,
			/// <summary>常驻干员（终末地）</summary>
			EndfieldDefault,
			/// <summary>特殊，比如博士/阿米娅/管理员套装，无法通过 Gacha 获得</summary>
			NoGacha,
		}

		public virtual ObtainTypes ObtainType => ObtainTypes.Default;

		/// <summary> 干员的星数，请输入1~6（必须由子类重写）</summary>
		public abstract int Rarity { get; }

		protected abstract List<int> GetItems();

		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.maxStack = 9999;
			Item.consumable = true;
			Item.width = 30;
			Item.height = 56;
			Item.rare = ItemRarityID.White;
		}

		public override bool CanRightClick() {
			return Item.stack == 1;
		}

		public override void ModifyItemLoot(ItemLoot itemLoot)
		{
			var items = GetItems();
			if (items == null || items.Count == 0)
				return;

			IItemDropRule rule = ItemDropRule.Common(items[0], 1);
			IItemDropRule currentRule = rule;

			for (int i = 1; i < items.Count; i++) {
				IItemDropRule nextRule = ItemDropRule.Common(items[i], 1);
				currentRule.OnSuccess(nextRule);
				currentRule = nextRule;
			}

			itemLoot.Add(rule);
		}
	}
}
