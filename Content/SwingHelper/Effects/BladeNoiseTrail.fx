// Procedural flowing trail. The only texture is a grayscale noise map bound to s0.
sampler uNoiseTex : register(s0);
float4x4 uTransform;
float uTime;
float uFlowSpeed;
float uNoiseScale;
float uWaveAmplitude;
float uWaveFrequency;
float uBreakup;
float uTrailAngle;
float3 uFlowColor;

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
    float Flow : TEXCOORD1;
};

VSOutput VS(VSInput input)
{
    VSOutput output;
    float2 uv = input.TexCoord;
    float phase = uv.x * uWaveFrequency - uTime * uFlowSpeed;
    float taper = sin(saturate(uv.x) * 3.14159265);
    float side = uv.y * 2.0 - 1.0;
    // Keep VS texture-free: the XNA vs_3_0 compiler cannot map tex2D here.
    float wave = sin(phase + cos(phase * 1.73 + uv.y * 4.0) * 0.35);

    float2 direction = float2(cos(uTrailAngle), sin(uTrailAngle));
    float2 normal = float2(-direction.y, direction.x);
    float2 offset = normal * wave * side * taper * uWaveAmplitude;
    output.Pos = mul(float4(input.Pos + offset, 0.0, 1.0), uTransform);
    output.Color = input.Color;
    output.TexCoord = uv;
    output.Flow = wave * 0.5 + 0.5;
    return output;
}

float4 PS(VSOutput input) : COLOR0
{
    float2 uv = input.TexCoord;
    float2 flowUv = uv * uNoiseScale + float2(-uTime * uFlowSpeed, 0.0);
    float noise = tex2D(uNoiseTex, flowUv).r;
    float detail = tex2D(uNoiseTex, flowUv * 3.0 + float2(0.37, 0.19)).r;
    noise = noise * 0.7 + detail * 0.3;

    // Pixel xy and noise jointly form the moving stream bands.
    float pixelPhase = (uv.x + uv.y * 0.18) * uWaveFrequency;
    float stream = sin(pixelPhase - uTime * uFlowSpeed + noise * 6.2831853) * 0.5 + 0.5;
    float edge = 1.0 - abs(uv.y * 2.0 - 1.0);
    float tailFade = 1.0 - smoothstep(0.55, 1.0, uv.x);
    float breakup = smoothstep(uBreakup, 1.0, noise);
    float gradient = saturate(0.25 + uv.x * 0.75);
    float intensity = (0.25 + stream * 0.75) * (0.35 + noise * 0.65);
    float alpha = edge * tailFade * breakup * intensity * input.Color.a;
    float3 rgb = uFlowColor * (0.3 + gradient * 0.7 + intensity * 0.8);
    rgb += uFlowColor * stream * edge * 0.35;
    return float4(rgb, alpha);
}

technique Technique1
{
    pass Base
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}
