cbuffer cbPerObject
{
	float4 Color;
};

cbuffer cbPerFrame
{
	float4x4 ProjectionTransform;
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

	vout.Position = mul(float4(vin.Position.x, vin.Position.y, 0.0f, 1.0f), ProjectionTransform);

	return vout;
}

float4 PS(VertexOut pin) : SV_TARGET
{
	return Color;
}

technique Debug
{
	pass P0
	{		
		VertexShader = compile vs_4_0_level_9_1 VS();
		PixelShader = compile ps_4_0_level_9_1 PS();
	}
}
