using System;
using System.IO;
using System.Security.Cryptography;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct StreamHashComputer<T>
		where T : HashAlgorithm
	{
		/// <summary>Max size of the scratch buffer when we don't use a user specified preallocated buffer</summary>
		const int kMaxScratchBufferSize = 0x1000;

		private readonly T mAlgo;
		private readonly Stream mInputStream;
		private byte[] mScratchBuffer;
		private long mStartOffset;
		private long mCount;
		private readonly bool mRestorePosition;

		public Stream InputStream { get { return this.mInputStream; } }
		public long StartOffset { get { return this.mStartOffset; } }
		public long Count { get { return this.mCount; } }
		/// <summary>
		/// Does the input stream's current position get treated as the starting offset?
		/// </summary>
		public bool StartOffsetIsStreamPosition { get { return this.mStartOffset.IsNone(); } }

		public StreamHashComputer(T algo, Stream inputStream
			, bool restorePosition = false
			, byte[] preallocatedBuffer = null)
		{
			Contract.Requires<ArgumentNullException>(inputStream != null);
			Contract.Requires<ArgumentException>(inputStream.CanSeek);
			Contract.Requires<ArgumentException>(preallocatedBuffer == null || preallocatedBuffer.Length > 0);

			this.mAlgo = algo;
			this.mInputStream = inputStream;
			this.mScratchBuffer = preallocatedBuffer;
			this.mStartOffset = TypeExtensions.kNone;
			this.mCount = TypeExtensions.kNone;
			this.mRestorePosition = restorePosition;

			this.mAlgo.Initialize();
		}

		public void SetRangeAtCurrentOffset(long count)
		{
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);

			this.SetRangeAndOffset(TypeExtensions.kNone, count);
		}
		public void SetRangeAndOffset(long offset, long count)
		{
			Contract.Requires<ArgumentOutOfRangeException>(offset.IsNoneOrPositive());
			Contract.Requires<ArgumentOutOfRangeException>(count >= 0);
			Contract.Requires<ArgumentOutOfRangeException>(offset.IsNone() || (offset+count) <= this.InputStream.Length);

			this.mStartOffset = offset;
			this.mCount = count;
		}

		public T Compute()
		{
			Contract.Requires<InvalidOperationException>(this.StartOffset.IsNoneOrPositive(),
				"You need to call SetRange before calling this");
			Contract.Requires<InvalidOperationException>(this.Count >= 0,
				"You need to call SetRange before calling this");

			#region prologue

			this.mAlgo.Initialize();

			int buffer_size;
			byte[] buffer;

			bool uses_preallocated_buffer = this.mScratchBuffer != null;
			if (!uses_preallocated_buffer)
			{
				buffer_size = Math.Min((int) this.Count, kMaxScratchBufferSize);
				this.mScratchBuffer = new byte[buffer_size];
			}

			buffer = this.mScratchBuffer;
			buffer_size = this.mScratchBuffer.Length;

			long orig_pos = this.mInputStream.Position;
			if (!this.StartOffsetIsStreamPosition && this.StartOffset != orig_pos)
				this.mInputStream.Seek(this.StartOffset, SeekOrigin.Begin);
			#endregion

			for (long bytes_remaining = this.Count; bytes_remaining > 0;)
			{
				long num_bytes_to_read = Math.Min(bytes_remaining, buffer_size);
				int num_bytes_read = 0;
				do
				{
					int n = this.mInputStream.Read(buffer, num_bytes_read, (int)num_bytes_to_read);
					if (n == 0)
						break;

					num_bytes_read += n;
					num_bytes_to_read -= n;
				} while (num_bytes_to_read > 0);

				if (num_bytes_read > 0)
					this.mAlgo.TransformBlock(buffer, 0, num_bytes_read, null, 0);
				else
					break;

				bytes_remaining -= num_bytes_read;
			}

			this.mAlgo.TransformFinalBlock(buffer, 0, 0); // yes, 0 bytes, all bytes should have been taken care of already

			#region epilogue
			if (this.mRestorePosition)
				this.mInputStream.Seek(orig_pos, SeekOrigin.Begin);

			if (!uses_preallocated_buffer)
			{
				this.mScratchBuffer = null;
			}
			#endregion

			return this.mAlgo;
		}
	};
}
