
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Runtime
{
	sealed class BParametricSplineCurve
		: IO.IEndianStreamSerializable
	{
		public BVector A0, A1, A2;
		public bool Valid;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamV(ref this.A0);
			s.StreamV(ref this.A1);
			s.StreamV(ref this.A2);
			s.Stream(ref this.Valid);
		}
		#endregion
	};
}