cbuffer cbPerObject
{
	float4x4 WVP;
};

struct VertexIn
{
	float3 occluderCoord_radius : SV_POSITION0;
	float2 segmentA_soften : TEXCOORD0;
	float2 segmentB : TEXCOORD1;
};

struct VertexOut
{
	float4 position : SV_POSITION;
	float4 penumbra : TEXCOORD1;
	float clipValue : TEXCOORD2;
};

float2x2 penumbraMatrix(float2 basisX, float2 basisY) {
	//float2x2 m = transpose(float2x2(basisX, basisY));
	float2x2 m = float2x2(basisX, basisY);
	return float2x2(m._m11, -m._m01, -m._m10, m._m00) / determinant(m);
}

VertexOut VS(VertexIn vin)
{
	float2 occluderCoord = vin.occluderCoord_radius.xy;	
	// Ensure radius never reaches 0.
	float radius = max(1e-5, vin.occluderCoord_radius.z);	

	float2 segmentA = vin.segmentA_soften.xy;
	float2 segmentB = vin.segmentB;

	// Derived values.
	float2 lightOffsetA = float2(-radius, radius)*normalize(segmentA).yx;
	float2 lightOffsetB = float2(radius, -radius)*normalize(segmentB).yx;

	float2 position = lerp(segmentA, segmentB, occluderCoord.x);
	float2 projectionOffset = lerp(lightOffsetA, lightOffsetB, occluderCoord.x);
	float4 projected = float4(position - projectionOffset*occluderCoord.y, 0.0, 1.0 - occluderCoord.y);

	// Output values	
	float4 clipPosition = mul(projected, WVP);
	
	float2 penumbraA = mul(projected.xy - segmentA*projected.w, penumbraMatrix(lightOffsetA, segmentA));
	float2 penumbraB = mul(projected.xy - segmentB*projected.w, penumbraMatrix(lightOffsetB, segmentB));

	float2 clipNormal = normalize(segmentB - segmentA).yx*float2(-1.0, 1.0);
	float clipValue = dot(clipNormal, projected.xy - projected.w*position);
	
	VertexOut vout;
	vout.position = clipPosition;
	vout.penumbra = float4(penumbraA, penumbraB);
	vout.clipValue = clipValue;

	return vout;
}

float4 PS(VertexOut pin) : SV_TARGET
{
	float2 p = clamp(pin.penumbra.xz / pin.penumbra.yw, -1.0, 1.0);
	//float2 value = lerp(p*(3.0 - p*p)*0.25 + 0.5, 1.0, step(pin.penumbra.yw, 0.0));
	float2 value = lerp(p*(3.0 - p*p)*0.25 + 0.5, 1.0, step(pin.penumbra.yw, 0.0));
	float occlusion = (value[0] + value[1] - 1.0);

	return float4(0.0, 0.0, 0.0, occlusion*step(pin.clipValue, 0.0));
}

technique Main
{
	pass P0
	{
		VertexShader = compile vs_4_0_level_9_1 VS();
		PixelShader = compile ps_4_0_level_9_1 PS();
	}
}
