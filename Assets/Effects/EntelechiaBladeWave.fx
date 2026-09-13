// 隐德来希·普通攻击「刀光」
// 月牙形厚环弧段：中段最宽，向两端连续收尖；轮廓描边式红色辉光；
// 凹处（开口侧，-x 方向）叠加严格水平、近粗远细、密集独立的深浅红色带状拖尾。
//
// 关键几何（经 DEBUG 验证）：
//   月牙本体在 +x 半侧（飞行方向），凹口朝 -x（飞行反方向）。
//   内缘在某高度 py 的 x 坐标 = sqrt(rEdge² - py²)（恒为正），拖尾从此处朝 -x 延伸。

float4x4 MatrixTransform;

float3 uColor;          // 辉光/拖尾颜色（红）
float3 uSecondaryColor; // 核心颜色（白）
float  uProgress;       // 白色核心淡入淡出进度（先出现、后消失）
float  uRedProgress;    // 红色辉光/拖尾淡入淡出进度（后出现、先消失）
float  uIntensity;
float  uTime;

static const float PI = 3.14159265358979323846;

struct VS_IN  { float4 Position : POSITION; float2 TexCoord : TEXCOORD0; };
struct VS_OUT { float4 Position : POSITION; float2 TexCoord : TEXCOORD0; };

VS_OUT VS(VS_IN input) {
    VS_OUT o;
    o.Position = mul(input.Position, MatrixTransform);
    o.TexCoord = input.TexCoord;
    return o;
}

float4 PS(float2 coords : TEXCOORD0) : COLOR0
{
    float2 p    = (coords - 0.5) * 2.0;
    float  dist = length(p);
    float  ang  = atan2(p.y, p.x);

    // ════ 月牙本体 ════════════════════════════════════════════════════
    float baseInnerR = 0.58;
    float baseOuterR = 0.72;
    float midR       = (baseInnerR + baseOuterR) * 0.5;

    float arcOpen  = 1.65;            // 单侧半开口，总缺口 ≈ 189°
    float tipAngle = PI - arcOpen;

    // 当前像素角度对应的收窄系数（仅本体/描边发光使用，不影响拖尾）
    float angNorm = saturate(abs(ang) / tipAngle);
    float taper   = pow(cos(angNorm * 1.5707963), 0.7);
    float crescentOn = smoothstep(0.0, 0.05, taper); // taper→0 时本体消失（不再 discard 整个像素）

    float innerR = lerp(midR, baseInnerR, taper);
    float outerR = lerp(midR, baseOuterR, taper);

    // 内缘过渡加宽，让白色到红色的渐变更柔、内边缘更模糊
    float maskInner = saturate((dist - innerR) / 0.090);
    float maskOuter = saturate((outerR - dist) / 0.030);
    float ringMask  = min(maskInner, maskOuter) * crescentOn;

    // 轮廓描边发光
    float glowWidth = 0.06;
    float outerGlow = exp(-max(dist - outerR, 0.0) / glowWidth) * (1.0 - maskOuter);
    float innerGlow = exp(-max(innerR - dist, 0.0) / glowWidth) * (1.0 - maskInner);
    float totalGlow = (outerGlow + innerGlow) * taper * crescentOn;

    // ════ 两种红色（交织拖尾用）════════════════════════════════════════
    float3 darkRed   = float3(0.110, 0.016, 0.020); // #1c0405
    float3 brightRed = float3(0.706, 0.137, 0.176); // #b4232d

    // 月牙上/下尖端坐标（细线拖尾与底层修饰拖尾的范围都用到）
    float tipY = midR * sin(tipAngle);   // 尖端高度 ≈ ±0.65
    float tipX = midR * cos(tipAngle);   // 尖端 x ≈ 0.05

    // 更深的暗红打底色
    float3 deepRed = float3(0.050, 0.006, 0.008);

    // ════ 凹处水平拖尾（独立于当前像素角度/taper）════════════════════════
    // 1. 当前高度 py 处内缘的 x 坐标（恒为正，月牙本体在 +x 侧）；
    //    rEdge 取得较大，使拖尾根部深入白色本体内缘的红裙区，红色连续过渡、无界限
    float rEdge = 0.66;
    float edgeX = sqrt(max(rEdge * rEdge - p.y * p.y, 0.0));

    // 2. 拖尾从内缘朝 -x 延伸：当前像素在内缘左侧的水平距离
    float trailDepth = edgeX - p.x;                 // >0 = 在内缘左侧（凹口/后方）
    float inZone     = step(0.0, trailDepth);

    // 3. 垂直范围限制 + 上下尖端淡出
    float vSpan   = rEdge * 0.98;
    float endFade = smoothstep(0.0, 1.0, saturate((vSpan - abs(p.y)) / (rEdge * 0.5)));

    // 4. 连续红色根部：深入本体内缘红裙、沿 y 不留缝隙，白→红 无缝衔接
    float root = smoothstep(0.20, 0.0, trailDepth);

    // 5. 多倍频噪声场 → 密集、细如发丝、富纹理的拖尾（深/浅红相位错开互相交织）
    //    叠三层不同频率（粗丝 + 中丝 + 细纹），再 1-sqrt(1-v) 锐化成细丝
    float dNoise = sin(p.y * 38.0  + uTime * 1.4) * 0.45
                 + sin(p.y * 73.0  - uTime * 1.0) * 0.33
                 + sin(p.y * 142.0 + uTime * 2.1) * 0.22;
    dNoise = saturate(dNoise * 0.5 + 0.5);
    dNoise = 1.0 - sqrt(1.0 - dNoise);

    float bNoise = sin(p.y * 38.0  + uTime * 1.4 + 2.1) * 0.45
                 + sin(p.y * 73.0  - uTime * 1.0 + 1.3) * 0.33
                 + sin(p.y * 142.0 + uTime * 2.1 + 0.7) * 0.22;
    bNoise = saturate(bNoise * 0.5 + 0.5);
    bNoise = 1.0 - sqrt(1.0 - bNoise);

    // 逐丝长度变化（低频）：不同高度的丝伸出长短不一，呈扇形参差
    // 逐丝长度差距加大（min↓、max↑），并整体偏长（pow 把分布往长端拉）
    float dLenVar = sin(p.y * 7.0  + 0.5) * 0.55 + sin(p.y * 13.0 - 0.9) * 0.30
                  + sin(p.y * 23.0 + 1.7) * 0.15;
    dLenVar = saturate(dLenVar * 0.5 + 0.5);
    float dLen = lerp(0.32, 1.55, pow(dLenVar, 0.80));

    float bLenVar = sin(p.y * 7.0  + 2.7) * 0.55 + sin(p.y * 13.0 - 0.2) * 0.30
                  + sin(p.y * 23.0 + 0.4) * 0.15;
    bLenVar = saturate(bLenVar * 0.5 + 0.5);
    float bLen = lerp(0.22, 0.95, pow(bLenVar, 0.85));

    // 6. 沿长度抬高阈值 → 丝在 y 向收窄、末端散开；侧边用较宽 smoothstep 做柔和模糊边界
    float edgeSoft = 0.18;

    float dThresh = lerp(0.0,  0.96, saturate(trailDepth / dLen));
    float dBand   = smoothstep(dThresh, dThresh + edgeSoft, dNoise);

    float bThresh = lerp(0.22, 0.96, saturate(trailDepth / bLen));
    float bBand   = smoothstep(bThresh, bThresh + edgeSoft, bNoise);

    float dMask = dBand * inZone * endFade;                 // 深红细丝层
    float bMask = max(bBand, root) * inZone * endFade;      // 浅红细丝 + 连续根部

    // 7. 更深红打底：满覆盖、沿长度淡出、后端矩形截断（纯修饰底色，填满内侧不留空）
    float baseLen  = 0.88;
    float baseFade = saturate(1.0 - trailDepth / baseLen);
    float baseMask = inZone * step(trailDepth, baseLen) * step(abs(p.y), tipY) * baseFade;

    // 颜色层次：整体降亮，保留深浅红区分、减少 additive 过曝带来的塑料/泛白感
    float3 trail = deepRed * baseMask * 1.3
                 + darkRed * dMask * 1.35
                 + brightRed * bMask * 0.85;
    float trailAny = dMask + bMask + baseMask;

    // 8. 月牙两端尖端各拖出一条独立细线，根部探入本体闭合缝隙，尾部渐隐淡出
    float dyTip   = min(abs(p.y - tipY), abs(p.y + tipY));
    float tipStart = tipX + 0.12;        // 起点探入本体，紧贴月牙端点、无缝隙
    float tipDep  = tipStart - p.x;      // 朝 -x 延伸
    // 头部从端点处即开始平滑淡入（消除圆点，又不留缝隙）
    float tipHeadFade = smoothstep(0.0, 0.12, tipDep);
    float tipLen  = 0.62;        // 缩短两侧条状拖尾尾部长度
    float tipT    = saturate(tipDep / tipLen);
    float tipWid  = 0.018 * (1.0 - tipT * 0.55);         // 越往后越细
    float tipLineY = exp(-(dyTip * dyTip) / (tipWid * tipWid));
    float tipFade = smoothstep(1.0, 0.0, tipT) * tipHeadFade;
    float tipMask = tipLineY * tipFade;
    trail += brightRed * tipMask * 0.7;
    trailAny += tipMask;

    // ════ 合成与早退 ════════════════════════════════════════════════════
    if (ringMask <= 0.001 && totalGlow <= 0.001 && trailAny <= 0.001)
        return float4(0, 0, 0, 0);

    // 白色核心：内缘一侧较宽距离模糊地渐变到红色（白→红 模糊内边缘）
    float bandPos   = saturate((dist - innerR) / (outerR - innerR)); // 0=内缘 1=外缘
    float innerTint = smoothstep(0.50, 0.0, bandPos);                 // 内缘处=1，向外 50% 归 0
    float3 coreCol  = lerp(uSecondaryColor, brightRed, innerTint);
    float3 core = coreCol * ringMask;

    float3 glow = uColor * totalGlow * 1.1;

    // 白色核心 + 红色外发光描边一起走 uProgress（先出现、最后一起消失，避免白色突兀裸露）；
    // 仅凹侧拖尾走 uRedProgress（后出现、先消失）
    float wfade = uProgress    * uIntensity;
    float rfade = uRedProgress * uIntensity;
    float3 rgb  = (core + glow) * wfade + trail * rfade;

    // 预乘 Alpha 输出：coverage = 最大通道值。
    //  - 弱处(coverage 小) → 近似加性，保留夜间柔和辉光；
    //  - 强处(coverage→1) → 不透明覆盖背景，白天/亮背景下也能显示本色（拖尾不再被冲淡）。
    float coverage = saturate(max(rgb.r, max(rgb.g, rgb.b)));
    return float4(rgb, coverage); // 预乘 Alpha 混合（BlendState.AlphaBlend）
}

technique Technique1
{
    pass EntelechiaBladeWave
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 PS();
    }
}
