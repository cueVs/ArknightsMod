// 隐德来希·刀光「拖尾」屏幕空间折射扭曲
// 复用 SchwarzDistortion 的抓屏管线：uImage0 = 当前屏幕快照（s0），coords = 屏幕 UV。
// 在月牙凹侧（拖尾区域）沿拖尾方向(-x local)与纹路横向起伏偏移采样，使周围画面被拖尾扭曲。

sampler uImage0 : register(s0);

float2 uRes;        // 屏幕分辨率（像素）
float2 uCenter;     // 刀光中心屏幕 UV [0,1]
float2 uDir;        // 刀光本地 +x 在屏幕空间的单位向量（含视图旋转/缩放方向）
float  uHalfPx;     // 刀光包围盒半边长（屏幕像素，含 zoom）
float  uIntensity;  // 扭曲强度（占 halfPx 的比例）
float  uTime;

float4 PS(float2 coords : TEXCOORD0) : COLOR0
{
    // 屏幕像素 → 刀光本地坐标 q（与可视 shader 的 p 同空间，[-1,1]）
    float2 d   = coords * uRes - uCenter * uRes;
    float2 dir = uDir;
    float2 perp = float2(-dir.y, dir.x);
    float2 q = float2(dot(d, dir), dot(d, perp)) / uHalfPx;

    // 拖尾区域几何（与可视 shader 一致）
    float rEdge = 0.66;
    float edgeX = sqrt(max(rEdge * rEdge - q.y * q.y, 0.0));
    float depth = edgeX - q.x;
    float inZone = step(0.0, depth);

    float vSpan   = rEdge * 0.98;
    float endFade = smoothstep(0.0, 1.0, saturate((vSpan - abs(q.y)) / (rEdge * 0.5)));

    // 与可视拖尾相同频率的纹路噪声 → 扭曲沿拖尾纹路
    float n = sin(q.y * 38.0  + uTime * 1.4) * 0.45
            + sin(q.y * 73.0  - uTime * 1.0) * 0.33
            + sin(q.y * 142.0 + uTime * 2.1) * 0.22;
    n = saturate(n * 0.5 + 0.5);

    float lenFall = saturate(1.0 - depth / 1.25);   // 越往后扭曲越弱
    float mask = inZone * endFade * lenFall;
    if (mask <= 0.001)
        return float4(0, 0, 0, 0);

    // 偏移：主沿拖尾方向(-x local)，并沿纹路横向起伏
    float warp = (n - 0.5) * 2.0;
    float2 localOff = float2(-(0.5 + 0.5 * n), warp * 0.45);
    localOff *= mask * uIntensity;

    // 本地偏移 → 屏幕方向（像素）→ UV
    float2 offPx = dir * (localOff.x * uHalfPx) + perp * (localOff.y * uHalfPx);
    float2 offUV = offPx / uRes;

    float4 col = tex2D(uImage0, coords + offUV);
    float alpha = saturate(mask * 1.2);
    return float4(col.rgb * alpha, alpha); // premultiplied（AlphaBlend）
}

technique Technique1
{
    pass EntelechiaBladeWaveDistort
    {
        PixelShader = compile ps_3_0 PS();
    }
}
