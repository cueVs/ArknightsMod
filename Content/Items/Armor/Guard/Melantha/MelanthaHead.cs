using ArknightsMod.Common;
using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using ArknightsMod.Content.Items.Material;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Guard.Melantha
{
	[AutoloadEquip(EquipType.Head)]
	public class MelanthaHead : NeoArmorReforgeVanityHead
	{
		public override int Rarity => 3;

		// 迁移补记：玫兰莎三件在旧 NeoArmor 系统里从来没有写过 AddRecipes，套装升不出来，
		// MelanthaSetPlayer 里的效果（近战 104%、「无畏者」层数再生）一直够不到，尽管
		// ArmorSets.hjson 四条文案都写全了。这里按电弧/W 的先例补齐配方，材料参照
		// 同为三星的翎羽/安德切尔（源石 ×30 + 一种基础材料）。
		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 0,
			LifeBonus = 140,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Melantha",
			Materials = recipe => recipe
				.AddIngredient<Orundum>(30)
				.AddIngredient<Orirock>(2),
			SetBonusKey = "Mods.ArknightsMod.ArmorSets.Melantha.SetBonus",
		};

		// ⚠ 必须调 base.Load()：基类的 Load() 负责注册替代外观和「配套的套装件」，
		// 漏掉就不会生成套装件，配方和套装效果一起消失，且没有任何报错。
		public override void Load() {
			base.Load();

			if (Main.netMode == NetmodeID.Server)
				return;

			// 旧代码遗留：额外注册一份 Back 类型的装备贴图。下面的图层是自己 Request 贴图
			// 直接画的、并不依赖这个槽位，但保留以免改变既有行为。
			EquipLoader.AddEquipTexture(Mod, $"{Texture}_{EquipType.Back}", EquipType.Back, this);
		}

		// 背后披风叠加层，走文档 12.5 的【路 A】。
		internal class MelanthaHeadLayer : PlayerDrawLayer
		{
			public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

			public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
				Player player = drawInfo.drawPlayer;
				// 旧代码是 player.head == EquipLoader.GetEquipSlot(...)：按名字反查槽位再比对，
				// 既漏掉套装形态，又会被 Player.SetMatch 静默打断（见 IsPartVisible 注释）。
				return NeoArmorReforgeSetLoader.IsPartVisible<MelanthaHead>(player, EquipType.Head) && !player.dead;
			}

			protected override void Draw(ref PlayerDrawSet drawInfo) {
				var texture = ModContent.Request<Texture2D>(
					"ArknightsMod/Content/Items/Armor/Guard/Melantha/MelanthaHead_Back",
					ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;

				var offset = new Vector2(0, -3) + new Vector2(0, -8);
				PlayerLayerHelper.AddPlayerDrawLayer(ref drawInfo, texture, 0, offset);
			}
		}
	}
}
