#ifndef GPU_ANIMATION_INCLUDED
#define GPU_ANIMATION_INCLUDED

#if !defined(_GPU_ANIMATION)

#define GPU_ANIMATION_DATA(boneIdx, boneWeightIdx)
#define GPU_ANIMATE(input)

#else // _GPU_ANIMATION

//Dont bother with normals / tangents during a shadowcaster pass
#if defined(UNITY_PASS_SHADOWCASTER)
#define _SKIN_POS_ONLY
#endif

uniform int _boneCount;
uniform sampler2D _animationTexture;
uniform int _animationTextureWidth;
uniform int _animationTextureHeight;

#if (SHADER_TARGET < 30 || SHADER_API_GLES)

uniform float _mainAnimationFrame;
uniform float _mainAnimationWeight;
uniform float _backgroundAnimationFrame;

uniform float _layerTwoWeight;
uniform float _layerTwoMainAnimFrame;
uniform float _layerTwoMainAnimWeight;
uniform float _layerTwoBackgroundAnim;

#else

UNITY_INSTANCING_BUFFER_START(Props)

	UNITY_DEFINE_INSTANCED_PROP(float, _mainAnimationFrame)
#define _mainAnimationFrame_arr Props
	UNITY_DEFINE_INSTANCED_PROP(float, _mainAnimationWeight)
#define _mainAnimationWeight_arr Props
	UNITY_DEFINE_INSTANCED_PROP(float, _backgroundAnimationFrame)
#define _backgroundAnimationFrame_arr Props

	UNITY_DEFINE_INSTANCED_PROP(float, _layerTwoWeight)
#define _layerTwoWeight_arr Props
	UNITY_DEFINE_INSTANCED_PROP(float, _layerTwoMainAnimFrame)
#define _layerTwoMainAnimFrame_arr Props
	UNITY_DEFINE_INSTANCED_PROP(float, _layerTwoMainAnimWeight)
#define _layerTwoMainAnimWeight_arr Props
	UNITY_DEFINE_INSTANCED_PROP(float, _layerTwoBackgroundAnim)
#define _layerTwoBackgroundAnim_arr Props


UNITY_INSTANCING_BUFFER_END(Props)
#endif

//fixed number of pixels 
static const uint pixelsPerFrame = 4;

half4x4 readBoneMatrixFromTexture(float4 uv, float boneIndex)
{
	//Shift UVs down for correct bone
	uv.y += boneIndex / _animationTextureHeight;

	float pixelUVWidth = 1.0f / _animationTextureWidth;
	half4 c1 = tex2Dlod(_animationTexture, uv);
	uv.x += pixelUVWidth;
	half4 c2 = tex2Dlod(_animationTexture, uv);
	uv.x += pixelUVWidth;
	half4 c3 = tex2Dlod(_animationTexture, uv);
	
	//uv.x += pixelUVWidth;
	//half4 c4 = tex2Dlod(_animationTexture, uv);
	
	//dont bother with final row of matrix as its always the same
	half4 c4 = half4(0, 0, 0, 1);
	
	half4x4 m;
	m._11_21_31_41 = c1;
	m._12_22_32_42 = c2;
	m._13_23_33_43 = c3;
	m._14_24_34_44 = c4;
	return m;
}

half4x4 calcVertexMatrix(float4 boneIDs, float4 boneWeights, int frame)
{
	//how many frames per row? (TO DO: could be fixed const set on shader)
	uint framesPerRow = _animationTextureWidth / pixelsPerFrame;	

	//what row is our frame?
	int row = frame / framesPerRow;
	
	//what col is our frame?
	int col = frame - (row * framesPerRow);

	//Work out pixel coords for the frame
	float2 framePixel;
	framePixel.x = (col * pixelsPerFrame) + 0.5;
	framePixel.y = (row * _boneCount) + 0.5;

	//Work out uvs
	float4 uv;
	uv.x = framePixel.x / _animationTextureWidth;
	uv.y = framePixel.y / _animationTextureHeight;
	uv.z = 0;
	uv.w = 0;

	half4x4 localToWorldMatrix = readBoneMatrixFromTexture(uv, boneIDs.x) * boneWeights.x;
	if (boneWeights.y > 0.0f)
		localToWorldMatrix = localToWorldMatrix + readBoneMatrixFromTexture(uv, boneIDs.y) * boneWeights.y;
	if (boneWeights.z > 0.0f)
		localToWorldMatrix = localToWorldMatrix + readBoneMatrixFromTexture(uv, boneIDs.z) * boneWeights.z;
	if (boneWeights.w > 0.0f)
		localToWorldMatrix = localToWorldMatrix + readBoneMatrixFromTexture(uv, boneIDs.w) * boneWeights.w;
		
	return localToWorldMatrix;
}

void calcVertexFromAnimation(float4 boneIDs, float4 boneWeights, float curFrame, inout half4 vertex, inout half3 normal, inout half3 tangent)
{
	int prevFrame = floor(curFrame);
	int nextFrame = prevFrame + 1;
	float frameLerp = curFrame - prevFrame;
	
	half4x4 localToWorldMatrixPre = calcVertexMatrix(boneIDs, boneWeights, prevFrame);
	half4x4 localToWorldMatrixNext = calcVertexMatrix(boneIDs, boneWeights, nextFrame);

	//Work out vertex pos
	half4 localPosPrev = mul(vertex, localToWorldMatrixPre);
	half4 localPosNext = mul(vertex, localToWorldMatrixNext);
	vertex = lerp(localPosPrev, localPosNext, frameLerp);
	
#if !defined(_SKIN_POS_ONLY)
	//Work out normal
	half3 localNormPrev = mul(normal, (float3x3)localToWorldMatrixPre);
	half3 localNormNext = mul(normal, (float3x3)localToWorldMatrixNext);
	normal = lerp(localNormPrev, localNormNext, frameLerp);
	
	//Work out tangent
	half3 localTanPrev = mul(tangent, (float3x3)localToWorldMatrixPre);
	half3 localTanNext = mul(tangent, (float3x3)localToWorldMatrixNext);
	tangent = lerp(localTanPrev, localTanNext, frameLerp);
#endif
}


void calcBlendedAnimations(float4 boneIDs, float4 boneWeights, float mainAnimFrame, float mainAnimWeight, float backgroundAnimFrame, inout half4 vertex, inout half3 normal, inout half4 tangent)
{
	//Find vertex position for currently playing animation
	half4 curAnimVertex = vertex;
	half3 curAnimNormal = normal;
	half3 curAnimTangent = tangent.xyz;
	
	calcVertexFromAnimation(boneIDs, boneWeights, mainAnimFrame, curAnimVertex, curAnimNormal, curAnimTangent);	

	//Find vertex position for previous animation if blending on top of it and lerp current one on top
	if (mainAnimWeight < 1.0)
	{
		half4 prevAnimVertex = vertex;
		half3 prevAnimNormal = normal;
		half3 prevAnimTangent = tangent;
		calcVertexFromAnimation(boneIDs, boneWeights, backgroundAnimFrame, prevAnimVertex, prevAnimNormal, prevAnimTangent);
		
		curAnimVertex = lerp(prevAnimVertex, curAnimVertex, mainAnimWeight);
		
#if !defined(_SKIN_POS_ONLY)	
		curAnimNormal = lerp(prevAnimNormal, curAnimNormal, mainAnimWeight);
		curAnimTangent = lerp(prevAnimTangent, curAnimTangent, mainAnimWeight);
#endif
	}
	
	vertex = curAnimVertex;
	
#if !defined(_SKIN_POS_ONLY)	
	normal = normalize(curAnimNormal);
	tangent.xyz = normalize(curAnimTangent);
#endif
}

void applyGPUAnimations(float4 boneIDs, float4 boneWeights, inout half4 vertex, inout half3 normal, inout half4 tangent)
{
#if (SHADER_TARGET < 30 || SHADER_API_GLES)
	float mainAnimFrame = _mainAnimationFrame;
	float mainAnimWeight = _mainAnimationWeight;
	float backgroundAnimFrame = _backgroundAnimationFrame;
#else
	float mainAnimFrame = UNITY_ACCESS_INSTANCED_PROP(_mainAnimationFrame_arr, _mainAnimationFrame);
	float mainAnimWeight = UNITY_ACCESS_INSTANCED_PROP(_mainAnimationWeight_arr, _mainAnimationWeight);
	float backgroundAnimFrame = UNITY_ACCESS_INSTANCED_PROP(_backgroundAnimationFrame_arr, _backgroundAnimationFrame);
#endif
	
	calcBlendedAnimations(boneIDs, boneWeights, mainAnimFrame, mainAnimWeight, backgroundAnimFrame, vertex, normal, tangent);
}

void applyGPUAnimationsTwoLayers(float4 boneIDs, float4 boneWeights, inout half4 vertex, inout half3 normal, inout half4 tangent)
{
#if (SHADER_TARGET < 30 || SHADER_API_GLES)
	float layerTwoWeight = _layerTwoWeight;
	float layerTwoMainAnimFrame = _layerTwoMainAnimFrame;
	float layerTwoMainAnimWeight = _layerTwoMainAnimWeight;
	float layerTwoBackgroundAnim = _layerTwoBackgroundAnim;
#else
	float layerTwoWeight = UNITY_ACCESS_INSTANCED_PROP(_layerTwoWeight_arr, _layerTwoWeight);
	float layerTwoMainAnimFrame = UNITY_ACCESS_INSTANCED_PROP(_layerTwoMainAnimFrame_arr, _layerTwoMainAnimFrame);
	float layerTwoMainAnimWeight = UNITY_ACCESS_INSTANCED_PROP(_layerTwoMainAnimWeight_arr, _layerTwoMainAnimWeight);
	float layerTwoBackgroundAnim = UNITY_ACCESS_INSTANCED_PROP(_layerTwoBackgroundAnim_arr, _layerTwoBackgroundAnim);
#endif

	//Base Layer
	if (layerTwoWeight < 1.0)
	{
		applyGPUAnimations(boneIDs, boneWeights, vertex, normal, tangent);
	}
	
	//2nd Layer
	if (layerTwoWeight > 0.0)
	{
		half4 layerTwoVertex; 
		half3 layerTwoNormal;
		half4 layerTwoTangent;
		calcBlendedAnimations(boneIDs, boneWeights, layerTwoMainAnimFrame, layerTwoMainAnimWeight, layerTwoBackgroundAnim, layerTwoVertex, layerTwoNormal, layerTwoTangent);
		
		vertex = lerp(vertex, layerTwoVertex, layerTwoWeight);
		normal = lerp(normal, layerTwoNormal, layerTwoWeight);
		tangent = lerp(tangent, layerTwoTangent, layerTwoWeight);
	}
}

#define GPU_ANIMATION_DATA(boneIdsIdx, boneWeightsIdx) \
	float4 boneIDs : TEXCOORD##boneIdsIdx; \
	float4 boneWeights : TEXCOORD##boneWeightsIdx;
	
#define GPU_ANIMATE(input) \
	applyGPUAnimations(input.boneIDs, input.boneWeights, input.vertex, input.normal, input.tangent);
	
#endif // _GPU_ANIMATION

#endif // GPU_ANIMATION_INCLUDED