using ArknightsMod.Common.Players;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;

namespace ArknightsMod.Common.UI
{
	public class SummonSkillSlot : UIImage
	{
		public bool usable;
		public SummonSkillSlot(Texture2D icon) : base(icon) {
			OnLeftClick += SummonMode;
		}
		protected override void DrawSelf(SpriteBatch spriteBatch) {
			if (usable) {
				base.DrawSelf(spriteBatch);
				if (IsMouseHovering)
					Main.LocalPlayer.mouseInterface = true;
			}
		}
		public void SummonMode(UIMouseEvent evt, UIElement listeningElement) {
			if (!usable)
				return;
			var mp = Main.LocalPlayer.GetModPlayer<WeaponPlayer>();
			if (mp.CurrentSkill.SummonSkill) {
				mp.SummonMode = true;
				SoundEngine.PlaySound(SoundID.MenuTick);
			}
		}
	}
}
