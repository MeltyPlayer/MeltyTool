using System;
#if CONTRACTS_FULL_SHIM
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif
using HashAlgorithm = System.Security.Cryptography.HashAlgorithm;

namespace KSoft.Security.Cryptography
{
	public abstract class BlockHashAlgorithm
		: HashAlgorithm
	{
		byte[] mBlockBuffer;

		protected long TotalBytesProcessed { get; private set; }

		/// <summary>The size in bytes of an individual block.</summary>
		public int BlockSize { get { return mBlockBuffer.Length; } }
		/// <summary>The number of bytes currently in the buffer waiting to be processed.</summary>
		public int BlockBytesRemaining { get; private set; }
		internal byte[] InternalBlockBuffer { get { return mBlockBuffer; } }

		/// <summary>Initializes a new instance of the BlockHashAlgorithm class.</summary>
		/// <param name="blockSize">The size in bytes of an individual block.</param>
		protected BlockHashAlgorithm(int blockSize, int hashSize) : base()
		{
			base.HashSizeValue = hashSize;

			mBlockBuffer = new byte[blockSize];
		}

		/// <summary>Process a block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		protected abstract void ProcessBlock(byte[] inputBuffer, int inputOffset, int inputLength);

		/// <summary>Process the last block of data.</summary>
		/// <param name="inputBuffer">The block of data to process.</param>
		/// <param name="inputOffset">Where to start in the block.</param>
		/// <param name="inputCount">How many bytes need to be processed.</param>
		/// <returns>The results of the completed hash calculation.</returns>
		protected abstract byte[] ProcessFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount);

		#region HashAlgorithm
		/// <summary>Initializes the algorithm.</summary>
		/// <remarks>If this function is overriden in a derived class, the new function should call back to
		/// this function or you could risk garbage being carried over from one calculation to the next.</remarks>
		public override void Initialize()
		{
			Array.Clear(mBlockBuffer, 0, mBlockBuffer.Length);
			BlockBytesRemaining = 0;
			TotalBytesProcessed = 0;
		}

		/// <summary>Performs the hash algorithm on the data provided.</summary>
		/// <param name="array">The array containing the data.</param>
		/// <param name="startIndex">The position in the array to begin reading from.</param>
		/// <param name="count">How many bytes in the array to read.</param>
		protected override void HashCore(byte[] array, int startIndex, int count)
		{
			// Use what may already be in the buffer.
			if (BlockBytesRemaining > 0)
			{
				if (count + BlockBytesRemaining < BlockSize)
				{
					// Still don't have enough for a full block, just store it.
					Array.Copy(array, startIndex, mBlockBuffer, BlockBytesRemaining, count);
					BlockBytesRemaining += count;
					return;
				}
				else
				{
					// Fill out the buffer to make a full block, and then process it.
					int i = BlockSize - BlockBytesRemaining;
					Array.Copy(array, startIndex, mBlockBuffer, BlockBytesRemaining, i);
					ProcessBlock(mBlockBuffer, 0, 1);
					TotalBytesProcessed += BlockSize;
					BlockBytesRemaining = 0;
					startIndex += i;
					count -= i;
				}
			}

			// For as long as we have full blocks, process them.
			if (count >= BlockSize)
			{
				ProcessBlock(array, startIndex, count / BlockSize);
				TotalBytesProcessed += count - count % BlockSize;
			}

			// If we still have some bytes left, store them for later.
			int bytesLeft = count % BlockSize;
			if (bytesLeft != 0)
			{
				Array.Copy(array, ((count - bytesLeft) + startIndex), mBlockBuffer, 0, bytesLeft);
				BlockBytesRemaining = bytesLeft;
			}
		}

		/// <summary>Performs any final activities required by the hash algorithm.</summary>
		/// <returns>The final hash value.</returns>
		protected override byte[] HashFinal()
		{
			return ProcessFinalBlock(mBlockBuffer, 0, BlockBytesRemaining);
		}
		#endregion
	};
}
