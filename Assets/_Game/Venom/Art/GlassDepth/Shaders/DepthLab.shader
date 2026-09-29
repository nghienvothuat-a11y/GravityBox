Shader "COghe/Depth Study Lab"
{
 Properties { _BaseColor("Bench",Color)=(.92,.925,.88,1) }
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque"}
  Pass
  {
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   struct A{float4 p:POSITION;};struct V{float4 p:SV_POSITION;float4 screen:TEXCOORD0;float3 world:TEXCOORD1;};
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;
   CBUFFER_END
   V Vert(A a){V o;o.p=TransformObjectToHClip(a.p.xyz);o.screen=ComputeScreenPos(o.p);o.world=TransformObjectToWorld(a.p.xyz);return o;}
   float box(float2 p,float2 c,float2 size,float blur)
   {float2 d=abs(p-c)-size;return 1-smoothstep(-blur,blur,max(d.x,d.y));}
   half4 Frag(V i):SV_Target
   {
    float2 uv=i.screen.xy/i.screen.w;
    float upper=smoothstep(.38,.84,uv.y);
    half3 c=lerp(_BaseColor.rgb,half3(.70,.80,.81),upper*.58);
    c+=.035*exp(-dot((uv-float2(.31,.62))*float2(1.3,1),(uv-float2(.31,.62))*float2(1.3,1))*4);
    // Soft, screen-space distant lab shapes. No blur pass or scene-color texture.
    float instruments=box(uv,float2(.13,.705),float2(.08,.007),.006);
    instruments+=box(uv,float2(.09,.749),float2(.023,.033),.009)*.55;
    instruments+=box(uv,float2(.15,.746),float2(.018,.029),.009)*.45;
    instruments+=box(uv,float2(.83,.755),float2(.09,.052),.012)*.35;
    instruments+=box(uv,float2(.83,.699),float2(.013,.025),.009)*.30;
    c=lerp(c,half3(.39,.55,.57),saturate(instruments)*.13);
    float window=box(uv,float2(.32,.735),float2(.065,.075),.016);
    c+=window*.035;
    Light key=GetMainLight(TransformWorldToShadowCoord(i.world));
    c*=lerp(.65,1,key.shadowAttenuation);
    return half4(c,1);
   }
   ENDHLSL
  }
 }
}
