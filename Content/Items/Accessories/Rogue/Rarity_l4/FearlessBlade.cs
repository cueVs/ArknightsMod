using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria.Localization;
namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l4
{
    public class FearlessBlade : ModItem
    {

        
        public override void SetStaticDefaults()
        {
            
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            Player player = Main.LocalPlayer;
            if (player == null) return;


            bool isEquipped = false;
            for (int i = 3; i < 10; i++) 
            {
                if (player.armor[i].type == Type) 
                {
                    isEquipped = true;
                    break;
                }
            }

            if (!isEquipped)
            {

                string tt = Language.GetTextValue("Mods.sk.Default.None");
                tooltips.Add(new TooltipLine(Mod, "NotEquipped", tt));
                return;
            }

            // 穿戴时显示额外信息
            var modPlayer = player.GetModPlayer<FearlessBladePlayer>();
            if (modPlayer == null) return;

            string TXT = Language.GetTextValue("Mods.sk.Default.UP");
            string c = Language.GetTextValue("Mods.sk.Default.debug");

            string weaponName = modPlayer.buffedWeapon?.Name ?? c;
            string armorName = modPlayer.buffedArmor?.Name ?? c;

            tooltips.Add(new TooltipLine(Mod, "WeaponBuff", $"{TXT} {weaponName}") { OverrideColor = Color.Gold });
            tooltips.Add(new TooltipLine(Mod, "ArmorBuff", $"{TXT} {armorName}") { OverrideColor = Color.Gold });
        }


        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.accessory = true;
            Item.rare = ItemRarityID.Master;
			Item.value = Item.sellPrice(0, 16, 0, 0);
		}

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<FearlessBladePlayer>().active = true;

        }
    }

    public class FearlessBladePlayer : ModPlayer
    {
        public bool active;

        // 当前增益的物品
        public Item buffedWeapon;
        public Item buffedArmor;


        private int originalWeaponDamage;
        private int originalArmorDefense;


        private int weaponIndex = -1;
        private int armorIndex = -1;
        private bool weaponIsEquipped;
        private bool armorIsEquipped;

		private int findTimer = 0;
		private const int FIND_INTERVAL = 30;
		public override void ResetEffects()
        {

            if (!active)
            {
                ResetBuffs();
            }
            active = false;
        }

        public override void PreUpdate()
        {

            if (!active)
            {
                ResetBuffs();
                return;
            }


            bool weaponValid = buffedWeapon != null && !buffedWeapon.IsAir && PlayerHasItem(buffedWeapon);
            bool armorValid = buffedArmor != null && !buffedArmor.IsAir && PlayerHasItem(buffedArmor);


            if (!weaponValid)
            {
                if (buffedWeapon != null) buffedWeapon.damage = originalWeaponDamage;
                buffedWeapon = null;
            }
            if (!armorValid)
            {
                if (buffedArmor != null) buffedArmor.defense = originalArmorDefense;
                buffedArmor = null;
            }

            // 查找新的最高稀有度物品
            FindHighestRarityItems();

            
        }

        private bool PlayerHasItem(Item item)
        {
            // 检查物品是否在背包中
            for (int i = 0; i < 50; i++)
            {
                if (Player.inventory[i] == item) return true;
            }

            // 检查物品是否在装备栏中
            for (int i = 0; i < Player.armor.Length; i++)
            {
                if (Player.armor[i] == item) return true;
            }

            return false;
        }

        private void ResetBuffs()
        {

            if (buffedWeapon != null)
            {
                buffedWeapon.damage = originalWeaponDamage;
                buffedWeapon = null;
            }

            if (buffedArmor != null)
            {
                buffedArmor.defense = originalArmorDefense;
                buffedArmor = null;
            }


            weaponIndex = -1;
            armorIndex = -1;
        }



		private void FindHighestRarityItems() {
			if (--findTimer > 0)
				return;
			findTimer = FIND_INTERVAL;

			// 每帧重新查找武器
			var weapons = GetAllWeapons();
			if (weapons.Count > 0) {
				var bestWeapon = weapons
					.OrderByDescending(item => GetAdjustedRarity(item))
					.ThenBy(item => GetItemIndex(item))
					.First();

				// 如果最优物品变化了
				if (buffedWeapon != bestWeapon) {
					// 恢复旧物品
					if (buffedWeapon != null)
						buffedWeapon.damage = originalWeaponDamage;

					// 应用新物品
					buffedWeapon = bestWeapon;
					originalWeaponDamage = bestWeapon.damage;
					bestWeapon.damage = (int)(bestWeapon.damage * 1.5f);
					weaponIndex = GetItemIndex(bestWeapon);
					weaponIsEquipped = IsItemEquipped(bestWeapon);
				}
			}
			else if (buffedWeapon != null) {
				// 没有武器了，恢复
				buffedWeapon.damage = originalWeaponDamage;
				buffedWeapon = null;
			}

			// 每帧重新查找防具
			var armors = GetAllArmors();
			if (armors.Count > 0) {
				var bestArmor = armors
					.OrderByDescending(item => GetAdjustedRarity(item))
					.ThenBy(item => GetItemIndex(item))
					.First();

				// 如果最优物品变化了
				if (buffedArmor != bestArmor) {
					// 恢复旧物品
					if (buffedArmor != null)
						buffedArmor.defense = originalArmorDefense;

					// 应用新物品
					buffedArmor = bestArmor;
					originalArmorDefense = bestArmor.defense;
					bestArmor.defense = (int)(bestArmor.defense * 1.5f);
					armorIndex = GetItemIndex(bestArmor);
					armorIsEquipped = IsItemEquipped(bestArmor);
				}
			}
			else if (buffedArmor != null) {
				// 没有防具了，恢复
				buffedArmor.defense = originalArmorDefense;
				buffedArmor = null;
			}
		}

		private bool IsItemEquipped(Item item)
        {
            return Player.armor.Contains(item);
        }

        private List<Item> GetAllWeapons()
        {
            var weapons = new List<Item>();

            // 扫描背包
            for (int i = 0; i < 50; i++)
            {
                Item item = Player.inventory[i];
                if (IsValidWeapon(item)) weapons.Add(item);
            }

            // 扫描装备栏
            for (int i = 0; i < Player.armor.Length; i++)
            {
                Item item = Player.armor[i];
                if (IsValidWeapon(item)) weapons.Add(item);
            }

            return weapons;
        }

        private List<Item> GetAllArmors()
        {
            var armors = new List<Item>();

            // 扫描背包
            for (int i = 0; i < 50; i++)
            {
                Item item = Player.inventory[i];
                if (IsValidArmor(item)) armors.Add(item);
            }

            // 扫描装备栏
            for (int i = 0; i < Player.armor.Length; i++)
            {
                Item item = Player.armor[i];
                if (IsValidArmor(item)) armors.Add(item);
            }

            return armors;
        }

        private int GetItemIndex(Item item)
        {
            // 在背包中查找
            for (int i = 0; i < 50; i++)
            {
                if (Player.inventory[i] == item) return i;
            }

            // 在装备栏中查找
            for (int i = 0; i < Player.armor.Length; i++)
            {
                if (Player.armor[i] == item) return 100 + i;
            }

            return int.MaxValue;
        }

        private bool IsValidWeapon(Item item)
        {
            return item != null && !item.IsAir && item.damage > 0 && item.rare != -11;
        }

        private bool IsValidArmor(Item item)
        {
            return item != null && !item.IsAir && item.defense > 0 && item.rare != -11;
        }

        private int GetAdjustedRarity(Item item)
        {

            if (item.rare < 0)
            {
//-12.-13这两比较特殊
                return item.rare == -12 ? int.MaxValue - 1 :
                       item.rare == -13 ? int.MaxValue - 2 : item.rare;
            }
            return item.rare;
        }

        
    }

    public class FearlessBladeGlobalItem : GlobalItem
    {
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            if (Main.netMode == NetmodeID.Server) return;

            Player player = Main.LocalPlayer;
            if (player == null) return;

            var modPlayer = player.GetModPlayer<FearlessBladePlayer>();
            if (modPlayer == null || !modPlayer.active) return;

            
        }

        public override bool InstancePerEntity => true;
        public override bool AppliesToEntity(Item entity, bool lateInstantiation) => true;
    }
}