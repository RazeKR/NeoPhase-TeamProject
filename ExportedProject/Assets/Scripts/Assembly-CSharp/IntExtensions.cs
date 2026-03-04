using System.Collections.Generic;
using UnityEngine;

public static class IntExtensions
{
	public static int NotifyModifiers<T>(this int value, string notification, object notifier, T e)
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
		return Mathf.CeilToInt(num);
	}

	public static int NotifyModifiers(this int value, string notification, object notifier)
	{
		List<ValueModifier> list = new List<ValueModifier>();
		notifier.PostNotification(notification, list);
		list.Sort(Compare);
		float num = value;
		for (int i = 0; i < list.Count; i++)
		{
			num = list[i].Modify(value, num);
		}
		return Mathf.FloorToInt(num);
	}

	private static int Compare(ValueModifier x, ValueModifier y)
	{
		return x.sortOrder.CompareTo(y.sortOrder);
	}
}
