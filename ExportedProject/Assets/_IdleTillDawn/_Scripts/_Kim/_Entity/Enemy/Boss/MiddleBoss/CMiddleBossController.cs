using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMiddleBossController : CBossBase
{
    #region 인스펙터
    [Header("중간 보스 패턴 설정")]
    [SerializeField] private float _dashForce = 10f;
    [SerializeField] private float _prepareTime = 1.5f;
    [SerializeField] private float _slidingTime = 0.8f;
    [SerializeField] private string _dashLayer = "Dash_Layer";
    #endregion

    protected override IEnumerator CoAttackSequence()
    {
        int originLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer(_dashLayer);

        yield return base.CoAttackSequence();

        gameObject.layer = originLayer;
    }

    protected override IEnumerator CoTelegraph()
    {
        Rb.velocity = Vector2.zero;

        Debug.Log($"{gameObject.name} 돌진 준비");

        yield return new WaitForSeconds(_prepareTime);
    }

    protected override IEnumerator CoProcessPattern()
    {
        if (CurrentTarget == null) yield break;

        Vector2 dashDir = (CurrentTarget.position - transform.position).normalized;

        Rb.AddForce(dashDir * _dashForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(_slidingTime);

        Rb.velocity = Vector2.zero;
    }
}
