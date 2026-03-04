using System;

[Serializable]
public struct StatChange
{
	public StatType type;

	public bool isFlatMod;

	public int flatValue;

	public float value;
}
