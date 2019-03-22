#ifndef MESH_VERTEX_RENDERING_INCLUDED
#define MESH_VERTEX_RENDERING_INCLUDED

uniform sampler2D _VertexPositions;
uniform float _VertexPositionsSize;

float3 getMeshVertexWorldPos(float4 vertex)
{
	//Vertex index is z value of vert
	float vertIndex = vertex.z;
	
	//Work out UV from texture
	float pixelY = floor(vertIndex / _VertexPositionsSize);
	float pixelX = vertIndex - (pixelY * _VertexPositionsSize);
	
	float4 uvs = float4((pixelX + 0.5) / _VertexPositionsSize, 1.0 - ((pixelY + 0.5) / _VertexPositionsSize), 0, 0);
	float3 worldPos = tex2Dlod(_VertexPositions, uvs).xyz;
	
	return worldPos;
}

float3 getMeshVertexBillboardedPos(float4 vertex, float3 worldPos, float size)
{
	const float3 cameraUp = UNITY_MATRIX_V[1].xyz;
	
	const float3 forward = normalize(_WorldSpaceCameraPos - worldPos);
	const float3 right = normalize(cross(forward, cameraUp));
	const float3 up = cross(right, forward);
	
	worldPos += up * vertex.y * size;
	worldPos += right * vertex.x * size;
	
	return worldPos;
}

#endif // MESH_VERTEX_RENDERING_INCLUDED