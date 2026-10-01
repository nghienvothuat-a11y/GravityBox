// COghe's skin with injected inks (Mrk 01/10: colour spreads through the liquid like inks in a wish jar).
// The isosurface carries, per vertex (TEXCOORD2), how much of each of up to four inks the liquid there holds; the rest is
// COghe's own dark liquid. Inks are swirled into marbling by animated noise, and each brings its material: colour,
// metal, gloss, glow, clarity (the body turns see-through), nebula, iridescence, glowing veins and twinkling stars.
Shader "COghe/Ink Skin"
{
    Properties
    {
        _BaseColor ("Base liquid", Color) = (.018, .025, .03, 1)
        _BaseMetallic ("Base metallic", Range(0, 1)) = .16
        _BaseSmoothness ("Base smoothness", Range(0, 1)) = .77
        _InkColor0 ("Ink 0", Color) = (0, 0, 0, 1)
        _InkColor1 ("Ink 1", Color) = (0, 0, 0, 1)
        _InkColor2 ("Ink 2", Color) = (0, 0, 0, 1)
        _InkColor3 ("Ink 3", Color) = (0, 0, 0, 1)
        _InkAccent0 ("Ink 0 accent", Color) = (0, 0, 0, 1)
        _InkAccent1 ("Ink 1 accent", Color) = (0, 0, 0, 1)
        _InkAccent2 ("Ink 2 accent", Color) = (0, 0, 0, 1)
        _InkAccent3 ("Ink 3 accent", Color) = (0, 0, 0, 1)
        _InkSurface0 ("Ink 0 metal gloss glow clarity", Vector) = (0, .7, 0, 0)
        _InkSurface1 ("Ink 1 surface", Vector) = (0, .7, 0, 0)
        _InkSurface2 ("Ink 2 surface", Vector) = (0, .7, 0, 0)
        _InkSurface3 ("Ink 3 surface", Vector) = (0, .7, 0, 0)
        _InkFx0 ("Ink 0 stars nebula iridescence veins", Vector) = (0, 0, 0, 0)
        _InkFx1 ("Ink 1 fx", Vector) = (0, 0, 0, 0)
        _InkFx2 ("Ink 2 fx", Vector) = (0, 0, 0, 0)
        _InkFx3 ("Ink 3 fx", Vector) = (0, 0, 0, 0)
        _BodyCentre ("Body centre (world) + seed", Vector) = (0, 0, 0, 0)
        _MeanInk ("Mean ink (meshes without ink data)", Vector) = (0, 0, 0, 0)
        _UseMeanInk ("Use mean ink", Float) = 0
        _Marble ("Marbling", Range(0, 1)) = .8
        _Stir ("Stir speed", Float) = .15
        [HideInInspector] _SrcBlend ("Src", Float) = 1
        [HideInInspector] _DstBlend ("Dst", Float) = 0
        [HideInInspector] _ZWrite ("ZWrite", Float) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "Geometry" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor; half _BaseMetallic, _BaseSmoothness;
            half4 _InkColor0, _InkColor1, _InkColor2, _InkColor3, _InkAccent0, _InkAccent1, _InkAccent2, _InkAccent3;
            float4 _InkSurface0, _InkSurface1, _InkSurface2, _InkSurface3, _InkFx0, _InkFx1, _InkFx2, _InkFx3;
            float4 _BodyCentre, _MeanInk; float _UseMeanInk, _Marble, _Stir;
        CBUFFER_END
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull Back
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local_fragment _ALPHAPREMULTIPLY_ON
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float4 ink : TEXCOORD2; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; float3 normalWS : TEXCOORD1;
                float4 ink : TEXCOORD2; float fog : TEXCOORD3;
            };

            Varyings Vert(Attributes a)
            {
                Varyings o; VertexPositionInputs p = GetVertexPositionInputs(a.positionOS.xyz);
                o.positionCS = p.positionCS; o.positionWS = p.positionWS; o.normalWS = TransformObjectToWorldNormal(a.normalOS);
                o.ink = a.ink; o.fog = ComputeFogFactor(p.positionCS.z); return o;
            }

            // small hash noise (no textures)
            float Hash(float3 p) { p = frac(p * .3183099 + .1); p *= 17; return frac(p.x * p.y * p.z * (p.x + p.y + p.z)); }
            float Noise(float3 x)
            {
                float3 i = floor(x), f = frac(x); f = f * f * (3 - 2 * f);
                return lerp(lerp(lerp(Hash(i), Hash(i + float3(1, 0, 0)), f.x), lerp(Hash(i + float3(0, 1, 0)), Hash(i + float3(1, 1, 0)), f.x), f.y),
                            lerp(lerp(Hash(i + float3(0, 0, 1)), Hash(i + float3(1, 0, 1)), f.x), lerp(Hash(i + float3(0, 1, 1)), Hash(i + float3(1, 1, 1)), f.x), f.y), f.z);
            }
            float Fbm(float3 p) { return .55 * Noise(p) + .3 * Noise(p * 2.03 + 7.1) + .15 * Noise(p * 4.1 + 13.7); }
            // twinkling points: one candidate per cell
            float Stars(float3 p, float t)
            {
                float3 c = floor(p), f = frac(p); float h = Hash(c + 3.7);
                if (h < .4) return 0;
                float3 at = float3(Hash(c + 1.3), Hash(c + 5.9), Hash(c + 9.1)) * .6 + .2;
                float d = length(f - at), core = saturate(1 - d * 3.2);
                return core * core * core * (.45 + .55 * sin(t * (2 + h * 3) + h * 40));   // ~2 mm points, twinkling
            }
            half3 Hue(half3 c, float shift)
            {
                // rotate the hue about the grey axis (cheap, keeps luminance roughly)
                const float3 k = float3(.57735, .57735, .57735);
                float cs = cos(shift * 6.2832), sn = sin(shift * 6.2832);
                return c * cs + cross(k, c) * sn + k * dot(k, c) * (1 - cs);
            }

            half4 Frag(Varyings i) : SV_Target
            {
                float t = _Time.y;
                float3 N = normalize(i.normalWS), V = GetWorldSpaceNormalizeViewDir(i.positionWS);
                float facing = saturate(dot(N, V)), rim = 1 - facing;
                float4 w = _UseMeanInk > .5 ? _MeanInk : saturate(i.ink);
                float cover = saturate(w.x + w.y + w.z + w.w);
                // marbling: the inks' shares are redistributed by slowly stirred, warped noise (cover kept)
                float3 p = (i.positionWS - _BodyCentre.xyz) * 34 + _BodyCentre.w;
                float3 q = p + float3(Noise(p * .5 + t * _Stir), Noise(p * .5 + 17 - t * _Stir), Noise(p * .5 + 31)) * 2.4;
                float4 jitter = float4(Noise(q), Noise(q + 11.3), Noise(q + 23.7), Noise(q + 41.1)) - .5;
                float4 m = saturate(w * (1 + _Marble * jitter * 2));
                m *= cover / max(1e-4, m.x + m.y + m.z + m.w);

                half3 albedo = _BaseColor.rgb * (1 - cover), emission = 0;
                half metallic = _BaseMetallic * (1 - cover), smooth = _BaseSmoothness * (1 - cover), clarity = 0;
                float nebula = Fbm(q * .55);
                [unroll] for (int k = 0; k < 4; k++)
                {
                    float a = m[k]; if (a <= 0) continue;
                    half3 c = k == 0 ? _InkColor0.rgb : k == 1 ? _InkColor1.rgb : k == 2 ? _InkColor2.rgb : _InkColor3.rgb;
                    half3 accent = k == 0 ? _InkAccent0.rgb : k == 1 ? _InkAccent1.rgb : k == 2 ? _InkAccent2.rgb : _InkAccent3.rgb;
                    float4 s = k == 0 ? _InkSurface0 : k == 1 ? _InkSurface1 : k == 2 ? _InkSurface2 : _InkSurface3;
                    float4 fx = k == 0 ? _InkFx0 : k == 1 ? _InkFx1 : k == 2 ? _InkFx2 : _InkFx3;
                    if (fx.y > 0) c = lerp(c, accent, smoothstep(.38, .72, nebula) * fx.y);                       // nebula clouds
                    if (fx.z > 0)                                                                                   // iridescence: a rainbow sheen by angle
                    {
                        float h = rim * .9 + nebula * .6 + t * .03 + k * .21;
                        half3 rainbow = saturate(abs(frac(h + float3(0, .333, .667)) * 6 - 3) - 1);
                        c = lerp(c, lerp(saturate(Hue(c, h)), rainbow * .85 + c * .15, .55), fx.z * .8);
                    }
                    if (fx.w > 0) { float v = pow(saturate(1 - abs(Noise(q * 1.4) * 2 - 1)), 7); emission += accent * v * fx.w * a * 2.5; } // veins
                    if (fx.x > 0) emission += accent * Stars(p * 2.6 + k * 19, t) * fx.x * a * 7;                  // stars
                    albedo += c * a; metallic += s.x * a; smooth += s.y * a; clarity += s.w * a;
                    emission += c * s.z * a * (.8 + .2 * sin(t * 1.6 + k * 2.1));                                    // glow, breathing
                }
                // a soft dark rim keeps a light COghe readable on the pale lab
                albedo *= lerp(1, .58, pow(rim, 2.5) * cover);

                InputData input = (InputData)0;
                input.positionWS = i.positionWS; input.normalWS = N; input.viewDirectionWS = V;
                input.shadowCoord = TransformWorldToShadowCoord(i.positionWS); input.fogCoord = i.fog;
                input.bakedGI = SampleSH(N); input.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                input.shadowMask = half4(1, 1, 1, 1);
                SurfaceData surface = (SurfaceData)0;
                surface.albedo = albedo; surface.metallic = saturate(metallic); surface.smoothness = saturate(smooth);
                surface.emission = emission; surface.occlusion = 1; surface.normalTS = half3(0, 0, 1);
                surface.alpha = saturate(1 - clarity * (1 - pow(rim, 2) * .7));   // clear inks, firmer at the silhouette
                half4 color = UniversalFragmentPBR(input, surface);
                color.rgb = MixFog(color.rgb, i.fog);
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull Back
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            float3 _LightDirection, _LightPosition;
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            float4 ShadowVert(Attributes a) : SV_POSITION
            {
                float3 positionWS = TransformObjectToWorld(a.positionOS.xyz), normalWS = TransformObjectToWorldNormal(a.normalOS);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 toLight = normalize(_LightPosition - positionWS);
                #else
                    float3 toLight = _LightDirection;
                #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, toLight));
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return positionCS;
            }
            half4 ShadowFrag() : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
