sampler uImage0 : register(s0);
sampler uImage1 : register(s1); // Automatically Images/Misc/Perlin via Force Shader testing option
sampler uImage2 : register(s2); // Automatically Images/Misc/noise via Force Shader testing option
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
//纵横
float2 noise1(in float2 uv)
{
    float noiseX = frac(sin(dot(uv, float2(1 - uIntensity, uIntensity) * 1)) * uIntensity); //第一二个参数跟方向有关，第三四个参数跟密度有关
    float noiseY = sqrt(1 - noiseX * noiseX);
    return float2(noiseX, noiseY);
}

float2 noise2(in float2 uv)
{
    float noiseX = frac(sin(dot(uv, float2(uIntensity, 1 - uIntensity) * 1)) * uIntensity);
    float noiseY = sqrt(1 - noiseX * noiseX);
    return float2(noiseX, noiseY);
}

float4 AACTSTG3RBFence(float2 coords : TEXCOORD0) : COLOR0
{
    float2 coords2 = noise1(coords);
    float2 coords3 = noise2(coords);
    float4 color = tex2D(uImage0, coords);
    float4 color2 = tex2D(uImage0, coords2);
    float4 color3 = tex2D(uImage0, coords3);
    
    color2.g = 0;
    color3.g = 0;
    
    if (color2.r > color2.b)
    {
        color2.b = 0;
    }
    else if (color2.r < color2.b)
    {
        color2.r = 0;
    }
    
    if (color3.r > color3.b)
    {
        color3.b = 0;
    }
    else if (color3.r < color3.b)
    {
        color3.r = 0;
    }
    
    return (1 - 0.25 * uIntensity) * color + 0.25 * uIntensity * color2 + 0.25 * uIntensity * color3;
}

technique Technique1
{
    pass AACTSTG3RBFence
    {
        PixelShader = compile ps_2_0 AACTSTG3RBFence();
    }
}