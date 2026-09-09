Shader "GravityBox/Retained Water"
{
    Properties { _Tint("Water tint", Color) = (.12,.46,.57,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent-20" "RenderType"="Transparent" }
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
                float4 _HalfSize;
                float4x4 _WaterWorldToLocal;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; };
            Varyings Vert(Attributes v)
            {
                Varyings o; o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS); o.normalWS = TransformObjectToWorldNormal(v.normalOS); return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                float3 view = normalize(_WorldSpaceCameraPos - i.positionWS);
                float fresnel = pow(1-saturate(dot(normalize(i.normalWS),view)),3);
                float3 p = mul(_WaterWorldToLocal,float4(i.positionWS,1)).xyz;
                float3 d = mul((float3x3)_WaterWorldToLocal,-view);
                float3 safeD = lerp(-1.0,1.0,step(0,d)) * max(abs(d),.0001);
                float3 farT = max((-_HalfSize.xyz-p)/safeD,(_HalfSize.xyz-p)/safeD);
                float lengthInWater = max(0,min(farT.x,min(farT.y,farT.z)));
                float absorption = 1-exp(-lengthInWater*2.1);
                return half4(_Tint.rgb + fresnel*.1, saturate(.035+absorption*.45+fresnel*.10)*_Tint.a);
            }
            ENDHLSL
        }
    }
}
