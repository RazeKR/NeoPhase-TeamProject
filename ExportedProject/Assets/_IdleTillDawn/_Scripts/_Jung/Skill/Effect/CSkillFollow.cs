using UnityEngine;

public class CSkillFollow : MonoBehaviour
{
    private Transform _targetTransform;


    public void SetTarget(Transform target)
    {
        _targetTransform = target;

        if (_targetTransform != null)
            transform.position = _targetTransform.position;
    }

    private void LateUpdate()
    {
        if (_targetTransform != null) 
            transform.position = _targetTransform.position;

        else Destroy(gameObject);
    }
}
