
namespace KSoft.Security.Cryptography
{
	// http://code.google.com/p/h2database/source/browse/trunk/h2/src/main/org/h2/security/XTEA.java
	public abstract class XTEABase
	{
		protected const uint kDelta = 0x9E3779B9;

		protected static readonly uint[] kDeltas = {
			//       0           1           2           3           4           5
			0x00000000, 0x9E3779B9, 0x9E3779B9, 0x3C6EF372, 0x3C6EF372, 0xDAA66D2B, // 0
			0xDAA66D2B, 0x78DDE6E4, 0x78DDE6E4, 0x1715609D, 0x1715609D, 0xB54CDA56, // 6
			0xB54CDA56, 0x5384540F, 0x5384540F, 0xF1BBCDC8, 0xF1BBCDC8, 0x8FF34781, // 12
			0x8FF34781, 0x2E2AC13A, 0x2E2AC13A, 0xCC623AF3, 0xCC623AF3, 0x6A99B4AC, // 18
			0x6A99B4AC, 0x08D12E65, 0x08D12E65, 0xA708A81E, 0xA708A81E, 0x454021D7, // 24
			0x454021D7, 0xE3779B90, 0xE3779B90, 0x81AF1549, 0x81AF1549, 0x1FE68F02, // 30
			0x1FE68F02, 0xBE1E08BB, 0xBE1E08BB, 0x5C558274, 0x5C558274, 0xFA8CFC2D, // 36
			0xFA8CFC2D, 0x98C475E6, 0x98C475E6, 0x36FBEF9F, 0x36FBEF9F, 0xD5336958, // 42
			0xD5336958, 0x736AE311, 0x736AE311, 0x11A25CCA, 0x11A25CCA, 0xAFD9D683, // 48
			0xAFD9D683, 0x4E11503C, 0x4E11503C, 0xEC48C9F5, 0xEC48C9F5, 0x8A8043AE, // 54
			0x8A8043AE, 0x28B7BD67, 0x28B7BD67, 0xC6EF3720							// 60
		};
		protected static readonly byte[] kKeyIndex = {
			0x00, 0x03, 0x01, 0x02, 0x02, 0x01, 0x03, 0x00, 0x00, 0x00, 0x01, 0x03,
			0x02, 0x02, 0x03, 0x01, 0x00, 0x00, 0x01, 0x00, 0x02, 0x03, 0x03, 0x02,
			0x00, 0x01, 0x01, 0x01, 0x02, 0x00, 0x03, 0x03, 0x00, 0x02, 0x01, 0x01,
			0x02, 0x01, 0x03, 0x00, 0x00, 0x03, 0x01, 0x02, 0x02, 0x01, 0x03, 0x01,
			0x00, 0x00, 0x01, 0x03, 0x02, 0x02, 0x03, 0x02, 0x00, 0x01, 0x01, 0x00,
			0x02, 0x03, 0x03, 0x02
		};

		protected static uint GetUInt32(byte[] b, int i)
		{
			return ((uint)(b[i++] << 24)) |
						((uint)(b[i++] << 16)) |
						((uint)(b[i++] << 8)) |
						((uint)b[i++]);
		}

		protected static void SetUInt32(byte[] b, int i, uint v)
		{
			b[i++] = (byte)(v >> 24); b[i++] = (byte)(v >> 16); b[i++] = (byte)(v >> 8); b[i++] = (byte)v;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
		protected uint k0, k1, k2, k3, k4, k5, k6, k7, k8, k9, k10, k11, k12, k13, k14, k15,
				k16, k17, k18, k19, k20, k21, k22, k23, k24, k25, k26, k27, k28, k29, k30, k31;

		public void SetKeyHackEnc()
		{
			uint[] r = new uint[32];
			for (int i = 1; i <= 32; i++)
				r[i - 1] = kDeltas[i];

			this.k0 = r[0];
			this.k1 = r[1];
			this.k2 = r[2];
			this.k3 = r[3];
			this.k4 = r[4];
			this.k5 = r[5];
			this.k6 = r[6];
			this.k7 = r[7];
			this.k8 = r[8];
			this.k9 = r[9];
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
		public void SetKeyHackDec()
		{
			uint[] r = new uint[32];
			for (int r_i = 0, d_i = 32; r_i < 32; r_i++, d_i--)
				r[r_i] = kDeltas[d_i];

			this.k0 = r[0];
			this.k1 = r[1];
			this.k2 = r[2];
			this.k3 = r[3];
			this.k4 = r[4];
			this.k5 = r[5];
			this.k6 = r[6];
			this.k7 = r[7];
			this.k8 = r[8];
			this.k9 = r[9];
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

		protected abstract void EncryptBlock(byte[] inb, byte[] outb, int off);
		protected abstract void DecryptBlock(byte[] inb, byte[] outb, int off);

		public void Encypt(byte[] bytes, int index, int length)
		{
			for (int x = index; x < (index + length); x += 8)
				this.EncryptBlock(bytes, bytes, x);
		}
		public void Decypt(byte[] bytes, int index, int length)
		{
			for (int x = index; x < (index + length); x += 8)
				this.DecryptBlock(bytes, bytes, x);
		}
		public void Decypt(byte[] inb, byte[] outb)
		{
			for (int x = 0; x < inb.Length; x += 8)
				this.DecryptBlock(inb, outb, x);
		}
	};
}
