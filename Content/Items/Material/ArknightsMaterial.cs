using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	public abstract class ArknightsMaterial : ModItem
	{
		/// <summary>
		/// 材料稀有度，0~4分别对应 白，绿，蓝，紫，金材料
		/// </summary>
		public virtual int Rarity => 0;
		public sealed override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 100;
			SafeSetStaticDefaults();
		}

		public sealed override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;

			Item.maxStack = Item.CommonMaxStack;
			Item.rare = GetRarity(Rarity);
			Item.value = GetValue(Rarity);

			SafeSetDefaults();
		}

		public virtual void SafeSetDefaults() { }
		public virtual void SafeSetStaticDefaults() { }

		public static int GetRarity(int rarity) {
			rarity = Math.Clamp(rarity, 0, 5);
			int result = rarity switch {
				0 => ItemRarityID.White,
				1 => ItemRarityID.Green,
				2 => ItemRarityID.Cyan,
				3 => ItemRarityID.LightPurple,
				4 => ItemRarityID.Quest,
				_ => ItemRarityID.White
			};
			return result;
		}

		public static int GetValue(int rarity) {
			rarity = Math.Clamp(rarity, 0, 5);
			int result = rarity switch {
				0 => Item.sellPrice(0, 0, 0, 50),//0
				1 => Item.sellPrice(0, 0, 2, 00),//1=4*0
				2 => Item.sellPrice(0, 0, 8, 00),//2=4*1
				3 => Item.sellPrice(0, 0, 32, 0),//3=4*2
				4 => Item.sellPrice(0, 1, 28, 0),//4=4*3
				_ => Item.sellPrice(0, 0, 0, 50)
			};
			return result;
		}
	}
}
