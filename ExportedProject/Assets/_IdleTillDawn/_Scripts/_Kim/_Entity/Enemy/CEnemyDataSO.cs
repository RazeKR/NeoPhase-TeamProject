using UnityEngine;

[CreateAssetMenu(menuName = "2D(SO)/Data/Enemy Data", fileName = "EnemyDataSO_")]
public class CEnemyDataSO : ScriptableObject
{
    #region 인스펙터
    [Header("기본 정보")]
    [SerializeField] private string _enemyName = "Character Name";
    [SerializeField] private string _description = "Character Description";

    [Header("적 스탯")]
    [SerializeField] private float _baseHealth = 50f;
    [SerializeField] private float _baseDamage = 10f;
    [SerializeField] private float _attackCooltime = 1.5f;
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _attackRange = 1.0f;

    [Header("스테이지당 성장 스탯")]
    [SerializeField] private float _healthGrowthPerStage = 10f;
    [SerializeField] private float _damageGrowthPerStage = 1f;

    [Header("소환할 오브젝트")]
    [SerializeField] GameObject _prefab;
    #endregion

    #region 프로퍼티
    public string EnemyName => _enemyName;
    public string Description => _description;

    public float BaseHealth => _baseHealth;
    public float BaseDamage => _baseDamage;
    public float AttackCooltime => _attackCooltime;
    public float MoveSpeed => _moveSpeed;
    public float AttackRange => _attackRange;

    public float HealthGrowthPerStage => _healthGrowthPerStage;
    public float DamageGrowthPerStage => _damageGrowthPerStage;

    public GameObject Prefab => _prefab;
    #endregion
}
