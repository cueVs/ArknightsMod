using ArknightsMod.Content.Tiles.Infrastructure.Medical;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Placeable.Infrastructure.Medical
{
	public class MedicalWallItem : ArknightsInfraBlock
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableWall(ModContent.WallType<MedicalWall>());
			Item.value = Item.sellPrice(0, 0, 0, 30);
		}
	}
}