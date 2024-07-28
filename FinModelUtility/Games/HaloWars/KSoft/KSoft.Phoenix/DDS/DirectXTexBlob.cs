using System;
using System.Runtime.InteropServices;

namespace KSoft.DDS
{
	[StructLayout(LayoutKind.Sequential)]
	public struct DirectXTexBlob
		: IDisposable
	{
		IntPtr Pointer;

		public bool IsNull { get { return this.Pointer == IntPtr.Zero; } }
		public bool IsNotNull { get { return this.Pointer != IntPtr.Zero; } }

		public void Dispose()
		{
			if (this.IsNull || DirectXTexDLL.EntryPointsNotFound)
				return;

			try
			{
				DirectXTexDLL.DirectXTex_BlobFree(this);
				this.Pointer = IntPtr.Zero;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}
		}

		public static DirectXTexBlob New()
		{
			try
			{
				DirectXTexBlob blob;
				var hresult = DirectXTexDLL.DirectXTex_BlobNew(out blob);
				DirectXTexDLL.ThrowIfFailed(hresult);
				return blob;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}

			return new DirectXTexBlob();
		}

		public IntPtr Buffer { get {
			if (this.IsNull || DirectXTexDLL.EntryPointsNotFound)
				return IntPtr.Zero;

			try
			{
				IntPtr bufferPointer;
				uint bufferSize;
				var hresult = DirectXTexDLL.DirectXTex_BlobGetBuffer(this, out bufferPointer, out bufferSize);
				DirectXTexDLL.ThrowIfFailed(hresult);
				return bufferPointer;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}

			return IntPtr.Zero;
		} }

		public uint BufferSize { get {
			if (this.IsNull || DirectXTexDLL.EntryPointsNotFound)
				return 0;

			try
			{
				IntPtr bufferPointer;
				uint bufferSize;
				var hresult = DirectXTexDLL.DirectXTex_BlobGetBuffer(this, out bufferPointer, out bufferSize);
				DirectXTexDLL.ThrowIfFailed(hresult);
				return bufferSize;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}

			return 0;
		} }
	};
}
