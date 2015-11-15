#include "Macros.fxh"

Texture2D Texture : register(t0);
//Texture2D NormalMap : register(t1);
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

cbuffer cbPerNormalLight : register(c7)
{
	float3 LightPosition;
	float4x4 World;
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

struct VertexOutNormal
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;	
	float3 Normal : TEXCOORD1;	
};

struct PixelOut
{
	float4 Color : COLOR0;
	float4 Normal : COLOR1;
};

VertexOut VS(VertexIn vin)
{
	VertexOut vout;
		
	vout.Position = mul(float4(vin.Position.x, vin.Position.y, 0.0, 1.0), WorldViewProjection);	
	vout.TexCoord = vin.TexCoord;	

	return vout;
}

VertexOutNormal VSNormal(VertexIn vin)
{
	VertexOutNormal vout;

	float4 posW = mul(float4(vin.Position.x, vin.Position.y, 0.0, 1.0), World);
	vout.Position = mul(float4(vin.Position.x, vin.Position.y, 0.0, 1.0), WorldViewProjection);
	vout.TexCoord = vin.TexCoord;
	vout.Normal = normalize(LightPosition - float3(posW.xy, 0.0));	 // TODO: normalize only in ps

	return vout;
}

float4 GetComputedColor(float alpha)
{
	alpha = abs(alpha);
	float3 lightColor = LightColor * alpha;
	lightColor = pow(lightColor, LightIntensity);
	return float4(lightColor, 1.0);
}

PixelOut PSPointLightNormal(VertexOutNormal pin) : SV_TARGET
{
	float halfMagnitude = length(pin.TexCoord - float2(0.5, 0.5));
	float alpha = saturate(1.0 - halfMagnitude * 2.0);

	/*float3 normal = NormalMap.Sample(TextureSampler, pin.TexCoord);

	float lol = max(dot(pin.Normal, normal), 0);*/

	PixelOut pout;
	pout.Color = GetComputedColor(alpha);
	pout.Normal = normalize(float4(pin.Normal.xyz, 0));
	return pout;
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

TECHNIQUE(PointLightNormal, VSNormal, PSPointLightNormal);
TECHNIQUE(PointLight, VS, PSPointLight);
TECHNIQUE(Spotlight, VS, PSSpotLight);
TECHNIQUE(TexturedLight, VS, PSTexturedLight);
TECHNIQUE(DebugLight, VS, PSDebugLight);
