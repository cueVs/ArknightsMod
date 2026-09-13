// 可露希尔·扫描枪普通攻击「能量镖」
// 尖头长条飞行弹幕（+x 为飞行方向）：
//   - 修长匀称的纺锤形，中段最宽、向两端平滑收尖（无突变鼓包）；
//   - 主体偏红的紫色（品红/洋红系），尖端与上下边缘泛白；
//   - 中央白色细线：前 70% 为连续实线，只在尾段 30% 逐渐断续成段/点；
//   - 头部白/淡粉紫外发光，波纹向后「逃逸」扩散；
//   - 尾缘噪声破碎散开，配合 C# 端掉落的粒子形成脱离感。
// 所有元素在 UV 边界内完全淡出（边界渐隐罩），避免矩形截断痕迹。
// 输出预乘 Alpha（coverage = 最大通道），BlendState.AlphaBlend 混合。

float4x4 MatrixTransform;

float uProgress;   // 整体淡入淡出 0→1
float uIntensity;
float uTime;
float uSeed;       // 每发弹幕的随机相位

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
    float2 p = (coords - 0.5) * 2.0;   // x∈[-1,1] 左→右，+x = 飞行方向

    // ════ 调色板：偏红的紫（更靠近红色一侧）══════════════════════════════
    float3 mainCol  = float3(0.90, 0.14, 0.28); // 主体：红紫（红多于紫）
    float3 paleCol  = float3(1.00, 0.86, 0.84); // 泛白亮色（尖端/边缘/发光，偏暖）
    float3 darkCol  = float3(0.26, 0.02, 0.08); // 暗红紫（尾部压暗）
    float3 white    = float3(1.00, 1.00, 1.00);

    // ════ 边界渐隐罩：一切元素在 UV 边界前完全淡出，杜绝矩形截断 ═════════
    float borderFade = smoothstep(1.0, 0.84, abs(p.x)) * smoothstep(1.0, 0.78, abs(p.y));

    // ════ 镖体轮廓：修长匀称的纺锤形 ═════════════════════════════════════
    float tipX  = 0.84;
    float tailX = -0.84;
    float t = saturate((p.x - tailX) / (tipX - tailX));  // 0=尾 1=尖

    // 纺锤半宽：4t(1-t) 抛物线（中段最宽、两端平滑归零），略向前偏移峰值
    float ts    = pow(t, 0.85);
    float wMax  = 0.20;
    float halfW = wMax * pow(saturate(4.0 * ts * (1.0 - ts)), 0.70);

    // 尾缘破碎：仅尾段边缘轻微噪声凹陷，参差散开
    float ragged = sin(p.y * 55.0 + uSeed * 7.1 + uTime * 2.3) * 0.5
                 + sin(p.x * 33.0 - uTime * 3.1 + uSeed * 3.7) * 0.5;
    ragged = ragged * 0.5 + 0.5;
    float raggedAmt = smoothstep(0.35, 0.0, t) * 0.40;
    halfW *= 1.0 - ragged * raggedAmt;

    float inX  = step(tailX, p.x) * step(p.x, tipX);
    float body = smoothstep(halfW, halfW * 0.70, abs(p.y)) * inX;

    // 尾端渐隐（配合破碎，避免生硬截断）
    float tailFade = smoothstep(0.0, 0.14, t);
    body *= tailFade;

    // ════ 主体配色：品红填充 + 尖端/边缘泛白 ════════════════════════════
    float edgeBand = smoothstep(halfW * 0.50, halfW * 0.95, abs(p.y));
    float tipZone  = smoothstep(0.72, 1.0, t);
    float paleMix  = saturate(edgeBand * 0.70 + tipZone * 0.90);
    float3 bodyCol = lerp(mainCol, paleCol, paleMix);
    bodyCol = lerp(darkCol, bodyCol, smoothstep(0.0, 0.50, t)); // 尾段压暗

    // ════ 中央白色细线：前 70% 连续实线，尾段 30% 渐碎成段/点 ═══════════
    float lineW = 0.024 * (0.55 + 0.45 * t);              // 细线，向尾部更细
    float lineY = exp(-(p.y * p.y) / (lineW * lineW));
    float dashNoise = sin(p.x * 46.0 + uSeed * 9.3) * 0.6
                    + sin(p.x * 91.0 - uSeed * 4.2) * 0.4;
    dashNoise = dashNoise * 0.5 + 0.5;
    // t>0.30（即前 70%）阈值为 0 → 完整实线；仅尾段 30% 阈值升高 → 断续
    float dashThresh = smoothstep(0.30, 0.0, t) * 0.78;
    float dashMask   = smoothstep(dashThresh, dashThresh + 0.16, dashNoise);
    float centerLine = lineY * dashMask * inX * tailFade * smoothstep(tipX, tipX - 0.06, p.x);

    // ════ 头部外发光：白/淡粉紫，波纹向后逃逸（強度降低，让位给下方的向后拖尾） ═══
    // 注意：此处不能写 max(a,0)+max(b,0)*k 的单行形式——老 D3DX 优化器会 AccessViolation 崩溃，
    // 用 saturate 等价替代（p.x-tipX 恒 <1，saturate 与 max(...,0) 语义一致）
    float dOut = max(abs(p.y) - halfW, 0.0) + saturate(p.x - tipX) * 1.6;
    float glowBase = exp(-dOut / 0.10);
    float glowZone = (1.0 - body) * inX;
    float frontness = 0.20 + 0.80 * smoothstep(0.35, 0.95, t);
    float escape = 0.65 + 0.35 * sin(p.x * 9.0 + uTime * 5.5 + uSeed * 2.4);
    float glow = glowBase * glowZone * frontness * escape * 0.55;

    // ════ 尖端向后拖尾：从尖端出发向后（-x）流线延伸，接替一部分外发光的装饰作用 ═══
    float trailDepth = tipX - p.x;                          // 尖端往后的距离（沿 -x 越来越大）
    float trailZone  = step(0.0, trailDepth);                // 只在尖端之后
    float trailLen   = 1.35;                                 // 拖尾长度（超出镖体本身，向后延伸）
    float trailFall  = saturate(1.0 - trailDepth / trailLen);
    trailFall = trailFall * trailFall;                        // 靠近尖端最亮，向后二次衰减

    // 拖尾宽度随深度收窄，叠加流动噪声形成发散丝线质感（随 uTime 向后流动）
    float trailNoise = sin(p.y * 42.0 + uTime * 3.4 + uSeed * 5.1) * 0.5
                      + sin(trailDepth * 9.0 - uTime * 6.0 + uSeed * 2.7) * 0.5;
    trailNoise = trailNoise * 0.5 + 0.5;
    float trailWid  = lerp(halfW * 1.15, halfW * 0.35, saturate(trailDepth / trailLen));
    float trailBand = smoothstep(trailWid, trailWid * 0.55, abs(p.y));
    float trail = trailFall * trailBand * trailZone * lerp(0.55, 1.0, trailNoise) * (1.0 - body);

    // ════ 合成（全部乘边界渐隐罩）════════════════════════════════════════
    body       *= borderFade;
    centerLine *= borderFade;
    glow       *= borderFade;
    trail      *= borderFade;

    if (body <= 0.001 && glow <= 0.001 && centerLine <= 0.001 && trail <= 0.001)
        return float4(0, 0, 0, 0);

    float fade = uProgress * uIntensity;

    float3 rgb = bodyCol * body
               + white * centerLine * 0.95
               + paleCol * glow * 0.80
               + lerp(mainCol, paleCol, 0.5) * trail * 0.75;
    rgb *= fade;

    float coverage = saturate(max(rgb.r, max(rgb.g, rgb.b)));
    return float4(rgb, coverage);
}

technique Technique1
{
    pass ClosureScanShot
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 PS();
    }
}
