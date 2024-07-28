
namespace KSoft.Phoenix.Runtime
{
	sealed class BProtoAction
		: IO.IEndianStreamSerializable
	{
		public const int kMaxCount = 0xC8;

		public float WorkRate, WorkRateVariance;
		public int AnimType;
		public /*float*/uint DamagePerAttack;
		public int MaxNumAttacksPerAnim;
		public float StrafingTurnRate, JoinBoardTime;
		public bool Disabled;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.WorkRate); s.Stream(ref this.WorkRateVariance);
			s.Stream(ref this.AnimType);
			s.Stream(ref this.DamagePerAttack);
			s.Stream(ref this.MaxNumAttacksPerAnim);
			s.Stream(ref this.StrafingTurnRate); s.Stream(ref this.JoinBoardTime);
			s.Stream(ref this.Disabled);
			s.StreamSignature(cSaveMarker.ProtoAction);
		}
		#endregion
	};
}