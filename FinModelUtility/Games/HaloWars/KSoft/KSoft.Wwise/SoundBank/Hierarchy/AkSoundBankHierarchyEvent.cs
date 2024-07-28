
namespace KSoft.Wwise.SoundBank
{
	sealed class AkSoundBankHierarchyEvent
		: AkSoundBankHierarchyObjectBase
	{
		public uint[] ActionList;

		public string Name;

		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.StreamArrayInt32(ref this.ActionList, s.Stream);
		}
	};
}