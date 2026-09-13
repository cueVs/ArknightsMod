using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;
using ArknightsMod.Content.Tiles;

namespace ArknightsMod.Content.Systems
{
	internal class ElevatorUIState : UIState
	{
		private const bool ElevatorUiTextEnabled = false;
		private static readonly SoundStyle ElevatorPressSound = new SoundStyle("ArknightsMod/Content/Sounds/电梯_按下按钮");
		// 楼层按钮列（右下角，无背景）
		private UIPanel _floorPanel;
		private UIList _floorList;

		// 设置窗口（屏幕中心，可关闭）
		private UIPanel _settingsPanel;
		private UIText _title;
		private UITextPanel<string> _closeButton;
		private UITextPanel<string> _modeButton;
		private UIPanel _debugPanel;
		private UIText _debugText;
		private float _debugScrollOffsetPx;
		private float _debugContentHeightPx;

		private int _targetX;
		private int _targetY;
		private int _targetTeId = -1;
		private readonly List<int> _floorBottomYs = new List<int>();
		private int _selectedFloorIndex;

		private bool _settingsVisible;
		private bool _debugExpanded = true;
		private string _debugRawText = "";

		// 设置面板刚打开后的宽限期：这段时间内不允许“走远自动关闭”逻辑触发，
		// 避免开窗当帧因 TE 成帧/同步时序导致的瞬时解析失败把面板一闪关掉。
		private ulong _settingsOpenedAtTick;
		private const ulong SettingsOpenGraceTicks = 30;

		private const int FloorPanelWidthPx = 220;
		private const int FloorPanelHeightPx = 300;
		private const int FloorMarginRightPx = 260;
		private const int FloorMarginBottomPx = 70;

		private const int SettingsPanelWidthPx = 360;
		private const int SettingsPanelHeightPx = 300;
		private const int SettingsPanelPaddingPx = 10;
		private const int TitleTopPx = 6;
		private const int CloseButtonSizePx = 28;
		private const int ModeButtonTopPx = 38;
		private const int ModeButtonHeightPx = 32;
		private const int SectionGapPx = 10;
		private const int DebugPanelTopPx = ModeButtonTopPx + ModeButtonHeightPx + SectionGapPx;
		private const int DebugCollapsedHeightPx = 28;
		private const int DebugExpandedHeightPx = 168;
		private const int SettingsPanelYOffsetPx = 18;
		private const float DebugLineHeightPx = 18f;
		private const float DebugScrollStepPx = 20f;

		public int TargetX => _targetX;
		public int TargetY => _targetY;
		public bool IsSettingsVisible => _settingsVisible;
		public bool ShouldKeepSettingsOpen(Player player)
		{
			if (!_settingsVisible || player == null || !player.active)
				return false;

			// 宽限期内强制保持打开，避免开窗瞬间被自动关闭逻辑误关（一闪消失）。
			if (Main.GameUpdateCount - _settingsOpenedAtTick < SettingsOpenGraceTicks)
				return true;

			if (TryGetTargetElevator(out TEElevator te))
				return TEElevator.IsPlayerInElevatorRange(player, te);

			return TEElevator.TryFindNearbyElevatorByWorld(
				(_targetX + 2f) * 16f,
				(_targetY + 3.5f) * 16f,
				maxDistanceTiles: 16,
				out TEElevator nearTe,
				out _,
				out _) && TEElevator.IsPlayerInElevatorRange(player, nearTe);
		}

		public bool IsMouseOverFloorPanel => _floorPanel != null && _floorPanel.ContainsPoint(Main.MouseScreen);
		public bool IsMouseOverSettingsPanel =>
			_settingsVisible && _settingsPanel != null && _settingsPanel.ContainsPoint(Main.MouseScreen);

		public override void OnInitialize()
		{
			// 右下角楼层按钮列
			_floorPanel = new UIPanel();
			_floorPanel.SetPadding(0);
			_floorPanel.Width.Set(FloorPanelWidthPx, 0f);
			_floorPanel.Height.Set(FloorPanelHeightPx, 0f);
			_floorPanel.Left.Set(-(FloorPanelWidthPx + FloorMarginRightPx), 1f);
			_floorPanel.Top.Set(-(FloorPanelHeightPx + FloorMarginBottomPx), 1f);
			_floorPanel.BackgroundColor = Color.Transparent;
			_floorPanel.BorderColor = Color.Transparent;
			Append(_floorPanel);

			_floorList = new UIList();
			_floorList.Width.Set(0f, 1f);
			_floorList.Height.Set(0f, 1f);
			_floorList.Top.Set(0f, 0f);
			_floorList.ListPadding = 6f;
			_floorPanel.Append(_floorList);

			// 屏幕中心设置窗口
			_settingsPanel = new UIPanel();
			_settingsPanel.SetPadding(SettingsPanelPaddingPx);
			_settingsPanel.Width.Set(SettingsPanelWidthPx, 0f);
			_settingsPanel.Height.Set(SettingsPanelHeightPx, 0f);
			_settingsPanel.Left.Set(-SettingsPanelWidthPx / 2f, 0.5f);
			_settingsPanel.Top.Set(-2000f, 0.5f); // 默认隐藏
			Append(_settingsPanel);

			_title = new UIText("电梯设置", 1f, large: false);
			_title.HAlign = 0.5f;
			_title.Width.Set(-(CloseButtonSizePx + 12f), 1f);
			_title.Top.Set(TitleTopPx, 0f);
			_settingsPanel.Append(_title);

			_closeButton = new UITextPanel<string>("X");
			_closeButton.Width.Set(CloseButtonSizePx, 0f);
			_closeButton.Height.Set(CloseButtonSizePx, 0f);
			_closeButton.HAlign = 0f;
			_closeButton.VAlign = 0f;
			float settingsInnerWidth = SettingsPanelWidthPx - SettingsPanelPaddingPx * 2f;
			_closeButton.Left.Set(settingsInnerWidth - CloseButtonSizePx, 0f);
			_closeButton.Top.Set(TitleTopPx, 0f);
			_closeButton.OnLeftClick += (_, __) => HideSettings();
			_modeButton = new UITextPanel<string>("");
			_modeButton.Width.Set(0f, 1f);
			_modeButton.Height.Set(ModeButtonHeightPx, 0f);
			_modeButton.HAlign = 0f;
			_modeButton.VAlign = 0f;
			_modeButton.Left.Set(0f, 0f);
			_modeButton.Top.Set(ModeButtonTopPx, 0f);
			_modeButton.PaddingLeft = 8f;
			_modeButton.PaddingRight = 8f;
			_modeButton.OnLeftClick += (_, __) =>
			{
				if (TryGetTargetElevator(out TEElevator te))
				{
					te.CycleFloorDetectMode();
					te.ScanFloors();
					RefreshModeButtonText();
					RefreshDebugText();
					RebuildFloorButtons();
				}
			};
			_settingsPanel.Append(_modeButton);

			_debugPanel = new UIPanel();
			_debugPanel.SetPadding(6);
			_debugPanel.OverflowHidden = true; // 限制文本只在调试框内显示
			_debugPanel.Left.Set(0f, 0f);
			_debugPanel.Top.Set(DebugPanelTopPx, 0f);
			// 右侧留一点空，避免文字贴边/溢出观感很差
			_debugPanel.Width.Set(-8f, 1f);
			_debugPanel.Height.Set(DebugCollapsedHeightPx, 0f);
			_settingsPanel.Append(_debugPanel);

			_debugText = new UIText("", 0.82f);
			_debugText.HAlign = 0f;
			_debugText.VAlign = 0f;
			_debugText.Left.Set(0f, 0f);
			_debugText.Top.Set(0f, 0f);
			_debugPanel.Append(_debugText);

			// 最后添加，保证关闭按钮在最上层且易于点击。
			_settingsPanel.Append(_closeButton);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			if (!_settingsVisible)
				return;

			// 鼠标悬停调试面板时，滚轮滚动调试文字
			if (_debugPanel.ContainsPoint(Main.MouseScreen))
			{
				int wheelDelta = PlayerInput.ScrollWheelDelta / 120;
				if (wheelDelta != 0)
				{
					_debugScrollOffsetPx -= wheelDelta * DebugScrollStepPx;
					ClampDebugScrollOffset();
					ApplyDebugScrollOffset();
				}
			}
		}

		public void SetFloorTarget(int topLeftX, int topLeftY, bool resetSelection)
		{
			if (_targetX != topLeftX || _targetY != topLeftY)
				_targetTeId = -1;
			_targetX = topLeftX;
			_targetY = topLeftY;
			if (TryGetTargetElevator(out TEElevator te))
				_targetTeId = te.ID;
			if (resetSelection)
				_selectedFloorIndex = 0;
			RebuildFloorButtons(autoSelectNearest: resetSelection);
		}

		public void ShowSettings(int topLeftX, int topLeftY)
		{
			_settingsVisible = true;
			_settingsOpenedAtTick = Main.GameUpdateCount;
			_targetTeId = -1;
			_targetX = topLeftX;
			_targetY = topLeftY;
			if (TryGetTargetElevator(out TEElevator te))
				_targetTeId = te.ID;
			RefreshModeButtonText();
			RefreshSettingsLayout();
			RefreshDebugText();
		}

		public void HideSettings()
		{
			_settingsVisible = false;
			RefreshSettingsLayout();
		}

		public void ScrollSelection(int wheelSteps)
		{
			if (_floorBottomYs.Count == 0 || wheelSteps == 0)
				return;
			_selectedFloorIndex -= wheelSteps;
			if (_selectedFloorIndex < 0)
				_selectedFloorIndex = _floorBottomYs.Count - 1;
			else if (_selectedFloorIndex >= _floorBottomYs.Count)
				_selectedFloorIndex = 0;
			RebuildFloorButtons();
		}

		public bool ConfirmSelectedFloor()
		{
			if (_floorBottomYs.Count == 0)
				return false;
			if (!TryGetTargetElevator(out TEElevator te))
				return false;
			int bottomY = _floorBottomYs[_selectedFloorIndex];
			TEElevator.RequestMoveToFloor(te.ID, bottomY);
			SoundEngine.PlaySound(ElevatorPressSound, Main.LocalPlayer?.Center ?? Vector2.Zero);
			return true;
		}

		private void RefreshSettingsLayout()
		{
			float hiddenTop = -2000f;
			_settingsPanel.Top.Set(_settingsVisible ? (-SettingsPanelHeightPx / 2f + SettingsPanelYOffsetPx) : hiddenTop, 0.5f);
			_debugPanel.Height.Set(_debugExpanded ? DebugExpandedHeightPx : DebugCollapsedHeightPx, 0f);
			ClampDebugScrollOffset();
			ApplyDebugScrollOffset();
		}

		private void RefreshModeButtonText()
		{
			if (TryGetTargetElevator(out TEElevator te))
				_modeButton.SetText($"楼层检测：{TEElevator.GetFloorDetectModeLabel(te.FloorMode)}");
			else
				_modeButton.SetText("楼层检测：—（未找到电梯）");
		}

		private static UITextPanel<string> CreateFloorButton(string label, bool selected, bool isCurrent)
		{
			var btn = new UITextPanel<string>(label);
			btn.Width.Set(0f, 1f);
			btn.Height.Set(36f, 0f);
			btn.PaddingLeft = 6f;
			btn.PaddingRight = 6f;
			btn.PaddingTop = 4f;
			btn.PaddingBottom = 4f;
			if (selected)
			{
				btn.BackgroundColor = new Color(85, 120, 185);
				btn.BorderColor = new Color(140, 180, 255);
			}
			else if (isCurrent)
			{
				btn.BackgroundColor = new Color(72, 95, 140) * 0.85f;
				btn.BorderColor = new Color(200, 180, 90);
			}
			else
			{
				btn.BackgroundColor = new Color(63, 82, 151) * 0.6f;
				btn.BorderColor = new Color(63, 82, 151) * 0.9f;
			}
			return btn;
		}

		private static string BuildElevatorSettingsReport(TEElevator te)
		{
			if (te == null)
				return "未找到电梯实体（TE）";

			te.ScanFloors();
			int currentBottomY = te.GetStandingSurfaceBottomY();
			int currentFloorUi = -1;
			for (int i = 0; i < te.FloorBottomYs.Count; i++)
			{
				if (Math.Abs(te.FloorBottomYs[i] - currentBottomY) <= 1)
				{
					currentFloorUi = i + 1;
					break;
				}
			}

			var lines = new List<string>
			{
				"【井道】",
				te.DebugLastLeftWallX >= 0 && te.DebugLastRightWallX >= 0
					? $"  左壁 X = {te.DebugLastLeftWallX}    右壁 X = {te.DebugLastRightWallX}"
					: $"  未识别井壁  （{te.DebugWallFailReason}）",
				"",
				"【楼层】",
				$"  检测模式：{TEElevator.GetFloorDetectModeLabel(te.FloorMode)}",
				$"  识别数量：{te.FloorBottomYs.Count} 层",
				currentFloorUi > 0
					? $"  轿厢所在：楼层 {currentFloorUi}（站立面 Y={currentBottomY}）"
					: $"  轿厢所在：未匹配列表（站立面 Y={currentBottomY}）",
				"",
				"【轿厢】",
				$"  左上角坐标：({te.Position.X}, {te.Position.Y})",
				te.IsMoving
					? $"  运行状态：移动中（方向 {(te.MoveDir > 0 ? "下" : "上")}）"
					: "  运行状态：静止",
			};

			if (te.DebugLastLeftWallX < 0 || te.DebugLastRightWallX < 0)
			{
				lines.Add("");
				lines.Add("【扫描候选】");
				lines.Add($"  左：X={te.DebugBestLeftCandidateX}  墙分={te.DebugBestLeftWallScore}  空气={te.DebugBestLeftInsideAirScore}");
				lines.Add($"  右：X={te.DebugBestRightCandidateX}  墙分={te.DebugBestRightWallScore}  空气={te.DebugBestRightInsideAirScore}");
			}
			else if (te.DebugDoorCandidateCount > 0)
			{
				lines.Add("");
				lines.Add("【门洞候选】");
				lines.Add($"  数量：{te.DebugDoorCandidateCount}");
				if (!string.IsNullOrWhiteSpace(te.DebugDoorSample))
					lines.Add($"  样例：{Ellipsize(te.DebugDoorSample.Trim(), 48)}");
			}

			return string.Join("\n", lines);
		}

		private static string Ellipsize(string text, int maxChars)
		{
			if (string.IsNullOrEmpty(text) || text.Length <= maxChars)
				return text;
			if (maxChars <= 1)
				return "…";
			return text.Substring(0, maxChars - 1) + "…";
		}

		private void RefreshDebugText()
		{
			TryGetTargetElevator(out TEElevator te);
			_debugRawText = BuildElevatorSettingsReport(te);
			_debugText.SetText(_debugRawText);

			int lineCount = Math.Max(1, _debugRawText.Split('\n').Length);
			_debugContentHeightPx = lineCount * DebugLineHeightPx;
			ClampDebugScrollOffset();
			ApplyDebugScrollOffset();
		}

		private void ClampDebugScrollOffset()
		{
			float viewportHeight = _debugPanel.Height.Pixels - 12f; // padding 上下各 6
			float maxOffset = Math.Max(0f, _debugContentHeightPx - viewportHeight);
			if (_debugScrollOffsetPx < 0f)
				_debugScrollOffsetPx = 0f;
			if (_debugScrollOffsetPx > maxOffset)
				_debugScrollOffsetPx = maxOffset;
		}

		private void ApplyDebugScrollOffset()
		{
			_debugText.Top.Set(-_debugScrollOffsetPx, 0f);
		}

		private void RebuildFloorButtons(bool autoSelectNearest = false)
		{
			_floorList?.Clear();
			_floorBottomYs.Clear();

			if (!TryGetTargetElevator(out TEElevator te))
			{
				_floorList.Add(CreateFloorButton("未找到电梯", selected: false, isCurrent: false));
				_floorPanel?.Recalculate();
				return;
			}
			te.ScanFloors();
			if (te.FloorBottomYs.Count == 0)
			{
				_floorList.Add(CreateFloorButton("无楼层", selected: false, isCurrent: false));
				_floorPanel?.Recalculate();
				return;
			}

			// 楼层 1 = 井道最上方（Y 较小），列表自上而下排列。
			_floorBottomYs.AddRange(te.FloorBottomYs);

			int currentBottomY = te.GetStandingSurfaceBottomY();
			if (_selectedFloorIndex < 0 || _selectedFloorIndex >= _floorBottomYs.Count)
				_selectedFloorIndex = 0;

			if (autoSelectNearest && Main.LocalPlayer != null && Main.LocalPlayer.active)
			{
				int nearestBottomY = te.FindNearestFloorBottomY(Main.LocalPlayer.Bottom.Y);
				for (int i = 0; i < _floorBottomYs.Count; i++)
				{
					if (_floorBottomYs[i] == nearestBottomY)
					{
						_selectedFloorIndex = i;
						break;
					}
				}
			}

			for (int i = 0; i < _floorBottomYs.Count; i++)
			{
				int bottomY = _floorBottomYs[i];
				int floorIndex = i;
				bool selected = floorIndex == _selectedFloorIndex;
				bool isCurrent = Math.Abs(bottomY - currentBottomY) <= 1;

				string label = $"楼层 {floorIndex + 1}";
				UITextPanel<string> btn = CreateFloorButton(label, selected, isCurrent);
				btn.OnLeftClick += (_, __) =>
				{
					_selectedFloorIndex = floorIndex;
					ConfirmSelectedFloor();
					RebuildFloorButtons();
				};
				_floorList.Add(btn);
			}

			_floorPanel?.Recalculate();
		}

		private bool TryGetTargetElevator(out TEElevator te)
		{
			te = null;
			if (_targetTeId >= 0 && TileEntity.ByID.TryGetValue(_targetTeId, out TileEntity byIdEntity) && byIdEntity is TEElevator byIdTe)
			{
				te = byIdTe;
				_targetX = te.Position.X;
				_targetY = te.Position.Y;
				return true;
			}

			int id = ModContent.GetInstance<TEElevator>().Find(_targetX, _targetY);
			if (id >= 0 && TileEntity.ByID.TryGetValue(id, out TileEntity entity) && entity is TEElevator direct)
			{
				te = direct;
				_targetTeId = te.ID;
				_targetX = te.Position.X;
				_targetY = te.Position.Y;
				return true;
			}

			// 兜底：移动中的电梯/成帧更新时，TE 坐标可能与 UI 缓存 topLeft 短暂不一致。
			float worldX = (_targetX + 2f) * 16f;
			float worldY = (_targetY + 3.5f) * 16f;
			if (TEElevator.TryFindNearbyElevatorByWorld(worldX, worldY, maxDistanceTiles: 12, out TEElevator nearTe, out _, out _))
			{
				te = nearTe;
				_targetTeId = te.ID;
				_targetX = te.Position.X;
				_targetY = te.Position.Y;
				return true;
			}

			return false;
		}
	}
}
