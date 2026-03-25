using UnityEngine;

public class CNodeConnector : MonoBehaviour
{
    [SerializeField] private RectTransform _parentNode;
    [SerializeField] private GameObject _line;
    [SerializeField] private float _lineThickness = 5f;

    private RectTransform _lineRect;

    private void Start()
    {
        if (_parentNode == null) return;

        GameObject line = Instantiate(_line, CSkillManager.Instance._lineParent);
        
        _lineRect = line.GetComponent<RectTransform>();

        UpdateLine();
    }

    private void UpdateLine()
    {
        if (_parentNode == null || _lineRect == null) return;

        Vector2 thisPos = GetComponent<RectTransform>().anchoredPosition;   // 앵커 포지선 기준으로 작동
        Vector2 targetPos = _parentNode.anchoredPosition;

        _lineRect.anchoredPosition = (thisPos + targetPos) / 2f;    // 노드 중앙

        // 두 지점 사이의 각도
        Vector2 dir = targetPos - thisPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        _lineRect.rotation = Quaternion.Euler(0, 0, angle);

        // 두 지점 사이의 거리
        float dist = dir.magnitude;
        _lineRect.sizeDelta = new Vector2(dist, _lineThickness);


        
    }
}
