#include "Macros.fxh"

cbuffer cbPerFrame
{
	float4x4 ViewProjection;
};

cbuffer cbPerObject
{
	float4 Color;
};

struct VertexOut
{
	float4 Position : SV_POSITION0;
};

VertexOut VS(float2 posW : SV_POSITION0)
{
	VertexOut vout;
	vout.Position = mul(float4(posW.x, posW.y, 0.0, 1.0), ViewProjection);
	return vout;
}

float4 PS(VertexOut pin) : SV_TARGET
{
	return Color;
}

TECHNIQUE(Main, VS, PS);
