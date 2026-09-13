using ArknightsMod.Content.Items.Armor.NeoArmorReforge;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Defender.Mudrock
{
	[AutoloadEquip(EquipType.Body)]
	internal class MudrockBody : NeoArmorReforgeVanityBody
	{
		public override int Rarity => 6;

		// 切换后：图标换成胸甲图标，穿戴帧表换成胸甲帧表。
		public override string AltIconTexture => "ArknightsMod/Content/Items/Armor/Defender/Mudrock/MudrockChestplate";
		internal override string AltEquipTexture => "ArknightsMod/Content/Items/Armor/Defender/Mudrock/MudrockChestplate_Body";
		protected override string ToggleHintKey => "Mods.ArknightsMod.ArmorSets.Mudrock.ToggleHint";

		public override NeoArmorReforgeSetProfile SetProfile => new() {
			Defense = 50,
			LifeBonus = 222,
			LocalizationPrefix = "Mods.ArknightsMod.ArmorSets.Mudrock",
		};
	}

	// 切换到胸甲形态时额外叠加的一层装饰贴图。
	internal class MudrockBodyDrawLayer : PlayerDrawLayer
	{
		private Asset<Texture2D> texture;

		public override void Load() {
			texture = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Armor/Defender/Mudrock/MudrockChestplate_Body_EX");
		}

		public override void Unload() {
			texture = null;
		}

		public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
			Player player = drawInfo.drawPlayer;
			// 实际显示的躯干 = 社交/时装栏(armor[11]) 优先，否则盔甲栏(armor[1])。
			// 时装和套装（MudrockBodySet）都可能是"实际显示"的那一件，两边都认，
			// 只要它当前处于切换后的形态就画这层额外胸甲。
			Item shown = !player.armor[11].IsAir ? player.armor[11] : player.armor[1];
			bool helmetForm = shown.ModItem switch {
				MudrockBody vanity => vanity.HelmetForm,
				NeoArmorReforgeSetPiece piece when piece.Vanity is MudrockBody => piece.HelmetForm,
				_ => false,
			};
			return helmetForm;
		}

		public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.Head);

		protected override void Draw(ref PlayerDrawSet drawInfo) {
			Player player = drawInfo.drawPlayer;
			if (player.dead || player.invis)
				return;

			int bodyFrameIndex = player.bodyFrame.Y / player.bodyFrame.Height;
			Vector2 headgearOffset = Main.OffsetsPlayerHeadgear[bodyFrameIndex];
			Texture2D tex = texture.Value;
			Vector2 position = drawInfo.Position - Main.screenPosition
				+ new Vector2(player.width / 2 - player.bodyFrame.Width / 2, player.height - player.bodyFrame.Height + 4f)
				+ player.bodyPosition;
			Vector2 origin = drawInfo.bodyVect;

			DrawData drawData = new(tex, position.Floor() + origin + headgearOffset + new Vector2(0, -2),
				tex.Frame(9, 4, 0, 1), drawInfo.colorArmorBody, player.bodyRotation, origin, 1f, drawInfo.playerEffect, 0) {
				shader = player.cBody
			};
			drawInfo.DrawDataCache.Add(drawData);
		}
	}
}
