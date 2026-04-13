using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 차후 필요 시 EffectSystem으로 승격하여 관리

public enum EffectList
{
    Heal,
    Mana
}

public class CPlayerEffectVisual : MonoBehaviour
{
    public static CPlayerEffectVisual Instance { get; private set; }
    [SerializeField] private GameObject _healVisual;
    [SerializeField] private GameObject _manaVisual;
    [SerializeField] private CSoundData _healSound;

    private void Awake()
    {
        Instance = this;
    }

    public void LoadEffect(EffectList effect)
    {
        if (_healVisual == null || _manaVisual == null) return;

        GameObject eft = null;

        if (effect == EffectList.Heal)
        {
            eft = Instantiate(_healVisual, transform.position, Quaternion.identity, transform);
        }
        else if (effect == EffectList.Mana)
        {
            eft = Instantiate(_manaVisual, transform.position, Quaternion.identity, transform);            
        }

        CAudioManager.Instance.Play(_healSound, transform.position);

        Destroy(eft, 0.5f);
    }
}
