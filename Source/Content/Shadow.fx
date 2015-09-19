cbuffer cbConstant
{
	float4 Color; // Used in debugging.
};

cbuffer cbPerObject
{
	float4x4 WorldViewProjection;
};

struct VertexIn
{	
	float2 segmentA : TEXCOORD0;
	float2 segmentB : TEXCOORD1;
	float2 occluderCoord : SV_POSITION0;
	float radius : NORMAL0;
};

struct VertexOut
{
	float4 position : SV_POSITION;
	float4 penumbra : TEXCOORD1;
	float clipValue : TEXCOORD2;
};

float2x2 penumbraMatrix(float2 basisX, float2 basisY) 
{
	float2x2 m = float2x2(basisX, basisY);
	// Find inverse of m.
	return float2x2(m._m11, -m._m01, -m._m10, m._m00) / determinant(m);
}

VertexOut VS(VertexIn vin)
{
	float2 occluderCoord = vin.occluderCoord;	
	// Ensure radius never reaches 0.
	float radius = max(1e-5, vin.radius);	

	float2 segmentA = vin.segmentA;
	float2 segmentB = vin.segmentB;

	// Find radius offsets 90deg left and right from light source relative to vertex.
	float2 lightOffsetA = float2(-radius, radius)*normalize(segmentA).yx; // 90 CCW.
	float2 lightOffsetB = float2(radius, -radius)*normalize(segmentB).yx; // 90 CW.

	// From each edge, project a quad. 4 vertices per edge.
	float2 position = lerp(segmentA, segmentB, occluderCoord.x);
	float2 projectionOffset = lerp(lightOffsetA, lightOffsetB, occluderCoord.x);
	float4 projected = float4(position - projectionOffset*occluderCoord.y, 0.0, 1.0 - occluderCoord.y);

	// Transform to ndc.
	float4 clipPosition = mul(projected, WorldViewProjection);
	
	float2 penumbraA = mul(projected.xy - segmentA*projected.w, penumbraMatrix(lightOffsetA, segmentA));
	float2 penumbraB = mul(projected.xy - segmentB*projected.w, penumbraMatrix(lightOffsetB, segmentB));

	float2 clipNormal = normalize(segmentB - segmentA).yx*float2(-1.0, 1.0);
	// 90 CCW. ClipValue > 0 means the projection is pointing towards us => no shadow should be generated.
	float clipValue = dot(clipNormal, projected.xy - projected.w*position);
	
	VertexOut vout;
	vout.position = clipPosition;
	vout.penumbra = float4(penumbraA, penumbraB);
	vout.clipValue = clipValue;

	/*if (penumbraA.x == 0 && penumbraA.y == 0)
	{
		vout.position = float4(-1, -1, 0, 1);
	}*/

	return vout;
}

float4 PS(VertexOut pin) : SV_TARGET
{
	// If clipvalue > 0, dont shadow.
	clip(-pin.clipValue);

	float2 p = clamp(pin.penumbra.xz / pin.penumbra.yw, -1.0, 1.0);
	float2 value = lerp(p*(3.0 - p*p)*0.25 + 0.5, 1.0, step(pin.penumbra.yw, 0.0));	
	float occlusion = (value[0] + value[1] - 1.0);
	return float4(0.0, 0.0, 0.0, occlusion);
}

float4 PSDebug(VertexOut pin) : SV_TARGET
{
	clip(-pin.clipValue);
	return Color;
}

technique Main
{
	pass P0
	{
		VertexShader = compile vs_4_0_level_9_1 VS();
		PixelShader = compile ps_4_0_level_9_1 PS();
	}
}

technique Debug
{
	pass P0
	{
		VertexShader = compile vs_4_0_level_9_1 VS();
		PixelShader = compile ps_4_0_level_9_1 PSDebug();
	}
}
