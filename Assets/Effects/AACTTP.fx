sampler uImage0 : register(s0); // The contents of the screen.
sampler uImage1 : register(s1); // Up to three extra textures you can use for various purposes (for instance as an overlay).
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition; // The position of the camera.
float2 uTargetPosition; // The "target" of the shader, what this actually means tends to vary per shader.
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
float4 uSourceRect; // Doesn't seem to be used, but included for parity.
float2 uZoom;

float4 AACTTP(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    if (!any(color))
        return color;
    // pos 就是中心了
    float2 pos = (uTargetPosition - uScreenPosition) / uScreenResolution;
    //float2 pos = float2(0.5, 0.5);
    // offset 是中心到当前点的向量
    float2 offset = (coords - pos);
    // 因为长宽比不同进行修正
    float2 rpos = offset * float2(uScreenResolution.x / uScreenResolution.y * 0.75, 1);
    float dis = length(rpos);
    // 在屏幕变长k（小于等于1）占比内启用，这里范围改成椭圆了
    //下面return那里的公式
    /*  n > 1
        * min = 球形放大
        * max = 正常缩小（外延为线性条带）
        / min = 全屏隧道
        / max = 正常放大

        n < 1
        * min = 球形放大
        * max = 球形放大
        / min = 球形隧道（外延为线性条带）
        / max = 球形缩小（中延为线性条带， 外延为隧道）*/
    if (dis <= uColor.x)
    {
        return tex2D(uImage0, pos + offset / max(uColor.y, dis));
    }
    //这几行启用后有点割裂，不太好看
    //else if (dis > 0.1 && dis <= 0.2)
    //{
    //    return tex2D(uImage0, pos + offset / max(0.1, dis));
    //}
    else
    {
        return tex2D(uImage0, pos + offset);
    }
}

technique Technique1
{
    pass AACTTP
    {
        PixelShader = compile ps_2_0 AACTTP();
    }
}