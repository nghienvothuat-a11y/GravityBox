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
  _Mode("Look (0 coat, 1 wet gel, 2 oil film, 3 glitter enamel)",Float)=0
  _Ripple("Gel ripples",Float)=.9
  _RippleScale("Ripple size (1/m)",Float)=30
  _Drops("Droplets",Float)=.45
  _Film("Oil film",Float)=1
  _Glitter("Glitter",Float)=2.5
  _FlowSpeed("Flow speed",Float)=1
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
   float _Mode,_Ripple,_RippleScale,_Drops,_Film,_Glitter,_FlowSpeed;
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
   float Noise(float2 p){float2 i=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(Hash(i),Hash(i+float2(1,0)),f.x),lerp(Hash(i+float2(0,1)),Hash(i+1),f.x),f.y);}
   float Fbm(float2 p){float t=0,a=.5;for(int k=0;k<4;k++){t+=a*Noise(p);p=p*2.03+17.1;a*=.5;}return t;}
   half4 Frag(V i):SV_Target
   {
    float3 n=normalize(i.n);float3 v=normalize(GetWorldSpaceViewDir(i.w));
    // the two world axes across the face, in metres (x/z on a top, along/up on a side)
    float3 an=abs(n);bool top=an.y>max(an.x,an.z),side=!top&&an.x>an.z;
    float2 s=top?i.w.xz:(side?i.w.zy:i.w.xy);
    float3 T=top?float3(1,0,0):(side?float3(0,0,1):float3(1,0,0)),B=top?float3(0,0,1):float3(0,1,0);
    float time=_Time.y*_FlowSpeed;float3 sn=n;   // the shading normal: ripples and drops tilt it
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
    float fresView=1-saturate(dot(n,v));half wet=0,drop=0;
    if(_Mode>.5&&_Mode<1.5)
    {
     // wet gel: a clear coat whose surface ripples and flows slowly, with droplets standing on it
     float2 p=s*_RippleScale+float2(.31,.17)*time;const float e=.12;
     float h0=Fbm(p),hx=Fbm(p+float2(e,0)),hy=Fbm(p+float2(0,e));
     float2 slope=float2(hx-h0,hy-h0)/e*_Ripple;
     sn=normalize(n-(slope.x*T+slope.y*B)*.35);
     // ripple glints: the slopes that tilt up the face catch a soft light, so bright crescents flow across it
     float tilt=slope.y*.6+slope.x*.25;wet=(smoothstep(.30,.42,tilt)-smoothstep(.48,.62,tilt))*.28;
     c*=.94+.12*h0;
     // droplets: little domes, a dark rim, a bright window highlight up-left and a small one low-right
     float2 g=floor(s/.014),f=frac(s/.014)-.5-(float2(Hash(g+3.7),Hash(g+8.3))-.5)*.25;float r=.20+.16*Hash(g+1.9);
     float on=step(1-_Drops,Hash(g+5.1));float d=length(f)/r;
     if(on>0)
     {
      float shadow=(1-smoothstep(.9,1.25,length(f-float2(.06,-.08))/r))*step(1,d);c*=1-.18*shadow;
      if(d<1)
      {
       sn=normalize(n+(f.x*T+f.y*B)/r*.9);
       c=lerp(c*1.12,c*.62,smoothstep(.55,1,d));
       drop=(1-smoothstep(.16,.26,length(f/r-float2(-.32,.34))))+.6*(1-smoothstep(.06,.12,length(f/r-float2(.36,-.30))));
      }
     }
    }
    else if(_Mode>1.5&&_Mode<2.5)
    {
     // pastel holographic film: soft swirls of pink, mint and sky drifting over lilac
     float2 p=s*4;float2 w=float2(Fbm(p*.7+float2(0,time*.05)),Fbm(p*.7+float2(5.2,1.3)-time*.04));
     float thick=Fbm(p+w*2.2);
     half3 film=lerp(half3(1,1,1),Hue(thick*2.0+fresView*.6+time*.02),.5);
     c=lerp(c,c*.62+film*.48,_Film*(.45+.35*smoothstep(.3,.7,thick)));
    }
    Light L=GetMainLight(TransformWorldToShadowCoord(i.w));
    half lit=L.shadowAttenuation*L.distanceAttenuation;
    half3 col=c*(L.color*saturate(dot(n,L.direction)*.75+.25)*lit*.85+SampleSH(n));
    float fres=pow(1-saturate(dot(sn,v)),4);
    // thin film: hue by viewing angle, drifting over the face
    half3 film=lerp(half3(1,1,1),Hue(dot(n,v)*1.4+(s.x+s.y)*3),_Iridescence);
    float3 h=normalize(L.direction+v);
    col+=pow(saturate(dot(sn,h)),_Gloss)*_Spec*lit*L.color*film;
    float3 r=reflect(-v,sn);
    col+=GlossyEnvironmentReflection(r,.06,1)*_Reflect*(.18+.82*fres)*film;
    col+=fres*_Fresnel*film;
    col+=_Iridescence*.18*Hue(dot(n,v)*2.1+(s.x-s.y)*4)*(.35+.65*fres);
    col+=(wet*.8+drop)*lerp(half3(1,1,1),L.color,.3);
    // the shine: a soft bright band that slides slowly across every slick face (one every 35 cm)
    float sweep=frac((s.x+s.y)/.35-_Time.y*_SheenSpeed);
    col+=_SheenColor.rgb*_SheenStrength*exp(-pow((sweep-.5)/_SheenWidth,2))*(.5+.5*fres);
    if(_Mode>2.5)
    {
     // glitter enamel: 1.6 mm flakes under the coat, each tilted its own way and slowly turning, so they twinkle
     float2 g=floor(s/.0022),f=frac(s/.0022)-.5;float a=Hash(g)*6.2832+time*(.6+Hash(g+4.4));
     float3 fn=normalize(n+(cos(a)*T+sin(a)*B)*(.35+.5*Hash(g+2.2)));
     float spark=pow(saturate(dot(reflect(-v,fn),L.direction)),30)*lit+pow(saturate(dot(reflect(-v,fn),normalize(float3(-.3,.8,-.5)))),40)*.6;
     spark+=.7*pow(saturate(sin(Hash(g+9.9)*40+time*3)),16);   // and each flake flashes now and then
     float flake=(1-smoothstep(.25,.5,length(f)))*step(.5,Hash(g+6.6));
     col+=spark*flake*_Glitter*lerp(half3(1,1,1),Hue(Hash(g+7.7)),.4);
    }
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
