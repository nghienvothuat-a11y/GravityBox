Shader "COghe/Lab Glass"
{
    Properties
    {
        _BaseColor("Tint", Color) = (.62,.78,.82,.05)
        _Frost("Slip coating", Range(0,1)) = 0
        _GripCentre("Grip centre and radius", Vector) = (0,.05,.15,0)
        _HasGrip("Clear grip disk", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
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
                float4 _GripCentre;
                float _Frost, _HasGrip;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; float3 normal:TEXCOORD1; float2 local:TEXCOORD2; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.world=TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.world);
                o.normal=TransformObjectToWorldNormal(v.normalOS);
                o.local=v.positionOS.xy;
                return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                float3 view=normalize(_WorldSpaceCameraPos-i.world);
                float edge=pow(1-abs(dot(normalize(i.normal),view)),4);
                float grip=1-smoothstep(_GripCentre.z-.001,_GripCentre.z+.001,length(i.local-_GripCentre.xy));
                float frost=_Frost*(1-grip*_HasGrip);
                // Surface-local fine satin grain. No scene-color copy, refraction,
                // depth texture or runtime reflection capture is required.
                float stripe=.5+.5*sin((i.local.x+i.local.y*.25)*470);
                float sheen=pow(saturate(1-abs(i.local.x+i.local.y*.35-.06)*4),8);
                half3 tint=lerp(_BaseColor.rgb,half3(.48,.66,.77),frost*.6);
                tint+=sheen*.13+stripe*frost*.018;
                float alpha=_BaseColor.a+edge*.08+frost*.22;
                return half4(tint,saturate(alpha));
            }
            ENDHLSL
        }
    }
}
