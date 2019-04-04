#include "Macros.fxh"

Texture2D DiffuseMap;
Texture2D Lightmap;
SamplerState TextureSampler;

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
	float4 diffuse = DiffuseMap.Sample(TextureSampler, pin.TexCoord);
	float4 light = Lightmap.Sample(TextureSampler, pin.TexCoord);
	return diffuse * light;
}

TECHNIQUE(Main, VS, PS);
