sampler uImage0 : register(s0);
texture2D tex0;
sampler2D uImage1 = sampler_state
{
    Texture = <tex0>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};
float i;
// 与 IACTSW / Entelechia KScreen0 一致：MGFX 对仅 TEXCOORD0 的 ps_2_0 入口偶发编译问题，显式带上 SV_POSITION。
float4 PSFunction(float4 position : SV_POSITION, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    float4 color2 = tex2D(uImage1, coords);
    if (!any(color2))
        return color;
    else
    {
        float2 vec = float2(0, 0);
        float rot = color2.r * 6.28;
        vec = float2(cos(rot), sin(rot)) * color2.g * i;
        return tex2D(uImage0, coords + vec);
    }
}
technique Technique1
{
    pass IceAltarKScreen0
    {
        PixelShader = compile ps_2_0 PSFunction();
    }
}
