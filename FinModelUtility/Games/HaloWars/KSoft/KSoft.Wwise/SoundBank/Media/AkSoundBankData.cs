
namespace KSoft.Wwise.SoundBank
{
	partial class AkSoundBankObjectBase
	{
		static readonly Values.GroupTagData32 kDataSignature =
					new Values.GroupTagData32("DATA", "audiokinetic_sound_bank_data"); // BankDataChunkID

		static AkSoundBankObjectBase NewDATA(uint generatorVersion)
		{
			return new AkSoundBankData();
		}
	};

	sealed class AkSoundBankData
		: AkSoundBankObjectBase
	{
		public byte[] Buffer;

		public override void Serialize(IO.EndianStream s, AkSubchunkHeader header)
		{
			if (s.IsReading)
				this.Buffer = new byte[header.ChunkSize];

			s.Stream(this.Buffer);
		}
	};
}