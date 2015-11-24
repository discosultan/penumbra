#include "Macros.fxh"

struct Light
{
	float3 Position;
	float3 Color;
	float Intensity;
};

struct VertexOut
{
	float4 PosN : SV_POSITION;
	float2 PosC : TEXCOORD0;
	float3 NormalW : TEXCOORD1;	
	float2 TexCoord : TEXCOORD2;
};

Texture2D Texture : register(t0);
Texture2D NormalMap : register(t1);
SamplerState TextureSampler;

cbuffer cbPerLight : register(c0)
{	
	float4x4 World;
	float4x4 ViewProjection;
	float4x4 WorldViewProjection;
	
	Light Light;

	float SpecularIntensity;
};

VertexOut VS(float2 posL : SV_POSITION, float2 texCoord)
{
	VertexOut vout;
	
	vout.PosN = mul(float4(posL.x, posL.y, 0.0, 1.0), WorldViewProjection);
	vout.PosC = vout.Position.xy / vout.Position.w;
	
	vout.NormalW = LightPosition - float3(vin.Position.x, vin.Position.y, 0);		
	vout.TexCoord = vin.TexCoord;

	return vout;
}

float4 PSPointLight(VertexOut pin) : SV_TARGET
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
				
	float3 reflectNormal = reflect(lightNormal, surfaceNormal); 
	//float3 reflect = normalize(lightNormal - 2 * surfaceNormal * amount);
	float specular = min(pow(saturate(dot(reflectNormal, halfVec)), 10), amount);	
	float3 finalLight = attenuation * LightColor * LightIntensity + specular * attenuation * SpecularIntensity;

	return float4(finalLight, 1.0f);
}

TECHNIQUE(PointLight, VS, PSPointLight);
