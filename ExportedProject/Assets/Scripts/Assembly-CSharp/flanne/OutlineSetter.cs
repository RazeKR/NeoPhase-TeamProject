using UnityEngine;

namespace flanne
{
	public class OutlineSetter : MonoBehaviour
	{
		public static OutlineSetter Instance;

		[SerializeField]
		private Material outlineMaterial;

		public static bool isOn
		{
			get
			{
				return Instance.outlineMaterial.GetColor("_OutlineColor").a == 1f;
			}
			set
			{
				if (!(Instance == null))
				{
					Color color = Instance.outlineMaterial.GetColor("_OutlineColor");
					if (value)
					{
						color.a = 1f;
					}
					else
					{
						color.a = 0f;
					}
					Instance.outlineMaterial.SetColor("_OutlineColor", color);
				}
			}
		}

		private void Awake()
		{
			if (Instance != null)
			{
				Object.Destroy(base.gameObject);
			}
			Instance = this;
		}
	}
}
