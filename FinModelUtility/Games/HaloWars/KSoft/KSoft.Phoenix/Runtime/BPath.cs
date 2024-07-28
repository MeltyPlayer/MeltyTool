
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			cSaveMarkerPath1 = 0x2710
			;
	};

	public sealed class BPath
		: IO.IEndianStreamSerializable
	{
		const int cMaximumWaypoints = 0x2710;

		public BVector[] Waypoints;
		public byte Flags;
		public float PathLength;
		public uint CreationTime;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamVectorArray16(s, ref this.Waypoints, cMaximumWaypoints);
			s.Stream(ref this.Flags);
			s.Stream(ref this.PathLength);
			s.Stream(ref this.CreationTime);

			s.StreamSignature(cSaveMarker.cSaveMarkerPath1);
		}
		#endregion
	};
}