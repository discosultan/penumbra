cbuffer cbPerFrame
{
	float4x4 ProjectionTransform;
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
	
	vout.Position = mul(float4(vin.Position.x, vin.Position.y, 0, 1), ProjectionTransform);
	vout.TexCoord = vin.TexCoord;

	return vout;
}

float4 PS(VertexOut pin) : SV_TARGET
{
	// Ref: http://stackoverflow.com/a/28667361/1466456
	//float alpha = 1 - pow(pin.TexCoord.x / (1 - pin.TexCoord.y), 2);
	float alpha = 1 - pow(pin.TexCoord.x / (1 - pin.TexCoord.y), 4);	
	//float alpha = pow(pin.TexCoord.x / (1 - pin.TexCoord.y), 4);
	return float4(0,0,0,alpha);
}

technique Main
{
	pass P0
	{		
		VertexShader = compile vs_4_0_level_9_1 VS();
		PixelShader = compile ps_4_0_level_9_1 PS();
	}
}
