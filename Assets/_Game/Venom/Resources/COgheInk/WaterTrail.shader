Shader "COghe/Water trail"
{
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent+2" "RenderType"="Transparent" }
  Pass
  {
   Tags { "LightMode"="UniversalForward" }
   Blend One OneMinusSrcAlpha
   ZWrite Off
   ZTest LEqual
   Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A { float4 positionOS:POSITION; half4 color:COLOR; };
   struct V { float4 positionCS:SV_POSITION; half4 color:COLOR; };
   V Vert(A a){V v;v.positionCS=TransformObjectToHClip(a.positionOS.xyz);v.color=a.color;return v;}
   half4 Frag(V v):SV_Target{return half4(v.color.rgb*v.color.a,v.color.a);}
   ENDHLSL
  }
 }
}
