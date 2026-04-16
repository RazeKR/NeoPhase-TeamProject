using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CStatusOrb : MonoBehaviour
{
    [Header("Image (Contain Material)")]
    [SerializeField] private Image imgHP;    
    [SerializeField] private Image imgMP;    
    [SerializeField] private Image imgEXP;

    [Header("Gauge Lerp Speed")]
    [SerializeField] private float lerpSpeed = 5f;

    [Header("Text")]
    [SerializeField] private Text txtLevel;
    [SerializeField] private Text textHP;
    [SerializeField] private Text textMP;

    private Coroutine hpCo;
    private Coroutine mpCo;
    private Coroutine expCo;

    private Material orbMatHP;
    private Material orbMatMP;

    private CEntityBase playerHPClass;
    private CPlayerStatManager playerMPClass;

    void Start()
    {
        if (imgHP != null) orbMatHP = imgHP.material;
        if (imgMP != null) orbMatMP = imgMP.material;

        StartCoroutine(CoFindPlayer());
    }
    private IEnumerator CoFindPlayer()
    {
        GameObject playerObj = null;

        while (playerObj == null)
        {
            playerObj = FindObjectOfType<CPlayerStatManager>()?.gameObject;

            if (playerObj == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        playerHPClass = playerObj.GetComponent<CEntityBase>();
        playerMPClass = playerObj.GetComponent<CPlayerStatManager>();   // == EXPClass

        // _baseData 초기화 완료까지 대기 (InitBaseData / SyncWithSaveData 호출 시점 보장)
        while (!playerMPClass.IsInitialized)
        {
            yield return null;
        }

        if (playerHPClass != null)
        {
            CDebug.Log("playerHPClass");
            playerHPClass.OnHealthChanged -= SetHealth;
            playerHPClass.OnHealthChanged += SetHealth;
            SetHealth(playerHPClass.CurrentHealth, playerHPClass.MaxHealth);
        }

        if (playerMPClass != null)
        {
            CDebug.Log("playerMPClass");
            playerMPClass.OnManaChanged -= SetMana;
            playerMPClass.OnExpChanged -= SetExp;
            playerMPClass.OnLevelUp   -= ResetExpBar;
            playerMPClass.OnLevelUp   -= SetLevel;
            playerMPClass.OnManaChanged += SetMana;
            playerMPClass.OnExpChanged += SetExp;
            playerMPClass.OnLevelUp   += ResetExpBar;
            playerMPClass.OnLevelUp   += SetLevel;
            SetMana(playerMPClass.CurrentMana, playerMPClass.MaxMana);
            SetLevel(playerMPClass.CurrentLevel);

            // 초기 경험치 바는 애니메이션 없이 즉시 현재 값으로 설정
            float expRatio = Mathf.Clamp01(playerMPClass.CurrentExp / playerMPClass.GetRequiredExp(playerMPClass.CurrentLevel));
            if (imgEXP != null) imgEXP.fillAmount = expRatio;
        }
    }

    private void OnDestroy()
    {
        if (playerHPClass != null) playerHPClass.OnHealthChanged -= SetHealth;
        if (playerMPClass != null)
        {
            playerMPClass.OnManaChanged -= SetMana;
            playerMPClass.OnExpChanged -= SetExp;
            playerMPClass.OnLevelUp   -= ResetExpBar;
            playerMPClass.OnLevelUp   -= SetLevel;
        }
    }

    private void SetHealth(float currentHP, float MaxHP)
    {
        if (orbMatHP == null) return;

        float targetFill = Mathf.Clamp01(currentHP / MaxHP);

        textHP.text = $"{Mathf.RoundToInt(currentHP)}/{Mathf.RoundToInt(MaxHP)}";

        if (hpCo != null) StopCoroutine(hpCo);
        hpCo = StartCoroutine(CoLerpFill(orbMatHP, targetFill));
    }

    private void SetMana(float currentMP, float MaxMP)
    {
        if (orbMatMP == null) return;

        float targetFill = Mathf.Clamp01(currentMP / MaxMP);

        textMP.text = $"{Mathf.RoundToInt(currentMP)}/{Mathf.RoundToInt(MaxMP)}";

        if (mpCo != null) StopCoroutine(mpCo);
        mpCo = StartCoroutine(CoLerpFill(orbMatMP, targetFill));
    }

    private void ResetExpBar(int newLevel)
    {
        if (imgEXP == null) return;

        if (expCo != null) StopCoroutine(expCo);
        imgEXP.fillAmount = 0f;
    }

    private void SetLevel(int level)
    {
        if (txtLevel == null) return;
        txtLevel.text = $"Lev. {level}";
    }

    private void SetExp(float currentEXP, float MaxEXP)
    {
        if (imgEXP == null) return;

        float targetFill = Mathf.Clamp01(currentEXP / MaxEXP);

        if (expCo != null) StopCoroutine(expCo);
        expCo = StartCoroutine(CoLerpImageFill(targetFill));
    }

    private IEnumerator CoLerpImageFill(float targetFill)
    {
        float currentFill = imgEXP.fillAmount;

        while (Mathf.Abs(currentFill - targetFill) > 0.001f)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * lerpSpeed);
            imgEXP.fillAmount = currentFill;
            yield return null;
        }

        imgEXP.fillAmount = targetFill;
    }

    private IEnumerator CoLerpFill(Material mat, float targetFill)
    {
        float currentFill = mat.GetFloat("_FillAmount");

        while (Mathf.Abs(currentFill - targetFill) > 0.001f)
        {
            currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * lerpSpeed);
            mat.SetFloat("_FillAmount", currentFill);
            yield return null;
        }

        mat.SetFloat("_FillAmount", targetFill);
    }
}
