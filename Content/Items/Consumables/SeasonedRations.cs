using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using ArknightsMod.Content.Buffs;
using ArknightsMod.Content.Items.Material.ReclamAlgor;

namespace ArknightsMod.Content.Items.Consumables
{

	public class SeasonedRations : ModItem
	{
		public override void SetStaticDefaults() {
			

			ItemID.Sets.IsFood[Type] = true;
		}

		public override void SetDefaults() {
			
			Item.DefaultToFood(32, 32, BuffID.WellFed2, 21600); 
			Item.value = Item.buyPrice(0, 3);
			Item.rare = ItemRarityID.Green;
		}

		public override void OnConsumeItem(Player player) {
			for (int i = 0; i < player.buffType.Length; i++) {
				foreach (var type in RAfood.RAfoodBuff) {
					if (type == player.buffType[i]) {
						player.buffTime[i] = 0;
					}
				}
			}
			player.AddBuff(ModContent.BuffType<SeasonedRationsBuff>(), 21600);
		}


		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<Content.Items.Material.ReclamAlgor.RiceGrain>(1);
			recipe.AddIngredient<Content.Items.Material.ReclamAlgor.RAMeat>(2);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}