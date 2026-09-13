sampler BaseTexture : register(s0);

float opacity : register(c0);
float orangeThreshold : register(c1);
float yellowThreshold : register(c2);

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 original = tex2D(BaseTexture, texCoord);
    
    if (original.a <= 0.0)
        return float4(0.0, 0.0, 0.0, 0.0);
    
    float whiteAmount = original.r;
    
    float4 yellow = float4(0.99, 0.86, 0.01, original.a);
    float4 orange = float4(0.99, 0.24, 0.01, original.a);
    
    float t = clamp((whiteAmount - orangeThreshold) / (yellowThreshold - orangeThreshold), 0.0, 1.0);
    float4 finalColor = lerp(orange, yellow, t);
    
    finalColor.a *= opacity;
    
    return finalColor;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}