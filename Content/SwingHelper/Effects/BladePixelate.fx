// Texture-space pixelation for blade and trail primitives.
// uPixelSize: number of pixel blocks across the UV area.
// uColorSteps: RGB quantization levels; set to 0 to disable color quantization.
sampler uImage0 : register(s0);
float4x4 uTransform;
float uPixelSize;
float uColorSteps;

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

float4 PS(VSOutput input) : COLOR0
{
    float2 uv = saturate(input.TexCoord);
    float size = max(uPixelSize, 1.0);
    float2 cell = floor(uv * size);
    float2 pixelUV = (cell + 0.5) / size;
    float4 color = tex2D(uImage0, pixelUV);

    if (uColorSteps > 1.0)
    {
        float steps = uColorSteps - 1.0;
        color.rgb = floor(color.rgb * steps + 0.5) / steps;
    }

    return color * input.Color;
}

technique Technique1
{
    pass Base
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}
