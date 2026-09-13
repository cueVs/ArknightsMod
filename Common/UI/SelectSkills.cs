using ArknightsMod.Content.Items.Weapons;
using ArknightsMod.Players;
using ArknightsMod.Systems.Gameplay.Skill;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArknightsMod.Common.UI
{
	internal class SelectSkills : UIState
	{
		private static SelectSkills _ins;

		private SkillSlotUI s1, s2, s3;
		private SummonSkillSlot summon;
		private UIText skillLevel;
		private UIElement expandedPanel;
		private UIElement collapsedPanel;
		private UIImage areaImage;
		private UIElement closeTabRef;
		private bool _isOpen = true;

		// 与 vanilla buff 栏布局匹配：每行 11 个，行间距 50，第一行起点 y=76，图标 32×32
		private const int BaseTop = 136;
		private int _currentTop = BaseTop;

		internal SelectSkills() => _ins = this;

		public override void OnInitialize()
		{
			expandedPanel = new UIElement();
			expandedPanel.Width.Set(0, 1f);
			expandedPanel.Height.Set(0, 1f);

			areaImage = new UIImage(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillBase", AssetRequestMode.ImmediateLoad));
			areaImage.Left.Set(20, 0f);
			areaImage.Top.Set(BaseTop, 0f);
			areaImage.Width.Set(300, 0f);
			areaImage.Height.Set(100, 0f);
			expandedPanel.Append(areaImage);
			UIImage area = areaImage;

			skillLevel = new UIText("0", 1f);
			skillLevel.Left.Set(262, 0f);
			skillLevel.Top.Set(8, 0f);
			skillLevel.Width.Set(20, 0f);
			skillLevel.Height.Set(20, 0f);
			area.Append(skillLevel);

			s1 = new SkillSlotUI();
			s1.Left.Set(2, 0f);   s1.Top.Set(-8, 0f);
			s1.Width.Set(64, 0f); s1.Height.Set(64, 0f);
			s1.OnLeftClick += (_, _) => ChangeSkill(0);
			area.Append(s1);

			s2 = new SkillSlotUI();
			s2.Left.Set(72, 0f);  s2.Top.Set(-8, 0f);
			s2.Width.Set(64, 0f); s2.Height.Set(64, 0f);
			s2.OnLeftClick += (_, _) => ChangeSkill(1);
			area.Append(s2);

			s3 = new SkillSlotUI();
			s3.Left.Set(142, 0f); s3.Top.Set(-8, 0f);
			s3.Width.Set(64, 0f); s3.Height.Set(64, 0f);
			s3.OnLeftClick += (_, _) => ChangeSkill(2);
			area.Append(s3);

			closeTabRef = MakeTab("ArknightsMod/Common/UI/CloseBar_1", "ArknightsMod/Common/UI/CloseBar_2");
			closeTabRef.Left.Set(300, 0f);
			closeTabRef.Top.Set(BaseTop, 0f);
			closeTabRef.OnLeftClick += (_, _) => SetOpen(false);
			expandedPanel.Append(closeTabRef);

			collapsedPanel = MakeTab("ArknightsMod/Common/UI/CloseBar_1", "ArknightsMod/Common/UI/CloseBar_3");
			collapsedPanel.Left.Set(20, 0f);
			collapsedPanel.Top.Set(BaseTop, 0f);
			collapsedPanel.OnLeftClick += (_, _) => SetOpen(true);

			Append(expandedPanel);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);
			int needed = ComputeBarTop();
			if (needed == _currentTop) return;
			_currentTop = needed;
			areaImage.Top.Set(needed, 0f);
			closeTabRef.Top.Set(needed, 0f);
			collapsedPanel.Top.Set(needed, 0f);
			if (summon != null) summon.Top.Set(needed + 14, 0f); // 原来 150 - 136 = 14 的偏移
			Recalculate();
		}

		// 根据玩家当前激活 buff 数量算出技能条应当下移到的 y。
		// 11 个以内（一行内）维持 BaseTop；多于 11 时移到 buff 最后一行下方留 4px 间距。
		private static int ComputeBarTop()
		{
			Player p = Main.LocalPlayer;
			if (p == null) return 80;
			int buffCount = 0;
			for (int i = 0; i < Player.MaxBuffs; i++)
				if (p.buffType[i] > 0) buffCount++;
			int rows = (buffCount + 10) / 11;
			return  76 + rows * 50 + 4;
		}

		// 把两张叠放图包进一个容器，点击事件由容器统一处理。
		private static UIElement MakeTab(string bgPath, string fgPath)
		{
			UIElement tab = new HoverBlockingElement();
			tab.Width.Set(20, 0f);
			tab.Height.Set(100, 0f);

			UIImage bg = new(ModContent.Request<Texture2D>(bgPath, AssetRequestMode.ImmediateLoad));
			bg.Width.Set(20, 0f); bg.Height.Set(100, 0f);
			tab.Append(bg);

			UIImage fg = new(ModContent.Request<Texture2D>(fgPath, AssetRequestMode.ImmediateLoad));
			fg.Width.Set(20, 0f); fg.Height.Set(100, 0f);
			tab.Append(fg);

			return tab;
		}

		private class HoverBlockingElement : UIElement
		{
			public override void Update(GameTime gameTime)
			{
				base.Update(gameTime);
				if (IsMouseHovering)
					Main.LocalPlayer.mouseInterface = true;
			}
		}

		private void SetOpen(bool open)
		{
			if (_isOpen == open) return;
			_isOpen = open;
			if (open)
			{
				Append(expandedPanel);
				collapsedPanel.Remove();
			}
			else
			{
				expandedPanel.Remove();
				Append(collapsedPanel);
			}
			Recalculate();
		}

		private static void ChangeSkill(int index, bool force = false)
		{
			Player p = Main.LocalPlayer;
			if (p.HeldItem.ModItem is not UpgradeWeaponBase ark) return;
			var mp = p.GetModPlayer<WeaponPlayer>();
			if (!mp.TrySelectSkill(ark, index, force)) return;
			SkillData data = mp.CurrentSkill;
			if (data == null) return;

			_ins.skillLevel.SetText((data.ForceReplaceLevel ?? data.Level).ToString());

			if (!data.SummonSkill)
			{
				if (_ins.summon != null) _ins.summon.usable = false;
				return;
			}
			_ins.ActiveSummonUI(data.SummonIcon.Value);
		}

		public static void ChangeSkillSlot(UpgradeWeaponBase ark)
		{
			_ins.s1.SetSkill(ark.GetSkillData(0));
			_ins.s2.SetSkill(ark.GetSkillData(1));
			_ins.s3.SetSkill(ark.GetSkillData(2));
			ChangeSkill(Main.LocalPlayer.GetModPlayer<WeaponPlayer>().Skill, true);
		}

		private void ActiveSummonUI(Texture2D icon)
		{
			if (summon == null)
			{
				summon = new SummonSkillSlot(icon);
				summon.Left.Set(330, 0f);
				summon.Top.Set(150, 0f);
				expandedPanel.Append(summon);
				Recalculate();
			}
			else
			{
				summon.SetImage(icon);
			}
			summon.usable = true;
		}
	}

	internal class SelectSkillsSystem : ModSystem
	{
		private UserInterface _ui;
		internal SelectSkills SelectSkillsUI;

		public override void Load()
		{
			if (Main.dedServ) return;
			SelectSkillsUI = new SelectSkills();
			_ui = new UserInterface();
			_ui.SetState(SelectSkillsUI);
		}

		public override void UpdateUI(GameTime gameTime)
		{
			_ui?.Update(gameTime);
			bool shouldShow = !Main.playerInventory && Main.LocalPlayer.HeldItem.ModItem is UpgradeWeaponBase;
			bool isShown = _ui?.CurrentState != null;
			if (shouldShow && !isShown)
				_ui?.SetState(SelectSkillsUI);
			else if (!shouldShow && isShown)
				_ui?.SetState(null);
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
		{
			int idx = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars")) + 1;
			if (idx < 1) return;
			layers.Insert(idx, new LegacyGameInterfaceLayer(
				"ArknightsMod: Skill Select",
				delegate {
					_ui?.Draw(Main.spriteBatch, new GameTime());
					return true;
				},
				InterfaceScaleType.UI));
		}
	}
}
