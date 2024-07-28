#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Security.Cryptography
{
	public sealed class XTEA256
		: XTEABase
	{
		public void SetKey(byte[] b)
		{
			// round = 6 + 52/_countof(uint[] v)
			const int k_round_count = 32;

			uint[] key = new uint[4];
			for (int i = 0; i < 16; )
			{
				key[i >> 2]=((uint)(b[i++] << 24)) |
							((uint)(b[i++] << 16)) |
							((uint)(b[i++] << 8)) |
							((uint) b[i++]);
			}

			uint[] r = new uint[k_round_count];
			for (int i = 0; i < r.Length; i++)
				r[i] = kDeltas[i] + key[kKeyIndex[i]];

			this.k0 =  r[0];
			this.k1 =  r[1];
			this.k2 =  r[2];
			this.k3 =  r[3];
			this.k4 =  r[4];
			this.k5 =  r[5];
			this.k6 =  r[6];
			this.k7 =  r[7];
			this.k8 =  r[8];
			this.k9 =  r[9];
			this.k10 = r[10];
			this.k11 = r[11];
			this.k12 = r[12];
			this.k13 = r[13];
			this.k14 = r[14];
			this.k15 = r[15];
			this.k16 = r[16];
			this.k17 = r[17];
			this.k18 = r[18];
			this.k19 = r[19];
			this.k20 = r[20];
			this.k21 = r[21];
			this.k22 = r[22];
			this.k23 = r[23];
			this.k24 = r[24];
			this.k25 = r[25];
			this.k26 = r[26];
			this.k27 = r[27];
			this.k28 = r[28];
			this.k29 = r[29];
			this.k30 = r[30];
			this.k31 = r[31];
		}

		protected override void EncryptBlock(byte[] input, byte[] output, int offset)
		{
			Contract.Assert(false); // #TODO
		}

		protected override void DecryptBlock(byte[] input, byte[] output, int offset)
		{
			uint y = GetUInt32(input, offset+0);
			uint z = GetUInt32(input, offset+4);

			uint v;
			v = y >> 5;
			v ^= this.k31;

			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k31; y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k30;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k29; y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k28;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k27; y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k26;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k25; y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k24;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k23; y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k22;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k21; y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k20;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k19; y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k18;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k17; y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k16;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k15; y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k14;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k13; y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k12;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k11; y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k10;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k9;  y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k8;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k7;  y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k6;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k5;  y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k4;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k3;  y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k2;
			z -= (((y >> 5) ^ (y << 4)) + y) ^ this.k1;  y -= (((z << 4) ^ (z >> 5)) + z) ^ this.k0;

			SetUInt32(output, offset + 0, y);
			SetUInt32(output, offset + 4, z);
		}
	};
}