#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Wwise.SoundBank
{
	sealed class AkSoundBankStringMapping
		: AkSoundBankStringMappingBase
	{
		static readonly Memory.Strings.StringStorage kStringStorage =
					new Memory.Strings.StringStorage(Memory.Strings.StringStorageWidthType.Ascii, Memory.Strings.StringStorageLengthPrefix.Int8);
		static readonly Text.StringStorageEncoding kStringEncoding = new Text.StringStorageEncoding(kStringStorage);

		void SerializeStringType(IO.EndianStream s, AKBKHashHeader hdr, AkSoundBank bank)
		{
			Contract.Assert(hdr.Type == StringType.Bank);

			uint bank_id = uint.MaxValue;
			string str = null;

			s.Stream(ref bank_id);
			s.Stream(ref str, kStringEncoding);

			bank.MapIdToName(bank_id, str);
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

				for (int x = 0; x < hdr.Size; x++)
					this.SerializeStringType(s, hdr, bank);
			}
		}
	};
}