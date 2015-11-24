Texture2D Texture : register(t0);
SamplerState TextureSampler;

cbuffer cbPerObject 
{
	float4x4 Transform;
};

struct VertexIn
{
	float3 Position : SV_POSITION0;
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

	vout.Position = mul(float4(vin.Position.x, vin.Position.y, vin.Position.z, 1.0), Transform);
	vout.TexCoord = vin.TexCoord;

	return vout;
}

float4 PS(VertexOut pin) : SV_TARGET
{
	return Texture.Sample(TextureSampler, pin.TexCoord);	
}

technique Main
{
	pass P0
	{
		VertexShader = compile vs_4_0_level_9_1 VS();
		PixelShader = compile ps_4_0_level_9_1 PS();
	}
}
