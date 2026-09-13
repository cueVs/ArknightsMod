// 水墨刀光 Shader — 蔓延式墨染
// 从 uSpreadPos 一点向整条刀光蔓延：半径内的像素染墨，
// 蔓延前沿聚集深墨环，内部随 uDry 干涸变淡（水晒干）
// ps_3_0：指令槽上限 512
// 结构照抄 LupineKnifeLight.fx（SV_POSITION + Color 直通）
sampler uImage0 : register(s0);
float4x4 uTransform;
float uTime;
float2 uSpreadPos;    // 墨滴中心（UV，例：刀身最前端 (0, 0.5)）
float uSpreadRadius;  // 当前蔓延半径 0~1.5，C# 每帧涨大
float uDry;           // 干涸进度 0-1：内部墨迹随时间变淡
float uWash;          // 晕染强度
float3 uInkColor;     // 墨色（深墨绿）
float3 uWashColor;    // 淡染色（浅绿白）
float3 uAccentColor;  // 强调色

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
    float4 c = tex2D(uImage0, uv);

    // ---- 蔓延场：每个像素算自己到墨滴中心的距离 ----
    float dist = length(uv - uSpreadPos);
    float spread = 1.0 - smoothstep(uSpreadRadius - 0.15, uSpreadRadius, dist);  // 半径内 = 1

    // 墨迹纹理（枯笔触），只在蔓延区内显现
    float noise = frac(sin(dot(uv * 25.0 + uTime * 0.4, float2(78.233, 12.9898))) * 43758.5453);
    float streak = sin(uv.x * 24.0 - uTime * 3.0 + noise * 6.2831) * 0.5 + 0.5;
    streak = streak * streak * streak * step(0.35, noise);

    // 墨色：中心深墨 → 边缘淡染（按刀宽 + 噪声扰动）
    float edgeDist = abs(uv.y - 0.5) * 2.0;
    float wash = smoothstep(0.4, 1.0, edgeDist + (noise - 0.5) * 0.3 * uWash);
    float3 ink = lerp(uInkColor, uWashColor, wash);

    // 干涸：越靠墨滴中心越早干（内部先变淡，像水渍向内收缩）
    float inner = 1.0 - smoothstep(uSpreadRadius * 0.5, 0.0, dist);
    float dry = 1.0 - uDry * inner;

    // 蔓延前沿：深墨环（墨往边缘聚集）
    float front = 1.0 - smoothstep(0.0, 0.1, abs(dist - uSpreadRadius));

    float3 color = c.rgb;
    color = lerp(color, ink, spread * dry);                 // 蔓延区内染墨
    color += ink * streak * 0.4 * spread * dry;             // 枯笔纹理
    color += uInkColor * front * 1.5;                       // 前沿深墨环
    color += float3(0.08, 0.22, 0.18) * spread * 0.4;       // 墨绿底色
    color += uAccentColor * front * spread * 0.3;           // 前沿一点朱砂
    color *= 1.3;                                           // 整体提亮

    // 尾部淡出（U 0=新 1=旧）
    color *= 1.0 - smoothstep(0.3, 1.0, uv.x);

    return float4(color, 1.0) * input.Color;
}

technique Technique1
{
    pass Base
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}
