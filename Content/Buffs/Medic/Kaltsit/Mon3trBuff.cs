using ArknightsMod.Content.Items.Armor.Medic.Kaltsit;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs.Medic.Kaltsit
{
	// 凯尔希套装效果的"存在标记"：纯粹的 UI 图标，不负责生成 M3。
	//
	// 一开始想照抄 CameraTruckBuff（可露希尔小车）那套"buff 自己在 Update() 里生成实体"
	// 的写法，结果没有真正生成 M3——回头细看才发现 CameraTruckBuff 其实也**不会**自己生成
	// 东西，它只负责"小车还在就续期，小车没了就摘掉"，真正的 NewProjectile 调用是武器
	// 右键那边主动触发的。这个 mod 里唯一"buff 常驻、自动补生成"的先例是可露希尔的跟随宠物
	// CommandCenterPet.EnsureFor——那是从 ModPlayer 每帧调用的静态方法，不是从 ModBuff.Update
	// 里生成。改成同样的写法：这里只管续期/摘除，生成逻辑挪到 Mon3tr.EnsureFor，由
	// KaltsitSetPlayer.PostUpdateEquips 每帧调用。
	public class Mon3trBuff : ModBuff
	{
		public override string Texture => "ArknightsMod/Content/Buffs/Medic/Kaltsit/Mon3trBuff";

		public override void SetStaticDefaults() {
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			if (!player.GetModPlayer<KaltsitSetPlayer>().KaltsitSetActive) {
				player.DelBuff(buffIndex);
				buffIndex--;
				return;
			}

			player.buffTime[buffIndex] = 18000;
		}
	}
}
