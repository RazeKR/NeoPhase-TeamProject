using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CWorldBossType1Controller : CBossBase
{
    #region 인스펙터
    [Header("패턴 설정")]
    [SerializeField] private float _dashForce = 10f;
    [SerializeField] private float _slidingTime = 1.5f;
    [SerializeField] private string _dashLayer = "Dash_Layer";
    #endregion

    protected override IEnumerator CoAttackSequence()
    {
        int originLayer = gameObject.layer;
        int dashLayer = LayerMask.NameToLayer(_dashLayer);

        SetLayerRecursively(gameObject, dashLayer);

        yield return base.CoAttackSequence();

        SetLayerRecursively(gameObject, originLayer);
    }

    protected override IEnumerator CoTelegraph()
    {
        Rb.velocity = Vector2.zero;

        Debug.Log($"{gameObject.name} 돌진 준비");

        yield return new WaitForSeconds(0.2f);
    }

    protected override IEnumerator CoProcessPattern()
    {
        if (CurrentTarget == null) yield break;

        Vector2 dashDir = (CurrentTarget.position - transform.position).normalized;

        Rb.AddForce(dashDir * _dashForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(_slidingTime);

        Rb.velocity = Vector2.zero;
    }

    /// <summary>
    /// 자신을 포함한 모든 자식의 레이어를 바꾸는 재귀함수
    /// </summary>
    /// <param name="go"></param>
    /// <param name="newLayer"></param>
    private void SetLayerRecursively(GameObject go, int newLayer)
    {
        go.layer = newLayer;

        foreach (Transform child in go.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}
