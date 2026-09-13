// Soft noise dissolve for blade textures.
// uDissolve: 0 = fully visible, 1 = fully dissolved.
sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4x4 uTransform;
float uTime;
float uDissolve;
float uNoiseScale;
float uEdgeWidth;
float3 uEdgeColor;
float uEdgeIntensity;

struct VSInput
{
    float2 Pos : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Pos : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

VSOutput VS(VSInput input)
{
    VSOutput o;
    o.Pos = mul(float4(input.Pos, 0, 1), uTransform);
    o.Color = input.Color;
    o.TexCoord = input.TexCoord;
    return o;
}

float NoiseField(float2 uv)
{
    float2 drift = float2(uTime * 0.012, -uTime * 0.008);
    float large = tex2D(uNoiseTex, uv * uNoiseScale + drift).r;
    float detail = tex2D(uNoiseTex, uv * (uNoiseScale * 3.0) - drift * 1.7 + 0.37).r;
    return large * 0.78 + detail * 0.22;
}

float VisibleAmount(float field, float threshold, float width)
{
    if (threshold <= 0.0001)
        return 1.0;
    if (threshold >= 0.9999)
        return 0.0;
    return smoothstep(threshold - width, threshold + width, field);
}

float4 PS(VSOutput input) : COLOR0
{
    float4 source = tex2D(uImage0, input.TexCoord);
    float field = NoiseField(input.TexCoord);
    float width = max(uEdgeWidth, 0.001);

    // A soft threshold keeps the blade readable instead of cutting it into a hard silhouette.
    float visible = VisibleAmount(field, uDissolve, width);
    float edge = smoothstep(uDissolve - width, uDissolve, field)
               * (1.0 - smoothstep(uDissolve, uDissolve + width, field));

    float3 rgb = source.rgb * visible;
    rgb += uEdgeColor * edge * uEdgeIntensity;
    float alpha = source.a * max(visible, edge * 0.9);

    return float4(rgb, alpha) * input.Color;
}

technique Technique1
{
    pass Base
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}
