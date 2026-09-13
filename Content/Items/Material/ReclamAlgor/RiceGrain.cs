using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material.ReclamAlgor
{
	/// <summary>
	/// 稻谷
	/// </summary>
	public class RiceGrain : ModItem
	{
		public override void SetDefaults()
		{
			Item.height = Item.width = 32;
			Item.rare = ItemRarityID.Yellow;
			Item.maxStack = 9999;
		}
		public override bool CanUseItem(Player player) => false;
	}
}
