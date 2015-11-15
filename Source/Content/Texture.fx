#include "Macros.fxh"

Texture2D Texture;
SamplerState TextureSampler;

Texture2D NormalMap1;
Texture2D NormalMap2;

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

VertexOut VS(VertexIn vin)
{
	VertexOut vout;

	vout.Position = float4(vin.Position.x, vin.Position.y, 0.0, 1.0);
	vout.TexCoord = vin.TexCoord;

	return vout;
}

float4 PS(VertexOut pin) : SV_TARGET
{
	return Texture.Sample(TextureSampler, pin.TexCoord);
}

float4 PSNormal(VertexOut pin) : SV_TARGET
{
	float4 normal1 = normalize(NormalMap1.Sample(TextureSampler, pin.TexCoord));
	float4 normal2 = normalize(NormalMap2.Sample(TextureSampler, pin.TexCoord));
	float dotProduct = dot(normal1, normal2);
	return Texture.Sample(TextureSampler, pin.TexCoord) * dotProduct;	
}

TECHNIQUE(Main, VS, PS);
TECHNIQUE(Normal, VS, PSNormal);
