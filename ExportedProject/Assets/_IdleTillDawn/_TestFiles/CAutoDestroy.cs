using UnityEngine;

public class CAutoDestroy : MonoBehaviour
{
    #region 인스펙터
    [Header("자동 파괴 딜레이")]
    [SerializeField] private float _delay = 1.0f;
    #endregion

    void Start()
    {
        Destroy(gameObject, _delay);
    }
}
