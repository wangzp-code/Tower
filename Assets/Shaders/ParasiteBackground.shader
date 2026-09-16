Shader "Game/UI/ParasiteBackground"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _MainColor ("Main Color", Color) = (0.05, 0.02, 0.1, 1)
        _SecondaryColor ("Secondary Color", Color) = (0.3, 0.1, 0.5, 1)
        _TertiaryColor ("Tertiary Color", Color) = (0.1, 0.4, 0.3, 1)
        _NoiseScale ("Noise Scale", Range(1, 20)) = 10
        _Speed ("Speed", Range(0, 2)) = 1
        _Viscosity ("Viscosity", Range(0, 1)) = 0.5
        _ParticleDensity ("Particle Density", Range(0, 100)) = 50
        _ParallaxStrength ("Parallax Strength", Range(0, 0.5)) = 0.1
        _Seed ("Floor Seed", Float) = 0
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "true"
        }
        LOD 100

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainColor;
            float4 _SecondaryColor;
            float4 _TertiaryColor;
            float _NoiseScale;
            float _Speed;
            float _Viscosity;
            float _ParticleDensity;
            float _ParallaxStrength;
            float _Seed;

            float hash(float2 p, float s)
            {
                float3 p3 = frac(float3(p.xyx) + float3(s * 0.1, s * 0.07, s * 0.13));
                p3 = frac(p3 * .1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float noise(float2 p, float s)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3 - 2 * f);

                float a = hash(i, s);
                float b = hash(i + float2(1, 0), s);
                float c = hash(i + float2(0, 1), s);
                float d = hash(i + float2(1, 1), s);

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float fbm(float2 p, float s)
            {
                float value = 0;
                float amplitude = 0.5;
                float frequency = 1;

                for (int i = 0; i < 6; i++)
                {
                    value += amplitude * noise(p * frequency + s * 0.37, s);
                    amplitude *= 0.5;
                    frequency *= 2;
                }

                return value;
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y * _Speed;
                float seed = _Seed;

                // 每层 unique 的 uv 偏移 (基于 seed)
                float2 seedOffset = float2(
                    sin(seed * 13.37f) * 1.7f,
                    cos(seed * 7.73f) * 2.3f
                );

                float2 parallaxOffset = _ParallaxStrength * (uv - 0.5);
                uv += parallaxOffset;

                float2 noiseCoord = (uv + seedOffset) * _NoiseScale + float2(time * 0.1, time * 0.15);
                float n1 = fbm(noiseCoord, seed);

                float2 noiseCoord2 = (uv + seedOffset * 1.3) * _NoiseScale * 0.7 + float2(-time * 0.12, time * 0.08);
                float n2 = fbm(noiseCoord2, seed);

                float combinedNoise = lerp(n1, n2, _Viscosity);

                float3 color = lerp(_MainColor.rgb, _SecondaryColor.rgb, combinedNoise);
                color = lerp(color, _TertiaryColor.rgb, n2 * 0.20);

                float2 particleUV = (uv + seedOffset * 0.5) * _ParticleDensity;
                float2 cell = floor(particleUV);
                float2 local = frac(particleUV) - 0.5;

                float minDist = 1;
                for (int ox = -1; ox <= 1; ox++)
                {
                    for (int oy = -1; oy <= 1; oy++)
                    {
                        float2 cellOffset = float2(ox, oy);
                        float2 cellPos = cell + cellOffset;
                        float cellHash = hash(cellPos, seed);
                        float2 offset = float2(cellHash, hash(cellPos + 99, seed)) * 0.7 + 0.15;
                        offset += sin(time + cellPos.x * 7 + cellPos.y * 11 + seed) * 0.3;
                        float dist = length(local - (cellOffset + offset));
                        minDist = min(minDist, dist);
                    }
                }

                float particle = 1 - minDist;
                particle = smoothstep(0.45, 0.90, particle);

                // 整体保留深邃但不吞色 — ×0.95
                // 粒子呼吸感点缀 ×0.15
                float3 finalColor = color * 0.95 + _TertiaryColor.rgb * particle * 0.18;
                float alpha = tex2D(_MainTex, i.uv).a;
                return float4(finalColor, alpha);
            }
            ENDCG
        }
    }
}
