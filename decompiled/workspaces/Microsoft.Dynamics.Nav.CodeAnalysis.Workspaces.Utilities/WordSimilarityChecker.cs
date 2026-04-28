using System;
using System.Collections.Generic;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

internal class WordSimilarityChecker
{
	private struct CacheResult
	{
		public readonly string CandidateText;

		public readonly bool AreSimilar;

		public readonly double SimilarityWeight;

		public CacheResult(string candidate, bool areSimilar, double similarityWeight)
		{
			CandidateText = candidate;
			AreSimilar = areSimilar;
			SimilarityWeight = similarityWeight;
		}
	}

	private CacheResult lastAreSimilarResult;

	private string source;

	private EditDistance editDistance;

	private int threshold;

	private bool substringsAreSimilar;

	private static readonly object poolGate = new object();

	private static readonly Stack<WordSimilarityChecker> pool = new Stack<WordSimilarityChecker>();

	public static WordSimilarityChecker Allocate(string text, bool substringsAreSimilar)
	{
		WordSimilarityChecker wordSimilarityChecker;
		lock (poolGate)
		{
			wordSimilarityChecker = ((pool.Count > 0) ? pool.Pop() : new WordSimilarityChecker());
		}
		wordSimilarityChecker.Initialize(text, substringsAreSimilar);
		return wordSimilarityChecker;
	}

	private WordSimilarityChecker()
	{
	}

	private void Initialize(string text, bool substringsAreSimilar)
	{
		source = text ?? throw new ArgumentNullException("text");
		threshold = GetThreshold(source);
		editDistance = new EditDistance(text);
		this.substringsAreSimilar = substringsAreSimilar;
	}

	public void Free()
	{
		editDistance?.Dispose();
		source = null;
		editDistance = null;
		lastAreSimilarResult = default(CacheResult);
		lock (poolGate)
		{
			pool.Push(this);
		}
	}

	public static bool AreSimilar(string originalText, string candidateText)
	{
		return AreSimilar(originalText, candidateText, substringsAreSimilar: false);
	}

	public static bool AreSimilar(string originalText, string candidateText, bool substringsAreSimilar)
	{
		double similarityWeight;
		return AreSimilar(originalText, candidateText, substringsAreSimilar, out similarityWeight);
	}

	public static bool AreSimilar(string originalText, string candidateText, out double similarityWeight)
	{
		return AreSimilar(originalText, candidateText, substringsAreSimilar: false, out similarityWeight);
	}

	public static bool AreSimilar(string originalText, string candidateText, bool substringsAreSimilar, out double similarityWeight)
	{
		WordSimilarityChecker wordSimilarityChecker = Allocate(originalText, substringsAreSimilar);
		bool result = wordSimilarityChecker.AreSimilar(candidateText, out similarityWeight);
		wordSimilarityChecker.Free();
		return result;
	}

	internal static int GetThreshold(string value)
	{
		if (value.Length > 4)
		{
			return 2;
		}
		return 1;
	}

	public bool AreSimilar(string candidateText)
	{
		double similarityWeight;
		return AreSimilar(candidateText, out similarityWeight);
	}

	public bool AreSimilar(string candidateText, out double similarityWeight)
	{
		if (source.Length < 3)
		{
			similarityWeight = double.MaxValue;
			return false;
		}
		if (lastAreSimilarResult.CandidateText == candidateText)
		{
			similarityWeight = lastAreSimilarResult.SimilarityWeight;
			return lastAreSimilarResult.AreSimilar;
		}
		bool flag = AreSimilarWorker(candidateText, out similarityWeight);
		lastAreSimilarResult = new CacheResult(candidateText, flag, similarityWeight);
		return flag;
	}

	private bool AreSimilarWorker(string candidateText, out double similarityWeight)
	{
		similarityWeight = double.MaxValue;
		if (Math.Abs(source.Length - candidateText.Length) <= threshold)
		{
			similarityWeight = editDistance.GetEditDistance(candidateText, threshold);
		}
		if (similarityWeight > (double)threshold)
		{
			if (!substringsAreSimilar || candidateText.IndexOf(source, StringComparison.OrdinalIgnoreCase) < 0)
			{
				return false;
			}
			similarityWeight = threshold;
		}
		similarityWeight += Penalty(candidateText, source);
		return true;
	}

	private static double Penalty(string candidateText, string originalText)
	{
		int num = Math.Abs(originalText.Length - candidateText.Length);
		if (num != 0)
		{
			return 1.0 - 1.0 / (double)(num + 1);
		}
		return 0.0;
	}
}
