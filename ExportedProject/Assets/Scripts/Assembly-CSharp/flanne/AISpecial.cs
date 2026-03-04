using UnityEngine;

namespace flanne
{
	public abstract class AISpecial : ScriptableObject
	{
		public abstract void Use(AIComponent ai, Transform target);
	}
}
