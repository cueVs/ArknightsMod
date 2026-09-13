// 刀光内流动 Shader — 图层蒙版流
// uImage0 = 蒙版（灰度，静止不动，决定哪里显示、显示多强）
// uImage1 = 流动贴图（能量纹理，沿 U 轴滚动）
// 输出 = 流动贴图 × 蒙版灰度 —— 类似图层的"蒙版遮罩"
// 结构照抄 LupineKnifeLight.fx（SV_POSITION + Color 直通）
sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float4x4 uTransform;
float uTime;
float uFlowSpeed;   // 流动速度（正值向左，负值向右）

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
    // 蒙版：静止采样
    float mask = tex2D(uImage0, input.TexCoord).r;

    // 流动贴图：UV 沿 U 轴滚动
    float2 flowUV = input.TexCoord;
    flowUV.x += uTime * uFlowSpeed;
    float4 flow = tex2D(uImage1, flowUV);

    // 蒙版遮罩流动层（灰度控制强度）
    float4 c = flow * mask;
    c.a = 1.0;
    return c * input.Color;
}

technique Technique1
{
    pass Base
    {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}
