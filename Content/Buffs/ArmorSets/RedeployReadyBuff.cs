using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Buffs.ArmorSets
{
	/// <summary>
	/// 【再部署准备】—— 快速再部署类套装的通用 Buff。
	/// <para>
	/// 这个 Buff <b>本身不产生任何效果</b>，只是一个「状态已就绪」的可视化标记：
	/// 真正的复活时间缩短逻辑写在
	/// <see cref="ArknightsMod.Content.Items.Armor.RedeploySetPlayer"/> 里。
	/// 这样拆分是因为玩家死亡时 Buff 会被清空，把逻辑挂在 Buff 上会读不到状态。
	/// </para>
	/// <para>
	/// <b>多个干员共用这一个 Buff</b>，不需要每个套装再各自新建一个。
	/// 如果某个干员需要显示不一样的图标/名称，再单独建新的 ModBuff，
	/// 并在其 SetPlayer 里重写 <c>RedeployBuffType</c> 指向它。
	/// </para>
	/// <para>
	/// TODO(美术)：<c>RedeployReadyBuff.png</c> 目前是程序生成的占位图标（32×32），
	/// 需要替换成正式素材。
	/// </para>
	/// </summary>
	public class RedeployReadyBuff : ModBuff
	{
		public override void SetStaticDefaults() {
			// 不写入存档：脱下套装/退出游戏后不应保留
			Main.buffNoSave[Type] = true;
			// 显示剩余时间没有意义（每帧都在续期，只要穿着套装就一直在）
			Main.buffNoTimeDisplay[Type] = true;
		}
	}
}
