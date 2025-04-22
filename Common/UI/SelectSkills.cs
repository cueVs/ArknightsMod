using ArknightsMod.Common.Items;
using ArknightsMod.Common.Players;
using ArknightsMod.Content.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArknightsMod.Common.UI
{
	internal class SelectSkills : UIState
	{
		private static SelectSkills ins;

		// For this bar we'll be using a frame texture and then a gradient inside bar, as it's one of the more simpler approaches while still looking decent.
		// Once this is all set up make sure to go and do the required stuff for most UI's in the ModSystem class.
		private readonly static SoundStyle click = new("ArknightsMod/Sounds/UIButton");
		private SkillSlotUI s1, s2, s3;
		private SummonSkillSlot summon;
		private UIText skillLevel;
		internal SelectSkills() => ins = this;

		public override void OnInitialize() {
			UIImage area = new(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/SkillBase", AssetRequestMode.ImmediateLoad));
			area.Left.Set(20, 0f);
			area.Top.Set(136, 0f);
			area.Width.Set(300, 0f);
			area.Height.Set(100, 0f);
			Append(area);

			skillLevel = new UIText("0", 1f);
			skillLevel.Left.Set(262, 0f);
			skillLevel.Top.Set(8, 0f);
			skillLevel.Width.Set(20, 0f);
			skillLevel.Height.Set(20, 0f);
			area.Append(skillLevel);

			s1 = new();
			s1.Left.Set(2, 0f);
			s1.Top.Set(-8, 0f);
			s1.Width.Set(64, 0f);
			s1.Height.Set(64, 0f);
			s1.OnLeftClick += (_, _) => ChangeSkill(0);
			area.Append(s1);

			s2 = new();
			s2.Left.Set(72, 0f);
			s2.Top.Set(-8, 0f);
			s2.Width.Set(64, 0f);
			s2.Height.Set(64, 0f);
			s2.OnLeftClick += (_, _) => ChangeSkill(1);
			area.Append(s2);

			s3 = new();
			s3.Left.Set(142, 0f);
			s3.Top.Set(-8, 0f);
			s3.Width.Set(64, 0f);
			s3.Height.Set(64, 0f);
			s3.OnLeftClick += (_, _) => ChangeSkill(2);
			area.Append(s3);

		}

		private static void ChangeSkill(int index, bool force = false) {
			Player p = Main.LocalPlayer;
			if (p.HeldItem.ModItem is not ArknightsWeapon)
				return;
			var mp = p.GetModPlayer<WeaponPlayer>();
			if (!force && (mp.SkillCount <= index || mp.Skill == index))
				return;
			mp.Skill = index;
			SkillData data = mp.CurrentSkill;
			ins.skillLevel.SetText((data.ForceReplaceLevel ?? data.Level).ToString());
			//SoundEngine.PlaySound(click);
			SummonSkillSlot summon = ins.summon;
			if (!data.SummonSkill) {
				if (summon == null)
					return;
				summon.usable = false;
				return;
			}
			ins.ActiveSummonUI(data.SummonIcon.Value);
		}
		public static void ChangeSkillSlot(ArknightsWeapon ark) {
			ins.s1.SetSkill(ark.GetSkillData(0));
			ins.s2.SetSkill(ark.GetSkillData(1));
			ins.s3.SetSkill(ark.GetSkillData(2));
			ChangeSkill(0, true);
		}
		private void ActiveSummonUI(Texture2D icon) {
			if (summon == null) {
				summon = new(icon);
				summon.Left.Set(330, 0f);
				summon.Top.Set(150, 0f);
				Append(summon);
				Recalculate();
			}
			else
				summon.SetImage(icon);
			summon.usable = true;
		}
	}

	class SelectSkillsSystem : ModSystem
	{
		private UserInterface SelectSkillsUserInterface;

		internal SelectSkills SelectSkillsUI;
		private bool Visiable;

		public void ShowMyUI() {
			if (Visiable)
				return;
			Visiable = true;
			SelectSkillsUserInterface?.SetState(SelectSkillsUI);
		}
		public void HideMyUI() {
			if (!Visiable)
				return;
			Visiable = false;
			SelectSkillsUserInterface?.SetState(null);
		}

		public override void Load() {
			// All code below runs only if we're not loading on a server
			if (!Main.dedServ) {
				SelectSkillsUI = new();
				SelectSkillsUserInterface = new();
				SelectSkillsUserInterface.SetState(SelectSkillsUI);
				Visiable = true;
			}
		}

		public override void UpdateUI(GameTime gameTime) {
			SelectSkillsUserInterface?.Update(gameTime);
			if (Main.playerInventory) {
				HideMyUI();
			}
			else {
				if (Main.LocalPlayer.HeldItem.ModItem is ArknightsWeapon) {
					ShowMyUI();
				}
				else {
					HideMyUI();
				}
			}
		}

		public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers) {
			int resourceBarIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Resource Bars")) + 1;
			if (resourceBarIndex != -1) {
				layers.Insert(resourceBarIndex, new LegacyGameInterfaceLayer(
					"ArknightsMod: Skill Select",
					delegate {
						SelectSkillsUserInterface.Draw(Main.spriteBatch, new GameTime());
						return true;
					},
					InterfaceScaleType.UI)
				);
			}
		}
	}
}
