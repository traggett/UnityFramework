#ifndef MESH_VERTEX_BILLBOARDS_INCLUDED
#define MESH_VERTEX_BILLBOARDS_INCLUDED

#include "UnityCG.cginc"
#include "MeshVertexRendering.cginc"

#if (SHADER_TARGET < 30 || SHADER_API_GLES)
uniform fixed4 _Color;
#else
UNITY_INSTANCING_BUFFER_START(ColorProps)
	UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)
#define _color_arr ColorProps
UNITY_INSTANCING_BUFFER_END(ColorProps)
#endif


////////////////////////////////////////
// Vertex structs
//
				
struct VertexInput
{
	float4 vertex : POSITION;
	float4 texcoord : TEXCOORD0;
	fixed4 color : COLOR;
	
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput
{
	float4 pos : SV_POSITION;
	fixed4 color : COLOR;
	float2 texcoord : TEXCOORD0;
	float3 posWorld : TEXCOORD1;

	UNITY_VERTEX_OUTPUT_STEREO
};


////////////////////////////////////////
// Vertex program
//

uniform sampler2D _MainTex;
uniform fixed4 _MainTex_ST;
uniform float _QuadSize;

VertexOutput vert(VertexInput v)
{
	VertexOutput output;
	
	UNITY_SETUP_INSTANCE_ID(v);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
	
	float3 worldPos = getMeshVertexWorldPos(v.vertex);
	worldPos = getMeshVertexBillboardedPos(v.vertex, worldPos, _QuadSize);
	
	output.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
	output.posWorld = worldPos;
	
	output.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

#if (SHADER_TARGET < 30 || SHADER_API_GLES)
	output.color = v.color * _Color;
#else
	output.color = v.color * UNITY_ACCESS_INSTANCED_PROP(_color_arr, _Color);
#endif

	return output;
}

////////////////////////////////////////
// Fragment program
//

fixed4 frag(VertexOutput input) : SV_Target
{
	fixed4 pixel = tex2D(_MainTex, input.texcoord);
	
	pixel.a = pixel.a * input.color.a;
	pixel.rgb = (pixel.rgb * input.color.rgb) * pixel.a;
	
	return pixel;
}

#endif // MESH_VERTEX_BILLBOARDS_INCLUDED