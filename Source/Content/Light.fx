#include "Macros.fxh"

Texture2D Texture : register(t0);
Texture2D NormalMap : register(t1);
SamplerState TextureSampler;

cbuffer cbConstant : register(c0)
{
	float4 Color;
};

cbuffer cbPerLight : register(c1)
{	
	float4x4 World;
	float4x4 ViewProjection;
	float4x4 WorldViewProjection;
	float3 LightColor;
	float LightIntensity;
	float SpecularIntensity;

	float3 LightPosition;
	float ScreenWidth;
	float ScreenHeight;
};

cbuffer cbPerSpotlight : register(c6)
{
	float ConeHalfAngle;
	float ConeDecay;
};

struct VertexIn
{
	float2 Position : SV_POSITION0;
	float2 TexCoord : TEXCOORD0;
};

struct VertexOut
{
	float4 Position : SV_POSITION;
	//float3 PosW : TEXCOORD1;
	float3 NormalW : TEXCOORD1;
	float2 PosC : TEXCOORD2;
	float2 TexCoord : TEXCOORD0;
};

VertexOut VS(VertexIn vin)
{
	VertexOut vout;
	
	vout.Position = mul(float4(vin.Position.x, vin.Position.y, 0.0, 1.0), WorldViewProjection);	
	vout.PosC = vout.Position.xy / vout.Position.w;

	float4 posW = mul(float4(vin.Position.x, vin.Position.y, 0.0, 1.0), World);

	//vout.PosW = mul(float4(vin.Position.x, vin.Position.y, 0.0, 1.0), World).xyz;
	vout.NormalW = LightPosition - float3(posW.x, posW.y, 0);
	vout.TexCoord = vin.TexCoord;

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

float4 PSPointLightNormalMapped(VertexOut pin) : SV_TARGET
{
	float halfMagnitude = length(pin.TexCoord - float2(0.5, 0.5));
	float attenuation = saturate(1.0 - halfMagnitude * 2.0);

	float2 clipPosition = pin.PosC;

	// Bring from clip space to fullscreen texture coord space.
	float2 normalMapTexCoord = float2(
		clipPosition.x*0.5 + 0.5,
		-clipPosition.y*0.5 + 0.5);
	
	float3 surfaceNormal = 2.0 * NormalMap.Sample(TextureSampler, normalMapTexCoord) - 1.0f;

	float3 lightNormal = normalize(pin.NormalW);
	float3 halfVec = float3(0, 0, 1);

	float amount = max(dot(surfaceNormal, lightNormal), 0);	
				
	//float3 reflectNormal = reflect(lightNormal, surfaceNormal); 
	float3 reflectNormal = normalize(lightNormal - 2 * surfaceNormal * amount);
	float specular = min(pow(saturate(dot(reflectNormal, halfVec)), 10), amount);	
	float3 finalLight = attenuation * LightColor * LightIntensity * amount; //+ specular * attenuation * SpecularIntensity;

	return float4(finalLight, 1.0f);
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
	float alpha = Texture.Sample(TextureSampler, pin.TexCoord).x;
	
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
TECHNIQUE(PointLightNormalMapped, VS, PSPointLightNormalMapped);
TECHNIQUE(Spotlight, VS, PSSpotLight);
TECHNIQUE(TexturedLight, VS, PSTexturedLight);
TECHNIQUE(DebugLight, VS, PSDebugLight);
