
using BVector = System.Numerics.Vector4;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscOdstUser
		: BPowerUser
	{
		public string HelpString;
		public BPowerHelperHudSounds HudSounds = new BPowerHelperHudSounds();
		public int LOSMode;
		public BProtoObjectID ODSTProtoSquadID, ODSTProtoObjectID;
		public int CanFire;
		public BVector ValidDropLocation;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.StreamPascalWideString32(ref this.HelpString);
			s.Stream(this.HudSounds);
			s.Stream(ref this.LOSMode);
			s.Stream(ref this.ODSTProtoSquadID); s.Stream(ref this.ODSTProtoObjectID);
			s.Stream(ref this.CanFire);
			s.StreamV(ref this.ValidDropLocation);
		}
		#endregion
	};
}