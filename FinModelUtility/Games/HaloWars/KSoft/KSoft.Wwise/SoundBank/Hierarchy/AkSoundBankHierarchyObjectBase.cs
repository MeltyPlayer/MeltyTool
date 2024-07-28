
namespace KSoft.Wwise.SoundBank
{
	abstract partial class AkSoundBankHierarchyObjectBase
		: IO.IEndianStreamSerializable
	{
		public uint ID;

		#region Factory
		class AkSoundBankHierarchyDefaultImpl
			: AkSoundBankHierarchyObjectBase
		{
			HircType mType;

			public AkSoundBankHierarchyDefaultImpl(HircType type)
			{
				this.mType = type;
			}

			public override string ToString()
			{
				return this.mType.ToString();
			}
		};

		public static AkSoundBankHierarchyObjectBase New(HircType type)
		{
			switch (type)
			{
			case HircType.Sound:		return new AkSoundBankHierarchySound();
			case HircType.Action:		return new AkSoundBankHierarchyAction();
			case HircType.Event:		return new AkSoundBankHierarchyEvent();
			case HircType.RanSeqCntr:	return new AkSoundBankHierarchyRanSeqCntr();

			default:					return new AkSoundBankHierarchyDefaultImpl(type);
			}
		}
		#endregion

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.ID);
		}
		#endregion
	};
}