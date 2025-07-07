Shader "Custom/DissolveShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _DissolveAmount ("Dissolve Amount", Range(0, 1)) = 0
        _EdgeWidth ("Edge Width", Range(0, 0.2)) = 0.05
        _EdgeColor ("Edge Color", Color) = (1, 0.5, 0, 1)
    }
    SubShader
    {
        Tags { 
            "Queue"="Transparent" 
            "RenderType"="Transparent" 
            "IgnoreProjector"="True" 
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float _DissolveAmount;
            float _EdgeWidth;
            float4 _EdgeColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float noise = tex2D(_NoiseTex, i.uv).r;
                
                // Clip pixels based on dissolve amount
                clip(noise - _DissolveAmount);
                
                // Edge effect
                if (noise - _DissolveAmount < _EdgeWidth)
                {
                    col.rgb = _EdgeColor.rgb;
                }
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent"
}