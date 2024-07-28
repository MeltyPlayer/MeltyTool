namespace KSoft.Wwise.FilePackage
{
	public sealed class AkLanguageMap
		: IO.IEndianStreamSerializable
	{
	AkLanguageMapEntry[] mEntries;

		bool mUseAsciiStrings;

		public AkLanguageMap(bool useAsciiStrings)
		{
			this.mUseAsciiStrings = useAsciiStrings;
		}

		internal uint TotalMapSize;

		public uint CalculateTotalMapSize()
		{
			uint char_size = (uint)(this.mUseAsciiStrings ? sizeof(byte) : sizeof(short));

			uint result = sizeof(uint) + (uint)(this.mEntries.Length * AkLanguageMapEntry.kSizeOf);
			foreach (var se in this.mEntries)
				result += (uint)(se.Value.Length + 1) * char_size;

			return result;
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			Memory.Strings.StringStorage ss;
			if (this.mUseAsciiStrings)
				ss = Memory.Strings.StringStorage.CStringAscii;
			else
			{
				ss = s.ByteOrder == Shell.EndianFormat.Little
					? Memory.Strings.StringStorage.CStringUnicode
					: Memory.Strings.StringStorage.CStringUnicodeBigEndian;
			}

			s.StreamArrayInt32(ref this.mEntries);
			for (int x = 0; x < this.mEntries.Length; x++)
				s.Stream(ref this.mEntries[x].Value, ss);
		}
		#endregion
	};
}
