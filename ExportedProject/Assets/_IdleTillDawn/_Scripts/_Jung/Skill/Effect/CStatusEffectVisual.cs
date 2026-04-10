using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CStatusEffectVisual : MonoBehaviour
{
    private GameObject _frozenVisual;
    private GameObject _burnVisual;
    private CEntityBase _target;
    private SpriteRenderer _targetRenderer;


    private void Awake()
    {
        _target = GetComponent<CEntityBase>();

        _targetRenderer = GetComponent<SpriteRenderer>();
        if (_targetRenderer == null)
            _targetRenderer = GetComponentInChildren<SpriteRenderer>();

        InitEffect(ref _frozenVisual, "_Prefabs/Skill_JHW/SkillEffects/FrozenVisual");
        InitEffect(ref _burnVisual, "_EffectPrefabs/effect_state_burn");
    }


    private void InitEffect(ref GameObject obj, string path)
    {
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab != null)
        {
            obj = Instantiate(prefab, transform.root);
            obj.transform.localPosition = Vector3.zero;
            obj.SetActive(false);
        }
    }


    void Update()
    {
        if (_target == null) return;

        bool isFrozen = _target.HasStatus(EStatusEffect.Freeze);

        if (_frozenVisual != null)
        {
            if (isFrozen && !_frozenVisual.activeSelf)
                FitVisualToSprite(_frozenVisual);

            _frozenVisual.SetActive(isFrozen);
        }

        bool isBurn = _target.HasStatus(EStatusEffect.Burn);

        if (_burnVisual != null)
        {
            if (isBurn && !_burnVisual.activeSelf)
                FitVisualToSprite(_burnVisual);

            _burnVisual.SetActive(isBurn);
        }
    }


    private void FitVisualToSprite(GameObject visual)
    {
        if (_targetRenderer == null || visual == null) return;

        Vector2 targetSize = _targetRenderer.bounds.size;
        float uniformScale;

        SpriteRenderer effectRenderer = visual.GetComponent<SpriteRenderer>();
        if (effectRenderer != null && effectRenderer.sprite != null)
        {
            // SpriteRenderer 기반 이펙트 : 스프라이트 자연 크기 대비 비율로 스케일 계산
            Vector2 spriteSize = effectRenderer.sprite.bounds.size;
            if (spriteSize.x == 0f || spriteSize.y == 0f) return;

            Vector3 parentScale = visual.transform.parent != null
                ? visual.transform.parent.lossyScale
                : Vector3.one;

            float scaleX = targetSize.x / (spriteSize.x * Mathf.Abs(parentScale.x));
            float scaleY = targetSize.y / (spriteSize.y * Mathf.Abs(parentScale.y));
            uniformScale = Mathf.Max(scaleX, scaleY);
        }
        else
        {
            // 파티클 시스템 기반 이펙트 : 타겟 스프라이트 크기를 직접 스케일로 사용
            uniformScale = Mathf.Max(targetSize.x, targetSize.y);
        }

        visual.transform.localScale = new Vector3(uniformScale, uniformScale, uniformScale);

        // 스프라이트 중심점에 이펙트 배치
        Vector3 center = _targetRenderer.bounds.center;
        visual.transform.position = new Vector3(center.x, center.y, visual.transform.position.z);
    }
}
