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

float4 CollapsedExplosionPart1(float2 coords : TEXCOORD0) : COLOR0
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
    float2 offset[4] =
    {
        coords - ABCDxy[0],
        coords - ABCDxy[1],
        coords - ABCDxy[2],
        coords - ABCDxy[3]
    }; //每个像素点到每个顶点的占比向量
    float2 fixedVec[4] =
    {
        offset[0] * float2(uScreenResolution.x / uScreenResolution.y, 1),
        offset[1] * float2(uScreenResolution.x / uScreenResolution.y, 1),
        offset[2] * float2(uScreenResolution.x / uScreenResolution.y, 1),
        offset[3] * float2(uScreenResolution.x / uScreenResolution.y, 1)
    }; //修正向量
    float dist[4] = { length(fixedVec[0]), length(fixedVec[1]), length(fixedVec[2]), length(fixedVec[3]) }; //每个像素点到顶点的占比距离
    float inner = 0.005f;
    if (dist[0] <= targetRad || 
        dist[1] <= targetRad ||
        dist[2] <= targetRad || 
        dist[3] <= targetRad ||
        coords.x <= ABCDxy[2].x + inner ||
        coords.x >= ABCDxy[0].x - inner ||
        coords.y <= ABCDxy[3].y + inner ||
        coords.y >= ABCDxy[1].y - inner)//挖去的四个洞和四条边，再挖去描边（内核部分）
    {
        return color;
    }
    else //区域范围内涂色
    {
        return float4(float4(float3(uOpacity * uColor), color.a) + color);
    }
}

technique Technique1
{
    pass CollapsedExplosionPart1
    {
        PixelShader = compile ps_2_0 CollapsedExplosionPart1();
    }
}