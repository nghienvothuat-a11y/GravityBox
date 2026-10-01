// Small glows and clear bits of COghe's wardrobe (star bits, bubbles, a jellyfish, the space helmet, the syringe barrel):
// unlit colour with a rim, premultiplied (or additive) blending. Kept tiny and in Resources so builds always carry it.
Shader "COghe/Accessory FX"
{
    Properties
    {
        _Color ("Colour (premultiplied by alpha)", Color) = (1, 1, 1, .4)
        _Rim ("Rim brightening", Float) = 1
        _RimAlpha ("Rim opacity", Float) = .4
        [HideInInspector] _SrcBlend ("Src", Float) = 1
        [HideInInspector] _DstBlend ("Dst", Float) = 10
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }
        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _Color; half _Rim, _RimAlpha;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 positionWS : TEXCOORD1; };
            Varyings Vert(Attributes a)
            {
                Varyings o; o.positionWS = TransformObjectToWorld(a.positionOS.xyz); o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(a.normalOS); return o;
            }
            half4 Frag(Varyings i) : SV_Target
            {
                float rim = 1 - saturate(abs(dot(normalize(i.normalWS), GetWorldSpaceNormalizeViewDir(i.positionWS))));
                half alpha = saturate(_Color.a + _RimAlpha * rim * rim);
                half3 c = _Color.rgb * (1 + _Rim * rim * rim);
                return half4(c * alpha, alpha);
            }
            ENDHLSL
        }
    }
}
