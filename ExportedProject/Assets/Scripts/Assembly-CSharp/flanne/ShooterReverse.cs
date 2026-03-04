using UnityEngine;

namespace flanne
{
	public class ShooterReverse : Shooter
	{
		public override void Shoot(ProjectileRecipe recipe, Vector2 pointDirection, int numProjectiles, float spread, float inaccuracy)
		{
			pointDirection *= -1f;
			base.Shoot(recipe, pointDirection, numProjectiles, spread, inaccuracy);
		}
	}
}
