namespace KSoft.Wwise.FilePackage
{
	public sealed class AkLanguageMap
		: IO.IEndianStreamSerializable
	{
	AkLanguageMapEntry[] mEntries;

		bool mUseAsciiStrings;

		public AkLanguageMap(bool useAsciiStrings)
		{
			mUseAsciiStrings = useAsciiStrings;
		}

		internal uint TotalMapSize;

		public uint CalculateTotalMapSize()
		{
			uint char_size = (uint)(mUseAsciiStrings ? sizeof(byte) : sizeof(short));

			uint result = sizeof(uint) + (uint)(mEntries.Length * AkLanguageMapEntry.kSizeOf);
			foreach (var se in mEntries)
				result += (uint)(se.Value.Length + 1) * char_size;

			return result;
		}

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			Memory.Strings.StringStorage ss;
			if (mUseAsciiStrings)
				ss = Memory.Strings.StringStorage.CStringAscii;
			else
			{
				ss = s.ByteOrder == Shell.EndianFormat.Little
					? Memory.Strings.StringStorage.CStringUnicode
					: Memory.Strings.StringStorage.CStringUnicodeBigEndian;
			}

			s.StreamArrayInt32(ref mEntries);
			for (int x = 0; x < mEntries.Length; x++)
				s.Stream(ref mEntries[x].Value, ss);
		}
		#endregion
	};
}
