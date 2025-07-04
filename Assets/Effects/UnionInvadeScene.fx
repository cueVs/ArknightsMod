parameters float Intensity;
sampler uImage0;

void PixelShader(float4 color, float2 uv)
{
    float4 original = tex2D(uImage0, uv);
    float3 tint = float3(1.0, 0.85, 0.4) * Intensity * 0.4;
    float3 result = lerp(original.rgb, tint, Intensity * 0.7);
    return float4(result, original.a);
}