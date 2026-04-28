using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Shared;

internal class NormalizedTextSpanCollection : ReadOnlyCollection<TextSpan>
{
	private class OrderedSpanList : List<TextSpan>
	{
	}

	public NormalizedTextSpanCollection()
		: base((IList<TextSpan>)new List<TextSpan>(0))
	{
	}

	public NormalizedTextSpanCollection(TextSpan span)
		: base(ListFromSpan(span))
	{
	}

	public NormalizedTextSpanCollection(IEnumerable<TextSpan> spans)
		: base(NormalizeSpans(spans))
	{
	}

	public static NormalizedTextSpanCollection Union(NormalizedTextSpanCollection left, NormalizedTextSpanCollection right)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		if (left.Count == 0)
		{
			return right;
		}
		if (right.Count == 0)
		{
			return left;
		}
		OrderedSpanList orderedSpanList = new OrderedSpanList();
		int i = 0;
		int j = 0;
		int start = -1;
		int end = int.MaxValue;
		while (i < left.Count && j < right.Count)
		{
			TextSpan span = left[i];
			TextSpan span2 = right[j];
			if (span.Start < span2.Start)
			{
				UpdateSpanUnion(span, orderedSpanList, ref start, ref end);
				i++;
			}
			else
			{
				UpdateSpanUnion(span2, orderedSpanList, ref start, ref end);
				j++;
			}
		}
		for (; i < left.Count; i++)
		{
			UpdateSpanUnion(left[i], orderedSpanList, ref start, ref end);
		}
		for (; j < right.Count; j++)
		{
			UpdateSpanUnion(right[j], orderedSpanList, ref start, ref end);
		}
		if (end != int.MaxValue)
		{
			orderedSpanList.Add(TextSpan.FromBounds(start, end));
		}
		return new NormalizedTextSpanCollection(orderedSpanList);
	}

	public static NormalizedTextSpanCollection Overlap(NormalizedTextSpanCollection left, NormalizedTextSpanCollection right)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		if (left.Count == 0)
		{
			return left;
		}
		if (right.Count == 0)
		{
			return right;
		}
		OrderedSpanList orderedSpanList = new OrderedSpanList();
		int num = 0;
		int num2 = 0;
		while (num < left.Count && num2 < right.Count)
		{
			TextSpan textSpan = left[num];
			TextSpan span = right[num2];
			if (textSpan.OverlapsWith(span))
			{
				orderedSpanList.Add(textSpan.Overlap(span).Value);
			}
			if (textSpan.End < span.End)
			{
				num++;
			}
			else if (textSpan.End == span.End)
			{
				num++;
				num2++;
			}
			else
			{
				num2++;
			}
		}
		return new NormalizedTextSpanCollection(orderedSpanList);
	}

	public static NormalizedTextSpanCollection Intersection(NormalizedTextSpanCollection left, NormalizedTextSpanCollection right)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		if (left.Count == 0)
		{
			return left;
		}
		if (right.Count == 0)
		{
			return right;
		}
		OrderedSpanList orderedSpanList = new OrderedSpanList();
		int num = 0;
		int num2 = 0;
		while (num < left.Count && num2 < right.Count)
		{
			TextSpan textSpan = left[num];
			TextSpan span = right[num2];
			if (textSpan.IntersectsWith(span))
			{
				orderedSpanList.Add(textSpan.Intersection(span).Value);
			}
			if (textSpan.End < span.End)
			{
				num++;
			}
			else
			{
				num2++;
			}
		}
		return new NormalizedTextSpanCollection(orderedSpanList);
	}

	public static NormalizedTextSpanCollection Difference(NormalizedTextSpanCollection left, NormalizedTextSpanCollection right)
	{
		if (left == null)
		{
			throw new ArgumentNullException("left");
		}
		if (right == null)
		{
			throw new ArgumentNullException("right");
		}
		if (left.Count == 0)
		{
			return left;
		}
		if (right.Count == 0)
		{
			return left;
		}
		OrderedSpanList orderedSpanList = new OrderedSpanList();
		int num = 0;
		int num2 = 0;
		int val = -1;
		do
		{
			TextSpan textSpan = left[num];
			TextSpan textSpan2 = right[num2];
			if (textSpan2.Length == 0 || textSpan.Start >= textSpan2.End)
			{
				num2++;
				continue;
			}
			if (textSpan.End <= textSpan2.Start)
			{
				orderedSpanList.Add(TextSpan.FromBounds(Math.Max(val, textSpan.Start), textSpan.End));
				num++;
				continue;
			}
			if (textSpan.Start < textSpan2.Start)
			{
				orderedSpanList.Add(TextSpan.FromBounds(Math.Max(val, textSpan.Start), textSpan2.Start));
			}
			if (textSpan.End < textSpan2.End)
			{
				num++;
			}
			else if (textSpan.End == textSpan2.End)
			{
				num++;
				num2++;
			}
			else
			{
				val = textSpan2.End;
				num2++;
			}
		}
		while (num < left.Count && num2 < right.Count);
		while (num < left.Count)
		{
			TextSpan textSpan3 = left[num++];
			orderedSpanList.Add(TextSpan.FromBounds(Math.Max(val, textSpan3.Start), textSpan3.End));
		}
		return new NormalizedTextSpanCollection(orderedSpanList);
	}

	public static bool operator ==(NormalizedTextSpanCollection left, NormalizedTextSpanCollection right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if ((object)left == null || (object)right == null)
		{
			return false;
		}
		if (left.Count != right.Count)
		{
			return false;
		}
		for (int i = 0; i < left.Count; i++)
		{
			if (left[i] != right[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool operator !=(NormalizedTextSpanCollection left, NormalizedTextSpanCollection right)
	{
		return !(left == right);
	}

	public bool OverlapsWith(NormalizedTextSpanCollection set)
	{
		if (set == null)
		{
			throw new ArgumentNullException("set");
		}
		int num = 0;
		int num2 = 0;
		while (num < base.Count && num2 < set.Count)
		{
			TextSpan textSpan = base[num];
			TextSpan span = set[num2];
			if (textSpan.OverlapsWith(span))
			{
				return true;
			}
			if (textSpan.End < span.End)
			{
				num++;
			}
			else if (textSpan.End == span.End)
			{
				num++;
				num2++;
			}
			else
			{
				num2++;
			}
		}
		return false;
	}

	public bool OverlapsWith(TextSpan span)
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i].OverlapsWith(span))
			{
				return true;
			}
		}
		return false;
	}

	public bool IntersectsWith(NormalizedTextSpanCollection set)
	{
		if (set == null)
		{
			throw new ArgumentNullException("set");
		}
		int num = 0;
		int num2 = 0;
		while (num < base.Count && num2 < set.Count)
		{
			TextSpan textSpan = base[num];
			TextSpan span = set[num2];
			if (textSpan.IntersectsWith(span))
			{
				return true;
			}
			if (textSpan.End < span.End)
			{
				num++;
			}
			else
			{
				num2++;
			}
		}
		return false;
	}

	public bool IntersectsWith(TextSpan span)
	{
		for (int i = 0; i < base.Count; i++)
		{
			if (base[i].IntersectsWith(span))
			{
				return true;
			}
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = 0;
		using IEnumerator<TextSpan> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			num ^= enumerator.Current.GetHashCode();
		}
		return num;
	}

	public override bool Equals(object obj)
	{
		NormalizedTextSpanCollection normalizedTextSpanCollection = obj as NormalizedTextSpanCollection;
		return this == normalizedTextSpanCollection;
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("{");
		using (IEnumerator<TextSpan> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				stringBuilder.Append(enumerator.Current.ToString());
			}
		}
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	private static IList<TextSpan> ListFromSpan(TextSpan span)
	{
		return new List<TextSpan>(1) { span };
	}

	private NormalizedTextSpanCollection(OrderedSpanList normalizedSpans)
		: base((IList<TextSpan>)normalizedSpans)
	{
	}

	private static void UpdateSpanUnion(TextSpan span, IList<TextSpan> spans, ref int start, ref int end)
	{
		if (end < span.Start)
		{
			spans.Add(TextSpan.FromBounds(start, end));
			start = -1;
			end = int.MaxValue;
		}
		if (end == int.MaxValue)
		{
			start = span.Start;
			end = span.End;
		}
		else
		{
			end = Math.Max(end, span.End);
		}
	}

	private static IList<TextSpan> NormalizeSpans(IEnumerable<TextSpan> spans)
	{
		if (spans == null)
		{
			throw new ArgumentNullException("spans");
		}
		List<TextSpan> list = new List<TextSpan>(spans);
		if (list.Count <= 1)
		{
			return list;
		}
		list.Sort((TextSpan s1, TextSpan s2) => s1.Start.CompareTo(s2.Start));
		IList<TextSpan> list2 = new List<TextSpan>(list.Count);
		int start = list[0].Start;
		int num = list[0].End;
		for (int i = 1; i < list.Count; i++)
		{
			int start2 = list[i].Start;
			int end = list[i].End;
			if (num < start2)
			{
				list2.Add(TextSpan.FromBounds(start, num));
				start = start2;
				num = end;
			}
			else
			{
				num = Math.Max(num, end);
			}
		}
		list2.Add(TextSpan.FromBounds(start, num));
		return list2;
	}
}
