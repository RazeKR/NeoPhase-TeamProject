Shader "Custom/StatusOrb"
{
    Properties {
        _MainTex ("Circle Mask", 2D) = "white" {}
        _Color ("Liquid Color", Color) = (1,0,0,1)
        _FillAmount ("Fill Amount", Range(0,1)) = 0.5
        _WaveSpeed ("Wave Speed", Float) = 2.0
        _WaveAmp ("Wave Amplitude", Float) = 0.02
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            sampler2D _MainTex;
            float4 _Color;
            float _FillAmount, _WaveSpeed, _WaveAmp;

            v2f vert (appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {

                float pixelScale = 32.0;

                float2 steppedUV = floor(i.uv * pixelScale) / pixelScale;

                // 물결 계산 (Sine 함수 사용)
                float wave = sin(steppedUV.x * 10.0 + _Time.y * _WaveSpeed) * _WaveAmp;
                
                // 마스크 이미지 (원형)
                fixed4 mask = tex2D(_MainTex, steppedUV);
                
                // 채우기 로직 (Y축 기준)
                float fill = step(steppedUV.y, _FillAmount + wave);
                
                fixed4 col = _Color;
                col.a = fill * mask.a; // 원형 마스크 안에서만 보이게 함
                return col;
            }
            ENDCG
        }
    }
}
