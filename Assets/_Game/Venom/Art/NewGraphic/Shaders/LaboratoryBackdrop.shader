Shader "COghe/NewGraphic Laboratory Backdrop"
{
    Properties { _BaseColor("Studio", Color)=(.945,.933,.897,1) _LabTex("Baked lab",2D)="black"{} }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_LabTex);SAMPLER(sampler_LabTex);
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            CBUFFER_END
            struct A {float4 positionOS:POSITION;};
            struct V {float4 positionCS:SV_POSITION;float4 screen:TEXCOORD0;};
            V Vert(A v){V o;o.positionCS=TransformObjectToHClip(v.positionOS.xyz);o.screen=ComputeScreenPos(o.positionCS);return o;}
            half4 Frag(V i):SV_Target
            {
                float2 uv=i.screen.xy/i.screen.w;
                // Distant instruments occupy only the quiet upper background; geometry depth
                // keeps them behind the chamber through orbit and zoom. No realtime blur.
                float2 labUV=float2(uv.x,(uv.y-.53)/.28);
                half4 lab=SAMPLE_TEXTURE2D(_LabTex,sampler_LabTex,saturate(labUV));
                float mask=step(0,labUV.y)*step(labUV.y,1);
                half3 ink=lerp(_BaseColor.rgb,lab.rgb,.22);
                return half4(lerp(_BaseColor.rgb,ink,lab.a*mask),1);
            }
            ENDHLSL
        }
    }
}
