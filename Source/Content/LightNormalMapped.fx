#include "Macros.fxh"

struct VertexOut
{
	float4 Position : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
	float2 PosN : TEXCOORD1;	
	float3 NormalW : TEXCOORD2;	
};

Texture2D LightTexture;
Texture2D NormalMap;
SamplerState TextureSampler;

cbuffer cbPerFrame
{
	//float4x4 ViewProjection;
	//float2 Resolution;
};

cbuffer cbPerLight
{	
	float4x4 WorldViewProjection;		
	float4x4 World;
	float3 LightPosition;
	float LightIntensity;
	float3 LightColor;
	//float SpecularIntensity;
};

VertexOut VS(float2 posL : SV_POSITION0, float2 texCoord : TEXCOORD0)
{
	VertexOut vout;
	
	//float3 posW = mul(float3(posL.x, posL.y, 0.0), World);
	//vout.NormalW = normalize(LightPosition - posW);

	vout.Position = mul(float4(posL.x, posL.y, 0.0, 1.0), WorldViewProjection);		
	vout.TexCoord = texCoord;		

	float2 posW = mul(float4(posL.x, posL.y, 0.0, 1.0), World).xy;	
	vout.NormalW.xy = LightPosition.xy - posW; // TODO: InvertY if req
	vout.NormalW.z = LightPosition.z;
	//vout.NormalW = normalize(vout.NormalW);

	//vout.Position = mul(float4(posL.x, posL.y, 0.0, 1.0), mul(World, ViewProjection));
	vout.PosN = vout.Position.xy / vout.Position.w; /// vout.Position			

	return vout;
}

float4 PSPointLight(VertexOut pin) : SV_TARGET
{
	// CALCULATE DIFFUSE

	float3 lightDir = normalize(pin.NormalW);	

	// Bring from clip space to fullscreen texture coord space.
	float2 normalMapTexCoord = float2(pin.PosN.x*0.5 + 0.5, -pin.PosN.y*0.5 + 0.5);
	//float2 normalMapTexCoord = screenPos / Resolution;
	
	// Sample the normal and convert from [0..1] to [-1..1].
	float3 surfaceNormal = normalize(2.0*NormalMap.Sample(TextureSampler, normalMapTexCoord) - 1.0);	

	float3 diffuse = max(dot(lightDir, surfaceNormal), 0.0) * LightColor;
	//float3 diffuse = lightDir;


	// CALCULATE ATTENUATION

	float3 falloff = float3(0.14, 3, 20);

	float magnitude = length(pin.TexCoord - float2(0.5, 0.5)) * 2.0;
	//float distanceAttenuation = max(1.0 - magnitude, 0.0);

	//float distanceAttenuation = 1.0 / (  falloff.x + ( falloff.y*magnitude) + ( falloff.z*magnitude*magnitude) );
	float distanceAttenuation = clamp(1.0 - magnitude*magnitude, 0.0, 1.0);
	distanceAttenuation *= distanceAttenuation;

	//distanceAttenuation = abs(distanceAttenuation);
	//float3 intensity = distanceAttenuation;
	//float3 lightColor = LightColor * distanceAttenuation;
	//return float4(lightColor, 1.0);

	//float3 viewDir = float3(0, 0, 1);	

	//float3 halfwayDir = normalize(lightDir + viewDir);
	
	//float specular = pow(saturate(dot(surfaceNormal, halfwayDir)), SpecularIntensity);	

	//float3 final = diffuse + specular;

	//float amount = max(dot(surfaceNormal, lightNormal), 0);	
				
	//float3 reflectNormal = reflect(lightNormal, surfaceNormal); 
	//float3 reflect = normalize(lightNormal - 2 * surfaceNormal * amount);
	//float specular = min(pow(saturate(dot(reflectNormal, halfVec)), 10), amount);	
	//float3 finalLight = attenuation * LightColor * LightIntensity + specular * attenuation * SpecularIntensity;

	float3 intensity = diffuse * LightIntensity * distanceAttenuation;

	return float4(intensity /*+ specular*/, 1.0f);
}

//float4 PSDebugLightNormals(VertexOut pin) : SV_TARGET
//{
//	//float3 lightDir = normalize(LightPosition - float3(pin.PosW, 0.0));
//	float3 lightDir = normalize(pin.NormalW);
//	float3 color = lightDir*0.5 + 0.5;
//	return float4(color, 1.0);
//}

technique PointLight
{ 
	pass 
	{ 
		VertexShader = compile vs_4_0_level_9_1 VS();
		PixelShader = compile ps_4_0_level_9_1 PSPointLight();
	} 
}

//technique DebugNormals
//{ 
//	pass 
//	{ 
//		VertexShader = compile vs_4_0_level_9_1 VS();
//		PixelShader = compile ps_4_0_level_9_1 PSDebugLightNormals();
//	} 
//}

//TECHNIQUE(PointLight, VS, PSPointLight);
