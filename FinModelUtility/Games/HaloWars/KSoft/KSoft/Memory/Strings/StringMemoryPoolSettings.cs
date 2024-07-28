
namespace KSoft.Memory.Strings
{
	/// <summary>
	/// Configuration properties for defining how a <see cref="StringMemoryPool"/> serializes strings and generates string reference data
	/// </summary>
	public class StringMemoryPoolSettings //: IO.IEndianStreamable
	{
		/// <summary>String serializing definition for the <see cref="StringMemoryPool"/></summary>
		public StringStorage Storage { get; private set; }

		/// <summary>Start address for all string address references</summary>
		public Values.PtrHandle BaseAddress { get; private set; }

		#region AddressSize
		readonly Shell.ProcessorSize mAddressSize;
		/// <summary>Size of addresses used for referencing serialized strings</summary>
		public Shell.ProcessorSize AddressSize { get { return this.mAddressSize; } }
		#endregion

		/// <summary>Is an empty string entry automatically add to the pool</summary>
		public bool ImplicitNull { get; private set; }

		/// <summary>Are duplicate strings allowed in the pool?</summary>
		public bool AllowDuplicates { get; set; }


		#region Ctor
		/// <summary>Define a new <see cref="StringMemoryPool"/> configuration</summary>
		/// <param name="method">Text storage definition</param>
		/// <param name="implicitNull">Is a null string entry atomically added?</param>
		/// <param name="addressSize">Size of string address references</param>
		/// <remarks>Base address defaults to the null equivlent on <paramref name="addressSize"/> platforms</remarks>
		public StringMemoryPoolSettings(StringStorage method, bool implicitNull, Shell.ProcessorSize addressSize)
		{
			{
				this.Storage = method;
				this.AllowDuplicates = false; }

			this.mAddressSize = addressSize;
			this.ImplicitNull = implicitNull;

			this.BaseAddress = this.mAddressSize == Shell.ProcessorSize.x64 ?
				Values.PtrHandle.Null64 : Values.PtrHandle.Null32;
		}
		/// <summary>Define a new <see cref="StringMemoryPool"/> configuration</summary>
		/// <param name="method">Text storage definition</param>
		/// <param name="implicitNull">Is a null string entry atomically added?</param>
		/// <param name="baseAddress">Base address for string references</param>
		/// <remarks><see cref="AddressSize"/> is determined from <see cref="baseAddress"/></remarks>
		public StringMemoryPoolSettings(StringStorage method, bool implicitNull, Values.PtrHandle baseAddress)
		{
			{
				this.Storage = method;
				this.AllowDuplicates = false; }

			this.mAddressSize = baseAddress.Is64bit ? Shell.ProcessorSize.x64 : Shell.ProcessorSize.x32;
			this.ImplicitNull = implicitNull;
			this.BaseAddress = baseAddress;
		}

		/// <summary>Define a new <see cref="StringMemoryPool"/> configuration</summary>
		/// <param name="method">Text storage definition</param>
		/// <param name="implicitNull">Is a null string entry atomically added?</param>
		/// <remarks><see cref="AddressSize"/> is determined from <see cref="Shell.Platform.Environment"/></remarks>
		public StringMemoryPoolSettings(StringStorage method, bool implicitNull)
			: this(method, implicitNull,
				Shell.Platform.Environment.ProcessorType.ProcessorSize)
		{
		}
		/// <summary>Define a new <see cref="StringMemoryPool"/> configuration</summary>
		/// <param name="method">Text storage definition</param>
		/// <param name="baseAddress">Base address for string references</param>
		/// <remarks>A null string entry <b>is</b> added by default</remarks>
		public StringMemoryPoolSettings(StringStorage method, Values.PtrHandle baseAddress)
			: this(method, true, baseAddress)
		{
		}
		/// <summary>Define a new <see cref="StringMemoryPool"/> configuration</summary>
		/// <param name="method">Text storage definition</param>
		/// <remarks>
		/// A null string entry <b>is</b> added by default.
		///
		/// <see cref="AddressSize"/> is determined from <see cref="Shell.Platform.Environment"/>
		/// </remarks>
		public StringMemoryPoolSettings(StringStorage method)
			: this(method, true,
				Shell.Platform.Environment.ProcessorType.ProcessorSize)
		{
		}
		#endregion

		#region IEndianStreamable Members
#if false // #TODO
		public void Read(KSoft.IO.EndianReader s)
		{
			var storage = new StringStorage(); storage.Read(s);
			Storage = storage;

			mAddressSize = (Shell.ProcessorSize)s.ReadByte();
			ImplicitNull = s.ReadBoolean();
			AllowDuplicates = s.ReadBoolean();
			s.Seek(sizeof(byte));

			var base_addr = new Values.PtrHandle(mAddressSize); base_addr.Read(s);
			BaseAddress.Read(s);
		}

		public void Write(KSoft.IO.EndianWriter s)
		{
			Storage.Write(s);

			s.Write((byte)mAddressSize);
			s.Write(ImplicitNull);
			s.Write(AllowDuplicates);
			s.Write(byte.MinValue);

			BaseAddress.Write(s);
		}
#endif
		#endregion
	};
}
