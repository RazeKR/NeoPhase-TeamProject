using System.Collections.Generic;

public static class FloatExtensions
{
	public static float NotifyModifiers<T>(this float value, string notification, object notifier, T e)
	{
		List<ValueModifier> list = new List<ValueModifier>();
		Info<List<ValueModifier>, T> e2 = new Info<List<ValueModifier>, T>(list, e);
		notifier.PostNotification(notification, e2);
		list.Sort(Compare);
		float num = value;
		for (int i = 0; i < list.Count; i++)
		{
			num = list[i].Modify(value, num);
		}
		return num;
	}

	public static float NotifyModifiers(this float value, string notification, object notifier)
	{
		List<ValueModifier> list = new List<ValueModifier>();
		notifier.PostNotification(notification, list);
		list.Sort(Compare);
		float num = value;
		for (int i = 0; i < list.Count; i++)
		{
			num = list[i].Modify(value, num);
		}
		return num;
	}

	private static int Compare(ValueModifier x, ValueModifier y)
	{
		return x.sortOrder.CompareTo(y.sortOrder);
	}
}
