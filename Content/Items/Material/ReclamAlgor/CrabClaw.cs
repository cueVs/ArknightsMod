using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material.ReclamAlgor
{
	/// <summary>
	/// 蟹钳
	/// </summary>
	public class CrabClaw : ModItem
	{
		public override void SetDefaults()
		{
			Item.height = 30;
			Item.width = 22;
			Item.rare = ItemRarityID.Yellow;
			Item.maxStack = 99999;
		}
		public override bool CanUseItem(Player player) => false;
	}
}
