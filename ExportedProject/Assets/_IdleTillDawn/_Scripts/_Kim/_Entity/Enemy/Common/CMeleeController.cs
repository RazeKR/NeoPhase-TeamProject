using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMeleeController : CEnemyBase
{
    #region 인스펙터
    [Header("넉백 설정")]
    [SerializeField] private float _knockBackForce = 5f;
    [SerializeField] private float _knockBackTime = 0.2f;
    [SerializeField] private string _knockBackLayer = "Enemy_KnockBack";
    #endregion

    #region 내부 변수
    private WaitForSeconds _knockBackWait;
    private float _defaultKnockBackForce;
    private float _defaultKnockBackTime;

    private int _originLayer;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        _knockBackWait = new WaitForSeconds(_knockBackTime);

        _defaultKnockBackForce = _knockBackForce;
        _defaultKnockBackTime = _knockBackTime;

        _originLayer = gameObject.layer;
    }

    public override void ResetForPool()
    {
        base.ResetForPool();

        _knockBackForce = _defaultKnockBackForce;
        _knockBackTime = _defaultKnockBackTime;

        gameObject.layer = _originLayer;
    }

    protected override void Start()
    {
        base.Start();

        if (IsPersonalScene)
        {
            CPlayerController player = FindFirstObjectByType<CPlayerController>();

            if (player != null)
            {
                SetTarget(player.transform);
                CDebug.Log($"타겟 : {player.name}");
            }
            else
            {
                CDebug.LogWarning($"{gameObject.name} : 플레이어를 찾을 수 없음");
            }

            InitEnemy(1);
        }
    }

    protected override void ExecuteAttack()
    {
        if (CurrentHealth <= 0) return;
        
        if (TargetDamageable != null)
        {
            TargetDamageable.TakeDamage(AttackDamage);
        }

        StartCoroutine(CoAttackKnockBack());
    }

    /// <summary>
    /// 공격 시 뒤로 밀려나는 코루틴 함수
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoAttackKnockBack()
    {
        AddStatus(EStatusEffect.Knockback);
        Rb.velocity = Vector2.zero;

        int originLayer = gameObject.layer;
        gameObject.layer = LayerMask.NameToLayer(_knockBackLayer);

        Vector2 knockBackDir = (transform.position - CurrentTarget.position).normalized;
        Rb.AddForce(knockBackDir * _knockBackForce, ForceMode2D.Impulse);

        yield return _knockBackWait;

        gameObject.layer = originLayer;

        Rb.velocity = Vector2.zero;
        RemoveStatus(EStatusEffect.Knockback);
    }
}
