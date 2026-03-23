// FogProcess.shader
// [역할] 이전 프레임의 포그 상태 RenderTexture를 입력으로 받아
//        Fade(포그 회복) + 광원 누적을 단일 패스에서 처리하고 새 포그 상태를 출력한다
//
// [Fade 방식] 지수 감소: prevVis * exp(-fadeRate * deltaTime)
//   - fadeRate=1.2 → 약 1.7초 만에 가시성이 절반으로 줄어드는 속도 (자연스러운 포그 회복)
//   - 매 프레임 동일한 비율로 감소하므로 프레임레이트와 무관하게 일관된 시각 효과 유지
//
// [광원 누적] smoothstep 그라데이션 원
//   - innerRadius 이내: 가시성 1.0 (완전 밝음)
//   - innerRadius ~ outerRadius: smoothstep으로 부드럽게 0까지 감소 (Hard Edge 금지)
//   - outerRadius 이외: 가시성 0.0
//   - 여러 광원이 겹칠 때 max 블렌드로 밝은 쪽 채택

Shader "FogOfWar/FogProcess"
{
    Properties
    {
        _MainTex ("Previous Fog State RT", 2D) = "black" {} // Blit 입력 — 이전 프레임 포그 상태
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
            #pragma target   3.5            // SetVectorArray 사용을 위해 SM 3.5 이상 필요
            #include "UnityCG.cginc"

            // ─── 유니폼 ───────────────────────────────────────────────
            sampler2D _MainTex;            // 이전 포그 상태 RT (R채널 = 가시성)

            float4 _LightSources[32];      // xy=월드좌표, z=outerRadius, w=innerRadius
            float4 _LightParams[32];       // x=intensity (강도), yzw=미사용
            int    _LightCount;            // 현재 활성 광원 수 (MAX 32)

            float4 _CameraBounds;          // x=minX, y=minY, z=maxX, w=maxY (월드 공간)
            float  _FadeRate;              // 지수 감소율 (클수록 포그가 빠르게 회복)
            float  _DeltaTime;             // Time.deltaTime (CPU에서 전달)
            float  _MinVisibility;         // 최소 가시성 (완전 암흑 방지)

            // ─── 프래그먼트 셰이더 ────────────────────────────────────
            fixed4 frag(v2f_img i) : SV_Target
            {
                // [1] UV → 월드 좌표 역변환
                //     포그 RT의 UV(0~1)를 카메라 기준 월드 공간 좌표로 변환한다
                //     이 좌표로 광원과의 거리를 계산해야 월드 스케일이 정확히 반영된다
                float  worldX   = lerp(_CameraBounds.x, _CameraBounds.z, i.uv.x);
                float  worldY   = lerp(_CameraBounds.y, _CameraBounds.w, i.uv.y);
                float2 worldPos = float2(worldX, worldY);

                // [2] 이전 포그 상태 + 지수 감소 (포그 회복)
                //     exp(-fadeRate * deltaTime) 형태로 매 프레임 일정 비율 감소
                //     프레임레이트 독립적: 60fps/30fps 모두 동일한 시각 결과
                float prevVis  = tex2D(_MainTex, i.uv).r;
                float fadedVis = prevVis * exp(-_FadeRate * _DeltaTime);

                // 최소 가시성 보장: 완전히 어두워지지 않고 희미하게 보이는 상태 유지
                fadedVis = max(fadedVis, _MinVisibility);

                // [3] 광원 기여도 계산 — 소프트 에지 원형 그라데이션
                float lightVis = 0.0;

                [loop] // 동적 루프 (가변 광원 수 지원)
                for (int l = 0; l < _LightCount; l++)
                {
                    float2 lPos      = _LightSources[l].xy;  // 광원 월드 위치
                    float  outerR    = _LightSources[l].z;   // 시야 외곽 반경
                    float  innerR    = _LightSources[l].w;   // 완전 밝음 내부 반경
                    float  intensity = _LightParams[l].x;    // 광원 강도

                    float  dist = distance(worldPos, lPos);

                    // smoothstep: innerR=1.0, outerR=0.0 으로 부드러운 그라데이션
                    // innerR 이내 → vis=1.0 (완전 밝음)
                    // outerR 이외 → vis=0.0 (완전 어둠)
                    float  vis  = 1.0 - smoothstep(innerR, outerR, dist);
                    vis *= intensity;

                    // 여러 광원 중 가장 밝은 기여도를 선택 (Max 블렌드)
                    lightVis = max(lightVis, vis);
                }

                // [4] 최종 가시성: Fade된 이전 상태와 현재 광원 기여도 중 최대값
                //     - 광원이 있는 영역: lightVis가 fadedVis를 덮어 밝게 유지
                //     - 광원이 없는 영역: fadedVis만 남아 서서히 어두워짐 (포그 회복)
                float finalVis = max(fadedVis, lightVis);
                finalVis = clamp(finalVis, 0.0, 1.0);

                return fixed4(finalVis, 0.0, 0.0, 1.0); // R채널에만 가시성 저장
            }
            ENDCG
        }
    }
}
