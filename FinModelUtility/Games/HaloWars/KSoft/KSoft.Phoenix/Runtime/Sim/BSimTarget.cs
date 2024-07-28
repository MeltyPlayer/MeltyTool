
using BVector = System.Numerics.Vector4;
using BAbilityID = System.Int32;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BSimTarget
		: IO.IEndianStreamSerializable
	{
		public BVector Position;
		public BEntityID ID;
		public float Range;
		public BAbilityID AbilityID;
		public bool PositionValid, RangeValid, AbilityIDValid;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamV(ref this.Position);
			s.Stream(ref this.ID);
			s.Stream(ref this.Range);
			s.Stream(ref this.AbilityID);
			s.Stream(ref this.PositionValid); s.Stream(ref this.RangeValid); s.Stream(ref this.AbilityIDValid);
		}
		#endregion
	};
}