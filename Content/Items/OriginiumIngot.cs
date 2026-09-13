using ArknightsMod.Players;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items
{
	public class OriginiumIngot : ModItem
	{
		public override void SetStaticDefaults() {
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(8, 4));
			ItemID.Sets.AnimatesAsSoul[Type] = true;
			ItemID.Sets.CoinLuckValue[Type] = 325;
		}

		public override void SetDefaults() {
			Item.width = 14;
			Item.height = 20;
			Item.maxStack = Item.CommonMaxStack;
			Item.rare = ItemRarityID.Green;
		}

		public override bool OnPickup(Player player) {
			int num = (int)MathHelper.Clamp(Item.stack, 0, (int)Math.Round(Math.Log2(Math.Max(Item.stack - 5, 5) - 5)) + 5);
			player.GetModPlayer<InventoryPlayer>().JustPickupOriginiumIngot += num;
			return base.OnPickup(player);
		}
	}
}
