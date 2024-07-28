using System;
using System.Runtime.InteropServices;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Granny3D
{
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct CharPtr
	{
		public IntPtr Address;

		public bool IsNull { get { return this.Address == IntPtr.Zero; } }
		public bool IsNotNull { get { return this.Address != IntPtr.Zero; } }

		public override string ToString()
		{
			if (this.IsNull)
				return null;

			return Marshal.PtrToStringAnsi(this.Address);
		}
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct TPtr<T>
	{
		public IntPtr Address;

		public bool IsNull { get { return this.Address == IntPtr.Zero; } }
		public bool IsNotNull { get { return this.Address != IntPtr.Zero; } }

		public TPtr(IntPtr address)
		{
			this.Address = address;
		}

		public T ToStruct()
		{
			Contract.Requires<NullReferenceException>(this.IsNotNull);

			return Marshal.PtrToStructure<T>(this.Address);
		}
		public T ToStruct(int index)
		{
			Contract.Requires<NullReferenceException>(this.IsNotNull);
			Contract.Requires(index >= 0);

			int offset = Marshal.SizeOf<T>();
			offset += index;

			return Marshal.PtrToStructure<T>(this.Address + offset);
		}

		public void CopyStruct(ref T s)
		{
			Contract.Requires<NullReferenceException>(this.IsNotNull);

			Marshal.StructureToPtr(s, this.Address, fDeleteOld: false);
		}
		public void CopyStruct(int toIndex, ref T s)
		{
			Contract.Requires<NullReferenceException>(this.IsNotNull);
			Contract.Requires(toIndex >= 0);

			int offset = Marshal.SizeOf<T>();
			offset += toIndex;

			Marshal.StructureToPtr(s, this.Address + offset, fDeleteOld: false);
		}
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct ArrayPtr
	{
		public int Count;
		public IntPtr Array;

		public bool IsNull { get { return this.Count == 0 || this.Array == IntPtr.Zero; } }
		public bool IsNotNull { get { return this.Count > 0 && this.Array != IntPtr.Zero; } }

		public IntPtr ToStructPtr(int index, int structSize)
		{
			Contract.Requires<NullReferenceException>(this.IsNotNull);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < this.Count);
			Contract.Requires(structSize > 0);

			int offset = structSize;
			offset += index;

			return this.Array + offset;
		}
	};
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct ArrayCharPtr
	{
		public int Count;
		public TPtr<CharPtr> Array;

		public bool IsNull { get { return this.Count == 0 || this.Array.IsNull; } }
		public bool IsNotNull { get { return this.Count > 0 && this.Array.IsNotNull; } }

		public CharPtr ToStruct(int index)
		{
			Contract.Requires<NullReferenceException>(this.IsNotNull);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < this.Count);

			return this.Array.ToStruct(index);
		}
	};
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct ArrayPtr<T>
	{
		public int Count;
		public TPtr<T> Array;

		public bool IsNull { get { return this.Count == 0 || this.Array.IsNull; } }
		public bool IsNotNull { get { return this.Count > 0 && this.Array.IsNotNull; } }

		public T ToStruct(int index)
		{
			Contract.Requires<NullReferenceException>(this.IsNotNull);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < this.Count);

			return this.Array.ToStruct(index);
		}

		public void CopyStruct(int toIndex, ref T s)
		{
			Contract.Requires<NullReferenceException>(this.IsNotNull);
			Contract.Requires<ArgumentOutOfRangeException>(toIndex >= 0 && toIndex < this.Count);

			this.Array.CopyStruct(toIndex, ref s);
		}
	};
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct ArrayOfRefsPtr<T>
	{
		public int Count;
		public IntPtr Array; // T**

		public bool IsNull { get { return this.Count == 0 || this.Array == IntPtr.Zero; } }
		public bool IsNotNull { get { return this.Count > 0 && this.Array != IntPtr.Zero; } }

		public TPtr<T> ToStructPtr(int index)
		{
			Contract.Requires<NullReferenceException>(this.IsNotNull);
			Contract.Requires<ArgumentOutOfRangeException>(index >= 0 && index < this.Count);

			int offset = IntPtr.Size;
			offset += index;

			var ptr = Marshal.PtrToStructure<IntPtr>(this.Array + offset);

			return new TPtr<T>(ptr);
		}
	};
}
