using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Formatting;

internal struct TriviaList : IEnumerable<SyntaxTrivia>, IEnumerable
{
	public struct Enumerator : IEnumerator<SyntaxTrivia>, IEnumerator, IDisposable
	{
		private readonly SyntaxTriviaList _list1;

		private readonly SyntaxTriviaList _list2;

		private SyntaxTriviaList.Enumerator _enumerator;

		private int _index;

		public SyntaxTrivia Current => _enumerator.Current;

		object IEnumerator.Current => Current;

		internal Enumerator(TriviaList triviaList)
		{
			_list1 = triviaList.list1;
			_list2 = triviaList.list2;
			_index = -1;
			_enumerator = _list1.GetEnumerator();
		}

		public bool MoveNext()
		{
			_index++;
			if (_index == _list1.Count)
			{
				_enumerator = _list2.GetEnumerator();
			}
			return _enumerator.MoveNext();
		}

		void IDisposable.Dispose()
		{
		}

		void IEnumerator.Reset()
		{
		}
	}

	private readonly SyntaxTriviaList list1;

	private readonly SyntaxTriviaList list2;

	public int Count => list1.Count + list2.Count;

	public TriviaList(SyntaxTriviaList list1, SyntaxTriviaList list2)
	{
		this.list1 = list1;
		this.list2 = list2;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	IEnumerator<SyntaxTrivia> IEnumerable<SyntaxTrivia>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
