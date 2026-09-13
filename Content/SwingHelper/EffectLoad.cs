using ArknightsMod.Content.Projectiles.Defender;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace ArknightsMod.Content.SwingHelper
{
    public class EffectLoad : ModSystem
    {
        public const string index = "ArknightsMod/Content/SwingHelper/Effects/";
        public override void Load()
        {
	        SwingHelper.Flow = ModContent.Request<Effect>(index+"BladeFlow",AssetRequestMode.ImmediateLoad).Value;
	        SwingHelper.Dissolve = ModContent.Request<Effect>(index+"BladeDissolve",AssetRequestMode.ImmediateLoad).Value;
	        SwingHelper.BladeFlicker = ModContent.Request<Effect>(index+"BladeFlicker",AssetRequestMode.ImmediateLoad).Value;
	        SwingHelper.BladeInk = ModContent.Request<Effect>(index+"BladeInk",AssetRequestMode.ImmediateLoad).Value;
	        SwingHelper.BladeWarp = ModContent.Request<Effect>(index+"BladeWarp",AssetRequestMode.ImmediateLoad);
	        SwingHelper.BladeSlashWarp = ModContent.Request<Effect>(index+"BladeSlashWarp",AssetRequestMode.ImmediateLoad);
			SwingHelper.Pixelate = ModContent.Request<Effect>(index+"BladePixelate",AssetRequestMode.ImmediateLoad).Value;
			SwingHelper.NoiseTrail = ModContent.Request<Effect>(index+"BladeNoiseTrail",AssetRequestMode.ImmediateLoad).Value;
	        Defender_Player.shieldFx = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/ShieldDissolve",
				AssetRequestMode.ImmediateLoad).Value;
	        if (Main.netMode != Terraria.ID.NetmodeID.Server)
	        {
		        Filters.Scene["BladeWarp"] = new Filter(
			        new ScreenShaderData(SwingHelper.BladeWarp, "Warp"), EffectPriority.VeryHigh);
		        Filters.Scene["BladeWarp"].Load();
		        Filters.Scene["BladeSlashWarp"] = new Filter(
			        new ScreenShaderData(SwingHelper.BladeSlashWarp, "SlashWarp"), EffectPriority.VeryHigh);
		        Filters.Scene["BladeSlashWarp"].Load();
	        }
        }
    }
}

