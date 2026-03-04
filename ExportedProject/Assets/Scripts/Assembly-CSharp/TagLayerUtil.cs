using UnityEngine;

public static class TagLayerUtil
{
	public static LayerMask FogOfWar;

	public static LayerMask VisibleFog;

	public static LayerMask Pickup;

	public static LayerMask PlayerPickupper;

	public static LayerMask Player;

	public static LayerMask PlayerProjectile;

	public static LayerMask PlayerProjectileMod;

	public static LayerMask Enemy;

	public static LayerMask EnemyProjectile;

	public static void Init()
	{
		FogOfWar = LayerMask.NameToLayer("FogOfWar");
		VisibleFog = LayerMask.NameToLayer("VisibleFog");
		Pickup = LayerMask.NameToLayer("Pickup");
		PlayerPickupper = LayerMask.NameToLayer("PlayerPickupper");
		Player = LayerMask.NameToLayer("Player");
		PlayerProjectile = LayerMask.NameToLayer("PlayerProjectile");
		PlayerProjectileMod = LayerMask.NameToLayer("PlayerProjectileMod");
		Enemy = LayerMask.NameToLayer("Enemy");
		EnemyProjectile = LayerMask.NameToLayer("EnemyProjectile");
	}
}
