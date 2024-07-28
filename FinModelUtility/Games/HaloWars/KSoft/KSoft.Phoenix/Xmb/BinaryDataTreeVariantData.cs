using System;
using Interop = System.Runtime.InteropServices;

namespace KSoft.Phoenix.Xmb
{
	using BDTypeDesc = BinaryDataTreeVariantTypeDesc;

	[Interop.StructLayout(Interop.LayoutKind.Explicit)]
	public struct BinaryDataTreeVariantData
	{
		#region Direct Data
		[Interop.FieldOffset(0)]
		public bool Bool;
		[Interop.FieldOffset(0)]
		public uint Offset;
		[Interop.FieldOffset(0)]
		public uint Int;
		[Interop.FieldOffset(0)]
		public ulong Int64;
		[Interop.FieldOffset(0)]
		public float Single;
		[Interop.FieldOffset(0)]
		public double Double;
		#endregion

		[Interop.FieldOffset(8)]
		public BDTypeDesc TypeDesc;
		[Interop.FieldOffset(12)]
		public int ArrayLength;

		// String must come last, because we don't know how big a .NET reference really is (we could be compiling for x64!)
		[Interop.FieldOffset(16)]
		public string String;
		[Interop.FieldOffset(16)]
		public Array OpaqueArrayRef;

		public BinaryDataTreeVariantType Type { get { return this.TypeDesc.Type; } }
		public bool IsUnicode { get { return this.TypeDesc.IsUnicode; } }

		public bool UseDirectEncoding { get { return this.TypeDesc.SizeOf <= sizeof(uint) && this.ArrayLength <= 1; } }

		public int StringOrArrayLength { get {
			if (this.Type == BinaryDataTreeVariantType.String)
				return this.String != null ? this.String.Length : 0;

			return this.OpaqueArrayRef != null ? this.OpaqueArrayRef.Length : 0;
		} }

		internal void Read(BinaryDataTreeMemoryPool pool, BinaryDataTreeNameValue nameValue)
		{
			this.TypeDesc = nameValue.GuessTypeDesc();

			bool direct_encoding = nameValue.DirectEncoding;

			uint total_data_size = nameValue.Size;
			if (nameValue.SizeIsIndirect)
			{
				if (direct_encoding)
					throw new InvalidOperationException();

				total_data_size = pool.GetSizeValue(nameValue.Offset);

				if (total_data_size < BinaryDataTreeNameValue.kIndirectSizeThreshold)
					throw new InvalidOperationException();
			}

			if (this.TypeDesc.SizeOf > 0)
			{
				if ((total_data_size % this.TypeDesc.SizeOf) != 0)
					throw new InvalidOperationException(nameValue.ToString());

				this.ArrayLength = (int)(total_data_size / this.TypeDesc.SizeOf);
			}

			if (this.ArrayLength > 1)
			{
				if (this.Type != BinaryDataTreeVariantType.String)
					this.OpaqueArrayRef = this.TypeDesc.MakeArray(this.ArrayLength);

				pool.InternalBuffer.Seek(nameValue.Offset);
			}

			switch (this.Type)
			{
				case BinaryDataTreeVariantType.Null:
					break;

				case BinaryDataTreeVariantType.Bool:
					if (this.ArrayLength > 1)
						this.TypeDesc.ReadArray(pool.InternalBuffer, this.OpaqueArrayRef);
					else
						this.Bool = nameValue.Bool;
					break;

				case BinaryDataTreeVariantType.Int:
					if (this.ArrayLength > 1)
						this.TypeDesc.ReadArray(pool.InternalBuffer, this.OpaqueArrayRef);
					else
					{
						if (this.TypeDesc.SizeOf < sizeof(long))
							this.Int = nameValue.Int;
						else
							this.Int64 = pool.InternalBuffer.ReadUInt64();
					}
					break;

				case BinaryDataTreeVariantType.Float:
					if (this.ArrayLength > 1)
						this.TypeDesc.ReadArray(pool.InternalBuffer, this.OpaqueArrayRef);
					else
					{
						if (this.TypeDesc.SizeOf < sizeof(double))
							this.Single = nameValue.Single;
						else
							this.Double = pool.InternalBuffer.ReadDouble();
					}
					break;

				case BinaryDataTreeVariantType.String:
					this.ArrayLength = 1;
					if (!this.IsUnicode && total_data_size <= sizeof(uint))
					{
						var sb = new System.Text.StringBuilder();
						for (uint x = 0, v = nameValue.Int; x < sizeof(uint); x++, v >>= Bits.kByteBitCount)
						{
							sb.Append((char)(v & 0xFF));
						}

						this.String = sb.ToString();
					}
					else
					{
						this.String = pool.InternalBuffer.ReadString(this.IsUnicode
							? Memory.Strings.StringStorage.CStringUnicode
							: Memory.Strings.StringStorage.CStringAscii);
					}
					break;

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
			}
		}

		public void ToStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (this.Type == BinaryDataTreeVariantType.Null)
				return;

			this.TypeDesc.ToStream(s, this.ArrayLength);

			if (this.ArrayLength > 1 && this.Type != BinaryDataTreeVariantType.String)
			{
				var array_str = this.TypeDesc.ArrayToString(this.OpaqueArrayRef);
				s.WriteCursor(array_str);
				return;
			}

			switch (this.Type)
			{
				case BinaryDataTreeVariantType.Bool:
					s.WriteCursor(this.Bool);
					break;

				case BinaryDataTreeVariantType.Int:
					s.WriteCursor(this.Int64);
					break;

				case BinaryDataTreeVariantType.Float:
					if (this.TypeDesc.SizeOf < sizeof(double))
						s.WriteCursor(this.Single);
					else
						s.WriteCursor(this.Double);
					break;

				case BinaryDataTreeVariantType.String:
					if (this.String.IsNotNullOrEmpty())
						s.WriteCursor(this.String);
					break;

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
			}
		}

		public void ToStreamAsAttribute<TDoc, TCursor>(string attributeName, IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			if (this.Type == BinaryDataTreeVariantType.Null)
				return;

			if (this.ArrayLength > 1 && this.Type != BinaryDataTreeVariantType.String)
			{
				var array_str = this.TypeDesc.ArrayToString(this.OpaqueArrayRef);
				s.WriteAttribute(attributeName, array_str);
				return;
			}

			switch (this.Type)
			{
				case BinaryDataTreeVariantType.Bool:
					s.WriteAttribute(attributeName, this.Bool);
					break;

				case BinaryDataTreeVariantType.Int:
					s.WriteAttribute(attributeName, this.Int64);
					break;

				case BinaryDataTreeVariantType.Float:
					if (this.TypeDesc.SizeOf < sizeof(double))
						s.WriteAttribute(attributeName, this.Single);
					else
						s.WriteAttribute(attributeName, this.Double);
					break;

				case BinaryDataTreeVariantType.String:
					if (this.String.IsNotNullOrEmpty())
						s.WriteAttribute(attributeName, this.String);
					break;

				default: throw new KSoft.Debug.UnreachableException(this.Type.ToString());
			}
		}

		public void FromStream<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			this.TypeDesc = BDTypeDesc.GuessFromStream(s, out this.ArrayLength);

			// #TODO
			throw new NotImplementedException();
		}
	};
}