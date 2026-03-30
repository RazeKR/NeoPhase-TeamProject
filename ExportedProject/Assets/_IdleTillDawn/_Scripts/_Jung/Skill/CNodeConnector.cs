using UnityEngine;
using UnityEngine.UI;

public class CNodeConnector : MonoBehaviour
{
    [SerializeField] private CSkillNode _parentNode;    
    [SerializeField] private GameObject _line;
    [SerializeField] private float _lineThickness = 5f;

    private RectTransform _lineRect;
    private Image _lineImage;

    private void Start()
    {
        if (_parentNode == null) return;

        RectTransform parent = (GameObject.Find("LinesParent")).GetComponent<RectTransform>();
        GameObject line = Instantiate(_line, parent);
        
        _lineRect = line.GetComponent<RectTransform>();
        _lineImage = line.GetComponent<Image>();

        UpdateLine();
        UpdateLineColor();
    }



    public void UpdateLineColor()
    {
        if (_parentNode == null || _lineImage == null) return;

        int parentLevel = CSkillManager.Instance.GetSkillLevel(_parentNode.SkillData.Id);

        if (parentLevel <= 0)
        {
            _lineImage.color = Color.gray;
        }

        else
        {
            _lineImage.color = Color.white;
        }
    }

    private void UpdateLine()
    {
        if (_parentNode == null || _lineRect == null) return;

        Vector2 thisPos = GetComponent<RectTransform>().anchoredPosition;   // 앵커 포지선 기준으로 작동
        Vector2 targetPos = _parentNode.GetComponent<RectTransform>().anchoredPosition;

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
