// FogOfWar.shader — Stateless 실시간 시야 제한 셰이더
//
// [잔상이 생기지 않는 이유]
// 이전 프레임의 RenderTexture를 전혀 읽지 않는다
// _MainTex 는 오직 현재 프레임의 씬 컬러 버퍼만을 의미한다
// 매 프레임 광원 배열(_LightSources)을 기반으로 처음부터 가시성을 계산하므로
// 광원이 이동해도 이전 위치에 어떤 흔적도 남지 않는다
//
// [처리 흐름 (픽셀 단위)]
// 1. 화면 UV → 카메라 기준 월드 좌표 변환
// 2. 모든 광원에 대해 거리 기반 smoothstep 가시성 계산
// 3. 가시성이 가장 높은 광원을 max 블렌드로 선택 (광원 간 독립성 유지)
// 4. 씬 컬러와 포그 컬러를 가시성 기반으로 lerp

Shader "FogOfWar/FogOfWar"
{
    Properties
    {
        _MainTex   ("Scene Color",   2D)          = "white" {} // OnRenderImage Blit 입력 — 현재 프레임 씬 컬러
        _FogColor  ("Fog Color",     Color)        = (0.04, 0, 0.08, 1)
        _FogDensity("Fog Density",   Range(0, 1))  = 0.95
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
            #pragma target   3.5
            #include "UnityCG.cginc"

            // ─── 유니폼 ─────────────────────────────────────────────────
            sampler2D _MainTex;              // 씬 컬러 버퍼 (현재 프레임만, 이전 RT 아님)

            float4 _LightSources[32];        // xy=월드좌표, z=outerRadius, w=innerRadius
            float4 _LightParams[32];         // x=intensity
            int    _LightCount;              // 현재 활성 광원 수

            // 카메라가 보고 있는 월드 공간 경계 (직교 투영 기반으로 매 프레임 정확히 계산됨)
            float4 _CameraBounds;            // x=minX, y=minY, z=maxX, w=maxY

            float4 _FogColor;
            float  _FogDensity;

            // ─── 프래그먼트 셰이더 ──────────────────────────────────────
            fixed4 frag(v2f_img i) : SV_Target
            {
                // [1] 씬 컬러 샘플링
                fixed4 sceneColor = tex2D(_MainTex, i.uv);

                // [2] 화면 UV → 월드 좌표 변환
                //     직교 카메라이므로 선형 보간으로 정확히 변환된다
                //     원근(Perspective) 카메라 사용 시 이 변환식을 수정해야 한다
                float  worldX   = lerp(_CameraBounds.x, _CameraBounds.z, i.uv.x);
                float  worldY   = lerp(_CameraBounds.y, _CameraBounds.w, i.uv.y);
                float2 worldPos = float2(worldX, worldY);

                // [3] 광원 기여도 계산 — 완전 Stateless
                //     이전 프레임 정보 없이 오직 현재 광원 배열만으로 계산한다
                //     각 광원은 서로 독립적이며 max 블렌드로 합산된다
                float visibility = 0.0;

                [loop]
                for (int l = 0; l < _LightCount; l++)
                {
                    float2 lPos      = _LightSources[l].xy;  // 광원 월드 위치
                    float  outerR    = _LightSources[l].z;   // 시야 외곽 반경 (이 거리 밖 = 완전 어둠)
                    float  innerR    = _LightSources[l].w;   // 완전 밝음 내부 반경
                    float  intensity = _LightParams[l].x;    // 광원 강도

                    float dist = distance(worldPos, lPos);

                    // smoothstep 그라데이션:
                    //   dist < innerR  → 1.0 (완전 밝음, Hard Edge 없음)
                    //   dist > outerR  → 0.0 (완전 어둠)
                    //   중간 구간      → 부드러운 페이드
                    float vis = 1.0 - smoothstep(innerR, outerR, dist);
                    vis *= intensity;

                    // 여러 광원이 겹쳐도 각각 독립적 원 유지 (max 블렌드)
                    visibility = max(visibility, vis);
                }

                // [4] 포그 합성
                //     visibility=1.0 → 씬 컬러 그대로 (완전 시야)
                //     visibility=0.0 → _FogColor × _FogDensity (완전 포그)
                float  fogAmount  = (1.0 - visibility) * _FogDensity;
                fixed3 finalColor = lerp(sceneColor.rgb, _FogColor.rgb, fogAmount);

                return fixed4(finalColor, 1.0);
            }
            ENDCG
        }
    }
}
