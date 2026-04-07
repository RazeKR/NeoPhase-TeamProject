using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CStatusEffectVisual : MonoBehaviour
{
    private GameObject _frozenVisual;
    private GameObject _burnVisual;
    private CEntityBase _target;


    private void Awake()
    {
        _target = GetComponent<CEntityBase>();

        InitEffect(ref _frozenVisual, "_Prefabs/Skill_JHW/SkillEffects/FrozenVisual");
        InitEffect(ref _burnVisual, "_Prefabs/Skill_JHW/SkillEffects/BurnVisual");
    }


    private void InitEffect(ref GameObject obj, string path)
    {
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab != null)
        {
            obj = Instantiate(prefab, transform);
            obj.transform.localPosition = Vector3.zero;
            obj.SetActive(false);
        }
    }


    void Update()
    {
        if (_target == null) return;

        if (_frozenVisual != null)
            _frozenVisual.SetActive(_target.HasStatus(EStatusEffect.Freeze));

        if (_burnVisual != null)
            _burnVisual.SetActive(_target.HasStatus(EStatusEffect.Burn));
    }
}
