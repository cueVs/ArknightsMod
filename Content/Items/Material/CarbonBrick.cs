using Terraria;

namespace ArknightsMod.Content.Items.Material
{
	public class CarbonBrick : ArknightsMaterial
	{
		public override int Rarity => 2;
		public override void SafeSetDefaults() {
			Item.value = Item.buyPrice(0, 0, 12, -76);
		}
		public override void AddRecipes() {
		}
	}
}
