sampler uImage0 : register(s0); // The contents of the screen.
sampler uImage1 : register(s1); // Up to three extra textures you can use for various purposes (for instance as an overlay).
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition; // The position of the camera.
float2 uTargetPosition; // The "target" of the shader, what this actually means tends to vary per shader.
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
float4 uSourceRect; // Doesn't seem to be used, but included for parity.
float2 uZoom;

float4 IACTSW(float4 position : SV_POSITION, float2 coords : TEXCOORD0) : COLOR0//冲击波效果
{
    //波纹量（uColor.x） 每个波纹的大小（uColor.y） 冲击波传播速度（uColor.z）uProgress负责传播 uOpacity改变效果强度
    //大小指扭曲范围所在点场的标量，这种写法的特性是：随着尺寸的增加 波纹间距反而变得更窄
    //核心原理：从特定点（冲击波的来源）生成径向映射，然后将该映射反馈到正弦函数中以生成波纹
    //代码改编自以下链接中的样本，依据其声明显示在这里：
    //https://forums.terraria.org/index.php?threads/tutorial-shockwave-effect-for-tmodloader.81685/

    float PI = 3.1415926535897932384626433832795028841971f;//诶，他奶奶滴！为啥不能直接用PI

    //不建议在这里修改这些量：
    //float rewritewaveamount = uColor.x * 0.5f;
    //float rewritewavescale = uColor.y * 2.0f;
    //float rewritewavevelocity = uColor.z * 0.002f*uTime*uTime-0.3f*uTime+10.8f;
    /*if(rewritewavevelocity < 0)
    {
        rewritewavevelocity = 0;
    }*/
    
    float2 rewritetargetposition = (uTargetPosition - uScreenPosition) / uScreenResolution;//目标触发位置
    float2 realcenter = (coords - rewritetargetposition) * (uScreenResolution / uScreenResolution.y);//正中央位置
    float bechangedfield = dot(realcenter, realcenter);//被改变区域
    float ripple = bechangedfield * uColor.y * PI - uProgress * uColor.z;//扭曲效果

    if (ripple < 0 && ripple > uColor.x * -2 * PI)//范围判定
    {
        ripple = saturate(sin(ripple));
    }
    else
    {
        ripple = 0;
    }

    float2 finaloutput = coords + ((ripple * uOpacity / uScreenResolution) * realcenter);//最终合成输出效果

    return tex2D(uImage0, finaloutput);//将效果应用于屏幕
}

technique Technique1
{
    pass IACTSW
    {
        PixelShader = compile ps_2_0 IACTSW();
    }
}