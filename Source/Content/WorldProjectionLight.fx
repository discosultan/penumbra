cbuffer cbPerObject
{
	float4x4 World;
};

cbuffer cbPerLight
{
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
	vout.TexCoord = vin.TexCoord;

	return vout;
}

float GetAlphaAtTexCoord(float2 texCoord)
{
	float len = length(texCoord - float2(0.5, 0.5));
	return saturate(1 - len * 2);
}

float4 PS(VertexOut pin) : SV_TARGET
{
	float alpha = GetAlphaAtTexCoord(pin.TexCoord);
	float4 color = float4(alpha, alpha, alpha, 1);
	float4 lightColor = float4(Color.x, Color.y, Color.z, 1);
	return pow(abs(color) * lightColor, Intensity);
}

technique Main
{
	pass P0
	{		
		VertexShader = compile vs_4_0_level_9_1 VS();
		PixelShader = compile ps_4_0_level_9_1 PS();
	}
}
