// 피격 시 스프라이트를 흰색으로 채우는 HitFlash 전용 셰이더
// 텍스처의 알파(투명도)는 그대로 유지하고, 불투명 픽셀의 RGB를 모두 흰색(1,1,1)으로 교체한다
// CEntityBase.HitFlash()에서 일시적으로 기존 머티리얼과 교체되며, 플래시 종료 후 원래대로 복구된다
// Built-in Render Pipeline(Standard) 기반 프로젝트 전용

Shader "Custom/SpriteFlash"
{
    Properties
    {
        // SpriteRenderer가 런타임에 스프라이트 텍스처를 주입하는 슬롯
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent"     // 반투명 오브젝트 렌더링 큐
            "IgnoreProjector" = "True"
            "RenderType"      = "Transparent"
            "PreviewType"     = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off       // 스프라이트는 양면 렌더링
        Lighting Off   // 조명 영향 없음
        ZWrite Off     // 깊이 버퍼에 기록하지 않음 (반투명 오브젝트 표준)

        // Premultiplied Alpha 블렌딩 (Unity Sprites/Default 셰이더와 동일)
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                float4 color    : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex   = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // 텍스처에서 알파값만 참조한다
                // 투명한 영역은 그대로 투명하게 유지하고, 불투명 영역은 흰색으로 채운다
                fixed alpha = tex2D(_MainTex, IN.texcoord).a;

                // Premultiplied Alpha 방식 : RGB = 1 * alpha, A = alpha
                // Blend One OneMinusSrcAlpha 와 결합되어 올바른 흰색 플래시를 출력한다
                return fixed4(alpha, alpha, alpha, alpha);
            }
            ENDCG
        }
    }
}
