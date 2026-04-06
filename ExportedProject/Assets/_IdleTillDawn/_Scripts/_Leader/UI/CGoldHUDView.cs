using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 골드 보유량을 UI Text에 실시간으로 표시하는 HUD 컴포넌트입니다.
/// CGoldManager.OnGoldChanged를 구독하여 골드가 변경될 때마다 자동 갱신됩니다.
///
/// [씬 설정 방법]
///   1. 골드를 표시할 Text 오브젝트에 이 컴포넌트를 추가합니다.
///   2. Inspector의 _goldText에 해당 Text를 연결합니다.
///   3. _prefix를 원하는 문자열로 변경합니다. (기본값: "Gold: ")
/// </summary>
public class CGoldHUDView : MonoBehaviour
{
    [SerializeField] private Text   _goldText;
    [SerializeField] private string _prefix = "Gold: "; // 표시 접두어 (e.g. "골드: ", "G ")

    private void Start()
    {
        if (CGoldManager.Instance == null) return;
        CGoldManager.Instance.OnGoldChanged += Refresh;
        Refresh(CGoldManager.Instance.Gold); // 초기값 즉시 반영
    }

    private void OnDestroy()
    {
        if (CGoldManager.Instance != null)
            CGoldManager.Instance.OnGoldChanged -= Refresh;
    }

    private void Refresh(int amount)
    {
        if (_goldText != null)
            _goldText.text = $"{_prefix}{amount:N0}";
    }
}
