using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor.Vanity.Defender
{
	[AutoloadEquip(EquipType.Body)]
	internal class MudrockChestplate : ModItem
	{
		public override void SetStaticDefaults() {
			ArmorIDs.Body.Sets.HidesTopSkin[Item.bodySlot] = true;
		}
		public override void SetDefaults() {
			Item.width = Item.height = 16;
			Item.rare = ItemRarityID.LightPurple;
			Item.vanity = true;
		}
		internal class MudrockChestplate_EX : PlayerDrawLayer
		{
			private Asset<Texture2D> MudrockChestplate_EX_Texture;

			public override void Load()
				=> MudrockChestplate_EX_Texture = ModContent.Request<Texture2D>("ArknightsMod/Content/Items/Armor/Vanity/Defender/MudrockChestplate_Body_EX");
			public override void Unload()
				=> MudrockChestplate_EX_Texture = null;
			public override bool GetDefaultVisibility(PlayerDrawSet drawInfo) {
				Item body = new(ModContent.ItemType<MudrockChestplate>());
				return drawInfo.drawPlayer.body == body.bodySlot;
			}
			public override Position GetDefaultPosition()
			=> new AfterParent(PlayerDrawLayers.Head);
			protected override void Draw(ref PlayerDrawSet drawInfo) {
				Player player = drawInfo.drawPlayer;
				if (player.dead || player.invis)
					return;

				int BodyFrameIndex = player.bodyFrame.Y / player.bodyFrame.Height;
				Vector2 HeadgearOffset = Main.OffsetsPlayerHeadgear[BodyFrameIndex];
				Texture2D texture = MudrockChestplate_EX_Texture.Value;
				Vector2 position = drawInfo.Position - Main.screenPosition + new Vector2(player.width / 2 - player.bodyFrame.Width / 2, player.height - player.bodyFrame.Height + 4f) + player.bodyPosition;
				Vector2 origin = drawInfo.bodyVect;

				DrawData drawData = new(texture, position.Floor() + origin + HeadgearOffset + new Vector2(0, -2), texture.Frame(9, 4, 0, 1), drawInfo.colorArmorBody, player.bodyRotation, origin, 1f, drawInfo.playerEffect, 0) {
					shader = player.cBody
				};
				drawInfo.DrawDataCache.Add(drawData);
			}
		}
	}
}
