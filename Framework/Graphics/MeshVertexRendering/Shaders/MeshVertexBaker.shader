Shader "Mesh Vertex Rendering/Mesh Vertex Baker"
{
    SubShader
    {
        Tags { "VertexBaking" = "Source" }
        Pass
        {
            ZTest Always 
			ZWrite Off
			Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "CGIncludes/MeshVertexBaker.cginc"
            ENDCG
        }
    }
}
