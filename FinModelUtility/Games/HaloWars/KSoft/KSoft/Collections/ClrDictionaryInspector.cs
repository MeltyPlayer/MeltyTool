using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using Reflect = System.Reflection;

namespace KSoft.Collections
{
	public sealed class ClrDictionaryInspector<TKey, TValue>
	{
		#region Dictionary field names
		const string kDicBucketsName = "buckets";
		const string kDicEntriesName = "entries";
		const string kDicCountName = "count";
		const string kDicVersionName = "version";
		const string kDicFreeListName = "freeList";
		const string kDicFreeCountName = "freeCount";
		#endregion
		#region Dictionary.Entry field names
		const string kEntryTypeName = "Entry";
		const string kEntryHashCodeName = "hashCode";
		const string kEntryNextEntryIndexName = "next";
		const string kEntryKeyName = "key";
		const string kEntryValueName = "value";
		#endregion

		[SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
		public struct DicEntry
		{
			public int HashCode; // only the lower 31 bits of the actual hash code
			public int NextEntryIndex;
			public TKey Key;
			public TValue Value;

			public bool IsFree { get => this.HashCode.IsNone(); }
			public bool IsLast { get => this.NextEntryIndex.IsNone(); }

			public DicEntry GetNext(ClrDictionaryInspector<TKey, TValue> inspector)
			{
				Contract.Requires<ArgumentNullException>(inspector != null);
				Contract.Requires<InvalidOperationException>(!this.IsLast);

				return inspector.Entries[this.NextEntryIndex];
			}
		};

		#region Dictionary getters
		static readonly Func<Dictionary<TKey, TValue>, int[]> kGetDicBuckets;
		static readonly Func<Dictionary<TKey, TValue>, Array> kGetDicEntries;
		static readonly Func<Dictionary<TKey, TValue>, int> kGetDicCount;
		static readonly Func<Dictionary<TKey, TValue>, int> kGetDicVersion;
		static readonly Func<Dictionary<TKey, TValue>, int> kGetDicFreeList;
		static readonly Func<Dictionary<TKey, TValue>, int> kGetDicFreeCount;
		#endregion
		#region Dictionary.Entry getters
		static readonly Func<object, int> kGetEntryHashCode;
		static readonly Func<object, int> kGetEntryNextEntryIndex;
		static readonly Func<object, TKey> kGetEntryKey;
		static readonly Func<object, TValue> kGetEntryValue;
		#endregion

		[SuppressMessage("Microsoft.Design", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		static ClrDictionaryInspector()
		{
			// implementations are totally different...
			if (Shell.Platform.IsMonoRuntime)
			{
				Debug.Trace.Collections.TraceDataSansId(System.Diagnostics.TraceEventType.Critical,
					nameof(ClrDictionaryInspector<TKey, TValue>) + " does not support Mono");
			}

			// "If a nested type is generic, this method returns its generic type definition. This is true even if the enclosing generic type is a closed constructed type."
			var dic_entry_type = typeof(Dictionary<TKey, TValue>)
				.GetNestedType(kEntryTypeName, Reflect.BindingFlags.NonPublic)
				.MakeGenericType(typeof(TKey), typeof(TValue));
			Contract.Assert(dic_entry_type != null);

			#region Dictionary getters
			kGetDicBuckets =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int[]>(kDicBucketsName);
			kGetDicEntries =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, Array>(kDicEntriesName);
			kGetDicCount =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int>(kDicCountName);
			kGetDicVersion =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int>(kDicVersionName);
			kGetDicFreeList =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int>(kDicFreeListName);
			kGetDicFreeCount =
				Reflection.Util.GenerateMemberGetter<Dictionary<TKey, TValue>, int>(kDicFreeCountName);
			#endregion
			#region Dictionary.Entry getters
			kGetEntryHashCode =
				Reflection.Util.GenerateMemberGetter<int>(dic_entry_type, kEntryHashCodeName);
			kGetEntryNextEntryIndex =
				Reflection.Util.GenerateMemberGetter<int>(dic_entry_type, kEntryNextEntryIndexName);
			kGetEntryKey =
				Reflection.Util.GenerateMemberGetter<TKey>(dic_entry_type, kEntryKeyName);
			kGetEntryValue =
				Reflection.Util.GenerateMemberGetter<TValue>(dic_entry_type, kEntryValueName);
			#endregion
		}

		readonly Dictionary<TKey, TValue> mDic;
		readonly int mExpectedVersion;
		DicEntry[] mEntries;

		public ClrDictionaryInspector(Dictionary<TKey, TValue> dic)
		{
			Contract.Requires<ArgumentNullException>(dic != null);

			this.mDic = dic;
			this.mExpectedVersion = this.Version;
		}

		[Contracts.ContractInvariantMethod]
		void ObjectInvariant()
		{
			Contract.Invariant(this.Version == this.mExpectedVersion,
				"Tried to inspect a dictionary that has been modified since the inspector was created");
		}

		void InitializeEntries()
		{
			this.mEntries = new DicEntry[this.Buckets.Count];
			var array = kGetDicEntries(this.mDic);

			for (int x = 0; x < array.Length; x++)
			{
				var entry = array.GetValue(x);

				this.mEntries[x] = new DicEntry()
				{
					HashCode = kGetEntryHashCode(entry),
					NextEntryIndex = kGetEntryNextEntryIndex(entry),
					Key = kGetEntryKey(entry),
					Value = kGetEntryValue(entry),
				};
			}
		}

		public IReadOnlyList<int> Buckets { get {
			var buckets = kGetDicBuckets(this.mDic);

			return buckets ?? Array.Empty<int>();
		} }
		public IReadOnlyList<DicEntry> Entries { get {
			if (this.mEntries == null)
				this.InitializeEntries();

			return this.mEntries;
		} }
		public int Count { get => kGetDicCount(this.mDic); }
		private int Version { get => kGetDicVersion(this.mDic); }
		public int FreeList { get => kGetDicFreeList(this.mDic); }
		public int FreeCount { get => kGetDicFreeCount(this.mDic); }

		public IEnumerable<int> BucketsInUse { get => this.Buckets.Where(b => b >= 0); }

		public IEnumerable<DicEntry> GetEntriesInBucket(int bucketIndex)
		{
			Contract.Requires<ArgumentOutOfRangeException>(bucketIndex >= 0 && bucketIndex < this.Buckets.Count);

			for (int x = this.Buckets[bucketIndex]; x >= 0; x = this.Entries[x].NextEntryIndex)
				yield return this.Entries[x];
		}

		public IEnumerable<DicEntry> EntryCollisions(TKey key)
		{
			int hash_code = this.mDic.Comparer.GetHashCode(key) & 0x7FFFFFFF;
			int target_bucket = hash_code % this.Buckets.Count;

#if false // result as entry indices
			for (int x = Buckets[target_bucket]; x >= 0; x = Entries[x].NextEntryIndex)
			{
				if (Entries[x].HashCode == hash_code && mDic.Comparer.Equals(Entries[x].Key, key))
					yield break;

				yield return x;
			}
#endif

			return
				from e in this.GetEntriesInBucket(target_bucket)
				where e.HashCode == hash_code && !this.mDic.Comparer.Equals(e.Key, key)
				select e;
		}
	};
}
