Texture2D Texture;
SamplerState TextureSampler;

cbuffer cbConstant
{
	float4 Color;
};

cbuffer cbPerObject
{	
	float4x4 WorldViewProjection;
	float3 LightColor;
	float Intensity;
};

cbuffer cbPerObject2
{
	float4x4 TextureTransform;
}

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

VertexOut VSPointLight(VertexIn vin)
{
	VertexOut vout;
	
	vout.Position = mul(float4(vin.Position.x, vin.Position.y, 0, 1), WorldViewProjection);	
	vout.TexCoord = vin.TexCoord;

	return vout;
}

VertexOut VSTexturedLight(VertexIn vin)
{
	VertexOut vout;

	vout.Position = mul(float4(vin.Position.x, vin.Position.y, 0, 1), WorldViewProjection);	
	vout.TexCoord = mul(float4(vin.TexCoord, 0.0f, 1.0f), TextureTransform).xy;

	return vout;
}

float GetAlphaAtTexCoord(float2 texCoord)
{
	// Point light linear attenuation.
	float len = length(texCoord - float2(0.5, 0.5));
	return saturate(1 - len * 2);
}

float4 GetComputedColor(float4 alpha)
{
	float4 lightColor = float4(LightColor, 1);
	return pow(abs(alpha) * lightColor, Intensity);
}

float4 PSPointLight(VertexOut pin) : SV_TARGET
{
	float alpha = GetAlphaAtTexCoord(pin.TexCoord);	
	return GetComputedColor(float4(alpha, alpha, alpha, 1));
}

float4 PSTexturedLight(VertexOut pin) : SV_TARGET
{
	return GetComputedColor(Texture.Sample(TextureSampler, pin.TexCoord));	
}

float4 PSDebugLight(VertexOut pin) : SV_TARGET
{
	return Color;
}

technique PointLight
{
	pass P0
	{		
		VertexShader = compile vs_4_0_level_9_1 VSPointLight();
		PixelShader = compile ps_4_0_level_9_1 PSPointLight();
	}
}

technique TexturedLight
{
	pass P0
	{
		VertexShader = compile vs_4_0_level_9_1 VSTexturedLight();
		PixelShader = compile ps_4_0_level_9_1 PSTexturedLight();
	}
}

technique DebugLight
{
	pass P0
	{
		VertexShader = compile vs_4_0_level_9_1 VSPointLight();
		PixelShader = compile ps_4_0_level_9_1 PSDebugLight();
	}
}