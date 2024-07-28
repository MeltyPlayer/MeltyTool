
namespace KSoft.Wwise.SoundBank
{
	sealed class AkSoundBankHierarchySound
		: AkSoundBankHierarchyObjectBase
	{
		public AkBankSourceData Source = new AkBankSourceData();

		public string Name;
		public uint BankId;

		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(this.Source);
			// There's more...
		}

		internal void PrepareForExtraction(AkSoundBank bank)
		{
			if (this.Source.StreamType != AkBankSourceData.SourceType.Data)
				this.BankId = bank.Id;
			else
				this.BankId = this.Source.MediaInfo.FileID;
		}
	};
}