using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor
{
    public abstract class NeoArmorItem : ModItem
    {
		/// <summary> 干员的星数，请输入1~6（必须由子类重写）</summary>
		public abstract int Rarity { get; }

		/// <summary> 生命值增幅（仅升级为盔甲后生效，必须由子类重写）</summary>
		public virtual int ArmorLifeBonus => 0;

		/// <summary> 物品价值，取代 item.value，请直接重写这个</summary>
		public virtual int Value => 15000;

        /// <summary> 由子类实现，设置该部位特有的静态属性（如 Head、Body、Legs 的隐藏/绘制规则）</summary>
        protected abstract void SetSlotStaticDefaults();
		private bool hasInitialized;
        public sealed override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
            if (Main.netMode != NetmodeID.Server)
            {
                SetSlotStaticDefaults();
                SetStaticDefaultsNoServer();
            }
        }

        public sealed override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.rare = ItemRarityID.White;
			Item.maxStack = 1;
			Item.value = Value;
            Item.vanity = true;
            SetVanityDefaults();

			if (Item.neoarmor().hasUpgraded)
				SetBaseArmorDefaults();

			Item.neoarmor().isNeoArmor = true;
		}

		public void SetBaseArmorDefaults() {

			if (!hasInitialized && Item.neoarmor().hasUpgraded)
			{
				Item.rare = GetRarity(Rarity);
				Item.vanity = false;
				SetArmorDefaults();
				hasInitialized = true;
			}
		}

		/// <summary> 在不重建 GlobalItem 实例（不丢失 hasUpgraded）的前提下，重新套用时装/盔甲属性。<br/>
		/// 用于右键切换 hasUpgraded 后刷新物品状态（双向切换，而非仅依赖一次性升级）。</summary>
		public void RefreshState() {
			hasInitialized = false;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 1;
			Item.value = Value;
			Item.vanity = true;
			SetVanityDefaults();
			if (Item.neoarmor().hasUpgraded)
				SetBaseArmorDefaults();
		}

		/// <summary> 升级为盔甲后，库存图标替换为的贴图路径（为 null 时不替换，沿用时装图标）</summary>
		protected virtual string ArmorIconTexture => null;

		public sealed override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
			if (Item.neoarmor().hasUpgraded)
				SetBaseArmorDefaults();

			// 库存图标跟随外观形态（helmetForm），与时装/套装归类（hasUpgraded）解耦
			if (Item.neoarmor().helmetForm && ArmorIconTexture != null)
			{
				Texture2D tex = ModContent.Request<Texture2D>(ArmorIconTexture).Value;
				spriteBatch.Draw(tex, position, null, drawColor, 0f, tex.Size() / 2f, scale, SpriteEffects.None, 0f);
				return false;
			}

			return true;
		}

		public sealed override void UpdateEquip(Player player) {

			UpdateVanityEquip(player);
			if (Item.neoarmor().hasUpgraded)
			{
				player.GetModPlayer<ArknightsArmorPlayer>().LifeCrystalAndFruitEffectReduction = 0.5f;
				player.statLifeMax2 += ArmorLifeBonus;
				UpdateArmorEquip(player);
			}
		}

		public sealed override void UpdateVanity(Player player) {
			UpdateVanityEquip(player);
		}

		public sealed override void ModifyTooltips(List<TooltipLine> tooltips) {
			if (Item.neoarmor().hasUpgraded) {
				int index = tooltips.FindIndex(t => t.Name == "Equipable");
				if (index == -1)
					index = tooltips.Count - 1;

				string lifeBonus = Language.GetTextValue("Mods.ArknightsMod.NeoArmor.LifeBonus", ArmorLifeBonus);
				OperatorOutfitTooltipLayout.InsertWrappedLines(Mod, tooltips, index + 1, lifeBonus, "LifeBonus");

				string lifeReduc = Language.GetTextValue("Mods.ArknightsMod.NeoArmor.LifeItemsReduction", 0.5.ToString("P0"));
				int reducInsert = index + 1 + OperatorOutfitTooltipLayout.WrapLines(lifeBonus).Count();
				OperatorOutfitTooltipLayout.InsertWrappedLines(Mod, tooltips, reducInsert, lifeReduc, "LifeReduction", Colors.RarityTrash);

				int indexName = tooltips.FindIndex(t => t.Name == "ItemName");
				if (indexName != -1) {
					string nameAddon = Language.GetTextValue("Mods.ArknightsMod.NeoArmor.Upgraded");
					tooltips[indexName].Text += " " + nameAddon;
				}

				int indexMaterial = tooltips.FindIndex(t => t.Name == "Material");
				if (indexMaterial != -1)
					tooltips.RemoveAt(indexMaterial);
				ModifyArmorTooltips(tooltips);
			}
			else {

				ModifyVanityTooltips(tooltips);
			}
		}

		// 星级→稀有度的换算统一由新系统那份实现负责，避免两处各写一份导致数值走漏。
		public static int GetRarity(int rarity) => NeoArmorReforge.NeoArmorReforgeRarity.Get(rarity);

		/// <summary>额外为 <see cref="SetDefaults"/> 设置盔甲属性<br/> - 会覆盖 <see cref="SetVanityDefaults"/> 中的相同字段</summary>
		public virtual void SetArmorDefaults() { }

		/// <summary>额外为 <see cref="SetDefaults"/> 设置时装属性<br/> - 相同字段会被 <see cref="SetArmorDefaults"/> 覆盖</summary>
		public virtual void SetVanityDefaults() { }

		/// <summary> 盔甲装备时效果（仅升级后盔甲栏生效，此时会覆盖UpdateVanityEquip相同字段的值） </summary>
		public virtual void UpdateArmorEquip(Player player) { }

		/// <summary> 时装装备时效果（时装栏/盔甲栏均生效） </summary>
		public virtual void UpdateVanityEquip(Player player) { }

		/// <summary>额外设置 <see cref="SetStaticDefaults"/> ，但仅在非 Server 情况下调用</summary>
		public virtual void SetStaticDefaultsNoServer() { }

		/// <summary>修改盔甲状态下的额外提示文本</summary>
		/// <param name="tooltips">当前工具提示列表，可直接修改</param>
		public virtual void ModifyArmorTooltips(List<TooltipLine> tooltips) { }

		/// <summary>修改时装状态下的额外提示文本</summary>
		/// <param name="tooltips">当前工具提示列表，可直接修改</param>
		public virtual void ModifyVanityTooltips(List<TooltipLine> tooltips) {
			OperatorOutfitTooltipLayout.ApplyWrappedVanityLine(Mod, tooltips, "Mods.ArknightsMod.NeoArmor.VanityHint");
		}
	}
}