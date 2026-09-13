```text
新增東西記得修改這個表格

ArknightsMod/
├── .github/
├── Assets/  # 程式載入的資源/資料
│   ├── Effects/  # Shader 與螢幕/天空效果
│   ├── GrayScaleTexture/  # 視覺效果用灰階貼圖
│   ├── LevelDatas/  # CSV 數值表（升級/技能/武器等）
│   │   └── Skills/  # 技能等級表（CSV）
│   ├── Menu/  # 主選單貼圖
│   ├── OriginalMusic/  # 以 MusicLoader.AddMusic 載入的音樂
│   ├── SceneEffects/  # 場景/環境效果類
│   └── Sound/  # 以 Assets 路徑引用的額外音效
│       ├── ImperialArtilleyCoreTargeteer/  # IACT Boss 音效組
│       └── WisdelCannon/  # Wisadel 炮台音效組
├── Common/  # 共用模組（UI/Config/粒子/視覺效果）
│   ├── Configs/  # ModConfig 設定檔
│   ├── Particle/  # 粒子系統
│   ├── UI/  # UI 程式與貼圖
│   │   ├── BattleRecord/  # 升級/戰鬥紀錄 UI
│   │   │   ├── Calculators/  # UI 用計算器（預覽/數值）
│   │   │   ├── Images/  # UI 圖片資源
│   │   │   └── UIElements/  # UI 元件
│   │   ├── SkillIcons/  # 技能圖示貼圖
│   │   └── SummonIcon/  # 召喚圖示貼圖
│   └── VisualEffects/  # 視覺效果工具（拖尾/震動等）
├── Content/  # tModLoader 內容（物品/NPC/彈幕/方塊）
│   ├── BossBars/  # Boss 血條（BossBar）
│   ├── Buffs/  # Buff/Debuff
│   ├── Currencies/  # 自訂貨幣
│   ├── Dusts/  # Dust（粒子）定義
│   │   └── Bosses/  # Boss 專用 Dust
│   ├── Events/  # 事件/入侵
│   ├── Items/  # 物品
│   │   ├── Accessories/  # 飾品
│   │   │   └── Rogue/  # 肉鴿/隨機模式飾品（按稀有度分）
│   │   │       ├── Rarity_l1/  # 稀有度 L1
│   │   │       ├── Rarity_l2/  # 稀有度 L2
│   │   │       ├── Rarity_l3/  # 稀有度 L3
│   │   │       └── Rarity_l4/  # 稀有度 L4
│   │   ├── Armor/  # 盔甲
│   │   │   └── Vanity/  # 外觀盔甲（時裝）
│   │   │       ├── Caster/  # 時裝：Caster
│   │   │       ├── Defender/  # 時裝：Defender
│   │   │       ├── Guard/  # 時裝：Guard
│   │   │       ├── Medic/  # 時裝：Medic
│   │   │       ├── Sniper/  # 時裝：Sniper
│   │   │       ├── Specialist/  # 時裝：Specialist
│   │   │       ├── Supporter/  # 時裝：Supporter
│   │   │       └── Vanguard/  # 時裝：Vanguard
│   │   ├── BattleRecords/  # 作戰紀錄物品（升級 UI）
│   │   ├── Bosssummon/  # 召喚 Boss 物品
│   │   ├── Consumables/  # 消耗品
│   │   │   └── VanityBags/  # 時裝袋
│   │   ├── DisplayForUI/  # UI 專用展示物品
│   │   ├── Material/  # 製作材料
│   │   │   └── ReclamAlgor/  # 生息演算材料
│   │   ├── Placeable/  # 可放置物品（方塊/家具）
│   │   │   ├── Banners/  # 旗幟物品
│   │   │   ├── Furniture/  # 家具物品
│   │   │   └── Infrastructure/  # 基建物品
│   │   │       ├── Canteen/  # 基建：Canteen
│   │   │       ├── ControlCenter/  # 基建：ControlCenter
│   │   │       ├── Deck/  # 基建：Deck
│   │   │       ├── Decorates/  # 基建：Decorates
│   │   │       ├── HROffice/  # 基建：HROffice
│   │   │       ├── Medical/  # 基建：Medical
│   │   │       ├── TrainingRoom/  # 基建：TrainingRoom
│   │   │       └── Workshop/  # 基建：Workshop
│   │   ├── Summon/  # 召喚類物品
│   │   └── Weapons/  # 武器（按職分/幹員分類）
│   │       ├── Caster/  # 武器：術士
│   │       │   ├── _12F/  # 術士 幹員：12F
│   │       │   ├── Durin/  # 術士 幹員：杜林
│   │       │   └── Lava/  # 術士 幹員：炎熔
│   │       ├── Defender/  # 武器：重裝
│   │       │   ├── Beagle/  # 重裝 幹員：米格魯
│   │       │   ├── Nian/  # 重裝 幹員：年
│   │       │   └── NoirCorne/  # 重裝 幹員：黑角
│   │       ├── Guard/  # 武器：近衛
│   │       │   ├── Chen/  # 近衛 幹員：陳
│   │       │   ├── Saki/  # 近衛 幹員：斯卡蒂
│   │       │   ├── SilverAsh/  # 近衛 幹員：銀灰
│   │       │   ├── Surtr/  # 近衛 幹員：史爾特爾
│   │       │   └── Thorns/  # 近衛 幹員：棘刺
│   │       ├── Sniper/  # 武器：狙擊
│   │       │   ├── Adnachiel/  # 狙擊 幹員：安德切爾
│   │       │   ├── Exusiai/  # 狙擊 幹員：能天使
│   │       │   ├── Kroos/  # 狙擊 幹員：KoKoDaYo
│   │       │   ├── KroosAlter/  # 狙擊 幹員：寒芒克洛絲
│   │       │   ├── Pozemka/  # 狙擊 幹員：鴻雪
│   │       │   ├── Rangers/  # 狙擊 幹員：巡林者
│   │       │   ├── Schwarz/  # 狙擊 幹員：黑
│   │       │   ├── Shirayuki/  # 狙擊 幹員：白雪
│   │       │   ├── Typhon/  # 狙擊 幹員：提豐
│   │       │   └── Wisadel/  # 狙擊 幹員：維神
│   │       ├── Specialist/  # 武器：特種
│   │       │   ├── Shaw/  # 特種 幹員：阿消
│   │       │   └── TexasAlter/  # 特種 幹員：異格德克薩斯
│   │       ├── Supporter/  # 武器：輔助
│   │       │   └── Orchid/  # 輔助 幹員：梓蘭
│   │       └── Vanguard/  # 武器：先鋒
│   │           ├── Bagpipe/  # 先鋒 幹員：風笛
│   │           ├── Fang/  # 先鋒 幹員：芬
│   │           ├── Texas/  # 先鋒 幹員：德克薩斯
│   │           └── Yato/  # 先鋒 幹員：夜刀
│   ├── NPCs/  # NPC（敵人/友方等）
│   │   ├── Enemy/  # 敵對 NPC（按章節/活動分類）
│   │   │   ├── Chapter6/  # 第 6 章敵人
│   │   │   │   └── FrostNova/  # Boss：霜星
│   │   │   ├── Evolution/  # Evolution 系列敵人
│   │   │   ├── GT/  # 活動/篇章：GT(騎兵與獵人)
│   │   │   ├── OF/  # 活動/篇章：OF(火藍之心)
│   │   │   │   └── Pmp/  # OF 子資料夾（Pmp）
│   │   │   ├── ReclamationAlgorithm/  # 生息演算相關敵人
│   │   │   │   └── Cragpincer/  # 敵人：Cragpincer
│   │   │   ├── RoaringFlare/  # 活動/篇章：RoaringFlare(大概是怒號光明的敵人)
│   │   │   │   └── ImperialArtilleyCoreTargeteer/  # Boss：IACT(帝國無人機砲手 大概吧)
│   │   │   ├── Seamonster/  # 海怪/海嗣相關敵人
│   │   │   ├── ThroughChapter4/  # 至第 4 章敵人
│   │   │   └── TillChapter7/  # 至第 7 章敵人
│   │   ├── Friendly/  # 友方 NPC
│   │   ├── Gores/  # Gore（死亡碎塊/血肉）
│   │   └── Levels/  # NPC 等級/關卡相關
│   ├── Projectiles/  # 彈幕（按職分/幹員/Boss 分類）
│   │   ├── Bosses/  # Boss 彈幕
│   │   │   └── FrostNova/  # FrostNova 彈幕
│   │   ├── Caster/  # Caster 彈幕
│   │   │   ├── _12F/  # 12F 彈幕
│   │   │   ├── Durin/  # Durin 彈幕
│   │   │   └── Lava/  # Lava 彈幕
│   │   ├── Defender/  # Defender 彈幕
│   │   │   ├── Beagle/  # Beagle 彈幕
│   │   │   ├── Nian/  # Nian 彈幕
│   │   │   └── NoirCorne/  # NoirCorne 彈幕
│   │   ├── Guard/  # Guard 彈幕
│   │   │   ├── Chen/  # Chen 彈幕
│   │   │   ├── Saki/  # Saki 彈幕
│   │   │   │   └── Assets/  # 彈幕用貼圖/資源
│   │   │   └── Thorns/  # Thorns 彈幕
│   │   ├── Sniper/  # Sniper 彈幕
│   │   │   ├── Adnachiel/  # Adnachiel 彈幕
│   │   │   ├── Exusiai/  # Exusiai 彈幕
│   │   │   ├── KroosAlter/  # KroosAlter 彈幕
│   │   │   ├── Pozemka/  # Pozemka 彈幕
│   │   │   ├── Rangers/  # Rangers 彈幕
│   │   │   ├── Schwarz/  # Schwarz 彈幕
│   │   │   ├── Shirayuki/  # Shirayuki 彈幕
│   │   │   ├── Typhon/  # Typhon 彈幕
│   │   │   └── Wisadel/  # Wisadel 彈幕
│   │   │       └── WisdelCannon/  # WisdelCannon 彈幕
│   │   ├── Specialist/  # Specialist 彈幕
│   │   │   ├── Shaw/  # Shaw 彈幕
│   │   │   └── TexasAlter/  # TexasAlter 彈幕
│   │   ├── Supporter/  # Supporter 彈幕
│   │   │   └── Orchid/  # Orchid 彈幕
│   │   └── Vanguard/  # Vanguard 彈幕
│   │       ├── Bagpipe/  # Bagpipe 彈幕
│   │       ├── Fang/  # Fang 彈幕
│   │       ├── Texas/  # Texas 彈幕
│   │       └── Yato/  # Yato 彈幕
│   ├── Rarities/  # 稀有度定義
│   ├── Textures/  # 共用貼圖
│   │   └── duaog/  # 貼圖集（duaog）
│   └── Tiles/  # 方塊（Tiles）
│       ├── Furniture/  # 家具方塊
│       └── Infrastructure/  # 基建方塊
│           ├── Canteen/  # 基建方塊：Canteen
│           ├── ControlCenter/  # 基建方塊：控制中樞
│           ├── Deck/  # 基建方塊：Deck(甲板?)
│           ├── Decorates/  # 基建方塊：Decorates(裝飾?)
│           ├── HROffice/  # 基建方塊：HROffice(招募中心)
│           ├── Medical/  # 基建方塊：Medical(醫療)
│           ├── TrainingRoom/  # 基建方塊：訓練室
│           └── Workshop/  # 基建方塊：Workshop(?)
├── Localization/  # 本地化語系（.hjson）
│   ├── en-US/  # 英文
│   └── zh-Hans/  # 简体中文
├── markdown/  # 開發筆記/歷史檔案樹
├── Players/  # ModPlayer（玩家掛載）
├── Properties/  # IDE 啟動設定
├── Sounds/  # 主要音效/音樂資源（tML 慣例）
│   ├── ImperialArtilleyCoreTargeteer/  # IACT Boss 音效（Sounds 路徑）
│   └── Music/  # 音樂（Sounds/Music）
├── Systems/  # 全域系統與 hook
│   └── Gameplay/  # 遊戲系統
│       ├── Elemental/  # 元素/理智系統
│       ├── Enums/  # 列舉/型別
│       │   └── Damageclasses/  # 自訂傷害類別/傷害邏輯
│       └── Skill/  # 技能系統核心
├── .editorconfig  # 編輯器/格式化規則
├── .gitignore  # Git 忽略規則
├── ArknightsMod.cs  # 模組入口（載入資料、註冊系統/Shader/音樂等）
├── ArknightsMod.csproj  # .NET 專案檔（tModLoader 建置設定）
├── ArknightsMod.sln  # 解決方案檔（IDE）
├── ArknightsModMenu.cs  # 模組主選單外觀（Logo/背景/BGM）
├── build.txt  # tModLoader 模組資訊與建置旗標
├── description.txt  # Workshop/Mod Browser 描述文案
├── icon.png  # 模組圖示
├── icon_old.png  # 舊版/歷史圖示
├── icon_workshop.png  # Workshop 用圖示
├── README.md  # 專案說明（英文）
├── README_JP.md  # 專案說明（日文）
├── README_ZH.md  # 專案說明（中文）
├── STRUCTURE.md  # 專案檔案樹（英文）
└── STRUCTURE_ZH.md  # 專案檔案樹（中文）
```
