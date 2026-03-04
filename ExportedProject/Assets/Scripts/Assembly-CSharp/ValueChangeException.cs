using System.Collections.Generic;

public class ValueChangeException : BaseException
{
	public readonly float fromValue;

	public readonly float toValue;

	private List<ValueModifier> modifiers;

	public float delta => toValue - fromValue;

	public ValueChangeException(float fromValue, float toValue)
		: base(defaultToggle: true)
	{
		this.fromValue = fromValue;
		this.toValue = toValue;
	}

	public void AddModifier(ValueModifier m)
	{
		if (modifiers == null)
		{
			modifiers = new List<ValueModifier>();
		}
		modifiers.Add(m);
	}

	public float GetModifiedValue()
	{
		if (modifiers == null)
		{
			return toValue;
		}
		float result = toValue;
		modifiers.Sort(Compare);
		for (int i = 0; i < modifiers.Count; i++)
		{
			result = modifiers[i].Modify(fromValue, result);
		}
		return result;
	}

	private int Compare(ValueModifier x, ValueModifier y)
	{
		return x.sortOrder.CompareTo(y.sortOrder);
	}
}
