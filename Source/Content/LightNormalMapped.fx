#include "Macros.fxh"

struct VertexOut
{
	float4 Position : SV_POSITION;
	float2 PosN : TEXCOORD0;
	float2 TexCoord : TEXCOORD1;
	float3 NormalW : TEXCOORD2;		
};

Texture2D LightTexture : register(t0);
Texture2D NormalMap : register(t1);
SamplerState TextureSampler : register (s0);

cbuffer cbPerFrame
{
	float4x4 ViewProjection;
	float2 Resolution;
};

cbuffer cbPerLight
{	
	float4x4 World;		
	float3 LightPosition;
	float LightIntensity;
	float3 LightColor;
	float SpecularIntensity;
};

VertexOut VS(float2 posL : SV_POSITION0, float2 texCoord : TEXCOORD0)
{
	VertexOut vout;
	
	float3 posW = mul(float3(posL, 0.0), World);		

	vout.NormalW = LightPosition - posW;	
	vout.NormalW.z=-vout.NormalW.z;

	vout.Position = mul(float4(posW, 1.0), ViewProjection);
	vout.PosN = vout.Position.xy; // / vout.Position.w;		
	
	vout.TexCoord = texCoord;

	return vout;
}

float4 PSPointLight(VertexOut pin) : SV_TARGET
{
	float2 clipPosition = pin.PosN;	
	float3 lightDir = normalize(pin.NormalW);

	//float halfMagnitude = length(pin.TexCoord - float2(0.5, 0.5));
	//float distanceAttenuation = saturate(1.0 - halfMagnitude * 2.0);	

	// Bring from clip space to fullscreen texture coord space.
	float2 normalMapTexCoord = float2(
		clipPosition.x*0.5 + 0.5,
		-clipPosition.y*0.5 + 0.5);

	//float2 normalMapTexCoord = screenPos / Resolution;
	
	// Sample the normal and convert from [0..1] to [-1..1].
	float3 surfaceNormal = normalize(2.0*NormalMap.Sample(TextureSampler, normalMapTexCoord) - 1.0);
	//surfaceNormal = float3(0,0,1);

	//lightDir = float3(0, 0, 1);

	float3 diffuse = max(dot(lightDir, surfaceNormal), 0.0) * LightColor;
		
	//float3 viewDir = float3(0, 0, 1);	

	//float3 halfwayDir = normalize(lightDir + viewDir);
	
	//float specular = pow(saturate(dot(surfaceNormal, halfwayDir)), SpecularIntensity);	

	//float3 final = diffuse + specular;

	//float amount = ax(dot(surfaceNormal, lightNormal), 0);	
				
	//float3 reflectNormal = reflect(lightNormal, surfaceNormal); 
	//float3 reflect = normalize(lightNormal - 2 * surfaceNormal * amount);
	//float specular = min(pow(saturate(dot(reflectNormal, halfVec)), 10), amount);	
	//float3 finalLight = attenuation * LightColor * LightIntensity + specular * attenuation * SpecularIntensity;

	return float4(diffuse /*+ specular*/, 1.0f);
}

float4 PSDebugLightNormals(VertexOut pin) : SV_TARGET
{
	float3 lightDir = normalize(pin.NormalW);
	float3 color = lightDir*0.5 + 0.5;
	return float4(color, 1.0);
}

technique PointLight
{ 
	pass 
	{ 
		VertexShader = compile vs_4_0_level_9_1 VS();
		PixelShader = compile ps_4_0_level_9_1 PSPointLight();
	} 
}

technique Debug
{ 
	pass 
	{ 
		VertexShader = compile vs_4_0_level_9_1 VS();
		PixelShader = compile ps_4_0_level_9_1 PSDebugLightNormals();
	} 
}

//TECHNIQUE(PointLight, VS, PSPointLight);
