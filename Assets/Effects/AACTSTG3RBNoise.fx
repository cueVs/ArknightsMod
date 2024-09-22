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

float2 noise(in float2 uv)
{
    float noiseX = (frac(sin(dot(uv, float2(403, 52) * 1.6)) * 140471));
    float noiseY = sqrt(1 - noiseX * noiseX);
    return float2(noiseX, noiseY);
}

float4 AACTSTG3RBNoise(float2 coords : TEXCOORD0) : COLOR0
{
    float2 coords2 = noise(coords);
    float4 color = tex2D(uImage0, coords);
    float4 color2 = tex2D(uImage0, coords2);
    
    color2.g = 0;
    
    if (color2.r > color2.b)
    {
        color2.b = 0;
    }
    else if (color2.r < color2.b)
    {
        color2.r = 0;
    }
    
    return (1 - 0.25 * uIntensity) * color + 0.75 * uIntensity * color2;
}

technique Technique1
{
    pass AACTSTG3RBNoise
    {
        PixelShader = compile ps_2_0 AACTSTG3RBNoise();
    }
}