using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 씬 내 모든 Button에 자동으로 CButtonEffect를 부착하고
/// 호버/클릭 효과음을 중앙에서 재생합니다.
///
/// DontDestroyOnLoad 싱글톤으로 동작하며, 씬이 로드될 때마다
/// SceneManager.sceneLoaded 이벤트를 통해 새 씬의 버튼들에도 효과를 자동 적용합니다.
/// 씬에 하나만 배치하면 별도 설정 없이 모든 씬의 모든 버튼에 효과가 적용됩니다.
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
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // 씬이 완전히 로드된 직후 해당 씬의 버튼들에 효과를 적용한다
        // Start() 대신 sceneLoaded 이벤트를 사용하는 이유:
        // DontDestroyOnLoad 오브젝트의 Start()는 최초 1회만 실행되므로
        // 이후 씬 전환 시 새로 생긴 버튼들을 잡을 수 없다
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyEffectToAllButtons();
    }

    /// <summary>현재 씬의 모든 Button(비활성 포함)에 CButtonEffect를 자동으로 부착합니다.</summary>
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
