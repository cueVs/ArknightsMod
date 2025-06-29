using ArknightsMod.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ArknightsMod.Common.UI.BattleRecord.UIElements
{
	public class UIUpgradeItemSlot : UIImage
	{
		public UpgradeItemBase UpgradeItem;
		public event Action<UIUpgradeItemSlot> OnUpgradeItemChange;
		public UIUpgradeItemSlot() : base(ModContent.Request<Texture2D>("ArknightsMod/Common/UI/BattleRecord/Images/WeaponSlot", AssetRequestMode.ImmediateLoad)) {
		}

		public override void LeftClick(UIMouseEvent evt) {
			base.LeftClick(evt);
			if (Main.mouseItem != null && Main.mouseItem.ModItem != null && Main.mouseItem.ModItem is UpgradeItemBase upgradeItem) {
				if (UpgradeItem == null) {
					UpgradeItem = upgradeItem;
					Main.mouseItem = new Item();
				}
				else {
					var item = UpgradeItem.Item;
					UpgradeItem = upgradeItem;
					Main.mouseItem = item;
				}
				OnUpgradeItemChange?.Invoke(this);
				//SoundEngine.PlaySound(12);
			}
			else if ((Main.mouseItem == null || Main.mouseItem.type == ItemID.None) && UpgradeItem != null) {
				Main.mouseItem = UpgradeItem.Item;
				UpgradeItem = null;
				OnUpgradeItemChange?.Invoke(this);
				//SoundEngine.PlaySound(12);
			}
		}

		protected override void DrawSelf(SpriteBatch sb) {
			base.DrawSelf(sb);

			if (UpgradeItem == null)
				return;
			var textureAsset = ModContent.Request<Texture2D>(UpgradeItem.Texture, AssetRequestMode.ImmediateLoad);
			if (textureAsset.State != AssetState.Loaded)
				return;
			var dimensions = GetDimensions();
			if (dimensions.ToRectangle().Contains(Main.MouseScreen.ToPoint())) {
				Main.HoverItem = UpgradeItem.Item.Clone();
				Main.instance.MouseText(UpgradeItem.Item.Name, UpgradeItem.Item.rare, 0);
			}
			float scale = Math.Min(dimensions.Width / textureAsset.Width(), dimensions.Height / textureAsset.Height()) * 0.6f;
			Vector2 size = new Vector2(textureAsset.Width(), textureAsset.Height()) * scale;
			sb.Draw(textureAsset.Value, dimensions.Center() - size / 2f, null, Color, 0f,
				Vector2.Zero, scale, SpriteEffects.None, 0f);
		}
	}
}