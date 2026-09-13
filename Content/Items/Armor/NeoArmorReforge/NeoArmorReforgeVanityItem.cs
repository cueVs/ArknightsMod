using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Content.Items.Armor.NeoArmorReforge
{
	// ══════════════════════════════════════════════════════════════════════════════
	//
	//   NeoArmor Reforge  —— 干员「时装 / 套装」系统
	//   本文件是整个系统的主文件与使用文档。加新干员只需要读这里。
	//
	// ══════════════════════════════════════════════════════════════════════════════
	//
	//  ┌─ 目录 ────────────────────────────────────────────────────────────────┐
	//  │  1. 这个系统是什么 / 和旧 NeoArmor 的区别                              │
	//  │  2. 快速上手：加一个新干员（最小示例）                                 │
	//  │  3. 需要准备的贴图文件                                                 │
	//  │  4. 需要准备的本地化文案                                               │
	//  │  5. NeoArmorReforgeSetProfile 字段速查表                                       │
	//  │  6. 套装效果怎么写（三种写法，按复杂度递增）                           │
	//  │  7. 外观形态切换（可选，如泥岩的头盔造型）                             │
	//  │  8. 纯时装（不做套装）怎么写                                           │
	//  │  9. 从旧 NeoArmor 迁移一个干员的步骤                                   │
	//  │ 10. 系统内各文件的分工                                                 │
	//  │ 11. 踩过的坑（改动本系统前必读）                                       │
	//  │ 12. 干员专属叠加图层（PlayerDrawLayer）写法                            │
	//  └────────────────────────────────────────────────────────────────────────┘
	//
	//
	// ── 1. 这个系统是什么 / 和旧 NeoArmor 的区别 ──────────────────────────────
	//
	// 一个干员有两样东西：**时装**（纯外观，穿时装栏）和**套装**（有防御和套装效果，
	// 穿装备栏）。玩家在制造加工站用时装 + 材料合成出套装。
	//
	// 和旧 NeoArmor 的根本区别：时装和套装是**两个独立的 ItemID**，不再靠一个
	// GlobalItem 上的 hasUpgraded 布尔值让同一个物品在运行时"变形"兼职两种身份。
	// 由此解决的三个老问题：
	//   · 配方是"时装 → 套装"两个不同物品，配方浏览器类模组能正常查到（旧系统的
	//     配方是 X → X 自我循环，浏览器无法表达）；
	//   · 套装 tooltip 无条件显示套装/头盔效果，玩家合成前就能看到；
	//   · 物品属性在 SetDefaults 一次定型，不再需要 hasInitialized / RefreshState /
	//     每帧补刀重设属性那一套补丁逻辑。
	//
	// **你只需要为每个部位写一个时装类。** 套装件（XxxHeadSet 等）由框架在加载时
	// 自动生成，它的配方、tooltip、装备效果、装备槽位注册全部自动完成，不需要写
	// 第二个类。
	//
	//
	// ── 2. 快速上手：加一个新干员（最小示例）──────────────────────────────────
	//
	// 在 Content/Items/Armor/<职业>/<干员名>/ 下建三个文件。三件必须放在**同一个
	// 命名空间**下——框架靠命名空间判断"这三件属于同一套"，不需要额外声明归属。
	//
	//   // ---------- XxxHead.cs ----------
	//   using Terraria.ModLoader;
	//   using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
	//   using ArknightsMod.Content.Items.Material;
	//
	//   namespace ArknightsMod.Content.Items.Armor.Caster.Xxx
	//   {
	//       [AutoloadEquip(EquipType.Head)]          // ← 别忘了，部位要和基类一致
	//       public class XxxHead : NeoArmorReforgeVanityHead
	//       {
	//           public override int Rarity => 5;     // 干员星级 1~6，决定稀有度颜色
	//
	//           public override NeoArmorReforgeSetProfile SetProfile => new() {
	//               Defense   = 8,                   // 套装件的防御力
	//               LifeBonus = 100,                 // 套装件的最大生命加成
	//               LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Xxx",
	//               Materials = r => r
	//                   .AddIngredient<Orundum>(50)
	//                   .AddIngredient<OrirockConcentration>(10),
	//               SetBonusKey = "Mods.ArknightsMod.ArmorSets.Xxx.SetBonus",
	//           };
	//       }
	//   }
	//
	//   // ---------- XxxBody.cs / XxxLegs.cs ----------
	//   // 一模一样，只是基类换成 NeoArmorReforgeVanityBody / NeoArmorReforgeVanityLegs，
	//   // [AutoloadEquip] 换成 EquipType.Body / EquipType.Legs，数值各自填。
	//   // SetBonusKey 只需要写在 Head 上（身体/腿填了也不会被用到）。
	//
	// 就这样，不需要写配方、不需要写套装件、不需要注册装备贴图槽位。
	//
	//
	// ── 3. 需要准备的贴图文件 ─────────────────────────────────────────────────
	//
	// 放在和 .cs 同一个目录下，文件名必须严格对应类名：
	//
	//   XxxHead.png           头部**图标**（背包里显示的小图，尺寸自定，约 26x24）
	//   XxxHead_Head.png      头部**穿戴帧表**，必须是 40 x 1120
	//   XxxBody.png           躯干图标（约 26x18）
	//   XxxBody_Body.png      躯干穿戴帧表，必须是 **360 x 224**（1.4 的九列格式）
	//   XxxLegs.png           腿部图标（约 22x14）
	//   XxxLegs_Legs.png      腿部穿戴帧表，必须是 40 x 1120
	//
	// ⚠ 躯干帧表是 360x224 的九列格式，**不是** 40x1120。本项目曾经因为把躯干图
	//   做成 40x1120 而出现过穿上后贴图错乱的 bug，务必确认尺寸。
	// ⚠ "_Head/_Body/_Legs" 这个后缀是 [AutoloadEquip] 内部约定的，别写错也别省略。
	//
	// 另外，如果这个干员要做成可抽奖/可获取的，还需要一个"三合一时装袋"
	//   XxxDefault.cs + XxxDefault.png（继承 ArknightsVanityBag，见既有干员的写法）。
	//
	//
	// ── 4. 需要准备的本地化文案 ───────────────────────────────────────────────
	//
	// (a) 物品名：Localization/<语言>/Mods.ArknightsMod.Items.hjson
	//       XxxHead:  { DisplayName: 某干员的头饰 }
	//       XxxBody:  { DisplayName: 某干员的上衣 }
	//       XxxLegs:  { DisplayName: 某干员的裤子 }
	//
	// (b) 套装效果文案：Localization/<语言>/Mods.ArknightsMod.ArmorSets.hjson
	//     key 前缀要和 SetProfile.LocalizationPrefix 对上：
	//       Xxx: {
	//           HeadName:     某干员的头盔          // 套装头部件的物品名
	//           HelmetEffect: "[c/beebff:头盔效果]：…"   // 只戴头盔就生效的效果
	//           SetEffect:    "[c/beebff:套装效果]：…"   // 三件齐全才生效的效果
	//           SetBonus:     一行简述               // 装备栏下方那行绿字
	//       }
	//     HelmetEffect / SetEffect 会显示在**套装件**的 tooltip 上（无条件显示），
	//     同时也会作为"升级后可获得"的预告显示在**时装**的 tooltip 上。
	//     HeadName 不填时，套装头部件的名字会自动退化成"时装名 +（套装）"。
	//
	//
	// ── 5. NeoArmorReforgeSetProfile 字段速查表 ───────────────────────────────────────
	//
	//   Defense             int              套装件防御力。时装恒为 0（框架强制清零）
	//   LifeBonus           int              套装件最大生命加成
	//   LocalizationPrefix  string           文案前缀，见上面第 4 节
	//   Materials           Action<Recipe>   升级所需的**额外**材料（时装本身和制造
	//                                        加工站已由框架自动加上，不要重复添加，
	//                                        也不要自己调 Register()）
	//   SetBonusKey         string           三件齐全时 player.setBonus 显示的文案 key
	//   OnHelmetActive      Action<Player>   头部套装穿在装备栏时**每帧**调用
	//   OnFullSetActive     Action<Player>   三件齐全时**每帧**调用（在上者之后）
	//
	//
	// ── 6. 套装效果怎么写（三种写法，按复杂度递增）────────────────────────────
	//
	// 【写法 A】效果很简单、只是加点属性 → 直接在 Profile 里写 lambda：
	//
	//     OnFullSetActive = p => p.moveSpeed += 0.15f,
	//
	// 【写法 B】效果需要"记住状态"或挂到别的钩子上（伤害修正、受击响应等）
	//          → 建一个 XxxSetPlayer : ModPlayer，用 Profile 的回调只负责"打标记"：
	//
	//     // XxxHead.cs
	//     OnHelmetActive = XxxSetPlayer.OnHelmetActive,
	//
	//     // XxxSetPlayer.cs
	//     internal class XxxSetPlayer : ModPlayer
	//     {
	//         public bool HelmetActive;
	//         public override void ResetEffects() => HelmetActive = false;
	//         public static void OnHelmetActive(Player p) =>
	//             p.GetModPlayer<XxxSetPlayer>().HelmetActive = true;
	//
	//         public override void ModifyWeaponDamage(Item item, ref StatModifier damage) {
	//             if (HelmetActive && item.DamageType.CountsAsClass(DamageClass.Magic))
	//                 damage *= 1.06f;
	//         }
	//     }
	//
	//   末柠(Mornia) 就是这种写法，可以直接照抄。
	//
	// 【写法 C】ModPlayer 里需要自己判断"是否穿着套装"（比如效果依赖计时器、
	//          或者要在 PostUpdate 里做事）→ 用 NeoArmorReforgeSetLoader.GetSetType：
	//
	//     bool full = Player.armor[0].type == NeoArmorReforgeSetLoader.GetSetType<XxxHead>()
	//              && Player.armor[1].type == NeoArmorReforgeSetLoader.GetSetType<XxxBody>()
	//              && Player.armor[2].type == NeoArmorReforgeSetLoader.GetSetType<XxxLegs>();
	//
	//   注意传的是**时装**类型，框架会换算成对应的套装件 ItemType；你不需要知道
	//   自动生成的套装类叫什么。泥岩(Mudrock)、迷迭香(Rosmontis) 是这种写法。
	//
	// ⚠ 不要在 ModPlayer 里再设一次 player.setBonus——SetBonusKey 已经由框架统一
	//   设置了，两处都设会互相覆盖（旧代码就踩过这个坑，两个不同文本抢同一个字段）。
	//
	//
	// ── 7. 外观形态切换（可选，如泥岩的头盔造型）──────────────────────────────
	//
	// 某些干员有第二套外观（例如泥岩戴上头盔）。只需在时装类里多写三个属性：
	//
	//     public override string   AltIconTexture  => "…/XxxHelmet";        // 切换后图标
	//     internal override string AltEquipTexture => "…/XxxHelmet_Head";   // 切换后帧表
	//     protected override string ToggleHintKey  => "Mods.…ArmorSets.Xxx.ToggleHint";
	//
	// 剩下的全自动：手持右键切换、图标替换、穿戴贴图替换、tooltip 提示文案，而且
	// **时装和它对应的套装件都会获得同样的切换能力**。不需要自己写 AltFunctionUse /
	// CanUseItem / UseItem 那一套样板代码。
	//
	// 注意：形态切换**只影响外观**，不改变任何数值，也不改变"这是时装还是套装"。
	// 只有部分部位有第二套外观时，其它部位就别声明这几个属性（声明了才会出现右键
	// 切换和提示）。
	//
	//
	// ── 8. 纯时装（不做套装）怎么写 ───────────────────────────────────────────
	//
	// 不重写 SetProfile（保持默认返回 null）即可。框架不会为它生成套装件、不会生成
	// 配方，它就是一件纯外观时装。
	//
	//
	// ── 9. 从旧 NeoArmor 迁移一个干员的步骤 ───────────────────────────────────
	//
	//   1) 基类：NeoArmorHead/Body/Legs  →  NeoArmorReforgeVanityHead/Body/Legs
	//      并加上 using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
	//   2) 删掉 ArmorLifeBonus、SetArmorDefaults()、AddRecipes()、
	//      IsArmorSet()、UpdateArmorSet()，把它们的内容搬进 SetProfile：
	//        ArmorLifeBonus        → LifeBonus
	//        SetArmorDefaults 里的 Item.defense → Defense
	//        AddRecipes 里除"自己 ×1"和制造加工站之外的材料 → Materials
	//        UpdateArmorSet 里的 setBonus 文案 → SetBonusKey
	//   3) 删掉 ArmorIconTexture / UpdateVanityEquip 里手动切槽位的代码，
	//      改用第 7 节的 AltIconTexture / AltEquipTexture。
	//   4) XxxSetPlayer 里的判定：OperatorSetEquipHelper.HasHelmet/HasFullSet 或
	//      `xxx.neoarmor().hasUpgraded`  →  第 6 节写法 C 的 GetSetType 写法。
	//   5) 时装类里**不要**再设 Item.defense——框架会强制清零（时装不给防御）。
	//
	//
	// ── 10. 系统内各文件的分工 ────────────────────────────────────────────────
	//
	//   NeoArmorReforgeVanityItem.cs       ← 本文件。时装基类 + 全部使用文档
	//   NeoArmorReforgeVanitySlots.cs      部位标记类（Head/Body/Legs）
	//   NeoArmorReforgeSetProfile.cs       套装描述数据（纯数据容器）
	//   NeoArmorReforgeSetPiece.cs         套装件本体，一个类服务所有干员
	//   NeoArmorReforgeSetLoader.cs        注册中枢 + GetSetType<T>() 对外查询
	//   NeoArmorReforgeAppearance.cs       装备贴图槽位的注册与形态切换
	//   NeoArmorReforgeAppearancePlayer.cs 每帧兜底同步装备槽位
	//   NeoArmorReforgeEquipSystem.cs      记录默认槽位、隐藏原版身体部位
	//   NeoArmorReforgeRarity.cs           星级 → 稀有度换算
	//
	//
	// ── 11. 踩过的坑（改动本系统前必读）───────────────────────────────────────
	//
	//  (1) 自动加载是**按类型全名字母序**的（Mod.Autoload 里 OrderBy(FullName)）。
	//      所以"用一个 ModSystem 统一扫描所有时装"的做法会静默漏掉字母序排在它
	//      后面的干员。这就是为什么套装件的注册放在每件时装自己的 Load() 里。
	//  (2) ModType 是**先 Load() 再 Register()**，所以在 Load() 里 this.Type 还
	//      没分配。需要 ItemType 的映射表只能延后到 PostSetupContent 再填。
	//  (3) ModItem.Item 属性**没有外部可访问的 setter**，所以不能用
	//      MemberwiseClone 自己实现 Clone/NewInstance（克隆体的 Item 会指向错误的
	//      实体）。套装件因此改用"无参构造 + 按 Type 查表"来拿 Vanity/SlotType。
	//  (4) EquipLoader.AddEquipTexture 传入非 null 的 item 时，会覆盖
	//      idToSlot[item.Type] 这条反向映射。注册"第二套外观"时必须传 null，
	//      否则默认外观的槽位映射会被覆盖，导致形态切换完全失效且无任何报错。
	//  (5) player.head/body/legs 只在 Player.Update 里从 armor[0..2] 的槽位字段
	//      读取；而 ItemLoader.UpdateEquip 的调用被 UpdateEquips_CanItemGrantBenefits
	//      挡着，时装（Item.vanity = true）穿在装备栏时可能一个钩子都不走。所以
	//      槽位同步必须放在 NeoArmorReforgeAppearancePlayer 里每帧兜底，不能只靠物品钩子。
	//  (6) 套装件必须在 **SetDefaults** 阶段就设好装备槽位，否则原版不认为它是防具，
	//      既进不了装备栏也进不了时装栏，于是永远等不到 UpdateEquip —— 死循环。
	//  (7) ArmorIDs.*.Sets 的那些数组（HidesArms / HidesTopSkin / HidesBottomSkin /
	//      DrawHead 等）会在 Load 阶段之后被**整体重置一次**：
	//          ModContent.Load()
	//            ├ LoadModContent(...)  第一遍 → Mod.Load / AddContent / ModType.Load()
	//            ├ ResizeArrays() → EquipLoader.ResizeAndFillArrays()
	//            │                   → LoaderUtils.ResetStaticMembers(typeof(ArmorIDs))  ★
	//            ├ LoadModContent(...)  第二遍 → SetStaticDefaults
	//            └ LoadModContent(...)  第三遍 → PostSetupContent
	//      所以任何在 Load() 里写进 ArmorIDs 的值都会被 ★ 抹掉，且没有任何报错，
	//      表现是"穿上后原版裸体胳膊/皮肤/头发和干员贴图重叠画出来"。
	//      本系统统一走 NeoArmorReforgeAppearance.HideVanillaSkin() 登记 →
	//      NeoArmorReforgeEquipSystem.PostSetupContent 里一次性写入，别再在 Load() 里直接写。
	//  (8) 自定义叠加图层（PlayerDrawLayer）有一整套坑，**单独见下面第 12 节**。
	//
	//
	// ── 12. 干员专属叠加图层（PlayerDrawLayer）写法 ────────────────────────────
	//
	// 有些干员除了三件套贴图，还要额外画一层东西：死芒的后发、莫斯提马的头部光环和
	// 背部翅膀、迷迭香的背部特效等。这类图层踩过的坑特别多，而且**全部都是静默失败
	// （不画 / 画错位置，一个报错都没有）**，所以这里把已验证的正确写法完整列出来。
	//
	// ── 12.1 模板（照抄这个）────────────────────────────────────────────────
	//
	//   internal class XxxHeadBackLayer : PlayerDrawLayer
	//   {
	//       // ① 图层顺序：见 12.2
	//       public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.HeadBack);
	//
	//       // ② 可见性：见 12.3
	//       public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
	//           Player player = drawInfo.drawPlayer;
	//           return NeoArmorReforgeSetLoader.IsPartVisible<XxxHead>(player, EquipType.Head)
	//               && !player.dead;
	//       }
	//
	//       protected override void Draw(ref PlayerDrawSet drawInfo) { … 见 12.4 / 12.5 }
	//   }
	//
	// ── 12.2 图层顺序：别乱挂父层 ───────────────────────────────────────────
	//
	// PlayerDrawLayers 的字段顺序就是绘制顺序，从后往前大致是：
	//   … Wings → HairBack → BackAcc → HeadBack → BalloonAcc → Skin → Leggings →
	//     Shoes → Robe → SkinLongCoat → ArmorLongCoat → Torso → OffhandAcc →
	//     WaistAcc → NeckAcc → Head → FinchNest → FaceAcc → … → HeldItem
	//
	//   · 要"在身体背后、被身体挡住"（后发、背部特效、翅膀）→ 挂 HeadBack / BackAcc /
	//     HairBack / Wings 这一段。
	//   · 要"盖在身体之上"（头部光环、面部发光）→ 挂 Head 之后。
	//   ⚠ 死芒后发最初挂在 BeforeParent(Head)（第 21 层），结果排在 Skin(12)、
	//     Torso(17)、NeckAcc(20) 之后，把整个角色糊住了。
	//   ⚠ 父层 Visible == false 时**整个子树会被跳过**（PlayerDrawLayer
	//     .DrawWithTransformationAndChildren 开头就 return），子层自己的
	//     GetDefaultVisibility 根本不会被调用。所以父层要挑恒可见的——已确认
	//     HairBack / HeadBack / BackAcc / Skin / Head / Wings 的 condition 都是 null，
	//     即恒可见，可以放心挂。若不确定，用 new Between(层A, 层B) 让它成为独立顶层
	//     图层，完全不受父层可见性影响。
	//
	// ── 12.3 可见性：不要比对装备槽位 ID ───────────────────────────────────
	//
	// ❌ 不要写 player.head == EquipLoader.GetEquipSlot(Mod, nameof(XxxHead), EquipType.Head)
	// ✅ 用 NeoArmorReforgeSetLoader.IsPartVisible<XxxHead>(player, EquipType.Head)
	//
	// 反编译 Player.PlayerFrame 可以看到 head/body/legs 的赋值是三步：先从 armor[0..2]
	// 取、再被 armor[10..12]（时装栏）覆盖、**最后还会被 Player.SetMatch 整个重写一遍**
	// （原版盔甲套装替换机制）。绘制时刻的 player.head 未必等于我们注册时拿到的槽位
	// ID，比对就静默失败、图层直接不画。IsPartVisible 是直接看"最终显示出来的那件装备
	// 是什么物品"，且时装和套装两个 ItemID 都认，确定且不受上述机制影响。
	//
	// ── 12.4 取动画帧：只能用 bodyFrame ────────────────────────────────────
	//
	// ❌ player.headFrame —— **恒为 {0,0,0,0}**。全 tModLoader 只有
	//    UICharacter.UpdateAnim（角色选择界面的预览动画）会写它，游戏内行走/战斗时
	//    从来不赋值。用它算帧号会得到 Width/Height == 0，被判非法直接 return，
	//    图层永远画不出来且无任何报错——死芒后发就是栽在这一条上。
	// ✅ player.bodyFrame —— 原版画头部装备的 DrawPlayer_21_Head 用的也是它。
	//    头部 / 躯干 / 腿部帧表本来就是同一套 20 帧 x 56px 的结构，所以头部叠加层
	//    按 bodyFrame 取帧就能和角色动画完全同步。
	//
	//    Rectangle frame = p.bodyFrame;
	//    if (frame.Height <= 0) return;
	//    int frameIndex = frame.Y / frame.Height;
	//    Rectangle sourceRect = new(0, frameIndex * frame.Height, texture.Width, frame.Height);
	//    if (sourceRect.Bottom > texture.Height) return;
	//
	// ── 12.5 坐标：两条路，别自己瞎凑 ──────────────────────────────────────
	//
	// 【路 A｜整张贴图、不需要按帧取】用现成的 PlayerLayerHelper（莫斯提马的光环和
	//   翅膀、迷迭香的背部都走这条）。它以 player.MountedCenter 为锚点，已经算好了
	//   gfxOffY 和行走时的 2px 上下抖动：
	//
	//     PlayerLayerHelper.AddPlayerDrawLayer(ref drawInfo, texture, 0, new Vector2(0, -3));
	//     // 第三个参数是部位（Head=0/Body=1/Legs=2，决定用哪个染料槽）
	//     // (0, -3) 是本项目头部叠加层通用的"身体中心 → 头部中心"偏移
	//
	// 【路 B｜需要按帧取，必须自己写 DrawData】完整照抄原版 DrawPlayer_21_Head 的
	//   坐标公式，三步一步都不能少（死芒后发是这一类）：
	//
	//     Vector2 position = Utils.Floor(
	//         new Vector2(
	//             -(p.bodyFrame.Width / 2) + (p.width / 2),   // ← 少了这项会右偏 10px
	//             p.height - p.bodyFrame.Height + 4f)         // ← 少了这项会下偏 10px
	//         + drawInfo.Position - Main.screenPosition)      // ← Utils.Floor 整体取整
	//         + p.headPosition + drawInfo.headVect;
	//     Vector2 origin = drawInfo.headVect;                 // ← 旋转轴心对齐原版头部
	//
	//   ⚠ Utils.Floor 不能省：headVect.Y 是 22.4 这类小数，不取整会半像素采样，
	//     表现是"游戏里的像素和贴图像素对不上、发虚错行"。
	//   ⚠ origin 用 drawInfo.headVect，不要用源矩形中心——后者会再多引入 5.6px 偏移。
	//
	//
	// ── 迁移进度 ──────────────────────────────────────────────────────────────
	//
	// 已迁移：末柠(Mornia)、泥岩(Mudrock)、迷迭香(Rosmontis)
	//         + 第一批【术师 Caster】：阿米娅、夜烟、深靛、炎熔、莫斯提马、死芒、史都华德
	//         + 第二批【先锋 Vanguard】：风笛、芬、翎羽、德克萨斯、香草
	//         + 第三批【重装 Defender】：米格鲁、卡缇、年、塞雷娅、斑点、火神（重装职业至此清空）
	//         + 第四批【医疗 Medic】：安赛尔、芙蓉、凯尔希、焰影苇草、华法琳（医疗职业至此清空）
	//         + 第五批【辅助 Supporter】：魔王、深海色、令、梓兰、电弧（辅助职业至此清空）
	//           ※ 电弧只迁了时装，套装仍缺配方，原因见 RaidianHead 顶部说明
	//         + 第六批【特种 Specialist】：多萝西、新约能天使、狮蝎、水月、若叶睦、缄默德克萨斯
	//                                     （特种职业至此清空）
	//         + 第七批【狙击 Sniper】：安德切尔、空爆、能天使、远牙、菲亚梅塔、克洛斯、
	//                                 寒芒克洛丝、玫拉、普罗旺斯、提丰、W、维什戴尔
	//                                 （加上首批的迷迭香，狙击职业至此清空）
	//         + 第八批【近卫 Guard】：陈、隐德来希、羽毛笔、拉普兰德、缠丸、玫兰莎、月见夜、
	//                                 玛恩纳、旋律之主、泡普卡、斯卡蒂、史尔特尔、乌尔比安、宴
	//                                 （近卫职业至此清空）
	//         + 终末地【管理员 / 别礼 / 伊冯】与博士初始装：均为纯时装（无套装文案与配方）
	//
	// ✅ 全部干员已迁移完毕，旧的 NeoArmor 系统已无任何引用，
	//    NeoArmorItem / NeoArmorGltem / NeoArmorSlots / NeoArmorEquipSystem /
	//    OperatorSetEquipHelper 这五个文件可以直接删除。
	//
	// ⚠ 迁移一批干员时，除了本文件第 9 节列的那些步骤，别忘了检查**跨干员的判定入口**：
	//   · Operator<职业>SetHelper（如 OperatorVanguardSetHelper）里的 HasFullSet 判定，
	//     要一起换成 GetSetType，否则"整套是否穿着"恒为 false，且没有任何报错。
	//   · 任何以「时装头部件 ItemType」为 key、却用 player.armor[0].type 去查的表。
	//     OperatorTagRegistry 就是这种，它已改为查不到时用套装件反查时装再重试一次，
	//     所以后续批次不需要再改它；但如果新写了类似的表，记得照这个思路处理。
	//
	// ══════════════════════════════════════════════════════════════════════════════
	public abstract class NeoArmorReforgeVanityItem : ModItem
	{
		/// <summary>干员的星数，1~6。</summary>
		public abstract int Rarity { get; }

		/// <summary>物品售价。</summary>
		public virtual int Value => 15000;

		/// <summary>升级成套装的描述数据；返回 null 表示这件时装没有对应套装（纯时装用法）。</summary>
		public virtual NeoArmorReforgeSetProfile SetProfile => null;

		/// <summary>
		/// 外观形态开关。只影响外观，不影响任何数值/功能——切换后时装还是时装、
		/// 套装还是套装。时装和套装各自独立记录这个值，互不影响。
		/// </summary>
		public bool HelmetForm;

		/// <summary>形态切换后库存图标要换成的贴图路径；为 null 时图标不变。</summary>
		public virtual string AltIconTexture => null;

		/// <summary>
		/// 形态切换后穿戴时显示的替代帧表贴图（注意是 _Head/_Body/_Legs 那张完整帧表，
		/// 不是图标）；为 null 表示这件装备没有形态切换功能。
		/// internal 是因为 NeoArmorReforgeSetPiece 需要读它来给套装件注册同一份替代外观。
		/// </summary>
		internal virtual string AltEquipTexture => null;

		/// <summary>
		/// 形态切换提示文案的本地化 key（比如 "Mods.ArknightsMod.ArmorSets.Mudrock.ToggleHint"）。
		/// 只有声明了 AltEquipTexture 的装备才需要填；为 null 时显示通用的时装提示。
		/// </summary>
		protected virtual string ToggleHintKey => null;

		/// <summary>由 NeoArmorReforgeVanityHead/Body/Legs 声明这件时装属于哪个装备类型。</summary>
		internal virtual EquipType? SlotType => null;

		public override void Load() {
			if (SlotType is EquipType type)
				NeoArmorReforgeAppearance.RegisterAltEquip(Mod, type, Name, AltEquipTexture);

			// 在时装自己的 Load() 里就把配套的套装件创建 + 注册掉，而不是交给一个统一的
			// ModSystem 去扫描所有时装——tModLoader 的自动加载是按类型全名的字母序进行的
			// （Mod.Autoload 里 OrderBy(FullName, InvariantCulture)），统一扫描的 ModSystem
			// 只能看到"字母序排在它自己前面"的那些干员，排在后面的会被整批漏掉，而且完全
			// 静默无报错。改成"每件时装各自注册自己的套装件"，就跟加载顺序彻底无关了。
			if (SetProfile != null)
				NeoArmorReforgeSetLoader.RegisterSetPieceFor(this);
		}

		public sealed override void SetStaticDefaults() {
			Item.ResearchUnlockCount = 1;
		}

		public sealed override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.rare = ItemRarityID.White;
			Item.maxStack = 1;
			Item.value = Value;
			Item.vanity = true;

			// 声明了替代外观的装备自动获得"手持右键切换形态"的能力。
			// 装备栏内右键会被原版用于卸下装备，没法用来做形态切换，所以只能做成
			// 手持右键触发；下面的 AltFunctionUse/CanUseItem/UseItem 配套实现。
			if (AltEquipTexture != null) {
				Item.useStyle = ItemUseStyleID.HoldUp;
				Item.useTime = 20;
				Item.useAnimation = 20;
				Item.UseSound = SoundID.Item4;
			}

			SetVanityDefaults();

			// 框架级保证：时装永远不提供防御力，防御力只属于套装件。
			// 放在 SetVanityDefaults() 之后，子类就算手滑写了 defense 也会被清掉。
			Item.defense = 0;
		}

		public override bool AltFunctionUse(Player player) => AltEquipTexture != null;

		public override bool CanUseItem(Player player) => AltEquipTexture != null && player.altFunctionUse == 2;

		public override bool? UseItem(Player player) {
			if (AltEquipTexture == null)
				return null;

			HelmetForm = !HelmetForm;
			return true;
		}

		public sealed override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale) {
			return NeoArmorReforgeAppearance.DrawInventoryIcon(spriteBatch, position, drawColor, scale, HelmetForm, AltIconTexture);
		}

		// 注意：这里不需要再同步装备槽位——NeoArmorReforgeAppearancePlayer 每帧会统一处理，
		// 原因见那个类的注释（原版对这两个钩子有前置判断，时装穿在盔甲栏时可能一个
		// 都不走，靠这里同步槽位并不可靠）。
		public sealed override void UpdateVanity(Player player) => UpdateVanityEquip(player);
		public sealed override void UpdateEquip(Player player) => UpdateVanityEquip(player);

		public sealed override void ModifyTooltips(List<TooltipLine> tooltips) {
			ModifyVanityTooltips(tooltips);
			if (SetProfile != null)
				NeoArmorReforgeSetPiece.AppendUpgradeHint(Mod, tooltips, SetProfile);
		}

		public override void SaveData(TagCompound tag) => tag["helmetForm"] = HelmetForm;
		public override void LoadData(TagCompound tag) => HelmetForm = tag.GetBool("helmetForm");
		public override void NetSend(BinaryWriter writer) => writer.Write(HelmetForm);
		public override void NetReceive(BinaryReader reader) => HelmetForm = reader.ReadBoolean();

		/// <summary>额外设置时装属性（贴图尺寸、使用手感等）。不要在这里设置防御力，会被清零。</summary>
		public virtual void SetVanityDefaults() { }

		/// <summary>时装装备时的效果（时装栏/社交栏/盔甲栏当纯时装穿都会生效）。</summary>
		public virtual void UpdateVanityEquip(Player player) { }

		/// <summary>修改时装的额外提示文本。默认显示形态切换提示（若有）或通用时装提示。</summary>
		public virtual void ModifyVanityTooltips(List<TooltipLine> tooltips) {
			string key = ToggleHintKey ?? "Mods.ArknightsMod.NeoArmor.VanityHint";
			OperatorOutfitTooltipLayout.ApplyWrappedVanityLine(Mod, tooltips, key);
		}
	}
}
