sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
matrix uWorldViewProjection;
float shouldFlip;
float4 uShaderSpecificData;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;

    return output;
}

float InverseLerp(float a, float b, float x)
{
    return saturate((x - a) / (b - a));
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float2 coords = input.TextureCoordinates;
    
    // Account for texture distortion artifacts.
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    // Read the fade map as a streak.
    float4 fadeMapColor = tex2D(uImage1, coords - float2(uTime * 0.6, 0));
    float opacity = fadeMapColor.r;
    float bloomOpacity = sin(coords.y * 3.141) * 1.4;
    return lerp(color * bloomOpacity, color * opacity, InverseLerp(0, 0.26, coords.x));
}

technique Technique1
{
    pass TrailPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}







//public Color TrailColor(float completionRatio)
//{
//    float opacity = Utils.GetLerpValue(1f, 0.5f, completionRatio, true) * Projectile.Opacity; // 让尾端透明
//    Color color = Color.Lime * opacity; // 让拖尾为亮绿色
//    color.A = (byte)(int)(Utils.GetLerpValue(0f, 0.2f, completionRatio) * 128); // 额外控制透明度
//    return color;
//}
//
//public float TrailWidth(float completionRatio)
//{
//    float widthInterpolant = Utils.GetLerpValue(0f, 0.3f, completionRatio, true) * Utils.GetLerpValue(1.0f, 0.7f, completionRatio, true);
//    return MathHelper.SmoothStep(6f, 20f, widthInterpolant); // 让拖尾起点宽，尾端窄
//}
//
//public override bool PreDraw(ref Color lightColor)
//{
//    Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
//    Vector2 drawPosition = Projectile.Center - Main.screenPosition + Vector2.UnitY * Projectile.gfxOffY;
//    Vector2 origin = texture.Size() * 0.5f;
//
//    // **进入 Shader 渲染区域**
//    Main.spriteBatch.EnterShaderRegion();
//
//    // **使用 `ArtAttack` 的 Shader**
//    GameShaders.Misc["CalamityMod:ArtAttack"].SetShaderTexture(
//        ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/FabstaffStreak")
//    );
//
//    // **应用 `ArtAttack` 的 Shader**
//    GameShaders.Misc["CalamityMod:ArtAttack"].Apply();
//
//    // **绘制拖尾**
//    PrimitiveRenderer.RenderTrail(
//        Projectile.oldPos,
//        new(TrailWidth, (completionRatio, vertexPos) => TrailColor(completionRatio), (_) = > Projectile.Size * 0.5f, shader: GameShaders.Misc["CalamityMod:ArtAttack"]),
//        180
//    );
//
//    // **退出 Shader 渲染**
//    Main.spriteBatch.ExitShaderRegion();
//
//    // **绘制弹幕本体**
//    Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation, origin, Projectile.scale, 0, 0);
//
//    return false;
//}