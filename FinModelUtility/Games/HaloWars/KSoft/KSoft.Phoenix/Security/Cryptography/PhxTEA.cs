namespace KSoft.Security.Cryptography
{
	public sealed class PhxTEA
	{
		#region Keys
		const int kKeySize = 3;

		public static readonly ulong[] kKeyEra = new ulong[kKeySize] {
			0xBC3EB6B4D0471DDB,
			0x8299E6431912BE73,
			0x4601515D43D26DF5,
		};
		public static readonly ulong[] kKeyGameFile = new ulong[kKeySize] {
			0x194F8D77DF360283,
			0x1385AC1E2122F575,
			0xA7392D249DC2C737,
		};

		ulong[] mKey;

		public void InitializeKey(ulong[] key, ulong userKey = 0)
		{
			if (userKey == 0)
				mKey = key;
			else
				mKey = new ulong[kKeySize] { key[0], key[1], userKey };
		}
		#endregion

		IO.EndianReader mStreamIn;
		IO.EndianWriter mStreamOut;
		ulong[] mBufferIn, mBufferOut;

		void InitializeBuffers()
		{
			mBufferIn = new ulong[kBlocksPerIteration];
			mBufferOut = new ulong[kBlocksPerIteration];
		}

		public PhxTEA(IO.EndianReader streamIn, IO.EndianWriter streamOut)
		{
			mStreamIn = streamIn;
			mStreamOut = streamOut;
			InitializeBuffers();
		}

		void FillBufferIn()
		{
			for (int x = 0; x < mBufferIn.Length; x++)
				mStreamIn.Read(out mBufferIn[x]);
		}
		void FillBufferOut()
		{
			for (int x = 0; x < mBufferOut.Length; x++)
				mStreamOut.Write(mBufferOut[x]);
		}

		void ProcessBuffer(long size, ProcessIterationProc proc)
		{
			if (size == 0)
				size = mStreamIn.BaseStream.Length - mStreamIn.BaseStream.Position;

			uint interation_count = GetIterationsCount(size);

			for (uint x = 0; x < interation_count; x++)
			{
				FillBufferIn();

				proc(mKey, mBufferIn, mBufferOut, x);

				FillBufferOut();
			}
		}
		public void Decrypt(long size = 0)
		{
			ProcessBuffer(size, DecryptIteration);
		}
		public void Encrypt(long size = 0)
		{
			ProcessBuffer(size, EncryptIteration);
		}

		#region Implementation
		delegate void ProcessIterationProc(ulong[] key, ulong[] buffIn, ulong[] buffOut, uint iteration);

		const uint kDelta = 0x9E3779B9;
		const int kRoundCountShift = 4;
		const int kRoundCount = 1 << 4;

		const int kBlocksPerIterationShift = 3;
		const int kBlocksPerIteration = 1 << kBlocksPerIterationShift;

		const ulong kIterationMod = 0xBCCD0923;

		static uint GetIterationsCount(long size)
		{
			const int kBlockSizeShift = 3;

			uint block_count = (uint)(size >> kBlockSizeShift);

			return block_count >> kBlocksPerIterationShift;
		}

		static void Mod0(ref ulong q, out ulong v)
		{
			const ulong kMod = 0x15EF0AF334248FE2;

			q = (((uint)q << 17) ^ q);
			q = ((uint)q >> 13) ^ q;
			q = ((uint)q << 5) ^ q;
			v = (uint)q + kMod;
		}
		static void Mod1(ref ulong q, out ulong v)
		{
			const ulong kMod = 0xEA10F50CCBDB701E;

			q = (((uint)q << 17) ^ q);
			q = ((uint)q >> 13) ^ q;
			q = ((uint)q << 5) ^ q;
			v = (uint)q + kMod;
		}

		static void DecryptRounds(ref ulong v, ulong key0, ulong key1)
		{
			ulong lv0 = v >> 32, lv1 = v & 0xFFFFFFFF;
			uint sum = kDelta << kRoundCountShift;
			for (int x = 0; x < kRoundCount; x++)
			{
				lv0 = (lv0
					- (((uint)lv1 >> 5) ^ sum))
					- (lv1 ^ key1)
					- ((uint)lv1 << 4)
					- (key1 >> 32);

				lv1 = (lv1
					- (((uint)lv0 >> 5) ^ sum))
					- (lv0 ^ key0)
					- ((uint)lv0 << 4)
					- (key0 >> 32);

				sum -= kDelta;
			}

			v = (lv0 << 32) | (lv1 & 0xFFFFFFFF);
		}
		static void EncryptRounds(ref ulong v, ulong key0, ulong key1)
		{
			ulong lv0 = v >> 32, lv1 = v & 0xFFFFFFFF;
			uint sum = 0;
			for (int x = 0; x < kRoundCount; x++)
			{
				sum += kDelta;

				lv1 = (lv1
					+ (((uint)lv0 >> 5) ^ sum))
					+ (lv0 ^ key0)
					+ ((uint)lv0 << 4)
					+ (key0 >> 32);

				lv0 = (lv0
					+ (((uint)lv1 >> 5) ^ sum))
					+ (lv1 ^ key1)
					+ ((uint)lv1 << 4)
					+ (key1 >> 32);
			}

			v = (lv0 << 32) | (lv1 & 0xFFFFFFFF);
		}

		static void DecryptBlock(ulong in0, ulong in1, ulong in2, ulong in3,
			out ulong out0, out ulong out1, out ulong out2, out ulong out3,
			ulong key0, ulong key1)
		{
			out0 = in0; DecryptRounds(ref out0, key0, key1);
			out1 = in1; DecryptRounds(ref out1, key0, key1);
			out2 = in2; DecryptRounds(ref out2, key0, key1);
			out3 = in3; DecryptRounds(ref out3, key0, key1);
		}
		static void EncryptBlock(ulong in0, ulong in1, ulong in2, ulong in3,
			out ulong out0, out ulong out1, out ulong out2, out ulong out3,
			ulong key0, ulong key1)
		{
			out0 = in0; EncryptRounds(ref out0, key0, key1);
			out1 = in1; EncryptRounds(ref out1, key0, key1);
			out2 = in2; EncryptRounds(ref out2, key0, key1);
			out3 = in3; EncryptRounds(ref out3, key0, key1);
		}

		static void DecryptIteration(ulong[] key, ulong[] buffIn, ulong[] buffOut, uint iteration)
		{
			const uint index = 0;// iteration * kBlocksPerIteration;
			ulong	block0 = buffIn[index+0],	block1 = buffIn[index+1],	block2 = buffIn[index+2],	block3 = buffIn[index+3],	
					block4 = buffIn[index+4],	block5 = buffIn[index+5],	block6 = buffIn[index+6],	block7 = buffIn[index+7];

			ulong t1, t2, t3, t4;
			DecryptBlock(	block0, block1, block2, block3,
							out t1, out t2, out t3, out t4,		key[0], key[1] );

			ulong t5, t6, t7, t8;
			DecryptBlock(	block6 ^ block7,	block4 ^ block5 ^ block7, 
							block4 ^ block6,	block7 ^ block5,
							out t5, out t6, out t7, out t8,		key[1], key[2]);

			ulong t9, t10, t11, t12;
			DecryptBlock(	t4 ^ block7, 
							(t1 - (block6 ^ block7)) ^ t2 ^ block6 ^ block5, 
							t2 ^ block6 ^ block5 ^ (t3 + (block5 ^ block4)),
							t4 ^ block7 ^ (t3 + (block5 ^ block4)),
							out t9, out t10, out t11, out t12,	key[1], key[0]);

			ulong iter_mod = iteration + kIterationMod;
			if (iter_mod == 0) iter_mod++;

			ulong q = iter_mod;
			ulong q0, q1, q2, q3, q4, q5, q6, q7;

			Mod0(ref q, out q0);	Mod1(ref q, out q1);
			Mod0(ref q, out q2);	Mod1(ref q, out q3);
			Mod0(ref q, out q4);	Mod1(ref q, out q5);
			Mod0(ref q, out q6);	Mod1(ref q, out q7);

			q0 ^= t9;
			q1 ^= t10;
			q2 ^= t11;
			q3 ^= t12;
			q4 ^= t5 ^ t4 ^ block7;
			q5 ^= t3 + (block5 ^ block4) + t6;
			q6 ^= t7 ^ t2 ^ block6 ^ block5;
			q7 ^= t8 - (t1 - (block6 ^ block7));

			buffOut[index+0] = q0; buffOut[index+1] = q1; buffOut[index+2] = q2; buffOut[index+3] = q3;
			buffOut[index+4] = q4; buffOut[index+5] = q5; buffOut[index+6] = q6; buffOut[index+7] = q7;
		}
		static void EncryptIteration(ulong[] key, ulong[] buffIn, ulong[] buffOut, uint iteration)
		{
			const uint index = 0;//iteration * kBlocksPerIteration;
			ulong	block0 = buffIn[index+0],	block1 = buffIn[index+1],	block2 = buffIn[index+2],	block3 = buffIn[index+3],	
					block4 = buffIn[index+4],	block5 = buffIn[index+5],	block6 = buffIn[index+6],	block7 = buffIn[index+7];

			ulong iter_mod = iteration + kIterationMod;
			if (iter_mod == 0) iter_mod++;

			ulong q = iter_mod;
			ulong q0, q1, q2, q3, q4, q5, q6, q7;

			Mod0(ref q, out q0);	Mod1(ref q, out q1);
			Mod0(ref q, out q2);	Mod1(ref q, out q3);
			Mod0(ref q, out q4);	Mod1(ref q, out q5);
			Mod0(ref q, out q6);	Mod1(ref q, out q7);

			ulong	t9 = q0 ^ block0, 
					t10 = q1 ^ block1, 
					t11 = q2 ^ block2, 
					t12 = q3 ^ block3;

			ulong o1, o2, o3, o4;
			EncryptBlock(	t9, t10, t11, t12, 
							out o1, out o2, out o3, out o4, key[1], key[0]);

			ulong o5, o6, o7, o8;
			EncryptBlock(	o1 ^ q4 ^ block4,
							(q5 ^ block5) - (o1 ^ o4),
							o1 ^ o3 ^ o4 ^ q6 ^ block6,
							(q7 ^ block7) + (o1 ^ o2 ^ o3 ^ o4),
							out o5, out o6, out o7, out o8, key[1], key[2]);

			ulong o9, o10, o11, o12;
			EncryptBlock(	(o1 ^ o2 ^ o3 ^ o4) + o5,
							o5 ^ o8 ^ o1 ^ o3 ^ o4,
							(o1 ^ o4) - (o5 ^ o7 ^ o8),
							o5 ^ o6 ^ o7 ^ o8 ^ o1,
							out o9, out o10, out o11, out o12, key[0], key[1]);

			buffOut[index+0] = o9; buffOut[index+1] = o10; buffOut[index+2] = o11; buffOut[index+3] = o12;
			buffOut[index+4] = o6 ^ o8;
			buffOut[index+5] = o5 ^ o6 ^ o7;
			buffOut[index+6] = o6 ^ o7 ^ o8;
			buffOut[index+7] = o5 ^ o6 ^ o7 ^ o8;
		}
		#endregion
	};
}
