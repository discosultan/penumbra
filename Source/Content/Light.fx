#include "Macros.fxh"

Texture2D LightTexture : register(t0);
SamplerState TextureSampler : register(s0);

cbuffer cbConstant 
{
	float4 DebugColor;
};

cbuffer cbPerLight 
{	
	float4x4 WorldViewProjection;	
	float3 LightColor;
	float LightIntensity;
};

cbuffer cbPerSpotlight 
{
	float ConeHalfAngle;
	float ConeDecay;
};

struct VertexOut
{
	float4 Position : SV_POSITION0;	
	float2 TexCoord : TEXCOORD0;
};

VertexOut VS(float2 posL : SV_POSITION0, float2 texCoord : TEXCOORD0)
{
	VertexOut vout;
	
	vout.Position = mul(float4(posL.x, posL.y, 0.0, 1.0), WorldViewProjection);		
	vout.TexCoord = texCoord;

	return vout;
}

float CalculateDistanceAttenuation(float magnitude) 
{
	// LINEAR
	return saturate(1.0 - magnitude);

	//return saturate(1.0 - magnitude*magnitude);

	// CONSTANT-LINEAR-QUADRATIC
	float3 falloff = float3(0.14, 3, 20);
	//return lerp(0.0, 1.0 / (falloff.x + (falloff.y*magnitude) + (falloff.z*magnitude*magnitude)), step(magnitude, 1.0));
	//return 1.0 / (falloff.x + (falloff.y*magnitude) + (falloff.z*magnitude*magnitude)) * (1 - magnitude*magnitude*magnitude*magnitude);
}

float4 GetComputedColor(float alpha)
{
	alpha = abs(alpha);
	float3 lightColor = LightColor * alpha;
	lightColor = pow(lightColor, LightIntensity);
	return float4(lightColor, 1.0);
}

float4 PSPointLight(VertexOut pin) : SV_TARGET
{	
	float magnitude = min(length(pin.TexCoord - float2(0.5, 0.5)) * 2, 1.0);
	float alpha = CalculateDistanceAttenuation(magnitude);
	return GetComputedColor(alpha);
}

float4 PSSpotLight(VertexOut pin) : SV_TARGET
{
	float2 lightVector = (pin.TexCoord - float2(0.0, 0.5));
	float magnitude = min(length(lightVector), 1.0);
	float2 lightDir = lightVector / magnitude;
	
	float halfAngle = acos(dot(lightDir, float2(1.0, 0.0)));

	float occlusion = step(halfAngle, ConeHalfAngle);

	//float distanceAttenuation = saturate(1.0 - magnitude);
	float distanceAttenuation = CalculateDistanceAttenuation(magnitude);
	float coneAttenuation = 1.0 - pow(halfAngle / ConeHalfAngle, ConeDecay);

	float alpha = distanceAttenuation * coneAttenuation;

	return GetComputedColor(alpha * occlusion);
}

float4 PSTexturedLight(VertexOut pin) : SV_TARGET
{
	float alpha = LightTexture.Sample(TextureSampler, pin.TexCoord).x;
	
	// Shift tex coord to range [-0.5...0.5] and take absolute value.
	float2 tc = abs(pin.TexCoord - float2(0.5, 0.5));

	// If tex coord is outside its normal range, don't lit the pixel.
	alpha = alpha * step(tc.x, 0.5) * step(tc.y, 0.5);

	return GetComputedColor(alpha);	
}

float4 PSDebugLight(VertexOut pin) : SV_TARGET
{
	return DebugColor;
}

TECHNIQUE(PointLight, VS, PSPointLight);
TECHNIQUE(Spotlight, VS, PSSpotLight);
TECHNIQUE(TexturedLight, VS, PSTexturedLight);
TECHNIQUE(DebugLight, VS, PSDebugLight);
