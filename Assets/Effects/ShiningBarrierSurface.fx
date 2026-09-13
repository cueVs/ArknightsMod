float globalTime;
float3 uShieldColor;
float3 uEdgeColor;
float3 uCoreColor;
float uCharge;
float uHitFlash;
float uOpacity;
float uRotation;
float uLayer;

sampler uNoiseSampler : register(s1);

float SampleValue(float4 sampleColor)
{
	return max(sampleColor.a, dot(sampleColor.rgb, float3(0.299, 0.587, 0.114)));
}

float2 Rotate(float2 value, float angle)
{
	float sine = sin(angle);
	float cosine = cos(angle);
	return float2(value.x * cosine - value.y * sine, value.x * sine + value.y * cosine);
}

float4 ShieldPixel(float4 inputColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
	float2 p = uv * 2.0 - 1.0;
	float radius = length(p);
	if (radius >= 1.0)
		return float4(0, 0, 0, 0);

	float sphereDepth = sqrt(saturate(1.0 - radius * radius));
	float2 sphereUv = p / (0.62 + sphereDepth * 0.78);
	sphereUv = Rotate(sphereUv, uRotation * (uLayer < 0.5 ? 0.42 : -0.67));
	sphereUv *= uLayer < 0.5 ? 0.88 : 1.31;
	sphereUv += float2(globalTime * (uLayer < 0.5 ? 0.18 : -0.11),
		globalTime * (uLayer < 0.5 ? -0.035 : 0.052));

	float noiseA = SampleValue(tex2D(uNoiseSampler, sphereUv));
	float noiseB = SampleValue(tex2D(uNoiseSampler,
		sphereUv * float2(-0.71, 1.47) + float2(globalTime * 0.07, 0.29)));
	float movingField = saturate(noiseA * 0.68 + noiseB * 0.42);

	float fresnel = pow(saturate(radius), 5.2);
	float edge = smoothstep(0.74, 0.94, radius) * (1.0 - smoothstep(0.975, 1.0, radius));
	float innerShell = smoothstep(0.48, 0.82, radius) * (1.0 - smoothstep(0.86, 0.96, radius));
	float scan = pow(saturate(sin((p.y + globalTime * 0.24) * 25.0) * 0.5 + 0.5), 7.0);
	float hitRingRadius = lerp(0.20, 0.91, 1.0 - uHitFlash);
	float hitRing = (1.0 - smoothstep(0.025, 0.11, abs(radius - hitRingRadius))) * uHitFlash;
	float fracture = pow(saturate(sin(atan2(p.y, p.x) * 7.0 + movingField * 5.0) * 0.5 + 0.5), 8.0)
		* edge * uHitFlash;

	float fieldAlpha = (0.045 + movingField * 0.16 + scan * 0.075) * (0.42 + uCharge * 0.58);
	fieldAlpha *= saturate(1.0 - radius * 0.64);
	float alpha = (fieldAlpha + fresnel * 0.22 + edge * (0.42 + uCharge * 0.22)
		+ innerShell * movingField * 0.10 + hitRing * 0.74 + fracture * 0.35) * uOpacity;
	alpha *= uLayer < 0.5 ? 1.0 : 0.42;

	float3 surface = lerp(uShieldColor, uEdgeColor,
		saturate(fresnel * 0.78 + edge * 0.72 + movingField * 0.16));
	surface = lerp(surface, uCoreColor, saturate(hitRing * 0.82 + fracture * 0.40));
	return float4(surface, saturate(alpha));
}

technique ShiningBarrierSurface
{
	pass ShiningBarrierPass
	{
		PixelShader = compile ps_3_0 ShieldPixel();
	}
}
