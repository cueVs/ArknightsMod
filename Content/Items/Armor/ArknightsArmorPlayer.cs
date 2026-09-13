using System;
using Terraria;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Items.Armor
{
    public class ArknightsArmorPlayer : ModPlayer
    {
		/// <summary>
		/// 额外防御力加成，按百分比算，要加算，不要直接赋值！！
		/// </summary>
		public float extraDefenseBonus = 0f;

		/// <summary>
		/// 生命水晶和生命果效果的削减百分比，必须>0
		/// </summary>
		public float LifeCrystalAndFruitEffectReduction = 0f;

		public override void PostUpdateEquips()
		{
			if (extraDefenseBonus != 0)
			{
				int bonusDefense = (int)(Player.statDefense * extraDefenseBonus);
				Player.statDefense += bonusDefense;
			}
		}

		public override void ResetEffects()
		{
			extraDefenseBonus = 0;
			LifeCrystalAndFruitEffectReduction = 0f;
		}

		public override void UpdateLifeRegen()
		{
			if (LifeCrystalAndFruitEffectReduction > 0)
			{
				int consumedLifeFruit = Player.ConsumedLifeFruit; //一个生命果提供5的生命上限
				int consumedLifeCrystals = Player.ConsumedLifeCrystals; //一个生命水晶提供20的生命上限

				//生命水晶
				float crystalReduction = consumedLifeCrystals * 20 * LifeCrystalAndFruitEffectReduction;
				int crystalReductionFinal;
				if (consumedLifeCrystals % 2 == 1) //奇数次消耗，向下取整
				{
					crystalReductionFinal = (int)Math.Floor(crystalReduction);
				}
				else //偶数次消耗，向上取整
				{
					crystalReductionFinal = (int)Math.Ceiling(crystalReduction);
				}

				//生命果
				float fruitReduction = consumedLifeFruit * 5 * LifeCrystalAndFruitEffectReduction;
				int fruitReductionFinal;
				if (consumedLifeFruit % 2 == 1) //奇数次消耗，向下取整
				{
					fruitReductionFinal = (int)Math.Floor(fruitReduction);
				}
				else //偶数次消耗，向上取整
				{
					fruitReductionFinal = (int)Math.Ceiling(fruitReduction);
				}
				Player.statLifeMax2 -= (crystalReductionFinal + fruitReductionFinal);
			}
		}
		#region (已弃用) 盔甲生命值比例替换系统
		/*public (float ratio, int value) LifeReplacement_Head = (0f, 0);
		public (float ratio, int value) LifeReplacement_Body = (0f, 0);
		public (float ratio, int value) LifeReplacement_Legs = (0f, 0);

		/// <summary>
		/// (已弃用) 盔甲生命值比例替换，在UpdateLifeRegen()里调用
		/// </summary>
		private void LifeRatioReplacement() {
			int baseLife = Player.statLifeMax;

			float totalRatio = 0f;
			int totalFixedValue = 0;

			//头盔
			if (LifeReplacement_Head.ratio > 0) {
				totalRatio += LifeReplacement_Head.ratio;
				totalFixedValue += LifeReplacement_Head.value;
			}
			//胸甲
			if (LifeReplacement_Body.ratio > 0) {
				totalRatio += LifeReplacement_Body.ratio;
				totalFixedValue += LifeReplacement_Body.value;
			}
			//腿甲
			if (LifeReplacement_Legs.ratio > 0) {
				totalRatio += LifeReplacement_Legs.ratio;
				totalFixedValue += LifeReplacement_Legs.value;
			}

			if (totalRatio > 0) {
				int newLife = (int)(baseLife * (1 - totalRatio)) + totalFixedValue;
				int offset = newLife - baseLife;
				Player.statLifeMax += offset;
				Player.statLifeMax2 += offset;
			}
		}

		/// <summary>
		/// (已弃用) 重置盔甲生命值比例替换，ResetEffects()里调用
		/// </summary>
		private void LifeRatioReplacement_Reset() {
			LifeReplacement_Head = (0f, 0);
			LifeReplacement_Body = (0f, 0);
			LifeReplacement_Legs = (0f, 0);
		}*/
		#endregion
	}
}
