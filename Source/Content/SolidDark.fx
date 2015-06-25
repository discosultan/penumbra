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

	vout.Position = float4(vin.Position.x, vin.Position.y, 0.0f, 1.0f);

	return vout;
}

float4 PS(VertexOut pin) : SV_TARGET
{
	return float4(0, 0 , 0, 0);
}

technique SolidDark
{
	pass P0
	{		
		VertexShader = compile vs_4_0_level_9_1 VS();
		PixelShader = compile ps_4_0_level_9_1 PS();
	}
}
