using ArknightsMod.Players;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Material
{
	// 背包里只要有它就会持续生效，具体判定逻辑在 BloodMushroomPlayer 里。
	// 估价会随连击等级实时刷新，所以每帧都在这里重写 Item.value，而不是走基类的固定估价表。
	public class BloodMushroom : RareCollectibleItem
	{
		public override int BaseOriginiumIngotValue => BloodMushroomPlayer.BaseValue;

		public override void UpdateInventory(Player player) {
			var bloodPlayer = player.GetModPlayer<BloodMushroomPlayer>();
			bloodPlayer.HasBloodMushroom = true;
			Item.value = BloodMushroomPlayer.BaseValue + bloodPlayer.ComboLevel * BloodMushroomPlayer.ValueStepPerKill;
		}
	}
}
