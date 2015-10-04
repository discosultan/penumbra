Texture2D Texture : register(t0);
SamplerState TextureSampler;

cbuffer cbConstant : register(c0)
{
	float4 Color;
};

cbuffer cbPerLight : register(c1)
{	
	float4x4 WorldViewProjection;
	float3 LightColor;
	float LightIntensity;
};

cbuffer cbPerSpotlight : register(c6)
{
	float ConeHalfAngle;
	float ConeDecay;
};

//cbuffer cbPerTexturedLight : register(c7)
//{
//	float4x4 TextureTransform;
//};

struct VertexIn
{
	float2 Position : SV_POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexOut
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
};

VertexOut VSLight(VertexIn vin)
{
	VertexOut vout;
	
	vout.Position = mul(float4(vin.Position.x, vin.Position.y, 0.0f, 1.0f), WorldViewProjection);	
	vout.TexCoord = vin.TexCoord;

	return vout;
}

//VertexOut VSTexturedLight(VertexIn vin)
//{
//	VertexOut vout;
//
//	vout.Position = mul(float4(vin.Position.x, vin.Position.y, 0.0f, 1.0f), WorldViewProjection);	
//	vout.TexCoord = mul(float4(vin.TexCoord, 0.0f, 1.0f), TextureTransform).xy;
//
//	return vout;
//}

float4 GetComputedColor(float alpha)
{
	alpha = abs(alpha);
	float3 lightColor = LightColor * alpha;
	lightColor = pow(lightColor, LightIntensity);
	return float4(lightColor, 1.0f);
}

float4 PSPointLight(VertexOut pin) : SV_TARGET
{	
	float halfMagnitude = length(pin.TexCoord - float2(0.5f, 0.5f));
	float alpha = saturate(1 - halfMagnitude * 2.0f);
	return GetComputedColor(alpha);
}

float4 PSSpotLight(VertexOut pin) : SV_TARGET
{
	float2 lightVector = (pin.TexCoord - float2(0.0f, 0.5f));
	float magnitude = length(lightVector);
	float2 lightDir = lightVector / magnitude;
	
	float halfAngle = acos(dot(lightDir, float2(1, 0)));

	float occlusion = step(halfAngle, ConeHalfAngle);

	float distanceAttenuation = saturate(1.0f - magnitude);
	float coneAttenuation = 1.0f - pow(halfAngle / ConeHalfAngle, ConeDecay);

	float alpha = distanceAttenuation * coneAttenuation;

	return GetComputedColor(alpha * occlusion);
}

float4 PSTexturedLight(VertexOut pin) : SV_TARGET
{
	float alpha = Texture.Sample(TextureSampler, pin.TexCoord).x;
	
	// Shift tex coord to range [-0.5...0.5] and take absolute value.
	float2 tc = abs(pin.TexCoord - float2(0.5f, 0.5f));

	// If tex coord is outside its normal range, don't lit the pixel.
	alpha = alpha * step(tc.x, 0.5f) * step(tc.y, 0.5f);

	return GetComputedColor(alpha);	
}

float4 PSDebugLight(VertexOut pin) : SV_TARGET
{
	return Color;
}

technique PointLight
{
	pass P0
	{		
		VertexShader = compile vs_4_0_level_9_1 VSLight();
		PixelShader = compile ps_4_0_level_9_1 PSPointLight();
	}
}

technique Spotlight
{
	pass P0
	{		
		VertexShader = compile vs_4_0_level_9_1 VSLight();
		PixelShader = compile ps_4_0_level_9_1 PSSpotLight();
	}
}

technique TexturedLight
{
	pass P0
	{
		VertexShader = compile vs_4_0_level_9_1 VSLight();
		PixelShader = compile ps_4_0_level_9_1 PSTexturedLight();
	}
}

technique DebugLight
{
	pass P0
	{
		VertexShader = compile vs_4_0_level_9_1 VSLight();
		PixelShader = compile ps_4_0_level_9_1 PSDebugLight();
	}
}