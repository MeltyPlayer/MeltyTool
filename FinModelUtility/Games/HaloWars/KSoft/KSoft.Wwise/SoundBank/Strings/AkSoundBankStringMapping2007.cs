#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Wwise.SoundBank
{
	sealed class AkSoundBankStringMapping2007
		: AkSoundBankStringMappingBase
	{
		public struct StringHashEntry
			: IO.IEndianStreamSerializable
		{
			public uint Offset; // if this is -1, we have to skip it. TODO: serialize entries into memory stream?
			public uint Key, ID;

			public string Value;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref Offset);
			}
			#endregion
		};
		public class StringGroup
			: IO.IEndianStreamSerializable
		{
			AKBKHashHeader mHeader;
			public StringHashEntry[] Entries;

			public uint ID { get { return mHeader.Hash; } }

			#region IEndianStreamSerializable Members
			void SerializeGroupEntries(IO.EndianStream s)
			{
				s.StreamArrayInt32(ref Entries);
				for (int x = 0; x < Entries.Length; x++)
				{
					s.Stream(ref Entries[x].ID);
					s.Stream(ref Entries[x].Value, Memory.Strings.StringStorage.CStringAscii);
					s.Stream(ref Entries[x].Key);
				}
			}
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref mHeader);

				long eos = EndOfStream(s, mHeader);
				SerializeGroupEntries(s);
				Contract.Assert(s.BaseStream.Position == eos);
			}
			#endregion
		};

		public StringHashEntry[] Events;

		static void SerializeEntries(IO.EndianStream s, ref StringHashEntry[] entries)
		{
			s.StreamArrayInt32(ref entries);
			for (int x = 0; x < entries.Length; x++)
			{
				s.Stream(ref entries[x].Value, Memory.Strings.StringStorage.CStringAscii);
				s.Stream(ref entries[x].Key);
			}
		}

	void SerializeStringType(IO.EndianStream s, AKBKHashHeader hdr, AkSoundBank bank)
		{
			switch (hdr.Type)
			{
				case AkSoundBankStringMappingBase.StringType.OldEvents:
					SerializeEntries(s, ref Events);
					break;
			}
		}

		public override void Serialize(IO.EndianStream s, AkSubchunkHeader header)
		{
			Contract.Assert(s.IsReading);

			var bank = s.Owner as AkSoundBank;

			long eos = EndOfStream(s, header);

			while (s.BaseStream.Position != eos)
			{
				AKBKHashHeader hdr = new AKBKHashHeader();
				hdr.Serialize(s);

				SerializeStringType(s, hdr, bank);
			}
		}
	};
}