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
	/// <summary>
	/// Records the current position of the stream, and returns the stream's cursor to that position when the context object is disposed
	/// </summary>
	[SuppressMessage("Microsoft.Design", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes")]
	public struct StreamPositionContext : IDisposable
	{
		readonly long mPosition;
		Stream mStream;

		#region Ctor
		public StreamPositionContext(Stream baseStream)
		{
			Contract.Requires<ArgumentNullException>(baseStream != null);
			Contract.Requires<InvalidOperationException>(baseStream.CanSeek);

			this.mPosition = baseStream.Position;
			this.mStream = baseStream;
		}

		public StreamPositionContext(BinaryReader stream) : this(stream.BaseStream)
		{
			Contract.Requires<ArgumentNullException>(stream != null);
		}
		public StreamPositionContext(BinaryWriter stream) : this(stream.BaseStream)
		{
			Contract.Requires<ArgumentNullException>(stream != null);
		}

		public StreamPositionContext(StreamReader stream) : this(stream.BaseStream)
		{
			Contract.Requires<ArgumentNullException>(stream != null);
		}
		public StreamPositionContext(StreamWriter stream) : this(stream.BaseStream)
		{
			Contract.Requires<ArgumentNullException>(stream != null);
		}
		#endregion

		public void Dispose()
		{
			if (this.mStream != null)
			{
				this.mStream.Seek(this.mPosition, SeekOrigin.Begin);
				this.mStream = null;
			}
		}
	};
}
