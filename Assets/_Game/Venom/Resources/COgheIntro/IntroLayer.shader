// COghe intro: one comic layer drawn in immediate mode (Graphics.DrawTexture). _Alpha fades it, _Invert flips the ink for
// the impact frame, _Blur softens it (a few taps, in texels, for a zoomed plate), _Dissolve eats it away in ink blotches to reveal the
// live 3D level underneath. The blotches are placed on the target (from the quad's _QuadRect, in target heights from the
// top-left, so no platform's screen flip can mirror them) and spread out from _DissolveCentre (COghe): every layer breaks
// up with one pattern and the game appears around the creature first.
Shader "COghe/IntroLayer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise", 2D) = "gray" {}
        _Alpha ("Alpha", Range(0, 1)) = 1
        _Invert ("Invert", Range(0, 1)) = 0
        _Blur ("Blur (texels)", Range(0, 4)) = 0
        _Dissolve ("Dissolve", Range(0, 1)) = 0
        _DissolveCentre ("Dissolve centre (target heights, top-left)", Vector) = (.3, .6, 0, 0)
        _QuadRect ("Quad on the target (x, y, w, h pixels, top-left)", Vector) = (0, 0, 1, 1)
        _TargetSize ("Target size (w, h pixels)", Vector) = (1, 1, 0, 0)
        _Tint ("Tint", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Cull Off ZWrite Off ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float2 quad : TEXCOORD1; };
            sampler2D _MainTex; float4 _MainTex_ST, _MainTex_TexelSize; sampler2D _NoiseTex;
            float _Alpha, _Invert, _Blur, _Dissolve; float4 _DissolveCentre, _QuadRect, _TargetSize; fixed4 _Tint;
            v2f vert (appdata v)
            {
                v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.quad = v.uv; return o;   // dissolving quads draw their whole texture: uv runs 0..1 over the quad, v up
            }
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.uv);
                if (_Blur > 0)   // no mipmaps (they cost these NPOT textures their compression on Android): five taps instead
                {
                    float2 d = _MainTex_TexelSize.xy * _Blur;
                    c = (c * 2 + tex2D(_MainTex, i.uv + d) + tex2D(_MainTex, i.uv - d)
                        + tex2D(_MainTex, i.uv + float2(d.x, -d.y)) + tex2D(_MainTex, i.uv + float2(-d.x, d.y))) / 6;
                }
                c *= _Tint;
                c.rgb = lerp(c.rgb, 1 - c.rgb, _Invert);
                if (_Dissolve > 0)
                {
                    float2 p = (_QuadRect.xy + float2(i.quad.x, 1 - i.quad.y) * _QuadRect.zw) / _TargetSize.y;
                    float n = tex2D(_NoiseTex, p * 2.2).r;
                    n = lerp(n, saturate(distance(p, _DissolveCentre.xy) / 1.1), .5);
                    float w = max(fwidth(n), 1e-4);   // one screen pixel in noise units: crisp edges at any resolution
                    float edge = smoothstep(_Dissolve - w, _Dissolve + w, n);
                    c.rgb *= lerp(.08, 1, smoothstep(_Dissolve + 3 * w, _Dissolve + 6 * w, n)); // an ink line on the eaten edge
                    c.a *= edge;
                }
                c.a *= _Alpha;
                return c;
            }
            ENDCG
        }
    }
}
