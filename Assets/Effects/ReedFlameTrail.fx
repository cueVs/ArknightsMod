// ============================================================
//  焰影苇草 火焰弹幕着色器（彗星型：头白热 → 黄 → 褐，整条一体）
//
//  设计要点：
//    · 头部近白高亮，外圈黄色外发光  → 宽度方向 core/edge 双色插值，
//      edge 用 uEdgeTint 压向黄橙；不靠额外 sprite
//    · 沿长度 白 → 黄 → 褐（无红）   → FlameRamp() 三段渐变
//    · 头尾一体                      → headCap 把鼻端修成圆头
//    · 尾部不规则扭曲                → 主形变在 C# 顶点侧
//
//  ⚠ 透明度稳定性（踩过的坑，改动时注意）：
//    1. 溶解**只在后半段生效**，且用 smoothstep 宽过渡带。早期版本用
//       (n - along*amount)/(1-...) 这种硬除法，因为噪声均值只有 0.5 左右、
//       而尾部阈值高达 0.82，导致大半条尾巴长期低于阈值被抹掉；噪声一滚动
//       就整片忽隐忽现。现在头段 dissolveMask 恒为 1，绝不闪。
//    2. alpha **不要再乘 taper/weight**——几何宽度已经收窄过一次，
//       再乘一遍等于衰减算两遍，尾部会过早塌成看不见。
// ============================================================

texture2D tex0;
sampler2D uNoiseTex = sampler_state
{
    Texture = <tex0>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

float4x4 uTransform;

float2 uFlowOffset;
float2 uNoiseScale;
float uDissolveAmount;  // 尾端溶解强度（只影响后半段）
float uDissolveStart;   // 从哪个 along 开始允许溶解（0.4~0.6）
float uIntensity;
float uHeadRound;       // 鼻端圆头长度占比
float uPixelWarp;       // 像素级细节抖动（很小的值，别调大）
float uTailBrightness;  // 尾部相对亮度下限（防止尾巴整个消失）

// 发光集中度控制：光晕只在头部，身体保持细而明确
float uGlowLength;      // 头部发光沿长度延伸多远（0.10~0.35）
float uHeadHeat;        // 鼻端亮度倍率（大 = 头部爆亮）
float uBodyHeat;        // 身体/尾部亮度倍率（小 = 身体不发光只是"一条"）

float3 uHeadColor;
float3 uMidColor;
float3 uTailColor;
float3 uEdgeTint;

struct VSInput
{
    float2 Pos : POSITION0;
    float4 Color : COLOR0;
    float3 Texcoord : TEXCOORD0;
};

struct PSInput
{
    float4 Pos : SV_POSITION;
    float4 Color : COLOR0;
    float3 Texcoord : TEXCOORD0;
};

PSInput VertexShaderFunction(VSInput input)
{
    PSInput output;
    output.Texcoord = input.Texcoord;
    output.Pos = mul(float4(input.Pos, 0, 1), uTransform);
    output.Color = input.Color;
    return output;
}

// 沿长度的火焰色带：白 → 黄 → 褐（刻意不经过红色）
float3 FlameRamp(float t)
{
    float3 c = lerp(uHeadColor, uMidColor, smoothstep(0.0, 0.30, t));
    c = lerp(c, uTailColor, smoothstep(0.35, 1.0, t));
    return c;
}

float4 FlamePS(PSInput input) : COLOR0
{
    float3 coord = input.Texcoord;
    float along = saturate(coord.x);      // 0 弹头 → 1 尾端
    float across = coord.y;

    // 像素级细节抖动：仅尾部、幅度很小，纯做纹理细节，不negatively影响轮廓
    float2 warpUV = float2(along * 2.6, across * 1.8) + uFlowOffset * 1.4;
    float warpN = tex2D(uNoiseTex, warpUV).r - 0.5;
    across = across + warpN * uPixelWarp * smoothstep(0.3, 1.0, along);

    float distToCenter = saturate(abs(across - 0.5) * 2.0);

    // 流动噪声
    float2 noiseUV = float2(along, across) * uNoiseScale + uFlowOffset;
    float n = tex2D(uNoiseTex, noiseUV).r;

    // 头部发光权重：只在前 uGlowLength 段内显著，二次衰减收得更紧
    float headZone = saturate(1.0 - along / max(uGlowLength, 0.001));
    float headGlow = headZone * headZone;

    // 横截面：边缘软硬沿长度变化 —— 头部软（=光晕外扩），尾部锐（=细而明确的一条线）
    // 指数小 = 衰减慢 = 边缘糊 = 发光感；指数大 = 边缘干净利落
    float edgeExp = lerp(2.7, 1.05, headGlow);
    float body = pow(saturate(1.0 - distToCenter), edgeExp);

    // 芯部：紧而亮
    float core = saturate(1.0 - distToCenter * 2.05);
    core = pow(core, 1.8);
    core = saturate(core - n * 0.14);   // 轻微噪声扰动，芯边界不死板

    // 鼻端修圆
    float headCap = 1.0;
    if (along < uHeadRound)
    {
        float2 p = float2((uHeadRound - along) / max(uHeadRound, 0.0001), distToCenter);
        headCap = saturate(1.0 - length(p));
        headCap = pow(headCap, 0.55);
    }

    // 沿长度衰减：平缓，且给尾部留一个亮度下限，避免整条后半段消失
    float lengthFade = pow(saturate(1.0 - along), 0.9);
    lengthFade = max(lengthFade, uTailBrightness * (1.0 - along * 0.55));

    // 溶解：只在 uDissolveStart 之后生效，宽过渡带，头/中段恒为 1（绝不闪烁）
    float zone = smoothstep(uDissolveStart, 1.0, along);
    float need = zone * uDissolveAmount;
    float dissolveMask = smoothstep(need - 0.30, need + 0.18, n);
    dissolveMask = lerp(1.0, dissolveMask, zone);

    // 颜色
    float3 rampCol = FlameRamp(along);
    float3 edgeCol = rampCol * uEdgeTint;                  // 外圈黄橙外发光
    float3 col = lerp(edgeCol, rampCol, core);
    // 白热芯只出现在头部（跟着 headGlow 收），身体不再泛白
    col = lerp(col, float3(1.0, 0.98, 0.93), saturate(core * core * headGlow * 1.35));
    col *= lerp(0.82, 1.15, n);                            // 噪声调制明暗（幅度收窄，避免忽明忽暗）

    // 亮度：头部爆亮，身体维持较低基准 —— 这是"发光集中在头部"的主要来源
    float heat = lerp(uBodyHeat, uHeadHeat, headGlow);

    float alpha = body * lengthFade * dissolveMask * headCap;
    alpha *= input.Color.a;

    return float4(col * input.Color.rgb * alpha * uIntensity * heat, alpha);
}

technique Technique1
{
    pass FlameTrail
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 FlamePS();
    }
}
