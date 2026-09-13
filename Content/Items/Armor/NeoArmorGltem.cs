using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
#if DEBUG
using ArknightsMod.Common.Configs;
#endif

namespace ArknightsMod.Content.Items.Armor
{
	public static class NeoArmorUtils {
		public static NeoArmorGItem neoarmor(this Item item) {
			return item.GetGlobalItem<NeoArmorGItem>();
		}
		public static readonly Condition NeedVanity = new Condition("Mods.ArknightsMod.NeoArmor.NeedVanity", () => true);
	}
	public class NeoArmorGItem : GlobalItem
	{
		public override bool InstancePerEntity => true;
		public bool hasUpgraded = false;
		// 外观形态（默认外观 / 升级外观），与 hasUpgraded（时装↔套装 归类）解耦：
		// 右键切换只改这个外观标记，不改变物品是时装还是套装。
		public bool helmetForm = false;
		public bool isNeoArmor;
		public override void PostUpdate(Item Item) {
			if (isNeoArmor && hasUpgraded) {
				Item.maxStack = 1;
				if (Item.ModItem is NeoArmorItem neoArmor)
					neoArmor.SetBaseArmorDefaults();
			}
		}

		public override void UpdateInventory(Item Item, Player player) {
			if (isNeoArmor && hasUpgraded) {
				Item.maxStack = 1;
			}
#if DEBUG
			if (isNeoArmor && !hasUpgraded && ModContent.GetInstance<DebugConfig>().AutoUpgradeNeoArmor) {
				hasUpgraded = true;
				helmetForm = true;
				if (Item.ModItem is NeoArmorItem neoArmor)
					neoArmor.SetDefaults();
			}
#endif
		}

		public override void OnCreated(Item Item, ItemCreationContext context) {
			if (isNeoArmor)
			{
#if DEBUG
				if (ModContent.GetInstance<DebugConfig>().AutoUpgradeNeoArmor) {
					hasUpgraded = true;
					helmetForm = true;
					if (Item.ModItem is NeoArmorItem neoArmor)
						neoArmor.SetDefaults();
					return;
				}
#endif
				if (context is RecipeItemCreationContext)
				{
					RecipeItemCreationContext c = context as RecipeItemCreationContext;
					foreach (var i in c.ConsumedItems)
					{
						if (Item.type == i.type && i.neoarmor().isNeoArmor && !i.neoarmor().hasUpgraded)
						{
							hasUpgraded = true;
							helmetForm = true; // 合成升级为套装时，默认呈现升级外观
							if(Item.ModItem is NeoArmorItem neoArmor)
								neoArmor.SetDefaults();
							break;
						}
					}
				}
			}
		}

		public override void NetSend(Item Item, BinaryWriter writer) {
			writer.Write(hasUpgraded);
			writer.Write(helmetForm);
		}
		public override void NetReceive(Item Item, BinaryReader reader) {
			hasUpgraded = reader.ReadBoolean();
			helmetForm = reader.ReadBoolean();
		}
		public override void LoadData(Item Item, TagCompound tag) {
			hasUpgraded = tag.GetBool("hasUpgraded");
			// 旧存档没有 helmetForm 时，外观默认跟随 hasUpgraded（保持旧行为，避免套装显示成默认外观）
			helmetForm = tag.ContainsKey("helmetForm") ? tag.GetBool("helmetForm") : hasUpgraded;
		}
		public override void SaveData(Item Item, TagCompound tagCompound) {
			tagCompound["hasUpgraded"] = hasUpgraded;
			tagCompound["helmetForm"] = helmetForm;
		}
	}
}
