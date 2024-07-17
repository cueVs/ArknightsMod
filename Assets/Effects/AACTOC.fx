sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 AACTOC(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    if (!any(color))
        return color;
    //直接改RGB
    //float4 negativecolor = float4(1 - color.r, 1 - color.g, 1 - color.b, color.a);
    //float4 deltacolor = float4(negativecolor.rgb - color.rgb, color.a);
    //float4 oppositecolor = float4(negativecolor.rgb - deltacolor.rgb * uColor.x / 60, color.a);//这样变化会变灰
    //return oppositecolor;
    
    //RGB转HSV
    float Cmax = max(max(color.r, color.g), color.b);
    float Cmin = min(min(color.r, color.g), color.b);
    float deltac = Cmax - Cmin;
    
    float H; //色相
    float S; //饱和度
    //float V; //明度
    
    if (deltac == 0)
    {
        H = 0;
        S = 0;
    }
    else
    {
        if (Cmax == color.r)
        {
            H = 60 * (((color.g - color.b) / deltac));
        }
        else if (Cmax == color.g)
        {
            H = 60 * (((color.b - color.r) / deltac) + 2);
        }
        else if (Cmax == color.b)
        {
            H = 60 * (((color.r - color.g) / deltac) + 4);
        }
        //else if (Cmax == color.r && Cmax == color.g)
        //{
        //    H = 60 * (((Cmax - color.b) / deltac));
        //}
        //else if (Cmax == color.g && Cmax == color.b)
        //{
        //    H = 60 * (((Cmax - color.r) / deltac) + 2);
        //}
        //else if (Cmax == color.b && Cmax == color.r)
        //{
        //    H = 60 * (((Cmax - color.g) / deltac) + 4);
        //}
        
        S = deltac / Cmax;
    }
    
    //V = Cmax;
    
    //if (H < 0)
    //{
    //    H += 360;
    //}
    //if (H >= 360)
    //{
    //    H -= 360;
    //}
    
    float transH = H + uIntensity * 360; //调整后的色相
    float4 transcolor; //调整后的颜色
    
    if (transH < 0)
    {
        transH += 360;
    }
    if (transH >= 360)
    {
        transH -= 360;
    }
    
    //HSV转RGB
    int hi = floor(transH / 60);
    float f = (transH / 60) - hi;
    //float p = V * (1 - S);
    //float q = V * (1 - f * S);
    //float t = V * (1 - (1 - f) * S);
    float p = Cmax * (1 - S);
    float q = Cmax * (1 - f * S);
    float t = Cmax * (1 - (1 - f) * S);
    
    if (hi == 0)
    {
        //transcolor = float4(V, t, p, color.a);
        transcolor = float4(Cmax, t, p, color.a);
        //return float4(V, t, p, color.a);
    }
    else if (hi == 1)
    {
        //transcolor = float4(q, V, p, color.a);
        transcolor = float4(q, Cmax, p, color.a);
        //return float4(q, V, p, color.a);
    }
    else if (hi == 2)
    {
        //transcolor = float4(p, V, t, color.a);
        transcolor = float4(p, Cmax, t, color.a);
        //return float4(p, V, t, color.a);
    }
    else if (hi == 3)
    {
        //transcolor = float4(p, q, V, color.a);
        transcolor = float4(p, q, Cmax, color.a);
        //return float4(p, q, V, color.a);
    }
    else if (hi == 4)
    {
        //transcolor = float4(t, p, V, color.a);
        transcolor = float4(t, p, Cmax, color.a);
        //return float4(t, p, V, color.a);
    }
    else
    {
        //transcolor = float4(V, p, q, color.a);
        transcolor = float4(Cmax, p, q, color.a);
        //return float4(V, p, q, color.a);
    }
    
    return float4(transcolor);
}

technique Technique1
{
    pass AACTOC
    {
        PixelShader = compile ps_2_0 AACTOC();
    }
}