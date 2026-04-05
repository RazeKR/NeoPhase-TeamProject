using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 씬 내 모든 Button에 자동으로 CButtonEffect를 부착하고
/// 호버/클릭 효과음을 중앙에서 재생합니다.
/// 씬에 하나만 배치하면 별도 설정 없이 모든 버튼에 효과가 적용됩니다.
/// </summary>
public class CButtonManager : MonoBehaviour
{
    public static CButtonManager Instance { get; private set; }

    [Header("버튼 효과음")]
    [Tooltip("마우스를 버튼 위에 올렸을 때 재생되는 효과음")]
    [SerializeField] private AudioClip _hoverSFX;

    [Tooltip("버튼을 클릭했을 때 재생되는 효과음")]
    [SerializeField] private AudioClip _clickSFX;

    [Header("버튼 효과 설정")]
    [Tooltip("호버 시 확대 배율")]
    [SerializeField] private float _hoverScale    = 1.1f;

    [Tooltip("스케일 전환 시간 (초)")]
    [SerializeField] private float _scaleDuration = 0.12f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        // Start에서 실행 → 씬의 모든 오브젝트 Awake 완료 후 탐색
        // Awake 시점에는 일부 버튼 오브젝트가 아직 초기화 전일 수 있음
        ApplyEffectToAllButtons();
    }

    /// <summary>씬의 모든 Button에 CButtonEffect를 자동으로 부착합니다.</summary>
    private void ApplyEffectToAllButtons()
    {
        foreach (Button btn in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            CButtonEffect effect = btn.gameObject.GetComponent<CButtonEffect>();
            if (effect == null)
                effect = btn.gameObject.AddComponent<CButtonEffect>();

            effect.Init(_hoverScale, _scaleDuration);
        }
    }

    public void PlayHoverSFX()
    {
        if (CAudioManager.Instance != null)
            CAudioManager.Instance.PlaySFX(_hoverSFX);
    }

    public void PlayClickSFX()
    {
        if (CAudioManager.Instance != null)
            CAudioManager.Instance.PlaySFX(_clickSFX);
    }
}
