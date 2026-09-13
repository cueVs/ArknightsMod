// 激光内部扰动 Shader — 能量波
// 原理：噪声灰度值 → 扰动幅度 → UV 采样偏移 → 光束内部波状扭曲
// 双路噪声：低频大浪（整体波动） + 高频细颤（表面抖动）
// 核心权重：光束中心扰动强（能量不稳定），边缘弱
// ps_3_0：sin/frac/sqrt 展开后 ps_2_0 的 64 槽放不下
// 结构照抄 LupineKnifeLight.fx（同机验证可用）：SV_POSITION + Color 直通
sampler uImage0 : register(s0);
float4x4 uTransform;
float uTime;
float uAmp;       // 扰动幅度（UV 单位，建议 0.02~0.1）
float uWaveFreq;  // 能量波频率（沿 U 的波纹密度）

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

    // 低频噪声（大波浪，沿 V 缓慢漂移）
    float n1 = frac(sin(dot(uv * 20.0 + float2(0.0, uTime * 1.5), float2(12.9898, 78.233))) * 43758.5453);
    // 高频噪声（细颤，沿 U 快速漂移）
    float n2 = frac(sin(dot(uv * 48.0 + float2(uTime * 2.3, 0.0), float2(78.233, 12.9898))) * 43758.5453);

    // 噪声灰度 → 扰动幅度（-0.5 ~ 0.5）
    float2 a = float2(n1 - 0.5, n2 - 0.5);

    // 核心权重：光束中心扰动强，边缘弱（sqrt 替代 pow(x,1.5)）
    float core = 1.0 - abs(uv.y - 0.5) * 2.0;
    core = core * sqrt(max(core, 0.0));

    // 扰动位移：大浪主导 + 细颤叠加（系数合并到一条向量乘法）
    float2 duv = float2(a.x + a.y * 0.3, a.y * 0.8 + a.x * 0.2) * (uAmp * core);

    // 扰动后采样
    float4 c = tex2D(uImage0, uv + duv);

    // 能量波亮纹：沿 U 传播，相位被噪声扰动（x*x 替代 pow(x,2)）
    float wave = sin(uv.x * uWaveFreq - uTime * 6.0 + n1 * 6.2831);
    wave = wave * 0.5 + 0.5;
    float energy = wave * wave * core;

    // 激光芯部加亮（青蓝能量，常量已合并）
    c.rgb += float3(0.24, 0.48, 0.78) * energy;
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
