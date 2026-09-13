// 狼之绯刀光的屏幕扭曲——安全捕获模式（参照 SchwarzArrow/SchwarzDistortionSystem 的
// "PreDraw 内按需抓 Main.screenTarget 快照，不挂任何渲染管线 hook" 做法，逐段包围盒重绘）。
//
// 和 SchwarzDistortion.fx 的关键区别：SchwarzDistortion 是纯解析的"向轴心拉伸"位移场；
// 这里的偏移量改成**从与可见刀光同一张噪声贴图里取**——满足"扭曲路径跟随颜色纹理"的要求，
// 而不是一个和视觉纹理无关的独立扭曲形状。
//
// 偏移方向用有限差分从噪声高度场里求一个近似梯度，再取梯度的垂直方向（沿"等高线"流动，
// 而不是径直冲向噪声峰值）——这样扭曲看起来像液体绕着纹理的亮暗结构打旋，而不是简单地
// 被吸向某一点。

sampler uImage0 : register(s0); // 屏幕快照（Main.screenTarget 的复制帧）
texture2D tex1;                  // 噪声贴图，和可见刀光的 Flow pass 共用同一张
sampler2D uNoiseTex = sampler_state
{
    Texture = <tex1>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

float2 uScreenResolution;
float2 uTargetPosition;   // 段起点屏幕UV（与 C# 端 DrawScreenDistortion 里的 uv 空间一致）
float2 uDirection;        // 段向量（uv 空间，终点-起点）
float2 uImageSize1;       // x = 段长度(uv)  y = 半宽(uv)
float2 uFlowOffset;       // 和可见层 Flow pass 完全相同的滚动偏移量
float2 uNoiseScale;       // 和可见层相同的噪声平铺密度
float uIntensity;         // 扭曲强度（像素）

float4 LupineSlashDistortion(float2 coords : TEXCOORD0) : COLOR0
{
    float2 start  = uTargetPosition;
    float  segLen = uImageSize1.x;
    float  halfW  = uImageSize1.y;
    if (segLen < 0.00001)
        return float4(0, 0, 0, 0);

    float2 dir  = uDirection / segLen;
    float2 perp = float2(-dir.y, dir.x);

    float2 toFrag = coords - start;
    float  projT  = dot(toFrag, dir);
    float  crossT = dot(toFrag, perp);
    if (projT < 0.0 || projT > segLen || abs(crossT) > halfW)
        return float4(0, 0, 0, 0);

    float u = projT / segLen;              // 沿刀光长度方向 0..1
    float v = 0.5 + 0.5 * (crossT / halfW); // 横跨宽度方向 0..1

    // 与可见层同一张噪声、同一套滚动偏移——扭曲的形状天然贴合颜色纹理的流动
    float2 noiseUV = float2(u, v) * uNoiseScale + uFlowOffset;
    float eps = 0.015;
    float h0 = tex2D(uNoiseTex, noiseUV).r;
    float hx = tex2D(uNoiseTex, noiseUV + float2(eps, 0)).r;
    float hy = tex2D(uNoiseTex, noiseUV + float2(0, eps)).r;
    float2 grad = float2(hx - h0, hy - h0) / eps;
    float2 flowDir = float2(-grad.y, grad.x); // 梯度的垂直方向：沿噪声"等高线"打旋

    float edgeFade = 1.0 - smoothstep(0.55, 1.0, abs(crossT) / halfW);
    float endFade  = smoothstep(0.0, 0.12, u) * smoothstep(1.0, 0.7, u);

    float2 offsetPx = flowDir * uIntensity * edgeFade * endFade;
    float2 offsetUV = offsetPx / uScreenResolution;

    float4 col = tex2D(uImage0, coords + offsetUV);
    return col;
}

technique Technique1
{
    pass LupineSlashDistortion
    {
        PixelShader = compile ps_3_0 LupineSlashDistortion();
    }
}
