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

float3 sincolor(float3 delta, float3 negative,float PI)//开始变化快后来变化慢
{
    return delta * sin(-uIntensity * PI / 120) + negative;
}

float3 linecolor(float3 delta, float3 negative)//线性变化
{
    return negative - delta * uIntensity / 60;
}

float3 coscolor(float3 delta, float3 normal, float PI)//开始变化慢后来变化快
{
    return delta * cos(uIntensity * PI / 120) + normal;
}

float4 AACTOC2(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    if (!any(color))
        return color;
    float PI = 3.1415926535897932384626433832795028841971f;
    float4 negativecolor = float4(1 - color.r, 1 - color.g, 1 - color.b, color.a); //1-t
    float4 deltacolor = float4(negativecolor.rgb - color.rgb, color.a); //1-t-t
    
    float3 s = sincolor(deltacolor.rgb, negativecolor.rgb, PI);
    float3 l = linecolor(deltacolor.rgb, negativecolor.rgb);
    float3 c = sincolor(deltacolor.rgb, color.rgb, PI);
    
    //slc & lsc：yellow to normal
    //scl & lcs：violet to normal
    //csl & cls：blue to normal

    float4 oppositecolor = float4(c.r, s.g, l.b, color.a); //这样变化不会变灰
    return oppositecolor;
}

technique Technique1
{
    pass AACTOC2
    {
        PixelShader = compile ps_2_0 AACTOC2();
    }
}