using UnityEngine;

public class CProjectileSkill : MonoBehaviour
{
    [Header("捧荤眉 可记")]
    public float speed = 10f;
    public float lifeTime = 3f;


    // lifeTime 第 昏力 贸府
    private void Start() => Destroy(gameObject, lifeTime);

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }
}
