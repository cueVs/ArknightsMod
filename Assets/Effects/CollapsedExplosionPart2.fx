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

float4 CollapsedExplosionPart2(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    if (!any(color))
        return color;
    float3 targetColor = uColor; //目标颜色
    float targetRad = uIntensity / uScreenResolution.x; //顶点阈限半径
    //float2 ABCDPos[4] = { uTargetPosition, uImageSize1, uImageSize2, uImageSize3 }; //4个顶点的坐标
    //ABCD分别为右下左上
    float2 ABCDxy[4] =
    {
        (uTargetPosition - uScreenPosition) / uScreenResolution,
        (uImageSize1 - uScreenPosition) / uScreenResolution,
        (uImageSize2 - uScreenPosition) / uScreenResolution,
        (uImageSize3 - uScreenPosition) / uScreenResolution
    }; //4个顶点的占比位置
    //对应四边占比阈限为：[2].x ~ [0].x, [3].y ~ [1].y
    float outer = 0.0025f;
    float inner = 0.005f;
    //if (coords.x >= ABCDxy[2].x + outer || coords.x <= ABCDxy[2].x + inner ||
    //    coords.x <= ABCDxy[0].x - outer || coords.x >= ABCDxy[0].x - inner ||
    //    coords.y >= ABCDxy[3].y + outer || coords.y <= ABCDxy[3].y + inner ||
    //    coords.y <= ABCDxy[1].y - outer || coords.y >= ABCDxy[1].y - inner)//描边
    //{
    float shortestdis = min(min(abs((coords.x - ABCDxy[0].x + inner) / uScreenResolution.y * uScreenResolution.x), abs((coords.x - ABCDxy[2].x - inner) / uScreenResolution.y * uScreenResolution.x)), min(abs(coords.y - ABCDxy[1].y + inner), abs(coords.y - ABCDxy[3].y - inner))); //到直角边最近距离
    float colorscale = max(1 - shortestdis / (inner - outer), 0);
    return float4(float4(float3(colorscale * uOpacity * uColor), color.a) + color);
    //}
    //else
    //{
    //    return color;
    //}
}

technique Technique1
{
    pass CollapsedExplosionPart2
    {
        PixelShader = compile ps_2_0 CollapsedExplosionPart2();
    }
}