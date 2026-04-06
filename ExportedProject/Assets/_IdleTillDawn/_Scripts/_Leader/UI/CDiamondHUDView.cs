using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 다이아 보유량을 UI Text에 실시간으로 표시하는 HUD 컴포넌트입니다.
/// CGoldManager.OnDiamondChanged를 구독하여 다이아가 변경될 때마다 자동 갱신됩니다.
/// 아이콘은 별도 Image 오브젝트로 배치하며, 이 컴포넌트는 숫자 Text만 담당합니다.
/// </summary>
public class CDiamondHUDView : MonoBehaviour
{
    [SerializeField] private Text _diamondText;

    private void Start()
    {
        if (CGoldManager.Instance == null) return;
        CGoldManager.Instance.OnDiamondChanged += Refresh;
        Refresh(CGoldManager.Instance.Diamond);
    }

    private void OnDestroy()
    {
        if (CGoldManager.Instance != null)
            CGoldManager.Instance.OnDiamondChanged -= Refresh;
    }

    private void Refresh(int amount)
    {
        if (_diamondText != null)
            _diamondText.text = amount.ToString("N0");
    }
}
