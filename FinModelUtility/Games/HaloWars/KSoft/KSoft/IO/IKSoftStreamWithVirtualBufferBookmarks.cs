using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.IO
{
	/// <summary>Forces the stream to seek to the end of the virtual buffer when disposed</summary>
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct IKSoftStreamWithVirtualBufferCleanup : IDisposable
	{
		IKSoftStreamWithVirtualBuffer mStream;
		readonly long mBufferEnd;

		public IKSoftStreamWithVirtualBufferCleanup(IKSoftStreamWithVirtualBuffer stream)
		{
			Contract.Requires(stream != null);
			Contract.Requires(stream.VirtualBufferStart > 0 && stream.VirtualBufferLength > 0);
			this.mStream = stream;
			this.mBufferEnd = stream.VirtualBufferStart + stream.VirtualBufferLength;
		}

		/// <summary>
		/// If the stream position is still inside the virtual buffer, seeks to the VirtualBuffer 'end'.
		/// Sets the VirtualBuffer properties of the underlying stream to 0.
		/// </summary>
		public void Dispose()
		{
			if (this.mStream != null)
			{
				long leftovers = this.mBufferEnd - this.mStream.BaseStream.Position;
				if (leftovers > 0)
					this.mStream.BaseStream.Seek(leftovers, SeekOrigin.Current);

				this.mStream.VirtualBufferStart = this.mStream.VirtualBufferLength = 0;
				this.mStream = null;
			}
		}
	};
	/// <summary>Temporarily bookmarks a stream's VirtualBuffer properties</summary>
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct IKSoftStreamWithVirtualBufferBookmark : IDisposable
	{
		IKSoftStreamWithVirtualBuffer mStream;
		readonly long mOldStart, mOldLength;

		public IKSoftStreamWithVirtualBufferBookmark(IKSoftStreamWithVirtualBuffer stream)
		{
			Contract.Requires(stream != null);
			this.mStream = stream;
			this.mOldStart = stream.VirtualBufferStart;
			this.mOldLength = stream.VirtualBufferLength;
		}

		/// <summary>Restores the VirtualBuffer properties of the underlying stream to their previous values</summary>
		public void Dispose()
		{
			if (this.mStream != null)
			{
				this.mStream.VirtualBufferStart = this.mOldStart;
				this.mStream.VirtualBufferLength = this.mOldLength;
				this.mStream = null;
			}
		}
	};
	/// <summary>
	/// Temporarily bookmarks a stream's VirtualBuffer properties and sets up a new virtual buffer concept
	/// </summary>
	/// <see cref="IKSoftStreamWithVirtualBufferBookmark"/>
	/// <see cref="IKSoftStreamWithVirtualBufferCleanup"/>
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct IKSoftStreamWithVirtualBufferAndBookmark : IDisposable
	{
		IKSoftStreamWithVirtualBufferBookmark mBookmark;
		IKSoftStreamWithVirtualBufferCleanup mCleanup;
		bool mDisposed;

		public IKSoftStreamWithVirtualBufferAndBookmark(IKSoftStreamWithVirtualBuffer stream, long bufferLength)
		{
			Contract.Requires(stream != null);
			this.mBookmark = stream.EnterVirtualBufferBookmark();
			this.mCleanup = stream.EnterVirtualBuffer(bufferLength);
			this.mDisposed = false;
		}

		public void Dispose()
		{
			if (!this.mDisposed)
			{
				this.mCleanup.Dispose();
				this.mBookmark.Dispose();
				this.mDisposed = true;
			}
		}
	};
}
