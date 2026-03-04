using UnityEngine;

namespace flanne
{
	public class ShanasHalo : MonoBehaviour
	{
		[SerializeField]
		private int piecesRequired;

		[SerializeField]
		private GameObject haloPrefab;

		[SerializeField]
		private GameObject haloSpriteObj;

		[SerializeField]
		private SoundEffectSO soundFX;

		private int counter = 1;

		public void CollectPiece()
		{
			counter++;
			if (counter >= piecesRequired)
			{
				DropHalo();
			}
		}

		public void DropHalo()
		{
			Object.Instantiate(haloPrefab).transform.position = base.transform.position + new Vector3(0f, -1.5f, 0f);
			soundFX?.Play();
		}

		public void ActivateSprite()
		{
			haloSpriteObj.SetActive(value: true);
			soundFX?.Play();
		}
	}
}
