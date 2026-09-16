Shader "Custom/PollutionSpread"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _PollutionAmount ("Pollution Amount", Range(0, 1)) = 0
        _PollutionColor ("Pollution Color", Color) = (0.8, 0.1, 0.4, 1)
        _DistortionAmount ("Distortion Amount", Range(0, 0.2)) = 0.1
        _SpreadSpeed ("Spread Speed", Vector) = (0.1, 0.1, 0, 0)
        _ChromaticAberration ("Chromatic Aberration", Range(0, 0.05)) = 0.02
        _ScanlineIntensity ("Scanline Intensity", Range(0, 0.5)) = 0.1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 200
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
                float4 uvDistorted : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _PollutionAmount;
            float4 _PollutionColor;
            float _DistortionAmount;
            float2 _SpreadSpeed;
            float _ChromaticAberration;
            float _ScanlineIntensity;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                float2 noiseUV = TRANSFORM_TEX(v.uv, _NoiseTex);
                noiseUV += _Time.y * _SpreadSpeed;
                
                float4 noise = tex2Dlod(_NoiseTex, float4(noiseUV, 0, 0));
                
                float2 distortion = (noise.xy - 0.5) * _DistortionAmount * _PollutionAmount;
                
                o.uvDistorted = float4(o.uv + distortion, o.uv - distortion * 0.5);
                
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 noiseUV = TRANSFORM_TEX(i.uv, _NoiseTex);
                noiseUV += _Time.y * _SpreadSpeed * 0.5;
                float noise = tex2D(_NoiseTex, noiseUV).r;
                
                float pollutionMask = smoothstep(0.3, 0.8, noise + _PollutionAmount * 0.5);
                pollutionMask *= _PollutionAmount;
                
                float4 baseColor = tex2D(_MainTex, i.uvDistorted.xy);
                
                float2 offsetR = i.uv + float2(_ChromaticAberration * pollutionMask, 0);
                float2 offsetB = i.uv - float2(_ChromaticAberration * pollutionMask, 0);
                float r = tex2D(_MainTex, offsetR).r;
                float b = tex2D(_MainTex, offsetB).b;
                
                float3 chromaticColor = float3(r, baseColor.g, b);
                
                float scanline = sin(i.uv.y * 100 + _Time.y * 5) * _ScanlineIntensity * pollutionMask;
                
                float3 finalColor = lerp(chromaticColor, _PollutionColor.rgb, pollutionMask);
                finalColor += scanline;
                
                float alpha = baseColor.a * (1 - pollutionMask * 0.3);
                
                return float4(finalColor, alpha);
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}