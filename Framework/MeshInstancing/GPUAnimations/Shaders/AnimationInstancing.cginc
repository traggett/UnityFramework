#ifndef ANIMATION_INSTANCING_INCLUDED
#define ANIMATION_INSTANCING_INCLUDED

#if !defined(_VERTEX_SKINNING)

#define APPLY_VERTEX_SKINNING(vertex, boneWeights, boneIDs, normal, tangent)

#else // _VERTEX_SKINNING

uniform int _boneCount;
uniform sampler2D _animationTexture;
uniform int _animationTextureWidth;
uniform int _animationTextureHeight;

#if (SHADER_TARGET < 30 || SHADER_API_GLES)
uniform float _animationFrame;
uniform float _blendedAnimationFrame;
uniform float _animationBlend;
#else
UNITY_INSTANCING_BUFFER_START(Props)
	UNITY_DEFINE_INSTANCED_PROP(float, _animationFrame)
#define frameIndex_arr Props
UNITY_INSTANCING_BUFFER_END(Props)
#endif

half4x4 loadAnimationInstanceMatrixFromTexture(uint frameIndex, uint boneIndex)
{
	uint pixelsPerFrame = 4;
	
	//how many frames per row?
	uint framesPerRow = _animationTextureWidth / pixelsPerFrame;
	float row = frameIndex / framesPerRow;
	
	int2 uv;
	uv.y = (row * _boneCount);
	uv.x = pixelsPerFrame * (frameIndex - _animationTextureWidth / pixelsPerFrame * uv.y);
	uv.y += boneIndex;

	float2 uvFrame;
	uvFrame.x = uv.x / (float)_animationTextureWidth;
	uvFrame.y = uv.y / (float)_animationTextureHeight;
	half4 uvf = half4(uvFrame, 0, 0);

	float offset = 1.0f / (float)_animationTextureWidth;
	half4 c1 = tex2Dlod(_animationTexture, uvf);
	uvf.x = uvf.x + offset;
	half4 c2 = tex2Dlod(_animationTexture, uvf);
	uvf.x = uvf.x + offset;
	half4 c3 = tex2Dlod(_animationTexture, uvf);
	uvf.x = uvf.x + offset;
	//half4 c4 = tex2Dlod(_animationTexture, uvf);
	half4 c4 = half4(0, 0, 0, 1);
	//float4x4 m = float4x4(c1, c2, c3, c4);
	half4x4 m;
	m._11_21_31_41 = c1;
	m._12_22_32_42 = c2;
	m._13_23_33_43 = c3;
	m._14_24_34_44 = c4;
	return m;
}

half4 animationInstanceSkinning(float4 vertex, float4 boneWeights, half4 boneIDs, inout half3 normal, inout half4 tangent)
{
#if (SHADER_TARGET < 30 || SHADER_API_GLES)
	float curFrame = _animationFrame;
#else
	float curFrame = UNITY_ACCESS_INSTANCED_PROP(frameIndex_arr, _animationFrame);
#endif

	int preFrame = floor(curFrame);
	int nextFrame = preFrame + 1;
	half4x4 localToWorldMatrixPre = loadAnimationInstanceMatrixFromTexture(preFrame, boneIDs.x) * boneWeights.x;
	if (boneWeights.y > 0.0f)
		localToWorldMatrixPre = localToWorldMatrixPre + loadAnimationInstanceMatrixFromTexture(preFrame, boneIDs.y) * boneWeights.y;
	if (boneWeights.z > 0.0f)
		localToWorldMatrixPre = localToWorldMatrixPre + loadAnimationInstanceMatrixFromTexture(preFrame, boneIDs.z) * boneWeights.z;
	if (boneWeights.w > 0.0f)
		localToWorldMatrixPre = localToWorldMatrixPre + loadAnimationInstanceMatrixFromTexture(preFrame, boneIDs.w) * boneWeights.w;

	half4x4 localToWorldMatrixNext = loadAnimationInstanceMatrixFromTexture(nextFrame, boneIDs.x) * boneWeights.x;
	if (boneWeights.y > 0.0f)
		localToWorldMatrixNext = localToWorldMatrixNext + loadAnimationInstanceMatrixFromTexture(nextFrame, boneIDs.y) * boneWeights.y;
	if (boneWeights.z > 0.0f)
		localToWorldMatrixNext = localToWorldMatrixNext + loadAnimationInstanceMatrixFromTexture(nextFrame, boneIDs.z) * boneWeights.z;
	if (boneWeights.w > 0.0f)
		localToWorldMatrixNext = localToWorldMatrixNext + loadAnimationInstanceMatrixFromTexture(nextFrame, boneIDs.w) * boneWeights.w;

	half4 localPosPre = mul(vertex, localToWorldMatrixPre);
	half4 localPosNext = mul(vertex, localToWorldMatrixNext);
	half4 localPos = lerp(localPosPre, localPosNext, curFrame - preFrame);

	half3 localNormPre = mul(normal.xyz, (float3x3)localToWorldMatrixPre);
	half3 localNormNext = mul(normal.xyz, (float3x3)localToWorldMatrixNext);
	normal = normalize(lerp(localNormPre, localNormNext, curFrame - preFrame));
	half3 localTanPre = mul(tangent.xyz, (float3x3)localToWorldMatrixPre);
	half3 localTanNext = mul(tangent.xyz, (float3x3)localToWorldMatrixNext);
	tangent.xyz = normalize(lerp(localTanPre, localTanNext, curFrame - preFrame));
	
	return localPos;
}

half4 animationInstanceSkinningShadows(float4 vertex, float4 boneWeights, half4 boneIDs, inout half3 normal, inout half4 tangent)
{
#if (SHADER_TARGET < 30 || SHADER_API_GLES)
	float curFrame = frameIndex;
#else
	float curFrame = UNITY_ACCESS_INSTANCED_PROP(frameIndex_arr, frameIndex);
#endif

	int preFrame = floor(curFrame);
	int nextFrame = preFrame + 1;
	half4x4 localToWorldMatrixPre = loadAnimationInstanceMatrixFromTexture(preFrame, boneIDs.x);
	half4x4 localToWorldMatrixNext = loadAnimationInstanceMatrixFromTexture(nextFrame, boneIDs.x);
	half4 localPosPre = mul(vertex, localToWorldMatrixPre);
	half4 localPosNext = mul(vertex, localToWorldMatrixNext);
	half4 localPos = lerp(localPosPre, localPosNext, curFrame - preFrame);
	
	return localPos;
}


#ifdef UNITY_PASS_SHADOWCASTER
#define APPLY_VERTEX_SKINNING(vertex, boneWeights, boneIDs, normal, tangent) \
	vertex = animationInstanceSkinningShadows(vertex, boneWeights, boneIDs, normal, tangent);
#else
#define APPLY_VERTEX_SKINNING(vertex, boneWeights, boneIDs, normal, tangent) \
	vertex = animationInstanceSkinning(vertex, boneWeights, boneIDs, normal, tangent);
#endif

#endif // _VERTEX_SKINNING

#endif // ANIMATION_INSTANCING_INCLUDED