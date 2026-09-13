using System;
using System.Collections.Generic;

// 统一武器数值覆写中心
// 开关打开时，GlobalItem 用此表的值覆盖各武器的原始数值
// 关闭时，各武器按自身 SetDefaults 的原始数值生效
//
// 使用方法：
//   1. 在游戏内 Mod 配置界面开启「启用统一武器数值覆写」
//   2. 在下方字典中找到目标武器，修改对应字段的值即可
//   3. 只需填写你想修改的字段，其余留 null 则不覆写

namespace ArknightsMod.Common
{
	// 覆写属性结构体，所有字段为 nullable，null 表示不覆写
	public struct WeaponStats
	{
		// 攻击力
		public int? Damage;
		// 使用间隔(tick)
		public int? UseTime;
		// 动画时长(tick)
		public int? UseAnimation;
		// 弹幕射速
		public float? ShootSpeed;
		// 击退
		public float? KnockBack;
		// 暴击率(不含默认4)
		public int? Crit;
		// 魔力消耗
		public int? Mana;
	}

	public static class WeaponStatOverride
	{
		// ══════════════════════════════════════════════════════════════════
		//  全局覆写表
		//  修改字段值即可覆写对应武器的属性，null 字段不生效
		// ══════════════════════════════════════════════════════════════════
		public static readonly Dictionary<Type, WeaponStats> Overrides = new()
		{
			// ══════════════════════════════════════════════════════════════
			//  Guard (近卫)
			// ══════════════════════════════════════════════════════════════

			// BeehunterBag - 蜜蜂猎手 - 蜂袋 (EliteDamage=[23,28,34])
			// 原始值: damage=23, useTime=20, useAnimation=20, knockBack=4
			[typeof(Content.Items.Weapons.Guard.Beehunter.BeehunterBag)] = new() {
				Damage = 23, UseTime = 20, UseAnimation = 20, KnockBack = 4f
			},

			// ChenSword_Item - 陈 - 赤霄
			// 原始值: damage=122, useTime=39, useAnimation=39, knockBack=9
			[typeof(Content.Items.Weapons.Guard.Chen.ChenSword_Item)] = new() {
				Damage = 122, UseTime = 39, UseAnimation = 39, KnockBack = 9f
			},

			// DobermannWhip - 杜宾 - 长鞭 (EliteDamage=[41,52,62])
			// 原始值: damage=41, useTime=38, useAnimation=38, knockBack=4, shootSpeed=6
			[typeof(Content.Items.Weapons.Guard.Dobermann.DobermannWhip)] = new() {
				Damage = 41, UseTime = 38, UseAnimation = 38, KnockBack = 4f, ShootSpeed = 6f
			},

			// EstelleGlove - 艾丝黛拉 - 拳套 (EliteDamage=[39,47,57])
			// 原始值: damage=39, useTime=22, useAnimation=22, knockBack=5, shootSpeed=16
			[typeof(Content.Items.Weapons.Guard.Estelle.EstelleGlove)] = new() {
				Damage = 39, UseTime = 22, UseAnimation = 22, KnockBack = 5f, ShootSpeed = 16f
			},

			// FrostleafAxe - 霜叶 - 战斧 (EliteDamage=[40,48,58])
			// 当前值: damage=40, useTime=14, useAnimation=14, knockBack=7
			[typeof(Content.Items.Weapons.Guard.Frostleaf.FrostleafAxe)] = new() {
				Damage = 40, UseTime = 14, UseAnimation = 14, KnockBack = 7f
			},

			// HongdouLance - 红豆 - 长枪 (EliteDamage=[44,52,60])
			// 当前值: damage=44, useTime=14, useAnimation=14, knockBack=5, shootSpeed=3
			[typeof(Content.Items.Weapons.Guard.Hongdou.HongdouLance)] = new() {
				Damage = 44, UseTime = 14, UseAnimation = 14, KnockBack = 5f, ShootSpeed = 3f
			},

			// LupineScarlet - 狼德 - 赤红
			// 原始值: damage=60, useTime=30, useAnimation=30, knockBack=6
			[typeof(Content.Items.Weapons.Guard.Lupine.LupineScarlet)] = new() {
				Damage = 60, UseTime = 30, UseAnimation = 30, KnockBack = 6f
			},

			// MataimaruGlaive - 真陀 - 长戟 (EliteDamage=[60,72,87])
			// 原始值: damage=60, useTime=28, useAnimation=28, knockBack=6
			[typeof(Content.Items.Weapons.Guard.Matoimaru.MataimaruGlaive)] = new() {
				Damage = 60, UseTime = 28, UseAnimation = 28, KnockBack = 6f
			},

			// MelanthasSword - 玛莉兰塔 - 剑 (EliteDamage=[67,83])
			// 原始值: damage=67, useTime=22, useAnimation=22, knockBack=5, shootSpeed=6
			[typeof(Content.Items.Weapons.Guard.Melantha.MelanthasSword)] = new() {
				Damage = 67, UseTime = 22, UseAnimation = 22, KnockBack = 5f, ShootSpeed = 6f
			},

			// MousseGlove - 慕斯 - 拳套 (EliteDamage=[43,52,62])
			// 原始值: damage=43, useTime=22, useAnimation=22, knockBack=4, shootSpeed=16
			[typeof(Content.Items.Weapons.Guard.Mousse.MousseGlove)] = new() {
				Damage = 43, UseTime = 22, UseAnimation = 22, KnockBack = 4f, ShootSpeed = 16f
			},

			// OblivionisSword - 乌萨斯教官 - 之剑
			// 原始值: damage=145, useTime=18, useAnimation=18, knockBack=5, shootSpeed=6
			[typeof(Content.Items.Weapons.Guard.Oblivionis.OblivionisSword)] = new() {
				Damage = 145, UseTime = 18, UseAnimation = 18, KnockBack = 5f, ShootSpeed = 6f
			},

			// SilverAshWeapon - 银灰 - 真银斩
			// 原始值: damage=142, useTime=39, useAnimation=39, knockBack=2, shootSpeed=4
			[typeof(Content.Items.Weapons.Guard.SilverAsh.SilverAshWeapon)] = new() {
				Damage = 142, UseTime = 39, UseAnimation = 39, KnockBack = 2f, ShootSpeed = 4f
			},

			// SkadisGreatsword - 斯卡蒂 - 巨剑 (EliteDamage=[78,96,113])
			// 原始值: damage=78, useTime=40, useAnimation=40, knockBack=8
			[typeof(Content.Items.Weapons.Guard.Skadi.SkadisGreatsword)] = new() {
				Damage = 78, UseTime = 40, UseAnimation = 40, KnockBack = 8f
			},

			// SurtrLaevatain - 史尔特尔 - 炎阳之剑
			// 原始值: damage=134, useTime=15, useAnimation=15, knockBack=10
			[typeof(Content.Items.Weapons.Guard.Surtr.SurtrLaevatain)] = new() {
				Damage = 134, UseTime = 15, UseAnimation = 15, KnockBack = 10f
			},

			// ThornsWeapon - 棘刺 - 至高之术
			// 原始值: damage=142, useTime=39, useAnimation=39, knockBack=2, shootSpeed=16
			[typeof(Content.Items.Weapons.Guard.Thorns.ThornsWeapon)] = new() {
				Damage = 142, UseTime = 39, UseAnimation = 39, KnockBack = 2f, ShootSpeed = 16f
			},

			// ══════════════════════════════════════════════════════════════
			//  Sniper (狙击)
			// ══════════════════════════════════════════════════════════════

			// AdnachielCrossbow - 安德切尔 - 十字弩
			// 原始值: damage=19, useTime=30, useAnimation=30, knockBack=2, shootSpeed=15
			[typeof(Content.Items.Weapons.Sniper.Adnachiel.AdnachielCrossbow)] = new() {
				Damage = 19, UseTime = 30, UseAnimation = 30, KnockBack = 2f, ShootSpeed = 15f
			},

			// ExusiaiVector - 能天使 - Vector冲锋枪
			// 原始值: damage=108, useTime=15, useAnimation=15, knockBack=5, shootSpeed=8
			[typeof(Content.Items.Weapons.Sniper.Exusiai.ExusiaiVector)] = new() {
				Damage = 108, UseTime = 15, UseAnimation = 15, KnockBack = 5f, ShootSpeed = 8f
			},

			// JessicaGun - 杰西卡 - 手枪 (EliteDamage=[37,44,53])
			// 原始值: damage=37, useTime=30, useAnimation=30, knockBack=3, shootSpeed=14
			[typeof(Content.Items.Weapons.Sniper.Jessica.JessicaGun)] = new() {
				Damage = 37, UseTime = 30, UseAnimation = 30, KnockBack = 3f, ShootSpeed = 14f
			},

			// KroosCrossbow - 克洛丝 - 十字弩
			// 原始值: damage=19, useTime=30, useAnimation=30, knockBack=2, shootSpeed=9, crit=16
			[typeof(Content.Items.Weapons.Sniper.Kroos.KroosCrossbow)] = new() {
				Damage = 19, UseTime = 30, UseAnimation = 30, KnockBack = 2f, ShootSpeed = 9f, Crit = 16
			},

			// KroosAlterCrossbow - 克洛丝(寒芒) - 十字弩
			// 原始值: damage=72, useTime=30, useAnimation=30, knockBack=3, shootSpeed=8
			[typeof(Content.Items.Weapons.Sniper.KroosAlter.KroosAlterCrossbow)] = new() {
				Damage = 72, UseTime = 30, UseAnimation = 30, KnockBack = 3f, ShootSpeed = 8f
			},

			// MeteorBow - 流星 - 弓 (EliteDamage=[35,42,51])
			// 原始值: damage=35, useTime=30, useAnimation=30, knockBack=2, shootSpeed=12, crit=4
			[typeof(Content.Items.Weapons.Sniper.Meteor.MeteorBow)] = new() {
				Damage = 35, UseTime = 30, UseAnimation = 30, KnockBack = 2f, ShootSpeed = 12f, Crit = 4
			},

			// PozemkaCrossbow - 帕捷契卡 - 十字弩
			// 原始值: damage=175, useTime=48, useAnimation=48, knockBack=5, shootSpeed=20
			[typeof(Content.Items.Weapons.Sniper.Pozemka.PozemkaCrossbow)] = new() {
				Damage = 175, UseTime = 48, UseAnimation = 48, KnockBack = 5f, ShootSpeed = 20f
			},

			// RangersBow - 猎人 - 弓
			// 原始值: damage=16, useTime=30, useAnimation=30, knockBack=4, shootSpeed=160, crit=2
			[typeof(Content.Items.Weapons.Sniper.Rangers.RangersBow)] = new() {
				Damage = 16, UseTime = 30, UseAnimation = 30, KnockBack = 4f, ShootSpeed = 160f, Crit = 2
			},

			// SchwarzBow - 黑 - 弩
			// 原始值: damage=168, useTime=48, useAnimation=48, knockBack=10, shootSpeed=20
			[typeof(Content.Items.Weapons.Sniper.Schwarz.SchwarzBow)] = new() {
				Damage = 168, UseTime = 48, UseAnimation = 48, KnockBack = 10f, ShootSpeed = 20f
			},

			// Shirayuki_Shuriken - 白雪 - 飞镖
			// 原始值: damage=74, useTime=84, useAnimation=84, knockBack=1, shootSpeed=16
			[typeof(Content.Items.Weapons.Sniper.Shirayuki.Shirayuki_Shuriken)] = new() {
				Damage = 74, UseTime = 84, UseAnimation = 84, KnockBack = 1f, ShootSpeed = 16f
			},

			// TyphonBow - 提丰 - 弓
			// 原始值: damage=209, useTime=72, useAnimation=72, knockBack=6, shootSpeed=8
			[typeof(Content.Items.Weapons.Sniper.Typhon.TyphonBow)] = new() {
				Damage = 209, UseTime = 72, UseAnimation = 72, KnockBack = 6f, ShootSpeed = 8f
			},

			// WisadelCannon - 维什戴尔 - 祖宗发射器(炮台)
			// 原始值: damage=138, useTime=30, useAnimation=30, knockBack=15, shootSpeed=16, crit=20
			[typeof(Content.Items.Weapons.Sniper.Wisadel.WisadelCannon)] = new() {
				Damage = 138, UseTime = 30, UseAnimation = 30, KnockBack = 15f, ShootSpeed = 16f, Crit = 20
			},

			// ══════════════════════════════════════════════════════════════
			//  Caster (术师)
			// ══════════════════════════════════════════════════════════════

			// _12FWand - 12F - 法杖
			// 原始值: damage=30, useTime=87, useAnimation=87, knockBack=4, mana=8
			[typeof(Content.Items.Weapons.Caster._12F._12FWand)] = new() {
				Damage = 30, UseTime = 87, UseAnimation = 87, KnockBack = 4f, Mana = 8
			},

			// DurinWand - 杜林 - 法杖
			// 原始值: damage=24, useTime=40, useAnimation=40, knockBack=4, shootSpeed=16, mana=5, crit=2
			[typeof(Content.Items.Weapons.Caster.Durin.DurinWand)] = new() {
				Damage = 24, UseTime = 40, UseAnimation = 40, KnockBack = 4f, ShootSpeed = 16f, Mana = 5, Crit = 2
			},

			// GoldenglowWand - 澄闪 - 法杖 (EliteDamage=[30,36,41])
			// 原始值: damage=30, useTime=23, useAnimation=23, knockBack=2, shootSpeed=10, mana=8, crit=4
			[typeof(Content.Items.Weapons.Caster.Goldenglow.GoldenglowWand)] = new() {
				Damage = 30, UseTime = 23, UseAnimation = 23, KnockBack = 2f, ShootSpeed = 10f, Mana = 8, Crit = 4
			},

			// HazeMagicBook - 阴云 - 魔法书
			// 原始值: damage=50, useTime=48, useAnimation=48, knockBack=3, shootSpeed=8, mana=6
			[typeof(Content.Items.Weapons.Caster.Haze.HazeMagicBook)] = new() {
				Damage = 50, UseTime = 48, UseAnimation = 48, KnockBack = 3f, ShootSpeed = 8f, Mana = 6
			},

			// Lava_Dagger - 炎熔 - 法杖
			// 原始值: damage=40, useTime=87, useAnimation=87, knockBack=3, shootSpeed=8
			[typeof(Content.Items.Weapons.Caster.Lava.Lava_Dagger)] = new() {
				Damage = 40, UseTime = 87, UseAnimation = 87, KnockBack = 3f, ShootSpeed = 8f
			},

			// StewardStaff - 史都华德 - 法杖 (EliteDamage=[43,54])
			// 原始值: damage=43, useTime=30, useAnimation=30, knockBack=2, shootSpeed=11, mana=7, crit=4
			[typeof(Content.Items.Weapons.Caster.Steward.StewardStaff)] = new() {
				Damage = 43, UseTime = 30, UseAnimation = 30, KnockBack = 2f, ShootSpeed = 11f, Mana = 7, Crit = 4
			},

			// ValarqvinWeapon - 瓦尔薇 - 法杖
			// 原始值: damage=65, useTime=48, useAnimation=48, knockBack=2, shootSpeed=13
			[typeof(Content.Items.Weapons.Caster.Valarqvin.ValarqvinWeapon)] = new() {
				Damage = 65, UseTime = 48, UseAnimation = 48, KnockBack = 2f, ShootSpeed = 13f
			},

			// ══════════════════════════════════════════════════════════════
			//  Defender (重装)
			// ══════════════════════════════════════════════════════════════

			// BeagleWeapon - 米格鲁 - 盾牌
			// 原始值: damage=23, useTime=25, useAnimation=25, knockBack=7, crit=2
			[typeof(Content.Items.Weapons.Defender.Beagle.BeagleWeapon)] = new() {
				Damage = 23, UseTime = 25, UseAnimation = 25, KnockBack = 7f, Crit = 2
			},

			// CuoraWeapon - 蛇屠箱 - 盾牌
			// 原始值: damage=23, useTime=25, useAnimation=25, knockBack=7, crit=2
			[typeof(Content.Items.Weapons.Defender.Cuora.CuoraWeapon)] = new() {
				Damage = 23, UseTime = 25, UseAnimation = 25, KnockBack = 7f, Crit = 2
			},

			// DN_Weapon - 杜纳 - 盾牌
			// 原始值: damage=23, useTime=25, useAnimation=25, knockBack=7, crit=2
			[typeof(Content.Items.Weapons.Defender.Durnar.DN_Weapon)] = new() {
				Damage = 23, UseTime = 25, UseAnimation = 25, KnockBack = 7f, Crit = 2
			},

			// NianWeapon - 年 - 棱镜
			// 原始值: damage=124, useTime=45, useAnimation=45, knockBack=2.5, crit=21
			[typeof(Content.Items.Weapons.Defender.Nian.NianWeapon)] = new() {
				Damage = 124, UseTime = 45, UseAnimation = 45, KnockBack = 2.5f, Crit = 21
			},

			// NoirShield - 黑角 - 盾牌
			// 原始值: damage=12, useTime=36, useAnimation=36, knockBack=12, crit=2
			[typeof(Content.Items.Weapons.Defender.NoirCorne.NoirShield)] = new() {
				Damage = 12, UseTime = 36, UseAnimation = 36, KnockBack = 12f, Crit = 2
			},

			// ══════════════════════════════════════════════════════════════
			//  Vanguard (先锋)
			// ══════════════════════════════════════════════════════════════

			// BagpipeSpear - 棒棒 - 长枪
			// 原始值: damage=118, useTime=30, useAnimation=30, knockBack=2, shootSpeed=9
			[typeof(Content.Items.Weapons.Vanguard.Bagpipe.BagpipeSpear)] = new() {
				Damage = 118, UseTime = 30, UseAnimation = 30, KnockBack = 2f, ShootSpeed = 9f
			},

			// FangSpear - 芬 - 长枪
			// 原始值: damage=20, useTime=33, useAnimation=20, knockBack=1, shootSpeed=3
			[typeof(Content.Items.Weapons.Vanguard.Fang.FangSpear)] = new() {
				Damage = 20, UseTime = 33, UseAnimation = 20, KnockBack = 1f, ShootSpeed = 3f
			},

			// PlumePike - 普莉姆 - 长枪 (EliteDamage=[40,52])
			// 原始值: damage=40, useTime=24, useAnimation=24, knockBack=5, shootSpeed=3
			[typeof(Content.Items.Weapons.Vanguard.Plume.PlumePike)] = new() {
				Damage = 40, UseTime = 24, UseAnimation = 24, KnockBack = 5f, ShootSpeed = 3f
			},

			// ArtsBlade - 德克萨斯 - 法术之刃
			// 原始值: damage=72, useTime=33, useAnimation=33, knockBack=3
			[typeof(Content.Items.Weapons.Vanguard.Texas.ArtsBlade)] = new() {
				Damage = 72, UseTime = 33, UseAnimation = 33, KnockBack = 3f
			},

			// VanillaAxe - 香草 - 斧头 (EliteDamage=[31,43])
			// 原始值: damage=31, useTime=30, useAnimation=30, knockBack=5
			[typeof(Content.Items.Weapons.Vanguard.Vanilla.VanillaAxe)] = new() {
				Damage = 31, UseTime = 30, UseAnimation = 30, KnockBack = 5f
			},

			// YatoKatana - 夜刀 - 太刀
			// 原始值: damage=15, useTime=33, useAnimation=33, knockBack=4
			[typeof(Content.Items.Weapons.Vanguard.Yato.YatoKatana)] = new() {
				Damage = 15, UseTime = 33, UseAnimation = 33, KnockBack = 4f
			},

			// ══════════════════════════════════════════════════════════════
			//  Specialist (特种)
			// ══════════════════════════════════════════════════════════════

			// GravelDualBlades - 暗索 - 双刀 (EliteDamage=[25,30,36])
			// 原始值: damage=25, useTime=22, useAnimation=22, knockBack=3, shootSpeed=1
			[typeof(Content.Items.Weapons.Specialist.Gravel.GravelDualBlades)] = new() {
				Damage = 25, UseTime = 22, UseAnimation = 22, KnockBack = 3f, ShootSpeed = 1f
			},

			// RopeClaw - 暗索 - 钩爪 (EliteDamage=[44,53,64])
			// 原始值: damage=44, useTime=32, useAnimation=32, knockBack=4, shootSpeed=6
			[typeof(Content.Items.Weapons.Specialist.Rope.RopeClaw)] = new() {
				Damage = 44, UseTime = 32, UseAnimation = 32, KnockBack = 4f, ShootSpeed = 6f
			},

			// SceneCamera - 稀音 - 相机
			// 原始值: damage=24, useTime=30, useAnimation=30, knockBack=0, shootSpeed=11, mana=8
			[typeof(Content.Items.Weapons.Specialist.Scene.SceneCamera)] = new() {
				Damage = 24, UseTime = 30, UseAnimation = 30, KnockBack = 0f, ShootSpeed = 11f, Mana = 8
			},

			// ShawAxe - 阿消 - 斧头 (EliteDamage=[39,47,57])
			// 原始值: damage=39, useTime=30, useAnimation=30, knockBack=6
			[typeof(Content.Items.Weapons.Specialist.Shaw.ShawAxe)] = new() {
				Damage = 39, UseTime = 30, UseAnimation = 30, KnockBack = 6f
			},

			// ShawWaterCannon - 阿消 - 水炮
			// 原始值: damage=18, useTime=45, useAnimation=45, knockBack=10, shootSpeed=12
			[typeof(Content.Items.Weapons.Specialist.Shaw.ShawWaterCannon)] = new() {
				Damage = 18, UseTime = 45, UseAnimation = 45, KnockBack = 10f, ShootSpeed = 12f
			},

			// DarkChocolate - 德克萨斯(异格) - 黑巧克力
			// 原始值: damage=114, useTime=36, useAnimation=36, knockBack=6, shootSpeed=2
			[typeof(Content.Items.Weapons.Specialist.TexasAlter.DarkChocolate)] = new() {
				Damage = 114, UseTime = 36, UseAnimation = 36, KnockBack = 6f, ShootSpeed = 2f
			},

			// BlueberryDarkChocolate - 德克萨斯(异格) - 蓝莓黑巧克力
			// 原始值: damage=114, useTime=28, useAnimation=28, knockBack=6, shootSpeed=2
			[typeof(Content.Items.Weapons.Specialist.TexasAlter.BlueberryDarkChocolate)] = new() {
				Damage = 114, UseTime = 28, UseAnimation = 28, KnockBack = 6f, ShootSpeed = 2f
			},

			// Blueberry - 德克萨斯(异格) - 蓝莓
			// 原始值: damage=86, useTime=28, useAnimation=28, knockBack=6, shootSpeed=2
			[typeof(Content.Items.Weapons.Specialist.TexasAlter.Blueberry)] = new() {
				Damage = 86, UseTime = 28, UseAnimation = 28, KnockBack = 6f, ShootSpeed = 2f
			},

			// ══════════════════════════════════════════════════════════════
			//  Supporter (辅助)
			// ══════════════════════════════════════════════════════════════

			// BeanstalkOrb - 豆苗 - 法球 (EliteDamage=[57,64,74])
			// 原始值: damage=57, useTime=37, useAnimation=37, knockBack=2, shootSpeed=9, mana=11
			[typeof(Content.Items.Weapons.Supporter.Beanstalk.BeanstalkOrb)] = new() {
				Damage = 57, UseTime = 37, UseAnimation = 37, KnockBack = 2f, ShootSpeed = 9f, Mana = 11
			},

			// DeepcolorSketch - 深海色 - 素描
			// 原始值: damage=46, useTime=30, useAnimation=30, knockBack=2, shootSpeed=0, mana=10
			[typeof(Content.Items.Weapons.Supporter.Deepcolor.DeepcolorSketch)] = new() {
				Damage = 46, UseTime = 30, UseAnimation = 30, KnockBack = 2f, ShootSpeed = 0f, Mana = 10
			},

			// EarthspiritWand - 地灵 - 法杖 (EliteDamage=[37,57,74])
			// 原始值: damage=37, useTime=29, useAnimation=29, knockBack=2, shootSpeed=9, mana=7
			[typeof(Content.Items.Weapons.Supporter.Earthspirit.EarthspiritWand)] = new() {
				Damage = 37, UseTime = 29, UseAnimation = 29, KnockBack = 2f, ShootSpeed = 9f, Mana = 7
			},

			// OrchidUmbrellla - 梓兰 - 雨伞
			// 原始值: damage=24, useTime=57, useAnimation=57, knockBack=0, shootSpeed=8, mana=1
			[typeof(Content.Items.Weapons.Supporter.Orchid.OrchidUmbrellla)] = new() {
				Damage = 24, UseTime = 57, UseAnimation = 57, KnockBack = 0f, ShootSpeed = 8f, Mana = 1
			},

			// SaintBell - 普罗姆 - 圣铃
			// 原始值: damage=48, useTime=32, useAnimation=32, knockBack=0, shootSpeed=0, mana=12
			[typeof(Content.Items.Weapons.Supporter.Pramanix.SaintBell)] = new() {
				Damage = 48, UseTime = 32, UseAnimation = 32, KnockBack = 0f, ShootSpeed = 0f, Mana = 12
			},
		};
	}
}
