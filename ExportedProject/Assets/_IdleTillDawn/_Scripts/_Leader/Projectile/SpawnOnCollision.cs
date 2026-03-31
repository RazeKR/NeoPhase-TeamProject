using UnityEngine;

namespace flanne
{
    public class SpawnOnCollision : MonoBehaviour
    {
        [SerializeField]
        private string hitTag;

        [SerializeField]
        private string objPoolTag;

        private void OnCollisionEnter2D(Collision2D other)
        {
            // ObjectPooler는 이 프로젝트에 없으므로 충돌 이펙트 스폰은 생략합니다.
            // 추후 프로젝트의 풀링 시스템이 도입되면 여기에 연동할 수 있습니다.
        }
    }
}
