#ifndef ANIMATION_INSTANCING_INCLUDED
#define ANIMATION_INSTANCING_INCLUDED

#if !defined(_VERTEX_SKINNING)

#define APPLY_VERTEX_SKINNING(vertex, boneWeights, boneIDs, normal, tangent)

#else // _VERTEX_SKINNING

sampler2D _boneTexture;

static const int _pixelsPerMatrix = 4;

int _numBones;
int _boneTextureWidth;
int _boneTextureHeight;

#if (SHADER_TARGET < 30 || SHADER_API_GLES)
uniform float frameIndex;
uniform float preFrameIndex;
uniform float transitionProgress;
#else
UNITY_INSTANCING_BUFFER_START(Props)
	UNITY_DEFINE_INSTANCED_PROP(float, preFrameIndex)
#define preFrameIndex_arr Props
	UNITY_DEFINE_INSTANCED_PROP(float, frameIndex)
#define frameIndex_arr Props
	UNITY_DEFINE_INSTANCED_PROP(float, transitionProgress)
#define transitionProgress_arr Props
UNITY_INSTANCING_BUFFER_END(Props)
#endif

half4x4 loadAnimationInstanceMatrixFromTexture(uint frameIndex, uint boneIndex)
{
	uint blockCount = _boneTextureWidth / _pixelsPerMatrix;
	int2 uv;
	uv.y = frameIndex / blockCount * _numBones;
	uv.x = _pixelsPerMatrix * (frameIndex - _boneTextureWidth / _pixelsPerMatrix * uv.y);

	int matCount = _pixelsPerMatrix / 4;
	uv.x = uv.x + (boneIndex % matCount) * 4;
	uv.y = uv.y + boneIndex / matCount;

	float2 uvFrame;
	uvFrame.x = uv.x / (float)_boneTextureWidth;
	uvFrame.y = uv.y / (float)_boneTextureHeight;
	half4 uvf = half4(uvFrame, 0, 0);

	float offset = 1.0f / (float)_boneTextureWidth;
	half4 c1 = tex2Dlod(_boneTexture, uvf);
	uvf.x = uvf.x + offset;
	half4 c2 = tex2Dlod(_boneTexture, uvf);
	uvf.x = uvf.x + offset;
	half4 c3 = tex2Dlod(_boneTexture, uvf);
	uvf.x = uvf.x + offset;
	//half4 c4 = tex2Dlod(_boneTexture, uvf);
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
	float curFrame = frameIndex;
	float preAniFrame = preFrameIndex;
	float progress = transitionProgress;
#else
	float curFrame = UNITY_ACCESS_INSTANCED_PROP(frameIndex_arr, frameIndex);
	float preAniFrame = UNITY_ACCESS_INSTANCED_PROP(preFrameIndex_arr, preFrameIndex);
	float progress = UNITY_ACCESS_INSTANCED_PROP(transitionProgress_arr, transitionProgress);
#endif

	//float curFrame = UNITY_ACCESS_INSTANCED_PROP(frameIndex);
	int preFrame = curFrame;
	int nextFrame = curFrame + 1.0f;
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

	if (preAniFrame >= 0.0f)
	{
		half4x4 localToWorldMatrixPreAni = loadAnimationInstanceMatrixFromTexture(preAniFrame, boneIDs.x);
		half4 localPosPreAni = mul(vertex, localToWorldMatrixPreAni);
		localPos = lerp(localPosPreAni, localPos, progress);
	}
	return localPos;
}

half4 animationInstanceSkinningShadows(float4 vertex, float4 boneWeights, half4 boneIDs, inout half3 normal, inout half4 tangent)
{
#if (SHADER_TARGET < 30 || SHADER_API_GLES)
	float curFrame = frameIndex;
	float preAniFrame = preFrameIndex;
	float progress = transitionProgress;
#else
	float curFrame = UNITY_ACCESS_INSTANCED_PROP(frameIndex_arr, frameIndex);
	float preAniFrame = UNITY_ACCESS_INSTANCED_PROP(preFrameIndex_arr, preFrameIndex);
	float progress = UNITY_ACCESS_INSTANCED_PROP(transitionProgress_arr, transitionProgress);
#endif
	int preFrame = curFrame;
	int nextFrame = curFrame + 1.0f;
	half4x4 localToWorldMatrixPre = loadAnimationInstanceMatrixFromTexture(preFrame, boneIDs.x);
	half4x4 localToWorldMatrixNext = loadAnimationInstanceMatrixFromTexture(nextFrame, boneIDs.x);
	half4 localPosPre = mul(vertex, localToWorldMatrixPre);
	half4 localPosNext = mul(vertex, localToWorldMatrixNext);
	half4 localPos = lerp(localPosPre, localPosNext, curFrame - preFrame);
	if (preAniFrame >= 0.0f)
	{
		half4x4 localToWorldMatrixPreAni = loadAnimationInstanceMatrixFromTexture(preAniFrame, boneIDs.x);
		half4 localPosPreAni = mul(vertex, localToWorldMatrixPreAni);
		localPos = lerp(localPosPreAni, localPos, progress);
	}
	
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