#include "Macros.fxh"

Texture2D LightTexture : register(t0);
SamplerState TextureSampler : register(s0);

cbuffer cbConstant 
{
	float4 Color;
};

//cbuffer cbPerFrame 
//{
//	float4x4 ViewProjection;
//};

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

float4 GetComputedColor(float alpha)
{
	alpha = abs(alpha);
	float3 lightColor = LightColor * alpha;
	lightColor = pow(lightColor, LightIntensity);
	return float4(lightColor, 1.0);
}

float4 PSPointLight(VertexOut pin) : SV_TARGET
{	
	float halfMagnitude = length(pin.TexCoord - float2(0.5, 0.5));
	float alpha = saturate(1.0 - halfMagnitude * 2.0);
	return GetComputedColor(alpha);
}

float4 PSSpotLight(VertexOut pin) : SV_TARGET
{
	float2 lightVector = (pin.TexCoord - float2(0.0, 0.5));
	float magnitude = length(lightVector);
	float2 lightDir = lightVector / magnitude;
	
	float halfAngle = acos(dot(lightDir, float2(1.0, 0.0)));

	float occlusion = step(halfAngle, ConeHalfAngle);

	float distanceAttenuation = saturate(1.0 - magnitude);
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
	return Color;
}

TECHNIQUE(PointLight, VS, PSPointLight);
TECHNIQUE(Spotlight, VS, PSSpotLight);
TECHNIQUE(TexturedLight, VS, PSTexturedLight);
TECHNIQUE(DebugLight, VS, PSDebugLight);
