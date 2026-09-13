texture2D tex0;
sampler2D uShapeTex = sampler_state
{
    Texture = <tex0>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};
texture2D tex1;
sampler2D uColorTex = sampler_state
{
    Texture = <tex1>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};
float4x4 uTransform;
struct VSInput 
{
	float2 Pos : POSITION0;
    float4 Color : COLOR0;
	float3 Texcoord : TEXCOORD0;
};

struct PSInput 
{
	float4 Pos : SV_POSITION;
    float4 Color : COLOR0;
	float3 Texcoord : TEXCOORD0;
};
PSInput VertexShaderFunction(VSInput input) 
{
	PSInput output;
	output.Texcoord = input.Texcoord;
	output.Pos = mul(float4(input.Pos, 0, 1), uTransform);
    output.Color = input.Color;
	return output;
}

float4 PixelShaderFunction(PSInput input) : COLOR0
{
    float3 coord = input.Texcoord;
    float4 c = tex2D(uShapeTex, float2(coord.x, coord.y)); //主纹理
    c *= tex2D(uColorTex, float2(c.r * coord.z, 0.5)); //乘颜色图
    return c * coord.z * input.Color;
}

float4 PixelShaderFunction2(PSInput input) : COLOR0
{
    float3 coord = input.Texcoord;
    float4 c = tex2D(uShapeTex, float2(coord.x, coord.y)); //主纹理
    c = tex2D(uColorTex, float2(c.r * coord.z, 0.5)); //取颜色
    float a = 1-(c.r - 1) * 0.7f;
    c *= a; //乘上透明度
    return c * input.Color;
}

// ============================================================
// Flow pass —— 狼之绯专用：紫红渐变 + 沿挥砍反方向流动的水墨噪声调制 + 尾部溶解消散
// tex2 = 流动噪声贴图（灰度，复用现成的 NoiseTexture.png）
// uFlowOffset：C# 每帧累加、方向与挥砍相反，让噪声看起来"逆着挥砍流动"
// uDissolveAmount：0=不溶解 1=尾部大量溶解（越靠尾部需要越高的噪声亮度才能保留，制造不规则消散边缘）
// uAccentThreshold/uAccentColor：噪声亮度超过阈值处混入更亮的紫/近白高光纹理
// ============================================================
texture2D tex2;
sampler2D uNoiseTex = sampler_state
{
    Texture = <tex2>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};
float2 uFlowOffset;
float2 uNoiseScale;
float uDissolveAmount;
float uAccentThreshold;
float3 uAccentColor;

float4 PixelShaderFunction_Flow(PSInput input) : COLOR0
{
    // x: 挥砍历史轴，1=刚扫过的新位置(刀尖当前所在) → 0=较早扫过的旧位置(拖尾尾部)——
    //    这条轴管"新旧/溶解"，和"离刀柄多近"是两回事，不要混用
    // y: 径向轴，0=inner(贴近握持位置，即"刀柄"一侧) → 1=outer(刀尖沿弧线扫出的外缘)——
    //    "越靠近刀柄颜色越深"要用这条轴取色，不是 x
    // z: 基础权重
    float3 coord = input.Texcoord;
    float age = 1 - coord.x;                 // 0=刚生成，1=最老（尾部，最先开始溶解）

    float shapeMask = tex2D(uShapeTex, float2(coord.x, coord.y)).r;

    float2 noiseUV = float2(coord.x, coord.y) * uNoiseScale + uFlowOffset;
    float n = tex2D(uNoiseTex, noiseUV).r;

    // 水墨溶解：尾部需要越来越高的噪声亮度才能保留像素，形成不规则消散边缘，而非线性淡出
    float dissolveThreshold = saturate(age * uDissolveAmount);
    float dissolveMask = saturate((n - dissolveThreshold) / max(0.001, 1 - dissolveThreshold));

    // 底色渐变取自径向轴 coord.y（0=刀柄端更深，1=刀尖端更亮），不是挥砍历史轴
    float3 baseColor = tex2D(uColorTex, float2(coord.y, 0.5)).rgb;
    // 噪声同时调制底色明暗，制造不规则纹理感（不是纯色平铺，也不是规则渐变）
    baseColor *= lerp(0.55, 1.2, n);
    // 高噪声亮度处混入更亮的紫/近白高光纹理
    float accent = smoothstep(uAccentThreshold, 1.0, n);
    baseColor = lerp(baseColor, uAccentColor, accent * 0.85);

    float alpha = shapeMask * coord.z * dissolveMask;
    return float4(baseColor * alpha, alpha) * input.Color;
}

technique Technique1 {
	pass Trail {
		VertexShader = compile vs_2_0 VertexShaderFunction();
		PixelShader = compile ps_2_0 PixelShaderFunction();
	}
    pass Trail0
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction2();
    }
    pass Flow
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction_Flow();
    }
}
