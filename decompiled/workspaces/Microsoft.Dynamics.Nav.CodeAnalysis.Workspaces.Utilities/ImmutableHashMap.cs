using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Workspaces.Utilities;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(ImmutableHashMap<, >.DebuggerProxy))]
internal sealed class ImmutableHashMap<TKey, TValue> : IImmutableDictionary<TKey, TValue>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyCollection<KeyValuePair<TKey, TValue>>, IReadOnlyDictionary<TKey, TValue>
{
	private abstract class Bucket
	{
		internal abstract int Count { get; }

		internal abstract Bucket Add(int suggestedHashRoll, ValueBucket bucket, IEqualityComparer<TKey> comparer, IEqualityComparer<TValue> valueComparer, bool overwriteExistingValue);

		internal abstract Bucket Remove(int hash, TKey key, IEqualityComparer<TKey> comparer);

		internal abstract ValueBucket Get(int hash, TKey key, IEqualityComparer<TKey> comparer);

		internal abstract IEnumerable<Bucket> GetAll();
	}

	private abstract class ValueOrListBucket : Bucket
	{
		internal readonly int Hash;

		protected ValueOrListBucket(int hash)
		{
			Hash = hash;
		}
	}

	private sealed class ValueBucket : ValueOrListBucket
	{
		internal readonly TKey Key;

		internal readonly TValue Value;

		internal override int Count => 1;

		internal ValueBucket(TKey key, TValue value, int hashcode)
			: base(hashcode)
		{
			Key = key;
			Value = value;
		}

		internal override Bucket Add(int suggestedHashRoll, ValueBucket bucket, IEqualityComparer<TKey> comparer, IEqualityComparer<TValue> valueComparer, bool overwriteExistingValue)
		{
			if (Hash == bucket.Hash)
			{
				if (comparer.Equals(Key, bucket.Key))
				{
					if (valueComparer.Equals(Value, bucket.Value))
					{
						return this;
					}
					if (overwriteExistingValue)
					{
						return bucket;
					}
					throw new ArgumentException(Strings.DuplicateKey);
				}
				return new ListBucket(new ValueBucket[2] { this, bucket });
			}
			return new HashBucket(suggestedHashRoll, this, bucket);
		}

		internal override Bucket Remove(int hash, TKey key, IEqualityComparer<TKey> comparer)
		{
			if (Hash == hash && comparer.Equals(Key, key))
			{
				return null;
			}
			return this;
		}

		internal override ValueBucket Get(int hash, TKey key, IEqualityComparer<TKey> comparer)
		{
			if (Hash == hash && comparer.Equals(Key, key))
			{
				return this;
			}
			return null;
		}

		internal override IEnumerable<Bucket> GetAll()
		{
			return SpecializedCollections.SingletonEnumerable(this);
		}
	}

	private sealed class ListBucket : ValueOrListBucket
	{
		private readonly ValueBucket[] _buckets;

		internal override int Count => _buckets.Length;

		internal ListBucket(ValueBucket[] buckets)
			: base(buckets[0].Hash)
		{
			_buckets = buckets;
		}

		internal override Bucket Add(int suggestedHashRoll, ValueBucket bucket, IEqualityComparer<TKey> comparer, IEqualityComparer<TValue> valueComparer, bool overwriteExistingValue)
		{
			if (Hash == bucket.Hash)
			{
				int num = Find(bucket.Key, comparer);
				if (num >= 0)
				{
					if (valueComparer.Equals(bucket.Value, _buckets[num].Value))
					{
						return this;
					}
					if (overwriteExistingValue)
					{
						return new ListBucket(_buckets.ReplaceAt(num, bucket));
					}
					throw new ArgumentException(Strings.DuplicateKey);
				}
				return new ListBucket(_buckets.InsertAt(_buckets.Length, bucket));
			}
			return new HashBucket(suggestedHashRoll, this, bucket);
		}

		internal override Bucket Remove(int hash, TKey key, IEqualityComparer<TKey> comparer)
		{
			if (Hash == hash)
			{
				int num = Find(key, comparer);
				if (num >= 0)
				{
					if (_buckets.Length == 1)
					{
						return null;
					}
					if (_buckets.Length == 2)
					{
						if (num != 0)
						{
							return _buckets[0];
						}
						return _buckets[1];
					}
					return new ListBucket(_buckets.RemoveAt(num));
				}
			}
			return this;
		}

		internal override ValueBucket Get(int hash, TKey key, IEqualityComparer<TKey> comparer)
		{
			if (Hash == hash)
			{
				int num = Find(key, comparer);
				if (num >= 0)
				{
					return _buckets[num];
				}
			}
			return null;
		}

		private int Find(TKey key, IEqualityComparer<TKey> comparer)
		{
			for (int i = 0; i < _buckets.Length; i++)
			{
				if (comparer.Equals(key, _buckets[i].Key))
				{
					return i;
				}
			}
			return -1;
		}

		internal override IEnumerable<Bucket> GetAll()
		{
			return _buckets;
		}
	}

	private sealed class HashBucket : Bucket
	{
		private readonly int _hashRoll;

		private readonly uint _used;

		private readonly Bucket[] _buckets;

		private readonly int _count;

		internal override int Count => _count;

		private HashBucket(int hashRoll, uint used, Bucket[] buckets, int count)
		{
			_hashRoll = hashRoll & 0x1F;
			_used = used;
			_buckets = buckets;
			_count = count;
		}

		internal HashBucket(int suggestedHashRoll, ValueOrListBucket bucket1, ValueOrListBucket bucket2)
		{
			int hash = bucket1.Hash;
			int hash2 = bucket2.Hash;
			for (int i = 0; i < 32; i++)
			{
				_hashRoll = (suggestedHashRoll + i) & 0x1F;
				int num = ComputeLogicalSlot(hash);
				int num2 = ComputeLogicalSlot(hash2);
				if (num != num2)
				{
					_count = 2;
					_used = (uint)((1 << num) | (1 << num2));
					_buckets = new Bucket[2];
					_buckets[ComputePhysicalSlot(num)] = bucket1;
					_buckets[ComputePhysicalSlot(num2)] = bucket2;
					return;
				}
			}
			throw new InvalidOperationException();
		}

		internal override Bucket Add(int suggestedHashRoll, ValueBucket bucket, IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer, bool overwriteExistingValue)
		{
			int num = ComputeLogicalSlot(bucket.Hash);
			if (IsInUse(num))
			{
				int num2 = ComputePhysicalSlot(num);
				Bucket bucket2 = _buckets[num2];
				Bucket bucket3 = bucket2.Add(_hashRoll + 5, bucket, keyComparer, valueComparer, overwriteExistingValue);
				if (bucket3 != bucket2)
				{
					Bucket[] buckets = _buckets.ReplaceAt(num2, bucket3);
					return new HashBucket(_hashRoll, _used, buckets, _count - bucket2.Count + bucket3.Count);
				}
				return this;
			}
			int position = ComputePhysicalSlot(num);
			Bucket[] buckets2 = _buckets.InsertAt(position, bucket);
			uint used = InsertBit(num, _used);
			return new HashBucket(_hashRoll, used, buckets2, _count + bucket.Count);
		}

		internal override Bucket Remove(int hash, TKey key, IEqualityComparer<TKey> comparer)
		{
			int num = ComputeLogicalSlot(hash);
			if (IsInUse(num))
			{
				int num2 = ComputePhysicalSlot(num);
				Bucket bucket = _buckets[num2];
				Bucket bucket2 = bucket.Remove(hash, key, comparer);
				if (bucket2 == null)
				{
					if (_buckets.Length == 1)
					{
						return null;
					}
					if (_buckets.Length == 2)
					{
						if (num2 != 0)
						{
							return _buckets[0];
						}
						return _buckets[1];
					}
					return new HashBucket(_hashRoll, RemoveBit(num, _used), _buckets.RemoveAt(num2), _count - bucket.Count);
				}
				if (_buckets[num2] != bucket2)
				{
					return new HashBucket(_hashRoll, _used, _buckets.ReplaceAt(num2, bucket2), _count - bucket.Count + bucket2.Count);
				}
			}
			return this;
		}

		internal override ValueBucket Get(int hash, TKey key, IEqualityComparer<TKey> comparer)
		{
			int logicalSlot = ComputeLogicalSlot(hash);
			if (IsInUse(logicalSlot))
			{
				int num = ComputePhysicalSlot(logicalSlot);
				return _buckets[num].Get(hash, key, comparer);
			}
			return null;
		}

		internal override IEnumerable<Bucket> GetAll()
		{
			return _buckets;
		}

		private bool IsInUse(int logicalSlot)
		{
			return ((1 << logicalSlot) & _used) != 0;
		}

		private int ComputeLogicalSlot(int hc)
		{
			return (int)(RotateRight((uint)hc, _hashRoll) & 0x1F);
		}

		private static uint RotateRight(uint v, int n)
		{
			if (n == 0)
			{
				return v;
			}
			return (v >> n) | (v << 32 - n);
		}

		private int ComputePhysicalSlot(int logicalSlot)
		{
			if (_buckets.Length == 32)
			{
				return logicalSlot;
			}
			if (logicalSlot == 0)
			{
				return 0;
			}
			uint num = uint.MaxValue >> 32 - logicalSlot;
			return CountBits(_used & num);
		}

		private static int CountBits(uint v)
		{
			v -= (v >> 1) & 0x55555555;
			v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
			return (int)(((v + (v >> 4)) & 0xF0F0F0F) * 16843009) >> 24;
		}

		private static uint InsertBit(int position, uint bits)
		{
			return bits | (uint)(1 << position);
		}

		private static uint RemoveBit(int position, uint bits)
		{
			return bits & (uint)(~(1 << position));
		}
	}

	private class DebuggerProxy
	{
		private readonly ImmutableHashMap<TKey, TValue> _map;

		private KeyValuePair<TKey, TValue>[] _contents;

		[DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
		public KeyValuePair<TKey, TValue>[] Contents
		{
			get
			{
				if (_contents == null)
				{
					_contents = _map.ToArray();
				}
				return _contents;
			}
		}

		public DebuggerProxy(ImmutableHashMap<TKey, TValue> map)
		{
			Requires.NotNull(map, "map");
			_map = map;
		}
	}

	private static class Requires
	{
		[DebuggerStepThrough]
		public static T NotNullAllowStructs<T>(T value, string parameterName)
		{
			if (value == null)
			{
				throw new ArgumentNullException(parameterName);
			}
			return value;
		}

		[DebuggerStepThrough]
		public static T NotNull<T>(T value, string parameterName) where T : class
		{
			if (value == null)
			{
				throw new ArgumentNullException(parameterName);
			}
			return value;
		}

		[DebuggerStepThrough]
		public static Exception FailRange(string parameterName, string message = null)
		{
			if (string.IsNullOrEmpty(message))
			{
				throw new ArgumentOutOfRangeException(parameterName);
			}
			throw new ArgumentOutOfRangeException(parameterName, message);
		}

		[DebuggerStepThrough]
		public static void Range(bool condition, string parameterName, string message = null)
		{
			if (!condition)
			{
				FailRange(parameterName, message);
			}
		}
	}

	private static class Strings
	{
		public static string DuplicateKey => WorkspacesResources.DuplicateKey;
	}

	private readonly Bucket _root;

	private readonly IEqualityComparer<TKey> _keyComparer;

	private readonly IEqualityComparer<TValue> _valueComparer;

	public static ImmutableHashMap<TKey, TValue> Empty { get; } = new ImmutableHashMap<TKey, TValue>();


	public int Count
	{
		get
		{
			if (_root == null)
			{
				return 0;
			}
			return _root.Count;
		}
	}

	public bool IsEmpty => Count == 0;

	public IEnumerable<TKey> Keys
	{
		get
		{
			if (_root == null)
			{
				yield break;
			}
			Stack<IEnumerator<Bucket>> stack = new Stack<IEnumerator<Bucket>>();
			stack.Push(_root.GetAll().GetEnumerator());
			while (stack.Count > 0)
			{
				IEnumerator<Bucket> enumerator = stack.Peek();
				if (enumerator.MoveNext())
				{
					if (enumerator.Current is ValueBucket valueBucket)
					{
						yield return valueBucket.Key;
					}
					else
					{
						stack.Push(enumerator.Current.GetAll().GetEnumerator());
					}
				}
				else
				{
					stack.Pop();
				}
			}
		}
	}

	public IEnumerable<TValue> Values => from vb in GetValueBuckets()
		select vb.Value;

	public TValue this[TKey key]
	{
		get
		{
			if (TryGetValue(key, out var value))
			{
				return value;
			}
			throw new KeyNotFoundException();
		}
	}

	private ImmutableHashMap(Bucket root, IEqualityComparer<TKey> comparer, IEqualityComparer<TValue> valueComparer)
		: this(comparer, valueComparer)
	{
		_root = root;
	}

	internal ImmutableHashMap(IEqualityComparer<TKey> comparer = null, IEqualityComparer<TValue> valueComparer = null)
	{
		_keyComparer = comparer ?? EqualityComparer<TKey>.Default;
		_valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
	}

	public ImmutableHashMap<TKey, TValue> Clear()
	{
		if (!IsEmpty)
		{
			return Empty.WithComparers(_keyComparer, _valueComparer);
		}
		return this;
	}

	public ImmutableHashMap<TKey, TValue> Add(TKey key, TValue value)
	{
		Requires.NotNullAllowStructs(key, "key");
		ValueBucket valueBucket = new ValueBucket(key, value, _keyComparer.GetHashCode(key));
		if (_root == null)
		{
			return Wrap(valueBucket);
		}
		return Wrap(_root.Add(0, valueBucket, _keyComparer, _valueComparer, overwriteExistingValue: false));
	}

	public ImmutableHashMap<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
	{
		Requires.NotNull(pairs, "pairs");
		return AddRange(pairs, overwriteOnCollision: false, avoidToHashMap: false);
	}

	public ImmutableHashMap<TKey, TValue> SetItem(TKey key, TValue value)
	{
		Requires.NotNullAllowStructs(key, "key");
		ValueBucket valueBucket = new ValueBucket(key, value, _keyComparer.GetHashCode(key));
		if (_root == null)
		{
			return Wrap(valueBucket);
		}
		return Wrap(_root.Add(0, valueBucket, _keyComparer, _valueComparer, overwriteExistingValue: true));
	}

	public ImmutableHashMap<TKey, TValue> SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
	{
		Requires.NotNull(items, "items");
		return AddRange(items, overwriteOnCollision: true, avoidToHashMap: false);
	}

	public ImmutableHashMap<TKey, TValue> Remove(TKey key)
	{
		Requires.NotNullAllowStructs(key, "key");
		if (_root != null)
		{
			return Wrap(_root.Remove(_keyComparer.GetHashCode(key), key, _keyComparer));
		}
		return this;
	}

	public ImmutableHashMap<TKey, TValue> RemoveRange(IEnumerable<TKey> keys)
	{
		Requires.NotNull(keys, "keys");
		Bucket bucket = _root;
		if (bucket != null)
		{
			foreach (TKey key in keys)
			{
				bucket = bucket.Remove(_keyComparer.GetHashCode(key), key, _keyComparer);
				if (bucket == null)
				{
					break;
				}
			}
		}
		return Wrap(bucket);
	}

	public ImmutableHashMap<TKey, TValue> WithComparers(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
	{
		if (keyComparer == null)
		{
			keyComparer = EqualityComparer<TKey>.Default;
		}
		if (valueComparer == null)
		{
			valueComparer = EqualityComparer<TValue>.Default;
		}
		if (_keyComparer == keyComparer)
		{
			if (_valueComparer == valueComparer)
			{
				return this;
			}
			return new ImmutableHashMap<TKey, TValue>(_root, _keyComparer, valueComparer);
		}
		return new ImmutableHashMap<TKey, TValue>(keyComparer, valueComparer).AddRange(this, overwriteOnCollision: false, avoidToHashMap: true);
	}

	public ImmutableHashMap<TKey, TValue> WithComparers(IEqualityComparer<TKey> keyComparer)
	{
		return WithComparers(keyComparer, _valueComparer);
	}

	public bool ContainsValue(TValue value)
	{
		return Values.Contains(value, _valueComparer);
	}

	public bool ContainsKey(TKey key)
	{
		if (_root != null)
		{
			return _root.Get(_keyComparer.GetHashCode(key), key, _keyComparer) != null;
		}
		return false;
	}

	public bool Contains(KeyValuePair<TKey, TValue> keyValuePair)
	{
		if (_root != null)
		{
			ValueBucket valueBucket = _root.Get(_keyComparer.GetHashCode(keyValuePair.Key), keyValuePair.Key, _keyComparer);
			if (valueBucket != null)
			{
				return _valueComparer.Equals(valueBucket.Value, keyValuePair.Value);
			}
			return false;
		}
		return false;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		if (_root != null)
		{
			ValueBucket valueBucket = _root.Get(_keyComparer.GetHashCode(key), key, _keyComparer);
			if (valueBucket != null)
			{
				value = valueBucket.Value;
				return true;
			}
		}
		value = default(TValue);
		return false;
	}

	public bool TryGetKey(TKey equalKey, out TKey actualKey)
	{
		if (_root != null)
		{
			ValueBucket valueBucket = _root.Get(_keyComparer.GetHashCode(equalKey), equalKey, _keyComparer);
			if (valueBucket != null)
			{
				actualKey = valueBucket.Key;
				return true;
			}
		}
		actualKey = equalKey;
		return false;
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return (from vb in GetValueBuckets()
			select new KeyValuePair<TKey, TValue>(vb.Key, vb.Value)).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder("ImmutableHashMap[");
		bool flag = false;
		using (IEnumerator<KeyValuePair<TKey, TValue>> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				KeyValuePair<TKey, TValue> current = enumerator.Current;
				stringBuilder.Append(current.Key);
				stringBuilder.Append(":");
				stringBuilder.Append(current.Value);
				if (flag)
				{
					stringBuilder.Append(",");
				}
				flag = true;
			}
		}
		stringBuilder.Append("]");
		return stringBuilder.ToString();
	}

	internal bool TryExchangeKey(TKey key, out TKey existingKey)
	{
		ValueBucket valueBucket = ((_root != null) ? _root.Get(_keyComparer.GetHashCode(key), key, _keyComparer) : null);
		if (valueBucket != null)
		{
			existingKey = valueBucket.Key;
			return true;
		}
		existingKey = default(TKey);
		return false;
	}

	private static bool TryCastToImmutableMap(IEnumerable<KeyValuePair<TKey, TValue>> sequence, out ImmutableHashMap<TKey, TValue> other)
	{
		other = sequence as ImmutableHashMap<TKey, TValue>;
		if (other != null)
		{
			return true;
		}
		return false;
	}

	private ImmutableHashMap<TKey, TValue> Wrap(Bucket root)
	{
		if (root == null)
		{
			return Clear();
		}
		if (_root != root)
		{
			if (root.Count != 0)
			{
				return new ImmutableHashMap<TKey, TValue>(root, _keyComparer, _valueComparer);
			}
			return Clear();
		}
		return this;
	}

	private ImmutableHashMap<TKey, TValue> AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs, bool overwriteOnCollision, bool avoidToHashMap)
	{
		if (IsEmpty && !avoidToHashMap && TryCastToImmutableMap(pairs, out ImmutableHashMap<TKey, TValue> other))
		{
			return other.WithComparers(_keyComparer, _valueComparer);
		}
		ImmutableHashMap<TKey, TValue> immutableHashMap = this;
		foreach (KeyValuePair<TKey, TValue> pair in pairs)
		{
			immutableHashMap = (overwriteOnCollision ? immutableHashMap.SetItem(pair.Key, pair.Value) : immutableHashMap.Add(pair.Key, pair.Value));
		}
		return immutableHashMap;
	}

	private IEnumerable<ValueBucket> GetValueBuckets()
	{
		if (_root == null)
		{
			yield break;
		}
		Stack<IEnumerator<Bucket>> stack = new Stack<IEnumerator<Bucket>>();
		stack.Push(_root.GetAll().GetEnumerator());
		while (stack.Count > 0)
		{
			IEnumerator<Bucket> enumerator = stack.Peek();
			if (enumerator.MoveNext())
			{
				if (enumerator.Current is ValueBucket valueBucket)
				{
					yield return valueBucket;
				}
				else
				{
					stack.Push(enumerator.Current.GetAll().GetEnumerator());
				}
			}
			else
			{
				stack.Pop();
			}
		}
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Clear()
	{
		return Clear();
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Add(TKey key, TValue value)
	{
		return Add(key, value);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItem(TKey key, TValue value)
	{
		return SetItem(key, value);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.SetItems(IEnumerable<KeyValuePair<TKey, TValue>> items)
	{
		return SetItems(items);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.AddRange(IEnumerable<KeyValuePair<TKey, TValue>> pairs)
	{
		return AddRange(pairs);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.RemoveRange(IEnumerable<TKey> keys)
	{
		return RemoveRange(keys);
	}

	IImmutableDictionary<TKey, TValue> IImmutableDictionary<TKey, TValue>.Remove(TKey key)
	{
		return Remove(key);
	}
}
