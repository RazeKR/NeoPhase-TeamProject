using UnityEngine;

public class CSkillAreaSpawner : MonoBehaviour, ISkill
{
    [Header("소환 설정")]
    public GameObject areaPrefab;
    public float spawnInterval = 1f;
    public float areaLifeTime;

    private CSkillDataSO _data;
    private int _level;
    private float _timer;

    public void Init(CSkillDataSO data, int level)
    {
        _data = data;
        _level = level;
        _timer = spawnInterval;

        if (data.lifeTime != 0f)
            Destroy(gameObject, data.lifeTime);
    }

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= spawnInterval)
        {
            _timer -= spawnInterval;
            SpawnArea();
        }
    }

    private void SpawnArea()
    {
        if (areaPrefab == null) return;

        GameObject area = Instantiate(areaPrefab, transform.position, Quaternion.identity);

        if (_data.useScaleMagnification)
            area.transform.localScale *= (1 + (_level - 1) * 0.1f * _data.scalePreset);

        if (area.TryGetComponent(out ISkill iSkill))
        {
            iSkill.Init(_data, _level);
        }
        Destroy(area, areaLifeTime);
    }
}
