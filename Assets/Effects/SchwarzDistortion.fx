// 屏幕空间扭曲 shader，用于 SchwarzArrow 飞行轨迹
// progress=0 = 箭矢头部（被贴图遮挡），progress=1 = 尾部（玩家可见区域）
// 效果强度从头部 0 向尾部递增，让可见的尾迹产生明显折射感

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

// uniform 槽（与 SchwarzArrow.DrawDistortionTrail 中 UV 空间一致）：
//   uTargetPosition : 段起点屏幕 UV [0,1]
//   uDirection      : 段向量（uvEnd - uvStart）
//   uImageSize1.x   : 段长度（UV）
//   uImageSize1.y   : 轨迹半宽（UV，按 snapW 归一化）
//   uColor.xy       : progress 起止 (0=头, 1=尾)
//   uIntensity      : 扭曲强度
//   uTime           : 动画时间

float4 SchwarzDistortion(float2 coords : TEXCOORD0) : COLOR0
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

    float progress  = lerp(uColor.x, uColor.y, projT / segLen);
    float crossDist = saturate(abs(crossT) / halfW);

    // 段端 / 横向 / 长度方向淡入淡出
    float tNorm     = projT / segLen;
    float endFade   = smoothstep(0.0, 0.15, tNorm) * smoothstep(1.0, 0.85, tNorm);
    float edgeFade  = 1.0 - smoothstep(0.0, 1.0, crossDist);   // 1=轴心, 0=边缘
    // 头部已不再"完全压掉"，让箭身两侧也能看到扭曲
    // 仅保留温和的"尾部稍强"梯度，避免最末端突兀消失
    float trailFade = lerp(0.75, 1.0, sqrt(saturate(progress)));

    // 引力透镜：offset 沿 perp 方向、指向远离轴的一侧。
    //   采样点远离轴 → 画面看起来被吸向轴 → "空间被拉紧"
    //   pullMag 在轴附近最强，向边缘平滑衰减；曲线变陡使凹陷更锐利
    float pullMag   = pow(1.0 - crossDist, 1.8);
    float pulse     = 0.85 + 0.20 * sin(progress * 4.0 + uTime * 2.5);
    float crossSign = (crossT >= 0.0) ? 1.0 : -1.0;
    float2 offset   = perp * crossSign * pullMag * pulse * uIntensity * trailFade * endFade;

    // 采样原画面（保持原色，无 tint）
    float4 col = tex2D(uImage0, coords + offset);

    // alpha：轴心几乎不透明、向边缘溶解，两端淡入淡出
    float alpha = trailFade * endFade * edgeFade;
    alpha = saturate(alpha * 1.25);

    // SpriteBatch AlphaBlend = premultiplied，rgb 必须乘 alpha
    return float4(col.rgb * alpha, alpha);
}

technique Technique1
{
    pass SchwarzDistortion
    {
        PixelShader = compile ps_3_0 SchwarzDistortion();
    }
}
