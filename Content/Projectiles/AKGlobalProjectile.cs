// 这里原来放着第一版"法术伤害标记"的尝试（SpellDamageModification / SpellDamageMarker），
// 整段是注释掉的，而且引用了一个从未存在过的 SpellDamageConfig 类，从来没有生效过。
// 法伤系统现在的正式实现在 Systems/Gameplay/Damage/ 下：
//   ● ArtsWeaponRegistry —— 哪些武器/哪些条件下的攻击算法术伤害
//   ● ArtsProjectileMarker —— 在弹幕生成那一刻打上"这发是法伤"的标记
//   ● DamageCategoryNPC —— 命中时按目标法抗扣减伤害
// 为避免两套同名概念并存造成混淆，这里的死代码已移除。
