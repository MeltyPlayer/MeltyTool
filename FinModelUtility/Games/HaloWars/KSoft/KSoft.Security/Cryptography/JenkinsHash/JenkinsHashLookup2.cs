#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	/// <remarks>http://bretm.home.comcast.net/~bretm/hash/7.html</remarks>
	// http://burtleburtle.net/bob/c/lookup2.c
	public abstract class JenkinsHashLookup2 : JenkinsHashLookup
	{
		const uint kGoldenRatio = 0x9E3779B9;
		const int kBlockSize = 12; // 96 bits

		struct HashState
		{
			uint a, b, c;

			public uint Result { get { return this.c; } }

			public HashState(uint seed)
			{
				this.a = this.b = kGoldenRatio;
				this.c = seed;
			}

			void Mix()
			{
				this.a -= this.b;
				this.a -= this.c;
				this.a ^= (this.c >> 13);
				this.b -= this.c;
				this.b -= this.a;
				this.b ^= (this.a <<  8);
				this.c -= this.a;
				this.c -= this.b;
				this.c ^= (this.b >> 13);
				this.a -= this.b;
				this.a -= this.c;
				this.a ^= (this.c >> 12);
				this.b -= this.c;
				this.b -= this.a;
				this.b ^= (this.a << 16);
				this.c -= this.a;
				this.c -= this.b;
				this.c ^= (this.b >>  5);
				this.a -= this.b;
				this.a -= this.c;
				this.a ^= (this.c >>  3);
				this.b -= this.c;
				this.b -= this.a;
				this.b ^= (this.a << 10);
				this.c -= this.a;
				this.c -= this.b;
				this.c ^= (this.b >> 15);
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
				this.c += (uint)length;

				JenkinsHashLookup.FinalFill(ref this.a, ref this.b, ref this.c, data, ref i, length);
			}

			void FinalFill(char[] data, ref int i, int length)
			{
				this.c += (uint)length;

				JenkinsHashLookup.FinalFill(ref this.a, ref this.b, ref this.c, data, ref i, length);
			}

			void FinalFill(string data, ref int i, int length)
			{
				this.c += (uint)length;

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
				this.Mix();
			}

			public void ProcessFinalBlock(char[] buffer, ref int index, int length)
			{
				this.FinalFill(buffer, ref index, length);
				this.Mix();
			}

			public void ProcessFinalBlock(string buffer, ref int index, int length)
			{
				this.FinalFill(buffer, ref index, length);
				this.Mix();
			}
		};

		public static uint Hash(byte[] buffer, uint seed = 0, int index = 0, int length = -1)
		{
			Contract.Requires(buffer != null);

			if (length.IsNone())
				length = buffer.Length - index;

			HashState state = new HashState(seed);
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

			HashState state = new HashState(seed);
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

			HashState state = new HashState(seed);
			for (; index + kBlockSize <= length; )
				state.ProcessBlock(buffer, ref index);

			state.ProcessFinalBlock(buffer, ref index, length);

			return state.Result;
		}
	};
}