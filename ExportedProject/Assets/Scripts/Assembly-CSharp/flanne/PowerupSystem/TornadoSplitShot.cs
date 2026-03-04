using UnityEngine;

namespace flanne.PowerupSystem
{
	public class TornadoSplitShot : MonoBehaviour
	{
		private void OnTornadoCollideBullet(object sender, object args)
		{
			Projectile component = (args as GameObject).GetComponent<Projectile>();
			if (!(component == null) && !component.isSecondary)
			{
				component.isSecondary = true;
				component.damage /= 2f;
				Projectile component2 = Object.Instantiate(component.gameObject).GetComponent<Projectile>();
				float magnitude = component.vector.magnitude;
				component.vector = Vector2.up.Rotate(Random.Range(0, 360)) * magnitude;
				component2.vector = Vector2.up.Rotate(Random.Range(0, 360)) * magnitude;
			}
		}

		private void Start()
		{
			this.AddObserver(OnTornadoCollideBullet, "TornadoBulletCollision");
		}

		private void OnDestroy()
		{
			this.RemoveObserver(OnTornadoCollideBullet, "TornadoBulletCollision");
		}
	}
}
