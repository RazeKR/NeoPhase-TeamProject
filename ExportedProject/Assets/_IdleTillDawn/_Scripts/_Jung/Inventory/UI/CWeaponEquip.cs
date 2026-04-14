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

    [Header("총구 화염")]
    [SerializeField] private Sprite _muzzleFlashSprite = null;   // 총구화염 스프라이트 (1장)
    [SerializeField] private float  _muzzleOffsetAdjust = 0f;    // 스프라이트 오른쪽 끝에서 추가 보정(양수=앞, 음수=뒤)
    [SerializeField] private float  _muzzleFlashDuration = 0.08f; // 화염 표시 시간(초)

    [Header("Melee Attack Swing")]
    [SerializeField] private float _swingArc = 200f;
    [SerializeField] private float _duration = 0.3f;

    [Header("총구 포그 플래시 (안개 밝기 연출)")]
    [Tooltip("발사 시 총구 위치에서 밝아지는 외곽 반경 (월드 유닛) — 플레이어 고유 반경보다 크게 설정 권장")]
    [SerializeField] private float _fogMuzzleOuterRadius   = 6f;
    [Tooltip("내부 완전 밝음 비율 (0~1)")]
    [SerializeField] [Range(0f, 1f)] private float _fogMuzzleInnerRatio  = 0.35f;
    [Tooltip("플래시 최대 밝기 (0~1) — 플레이어 고유 광원 강도보다 높게 설정 권장")]
    [SerializeField] [Range(0f, 1f)] private float _fogMuzzlePeakIntensity = 1f;
    [Tooltip("총구 끝에서 조준 방향으로 추가 이동할 거리 (월드 유닛, 양수=앞)")]
    [SerializeField] private float _fogMuzzleForwardOffset = 0.5f;

    private string _currentInstanceID;
    private SpriteRenderer _targetSpriteRdr;
    private CItemDataSO _itemDataSO;

    private int _petProjectileBonus = 0;

    // 총구화염 전용 오브젝트 (풀링 대신 단일 재사용)
    private GameObject     _muzzleFlashObj;
    private SpriteRenderer _muzzleFlashRdr;
    private Coroutine      _muzzleFlashCoroutine;

    // 총구 포그 플래시 — 비주얼 화염과 별개로 항상 동작하는 안개 광원
    private GameObject      _fogMuzzleObj;
    private CFogFlashSource _muzzleFogSource;

    // Melee Attack Swing
    private float _swingOffset = 0f;
    private bool _isSwinging = false;
    private float _lastAimAngle;

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
            return;
        }

        SetupMuzzleFlash();
        SetupFogMuzzleFlash();
    }

    private void Start()
    {
        StartCoroutine(CoLoadEquippedWeapon());
    }

    private IEnumerator CoLoadEquippedWeapon()
    {
        yield return new WaitUntil(() => CInventorySystemJ.Instance != null);
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

        float meleeOffset = 0;

        CWeaponDataSO weaponDataSO = _itemDataSO as CWeaponDataSO;
        if (weaponDataSO == null) return;

        if (weaponDataSO.IsMelee) meleeOffset = -90f;

        if (_isSwinging)
        {
            ApplyRotation(_lastAimAngle + _swingOffset + meleeOffset);
            return;
        }
                
        Transform target = _playerController.CurrentTarget;        
        if (target == null) return;

        

        // 적까지의 월드 방향
        Vector2 worldDir = (target.position - _targetObject.transform.position).normalized;
        float worldAngle = Mathf.Atan2(worldDir.y, worldDir.x) * Mathf.Rad2Deg;

        _lastAimAngle = worldAngle;

        ApplyRotation(worldAngle + _swingOffset + meleeOffset);        
    }

    private void ApplyRotation(float angle)
    {
        Transform weaponTransform = _targetObject.transform;

        // lossyScale.x : 부모 계층 전체 X scale의 곱
        // 음수이면 스프라이트가 X 미러링된 상태 → localAngle = 180 - worldAngle 로 역보정
        bool isParentFlipped = weaponTransform.lossyScale.x < 0f;
        
        float localAngle = isParentFlipped ? 180f - angle : angle;

        weaponTransform.localRotation = Quaternion.Euler(0f, 0f, localAngle);

        // 총구가 왼쪽을 향할 때(|worldAngle| > 90) 스프라이트 Y 반전
        // 반전하지 않으면 총이 위아래 뒤집어진 채로 보임
        bool pointingLeft = Mathf.Abs(angle) > 90f;
        weaponTransform.localScale = new Vector3(1f, pointingLeft ? -1f : 1f, 1f);

        // 씬 뷰 디버그 — 빨간선이 적을 향하지 않으면 CurrentTarget 또는 Layer 설정 문제
        // Vector2 worldDir = (target.position - _targetObject.transform.position).normalized;
        // Debug.DrawRay(weaponTransform.position, worldDir * 1.5f, Color.red);
    }

    private void LoadEquippedWeapon()
    {
        if (CInventorySystemJ.Instance == null) return;
        if (_targetSpriteRdr == null) return;
        CWeaponInstance weapon = CInventorySystemJ.Instance.EquippedWeapon;
        if (weapon == null || weapon._itemData == null) return;

        _itemDataSO = weapon._itemData;
        _targetSpriteRdr.sprite = weapon._itemData.ItemSprite;

        UpdateMuzzlePosition();
    }

    /// <summary>
    /// 현재 무기 스프라이트의 오른쪽 끝(bounds.max.x)을 총구 위치로 설정합니다.
    /// 스프라이트마다 크기가 달라도 자동으로 맞춰집니다.
    /// </summary>
    private void UpdateMuzzlePosition()
    {
        Sprite weaponSprite = _targetSpriteRdr != null ? _targetSpriteRdr.sprite : null;
        // bounds.max.x : 스프라이트 로컬 공간에서 오른쪽 끝 (총구 방향)
        float autoX = weaponSprite != null
            ? weaponSprite.bounds.max.x + _muzzleOffsetAdjust
            : _muzzleOffsetAdjust;

        if (_muzzleFlashObj != null)
            _muzzleFlashObj.transform.localPosition = new Vector3(autoX, 0f, 0f);

        // 포그 총구 광원은 총구 끝에서 조준 방향으로 _fogMuzzleForwardOffset 만큼 앞에 배치
        // _fogMuzzleObj는 무기의 자식이므로 로컬 X가 항상 조준 방향과 일치한다
        if (_fogMuzzleObj != null)
            _fogMuzzleObj.transform.localPosition = new Vector3(autoX + _fogMuzzleForwardOffset, 0f, 0f);
    }

    /// <summary>
    /// 총구화염 전용 GameObject를 무기 스프라이트의 자식으로 생성합니다.
    /// _muzzleFlashSprite가 null이면 아무것도 만들지 않습니다.
    /// </summary>
    private void SetupMuzzleFlash()
    {
        if (_muzzleFlashSprite == null || _targetObject == null) return;

        // 이미 존재하면 재생성하지 않음
        if (_muzzleFlashObj != null) return;

        _muzzleFlashObj = new GameObject("MuzzleFlash");
        _muzzleFlashObj.transform.SetParent(_targetObject.transform, false);
        _muzzleFlashObj.transform.localPosition = Vector3.zero; // LoadEquippedWeapon에서 갱신
        _muzzleFlashObj.transform.localRotation = Quaternion.identity;
        _muzzleFlashObj.transform.localScale    = Vector3.one;

        _muzzleFlashRdr          = _muzzleFlashObj.AddComponent<SpriteRenderer>();
        _muzzleFlashRdr.sprite   = _muzzleFlashSprite;
        _muzzleFlashRdr.sortingLayerName = _targetSpriteRdr.sortingLayerName;
        _muzzleFlashRdr.sortingOrder     = _targetSpriteRdr.sortingOrder + 1;

        _muzzleFlashObj.SetActive(false);
    }

    /// <summary>
    /// 총구 포그 플래시 광원을 무기 자식으로 생성합니다.
    /// 비주얼 화염 스프라이트 유무와 무관하게 항상 동작합니다.
    /// </summary>
    private void SetupFogMuzzleFlash()
    {
        if (_targetObject == null) return;
        if (_fogMuzzleObj != null) return; // 중복 생성 방지

        _fogMuzzleObj = new GameObject("MuzzleFogSource");
        _fogMuzzleObj.transform.SetParent(_targetObject.transform, false);
        _fogMuzzleObj.transform.localPosition = Vector3.zero; // LoadEquippedWeapon → UpdateMuzzlePosition에서 갱신

        _muzzleFogSource = _fogMuzzleObj.AddComponent<CFogFlashSource>();
        // _muzzleFlashDuration을 포그 페이드 시간으로 재사용 (비주얼 화염과 동기화)
        _muzzleFogSource.InitializeFlash(_fogMuzzleOuterRadius, _fogMuzzleInnerRatio, _fogMuzzlePeakIntensity, _muzzleFlashDuration);
    }

    private IEnumerator CoHideMuzzleFlash()
    {
        yield return new WaitForSeconds(_muzzleFlashDuration);
        if (_muzzleFlashObj != null)
            _muzzleFlashObj.SetActive(false);
        _muzzleFlashCoroutine = null;
    }

    private IEnumerator CoBulletLifeTime(GameObject a, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(a);
    }

    #endregion

    #region PublicMethods

    /// <summary>
    /// 총구의 월드 좌표. _muzzleFlashObj가 없으면 무기 스프라이트 위치를 반환합니다.
    /// </summary>
    public Vector3 MuzzleWorldPosition =>
        _muzzleFlashObj != null
            ? _muzzleFlashObj.transform.position
            : (_targetObject != null ? _targetObject.transform.position : Vector3.zero);

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

        // Awake에서 이미 생성된 MuzzleFlash 오브젝트가 있으면
        // 새 타겟(clone WeaponTarget)으로 재부모 설정
        // 이렇게 하지 않으면 _muzzleFlashObj가 original WeaponTarget 하위에 남아
        // lossyScale.x 기준 오브젝트와 실제 렌더링 오브젝트가 달라 회전이 틀어짐
        if (_muzzleFlashObj != null)
            _muzzleFlashObj.transform.SetParent(_targetObject.transform, false);
        if (_fogMuzzleObj != null)
            _fogMuzzleObj.transform.SetParent(_targetObject.transform, false);

        SetupMuzzleFlash();
        SetupFogMuzzleFlash();
        enabled = true;
        LoadEquippedWeapon();
    }

    /// <summary>
    /// 발사 시 총구화염(비주얼)과 포그 플래시(안개 밝기)를 재생합니다.
    /// 근접무기(IsMelee)이면 두 연출 모두 무시됩니다.
    /// </summary>
    public void ShowMuzzleFlash()
    {
        // 근접무기는 제외
        CWeaponDataSO weaponData = _itemDataSO as CWeaponDataSO;
        if (weaponData == null || weaponData.IsMelee) return;

        // 비주얼 총구화염 (스프라이트 오브젝트가 있을 때만)
        if (_muzzleFlashObj != null)
        {
            if (_muzzleFlashCoroutine != null)
                StopCoroutine(_muzzleFlashCoroutine);

            _muzzleFlashObj.SetActive(true);
            _muzzleFlashCoroutine = StartCoroutine(CoHideMuzzleFlash());
        }

        // 포그 총구 플래시 — 스프라이트 유무와 무관하게 항상 실행
        _muzzleFogSource?.Trigger();
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
        
    public int GetProjectileAmount()
    {
        CWeaponDataSO data = _itemDataSO as CWeaponDataSO;
        if (data == null) return 1;
        return data.ProjectileAmount + _petProjectileBonus;
    }

    /// <summary>펫 투사체 보너스를 설정합니다. 해제 시 0으로 호출합니다.</summary>
    public void SetPetProjectileBonus(int bonus)
    {
        _petProjectileBonus = Mathf.Max(0, bonus);
    }


    public void PlaySwingAnimation()
    {
        if (_isSwinging) return;
        StartCoroutine(SwingCoroutine());
    }

    private IEnumerator SwingCoroutine()
    {
        _isSwinging = true;
        float elapsed = 0f;        
        float start = _swingArc / 2f;
        float end = -_swingArc / 2f;

        while (elapsed < _duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _duration;

            float curveT = t * (2 - t);
            _swingOffset = Mathf.Lerp(start, end, curveT);

            yield return null;
        }

        _swingOffset = 0f;
        _isSwinging = false;
    }


    #endregion



}
