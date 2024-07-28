
using BVector = System.Numerics.Vector4;
using BCost = System.Single;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovGlassing
		: BPower
	{
		const int cMaximumWaypoints = 0xC8;
		const int cMaximumBeamPathLength = 0xC8;

		public BVector[] Waypoints;
		public BEntityID BeamID, AirImpactObjectID;
		public double NextDamageTime;
		public BVector DesiredBeamPosition;
		public BVector[] BeamPath;
		public int[] RevealedTeamIDs; // BTeamID
		public BCost[] CostPerTick;
		public BProtoObjectID Projectile;
		public float TickLength, MinBeamDistance, MaxBeamDistance;
		public uint CommandInterval;
		public float MaxBeamSpeed;
		public int LOSMode;
		public bool UsePath;
		public float AudioReactionTimer;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);
			var sg = s.Owner as BSaveGame;

			BSaveGame.StreamVectorArray(s, ref this.Waypoints, cMaximumWaypoints);
			s.Stream(ref this.BeamID); s.Stream(ref this.AirImpactObjectID);
			s.Stream(ref this.NextDamageTime);
			s.StreamV(ref this.DesiredBeamPosition);
			BSaveGame.StreamVectorArray(s, ref this.BeamPath, cMaximumBeamPathLength);
			BSaveGame.StreamArray(s, ref this.RevealedTeamIDs);
			sg.StreamBCost(s, ref this.CostPerTick);
			s.Stream(ref this.Projectile);
			s.Stream(ref this.TickLength); s.Stream(ref this.MinBeamDistance); s.Stream(ref this.MaxBeamDistance);
			s.Stream(ref this.CommandInterval);
			s.Stream(ref this.MaxBeamSpeed);
			s.Stream(ref this.LOSMode);
			s.Stream(ref this.UsePath);
			s.Stream(ref this.AudioReactionTimer);
		}
		#endregion
	};
}