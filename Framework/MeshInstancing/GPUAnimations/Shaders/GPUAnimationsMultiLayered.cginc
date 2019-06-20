#ifndef GPU_ANIMATION_MULTI_LAYERED_INCLUDED
#define GPU_ANIMATION_MULTI_LAYERED_INCLUDED

#include "GPUAnimations.cginc"

#if !defined(_GPU_ANIMATION)

#define GPU_ANIMATE_MULTI_LAYERED(input)

#else // _GPU_ANIMATION

#if (SHADER_TARGET < 30 || SHADER_API_GLES)

uniform float _layerTwoWeight;
uniform float _layerTwoMainAnimFrame;
uniform float _layerTwoMainAnimWeight;
uniform float _layerTwoBackgroundAnim;

#else

UNITY_INSTANCING_BUFFER_START(MultiLayerProps)

	UNITY_DEFINE_INSTANCED_PROP(float, _layerTwoWeight)
#define _layerTwoWeight_arr MultiLayerProps
	UNITY_DEFINE_INSTANCED_PROP(float, _layerTwoMainAnimFrame)
#define _layerTwoMainAnimFrame_arr MultiLayerProps
	UNITY_DEFINE_INSTANCED_PROP(float, _layerTwoMainAnimWeight)
#define _layerTwoMainAnimWeight_arr MultiLayerProps
	UNITY_DEFINE_INSTANCED_PROP(float, _layerTwoBackgroundAnim)
#define _layerTwoBackgroundAnim_arr MultiLayerProps

UNITY_INSTANCING_BUFFER_END(MultiLayerProps)

#endif

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
	
	half4 layerTwoVertex = vertex; 
	half3 layerTwoNormal = normal;
	half4 layerTwoTangent = tangent;
	
	//Base Layer
	if (layerTwoWeight < 1.0)
	{
		applyGPUAnimations(boneIDs, boneWeights, vertex, normal, tangent);
	}
	
	//2nd Layer
	if (layerTwoWeight > 0.0)
	{
		calcBlendedAnimations(boneIDs, boneWeights, layerTwoMainAnimFrame, layerTwoMainAnimWeight, layerTwoBackgroundAnim, layerTwoVertex, layerTwoNormal, layerTwoTangent);
		
		vertex = lerp(vertex, layerTwoVertex, layerTwoWeight);
		normal = lerp(normal, layerTwoNormal, layerTwoWeight);
		tangent = lerp(tangent, layerTwoTangent, layerTwoWeight);
	}
}

#define GPU_ANIMATE_MULTI_LAYERED(input) \
	applyGPUAnimationsTwoLayers(input.boneIDs, input.boneWeights, input.vertex, input.normal, input.tangent);
	
#endif // _GPU_ANIMATION

#endif // GPU_ANIMATION_MULTI_LAYERED_INCLUDED