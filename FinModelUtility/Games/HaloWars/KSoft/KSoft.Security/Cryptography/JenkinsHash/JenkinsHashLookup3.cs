#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	// http://burtleburtle.net/bob/c/lookup3.c
	// #REVIEW: hashlittle2 support (two results)?
	public abstract class JenkinsHashLookup3 : JenkinsHashLookup
	{
		const uint kGoldenRatio = 0xDEADBEEF;
		const int kBlockSize = 12;

		static uint rot(uint x, int k)
		{
			return (x << k) | (x >> (Bits.kInt32BitCount-k));
		}

		struct HashState
		{
			uint a, b, c;

			public uint Result { get { return this.c; } }

			public HashState(int length, uint seed)
			{
				this.a = this.b = this.c = kGoldenRatio + (uint)length + seed;
			}

			void Mix()
			{
				this.a -= this.c;
				this.a ^= rot(this.c, 4);
				this.c += this.b;
				this.b -= this.a;
				this.b ^= rot(this.a, 6);
				this.a += this.c;
				this.c -= this.b;
				this.c ^= rot(this.b, 8);
				this.b += this.a;
				this.a -= this.c;
				this.a ^= rot(this.c,16);
				this.c += this.b;
				this.b -= this.a;
				this.b ^= rot(this.a,19);
				this.a += this.c;
				this.c -= this.b;
				this.c ^= rot(this.b, 4);
				this.b += this.a;
			}

			void FinalMix()
			{
				this.c ^= this.b;
				this.c -= rot(this.b, 14);
				this.a ^= this.c;
				this.a -= rot(this.c, 11);
				this.b ^= this.a;
				this.b -= rot(this.a, 25);
				this.c ^= this.b;
				this.c -= rot(this.b, 16);
				this.a ^= this.c;
				this.a -= rot(this.c, 4);
				this.b ^= this.a;
				this.b -= rot(this.a, 14);
				this.c ^= this.b;
				this.c -= rot(this.b, 24);
			}

			void Fill(byte[] data, ref int i)
			{
				JenkinsHashLookup.Fill(ref this.a, ref this.b, ref this.c, data, ref i);
			}

			void Fill(char[] data, ref int i)
			{
				JenkinsHashLookup.Fill(ref this.a, ref this.b, ref this.c, data, ref i);
			}

			void Fill(string data, ref int i)
			{
				JenkinsHashLookup.Fill(ref this.a, ref this.b, ref this.c, data, ref i);
			}

			void FinalFill(byte[] data, ref int i, int length)
			{
				JenkinsHashLookup.FinalFill(ref this.a, ref this.b, ref this.c, data, ref i, length);
			}

			void FinalFill(char[] data, ref int i, int length)
			{
				JenkinsHashLookup.FinalFill(ref this.a, ref this.b, ref this.c, data, ref i, length);
			}

			void FinalFill(string data, ref int i, int length)
			{
				JenkinsHashLookup.FinalFill(ref this.a, ref this.b, ref this.c, data, ref i, length);
			}

			public void ProcessBlock(byte[] buffer, ref int index)
			{
				this.Fill(buffer, ref index);
				this.Mix();
			}

			public void ProcessBlock(char[] buffer, ref int index)
			{
				this.Fill(buffer, ref index);
				this.Mix();
			}

			public void ProcessBlock(string buffer, ref int index)
			{
				this.Fill(buffer, ref index);
				this.Mix();
			}

			public void ProcessFinalBlock(byte[] buffer, ref int index, int length)
			{
				this.FinalFill(buffer, ref index, length);
				if(length > 0)
					this.FinalMix();
			}

			public void ProcessFinalBlock(char[] buffer, ref int index, int length)
			{
				this.FinalFill(buffer, ref index, length);
				if (length > 0)
					this.FinalMix();
			}

			public void ProcessFinalBlock(string buffer, ref int index, int length)
			{
				this.FinalFill(buffer, ref index, length);
				if (length > 0)
					this.FinalMix();
			}
		};

		public static uint Hash(byte[] buffer, uint seed = 0, int index = 0, int length = -1)
		{
			Contract.Requires(buffer != null);

			if (length.IsNone())
				length = buffer.Length - index;

			HashState state = new HashState(length, seed);
			for (; index + kBlockSize <= length; )
				state.ProcessBlock(buffer, ref index);

			state.ProcessFinalBlock(buffer, ref index, length);

			return state.Result;
		}

		/// <remarks>Assumes all characters are ASCII bytes (ie, &lt;=0xFF)</remarks>
		public static uint Hash(char[] buffer, uint seed = 0, int index = 0, int length = -1)
		{
			Contract.Requires(buffer != null);

			if (length.IsNone())
				length = buffer.Length - index;

			HashState state = new HashState(length, seed);
			for (; index + kBlockSize <= length; )
				state.ProcessBlock(buffer, ref index);

			state.ProcessFinalBlock(buffer, ref index, length);

			return state.Result;
		}

		/// <remarks>Assumes all characters are ASCII bytes (ie, &lt;=0xFF)</remarks>
		public static uint Hash(string buffer, uint seed = 0)
		{
			Contract.Requires(buffer != null);

			int length = buffer.Length;
			int index = 0;

			HashState state = new HashState(length, seed);
			for (; index + kBlockSize <= length; )
				state.ProcessBlock(buffer, ref index);

			state.ProcessFinalBlock(buffer, ref index, length);

			return state.Result;
		}
	};
}