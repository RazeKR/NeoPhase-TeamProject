using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CStatusOrb : MonoBehaviour
{
    [Header("Image (Contain Material)")]
    [SerializeField] private Image imgHP;
    [SerializeField] private Image imgMP;
    [SerializeField] private Image imgEXP;
    [SerializeField] private float lerpSpeed = 5f;

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

        if (playerHPClass != null)
        {
            Debug.Log("playerHPClass");
            playerHPClass.OnHealthChanged -= SetHealth;
            playerHPClass.OnHealthChanged += SetHealth;
            SetHealth(playerHPClass.CurrentHealth, playerHPClass.MaxHealth);
        }

        if (playerMPClass != null)
        {
            Debug.Log("playerMPClass");
            playerMPClass.OnManaChanged -= SetMana;
            playerMPClass.OnExpChanged -= SetExp;
            playerMPClass.OnLevelUp   -= ResetExpBar;
            playerMPClass.OnManaChanged += SetMana;
            playerMPClass.OnExpChanged += SetExp;
            playerMPClass.OnLevelUp   += ResetExpBar;
            SetMana(playerMPClass.CurrentMana, playerMPClass.MaxMana);
            SetExp(playerMPClass.CurrentExp, playerMPClass.GetRequiredExp(playerMPClass.CurrentLevel));
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
        }
    }

    private void SetHealth(float currentHP, float MaxHP)
    {
        if (orbMatHP == null) return;

        float targetFill = Mathf.Clamp01(currentHP / MaxHP);

        if (hpCo != null) StopCoroutine(hpCo);
        hpCo = StartCoroutine(CoLerpFill(orbMatHP, targetFill));
    }

    private void SetMana(float currentMP, float MaxMP)
    {
        if (orbMatMP == null) return;

        float targetFill = Mathf.Clamp01(currentMP / MaxMP);

        if (mpCo != null) StopCoroutine(mpCo);
        mpCo = StartCoroutine(CoLerpFill(orbMatMP, targetFill));
    }

    private void ResetExpBar(int newLevel)
    {
        if (imgEXP == null) return;

        if (expCo != null) StopCoroutine(expCo);
        imgEXP.fillAmount = 0f;
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
