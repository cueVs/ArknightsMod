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
	internal class UILevelSelectorRefactor : UIImage
	{
		private const int LEVEL_TEXT_COUNT = 5;

		private readonly UIText[] _levelTexts = new UIText[LEVEL_TEXT_COUNT];
		private readonly List<string> _levelNames = new List<string>();

		public event Action<UILevelSelectorRefactor> OnRound;

		private Vector2 _mousePosCache = Vector2.Zero;
		private bool _mouseHandle = false;

		private float _elementHeight = 0f;
		private float _intervalPixel;
		private float _offset = 0f;
		private float _waitToOffset = 0f;
		private float _wheelCache = 0f;

		private int _currentIndex;
		private int _waitToIndex;

		public int MinLevel;
		public int MaxLevel;

		public int CurrentLevel => _levelNames.Count - _currentIndex;

		public int Level {
			get => _levelNames.Count - _waitToIndex;
			set {
				_waitToOffset = (_levelNames.Count - value + 1) * (_elementHeight + _intervalPixel) - _intervalPixel;
				_waitToIndex = (int)(_waitToOffset / (_elementHeight + _intervalPixel));
			}
		}

		public UILevelSelectorRefactor() : base(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/BattleRecord/Images/LevelBG", AssetRequestMode.ImmediateLoad)) {
			OverflowHidden = true;
			for (int i = 0; i < LEVEL_TEXT_COUNT; i++) {
				_levelTexts[i] = new UIText(string.Empty, 1f, true);
				Append(_levelTexts[i]);
			}

			SetToDefaultStatus();
		}

		public override void LeftMouseDown(UIMouseEvent evt) {
			base.LeftMouseDown(evt);
			_mouseHandle = true;
			_mousePosCache = evt.MousePosition;
		}

		public override void LeftMouseUp(UIMouseEvent evt) {
			base.LeftMouseUp(evt);
			_mouseHandle = false;
			if (OnRound == null)
				Level = Level;
			else
				OnRound(this);
		}

		public override void Update(GameTime gameTime) {
			base.Update(gameTime);
			MouseState state = PlayerInput.MouseInfo;
			if (_wheelCache != state.ScrollWheelValue) {
				if (IsMouseHovering) {
					if (_wheelCache < state.ScrollWheelValue) {
						Level++;
						OnRound?.Invoke(this);
					}
					else {
						Level--;
						OnRound?.Invoke(this);
					}
				}
				_wheelCache = state.ScrollWheelValue;
			}

			if (_mouseHandle && _mousePosCache != Main.MouseScreen) {
				_waitToOffset -= Main.MouseScreen.Y - _mousePosCache.Y;
				_mousePosCache = Main.MouseScreen;
			}

			float minOffset = _elementHeight * (_levelNames.Count - MaxLevel + 1) + _intervalPixel * (_levelNames.Count - MaxLevel);
			float maxOffset = _elementHeight * (_levelNames.Count - MinLevel + 1) + _intervalPixel * (_levelNames.Count - MinLevel);
			if (_waitToOffset < minOffset)
				_waitToOffset = minOffset;
			else if (_waitToOffset > maxOffset)
				_waitToOffset = maxOffset;

			if (_waitToOffset != _offset) {
				_offset += (_waitToOffset - _offset) / 6f;
				Recalculate();
			}
		}

		public override void Recalculate() {
			base.Recalculate();
			for (int i = 0; i < LEVEL_TEXT_COUNT; i++) {
				_elementHeight = Math.Max(_elementHeight, _levelTexts[i].GetDimensions().Height);
			}
			float selectorHeight = GetDimensions().Height;
			_intervalPixel = selectorHeight * 0.14f;
			_waitToIndex = (int)(_waitToOffset / (_elementHeight + _intervalPixel));

			float offset = _offset;
			int index = (int)(offset / (_elementHeight + _intervalPixel));
			offset -= index * (_elementHeight + _intervalPixel);
			_currentIndex = index;
			index -= (LEVEL_TEXT_COUNT - 1) / 2;
			offset *= -1;

			for (int i = 0; i < LEVEL_TEXT_COUNT; i++) {
				int showIndex = index + i;
				if (showIndex >= 0 && showIndex < _levelNames.Count)
					_levelTexts[i].SetText(_levelNames[showIndex]);
				else
					_levelTexts[i].SetText(string.Empty);
				_levelTexts[i].Top.Set(offset, 0f);
				_levelTexts[i].Left.Set(-_levelTexts[i].MinWidth.Pixels / 2f, 0.5f);
				float elementCenterY = _levelTexts[i].Top.GetValue(selectorHeight) + _levelTexts[i].MinHeight.Pixels / 2f;
				if (elementCenterY < 0f || elementCenterY > selectorHeight) {
					_levelTexts[i].TextColor = Color.Gray;
				}
				else {
					_levelTexts[i].TextColor = Color.Lerp(Color.Gray, Color.White, (float)Math.Pow((0.5f - Math.Abs(0.5f - elementCenterY / selectorHeight)) * 2f, 2));
				}
				offset += _levelTexts[i].MinHeight.Pixels + _intervalPixel;
			}
		}

		public void SetLevel(int maxLevel, int minLevel, int nowLevel = 1) {
			minLevel = (int)MathHelper.Clamp(minLevel, 1, maxLevel);
			if (nowLevel < minLevel)
				nowLevel = minLevel;

			_levelNames.Clear();
			for (int i = maxLevel; i > 0; i--) {
				_levelNames.Add(i.ToString());
			}

			MaxLevel = maxLevel;
			MinLevel = minLevel;
			Level = nowLevel;
		}

		public void SetToDefaultStatus() {
			_levelNames.Clear();
			_levelNames.Add("-");
			MinLevel = MaxLevel = Level = 1;
		}
	}
}