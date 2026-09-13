using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	// 血蕈/霜晶树/回声玉米这类“稀有自然采集物”的共同基类：统一强制紫色稀有度，
	// 不用基类 ArknightsMaterial 的稀有度色表（那套只到 LightPurple）。
	// Item.value 在这类物品上表示“卖给坎诺特时，每件可换取的源石锭数量”，不是原版钱币价值。
	// 实际出售由 RareCollectibleSellPlayer 接管；这里同时移除原版钱币售价并显示源石锭售价。
	public abstract class RareCollectibleItem : ArknightsMaterial
	{
		public override int Rarity => 0; // 不生效，颜色统一在下面改写
		public abstract int BaseOriginiumIngotValue { get; }

		public sealed override void SafeSetDefaults() {
			Item.rare = ItemRarityID.Purple;
			Item.value = BaseOriginiumIngotValue;
			SafeSetCollectibleDefaults();
		}

		public virtual void SafeSetCollectibleDefaults() { }

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			// Item.value 只是本模组保存动态估值的载体，不能让原版把它解释成铜币。
			tooltips.RemoveAll(line => line.Mod == "Terraria" && line.Name == "Price");
			tooltips.Add(new TooltipLine(Mod, "CannotOriginiumIngotValue",
				Language.GetTextValue("Mods.ArknightsMod.CommonTooltips.CannotOriginiumIngotValue",
					$"[i:{ModContent.ItemType<OriginiumIngot>()}]", Item.value)));
		}
	}
}
