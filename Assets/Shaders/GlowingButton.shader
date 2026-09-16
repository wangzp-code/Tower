Shader "Game/UI/GlowingButton"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, 0.3, 0.8, 1)
        _GlowStrength ("Glow Strength", Range(0, 2)) = 1
        _GlowSpeed ("Glow Speed", Range(0, 5)) = 2
        _BackgroundColor ("Background Color", Color) = (0.1, 0.05, 0.2, 1)
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
            };

            sampler2D _MainTex;
            float4 _GlowColor;
            float _GlowStrength;
            float _GlowSpeed;
            float4 _BackgroundColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 centered = i.uv - 0.5;
                float distance = length(centered) * 2;
                float glow = 1 - distance;
                glow = pow(glow, 2) * _GlowStrength;
                
                float time = _Time.y * _GlowSpeed;
                float pulse = 0.5 + 0.5 * sin(time);
                glow *= pulse;
                
                float4 texColor = tex2D(_MainTex, i.uv);
                float4 glowColor = _GlowColor * glow;
                float4 bgColor = _BackgroundColor;
                
                float4 finalColor = bgColor + glowColor;
                finalColor.a = saturate(0.8 + glow * 0.5);
                
                return finalColor;
            }
            ENDCG
        }
    }
}
