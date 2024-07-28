using System;
using System.Collections.Generic;
using System.IO;

namespace KSoft.Phoenix.zPatching
{
	// Patches? We don't need no stinkin- owait.
	// I hacked this together. I hope I never have to touch this again.

	public sealed class WinExePatcherProcessHeaderData
	{
		public short[] BytePattern;
		public int BytePatternNextJmpOffset;
		public int BytePatternModJmpOffset;

		public string SourceExeFileName;
		public byte[] SourceExeBytes;
		public List<int> PatternFileOffsets = new List<int>();

		public int ModJmpFileOffset;
		public int ModJmpVa;

		public WinExePatcherProcessHeaderData()
		{
			this.BytePattern = new short[] {
				/*
					call    sub
					nop
					mov     edi, ebx
					cmp     ebx, [r14+20h]
					jnb     i32
				*/
				0xE8, /*0xD7*/-1, /*0xE5*/-1, 0xFF, 0xFF,
				0x90,
				/*0x8B*/-1, 0xFB,
				0x41, 0x3B, 0x5E, 0x20,
				0x0F, 0x83,   -1, 0x00, 0x00, 0x00,

				// alignment asm bytes, can't rely on these :(
				//0x66, 0x66,
				//0x0F, 0x1F, 0x84, 0x00, 0x00, 0x00, 0x00, 0x00,
			};
			this.BytePatternNextJmpOffset = this.BytePattern.Length - sizeof(uint);
			this.BytePatternModJmpOffset = this.BytePattern.Length - sizeof(uint) - sizeof(ushort);
		}

		public bool ReadSourceExeBytes(string sourceExeFileName)
		{
			try
			{
				this.SourceExeFileName = sourceExeFileName;
				this.SourceExeBytes = File.ReadAllBytes(this.SourceExeFileName);
			} catch (Exception ex)
			{
				Debug.Trace.Phoenix.TraceData(System.Diagnostics.TraceEventType.Error, TypeExtensions.kNone
					, "Failed to read " + sourceExeFileName
					, ex);
				return false;
			}

			return true;
		}

		public bool FindPatterns()
		{
			this.PatternFileOffsets.Clear();

			int offset = 0;
			while (PhxUtil.FindBytePattern(this.PatternFileOffsets, this.SourceExeBytes, ref offset, this.BytePattern))
			{
			}

			return this.PatternFileOffsets.Count == 1;
		}

		public bool CalculateModJmp()
		{
			this.ModJmpFileOffset = this.ModJmpVa = TypeExtensions.kNone;

			int file_offset = this.PatternFileOffsets[0];
			int next_jmp_index = file_offset + this.BytePatternNextJmpOffset;
			int next_jmp_offset_va = BitConverter.ToInt32(this.SourceExeBytes, next_jmp_index);
			int good_jmp_base = next_jmp_offset_va;
			good_jmp_base += sizeof(uint);
			good_jmp_base += next_jmp_index;

			// jnz short i8
			if (0x75 != this.SourceExeBytes[good_jmp_base])
				return false;
			int good_asm_offset_va = this.SourceExeBytes[good_jmp_base + 1];
			int good_asm_index = (good_jmp_base + 2);
			good_asm_index += good_asm_offset_va;

			// 0xE9 .. .. .. ..
			int mod_jmp_base = file_offset + this.BytePatternModJmpOffset;
			mod_jmp_base += sizeof(byte) + sizeof(uint);
			int mod_jmp_offset_va = good_asm_index;
			mod_jmp_offset_va -= mod_jmp_base;

			this.ModJmpFileOffset = file_offset + this.BytePatternModJmpOffset;
			this.ModJmpVa = mod_jmp_offset_va;

			return true;
		}

		public void ApplyModJmp()
		{
			int index = this.ModJmpFileOffset;
			this.SourceExeBytes[index++] = 0xE9;
			Bitwise.ByteSwap.ReplaceBytes(this.SourceExeBytes, index, this.ModJmpVa);
		}
	};
}