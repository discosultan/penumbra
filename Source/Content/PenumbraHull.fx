#include "Macros.fxh"

cbuffer cbPerObject
{
	float4 Color;
};

cbuffer cbPerFrame
{
	float4x4 ViewProjection;
};

struct VertexIn
{
	float2 Position : SV_POSITION0;
};

struct VertexOut
{
	float4 Position : SV_POSITION;
};

VertexOut VS(VertexIn vin)
{
	VertexOut vout;

	vout.Position = mul(float4(vin.Position.x, vin.Position.y, 0.0, 1.0), ViewProjection);

	return vout;
}

float4 PS(VertexOut pin) : SV_TARGET
{
	return Color;
}

TECHNIQUE(Main, VS, PS);
