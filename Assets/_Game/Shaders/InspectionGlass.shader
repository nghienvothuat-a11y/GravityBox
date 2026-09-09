Shader "GravityBox/Inspection Glass"
{
    Properties
    {
        _Tint("Glass tint", Color) = (.8,.73,.58,1)
        _Opacity("Face opacity", Range(0,1)) = .018
        _EdgeOpacity("Edge opacity", Range(0,1)) = .10
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                float _Opacity, _EdgeOpacity;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; };
            Varyings Vert(Attributes v)
            {
                Varyings o; o.positionWS=TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.positionWS);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS); return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                float3 view=normalize(_WorldSpaceCameraPos-i.positionWS);
                float edge=pow(1-saturate(dot(normalize(i.normalWS),view)),3);
                return half4(_Tint.rgb,lerp(_Opacity,_EdgeOpacity,edge)*_Tint.a);
            }
            ENDHLSL
        }
    }
}
