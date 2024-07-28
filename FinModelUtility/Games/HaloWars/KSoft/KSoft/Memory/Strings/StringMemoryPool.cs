using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Contracts = System.Diagnostics.Contracts;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Memory.Strings
{
	/// <summary>Builds representations of unmanaged string pools</summary>
	/// <remarks>
	/// Equal (case-sensitive) strings will only ever appear once.
	///
	/// While this builds a representation of an unmanaged string pool,
	/// the implementation is entirely "safe" and managed in .NET.
	///
	/// Call the explicit Read\Write methods to fragment where the respected
	/// information is streamed. Otherwise use the default
	/// <see cref="IO.IEndianStreamable"/> implementation to stream this class
	/// </remarks>
	[SuppressMessage("Microsoft.Design", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
	public partial class StringMemoryPool
		: IO.IEndianStreamable
		, IO.IEndianStreamSerializable
		, ICollection<string>
		, IEnumerable<string>
	{
		/// <summary>Default amount of entry memory allocated for use</summary>
		const int kEntryStartCount = 64;
		/// <summary>Sentinel value of an invalid string address reference</summary>
		public static readonly Values.PtrHandle kInvalidReference = new Values.PtrHandle(ulong.MaxValue);


		/// <summary>Configuration instance data for this pool</summary>
		public StringMemoryPoolSettings Settings { get; private set; }

		List<string> mPool;
		List<Values.PtrHandle> mReferences;
		/// <remarks>Only created when <see cref="UseStringToIndex"/> is true</remarks>
		Dictionary<string, int> mStringToIndex;
		// null string offset starts off null in case the user doesn't want an implicit null
		Values.PtrHandle mNullReference = kInvalidReference;
		Text.StringStorageEncoding mEncoding;

		#region Count
		/// <summary>Get the number of strings in the pool</summary>
		public int Count { get { return this.mPool.Count; } }
		#endregion

		#region Size
		/// <summary>Total size in bytes of the pool</summary>
		public uint Size { get; private set; }

		/// <summary>Calculate how many bytes of storage <paramref name="value"/> will consume in the pool</summary>
		/// <param name="value"></param>
		/// <returns>Number of bytes <paramref name="value"/> will consume</returns>
		int CalculateStringByteLength(string value)
		{
			return this.mEncoding.GetByteCount(value);
		}
		#endregion

		bool UseStringToIndex => !this.Settings.AllowDuplicates;

		void InitializeCollections(int capacity)
		{
			this.mPool = new List<string>(capacity);
			this.mReferences = new List<Values.PtrHandle>(capacity);
			if (this.UseStringToIndex)
				this.mStringToIndex = new Dictionary<string, int>(capacity, StringComparer.Ordinal);
		}
		/// <summary>Create a <see cref="StringMemoryPool"/> from a <see cref="StringMemoryPoolSettings"/> definition</summary>
		/// <param name="definition"></param>
		public StringMemoryPool(StringMemoryPoolSettings definition)
		{
			this.Settings = definition;
			this.InitializeCollections(kEntryStartCount);
			this.mEncoding = new Text.StringStorageEncoding(definition.Storage);
		}

		#region Add
		/// <summary>Takes a string and adds it to the pool</summary>
		/// <param name="str">value to add</param>
		/// <returns>address reference of the string</returns>
		/// <remarks>
		/// If <see cref="Configuration.AllowDuplicates"/> is NOT true, this will return an address
		/// of a string which is equal to <paramref name="str"/>
		/// </remarks>
		public Values.PtrHandle Add(string str)
		{
			int index;

			if (string.IsNullOrEmpty(str))
			{
				if (this.Settings.ImplicitNull) // if we're setup to use a implicit null string, its always the first string in the pool
					return this.Settings.BaseAddress;

				if (this.mNullReference == kInvalidReference) // if not, check to see if a null string has been added yet
				{
					index = this.Count;
					this.AddInternal("");
					this.mNullReference = this.Settings.BaseAddress + this.mReferences[index];
				}
				return this.mNullReference;
			}

			// If we allow dups, we won't try to find a matching entry, we'll immediately add it.
			if (this.Settings.AllowDuplicates || !this.mStringToIndex.TryGetValue(str, out index))
			{
				index = this.Count;
				this.AddInternal(str);
			}

			return this.mReferences[index];
		}

		void AddInternal(string value)
		{
			if (this.UseStringToIndex)
				this.mStringToIndex.Add(value, this.mPool.Count);
			// the PtrHandle created will implicitly take after [BaseAddress]'s address size
			this.mReferences.Add(this.Settings.BaseAddress + this.Size);
			this.mPool.Add(value);

			this.Size += (uint) this.CalculateStringByteLength(value);
		}

		void AddFromRead(int index, string value)
		{
			if (this.UseStringToIndex)
				this.mStringToIndex.Add(value, index);
			this.mPool[index] = value;
		}
		#endregion

		#region Get
		/// <summary>
		/// Takes a string and gets the address it would have if the pool was located at
		/// <see cref="Configuration.BaseAddress"/> in memory
		/// </summary>
		/// <param name="value"></param>
		/// <returns>
		/// Address of <paramref name="value"/> in the pool or <see cref="kInvalidReference"/>
		/// if there is no matching string
		/// </returns>
		/// <remarks>
		/// This method is not written to support configurations whose <see cref="Config.AllowDuplicates"/>
		/// is set to true. The first instance will ALWAYS be returned.
		/// </remarks>
		[Contracts.Pure]
		public Values.PtrHandle GetAddress(string value)
		{
			int index;
			if (this.UseStringToIndex)
				this.mStringToIndex.TryGetValue(value, out index);
			else
				index = this.mPool.IndexOf(value);

			if (index.IsNone())
				return this.Settings.BaseAddress + this.mReferences[index];

			return kInvalidReference;
		}

		/// <summary>Get the reference index of the string at <paramref name="address"/></summary>
		/// <param name="address"></param>
		/// <returns>Index of <paramref name="address"/> or -1 if not found</returns>
		/// <remarks>Actually returns the index used internally tracking strings\offsets</remarks>
		[Contracts.Pure]
		/*public*/ int GetIndex(Values.PtrHandle address)
		{
			return this.mReferences.FindIndex(x => x == address);
		}

		/// <summary>Get the address of the 'null' string</summary>
		/// <returns></returns>
		public Values.PtrHandle GetNull() { return this.mNullReference; }

		/// <summary>Get the string thats located at <paramref name="address"/></summary>
		/// <param name="address"></param>
		/// <returns></returns>
		/// <remarks>
		/// Code contracts will cause an assert if the address doesn't start a new string
		/// </remarks>
		[Contracts.Pure]
		public string Get(Values.PtrHandle address)	{ return this.mPool[this.GetIndex(address)]; }
		/// <summary>Get the string thats located at <paramref name="address"/></summary>
		/// <param name="address"></param>
		/// <returns></returns>
		/// <remarks>
		/// Code contracts will cause an assert if the address doesn't start a new string
		/// </remarks>
		[SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
		public string this[Values.PtrHandle address]	{ get { return this.Get(address); } }
		#endregion

		#region IEnumerable Members
		/// <summary>Get an enumerator that iterates through this pool's stored string values</summary>
		/// <returns></returns>
		public IEnumerator<string> GetEnumerator()										{ return this.mPool.GetEnumerator(); }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()	{ return this.mPool.GetEnumerator(); }
		/// <summary>Get an enumerator that iterates through this pool using address/string pairs</summary>
		/// <returns></returns>
		public IEnumerator<KeyValuePair<Values.PtrHandle, string>> GetKeyValueEnumerator()		{ return new KeyValueEnumerator(this); }
		#endregion

		#region IEndianStreamable/IEndianStreamSerializable Members
		/// <summary>Read the header for the pool from a stream to properly initialize this pool's configuration, counts, etc</summary>
		/// <param name="s"></param>
		public void ReadHeader(IO.EndianReader s)
		{
			Contract.Requires(s != null);
			// #TODO: test to see if the config is a built-in, else we could overwrite an existing config
//			Configuration.Read(s);
			int count = s.ReadInt32();
			this.Size = s.ReadUInt32();

			this.InitializeCollections(count);
			for (int x = 0; x < this.mReferences.Count; x++)
				this.mReferences[x] = new Values.PtrHandle(this.Settings.AddressSize);
		}
		/// <summary>
		/// Write the header for this pool to a stream for future re-initializing
		/// and usage, storing things such as the configuration, count, etc
		/// </summary>
		/// <param name="s"></param>
		public void WriteHeader(IO.EndianWriter s)
		{
			Contract.Requires(s != null);

//			Configuration.Write(s);
			s.Write(this.Count);
			s.Write(this.Size);
		}

		int[] ioStringLengths;
		/// <summary>Read the character count for the string values from a stream</summary>
		/// <param name="s"></param>
		/// <remarks>
		/// Obviously, this needs to be called before <see cref="ReadStrings(IO.EndianReader s)"/>
		/// if you're going to even use this (due to performance reasons or due to the storage definition)
		/// </remarks>
		public void ReadStringCharacterLengths(IO.EndianReader s)
		{
			Contract.Requires(s != null);

			this.ioStringLengths = new int[this.Count];
			for (int x = 0; x < this.ioStringLengths.Length; x++)
				this.ioStringLengths[x] = s.ReadInt32();
		}
		/// <summary>Write the character count for the string values to a stream</summary>
		/// <param name="s"></param>
		public void WriteStringCharacterLengths(IO.EndianWriter s)
		{
			Contract.Requires(s != null);

			foreach (string str in this.mPool)
				s.Write(str.Length);
		}
		/// <summary>Only used for Interop situations where explicit lengths are needed for cases (like enumeration) in unmanaged code</summary>
		/// <param name="s"></param>
		public void WriteStringByteLengths(IO.EndianWriter s)
		{
			Contract.Requires(s != null);

			foreach (string str in this.mPool)
				s.Write(this.CalculateStringByteLength(str));
		}

		/// <summary>Read the string addresses from a stream</summary>
		/// <param name="s"></param>
		public void ReadReferences(IO.EndianReader s)
		{
			Contract.Requires(s != null);

			for (int x = 0; x < this.mReferences.Count; x++)
				this.mReferences[x].Read(s);
		}
		/// <summary>Write the string addresses to a stream</summary>
		/// <param name="s"></param>
		public void WriteReferences(IO.EndianWriter s)
		{
			Contract.Requires(s != null);

			foreach (Values.PtrHandle r in this.mReferences)
				r.Write(s);
		}

		/// <summary>Read the string values from a stream</summary>
		/// <param name="s"></param>
		public void ReadStrings(IO.EndianReader s)
		{
			Contract.Requires(s != null);

			if (this.ioStringLengths == null)
				for (int x = 0; x < this.mPool.Count; x++)
					this.AddFromRead(x, s.ReadString(this.mEncoding));
			else
			{
				for (int x = 0; x < this.Count; x++)
					this.AddFromRead(x, s.ReadString(this.mEncoding, this.ioStringLengths[x]));
				this.ioStringLengths = null;
			}
		}
		/// <summary>Write the string values to a stream</summary>
		/// <param name="s"></param>
		public void WriteStrings(IO.EndianWriter s)
		{
			Contract.Requires(s != null);

			foreach (string str in this.mPool)
				s.Write(str, this.mEncoding);
		}

		/// <summary>Read a <see cref="StringMemoryPool"/> from a stream</summary>
		/// <param name="s"></param>
		/// <remarks>
		/// Stream order:
		/// <see cref="ReadHeader(IO.EndianReader)"/>
		/// <see cref="ReadReferences(IO.EndianReader)"/>
		/// <see cref="ReadStrings(IO.EndianReader)"/>
		/// </remarks>
		public void Read(IO.EndianReader s)
		{
			this.ReadHeader(s);
			this.ReadReferences(s);
			this.ReadStrings(s);
		}
		/// <summary>Write this <see cref="StringMemoryPool"/> to a stream</summary>
		/// <param name="s"></param>
		/// <remarks>
		/// Stream order:
		/// <see cref="WriteHeader(IO.EndianWriter)"/>
		/// <see cref="WriteReferences(IO.EndianWriter)"/>
		/// <see cref="WriteStrings(IO.EndianWriter)"/>
		/// </remarks>
		public void Write(IO.EndianWriter s)
		{
			this.WriteHeader(s);
			this.WriteReferences(s);
			this.WriteStrings(s);
		}

		public void SerializeHeader(IO.EndianStream s)
		{
				 if (s.IsReading)
					 this.ReadHeader(s.Reader);
			else if (s.IsWriting)
				this.WriteHeader(s.Writer);
		}
		public void SerializeStringCharacterLengths(IO.EndianStream s)
		{
				 if (s.IsReading)
					 this.ReadStringCharacterLengths(s.Reader);
			else if (s.IsWriting)
				this.WriteStringCharacterLengths(s.Writer);
		}
		public void SerializeReferences(IO.EndianStream s)
		{
				 if (s.IsReading)
					 this.ReadReferences(s.Reader);
			else if (s.IsWriting)
				this.WriteReferences(s.Writer);
		}
		public void SerializeStrings(IO.EndianStream s)
		{
				 if (s.IsReading)
					 this.ReadStrings(s.Reader);
			else if (s.IsWriting)
				this.WriteStrings(s.Writer);
		}
		public void Serialize(IO.EndianStream s)
		{
				 if (s.IsReading)
					 this.Read(s.Reader);
			else if (s.IsWriting)
				this.Write(s.Writer);
		}
		#endregion

		#region ICollection<string> Members
		void ICollection<string>.Add(string item)						{ var handle = this.Add(item); }
		void ICollection<string>.Clear()								{ throw new NotSupportedException("Can't clear items from a StringMemoryPool"); }
		public bool Contains(string item)								{ return this.UseStringToIndex ? this.mStringToIndex.ContainsKey(item) : this.mPool.Contains(item); }
		void ICollection<string>.CopyTo(string[] array, int arrayIndex)	{
			this.mPool.CopyTo(array, arrayIndex); }
		bool ICollection<string>.IsReadOnly								{ get { return false; } }

		bool ICollection<string>.Remove(string item)					{ throw new NotSupportedException("Can't remove items from a StringMemoryPool"); }
		#endregion
	};
}
