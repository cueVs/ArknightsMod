namespace ArknightsMod.Content.Items.Material
{
	// 没有动态估价逻辑，固定基础估价，靠 SafeSetCollectibleDefaults 设一次就够了。
	public class WitheredMossBall : RareCollectibleItem
	{
		public override int BaseOriginiumIngotValue => 2;
	}
}
