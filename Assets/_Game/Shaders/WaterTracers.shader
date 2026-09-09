Shader "GravityBox/Water Tracers"
{
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-10" "RenderType"="Transparent" }
        Pass
        {
            Blend SrcAlpha One
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                float4 _HalfSize;
            CBUFFER_END
            struct Attributes { float4 p:POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; };
            struct Varyings { float4 p:SV_POSITION; float2 uv:TEXCOORD0; half4 color:COLOR; float3 local:TEXCOORD1; };
            Varyings Vert(Attributes v) { Varyings o; o.p=TransformObjectToHClip(v.p.xyz);o.uv=v.uv;o.color=v.color;o.local=v.p.xyz;return o; }
            half4 Frag(Varyings i):SV_Target
            {
                float3 inside=_HalfSize.xyz-abs(i.local);
                clip(min(inside.x,min(inside.y,inside.z)));
                float r=dot(i.uv*2-1,i.uv*2-1);
                return half4(i.color.rgb,i.color.a*pow(saturate(1-r),2));
            }
            ENDHLSL
        }
    }
}
