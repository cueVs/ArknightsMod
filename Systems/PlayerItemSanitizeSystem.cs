using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.IO;
using Terraria.ModLoader;

namespace ArknightsMod.Systems
{
	// 角色加载后清理无法绘制贴图的物品，避免角色选择界面 GetItemDrawFrame 处 NullReferenceException
	internal sealed class PlayerItemSanitizeSystem : ModSystem
	{
		public override void Load()
		{
			On_Player.GetFileData += Player_GetFileData;
		}

		public override void Unload()
		{
			On_Player.GetFileData -= Player_GetFileData;
		}

		private static PlayerFileData Player_GetFileData(On_Player.orig_GetFileData orig, string file, bool cloudSave)
		{
			PlayerFileData data = orig(file, cloudSave);
			if (data?.Player != null)
				SanitizePlayerItems(data.Player);
			return data;
		}

		internal static void SanitizePlayerItems(Player player)
		{
			SanitizeItems(player.inventory);
			SanitizeItems(player.armor);
			SanitizeItems(player.dye);
			SanitizeItems(player.miscEquips);
			SanitizeItems(player.miscDyes);

			for (int i = 0; i < player.Loadouts.Length; i++) {
				var loadout = player.Loadouts[i];
				SanitizeItems(loadout.Armor);
				SanitizeItems(loadout.Dye);
			}

			if (!CanDrawItemType(player.inventory[player.selectedItem].type))
				player.selectedItem = 0;
		}

		private static void SanitizeItems(Item[] items)
		{
			for (int i = 0; i < items.Length; i++) {
				if (!CanDrawItemType(items[i].type))
					items[i].SetDefaults(ItemID.None);
			}
		}

		private static bool CanDrawItemType(int type)
		{
			if (type <= 0)
				return true;

			if (type >= TextureAssets.Item.Length || TextureAssets.Item[type] == null)
				return false;

			Main.instance.LoadItem(type);
			return TextureAssets.Item[type].Value != null;
		}
	}
}
