using ArknightsMod.Common.UI.BattleRecord.Calculators;
using ArknightsMod.Common.UI.BattleRecord.UIElements;
using ArknightsMod.Content.Items;
using ArknightsMod.Content.Items.BattleRecords;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArknightsMod.Common.UI.BattleRecord
{
	internal class UpgradeUIState : UIState
	{
		private const string IMAGE_PATH = "ArknightsMod/Common/UI/BattleRecord/Images/";
		private const int BATTLE_RECORD_TYPES_COUNT = 4;
		public static UpgradeUIState Instance;
		public bool IsVisible => _visible;

		public UpgradeItemBase UpgradeItem {
			get => _itemSlot.UpgradeItem;
			set {
				_itemSlot.UpgradeItem = value;
				Refresh();
			}
		}

		private bool _visible = false;
		private UIImage _panel;
		private UIUpgradeItemSlot _itemSlot;
		private UILevelSelectorRefactor _levelSelector;
		private UIImageButton _upgradeButton;
		private UIText _upgradeButtonText;
		private UIImage[] _battleRecord = new UIImage[BATTLE_RECORD_TYPES_COUNT];
		private UIBlock[] _battleRecordCountBlock = new UIBlock[BATTLE_RECORD_TYPES_COUNT];
		private UIText[] _battleRecordCountText = new UIText[BATTLE_RECORD_TYPES_COUNT];
		private UIText[] _battleRecordUPCountText = new UIText[BATTLE_RECORD_TYPES_COUNT];
		private UILevelBar _levelBar = new UILevelBar();
		private UIUpgradePreview _upgradePreview;

		private ExperienceCalculator _experienceCalculator = new ExperienceCalculator();
		private BattleRecordCalculator _battleRecordCalculator = new BattleRecordCalculator();

		public UpgradeUIState() {
			Instance = this;
		}

		public override void OnInitialize() {
			base.OnInitialize();
			_panel = new UIImage(ModContent.Request<Texture2D>(IMAGE_PATH + "Panel", AssetRequestMode.ImmediateLoad));
			_panel.Left.Set(-_panel.Width.Pixels / 2f, 0.5f);
			_panel.Top.Set(-_panel.Height.Pixels / 2f, 0.5f);
			_panel.SetPadding(1f);
			Append(_panel);

			UIImageButton closeButton = new UIImageButton(ModContent.Request<Texture2D>(IMAGE_PATH + "CloseButton", AssetRequestMode.ImmediateLoad));
			closeButton.Left.Set(-closeButton.Width.Pixels, 1f);
			closeButton.Top.Set(0f, 0f);
			closeButton.OnLeftClick += (evt, element) => {
				Close();
			};
			_panel.Append(closeButton);

			_itemSlot = new UIUpgradeItemSlot();
			_itemSlot.Left.Set(18f, 0f);
			_itemSlot.Top.Set(-_itemSlot.Height.Pixels / 2f, 0.5f);
			_itemSlot.OnUpgradeItemChange += _itemSlot_OnUpgradeItemChange;
			_panel.Append(_itemSlot);

			_upgradePreview = new UIUpgradePreview();
			_upgradePreview.Width = _itemSlot.Width;
			_upgradePreview.Height = _itemSlot.Height;
			_upgradePreview.Height.Pixels += 44f;
			_upgradePreview.Left = _itemSlot.Left;
			_upgradePreview.Left.Pixels += _itemSlot.Width.Pixels + 20f;
			_upgradePreview.Left.Percent += _itemSlot.Width.Percent;
			_upgradePreview.Top = _itemSlot.Top;
			_upgradePreview.Top.Pixels -= 22f;
			_upgradePreview.BattleRecordCalculator = _battleRecordCalculator;
			_upgradePreview.ExperienceCalculator = _experienceCalculator;
			_panel.Append(_upgradePreview);

			_levelBar = new UILevelBar();
			_levelBar.Width.Set(244f, 0f);
			_levelBar.Height.Set(18f, 0f);
			_levelBar.Left.Set(58f, 0f);
			_levelBar.Top = _itemSlot.Top;
			_levelBar.Top.Pixels -= _levelBar.Height.Pixels + 28f;
			_panel.Append(_levelBar);

			UIText levelText = new UIText("Lv");
			levelText.Left = _levelBar.Left;
			levelText.Left.Pixels -= levelText.MinWidth.Pixels + 10f;
			levelText.Top = _levelBar.Top;
			levelText.Top.Pixels += _levelBar.Height.Pixels / 2f - levelText.MinHeight.Pixels / 2f;
			_panel.Append(levelText);

			_levelSelector = new UILevelSelectorRefactor();
			_levelSelector.Left.Set(-_levelSelector.Width.Pixels - 24f, 1f);
			_levelSelector.Top.Set(-_levelSelector.Height.Pixels / 2f, 0.5f);
			_panel.Append(_levelSelector);
			_levelSelector.OnRound += e => {
				if (_itemSlot.UpgradeItem == null)
					return;
				_battleRecordCalculator.PlanBuyget(_experienceCalculator.GetUpgradeToLevelExperience(_levelSelector.Level), _levelSelector.Level != _itemSlot.UpgradeItem.LevelMax);
				_experienceCalculator.ExperienceBudget = _battleRecordCalculator.BuygetExperience;
				_levelSelector.Level = _itemSlot.UpgradeItem.Level + _experienceCalculator.UpgradeLevelPreview;
			};

			_upgradeButton = new UIImageButton(ModContent.Request<Texture2D>(IMAGE_PATH + "UpgradeButton", AssetRequestMode.ImmediateLoad));
			_upgradeButton.Left.Set(-_upgradeButton.Width.Pixels - 30f, 1f);
			_upgradeButton.Top.Set(-_upgradeButton.Height.Pixels - 18f, 1f);
			_upgradeButton.OnLeftClick += (evt, element) => {
				if (_itemSlot.UpgradeItem == null)
					return;
				_itemSlot.UpgradeItem.Experience += _battleRecordCalculator.BuygetExperience;
				_itemSlot.UpgradeItem.CheckUpgrade();
				_battleRecordCalculator.Fulfilled();
				_experienceCalculator.ExperienceBudget = _battleRecordCalculator.BuygetExperience;
				_levelSelector.Level = _itemSlot.UpgradeItem.Level + _experienceCalculator.UpgradeLevelPreview;
			};
			_panel.Append(_upgradeButton);

			_upgradeButtonText = new UIText(string.Empty);
			_upgradeButtonText.TextColor = Color.Lerp(Color.Gray, Color.White, 0.4f);
			_upgradeButtonText.Left.Set(-_upgradeButtonText.MinWidth.Pixels / 2f, 0.5f);
			_upgradeButtonText.Left.Pixels += 10f;
			_upgradeButtonText.Top.Set(-_upgradeButtonText.MinHeight.Pixels / 2f, 0.5f);
			_upgradeButtonText.Top.Pixels -= 2f;
			_upgradeButtonText.IgnoresMouseInteraction = true;
			_upgradeButton.Append(_upgradeButtonText);
			_upgradeButton.OnMouseOver += (evt, element) => {
				_upgradeButtonText.TextColor = Color.Lerp(Color.Gray, Color.White, 1f);
			};
			_upgradeButton.OnMouseOut += (evt, element) => {
				_upgradeButtonText.TextColor = Color.Lerp(Color.Gray, Color.White, 0.4f);
			};

			_battleRecord[0] = new UIImage(ModContent.Request<Texture2D>(IMAGE_PATH + "DrillBattleRecord", AssetRequestMode.ImmediateLoad));
			_battleRecord[0].Left.Set(22f, 0f);
			_battleRecord[0].Top.Set(-_battleRecord[0].Height.Pixels - 22f, 1f);
			_battleRecord[0].OnLeftClick += (evt, element) => {
				if (_itemSlot.UpgradeItem == null)
					return;
				if (_battleRecordCalculator.BuygetExperience < _experienceCalculator.UpgradeMaxLevelNeedExperience) {
					_battleRecordCalculator[typeof(DrillBattleRecord), true]++;
					_battleRecordCalculator.RefresBudget();
					_experienceCalculator.ExperienceBudget = _battleRecordCalculator.BuygetExperience;
				}
				_levelSelector.Level = _itemSlot.UpgradeItem.Level + _experienceCalculator.UpgradeLevelPreview;
			};
			_panel.Append(_battleRecord[0]);

			_battleRecord[1] = new UIImage(ModContent.Request<Texture2D>(IMAGE_PATH + "FrontlineBattleRecord", AssetRequestMode.ImmediateLoad));
			_battleRecord[1].Left.Set(_battleRecord[0].Left.Pixels + _battleRecord[0].Width.Pixels + 114f, 0f);
			_battleRecord[1].Top = _battleRecord[0].Top;
			_battleRecord[1].OnLeftClick += (evt, element) => {
				if (_itemSlot.UpgradeItem == null)
					return;
				if (_battleRecordCalculator.BuygetExperience < _experienceCalculator.UpgradeMaxLevelNeedExperience) {
					_battleRecordCalculator[typeof(FrontlineBattleRecord), true]++;
					_battleRecordCalculator.RefresBudget();
					_experienceCalculator.ExperienceBudget = _battleRecordCalculator.BuygetExperience;
				}
				_levelSelector.Level = _itemSlot.UpgradeItem.Level + _experienceCalculator.UpgradeLevelPreview;
			};
			_panel.Append(_battleRecord[1]);

			_battleRecord[3] = new UIImage(ModContent.Request<Texture2D>(IMAGE_PATH + "StrategicBattleRecord", AssetRequestMode.ImmediateLoad));
			_battleRecord[3].Left.Set(22f, 0f);
			_battleRecord[3].Top.Set(-_battleRecord[3].Height.Pixels - 72f, 1f);
			_battleRecord[3].OnLeftClick += (evt, element) => {
				if (_itemSlot.UpgradeItem == null)
					return;
				if (_battleRecordCalculator.BuygetExperience < _experienceCalculator.UpgradeMaxLevelNeedExperience) {
					_battleRecordCalculator[typeof(StrategicBattleRecord), true]++;
					_battleRecordCalculator.RefresBudget();
					_experienceCalculator.ExperienceBudget = _battleRecordCalculator.BuygetExperience;
				}
				_levelSelector.Level = _itemSlot.UpgradeItem.Level + _experienceCalculator.UpgradeLevelPreview;
			};
			_panel.Append(_battleRecord[3]);

			_battleRecord[2] = new UIImage(ModContent.Request<Texture2D>(IMAGE_PATH + "TacticalBattleRecord", AssetRequestMode.ImmediateLoad));
			_battleRecord[2].Left.Set(_battleRecord[3].Left.Pixels + _battleRecord[3].Width.Pixels + 114f, 0f);
			_battleRecord[2].Top = _battleRecord[3].Top;
			_battleRecord[2].OnLeftClick += (evt, element) => {
				if (_itemSlot.UpgradeItem == null)
					return;
				if (_battleRecordCalculator.BuygetExperience < _experienceCalculator.UpgradeMaxLevelNeedExperience) {
					_battleRecordCalculator[typeof(TacticalBattleRecord), true]++;
					_battleRecordCalculator.RefresBudget();
					_experienceCalculator.ExperienceBudget = _battleRecordCalculator.BuygetExperience;
				}
				_levelSelector.Level = _itemSlot.UpgradeItem.Level + _experienceCalculator.UpgradeLevelPreview;
			};
			_panel.Append(_battleRecord[2]);

			for (int i = 0; i < BATTLE_RECORD_TYPES_COUNT; i++) {
				_battleRecordCountBlock[i] = new UIBlock();
				_battleRecordCountBlock[i].Width.Set(102f, 0f);
				_battleRecordCountBlock[i].Height = _battleRecord[i].Height;
				_battleRecordCountBlock[i].Left.Set(_battleRecord[i].Left.Pixels + _battleRecord[i].Width.Pixels, 0f);
				_battleRecordCountBlock[i].Left.Pixels += 5f;
				_battleRecordCountBlock[i].Top = _battleRecord[i].Top;
				_panel.Append(_battleRecordCountBlock[i]);

				_battleRecordCountText[i] = new UIText("-");
				_battleRecordUPCountText[i] = new UIText(string.Empty);
				_battleRecordUPCountText[i].TextColor = new Color(254, 145, 2);

				Vector2 size = new Vector2(_battleRecordCountText[i].MinWidth.Pixels + _battleRecordUPCountText[i].MinWidth.Pixels,
					MathHelper.Max(_battleRecordCountText[i].MinHeight.Pixels, _battleRecordUPCountText[i].MinHeight.Pixels));
				_battleRecordCountText[i].Left.Set(-size.X / 2f, 0.5f);
				_battleRecordCountText[i].Top.Set(-size.Y / 2f, 0.5f);
				_battleRecordUPCountText[i].Left = _battleRecordCountText[i].Left;
				_battleRecordUPCountText[i].Left.Pixels += _battleRecordCountText[i].MinWidth.Pixels;
				_battleRecordUPCountText[i].Top = _battleRecordCountText[i].Top;

				_battleRecordCountBlock[i].Append(_battleRecordCountText[i]);
				_battleRecordCountBlock[i].Append(_battleRecordUPCountText[i]);
			}
		}

		private void _itemSlot_OnUpgradeItemChange(UIUpgradeItemSlot obj) {
			var upgradeItem = obj.UpgradeItem;
			_battleRecordCalculator.ClearBuyget();
			_experienceCalculator.UpgradeItem = upgradeItem;
			_upgradePreview.UpgradeItem = upgradeItem;
			if (upgradeItem != null) {
				_levelSelector.SetLevel(upgradeItem.LevelMax, upgradeItem.Level);
			}
			else {
				_levelSelector.SetToDefaultStatus();
			}
		}

		public void Refresh() {
			_battleRecordCalculator.ClearBuyget();
			_experienceCalculator.UpgradeItem = UpgradeItem;
			_upgradePreview.UpgradeItem = UpgradeItem;
			if (UpgradeItem != null) {
				_levelSelector.SetLevel(UpgradeItem.LevelMax, UpgradeItem.Level);
			}
			else {
				_levelSelector.SetToDefaultStatus();
			}
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			_battleRecordCalculator.RefreshCount();
			_experienceCalculator.ExperienceBudget = _battleRecordCalculator.BuygetExperience;

			if (_itemSlot.UpgradeItem != null)
				_levelSelector.MaxLevel = _itemSlot.UpgradeItem.Level + _experienceCalculator.GetUpgradeLevelForExperience(_battleRecordCalculator.SumExperience);

			_upgradeButtonText.SetText(Language.GetTextValue("Mods.ArknightsMod.UI.UpgradeUI.Upgrade"));
			_upgradeButtonText.Left.Set(-_upgradeButtonText.MinWidth.Pixels / 2f, 0.5f);
			_upgradeButtonText.Left.Pixels += 10f;
			_upgradeButtonText.Top.Set(-_upgradeButtonText.MinHeight.Pixels / 2f, 0.5f);
			_upgradeButtonText.Top.Pixels -= 2f;

			_battleRecordCountText[0].SetText(_battleRecordCalculator[typeof(DrillBattleRecord)].ToString());
			_battleRecordUPCountText[0].SetText($"(-{_battleRecordCalculator[typeof(DrillBattleRecord), true]})");
			_battleRecordCountText[1].SetText(_battleRecordCalculator[typeof(FrontlineBattleRecord)].ToString());
			_battleRecordUPCountText[1].SetText($"(-{_battleRecordCalculator[typeof(FrontlineBattleRecord), true]})");
			_battleRecordCountText[2].SetText(_battleRecordCalculator[typeof(TacticalBattleRecord)].ToString());
			_battleRecordUPCountText[2].SetText($"(-{_battleRecordCalculator[typeof(TacticalBattleRecord), true]})");
			_battleRecordCountText[3].SetText(_battleRecordCalculator[typeof(StrategicBattleRecord)].ToString());
			_battleRecordUPCountText[3].SetText($"(-{_battleRecordCalculator[typeof(StrategicBattleRecord), true]})");

			for (int i = 0; i < BATTLE_RECORD_TYPES_COUNT; i++) {
				_battleRecordCountBlock[i].Width.Set(102f, 0f);
				_battleRecordCountBlock[i].Height = _battleRecord[i].Height;
				_battleRecordCountBlock[i].Left.Set(_battleRecord[i].Left.Pixels + _battleRecord[i].Width.Pixels, 0f);
				_battleRecordCountBlock[i].Left.Pixels += 5f;
				_battleRecordCountBlock[i].Top = _battleRecord[i].Top;

				Vector2 size = new Vector2(_battleRecordCountText[i].MinWidth.Pixels + _battleRecordUPCountText[i].MinWidth.Pixels,
					MathHelper.Max(_battleRecordCountText[i].MinHeight.Pixels, _battleRecordUPCountText[i].MinHeight.Pixels));
				_battleRecordCountText[i].Left.Set(-size.X / 2f, 0.5f);
				_battleRecordCountText[i].Top.Set(-size.Y / 2f, 0.5f);
				_battleRecordUPCountText[i].Left = _battleRecordCountText[i].Left;
				_battleRecordUPCountText[i].Left.Pixels += _battleRecordCountText[i].MinWidth.Pixels;
				_battleRecordUPCountText[i].Top = _battleRecordCountText[i].Top;
			}

			float levelPreview = _experienceCalculator.UpgradeLevelPreview;
			float expBuyget = _experienceCalculator.ExperienceBudgetPreview;
			float exp = levelPreview > 0 ? 0 : _experienceCalculator.UpgradeItem == null ? 0 : _experienceCalculator.UpgradeItem.Experience;
			expBuyget -= exp;
			float expMax = _experienceCalculator.UpgradeLevelPreviewExperience;
			_levelBar.ExperiencePercent = exp / expMax;
			_levelBar.PreviewExperiencePercent = expBuyget / expMax;
			Recalculate();
		}

		public void Show() {
			_visible = true;
			Main.playerInventory = true;
		}

		public void Close() {
			_visible = false;
		}
	}

	internal class UpgradeSystem : ModSystem
	{
		private UserInterface UpgradeUserInterface;

		internal UpgradeUIState UpgradeUIState;

		public override void Load() {
			if (!Main.dedServ) {
				UpgradeUIState = new();
				UpgradeUserInterface = new();
				UpgradeUserInterface.SetState(UpgradeUIState);
			}
		}

		public override void UpdateUI(GameTime gameTime) {
			if (UpgradeUIState.IsVisible)
				UpgradeUserInterface?.Update(gameTime);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars"));
			if (resourceBarIndex != -1) {
				layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
					"ArknightsMod: Upgrade Item",
					delegate {
						if (UpgradeUIState.IsVisible)
							UpgradeUserInterface.Draw(Main.spriteBatch, Main.gameTimeCache);
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}