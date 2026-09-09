// Mercury is opaque. This intentionally translucent, labelled inspection view
// lets the player see the steel ball in a completely filled container.
Shader "GravityBox/Mercury Cutaway"
{
    Properties
    {
        _IsFloor("Enclosure floor", Float) = 0
        _BaseColor("Floor silver", Color) = (.24,.27,.3,1)
        _FloorOpacity("Inspection opacity", Range(0,1)) = 1
    }
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
                half4 _BaseColor;
                float _IsFloor, _FloorOpacity;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; float3 normalWS:TEXCOORD1; };
            Varyings Vert(Attributes v)
            {
                Varyings o; o.positionWS = TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS = TransformWorldToHClip(o.positionWS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS); return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                float3 view = normalize(_WorldSpaceCameraPos - i.positionWS);
                float3 normal = normalize(i.normalWS);
                float edge = pow(1-saturate(dot(normal,view)),3);
                float3 reflection = reflect(-view,normal);
                // Broad silver studio-light response, not water caustics or an air surface.
                float strip = pow(saturate(dot(reflection,normalize(float3(-.35,.8,.5)))),18);
                float sky = smoothstep(-.35,.85,reflection.y);
                half3 silver = lerp(half3(.18,.20,.23),half3(.68,.72,.78),sky) + strip*.45;
                if (_IsFloor > .5)
                    return half4(_BaseColor.rgb * (.7+sky*.3) + silver*.18, _FloorOpacity);
                return half4(silver,.09+edge*.18+strip*.10);
            }
            ENDHLSL
        }
    }
}
