// Vertex-primitive version of the flowing distortion effect.
// s0: trail/source texture, s1: grayscale noise texture.
sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4x4 uTransform;
float Intensity;       // UV distortion amount, normally 0.002 - 0.03
float Speed;           // animation time or speed-scaled time
float WaveAmplitude;   // vertex displacement in pixels
float WaveFrequency;   // number of waves along the trail
float2 TrailDirection; // normalized direction, e.g. velocity.SafeNormalize(Vector2.UnitX)
float3 FlowColor;      // optional tint, normally (1, 1, 1)

struct VSInput
{
    float2 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput output;
    float2 uv = input.TexCoord.xy;

    // No texture sampling in VS: XNA's vs_3_0 compiler does not support tex2D here.
    float phase = uv.x * WaveFrequency - Speed;
    float taper = sin(saturate(uv.x) * 3.14159265);
    float side = uv.y * 2.0 - 1.0;
    float wave = sin(phase + uv.y * 2.7) * taper * side;

    float2 direction = TrailDirection;
    float directionLength = max(length(direction), 0.0001);
    direction /= directionLength;
    float2 normal = float2(-direction.y, direction.x);
    float2 displaced = input.Position + normal * wave * WaveAmplitude;

    output.Position = mul(float4(displaced, 0.0, 1.0), uTransform);
    output.Color = input.Color;
    output.TexCoord = uv;
    return output;
}

float4 PixelShaderFunction(VSOutput input) : COLOR0
{
    float2 uv = input.TexCoord;

    float2 noiseUV1 = uv * 2.3 + Speed * float2(0.7, 1.1);
    float2 noiseUV2 = float2(uv.y * 1.7, uv.x * 1.4) + Speed * float2(-0.5, 0.8);
    float2 noiseUV3 = float2(uv.x - uv.y, uv.y - uv.x) * 2.1 + Speed * float2(1.3, -1.3);

    float4 noise1 = tex2D(uImage1, noiseUV1);
    float4 noise2 = tex2D(uImage1, noiseUV2);
    float4 noise3 = tex2D(uImage1, noiseUV3);

    float2 distortion =
        (noise1.rg - 0.5) * 0.5 +
        (noise2.ba - 0.5) * 0.35 +
        (noise3.rb - 0.5) * 0.35;

    // Preserve the sign of the offset. Squaring it would push both axes positive.
    distortion *= Intensity;

    float4 source = tex2D(uImage0, saturate(uv + distortion));

    // Stream-like color/alpha gradient from UV and the same noise field.
    float noiseValue = dot(noise1.rgb, float3(0.299, 0.587, 0.114));
    float stream = sin((uv.x + uv.y * 0.18) * WaveFrequency - Speed + noiseValue * 6.2831853) * 0.5 + 0.5;
    float edge = 1.0 - abs(uv.y * 2.0 - 1.0);
    float tailFade = 1.0 - smoothstep(0.55, 1.0, uv.x);
    float intensity = (0.35 + stream * 0.65) * (0.45 + noiseValue * 0.55);

    source.rgb *= FlowColor * (0.35 + intensity * 0.9);
    source.a *= edge * tailFade * (0.35 + intensity * 0.65);
    return source * input.Color;
}

technique myTechnique
{
    pass myPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
