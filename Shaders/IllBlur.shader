Shader "Hidden/IllBlur"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0.0, 1.0)) = 0.1
        _MaxBlur ("Max Blur", Range(0.0, 1.0)) = 0.2
        _UseLensBlur ("Use Lens Blur", Float) = 0.0 // 0 = 一律ブラー, 1 = レンズブラー
    }
    SubShader
    {
        Tags { "Queue" = "Overlay" }
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
                float4 pos : POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float _BlurSize;
            float _MaxBlur;
            float _UseLensBlur; // レンズブラーを使用するかどうか
            float2 _MainTex_TexelSize;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 texelSize = _MainTex_TexelSize;

                // ブラーのオフセット初期化
                float2 blurOffset = _BlurSize * texelSize * 0.01;

                // レンズブラーの場合、中心からの距離に基づいてブラーを適用
                if (_UseLensBlur > 0.5)
                {
                    // 中心からの距離を計算
                    float2 center = float2(0.5, 0.5);
                    float dist = distance(uv, center);

                    // 距離に応じてブラー強度を変化させる
                    float blurFactor = saturate(dist * _MaxBlur);

                    // 距離に基づくオフセットを計算
                    blurOffset = _BlurSize * texelSize * blurFactor * 0.01;
                }

                // 一律ブラー処理
                half4 color = tex2D(_MainTex, uv) * 0.2;
                color += tex2D(_MainTex, uv + float2(blurOffset.x, 0)) * 0.2;
                color += tex2D(_MainTex, uv - float2(blurOffset.x, 0)) * 0.2;
                color += tex2D(_MainTex, uv + float2(0, blurOffset.y)) * 0.2;
                color += tex2D(_MainTex, uv - float2(0, blurOffset.y)) * 0.2;

                return color;
            }
            ENDCG
        }
    }
}
