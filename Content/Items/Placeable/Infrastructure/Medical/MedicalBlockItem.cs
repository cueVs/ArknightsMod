using ArknightsMod.Content.Tiles.Infrastructure.Medical;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Medical
{
	public class MedicalBlockItem : ArknightsInfraBlock
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<MedicalBlockTile>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}