Shader "GravityBox/Underwater Caustics"
{
    Properties
    {
        _BaseColor("Underwater plate", Color) = (.12,.27,.31,1)
        _FloorOpacity("Inspection opacity", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent-30" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BallLocal;
                float _FluidClock, _Motion, _FloorOpacity;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 p:TEXCOORD0; float3 normalWS:TEXCOORD1; };
            Varyings Vert(Attributes v)
            {
                Varyings o; o.positionCS=TransformObjectToHClip(v.positionOS.xyz); o.p=v.positionOS.xyz;
                o.normalWS=TransformObjectToWorldNormal(v.normalOS); return o;
            }
            float2 Hash(float2 p) { return frac(sin(float2(dot(p,float2(127.1,311.7)),dot(p,float2(269.5,183.3))))*43758.5453); }
            float Caustic(float2 uv)
            {
                float2 base=floor(uv), f=frac(uv); float first=10,second=10;
                for(int y=-1;y<=1;y++) for(int x=-1;x<=1;x++)
                {
                    float2 id=float2(x,y), h=Hash(base+id);
                    float2 cellPosition=.5+.36*sin(6.2831*h + _FluidClock*.42);
                    float d=length(id+cellPosition-f);
                    if(d<first) {second=first;first=d;} else second=min(second,d);
                }
                return pow(saturate(1-(second-first)*5.5),5);
            }
            half4 Frag(Varyings i):SV_Target
            {
                float2 uv=i.p.xz*27;
                uv += float2(sin(uv.y*1.9+_FluidClock*.37)+cos(uv.x*1.3-_FluidClock*.24),
                             cos(uv.x*1.7-_FluidClock*.29)+sin(uv.y*1.2+_FluidClock*.31))*.48;
                float cells=Caustic(uv);
                float r=distance(i.p.xz,_BallLocal.xz);
                float wake=pow(saturate(.5+.5*sin(r*400-_FluidClock*10)),10)*exp(-r*24)*saturate(_Motion*2);
                Light light=GetMainLight();
                float diffuse=.5+.5*saturate(dot(normalize(i.normalWS),light.direction));
                float grid=1-smoothstep(.012,.035,min(abs(frac(i.p.x*20+.5)-.5),abs(frac(i.p.z*20+.5)-.5)));
                half3 color=_BaseColor.rgb*diffuse + half3(.21,.43,.42)*(cells*.46+wake*.20)+grid*.018;
                return half4(color,_FloorOpacity);
            }
            ENDHLSL
        }
    }
}
