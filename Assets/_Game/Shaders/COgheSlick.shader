// Slick surfaces (Mrk, 10/10/2026: "shader khác đẹp hơn và thể hiện rõ hơn về tính trơn trượt"): a glossy coat that
// reflects the studio, a soft shine that slides slowly across it, an optional pattern (stripes, waves, bubbles), an
// optional thin-film rainbow and sparkle. Opaque, one main light with its shadow, no textures: cheap on mobile.
// Coordinates are world metres across the face (tape and block faces need no UVs); the tape's UV.y marks its edges.
Shader "COghe/Slick"
{
 Properties
 {
  _BaseColor("Base",Color)=(.62,.54,.92,1)
  _PatternColor("Pattern",Color)=(.50,.42,.84,1)
  _Pattern("Pattern (0 none, 1 stripes, 2 waves, 3 bubbles)",Float)=2
  _PatternScale("Pattern size (m)",Float)=.02
  _EdgeColor("Edge",Color)=(.44,.37,.80,1)
  _EdgeBand("Edge band (UV.y, tape only)",Float)=0
  _Gloss("Highlight sharpness",Float)=220
  _Spec("Highlight strength",Float)=1.4
  _Reflect("Studio reflection",Float)=.6
  _Fresnel("Rim",Float)=.35
  _SheenColor("Shine",Color)=(1,1,1,1)
  _SheenStrength("Shine strength",Float)=.45
  _SheenSpeed("Shine speed",Float)=.25
  _SheenWidth("Shine width",Float)=.05
  _Iridescence("Rainbow film",Float)=0
  _Sparkle("Sparkle",Float)=0
 }
 SubShader
 {
  Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
  Pass
  {
   Name "ForwardLit" Tags{"LightMode"="UniversalForward"}
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
   #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
   #pragma multi_compile_fog
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor,_PatternColor,_EdgeColor,_SheenColor;
   float _Pattern,_PatternScale,_EdgeBand,_Gloss,_Spec,_Reflect,_Fresnel,_SheenStrength,_SheenSpeed,_SheenWidth,_Iridescence,_Sparkle;
   CBUFFER_END
   struct A{float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};
   struct V{float4 p:SV_POSITION;float3 w:TEXCOORD0;float3 n:TEXCOORD1;float2 uv:TEXCOORD2;float fog:TEXCOORD3;};
   V Vert(A a)
   {
    V o;o.w=TransformObjectToWorld(a.p.xyz);o.p=TransformWorldToHClip(o.w);o.n=TransformObjectToWorldNormal(a.n);o.uv=a.uv;
    o.fog=ComputeFogFactor(o.p.z);return o;
   }
   float Hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   half3 Hue(float h){return saturate(abs(frac(h+float3(0,2./3,1./3))*6-3)-1);}
   half4 Frag(V i):SV_Target
   {
    float3 n=normalize(i.n);float3 v=normalize(GetWorldSpaceViewDir(i.w));
    // the two world axes across the face, in metres (x/z on a top, along/up on a side)
    float3 an=abs(n);float2 s=an.y>max(an.x,an.z)?i.w.xz:(an.x>an.z?i.w.zy:i.w.xy);
    float2 q=s/_PatternScale;half pattern=0;
    if(_Pattern>2.5)
    {
     // bubbles: thin rings, a few per cell
     float2 cell=floor(q),f=frac(q)-.5,o=(float2(Hash(cell+3.1),Hash(cell+7.7))-.5)*.35;float r=.14+.16*Hash(cell);
     pattern=(1-smoothstep(.025,.06,abs(length(f-o)-r)))*step(.3,Hash(cell+1.3));
    }
    else if(_Pattern>1.5)pattern=1-smoothstep(.05,.11,abs(frac(q.y+.16*sin(q.x*6.2832))-.5));   // waves ≈
    else if(_Pattern>.5)pattern=1-smoothstep(.22,.26,abs(frac((q.x+q.y)*.5)-.5));                  // diagonal stripes
    half3 c=lerp(_BaseColor.rgb,_PatternColor.rgb,pattern);
    if(_EdgeBand>0)c=lerp(c,_EdgeColor.rgb,step(i.uv.y,_EdgeBand)+step(1-_EdgeBand,i.uv.y));
    Light L=GetMainLight(TransformWorldToShadowCoord(i.w));
    half lit=L.shadowAttenuation*L.distanceAttenuation;
    half3 col=c*(L.color*saturate(dot(n,L.direction)*.75+.25)*lit*.85+SampleSH(n));
    float fres=pow(1-saturate(dot(n,v)),4);
    // thin film: hue by viewing angle, drifting over the face
    half3 film=lerp(half3(1,1,1),Hue(dot(n,v)*1.4+(s.x+s.y)*3),_Iridescence);
    float3 h=normalize(L.direction+v);
    col+=pow(saturate(dot(n,h)),_Gloss)*_Spec*lit*L.color*film;
    float3 r=reflect(-v,n);
    col+=GlossyEnvironmentReflection(r,.06,1)*_Reflect*(.18+.82*fres)*film;
    col+=fres*_Fresnel*film;
    col+=_Iridescence*.18*Hue(dot(n,v)*2.1+(s.x-s.y)*4)*(.35+.65*fres);
    // the shine: a soft bright band that slides slowly across every slick face (one every 35 cm)
    float sweep=frac((s.x+s.y)/.35-_Time.y*_SheenSpeed);
    col+=_SheenColor.rgb*_SheenStrength*exp(-pow((sweep-.5)/_SheenWidth,2))*(.5+.5*fres);
    if(_Sparkle>0)
    {
     // small round glints in a 4 mm grid, each catching the light from its own angle
     float2 g=floor(s/.004),f=frac(s/.004)-.5-(float2(Hash(g+5.3),Hash(g+9.1))-.5)*.5;float on=step(.9,Hash(g));
     col+=on*_Sparkle*(1-smoothstep(.04,.16,length(f)))*pow(saturate(sin(Hash(g+2.7)*40+dot(v,float3(31,17,23)))),12);
    }
    col=MixFog(col,i.fog);
    return half4(col,1);
   }
   ENDHLSL
  }
  UsePass "Universal Render Pipeline/Lit/SHADOWCASTER"
  UsePass "Universal Render Pipeline/Lit/DEPTHONLY"
  UsePass "Universal Render Pipeline/Lit/DEPTHNORMALS"
 }
}
