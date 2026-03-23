using UnityEngine;

/// <summary>
/// Built-in Render Pipeline 포그 오버레이 컴포넌트
/// Main Camera 오브젝트에 부착하면 씬 렌더링 후 FogOfWar 셰이더를 단일 Blit으로 합성한다
///
/// [Stateless 구조에서의 역할]
/// CFogOfWarManager.Update() 가 매 프레임 셰이더 파라미터를 갱신하고
/// 이 컴포넌트의 OnRenderImage 가 그 머티리얼로 단순 Blit만 수행한다
/// 별도 RT 상태 관리 없이 Graphics.Blit 한 줄로 처리된다
/// </summary>
[RequireComponent(typeof(Camera))]
public class CFogRenderFeature : MonoBehaviour
{
    #region Unity Methods

    /// <summary>
    /// Built-in Pipeline 후처리 콜백 — 카메라 렌더링 완료 직후 호출된다
    /// src: 씬 렌더 결과 / dest: 최종 출력 대상
    ///
    /// 매니저 또는 머티리얼이 준비되지 않으면 원본 그대로 통과시켜
    /// 포그 없이도 게임이 정상 렌더링되도록 Fallback 처리한다
    /// </summary>
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Material mat = CFogOfWarManager.Instance?.FogMaterial;

        // Fallback: 매니저 미준비 시 원본 그대로 출력
        if (mat == null) { Graphics.Blit(src, dest); return; }

        // FogOfWar 셰이더 단일 Blit — src(씬)를 읽어 dest(포그 합성 결과) 출력
        Graphics.Blit(src, dest, mat);
    }

    #endregion
}
