Shader "Custom/ParasiteDissolve"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _EdgeColor ("Edge Color", Color) = (0.8, 0.2, 0.2, 1)
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05
        _EmissionIntensity ("Emission Intensity", Range(0, 3)) = 1
        _ParasiteColor ("Parasite Color", Color) = (0.2, 0.8, 0.6, 1)
        _UVScrollSpeed ("UV Scroll Speed", Vector) = (0.1, 0.1, 0, 0)
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
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NoiseTex;
            float4 _NoiseTex_ST;
            float _DissolveAmount;
            float4 _EdgeColor;
            float _EdgeWidth;
            float _EmissionIntensity;
            float4 _ParasiteColor;
            float2 _UVScrollSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 noiseUV = TRANSFORM_TEX(i.uv, _NoiseTex);
                noiseUV += _Time.y * _UVScrollSpeed;
                
                float noise = tex2D(_NoiseTex, noiseUV).r;
                
                float dissolve = noise - _DissolveAmount;
                
                clip(dissolve);
                
                float edge = smoothstep(-_EdgeWidth, 0, dissolve);
                float edgeGlow = smoothstep(-_EdgeWidth * 2, -_EdgeWidth * 0.5, dissolve);
                
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = pow(1 - max(0, dot(viewDir, normalize(i.worldNormal))), 3);
                
                float4 baseColor = tex2D(_MainTex, i.uv);
                float3 emission = _ParasiteColor.rgb * edgeGlow * _EmissionIntensity * fresnel;
                
                float3 finalColor = baseColor.rgb * (1 - edge) + _EdgeColor.rgb * edge;
                finalColor += emission;
                
                float alpha = baseColor.a * (1 - _DissolveAmount * 0.5);
                
                return float4(finalColor, alpha);
            }
            ENDCG
        }
    }
    FallBack "Transparent/VertexLit"
}