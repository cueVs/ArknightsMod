using Terraria;
using Terraria.ModLoader;
using ArknightsMod.Content.Items.Weapons.Guard.SilverAsh;

namespace ArknightsMod.Players
{
    public class yinhui2player : ModPlayer
    {
		public bool yinhui2=false;
		public override void ResetEffects()
        {
			//����2��������Ч��
			if (yinhui2 == true)
            {
				Player.statDefense *= 2f;
				Player.lifeRegen += (int)(Player.statLifeMax2 * 0.12f);
			}
			if (Main.myPlayer != Player.whoAmI)
				return;  // ֻ�����������
			bool isHoldingTargetWeapon = Player.HeldItem.type == ModContent.ItemType<SilverAshWeapon>();
			if (!isHoldingTargetWeapon) {
				Player.GetModPlayer<yinhui2player>().yinhui2 = false;
			}

		}
    }
	public class yinhui3player : ModPlayer
	{
		public bool yinhui3 = false;
		public override void ResetEffects() {
			if (yinhui3 == true) {
				Player.statDefense *= 0.3f;
			}
			if (Main.myPlayer != Player.whoAmI)
				return;  // ֻ�����������
			bool isHoldingTargetWeapon2 = Player.HeldItem.type == ModContent.ItemType<SilverAshWeapon>();
			if (!isHoldingTargetWeapon2) {
				Player.GetModPlayer<yinhui3player>().yinhui3 = false;
			}

		}
	}
}