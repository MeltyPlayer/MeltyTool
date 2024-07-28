
namespace KSoft.Wwise.SoundBank
{
	partial class AkSoundBankObjectBase
	{
		static readonly Values.GroupTagData32 kMediaIndexSignature =
					new Values.GroupTagData32("DIDX", "audiokinetic_sound_bank_data_index"); // BankDataIndexChunkID

		static AkSoundBankObjectBase NewDIDX(uint generatorVersion)
		{
			return new AkSoundBankDataIndex();
		}
	};

	sealed class AkSoundBankDataIndex
		: AkSoundBankObjectBase
	{
		public AkMediaHeader[] LoadedMedia;

		public override void Serialize(IO.EndianStream s, AkSubchunkHeader header)
		{
			if (s.IsReading)
				this.LoadedMedia = new AkMediaHeader[header.ChunkSize / AkMediaHeader.kSizeOf];

			s.StreamArray(this.LoadedMedia);
		}
	};
}