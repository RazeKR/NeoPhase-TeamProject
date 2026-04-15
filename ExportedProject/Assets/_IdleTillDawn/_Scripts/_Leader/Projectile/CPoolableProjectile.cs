using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPoolableProjectile : MonoBehaviour
{
    private string _poolKey;

    private void Awake()
    {
        _poolKey = gameObject.name.Replace("(Clone)", "").Trim();
    }

    private void OnDisable()
    {
        if (CProjectilePool.Instance != null)
        {
            CProjectilePool.Instance.ReturnToPool(_poolKey, gameObject);
        }
    }
}
