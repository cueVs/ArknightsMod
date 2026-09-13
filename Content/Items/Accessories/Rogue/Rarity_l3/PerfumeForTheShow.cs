using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ArknightsMod.Content.Items.Accessories.Rogue.Rarity_l3
{
    public class PerfumeForTheShow : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
			Item.value = Item.sellPrice(0, 6, 0, 0);
			Item.accessory = true;
            Item.rare = 1;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<PerfumeForTheShowPlayer>().active = true;
        }
        
    }

    public class PerfumeForTheShowPlayer : ModPlayer
    {
        public bool active;
        private int timer;

        public override void ResetEffects()
        {
            active = false;
        }
        private void SpawnHealEffect()
        {
            for (int i = 0; i < 8; i++)
            {
                Vector2 position = Player.Center + new Vector2(
                  Main.rand.Next(-20, 20),
                  Main.rand.Next(-Player.height / 2, Player.height / 2));

                Dust dust = Dust.NewDustPerfect(position, DustID.GreenTorch, Vector2.Zero);
                dust.noGravity = true;
                dust.scale = 1.2f;
                dust.velocity = new Vector2(0, -Main.rand.NextFloat(1f, 2f));
            }
        }
        public override void PostUpdate()
        {
            if (!active)
                return;

            // 每秒60帧，每秒触发一次
            if (++timer >= 60)
            {
                timer = 0;
                SpawnHealEffect();
                // 计算1%最大生命值
                int healAmount = Player.statLifeMax2 / 100;

                // 确保至少回复1点生命值
                if (healAmount < 1)
                    healAmount = 1;

                // 应用治疗效果
                Player.HealEffect(healAmount);
                Player.statLife += healAmount;

                // 防止生命值超过上限
                if (Player.statLife > Player.statLifeMax2)
                    Player.statLife = Player.statLifeMax2;
            }
        }
    }
}