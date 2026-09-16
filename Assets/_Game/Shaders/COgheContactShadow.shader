Shader "COghe/Soft Contact Shadow"
{
    Properties
    {
        _BaseColor("Shadow",Color) = (.18,.24,.25,.18)
        _Core("Core width",Range(0,1)) = .60
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-20" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _Core;
            CBUFFER_END
            struct Attributes { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct Varyings { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; };
            Varyings Vert(Attributes a){Varyings o;o.positionCS=TransformObjectToHClip(a.vertex.xyz);o.uv=a.uv;return o;}
            half4 Frag(Varyings i):SV_Target
            {
                float2 q=abs(i.uv*2-1);
                float d=length(max(q-_Core,0));
                float a=exp(-d*d*28)*(1-smoothstep(.88,1,max(q.x,q.y)));
                return half4(_BaseColor.rgb,_BaseColor.a*a);
            }
            ENDHLSL
        }
    }
}
