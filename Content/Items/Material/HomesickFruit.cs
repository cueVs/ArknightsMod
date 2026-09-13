using ArknightsMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	// 可以重新栽种（DefaultToPlaceableTile）：种下时的估价会被写进对应的 HomesickFruitTileEntity，
	// 之后每过一个游戏日回涨2点，采下来的时候再按 TileEntity 当时的估价现掉出物品。
	public class HomesickFruit : RareCollectibleItem
	{
		public override int BaseOriginiumIngotValue => HomesickFruitPlayer.BaseValue;

		public override void SafeSetCollectibleDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Natural.HomesickFruit>());
		}

		public override void UpdateInventory(Player player) {
			var fruitPlayer = player.GetModPlayer<HomesickFruitPlayer>();
			fruitPlayer.HasHomesickFruit = true;
			Item.value = fruitPlayer.Value;
		}
	}
}
