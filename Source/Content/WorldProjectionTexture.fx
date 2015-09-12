Texture2D Texture;
SamplerState TextureSampler;

cbuffer cbPerLight
{
	float4x4 TextureTransform;
	float4x4 World;	
	float3 Color;
	float Intensity;
};

cbuffer cbPerFrame
{
	float4x4 ViewProjection;
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

VertexOut VS(VertexIn vin)
{
	VertexOut vout;
	
	float4 posW = mul(float4(vin.Position.x, vin.Position.y, 0, 1), World);
	vout.Position = mul(posW, ViewProjection);
	vout.TexCoord = mul(float4(vin.TexCoord, 0.0f, 1.0f), TextureTransform).xy;

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
