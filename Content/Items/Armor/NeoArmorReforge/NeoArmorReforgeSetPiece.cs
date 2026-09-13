using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using ArknightsMod.Content.Tiles.Infrastructure;

namespace ArknightsMod.Content.Items.Armor.NeoArmorReforge
{
	// NeoArmor Reforge 的套装件。**不需要为每个干员手写一个子类**——框架会为每件声明了
	// SetProfile 的时装自动 new 一个这个类的实例注册进游戏（见 NeoArmorReforgeSetLoader），
	// 一个实例 = 一件具体的"某干员的头/身/腿套装"。数值/材料/效果全部来自
	// Vanity.SetProfile，这个类只负责把它包装成一个独立、可查配方、tooltip 无条件
	// 显示效果的 ItemID。
	//
	// [Autoload(false)] 必须加：这个类有公共无参构造函数（tModLoader 反射克隆的硬性
	// 要求），如果不加这个特性，自动扫描会把"类本身"也当成一个普通 ModItem 加载一份，
	// 而那一份的 Vanity 是 null，一跑 Load() 就空引用崩溃。真正要用的实例都由
	// NeoArmorReforgeSetLoader 手动创建并 AddContent，类本身绝不能被自动加载。
	[Autoload(false)]
	public sealed class NeoArmorReforgeSetPiece : ModItem
	{
		// ⚠ 不能用"构造函数传参 + 实例字段"来存 Vanity/SlotType：tModLoader 会用无参
		// 构造函数反射克隆出每一份物品实例，构造参数传不进去；而试图重写 Clone/
		// NewInstance 用 MemberwiseClone 保留字段也不行——ModItem.Item 这个属性没有
		// 任何外部可访问的 setter，我们没办法把克隆体的 Item 指向新的实体，只有框架
		// 内部有这个权限。所以反过来：交给框架的默认克隆流程，Vanity/SlotType 改成
		// "按 this.Type 查表"的懒加载属性——同一个 Type 不管哪份克隆，查出来都一样。
		private NeoArmorReforgeVanityItem _vanity;
		private EquipType? _slotType;

		public NeoArmorReforgeVanityItem Vanity => _vanity ??= NeoArmorReforgeSetLoader.GetVanity(Type);
		public EquipType SlotType => _slotType ??= NeoArmorReforgeSetLoader.GetSlotType(Type);
		public NeoArmorReforgeSetProfile Profile => Vanity.SetProfile;

		/// <summary>外观形态开关，与对应时装各自独立记录，互不影响。</summary>
		public bool HelmetForm;

		public NeoArmorReforgeSetPiece() { }

		// 只给 NeoArmorReforgeSetLoader 用：创建"母版"实例后、AddContent 注册之前先把
		// Vanity/SlotType 填上——Name/Texture 这些注册阶段就要用到的属性依赖它们，
		// 而这时 this.Type 还没分配（ModType 是先 Load() 再 Register()），懒加载
		// 查表那条路还查不到，所以母版必须直接赋值。
		internal void Init(NeoArmorReforgeVanityItem vanity, EquipType slotType) {
			_vanity = vanity;
			_slotType = slotType;
		}

		public override string Name => Vanity.Name + "Set";
		public override string Texture => Vanity.Texture;

		// 头部件优先用 ArmorSets.hjson 里已有的 HeadName（例如"泥岩的头盔"），
		// 其它部位用"时装名 +（套装）"。
		public override LocalizedText DisplayName => Language.GetOrRegister(
			$"Mods.{Mod.Name}.NeoArmor.SetName.{Vanity.Name}",
			() => SlotType == EquipType.Head && Language.Exists($"{Profile.LocalizationPrefix}.HeadName")
				? Language.GetTextValue($"{Profile.LocalizationPrefix}.HeadName")
				: Vanity.DisplayName.Value + Language.GetTextValue("Mods.ArknightsMod.NeoArmor.Upgraded"));

		public override void Load() {
			// 注意传的是 Vanity.Texture + 后缀（真正的穿戴帧表），不是 Vanity.Texture
			// 本身（那是图标）——见 NeoArmorReforgeAppearance.EquipTextureSuffix 的说明。
			int slot = EquipLoader.AddEquipTexture(Mod, Vanity.Texture + NeoArmorReforgeAppearance.EquipTextureSuffix(SlotType), SlotType, this, Name);
			NeoArmorReforgeAppearance.RecordDefaultSlot(Name, slot);
			NeoArmorReforgeAppearance.HideVanillaSkin(SlotType, slot);
			NeoArmorReforgeAppearance.RegisterAltEquip(Mod, SlotType, Name, Vanity.AltEquipTexture);
		}

		public override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 1;
			Item.vanity = false;
			Item.rare = NeoArmorReforgeRarity.Get(Vanity.Rarity);
			Item.value = Vanity.Value;
			Item.defense = Profile.Defense;

			// ⚠ 必须在 SetDefaults 阶段就把装备槽位定下来。这个类是手动注册的，没有
			// [AutoloadEquip] 帮它在 SetDefaults 阶段设槽位；而 UpdateEquip 只有在物品
			// 「已经穿在装备栏」之后才会被调用——不设槽位原版就不认为它是防具，既进不了
			// 装备栏也进不了时装栏，于是永远等不到 UpdateEquip，成为死循环。
			NeoArmorReforgeAppearance.ApplyEquipSlot(Item, SlotType, Name, HelmetForm);

			// 有替代外观的装备，套装件也要能手持右键切换（和时装同一套逻辑）。
			if (Vanity.AltEquipTexture != null) {
				Item.useStyle = ItemUseStyleID.HoldUp;
				Item.useTime = 20;
				Item.useAnimation = 20;
				Item.UseSound = SoundID.Item4;
			}
		}

		public override bool AltFunctionUse(Player player) => Vanity.AltEquipTexture != null;

		public override bool CanUseItem(Player player) => Vanity.AltEquipTexture != null && player.altFunctionUse == 2;

		public override bool? UseItem(Player player) {
			if (Vanity.AltEquipTexture == null)
				return null;

			HelmetForm = !HelmetForm;
			return true;
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			return NeoArmorReforgeAppearance.DrawInventoryIcon(spriteBatch, position, drawColor, scale, HelmetForm, Vanity.AltIconTexture);
		}

		// 只重写 UpdateEquip、故意不重写 UpdateVanity：这样套装件放进时装/社交栏时
		// 只有外观、不给防御也不触发任何套装效果（原版对社交栏只调 UpdateVanity），
		// 满足"套装效果只在装备栏生效"的要求。
		// 装备槽位的同步由 NeoArmorReforgeAppearancePlayer 每帧统一处理，这里不再重复。
		public override void UpdateEquip(Player player) {
			player.GetModPlayer<ArknightsArmorPlayer>().LifeCrystalAndFruitEffectReduction = 0.5f;
			player.statLifeMax2 += Profile.LifeBonus;

			if (SlotType != EquipType.Head)
				return;

			Profile.OnHelmetActive?.Invoke(player);

			if (!NeoArmorReforgeSetLoader.IsFullSetEquipped(player, Vanity))
				return;

			Profile.OnFullSetActive?.Invoke(player);
			if (Profile.SetBonusKey != null)
				player.setBonus = Language.GetTextValue(Profile.SetBonusKey);
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips) {
			// 头盔/套装效果说明无条件显示——不像旧 NeoArmor 那样要等升级完才看得到。
			// 用 Language.Exists 守一下：不是每个干员的 ArmorSets.hjson 都写了
			// HelmetEffect/SetEffect（比如 Necrass 从旧系统迁移过来时就一条都没有），
			// 不判断存在性会在 tooltip 里显示裸的本地化 key 字符串。
			string prefix = Profile.LocalizationPrefix;
			if (SlotType == EquipType.Head && Language.Exists($"{prefix}.HelmetEffect"))
				OperatorOutfitTooltipLayout.AddWrappedEffectLines(Mod, tooltips, $"{prefix}.HelmetEffect", "HelmetEffect");
			if (Language.Exists($"{prefix}.SetEffect"))
				OperatorOutfitTooltipLayout.AddWrappedEffectLines(Mod, tooltips, $"{prefix}.SetEffect", "SetEffect");

			int index = tooltips.FindIndex(t => t.Name == "Equipable");
			if (index == -1)
				index = tooltips.Count - 1;

			string lifeBonus = Language.GetTextValue("Mods.ArknightsMod.NeoArmor.LifeBonus", Profile.LifeBonus);
			OperatorOutfitTooltipLayout.InsertWrappedLines(Mod, tooltips, index + 1, lifeBonus, "LifeBonus");

			string lifeReduc = Language.GetTextValue("Mods.ArknightsMod.NeoArmor.LifeItemsReduction", 0.5.ToString("P0"));
			int reducInsert = index + 1 + OperatorOutfitTooltipLayout.WrapLines(lifeBonus).Count();
			OperatorOutfitTooltipLayout.InsertWrappedLines(Mod, tooltips, reducInsert, lifeReduc, "LifeReduction", Colors.RarityTrash);

			int indexName = tooltips.FindIndex(t => t.Name == "ItemName");
			if (indexName != -1)
				tooltips[indexName].Text += " " + Language.GetTextValue("Mods.ArknightsMod.NeoArmor.Upgraded");

			int indexMaterial = tooltips.FindIndex(t => t.Name == "Material");
			if (indexMaterial != -1)
				tooltips.RemoveAt(indexMaterial);
		}

		public override void AddRecipes() {
			// 材料 = 这件时装本身 + Profile.Materials 追加的额外材料。配方浏览器能直接
			// 查到"用 XxxHead 合成 XxxHeadSet"，这是拆成两个 ItemID 最主要的收益之一。
			Recipe recipe = CreateRecipe()
				.AddIngredient(Vanity.Type, 1)
				.AddTile(ModContent.TileType<FactoryTile>());
			Profile.Materials?.Invoke(recipe);
			recipe.DisableDecraft().Register();
		}

		public override void SaveData(TagCompound tag) => tag["helmetForm"] = HelmetForm;
		public override void LoadData(TagCompound tag) => HelmetForm = tag.GetBool("helmetForm");
		public override void NetSend(BinaryWriter writer) => writer.Write(HelmetForm);
		public override void NetReceive(BinaryReader reader) => HelmetForm = reader.ReadBoolean();

		// 时装 tooltip 里追加"升级后可获得"的效果预告——玩家不用真的合成出来，
		// 光拿着时装就能看到套装/头盔效果是什么。
		public static void AppendUpgradeHint(Mod mod, List<TooltipLine> tooltips, NeoArmorReforgeSetProfile profile) {
			string prefix = profile.LocalizationPrefix;
			if (Language.Exists($"{prefix}.HelmetEffect"))
				OperatorOutfitTooltipLayout.AddWrappedEffectLines(mod, tooltips, $"{prefix}.HelmetEffect", "UpgradePreview_Helmet");
			if (Language.Exists($"{prefix}.SetEffect"))
				OperatorOutfitTooltipLayout.AddWrappedEffectLines(mod, tooltips, $"{prefix}.SetEffect", "UpgradePreview_Set");
		}
	}
}
