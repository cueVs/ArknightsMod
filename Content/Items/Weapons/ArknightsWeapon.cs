using ArknightsMod.Common.Items;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ArknightsMod.Content.Items.Weapons
{
	public abstract class ArknightsWeapon : ModItem
	{
		private readonly static Dictionary<int, List<SkillData>> Skills = [];

		#region 技能等级
		// 等级系统开发完成后使用
		// Usable after the Level System completed
		private int[] skillLevel = new int[3];
		public override void LoadData(TagCompound tag) {
			tag[nameof(skillLevel)] = skillLevel;
		}

		public override void SaveData(TagCompound tag) {
			skillLevel = tag.GetIntArray(nameof(skillLevel));
		}
		#endregion

		/// <summary>
		/// 在这里注册技能数据，使用<see cref="AddSkillData(SkillData)"/>
		/// <br/>Add Skill Datas in here!Use <see cref="AddSkillData(SkillData)"/>
		/// </summary>
		public abstract void RegisterSkills();

		/// <summary>
		/// 密封<see cref="SetStaticDefaults()"/>以防止技能注册被覆盖
		/// Sealed override <see cref="SetStaticDefaults()"/> to prevent <see cref="RegisterSkills()"/> method being override
		/// </summary>
		public sealed override void SetStaticDefaults() {
			RegisterSkills();
			SetStaticDefaults(Type);
		}

		public virtual void SetStaticDefaults(int type) {

		}

		/// <summary>
		/// 添加技能数据
		/// </summary>
		/// <param name="skillData"></param>
		public void AddSkillData(SkillData skillData) {
			if (!Skills.TryGetValue(Type, out var list))
				Skills[Type] = list = [];
			list.Add(skillData);
		}

		/// <summary>
		/// 使用索引获取技能数据
		/// <br/>Use the index to get Skill Data
		/// </summary>
		/// <param name="index">Skill index, range must in 0~2</param>
		/// <returns></returns>
		public SkillData GetSkillData(int index) {
			if (!Skills.TryGetValue(Type, out var list)) {
				Main.NewText(DisplayName.Value + "未注册技能", Color.Red);
				return null;
			}

			return !list.IndexInRange(index) ? null : list[index];
		}
	}
}
