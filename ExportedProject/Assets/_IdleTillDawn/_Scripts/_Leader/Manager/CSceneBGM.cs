using UnityEngine;

/// <summary>
/// 씬별 BGM 지정 컴포넌트
///
/// [사용법]
/// 씬의 아무 GameObject에 붙이고 _bgmClip에 해당 씬 BGM을 연결한다.
///
/// 씬 시작 시 CAudioManager.PlayBGM()을 호출 →
/// 이전 씬 BGM이 페이드 아웃되고 이 씬의 BGM이 페이드 인된다.
///
/// [배치 예시]
/// MainMenu_KSH 씬 → _bgmClip = MainMenu_BGM
/// Stage1_KSH 씬   → _bgmClip = Battle_BGM
/// Stage2_KSH 씬   → _bgmClip = Battle_BGM  (같은 클립이면 전환 없이 유지)
/// Stage3_KSH 씬   → _bgmClip = Battle_BGM
/// </summary>
public class CSceneBGM : MonoBehaviour
{
    [Tooltip("이 씬에서 재생할 BGM 클립")]
    [SerializeField] private AudioClip _bgmClip;

    private void Start()
    {
        if (CAudioManager.Instance == null)
        {
            CDebug.LogWarning("[CSceneBGM] CAudioManager 인스턴스가 없습니다. AudioManager 오브젝트를 씬에 배치하세요.");
            return;
        }

        CAudioManager.Instance.PlayBGM(_bgmClip);
    }
}
