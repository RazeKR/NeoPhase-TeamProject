using System.Collections;
using UnityEngine;

/// <summary>
/// 장착된 무기 정보를 가져와 스프라이트 이미지를 적용합니다.
/// </summary>

public class CWeaponEquip : MonoBehaviour
{
    #region SingleTon
    public static CWeaponEquip Instance { get; private set; }
    #endregion

    #region Inspectors & Private Variables

    [SerializeField] private GameObject _targetObject = null;   // 스프라이트 바꿀 타겟
    [SerializeField] private CPlayerController _playerController = null; // 플레이어 (자동 탐색)

    private string _currentInstanceID;
    private SpriteRenderer _targetSpriteRdr;
    private CItemDataSO _itemDataSO;

    #endregion




    #region UnityMethods

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (_targetObject == null)
        {
            enabled = false;
            return;
        }

        bool getSpriteRenderer = _targetObject.TryGetComponent<SpriteRenderer>(out _targetSpriteRdr);

        if (!getSpriteRenderer)
        {
            enabled = false;
        }
    }

    private void Start()
    {
        LoadEquippedWeapon();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        // 무기 스프라이트 갱신 — 인벤토리 의존
        if (CInventorySystemJ.Instance != null)
        {
            CWeaponInstance equipped = CInventorySystemJ.Instance.EquippedWeapon;
            if (equipped != null)
            {
                if (_currentInstanceID != equipped._instanceID)
                {
                    _currentInstanceID = equipped._instanceID;
                    _itemDataSO = equipped._itemData as CWeaponDataSO;
                    LoadEquippedWeapon();
                }
                else if (_itemDataSO != equipped._itemData)
                {
                    LoadEquippedWeapon();
                }
            }
        }

        // 조준은 인벤토리와 무관하게 항상 실행
        AimAtNearestEnemy();
    }

    #endregion

    #region PrivateMethods

    /// <summary>
    /// 가장 가까운 적 방향으로 무기를 회전시킵니다.
    /// 계층은 그대로 유지하고, 부모(플레이어)의 Scale 반전을
    /// 무기 자신의 localScale로 상쇄하여 회전을 완전히 독립시킵니다.
    /// </summary>
    private void AimAtNearestEnemy()
    {
        if (_targetObject == null || _playerController == null) return;

        Transform weaponTransform = _targetObject.transform;
        Transform target = _playerController.CurrentTarget;

        if (target == null) return;

        // 적까지의 월드 방향
        Vector2 worldDir = (target.position - weaponTransform.position).normalized;
        float worldAngle = Mathf.Atan2(worldDir.y, worldDir.x) * Mathf.Rad2Deg;

        // lossyScale.x : 부모 계층 전체 X scale의 곱
        // 음수이면 스프라이트가 X 미러링된 상태 → localAngle = 180 - worldAngle 로 역보정
        bool isParentFlipped = weaponTransform.lossyScale.x < 0f;
        float localAngle = isParentFlipped ? 180f - worldAngle : worldAngle;
        weaponTransform.localRotation = Quaternion.Euler(0f, 0f, localAngle);

        // 총구가 왼쪽을 향할 때(|worldAngle| > 90) 스프라이트 Y 반전
        // 반전하지 않으면 총이 위아래 뒤집어진 채로 보임
        bool pointingLeft = Mathf.Abs(worldAngle) > 90f;
        weaponTransform.localScale = new Vector3(1f, pointingLeft ? -1f : 1f, 1f);

        // 씬 뷰 디버그 — 빨간선이 적을 향하지 않으면 CurrentTarget 또는 Layer 설정 문제
        Debug.DrawRay(weaponTransform.position, worldDir * 1.5f, Color.red);
    }

    private void LoadEquippedWeapon()
    {
        CWeaponInstance weapon = CInventorySystemJ.Instance.EquippedWeapon;
        if (weapon == null || weapon._itemData == null) return;

        _itemDataSO = weapon._itemData;
        _targetSpriteRdr.sprite = weapon._itemData.ItemSprite;
    }

    private IEnumerator CoBulletLifeTime(GameObject a, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(a);
    }

    #endregion

    #region PublicMethods

    /// <summary>
    /// 스폰 후 플레이어 컨트롤러를 주입합니다.
    /// </summary>
    public void SetPlayerController(CPlayerController playerController)
    {
        _playerController = playerController;
    }

    /// <summary>
    /// 스폰 후 무기 스프라이트를 표시할 대상 오브젝트를 런타임에 설정합니다.
    /// </summary>
    public void SetTargetObject(GameObject targetObject)
    {
        _targetObject = targetObject;

        if (_targetObject == null)
        {
            enabled = false;
            return;
        }

        if (!_targetObject.TryGetComponent<SpriteRenderer>(out _targetSpriteRdr))
        {
            enabled = false;
            return;
        }

        enabled = true;
        LoadEquippedWeapon();
    }

    public void GenerateBullet()
    {
        GameObject a = Instantiate((_itemDataSO as CWeaponDataSO).BulletPrefab);

        a.transform.position = _targetObject.transform.position + Vector3.right * 0.2f;

        Rigidbody2D rb = a.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.AddForce(a.transform.right * 10, ForceMode2D.Impulse);
        }

        StartCoroutine(CoBulletLifeTime(a, (_itemDataSO as CWeaponDataSO).LifeTime));
    }

    #endregion



}
