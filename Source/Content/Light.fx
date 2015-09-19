Texture2D Texture;
SamplerState TextureSampler;

cbuffer cbConstant
{
	float4 Color;
};

cbuffer cbPerLight
{	
	float4x4 WorldViewProjection;
	float3 LightColor;
	float LightIntensity;
};

cbuffer cbPerSpotlight
{
	float2 ConeDirection;
	float ConeAngle;
	float ConeDecay;
};

cbuffer cbPerTexturedLight
{
	float4x4 TextureTransform;
};

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

VertexOut VSPointLight(VertexIn vin)
{
	VertexOut vout;
	
	vout.Position = mul(float4(vin.Position.x, vin.Position.y, 0.0f, 1.0f), WorldViewProjection);	
	vout.TexCoord = vin.TexCoord;

	return vout;
}

VertexOut VSTexturedLight(VertexIn vin)
{
	VertexOut vout;

	vout.Position = mul(float4(vin.Position.x, vin.Position.y, 0.0f, 1.0f), WorldViewProjection);	
	vout.TexCoord = mul(float4(vin.TexCoord, 0.0f, 1.0f), TextureTransform).xy;

	return vout;
}

float4 GetComputedColor(float alpha)
{
	alpha = abs(alpha);
	float3 lightColor = LightColor * alpha;
	lightColor = pow(lightColor, 1.0f / LightIntensity);
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
	float2 lightVector = (pin.TexCoord - float2(0.5f, 0.5f));
	float halfMagnitude = length(lightVector);
	float2 lightDir = lightVector / halfMagnitude;

	float halfConeAngle = ConeAngle * 0.5f;
	float halfAngle = acos(dot(ConeDirection, lightDir));

	float occlusion = step(halfAngle, halfConeAngle);

	float distanceAttenuation = saturate(1.0f - halfMagnitude * 2.0f);
	float coneAttenuation = 1.0f - pow(halfAngle / halfConeAngle, ConeDecay);

	float alpha = distanceAttenuation * coneAttenuation;

	return GetComputedColor(alpha * occlusion);
}

float4 PSTexturedLight(VertexOut pin) : SV_TARGET
{
	return GetComputedColor(Texture.Sample(TextureSampler, pin.TexCoord).x);	
}

float4 PSDebugLight(VertexOut pin) : SV_TARGET
{
	return Color;
}

technique PointLight
{
	pass P0
	{		
		VertexShader = compile vs_4_0_level_9_1 VSPointLight();
		PixelShader = compile ps_4_0_level_9_1 PSPointLight();
	}
}

technique SpotLight
{
	pass P0
	{		
		VertexShader = compile vs_4_0_level_9_1 VSPointLight();
		PixelShader = compile ps_4_0_level_9_1 PSSpotLight();
	}
}

technique TexturedLight
{
	pass P0
	{
		VertexShader = compile vs_4_0_level_9_1 VSTexturedLight();
		PixelShader = compile ps_4_0_level_9_1 PSTexturedLight();
	}
}

technique DebugLight
{
	pass P0
	{
		VertexShader = compile vs_4_0_level_9_1 VSPointLight();
		PixelShader = compile ps_4_0_level_9_1 PSDebugLight();
	}
}