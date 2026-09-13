// Noise dissolve that replaces the dissolved area with a caller supplied color.
// uDissolve: 0 = no colored area, 1 = fully colored.
sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4x4 uTransform;
float uTime;
float uDissolve;
float uNoiseScale;
float uEdgeWidth;
float3 uDissolveColor;
float uDissolveColorStrength;
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
    float2 drift = float2(-uTime * 0.01, uTime * 0.014);
    float large = tex2D(uNoiseTex, uv * uNoiseScale + drift).r;
    float detail = tex2D(uNoiseTex, uv * (uNoiseScale * 2.8) - drift * 1.5 + 0.19).r;
    return large * 0.72 + detail * 0.28;
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

    float visible = VisibleAmount(field, uDissolve, width);
    float removed = 1.0 - visible;
    float edge = smoothstep(uDissolve - width, uDissolve, field)
               * (1.0 - smoothstep(uDissolve, uDissolve + width, field));

    // Replace only the dissolved portion; the original texture remains readable elsewhere.
    float colorAmount = saturate(removed * uDissolveColorStrength);
    float3 rgb = lerp(source.rgb, uDissolveColor, colorAmount);
    rgb = lerp(rgb, uEdgeColor, edge * uEdgeIntensity);
    float alpha = source.a * max(visible, colorAmount);

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
