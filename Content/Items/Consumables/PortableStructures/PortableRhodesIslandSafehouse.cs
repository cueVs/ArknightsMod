using Terraria.ID;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Material;

namespace ArknightsMod.Content.Items.Consumables.PortableStructures
{
	// 便携罗德岛安全屋——第一个基于 .akstruct 格式的便携建筑。
	// 结构来自开发测试扳手实际导出的 Assets/Structures/PortableRhodesIslandSafehouse.akstruct
	// （22x11，2026-07-28 导出）。
	public class PortableRhodesIslandSafehouse : PortableStructureItemBase
	{
		protected override string StructureAssetPath => "Assets/Structures/PortableRhodesIslandSafehouse.akstruct";

		public override void SetDefaults() {
			ApplyCommonDefaults();
			Item.width = 32;
			Item.height = 32;
			Item.rare = ItemRarityID.LightPurple;
			Item.value = Terraria.Item.sellPrice(silver: 20);
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddIngredient<CarbonBrick>(4)
				.AddTile(TileID.WorkBenches)
				.Register();
		}
	}
}
