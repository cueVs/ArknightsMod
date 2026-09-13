```text
All terms used below were translated using GPT. Please assist with corrections if there are any inaccuracies.

ArknightsMod/
├── .github/  # GitHub metadata (PR template, CODEOWNERS)
├── Assets/  # Data & assets loaded by code
│   ├── Effects/  # Shaders and screen/sky effects
│   ├── GrayScaleTexture/  # Grayscale textures for effects
│   ├── LevelDatas/  # CSV tables (upgrade/skill/weapon stats)
│   │   └── Skills/  # Per-skill level data (CSV)
│   ├── Menu/  # Mod menu textures
│   ├── OriginalMusic/  # Music loaded via MusicLoader.AddMusic
│   ├── SceneEffects/  # Scene effect classes (biome/space)
│   └── Sound/  # Extra sound assets referenced via Assets path
│       ├── ImperialArtilleyCoreTargeteer/  # IACT boss SFX set
│       └── WisdelCannon/  # Wisadel cannon SFX set
├── Common/  # Shared code/assets (UI, configs, particles, VFX)
│   ├── Configs/  # ModConfig definitions
│   ├── Particle/  # Particle system implementation
│   ├── UI/  # UI code + textures
│   │   ├── BattleRecord/  # Upgrade/BattleRecord UI
│   │   │   ├── Calculators/  # UI calculators (preview/stats)
│   │   │   ├── Images/  # UI image assets
│   │   │   └── UIElements/  # UI element components
│   │   ├── SkillIcons/  # Skill icon textures
│   │   └── SummonIcon/  # Summon icon textures
│   └── VisualEffects/  # Visual effect helpers (trails, shake, etc.)
├── Content/  # tModLoader content (items/NPCs/projectiles/tiles)
│   ├── BossBars/  # Custom boss bars
│   ├── Buffs/  # Buffs/debuffs
│   ├── Currencies/  # Custom currency classes
│   ├── Dusts/  # Dust (particle) definitions
│   │   └── Bosses/  # Boss-specific dusts
│   ├── Events/  # World/invasion events
│   ├── Items/  # All items
│   │   ├── Accessories/  # Accessories
│   │   │   └── Rogue/  # Rogue-mode accessories (grouped by rarity)
│   │   │       ├── Rarity_l1/  # Rarity tier L1
│   │   │       ├── Rarity_l2/  # Rarity tier L2
│   │   │       ├── Rarity_l3/  # Rarity tier L3
│   │   │       └── Rarity_l4/  # Rarity tier L4
│   │   ├── Armor/  # Armor items
│   │   │   └── Vanity/  # Vanity (cosmetic) armor
│   │   │       ├── Caster/  # Vanity: Caster
│   │   │       ├── Defender/  # Vanity: Defender
│   │   │       ├── Guard/  # Vanity: Guard
│   │   │       ├── Medic/  # Vanity: Medic
│   │   │       ├── Sniper/  # Vanity: Sniper
│   │   │       ├── Specialist/  # Vanity: Specialist
│   │   │       ├── Supporter/  # Vanity: Supporter
│   │   │       └── Vanguard/  # Vanity: Vanguard
│   │   ├── BattleRecords/  # Battle record items (upgrade UI)
│   │   ├── Bosssummon/  # Boss summon items
│   │   ├── Consumables/  # Consumable items
│   │   │   └── VanityBags/  # Vanity bags
│   │   ├── DisplayForUI/  # UI-only display items
│   │   ├── Material/  # Crafting materials
│   │   │   └── ReclamAlgor/  # Reclamation Algorithm materials
│   │   ├── Placeable/  # Placeable items (tiles/furniture)
│   │   │   ├── Banners/  # Banner items
│   │   │   ├── Furniture/  # Furniture items
│   │   │   └── Infrastructure/  # Infrastructure items (base building)
│   │   │       ├── Canteen/  # Infrastructure: Canteen
│   │   │       ├── ControlCenter/  # Infrastructure: Control Center
│   │   │       ├── Deck/  # Infrastructure: Deck
│   │   │       ├── Decorates/  # Infrastructure: Decorations
│   │   │       ├── HROffice/  # Infrastructure: HR Office
│   │   │       ├── Medical/  # Infrastructure: Medical
│   │   │       ├── TrainingRoom/  # Infrastructure: Training Room
│   │   │       └── Workshop/  # Infrastructure: Workshop
│   │   ├── Summon/  # Summon items
│   │   └── Weapons/  # Weapons (grouped by class/operator)
│   │       ├── Caster/  # Weapons: Caster
│   │       │   ├── _12F/  # Caster operator: 12F
│   │       │   ├── Durin/  # Caster operator: Durin
│   │       │   └── Lava/  # Caster operator: Lava
│   │       ├── Defender/  # Weapons: Defender
│   │       │   ├── Beagle/  # Defender operator: Beagle
│   │       │   ├── Nian/  # Defender operator: Nian
│   │       │   └── NoirCorne/  # Defender operator: Noir Corne
│   │       ├── Guard/  # Weapons: Guard
│   │       │   ├── Chen/  # Guard operator: Ch'en
│   │       │   ├── Saki/  # Guard operator: Saki
│   │       │   ├── SilverAsh/  # Guard operator: SilverAsh
│   │       │   ├── Surtr/  # Guard operator: Surtr
│   │       │   └── Thorns/  # Guard operator: Thorns
│   │       ├── Sniper/  # Weapons: Sniper
│   │       │   ├── Adnachiel/  # Sniper operator: Adnachiel
│   │       │   ├── Exusiai/  # Sniper operator: Exusiai
│   │       │   ├── Kroos/  # Sniper operator: Kroos
│   │       │   ├── KroosAlter/  # Sniper operator: Kroos (Alter)
│   │       │   ├── Pozemka/  # Sniper operator: Pozëmka
│   │       │   ├── Rangers/  # Sniper operator: Rangers
│   │       │   ├── Schwarz/  # Sniper operator: Schwarz
│   │       │   ├── Shirayuki/  # Sniper operator: Shirayuki
│   │       │   ├── Typhon/  # Sniper operator: Typhon
│   │       │   └── Wisadel/  # Sniper operator: Wis'adel
│   │       ├── Specialist/  # Weapons: Specialist
│   │       │   ├── Shaw/  # Specialist operator: Shaw
│   │       │   └── TexasAlter/  # Specialist operator: Texas (Alter)
│   │       ├── Supporter/  # Weapons: Supporter
│   │       │   └── Orchid/  # Supporter operator: Orchid
│   │       └── Vanguard/  # Weapons: Vanguard
│   │           ├── Bagpipe/  # Vanguard operator: Bagpipe
│   │           ├── Fang/  # Vanguard operator: Fang
│   │           ├── Texas/  # Vanguard operator: Texas
│   │           └── Yato/  # Vanguard operator: Yato
│   ├── NPCs/  # NPCs (enemies, friendly, etc.)
│   │   ├── Enemy/  # Enemy NPCs (grouped by chapter/event)
│   │   │   ├── Chapter6/  # Enemies: Chapter 6
│   │   │   │   └── FrostNova/  # Boss: FrostNova
│   │   │   ├── Evolution/  # Enemies: Evolution
│   │   │   ├── GT/  # Enemies: event/arc GT
│   │   │   ├── OF/  # Enemies: event/arc OF
│   │   │   │   └── Pmp/  # OF subfolder (Pmp)
│   │   │   ├── ReclamationAlgorithm/  # Enemies: Reclamation Algorithm
│   │   │   │   └── Cragpincer/  # Enemy: Cragpincer
│   │   │   ├── RoaringFlare/  # Enemies: Roaring Flare
│   │   │   │   └── ImperialArtilleyCoreTargeteer/  # Boss: Imperial Artillery Core Targeteer
│   │   │   ├── Seamonster/  # Enemies: Seamonsters
│   │   │   ├── ThroughChapter4/  # Enemies: through Chapter 4
│   │   │   └── TillChapter7/  # Enemies: till Chapter 7
│   │   ├── Friendly/  # Friendly NPCs
│   │   ├── Gores/  # Gore pieces
│   │   └── Levels/  # NPC level/variants
│   ├── Projectiles/  # Projectiles (grouped by class/operator/boss)
│   │   ├── Bosses/  # Boss projectiles
│   │   │   └── FrostNova/  # FrostNova projectiles
│   │   ├── Caster/  # Caster projectiles
│   │   │   ├── _12F/  # 12F projectiles
│   │   │   ├── Durin/  # Durin projectiles
│   │   │   └── Lava/  # Lava projectiles
│   │   ├── Defender/  # Defender projectiles
│   │   │   ├── Beagle/  # Beagle projectiles
│   │   │   ├── Nian/  # Nian projectiles
│   │   │   └── NoirCorne/  # Noir Corne projectiles
│   │   ├── Guard/  # Guard projectiles
│   │   │   ├── Chen/  # Ch'en projectiles
│   │   │   ├── Saki/  # Saki projectiles
│   │   │   │   └── Assets/  # Projectile textures/assets
│   │   │   └── Thorns/  # Thorns projectiles
│   │   ├── Sniper/  # Sniper projectiles
│   │   │   ├── Adnachiel/  # Adnachiel projectiles
│   │   │   ├── Exusiai/  # Exusiai projectiles
│   │   │   ├── KroosAlter/  # Kroos (Alter) projectiles
│   │   │   ├── Pozemka/  # Pozëmka projectiles
│   │   │   ├── Rangers/  # Rangers projectiles
│   │   │   ├── Schwarz/  # Schwarz projectiles
│   │   │   ├── Shirayuki/  # Shirayuki projectiles
│   │   │   ├── Typhon/  # Typhon projectiles
│   │   │   └── Wisadel/  # Wis'adel projectiles
│   │   │       └── WisdelCannon/  # Wisdel cannon projectiles
│   │   ├── Specialist/  # Specialist projectiles
│   │   │   ├── Shaw/  # Shaw projectiles
│   │   │   └── TexasAlter/  # Texas (Alter) projectiles
│   │   ├── Supporter/  # Supporter projectiles
│   │   │   └── Orchid/  # Orchid projectiles
│   │   └── Vanguard/  # Vanguard projectiles
│   │       ├── Bagpipe/  # Bagpipe projectiles
│   │       ├── Fang/  # Fang projectiles
│   │       ├── Texas/  # Texas projectiles
│   │       └── Yato/  # Yato projectiles
│   ├── Rarities/  # Rarity definitions
│   ├── Textures/  # Shared textures
│   │   └── duaog/  # Misc texture set (duaog)
│   └── Tiles/  # Tiles and related placeables
│       ├── Furniture/  # Furniture tiles
│       └── Infrastructure/  # Infrastructure tiles (base building)
│           ├── Canteen/  # Infrastructure tiles: Canteen
│           ├── ControlCenter/  # Infrastructure tiles: Control Center
│           ├── Deck/  # Infrastructure tiles: Deck
│           ├── Decorates/  # Infrastructure tiles: Decorations
│           ├── HROffice/  # Infrastructure tiles: HR Office
│           ├── Medical/  # Infrastructure tiles: Medical
│           ├── TrainingRoom/  # Infrastructure tiles: Training Room
│           └── Workshop/  # Infrastructure tiles: Workshop
├── Localization/  # Localization packs (.hjson)
│   ├── en-US/  # English localization
│   └── zh-Hans/  # Simplified Chinese localization
├── markdown/  # Dev notes / historical trees
├── Players/  # ModPlayer implementations
├── Properties/  # IDE launch profiles
├── Sounds/  # Main sound assets (tML convention)
│   ├── ImperialArtilleyCoreTargeteer/  # IACT boss SFX (Sounds path)
│   └── Music/  # Music tracks (Sounds/Music)
├── Systems/  # Global systems and hooks
│   └── Gameplay/  # Gameplay systems
│       ├── Elemental/  # Elemental/sanity system
│       ├── Enums/  # Gameplay enums/types
│       │   └── Damageclasses/  # Custom DamageClass and damage logic
│       └── Skill/  # Skill system core
├── .editorconfig  # Editor/formatting rules
├── .gitignore  # Git ignore rules
├── ArknightsMod.cs  # Mod entry point (Load/register data, shaders, music, etc.)
├── ArknightsMod.csproj  # .NET project file (tModLoader build settings)
├── ArknightsMod.sln  # Solution file (IDE)
├── ArknightsModMenu.cs  # Mod menu visuals (logo/background/BGM)
├── build.txt  # tModLoader mod metadata & build flags
├── description.txt  # Workshop/Mod Browser description text
├── icon.png  # Mod icon
├── icon_old.png  # Legacy/previous icon
├── icon_workshop.png  # Workshop icon variant
├── README.md  # Project README (English)
├── README_JP.md  # Project README (Japanese)
├── README_ZH.md  # Project README (Chinese)
├── STRUCTURE.md  # Project tree (English)
└── STRUCTURE_ZH.md  # Project tree (Chinese)
```
