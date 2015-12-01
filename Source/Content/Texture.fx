#include "Macros.fxh"

Texture2D DiffuseMap : register(t0);
Texture2D Lightmap : register(t1);
SamplerState TextureSampler : register(s0);

struct VertexOut
{
	float4 Position : SV_POSITION0;
	float2 TexCoord : TEXCOORD0;
};

VertexOut VS(float2 posC : SV_POSITION0, float2 texCoord : TEXCOORD0)
{
	VertexOut vout;

	vout.Position = float4(posC.x, posC.y, 0.0, 1.0);
	vout.TexCoord = texCoord;

	return vout;
}

float4 PS(VertexOut pin) : SV_TARGET
{
	float4 diffuse = DiffuseMap.Sample(TextureSampler, pin.TexCoord);
	float4 light = Lightmap.Sample(TextureSampler, pin.TexCoord);
	return diffuse * light;
}

TECHNIQUE(Main, VS, PS);
