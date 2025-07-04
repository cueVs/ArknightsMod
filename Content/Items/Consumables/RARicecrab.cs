using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;
using Terraria.ModLoader.IO;
using System.IO;
using System.Collections.Generic;
using Terraria.GameContent.ItemDropRules;
using Terraria.Utilities;
using Terraria.Localization;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using ArknightsMod.Content.Buffs;

namespace ArknightsMod.Content.Items.Consumables
{

	public class RARicecrab:ModItem
	{
		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 5;

			// This is to show the correct frame in the inventory
			// The MaxValue argument is for the animation speed, we want it to be stuck on frame 1
			// Setting it to max value will cause it to take 414 days to reach the next frame
			// No one is going to have game open that long so this is fine
			// The second argument is the number of frames, which is 3
			// The first frame is the inventory texture, the second frame is the holding texture,
			// and the third frame is the placed texture
			Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));

			// This allows you to change the color of the crumbs that are created when you eat.
			// The numbers are RGB (Red, Green, and Blue) values which range from 0 to 255.
			// Most foods have 3 crumb colors, but you can use more or less if you desire.
			// Depending on if you are making solid or liquid food switch out FoodParticleColors
			// with DrinkParticleColors. The difference is that food particles fly outwards
			// whereas drink particles fall straight down and are slightly transparent
			ItemID.Sets.FoodParticleColors[Item.type] = new Color[3] {
				new Color(249, 230, 136),
				new Color(152, 93, 95),
				new Color(174, 192, 192)
			};

			ItemID.Sets.IsFood[Type] = true; //This allows it to be placed on a plate and held correctly
		}

		public override void SetDefaults() {
			// This code matches the ApplePie code.

			// DefaultToFood sets all of the food related item defaults such as the buff type, buff duration, use sound, and animation time.
			Item.DefaultToFood(32, 32, BuffID.WellFed2, 14400); // 57600 is 16 minutes: 16 * 60 * 60
			Item.value = Item.buyPrice(0, 3);
			Item.rare = ItemRarityID.Green;
		}

		// If you want multiple buffs, you can apply the remainder of buffs with this method.
		// Make sure the primary buff is set in SetDefaults so that the QuickBuff hotkey can work properly.
		public override void OnConsumeItem(Player player) {
			for (int i = 0; i < player.buffType.Length; i++) {
				foreach (var type in RAfood.RAfoodBuff) {
					if (type == player.buffType[i]) {
						player.buffTime[i] = 0;
					}
				}
			}
			player.AddBuff(ModContent.BuffType<RARicecrabBuff>(), 14400);
		}
		
		


	}
}
