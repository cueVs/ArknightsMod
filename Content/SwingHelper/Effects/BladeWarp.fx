// 刀光拖尾屏幕扭曲（ScreenShaderData 滤镜）
// uImage0 = 屏幕后缓冲（Filter 系统自动绑定）
// uTime / uOpacity = ScreenShaderData.Apply 自动设置
// uCenter = 扭曲中心（归一化 UV 0-1，C# 手动 SetValue）
// 注意：屏幕滤镜 shader 不写顶点着色器（Filter 系统自己处理全屏四边形，
//       写了 VS 会导致 MatrixTransform 未设置 → 顶点塌缩 → 黑屏）
sampler uImage0 : register(s0);
float uTime;
float uOpacity;
float2 uCenter;
float uRadius;    // 影响半径（归一化 UV）
float uStrength;  // 扭曲强度（UV 偏移量）

float4 PS(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float2 uv = texCoord;

    // 原图（lerp 的基准）
    float4 original = tex2D(uImage0, uv);

    // 到中心的距离
    float2 dir = uv - uCenter;
    float dist = length(dir);

    // 半径内衰减（中心 1 → 边缘 0）
    float mask = 1.0 - smoothstep(0.0, uRadius, dist);

    // 旋转扰动：角度随时间与距离变化
    float angle = uTime * 10.0 + dist * 40.0;
    float2 rotDir = float2(
        dir.x * cos(angle) - dir.y * sin(angle),
        dir.x * sin(angle) + dir.y * cos(angle));
    float len = length(rotDir);
    rotDir = rotDir / max(len, 0.0001);

    float2 offset = rotDir * uStrength * mask;

    // 色散：红蓝通道反向偏移
    float chromatic = uStrength * 0.4 * mask;
    float r = tex2D(uImage0, uv + offset + float2(chromatic, 0.0)).r;
    float g = tex2D(uImage0, uv + offset).g;
    float b = tex2D(uImage0, uv + offset - float2(chromatic, 0.0)).b;
    float4 modified = float4(r, g, b, 1.0);

    return lerp(original, modified, uOpacity);
}

technique WarpTechnique
{
    pass Warp
    {
        PixelShader = compile ps_3_0 PS();
    }
}
