using System;
using System.Runtime.InteropServices;

namespace KSoft.DDS
{
	[StructLayout(LayoutKind.Sequential)]
	public struct DirectXTexScratchImage
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
				DirectXTexDLL.DirectXTex_ScratchImageFree(this);
				this.Pointer = IntPtr.Zero;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}
		}

		public static DirectXTexScratchImage New()
		{
			try
			{
				DirectXTexScratchImage image;
				var hresult = DirectXTexDLL.DirectXTex_ScratchImageNew(out image);
				DirectXTexDLL.ThrowIfFailed(hresult);
				return image;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}

			return new DirectXTexScratchImage();
		}

		public TexMetadata Metadata { get {
			if (this.IsNull || DirectXTexDLL.EntryPointsNotFound)
				return TexMetadata.Empty;

			try
			{
				TexMetadata metadata;
				var hresult = DirectXTexDLL.DirectXTex_ScratchImageGetMetadata(this, out metadata);
				DirectXTexDLL.ThrowIfFailed(hresult);
				return metadata;
			}
			catch (EntryPointNotFoundException ex)
			{
				DirectXTexDLL.HandleEntryPointNotFound(ex);
			}

			return TexMetadata.Empty;
		} }
	};
}