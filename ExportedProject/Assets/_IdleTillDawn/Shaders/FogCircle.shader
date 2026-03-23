// FogCircle.shader — Visibility RenderTexture에 광원 원(circle)을 가산 혼합으로 그린다
//
// [역할]
// CFogOfWarManager가 GL 즉시 모드로 각 광원마다 사각형 쿼드를 이 셰이더로 그린다
// 프래그먼트에서 로컬 UV 거리를 이용해 smoothstep 그라데이션 원을 출력한다
// Blend One One(가산 혼합)으로 겹치는 광원은 자연스럽게 합산된다
//
// [입력 약속 (GL 즉시 모드)]
// POSITION   : 카메라 UV 공간(0~1) 상의 쿼드 꼭짓점
// TEXCOORD0  : 원 로컬 UV (-1~+1), 중심=0, 외곽=±1
// COLOR      : r=innerRatio (내부 완전 밝음 비율), g=intensity (광원 강도)

Shader "Hidden/FogCircle"
{
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Blend One One   // 가산 혼합 — 겹친 광원 자연 합산, saturate는 FogOfWar 셰이더에서 처리

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0; // 원 로컬 UV: -1 ~ +1
                float4 color    : COLOR;     // r=innerRatio, g=intensity
            };

            struct v2f
            {
                float4 pos   : SV_POSITION;
                float2 uv    : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos   = UnityObjectToClipPos(v.vertex); // GL.LoadOrtho 행렬 적용
                o.uv    = v.texcoord;
                o.color = v.color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float dist       = length(i.uv);   // 원 중심 0, 외곽 1
                float innerRatio = i.color.r;
                float intensity  = i.color.g;

                // innerRatio 이내 = 완전 밝음, 1.0 밖 = 완전 어둠, 그 사이 = 부드러운 페이드
                float vis = 1.0 - smoothstep(innerRatio, 1.0, dist);
                vis *= intensity;

                clip(vis - 0.001); // 기여 없는 픽셀 조기 탈출 (성능 최적화)
                return fixed4(vis, vis, vis, 1.0);
            }
            ENDCG
        }
    }
}
