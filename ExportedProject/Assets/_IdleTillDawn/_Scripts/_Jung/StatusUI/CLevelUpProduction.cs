using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLevelUpProduction : MonoBehaviour
{
    [SerializeField] private CSoundData _soundData;
    [SerializeField] private GameObject _effect;


    private CPlayerStatManager ps;
    void Start()
    {
        StartCoroutine(CoFindPS());
    }

    private IEnumerator CoFindPS()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);
        while (ps == null)
        {
            ps = FindAnyObjectByType<CPlayerStatManager>();

            if (ps != null) break;

            yield return wait;
        }

        ps.OnLevelUp -= LevelUpProduction;
        ps.OnLevelUp += LevelUpProduction;
    }

    private void OnDestroy()
    {
        if (ps != null)
        {
            ps.OnLevelUp -= LevelUpProduction;
        }
    }

    private void LevelUpProduction(int level)
    {
        if (ps == null) return;
        Transform t = ps.transform;
        GameObject eft = Instantiate(_effect, t.position, Quaternion.identity, t);
        Destroy(eft, 0.75f);
        CAudioManager.Instance.Play(_soundData, t.position);
    }
}
