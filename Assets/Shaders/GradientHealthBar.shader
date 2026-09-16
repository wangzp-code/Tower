Shader "Game/UI/GradientHealthBar"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _FillAmount ("Fill Amount", Range(0, 1)) = 1
        _LowHealthColor ("Low Health Color", Color) = (1, 0.2, 0.2, 1)
        _MidHealthColor ("Mid Health Color", Color) = (1, 0.6, 0.2, 1)
        _HighHealthColor ("High Health Color", Color) = (0.2, 1, 0.4, 1)
        _BorderColor ("Border Color", Color) = (0.5, 0.3, 0.8, 1)
        _GlowColor ("Glow Color", Color) = (1, 0.5, 0.8, 0.5)
        _GlowIntensity ("Glow Intensity", Range(0, 2)) = 1
        _FlowSpeed ("Flow Speed", Range(0, 3)) = 1.2
        _FlowWidth ("Flow Width", Range(0.01, 0.5)) = 0.12
        _FlowIntensity ("Flow Intensity", Range(0, 2)) = 0.8
        _FlowColor ("Flow Color", Color) = (1, 1, 1, 1)
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
            float _FillAmount;
            float4 _LowHealthColor;
            float4 _MidHealthColor;
            float4 _HighHealthColor;
            float4 _BorderColor;
            float4 _GlowColor;
            float _GlowIntensity;
            float _FlowSpeed;
            float _FlowWidth;
            float _FlowIntensity;
            float4 _FlowColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // 填充 (硬边, 仅边界过渡更柔和)
                float fillProgress = step(_FillAmount, uv.x);  // 0 或 1
                float fillEdge = smoothstep(_FillAmount - 0.01, _FillAmount, uv.x);
                float fillMask = 1 - fillEdge;  // 在已填充区为 1, 边界处渐隐

                // 决定 health 颜色
                float3 healthColor;
                if (_FillAmount <= 0.33)
                    healthColor = lerp(_LowHealthColor.rgb, _MidHealthColor.rgb, _FillAmount / 0.33);
                else if (_FillAmount <= 0.66)
                    healthColor = lerp(_MidHealthColor.rgb, _HighHealthColor.rgb, (_FillAmount - 0.33) / 0.33);
                else
                    healthColor = _HighHealthColor.rgb;

                // 边框 (上下左右细线)
                float borderMask = max(step(uv.y, 0.05), step(0.95, uv.y));
                borderMask = max(borderMask, max(step(uv.x, 0.01), step(0.99, uv.x)));

                // 垂直方向发光 (薄 bar 中)
                float glowDistance = abs(uv.y - 0.5);
                float glow = exp(-glowDistance * 4) * _GlowIntensity;
                float3 glowColor = _GlowColor.rgb * glow * fillMask;

                // === 光流高光 ===
                // 光流位置: 从左向右循环流动, 只在已填充区域内可见
                float flowPos = frac(uv.x + _Time.y * _FlowSpeed);
                // 在 _FillAmount 边界处截断 (不让光流溢出未填充区)
                float flowEdgeMask = step(flowPos, _FillAmount + 0.001);
                // 光流条: 当前 uv.x 离 flowPos 的距离, 高斯曲线形成柔边高光条
                float dist = abs(uv.x - flowPos);
                float flowMask = smoothstep(_FlowWidth, 0, dist) * flowEdgeMask * fillMask;
                // 光流颜色 = 纯白高光 (用 _FlowColor 可自定义)
                float3 flowRgb = _FlowColor.rgb * flowMask * _FlowIntensity;

                // 最终合成
                float3 finalRGB =
                    borderMask * _BorderColor.rgb
                    + (1 - borderMask) * healthColor * fillMask
                    + glowColor
                    + flowRgb;
                float finalAlpha =
                    borderMask * _BorderColor.a
                    + (1 - borderMask) * fillMask;

                return float4(finalRGB, finalAlpha);
            }
            ENDCG
        }
    }
}
