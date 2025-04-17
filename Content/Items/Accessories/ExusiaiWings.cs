using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.CodeAnalysis;
using System;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace ArknightsMod.Content.Items.Accessories
{
	[AutoloadEquip(EquipType.Wings)]
	public class ExusiaiWings : ModItem
	{
		public override void SetStaticDefaults() {
			// DisplayName.SetDefault("Exusiai's wing");
			// Tooltip.SetDefault("Apple pie!");
			
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

			// These wings use the same values as the solar wings
			// Fly time: 180 ticks = 3 seconds
			// Fly speed: 9
			// Acceleration multiplier: 2.5
			ArmorIDs.Wing.Sets.Stats[Item.wingSlot] = new WingStats(180, 9f, 2.5f);
		}

		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 28;
			Item.value = Item.sellPrice(0, 3, 0, 0);
			Item.rare = ItemRarityID.Yellow;
			Item.accessory = true;
		}


		public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising,
			ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) {
			ascentWhenFalling = 0.85f; // Falling glide speed
			ascentWhenRising = 0.15f; // Rising speed
			maxCanAscendMultiplier = 1f;
			maxAscentMultiplier = 3f;
			constantAscend = 0.135f;
		}

		//public override bool WingUpdate(Player player, bool inUse)
		//{
		//    Vector2 position = new Vector2(player.position.X, player.position.Y);
		//    Lighting.AddLight(position, 0.5f, 0.5f, 0.5f);
		//    return true;
		//}


		public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI) {
			Texture2D texture = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Accessories/ExusiaiWings_Glowmask", AssetRequestMode.ImmediateLoad).Value;
			spriteBatch.Draw
			(
				texture,
				new Vector2
				(
					Item.position.X - Main.screenPosition.X + Item.width * 0.5f,
					Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f + 2f
				),
				new Rectangle(0, 0, texture.Width, texture.Height),
				Color.White,
				rotation,
				texture.Size() * 0.5f,
				scale,
				SpriteEffects.None,
				0f
			);
		}

		// Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
		//      public override void AddRecipes() {
		//	CreateRecipe()
		//		.AddIngredient<Material.OrirockConcentration>(3)
		//		.AddTile(TileID.WorkBenches)
		//		.SortBefore(Main.recipe.First(recipe => recipe.createItem.wingSlot != -1)) // Places this recipe before any wing so every wing stays together in the crafting menu.
		//		.Register();
		//}
	}
}
