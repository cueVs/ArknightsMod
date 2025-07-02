using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArknightsMod.Common.UI.BattleRecord.UIElements
{
	internal class UILevelSelector : UIImage
	{
		private const int LEVEL_TEXT_COUNT = 5;
		private const float ELEMENT_HEIGHT = 32f;

		private UIText[] _levelTexts = new UIText[LEVEL_TEXT_COUNT];
		private Vector2 _mousePosCache = Vector2.Zero;
		private bool _mouseHandle = false;
		private List<string> _levelNames = new List<string>();
		private int _levelIndex = 0;
		private int _willChangeIndex = 0;
		private float _topOffsetPixel = 0f;
		private float _wheelCache = 0f;
		private float _topOffsetY = 0f;
		private float _waitToTopOffsetY = 0f;
		private float intervalPixel;
		private int _minLevelIndex = 0;
		private int _maxLevelIndex = 0;

		/// <summary>
		/// 目前的等级索引
		/// </summary>
		public int Index => _levelIndex;

		/// <summary>
		/// 目前在UI中心的等级索引
		/// </summary>
		public int CursorIndex => _levelIndex + _willChangeIndex;

		/// <summary>
		/// 目标等级索引
		/// </summary>
		public int TargetIndex {
			get {
				float waitToTopOffsetY = 0f;
				if (_wheelCache != PlayerInput.MouseInfo.ScrollWheelValue)
					waitToTopOffsetY += GetMoveContentOffsetY(_levelIndex + (int)(PlayerInput.MouseInfo.ScrollWheelValue - _wheelCache) / 120);
				if (_mouseHandle) {
					waitToTopOffsetY += Main.MouseScreen.Y - _mousePosCache.Y;
				}
				waitToTopOffsetY += _waitToTopOffsetY;
				return _levelIndex + (int)(waitToTopOffsetY / (ELEMENT_HEIGHT + intervalPixel));
			}
		}

		public int Level => _levelNames.Count - Index;
		public int CursorLevel => _levelNames.Count - CursorIndex - 2;
		public int TargetLevel => _levelNames.Count - TargetIndex - 2;

		public int MinLevel {
			get => _minLevelIndex - 1;
			set {
				_minLevelIndex = value - 1;
				if (CursorIndex > _minLevelIndex) {
					_levelIndex = _minLevelIndex;
				}
			}
		}

		public int MaxLevel {
			get => _levelNames.Count - _maxLevelIndex - 2;
			set {
				_maxLevelIndex = _levelNames.Count - value - 2;
				if (CursorIndex < _maxLevelIndex) {
					_levelIndex = _maxLevelIndex;
				}
			}
		}

		public bool InvokeOnCursorLevelChange = true;

		public event Action<UILevelSelector> OnTargetLevelChange;

		public UILevelSelector() : base(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/BattleRecord/Images/LevelBG", AssetRequestMode.ImmediateLoad)) {
			OverflowHidden = true;

			int intervalCount = LEVEL_TEXT_COUNT + 1;
			float selectorHeight = Height.Pixels;
			float height = 0f;
			for (int i = 0; i < LEVEL_TEXT_COUNT; i++) {
				_levelTexts[i] = new UIText(string.Empty, 1f, true);
				Append(_levelTexts[i]);
			}
			intervalPixel = new StyleDimension(-height / intervalCount + 22f, 1f / intervalCount).GetValue(selectorHeight);
			SetToDefaultStatus(true);
		}

		public void SetLevel(int maxLevel, int minLevel, int nowLevel = 1, bool dotNeedAni = false) {
			minLevel = (int)MathHelper.Clamp(minLevel, 1, maxLevel);
			if (nowLevel < minLevel)
				nowLevel = minLevel;

			_levelNames.Clear();
			for (int i = maxLevel; i > 0; i--) {
				_levelNames.Add(i.ToString());
			}

			MinLevel = minLevel;
			MoveCursorToLevel(nowLevel, dotNeedAni);
		}

		public void SetToDefaultStatus(bool dotNeedAni = false) {
			_levelNames.Clear();
			_levelNames.Add("-");
			MoveCursorToLevel(1, dotNeedAni);
		}

		public override void LeftMouseDown(UIMouseEvent evt) {
			base.LeftMouseDown(evt);
			_mouseHandle = true;
			_mousePosCache = evt.MousePosition;
		}

		public override void LeftMouseUp(UIMouseEvent evt) {
			base.LeftMouseUp(evt);
			_mouseHandle = false;
		}

		private int _cursorIndexCache;

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);

			var index = CursorIndex;
			if (_cursorIndexCache != index) {
				if (InvokeOnCursorLevelChange)
					OnTargetLevelChange?.Invoke(this);
				_cursorIndexCache = index;
			}
			MouseState state = PlayerInput.MouseInfo;

			float selectorHeight = GetDimensions().Height;
			float height = 0f;
			for (int i = 0; i < LEVEL_TEXT_COUNT; i++) {
				height += _levelTexts[i].MinHeight.Pixels;
			}

			int intervalCount = LEVEL_TEXT_COUNT + 1;

			intervalPixel = new StyleDimension(-height / intervalCount + 22f, 1f / intervalCount).GetValue(selectorHeight);
			StyleDimension top = new StyleDimension(-(height + intervalPixel * (LEVEL_TEXT_COUNT - 1)) / 2f, 0.5f);

			float waitToTopOffsetY = 0f;

			if (!IsMouseHovering)
				_wheelCache = state.ScrollWheelValue;

			if (_wheelCache != state.ScrollWheelValue) {
				waitToTopOffsetY += GetMoveContentOffsetY(_levelIndex + (int)(state.ScrollWheelValue - _wheelCache) / 120);
			}

			if (_mouseHandle) {
				waitToTopOffsetY += Main.MouseScreen.Y - _mousePosCache.Y;
			}

			waitToTopOffsetY += _waitToTopOffsetY;

			if (Math.Abs(waitToTopOffsetY - _topOffsetY) < 0.01f) {
				_waitToTopOffsetY = 0f;
				InvokeOnCursorLevelChange = true;
			}

			if (waitToTopOffsetY != 0f) {
				_topOffsetY += (waitToTopOffsetY - _topOffsetY) / 4f;
				var data = MoveContent(_topOffsetY, height, selectorHeight);
				_willChangeIndex = data.willChangeIndex;
				_topOffsetPixel = data.topOffsetPixel;
				return;
			}

			InvokeOnCursorLevelChange = true;
			_topOffsetY = 0f;
			_levelIndex += _willChangeIndex;
			_willChangeIndex = 0;

			_topOffsetPixel -= _topOffsetPixel / 6f;
			for (int i = 0; i < LEVEL_TEXT_COUNT; i++) {
				int ind = _levelIndex + i;
				if (ind >= 0 && ind < _levelNames.Count)
					_levelTexts[i].SetText(_levelNames[ind]);
				else
					_levelTexts[i].SetText(string.Empty);
				_levelTexts[i].Top = top;
				_levelTexts[i].Top.Pixels += _topOffsetPixel;
				_levelTexts[i].Left.Set(-_levelTexts[i].MinWidth.Pixels / 2f, 0.5f);
				float elementCenterY = _levelTexts[i].Top.GetValue(selectorHeight) + _levelTexts[i].MinHeight.Pixels / 2f;
				if (elementCenterY < 0f || elementCenterY > selectorHeight) {
					_levelTexts[i].TextColor = Color.Gray;
				}
				else {
					_levelTexts[i].TextColor = Color.Lerp(Color.Gray, Color.White, (float)Math.Pow((0.5f - Math.Abs(0.5f - elementCenterY / selectorHeight)) * 2f, 2));
				}
				top.Pixels += _levelTexts[i].MinHeight.Pixels + intervalPixel;
			}
			Recalculate();
		}

		public void MoveCursorToLevel(int level, bool dontNeedAni = false) {
			MoveToIndex(_levelNames.Count - level - 2, dontNeedAni);
		}

		public void MoveToIndex(int index, bool dontNeedAni = false) {
			if (dontNeedAni)
				_levelIndex = index;
			else
				_waitToTopOffsetY = -GetMoveContentOffsetY(index);
		}

		private float GetMoveContentOffsetY(int targetIndex) {
			int indexOffset = targetIndex - _levelIndex;
			return indexOffset * (ELEMENT_HEIGHT + intervalPixel);
		}

		private (int willChangeIndex, float topOffsetPixel) MoveContent(float offsetY, float allElementsHeight, float selectorHeight) {
			if (offsetY == 0f)
				return (0, 0f);

			StyleDimension top = new StyleDimension(-(allElementsHeight + intervalPixel * (LEVEL_TEXT_COUNT - 1)) / 2f, 0.5f);

			float topPixels = top.GetValue(selectorHeight);
			float itemHeight = intervalPixel + ELEMENT_HEIGHT;

			int willChangeIndex = 0;
			float topOffsetPixel = 0f;
			if (offsetY > 0) {
				if (offsetY >= -topPixels) {
					willChangeIndex = -1;
					topOffsetPixel -= topPixels;
					offsetY += topPixels;
				}
			}
			else {
				if (-offsetY >= topPixels + ELEMENT_HEIGHT) {
					willChangeIndex = 1;
					topOffsetPixel -= topPixels + ELEMENT_HEIGHT;
					offsetY += topPixels + ELEMENT_HEIGHT;
				}
			}
			willChangeIndex += (int)(-offsetY / itemHeight);

			int index = _levelIndex + willChangeIndex;
			topOffsetPixel += offsetY + willChangeIndex * itemHeight;
			if (index <= _maxLevelIndex) {
				willChangeIndex = _maxLevelIndex - _levelIndex;
				topOffsetPixel = 0f;
				topOffsetPixel -= 1f;
			}
			else if (index >= _levelNames.Count - (LEVEL_TEXT_COUNT + 1) / 2 - _minLevelIndex) {
				willChangeIndex = _levelNames.Count - (LEVEL_TEXT_COUNT + 1) / 2 - _levelIndex - _minLevelIndex;
				topOffsetPixel = 0f;
				topOffsetPixel -= 1f;
			}

			if (topOffsetPixel < 0f)
				topOffsetPixel = -(-topOffsetPixel % itemHeight);
			else if (topOffsetPixel > 0f)
				topOffsetPixel %= itemHeight;

			for (int i = 0; i < LEVEL_TEXT_COUNT; i++) {
				index = _levelIndex + i + willChangeIndex;
				if (index >= 0 && index < _levelNames.Count)
					_levelTexts[i].SetText(_levelNames[index]);
				else
					_levelTexts[i].SetText(string.Empty);
				_levelTexts[i].Top = top;
				_levelTexts[i].Top.Pixels += topOffsetPixel;
				_levelTexts[i].Left.Set(-_levelTexts[i].MinWidth.Pixels / 2f, 0.5f);
				float elementCenterY = _levelTexts[i].Top.GetValue(selectorHeight) + _levelTexts[i].MinHeight.Pixels / 2f;
				if (elementCenterY < 0f || elementCenterY > selectorHeight) {
					_levelTexts[i].TextColor = Color.Gray;
				}
				else {
					_levelTexts[i].TextColor = Color.Lerp(Color.Gray, Color.White, (float)Math.Pow((0.5f - Math.Abs(0.5f - elementCenterY / selectorHeight)) * 2f, 2));
				}
				top.Pixels += _levelTexts[i].MinHeight.Pixels + intervalPixel;
			}
			return (willChangeIndex, topOffsetPixel);
		}
	}
}