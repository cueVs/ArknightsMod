using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using Terraria;
using ArknightsMod.Content.Events;

namespace ArknightsMod.Assets.Effects
{
    public class UnionInvadeSky : CustomSky
    {
        private bool _isActive;
        private float _intensity;
        private Texture2D _skyTexture;
        private Vector2 playerWorldPos;

        public override string ToString() => "ArknightsMod:UnionInvadeSky";

        public override void Activate(Vector2 position, params object[] args)
        {
            _isActive = true;
            _skyTexture = ArknightsMod.UnionInvadeSkyTexture; // 使用预加载纹理
            if (args?.Length > 0 && args[0] is float initIntensity)
                _intensity = initIntensity;
        }

        public override void Deactivate(params object[] args) => _isActive = false;

        public override void Update(GameTime gameTime)
        {
            Vector2 worldCenter = new Vector2(Main.maxTilesX * 8f, Main.maxTilesY * 4f);
            playerWorldPos = Main.screenPosition - worldCenter;

            // 建议添加事件状态检测（仿照灾厄）
            bool shouldActive = UnionInvade.EventActive && !Main.gameMenu;

            float target = shouldActive ? 1f : 0f;
            _intensity = MathHelper.Clamp(_intensity + (target > _intensity ? 0.02f : -0.02f), 0f, 1f);
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            var destRect = new Rectangle(
    -(int)(playerWorldPos.X / (Main.maxTilesX * 20f) * Main.screenWidth),
    -730 - (int)(playerWorldPos.Y / (Main.maxTilesY * 7f) * Main.screenHeight),
    (int)(1.6f * Main.screenWidth),
    (int)(1.6f * Main.screenHeight)

);
            destRect.X -= (int)(destRect.Width * 0.15f);
            if (_intensity > 0.01f && maxDepth >= 0 && minDepth < 0)
            {
                spriteBatch.Draw(
     _skyTexture,
     destRect,
     Color.White * _intensity
 );
                //spriteBatch.Draw(
                //_skyTexture ??= ArknightsMod.UnionInvadeSkyTexture, // 安全后备
                //new Rectangle((int)(playerWorldPos.X / Main.maxTilesX * 8f) * Main.screenWidth, -100 + (int)(playerWorldPos.Y / Main.maxTilesY * 4f) * Main.screenHeight, (int)2f * Main.screenWidth, (int)2f * Main.screenHeight),
                // Color.White * _intensity
                // ); ,

            }
        }

        public override void Reset() => _intensity = 0f;
        public override bool IsActive() => _intensity > 0.01f;
        public override float GetCloudAlpha() => 1f - _intensity;
    }
}