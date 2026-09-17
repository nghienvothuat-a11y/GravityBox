Shader "COghe/Guidance"
{
    Properties { _BaseColor("Color", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+15" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            struct Input { float4 positionOS : POSITION; half4 color : COLOR; };
            struct Output { float4 positionCS : SV_POSITION; half4 color : COLOR; };
            Output Vert(Input v) { Output o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz); o.color=v.color*_BaseColor; return o; }
            half4 Frag(Output i) : SV_Target { return i.color; }
            ENDHLSL
        }
    }
}
