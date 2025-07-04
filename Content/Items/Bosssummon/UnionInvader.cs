using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using ArknightsMod.Content.Events;

namespace ArknightsMod.Content.Items.Bosssummon
{
	public class UnionInvader : ModItem, ILocalizedModType
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 1;
			Item.useTime = 30;
			Item.useAnimation = 30;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.consumable = true;
			Item.rare = ItemRarityID.Orange;
			Item.value = Item.sellPrice(0, 1, 0, 0);
		}

		public override bool CanUseItem(Player player) {
			return !UnionInvade.EventActive && Main.dayTime;
		}

		public override bool? UseItem(Player player) {
			if (player.whoAmI == Main.myPlayer) {
				UnionInvade.StartEvent();
			}
			return true;
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient(ItemID.Wood, 3)
				.AddTile(TileID.DemonAltar)
				.Register();
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			tooltips.Add(new TooltipLine(Mod, "EventInfo", "只能在白昼使用"));
		}


	}
}
