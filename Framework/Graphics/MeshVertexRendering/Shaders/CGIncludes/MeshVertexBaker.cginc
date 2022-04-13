#ifndef MESH_VERTEX_BAKING_INCLUDED
#define MESH_VERTEX_BAKING_INCLUDED

////////////////////////////////////////
// Vertex structs
//

struct VertexInput
{
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;
};

struct VertexOutput
{
	float4 pos : SV_POSITION;
	float3 posWorld : TEXCOORD0;
};


////////////////////////////////////////
// Vertex program
//

VertexOutput vert(VertexInput v)
{
    VertexOutput o;

	//Convert from render target UV to clip space
	o.pos.x = (v.texcoord.x * 2) - 1;
	o.pos.y = (v.texcoord.y * 2) - 1;
	o.pos.z = 0;
	o.pos.w = 1;
	
    o.posWorld = mul(unity_ObjectToWorld, v.vertex).xyz;
	
    return o;
}


////////////////////////////////////////
// Fragment program
//

float4 frag(VertexOutput i) : SV_Target
{
    return float4(i.posWorld, 1);
}

#endif // MESH_VERTEX_BAKING_INCLUDED