// FogOfWar.shader — Visibility RenderTexture 기반 포그 합성 셰이더
//
// [구조 변경 이력]
// 구버전: _LightSources 배열(최대 64개) → 셰이더 루프로 가시성 계산 (광원 수 상한 존재)
// 신버전: _VisibilityTex RenderTexture 샘플링 → 광원 수 무제한, 루프 없음
//
// [처리 흐름 (픽셀 단위)]
// 1. _MainTex  에서 현재 프레임 씬 컬러 샘플링
// 2. _VisibilityTex 에서 가시성 값 샘플링 (CFogOfWarManager가 GL로 매 프레임 갱신)
// 3. visibility로 씬 컬러와 포그 컬러 lerp

Shader "FogOfWar/FogOfWar"
{
    Properties
    {
        _MainTex       ("Scene Color",    2D)         = "white" {} // OnRenderImage Blit 입력
        _VisibilityTex ("Visibility Map", 2D)         = "black" {} // CFogOfWarManager가 주입
        _FogColor      ("Fog Color",      Color)      = (0.04, 0, 0.08, 1)
        _FogDensity    ("Fog Density",    Range(0,1)) = 0.95
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert_img
            #pragma fragment frag
            #pragma target   3.0
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _VisibilityTex;
            float4    _FogColor;
            float     _FogDensity;

            fixed4 frag(v2f_img i) : SV_Target
            {
                // [1] 씬 컬러 샘플링
                fixed4 sceneColor = tex2D(_MainTex, i.uv);

                // [2] 가시성 샘플링
                // FogCircle 셰이더가 가산 혼합으로 그려 1.0을 초과할 수 있으므로 saturate로 클램프
                float visibility = saturate(tex2D(_VisibilityTex, i.uv).r);

                // [3] 포그 합성
                // visibility=1.0 → 씬 컬러 그대로 / visibility=0.0 → 포그 컬러
                float  fogAmount  = (1.0 - visibility) * _FogDensity;
                fixed3 finalColor = lerp(sceneColor.rgb, _FogColor.rgb, fogAmount);

                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}
