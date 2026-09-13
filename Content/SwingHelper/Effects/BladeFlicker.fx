// 刀光赛博闪烁 Shader — 色块水平偏移（RGB 分离 glitch）
// 按行分块随机触发，红蓝通道反向水平错位 = 赛博 bug 视觉
// ps_3_0：指令槽 512（双噪声 + 三次采样，ps_2_0 的 64 槽放不下）
// 结构照抄 LupineKnifeLight.fx（SV_POSITION + Color 直通）
sampler uImage0 : register(s0);
float4x4 uTransform;
float uTime;
float uIntensity;  // 0-1

struct VSInput
{
    float2 Pos : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Pos : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

VSOutput VS(VSInput input)
{
    VSOutput o;
    o.Pos = mul(float4(input.Pos, 0, 1), uTransform);
    o.Color = input.Color;
    o.TexCoord = input.TexCoord;
    return o;
}

float4 PS(VSOutput input) : COLOR0
{
    float2 uv = input.TexCoord;

    // 按行分块：把刀光切成 24 行，时间片让 glitch 行随时间滚动变化
    float2 cell = float2(floor(uv.y * 24.0), floor(uTime * 8.0));
    float rowNoise = frac(sin(dot(cell, float2(12.9898, 78.233))) * 43758.5453);
    float glitch = step(0.7, rowNoise);   // 30% 的行触发

    // 每行随机偏移量（带方向）
    float shiftNoise = frac(sin(dot(cell + 1.0, float2(78.233, 12.9898))) * 43758.5453);
    float shift = (shiftNoise - 0.5) * 0.25 * uIntensity * glitch;

    // 三通道不同偏移 → 色块错位
    float r = tex2D(uImage0, float2(uv.x + shift, uv.y)).r;
    float g = tex2D(uImage0, uv).g;
    float b = tex2D(uImage0, float2(uv.x - shift * 0.6, uv.y)).b;
    float4 c = float4(r, g, b, 1.0);

    // 高频明暗脉冲
    float flick = 0.75 + 0.25 * sin(uTime * 30.0 + uv.x * 10.0);
    c.rgb *= lerp(1.0, flick, uIntensity * 0.6);

    c.a = 1.0;
    return c * input.Color;
}

technique Technique1
{
    pass Base
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}
