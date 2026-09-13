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

float4 FNTwistedRing(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    if (!any(color))
        return color;
    float PI = 3.1415926535897932384626433832795028841971f;
    float3 targetColor = uColor; //光环颜色
    float ringRad = 8 * uOpacity / uScreenResolution.x; //光环粗细
    float2 pos = (uTargetPosition - uScreenPosition) / uScreenResolution; //中心位置
    float2 offset = (coords - pos); //中心点到当前位置的向量
    float2 rpos = offset * float2(uScreenResolution.x / uScreenResolution.y, 1); //修正向量
    float rad = acos(dot(rpos, float2(1, 0)) / length(rpos)); //夹角
    if (rpos.y < 0)
    {
        rad = 2 * PI - rad;
    }
    float dis = length(rpos); //到中心点的距离
    float targetRad = 1024 * uProgress / uScreenResolution.x + 0.00125 * sin(19 * rad + 17 * uTime) - 0.000875 * cos(29 * rad - 23 * uTime) + 0.000625 * sin(37 * rad - 31 * uTime) - 0.0005 * sin(43 * rad + 41 * uTime); //光环半径
    float deltaDis = abs(dis - targetRad); //差距距离
    if (deltaDis >= ringRad - 3 / uScreenResolution.x)//定义域之外，如果去掉就会全是光环，截尾数据是用于修正毛边效果
    {
        return color;
    }
    else //只有一条光环
    {
        float targetColorRate = 0.5f * cos(deltaDis * PI / ringRad) + 0.5f; //到目标环的距离决定了颜色的改变比率
        float3 totalColor = targetColor * targetColorRate + color.rgb * (1 - targetColorRate); //合并之后的RGB颜色（不保证平衡到加和为1）
        return float4(float3(uIntensity * totalColor + (1 - uIntensity) * color.rgb), color.a); //uprogress是用作呼吸光效果，sin值变化，绑定uintensity可以让光环展开时渐亮
    }
}

technique Technique1
{
    pass FNTwistedRing
    {
        PixelShader = compile ps_3_0 FNTwistedRing();
    }
}