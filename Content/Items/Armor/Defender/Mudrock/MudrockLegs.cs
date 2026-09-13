using Terraria.ModLoader;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;

namespace ArknightsMod.Content.Items.Armor.Defender.Mudrock
{
	// 腿部没有"切换形态"这回事（只有头和躯干有第二套贴图），所以不声明
	// AltIconTexture/AltEquipTexture，框架也就不会给它加右键切换和切换提示。
	// 旧版本这里挂了切换代码和切换提示，但腿部压根没有替代贴图，右键其实什么都不会
	// 发生，属于误导，已一并清掉。
	[AutoloadEquip(EquipType.Legs)]
	internal class MudrockLegs : NeoArmorReforgeVanityLegs
	{
		public override int Rarity => 6;

		public override string Texture => "ArknightsMod/Content/Items/Armor/Defender/Mudrock/MudrockGreaves";

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 17,
			LifeBonus = 222,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mudrock",
		};
	}
}
