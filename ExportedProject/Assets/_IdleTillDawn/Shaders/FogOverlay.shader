// FogOverlay.shader
// [역할] 씬 컬러 버퍼 위에 포그 오버레이를 합성한다
//        CFogRenderFeature 의 CFogOverlayPass 에서 Blit 머티리얼로 사용된다
//
// [합성 방식]
//   FogStateRT의 가시성(R채널) 값으로 씬 색상과 포그 색상 사이를 선형 보간한다
//   가시성=1.0 → 포그 없음 (원래 씬 색상 100%)
//   가시성=0.0 → 포그 최대 (포그 색상 × fogDensity)
//
// [비주얼 효과]
//   1. 펄스(Pulse): sin 함수로 포그 경계가 미세하게 진동 — 살아있는 느낌
//   2. 노이즈(Noise): Value Noise로 연기/안개 질감 — 불규칙성 추가
//      두 효과 모두 어두운 영역에서만 강하게 적용 (밝은 시야 영역에는 영향 최소화)

Shader "FogOfWar/FogOverlay"
{
    Properties
    {
        _MainTex    ("Scene Color",    2D)            = "white" {} // Blit 입력 — 씬 컬러 버퍼
        _FogTex     ("Fog State RT",   2D)            = "black" {} // 포그 상태 RT (CFogOfWarManager가 매 프레임 갱신)
        _FogColor   ("Fog Color",      Color)         = (0.04, 0, 0.08, 1) // 포그 색상 (어두운 보라 계열 권장)
        _FogDensity ("Fog Density",    Range(0, 1))   = 0.9               // 포그 최대 불투명도
        _PulseSpeed ("Pulse Speed",    Float)         = 1.1               // 펄스 진동 주기
        _PulseAmount("Pulse Amount",   Range(0, 0.1)) = 0.025             // 펄스 가시성 진폭
        _NoiseScale ("Noise Scale",    Float)         = 4.0               // 노이즈 공간 스케일
        _NoiseAmount("Noise Amount",   Range(0, 0.1)) = 0.03              // 노이즈 가시성 진폭
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
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

            // ─── 유니폼 ───────────────────────────────────────────────
            sampler2D _MainTex;    // 씬 컬러 버퍼 (Blit 입력)
            sampler2D _FogTex;     // 포그 상태 RT

            float4 _FogColor;      // 포그 색상 (RGBA)
            float  _FogDensity;    // 포그 최대 불투명도
            float  _PulseSpeed;    // 펄스 속도
            float  _PulseAmount;   // 펄스 진폭
            float  _NoiseScale;    // 노이즈 공간 스케일
            float  _NoiseAmount;   // 노이즈 진폭

            // ─── 노이즈 유틸리티 ──────────────────────────────────────

            /// <summary>
            /// 수학 기반 2D 해시 함수 — 텍스처 샘플링 없이 의사 난수 생성
            /// 외부 노이즈 텍스처 없이 동작하므로 에셋 의존성 없음
            /// </summary>
            float Hash21(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            /// <summary>
            /// Value Noise — 4개의 격자점 해시를 Smoothstep 이중 보간
            /// 결과: 0~1 범위의 연속적이고 부드러운 노이즈 값
            /// 연기/안개 질감 표현에 사용
            /// </summary>
            float ValueNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);

                // Smoothstep 보간 가중치 (3t²-2t³)
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = Hash21(i);
                float b = Hash21(i + float2(1.0, 0.0));
                float c = Hash21(i + float2(0.0, 1.0));
                float d = Hash21(i + float2(1.0, 1.0));

                return lerp(
                    lerp(a, b, u.x),
                    lerp(c, d, u.x),
                    u.y
                );
            }

            // ─── 프래그먼트 셰이더 ────────────────────────────────────
            fixed4 frag(v2f_img i) : SV_Target
            {
                // [1] 씬 컬러와 포그 가시성 샘플링
                fixed4 sceneColor = tex2D(_MainTex, i.uv);
                float  visibility = tex2D(_FogTex,  i.uv).r; // FogProcess 출력 R채널

                // [2] 펄스 효과 — 포그 경계가 미세하게 진동하여 살아있는 느낌 연출
                //     (1.0 - visibility): 어두운 영역에서만 펄스가 강하게 적용
                //     밝은 시야 영역(visibility≈1.0)에서는 펄스 효과 거의 없음
                float pulse    = sin(_Time.y * _PulseSpeed) * _PulseAmount;
                visibility     = saturate(visibility + pulse * (1.0 - visibility));

                // [3] Value Noise — 느리게 흐르는 노이즈로 연기/안개 질감 부여
                //     uv * _NoiseScale: 노이즈 공간 주파수 제어
                //     _Time.y * 0.04: 노이즈가 천천히 흘러가는 애니메이션
                //     (2.0 * noise - 1.0): 0~1 → -1~1 범위 재매핑
                float2 noiseUV = i.uv * _NoiseScale + _Time.y * float2(0.04, 0.02);
                float  noise   = ValueNoise(noiseUV) * 2.0 - 1.0; // -1 ~ 1 정규화
                visibility     = saturate(visibility + noise * _NoiseAmount * (1.0 - visibility));

                // [4] 포그 양 계산
                //     가시성이 낮을수록 포그가 강하게 적용된다
                //     _FogDensity: 포그의 최대 불투명도 상한 (0.9 = 최대 90% 덮임)
                float fogAmount = (1.0 - visibility) * _FogDensity;

                // [5] 씬 컬러와 포그 색상 선형 보간 (알파 포함)
                //     visibility=1.0 → sceneColor 그대로
                //     visibility=0.0 → fogColor × fogDensity 블렌드
                fixed3 finalColor = lerp(sceneColor.rgb, _FogColor.rgb, fogAmount);

                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}
