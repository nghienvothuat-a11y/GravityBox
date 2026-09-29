Shader "COghe/Depth Study Glass"
{
    Properties
    {
        _BaseColor("Tint", Color) = (.62,.78,.82,.05)
        _Floor("Lit floor",Float)=0
        _ReflectionStrength("Reflection strength",Float)=0
        _StudioCube("Static studio",Cube)=""{}
        _Contact("Contact centre and radii",Vector)=(0,0,.01,.01)
        _ContactAlpha("Contact opacity",Float)=0
        _Frost("Slip coating", Range(0,1)) = 0
        _CoatingColor("Slip material tint", Color) = (.43,.37,.76,1)
        _GripCentre("Grip centre and radius", Vector) = (0,.05,.15,0)
        _HasGrip("Clear grip disk", Float) = 0
        _SlipRect("Slippery rectangle bounds", Vector) = (0,0,0,0)
        _RegionOnly("Restrict coating to rectangle", Float) = 0
        _NearFade("Clear foreground pane", Float) = 0
        _Spherical("Spherical inner normal", Float) = 0
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
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            TEXTURECUBE(_StudioCube);SAMPLER(sampler_StudioCube);
            CBUFFER_START(UnityPerMaterial)
                float4 _Contact;
                float _ContactAlpha,_Floor,_ReflectionStrength;
                half4 _BaseColor, _CoatingColor;
                float4 _GripCentre;
                float _Frost, _HasGrip, _RegionOnly, _NearFade, _Spherical;
                float4 _SlipRect;
            CBUFFER_END
            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings { float4 positionCS:SV_POSITION; float3 world:TEXCOORD0; float3 normal:TEXCOORD1; float3 local:TEXCOORD2; };
            Varyings Vert(Attributes v)
            {
                Varyings o;
                o.world=TransformObjectToWorld(v.positionOS.xyz);
                o.positionCS=TransformWorldToHClip(o.world);
                o.normal=TransformObjectToWorldNormal(v.normalOS);
                o.local=v.positionOS.xyz;
                return o;
            }
            half4 Frag(Varyings i):SV_Target
            {
                float3 view=normalize(_WorldSpaceCameraPos-i.world);
                float3 normal=normalize(i.normal);
                if(_Spherical>.5)normal=TransformObjectToWorldNormal(-normalize(i.local));
                float facing=dot(normal,view);
                float edge=pow(1-abs(facing),4);
                float grip=1-smoothstep(_GripCentre.z-.001,_GripCentre.z+.001,length(i.local.xy-_GripCentre.xy));
                float frost=_Frost*(1-grip*_HasGrip);
                float2 inRect=step(_SlipRect.xy,i.local.xy)*step(i.local.xy,_SlipRect.zw);
                frost*=lerp(1,inRect.x*inRect.y,_RegionOnly);
                // Surface-local fine satin grain. No scene-color copy, refraction,
                // depth texture or runtime reflection capture is required.
                float stripe=.5+.5*sin((i.local.x+i.local.y*.25)*470);
                float sheen=pow(saturate(1-abs(i.local.x+i.local.y*.35-.06)*4),8);
                half3 tint=lerp(_BaseColor.rgb,_CoatingColor.rgb,saturate(frost*1.6));
                tint+=sheen*.13+stripe*frost*.018;
                float baseAlpha=lerp(_BaseColor.a,.025,_RegionOnly*(1-inRect.x*inRect.y));
                baseAlpha=lerp(baseAlpha,.025,grip*_HasGrip);
                // Preserve the visible satin coating on a foreground wall: it
                // communicates a gameplay property even when clear glass fades.
                float nearVisibility=lerp(.20,.68,saturate(frost));
                float alpha=(baseAlpha+edge*.08+frost*.30)*lerp(1,nearVisibility,_NearFade*saturate(-facing*4));
                if(_Floor>.5)
                {
                    Light key=GetMainLight(TransformWorldToShadowCoord(i.world));
                    float diffuse=saturate(dot(normal,key.direction));
                    tint*=.68+.30*diffuse;
                    tint*=lerp(.52,1,key.shadowAttenuation);
                }
                if(_ReflectionStrength>.001)
                {
                    float3 reflected=reflect(-view,normal);
                    half3 reflection=SAMPLE_TEXTURECUBE_LOD(_StudioCube,sampler_StudioCube,reflected,3).rgb;
                    float fresnel=.035+.16*pow(1-abs(facing),3);
                    tint+=reflection*_ReflectionStrength*(.20+fresnel);
                    alpha+=_ReflectionStrength*fresnel*lerp(1,.28,_NearFade*saturate(-facing*4));
                }
                float2 contact=(i.local.xy-_Contact.xy)/max(_Contact.zw,.001);
                float contactMask=exp(-dot(contact,contact)*2.4)*_ContactAlpha;
                tint=lerp(tint,half3(.17,.25,.27),contactMask);
                alpha=max(alpha,contactMask*.75);
                return half4(tint,saturate(alpha));
            }
            ENDHLSL
        }
    }
}
