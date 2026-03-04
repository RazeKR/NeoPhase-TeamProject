using UnityEngine;

namespace flanne
{
	[CreateAssetMenu(fileName = "DifficultyModList", menuName = "DifficultyModList")]
	public class DifficultyModList : ScriptableObject
	{
		public DifficultyModifier[] mods;
	}
}
