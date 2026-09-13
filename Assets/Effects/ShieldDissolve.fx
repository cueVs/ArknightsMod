// ShieldDissolve - spritebatch dissolve by noise
// uTime: elapsed seconds, 0 = intact (pristine), 2 = fully dissolved (C# clamps 0~2)
// uEdgeColor: dissolve edge glow color (operator theme)
// 透明区域（贴图无像素处）直接 discard，不参与消融
// 编译限制（老管线）：不用 saturate；不嵌套 intrinsic；PS 内只允许一个 smoothstep
texture2D tex0;
sampler2D uImage0 = sampler_state
{
    Texture = <tex0>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};
texture2D tex1;
sampler2D uNoiseTex = sampler_state
{
    Texture = <tex1>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};
float4x4 uTransform;
float uTime;
float uNoiseScale;
float3 uEdgeColor;

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

VSOutput VertexShaderFunction(VSInput input)
{
    VSOutput o;
    o.Pos = mul(float4(input.Pos, 0, 1), uTransform);
    o.Color = input.Color;
    o.TexCoord = input.TexCoord;
    return o;
}

float4 PixelShaderFunction(VSOutput input) : COLOR0
{
    float2 uv = input.TexCoord;
    float4 c = tex2D(uImage0, uv);

    // 贴图无像素处直接丢弃，不参与消融
    if (c.a < 0.003)
        discard;

    // 贴图透明度
    float alpha = c.a;

    // 噪声场上移（最低 0.2）→ uTime=0 时 smoothstep(0, 0.2, field) 恒 1 → 盾完全干净
    float field = tex2D(uNoiseTex, uv * uNoiseScale).r * 0.8 + 0.2;

    // 消融：0 = 完整，2 秒 = 全消（唯一的 smoothstep）
    float dissolve = uTime * 0.5;
    float alive = smoothstep(dissolve, dissolve + 0.2, field);

    c.a *= alive;
    c.rgb *= alive;

    // 前沿辉光：alive 中段凸起 × 贴图 alpha（双重保险）
    float front = alive * (1.0 - alive) * 4.0 * alpha;
    c.rgb += uEdgeColor * front * 0.8;

    return c * input.Color;
}

technique Technique1
{
    pass Base
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
