using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace ArknightsMod.Content.Projectiles.Caster.Ebenholz;

// 复用模组已有的径向折射 shader，独立滤镜键；每帧只选一个近处核心，避免叠加全屏采样。
public sealed class EbenholzLensSystem : ModSystem
{
    private const string Key = "ArknightsMod:EbenholzLens";
    private bool loaded;
    public override void Load()
    {
        if (Main.dedServ) return;
        var effect = ModContent.Request<Effect>("ArknightsMod/Assets/Effects/IACTSW", AssetRequestMode.ImmediateLoad);
        Filters.Scene[Key] = new Filter(new ScreenShaderData(effect, "IACTSW"), EffectPriority.High);
        Filters.Scene[Key].Load();
        loaded = true;
    }
    public override void PostUpdateEverything()
    {
        if (!loaded || Main.dedServ) return;
        Projectile nearest = null;
        float best = 850f * 850f;
        int type = ModContent.ProjectileType<EbenholzRemnant>();
        if (!Main.gameMenu)
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.type != type || p.ai[0] != 1f) continue;
                float distance = Vector2.DistanceSquared(p.Center, Main.screenPosition + new Vector2(Main.screenWidth, Main.screenHeight) * .5f);
                if (distance >= best) continue;
                best = distance;
                nearest = p;
            }
        if (nearest == null)
        {
            if (Filters.Scene[Key].IsActive()) Filters.Scene[Key].Deactivate();
            return;
        }
        float t = MathHelper.Clamp(nearest.ai[1] / 60f, 0f, 1f);
        float zoom = MathHelper.Clamp(Main.GameViewMatrix.Zoom.Y, .25f, 4f);
        float radius = MathHelper.Lerp(145f, 50f, t) * zoom;
        float height = Math.Max(1, Main.screenHeight);
        float frequency = 3f * height * height / (radius * radius);
        if (!Filters.Scene[Key].IsActive()) Filters.Scene.Activate(Key, nearest.Center);
        Filters.Scene[Key].GetShader().UseTargetPosition(nearest.Center).UseColor(.7f, frequency, MathHelper.Pi * 3f)
            .UseProgress(1f).UseOpacity(45f * MathF.Sin(t * MathHelper.Pi));
    }
    public override void OnWorldUnload()
    {
        if (loaded && Filters.Scene[Key].IsActive()) Filters.Scene[Key].Deactivate();
    }
    public override void Unload()
    {
        OnWorldUnload();
        loaded = false;
        Texture2D old = EbenholzVisuals.Disc;
        EbenholzVisuals.Disc = null;
        if (old != null) Main.QueueMainThreadAction(old.Dispose);
    }
}
