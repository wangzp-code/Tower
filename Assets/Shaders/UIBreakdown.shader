Shader "Custom/UIBreakdown"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _BreakdownAmount ("Breakdown Amount", Range(0, 1)) = 0
        _VertexJitter ("Vertex Jitter", Range(0, 0.1)) = 0.05
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.1)) = 0.03
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _GlitchSpeed ("Glitch Speed", Range(0, 5)) = 2
        _ScanlineCount ("Scanline Count", Range(0, 50)) = 20
        _FlickerSpeed ("Flicker Speed", Range(0, 10)) = 5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
                float2 jitter : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _BreakdownAmount;
            float _VertexJitter;
            float _ChromaticAberration;
            float _GlitchSpeed;
            float _ScanlineCount;
            float _FlickerSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                
                float2 noiseUV = TRANSFORM_TEX(v.uv, _NoiseTex);
                float noise = tex2Dlod(_NoiseTex, float4(noiseUV, 0, 0)).r;
                
                float jitterX = sin(noise * 10 + _Time.y * _GlitchSpeed) * _VertexJitter * _BreakdownAmount;
                float jitterY = cos(noise * 15 + _Time.y * _GlitchSpeed * 1.3) * _VertexJitter * _BreakdownAmount;
                
                float4 jitteredVertex = v.vertex;
                jitteredVertex.x += jitterX;
                jitteredVertex.y += jitterY;
                
                o.vertex = UnityObjectToClipPos(jitteredVertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.jitter = float2(jitterX, jitterY);
                
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float flicker = sin(_Time.y * _FlickerSpeed) * 0.5 + 0.5;
                flicker = pow(flicker, 2);
                
                float scanline = mod(i.uv.y * _ScanlineCount, 1);
                scanline = step(0.95, scanline) * _BreakdownAmount * 0.3;
                
                float2 offsetR = i.uv + float2(_ChromaticAberration * _BreakdownAmount, 0);
                float2 offsetB = i.uv - float2(_ChromaticAberration * _BreakdownAmount * 0.8, 0);
                
                float r = tex2D(_MainTex, offsetR).r;
                float g = tex2D(_MainTex, i.uv).g;
                float b = tex2D(_MainTex, offsetB).b;
                
                float3 color = float3(r, g, b);
                color *= flicker;
                color += scanline;
                
                float4 baseColor = tex2D(_MainTex, i.uv);
                
                float noiseUV = tex2D(_NoiseTex, i.uv + _Time.y * 0.1).r;
                float noiseDistortion = (noiseUV - 0.5) * _BreakdownAmount * 0.2;
                color.g *= (1 + noiseDistortion);
                
                float alpha = baseColor.a * (1 - _BreakdownAmount * 0.2);
                
                return float4(color, alpha);
            }
            ENDCG
        }
    }
    FallBack "UI/Default"
}