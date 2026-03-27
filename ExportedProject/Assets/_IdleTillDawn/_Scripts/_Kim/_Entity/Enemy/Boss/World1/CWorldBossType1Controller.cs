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

    [Header("촉수 소환 설정")]
    [SerializeField] string _tentaclePoolKey = "Tentacle";
    [SerializeField] int _maxTentacleCount = 15;
    [SerializeField] float _spawnInterval = 5f;
    [SerializeField] float _spawnRadiusMin = 3f;
    [SerializeField] float _spawnRadiusMax = 6f;
    #endregion

    #region 내부 변수
    private int _currentTentacleCount = 0;
    private Coroutine _spawnLoopTentacle;
    #endregion

    #region 이벤트
    public static event System.Action<string, Vector2> OnRequestSpawn;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        CTentacleController.OnTentacleDestroyed += HandleTentacleDestroyed;
    }

    private void OnDestroy()
    {
        CTentacleController.OnTentacleDestroyed -= HandleTentacleDestroyed;

        if (_spawnLoopTentacle != null)
        {
            StopCoroutine(_spawnLoopTentacle);
            _spawnLoopTentacle = null;
        }
    }

    protected override void HandleAttack()
    {
        base.HandleAttack();

        if (_spawnLoopTentacle == null)
        {
            _spawnLoopTentacle = StartCoroutine(CoTentacleSpawnLoop());
        }
    }

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

    private IEnumerator CoTentacleSpawnLoop()
    {
        while (true)
        {
            if (_currentTentacleCount < _maxTentacleCount)
            {
                Vector2 spawnPos = GetRandomSpawnPos();

                OnRequestSpawn?.Invoke(_tentaclePoolKey, spawnPos);
                _currentTentacleCount++;

                yield return new WaitForSeconds(_spawnInterval);
            }
            else
            {
                yield return null;
            }
        }
    }

    private void HandleTentacleDestroyed()
    {
        _currentTentacleCount--;
    }

    private Vector2 GetRandomSpawnPos()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(_spawnRadiusMin, _spawnRadiusMax);

        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        return (Vector2)CurrentTarget.position + offset;
    }
}
