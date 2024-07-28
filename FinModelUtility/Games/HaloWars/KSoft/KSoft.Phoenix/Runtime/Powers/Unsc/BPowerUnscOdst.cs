
using BProtoObjectID = System.Int32;
using BAIMissionID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerUnscOdst
		: BPower
	{
		public struct BODSTDrop
			: IO.IEndianStreamSerializable
		{
			// A BVector field?
			public ulong Unknown0, Unknown8;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.Unknown0); s.Stream(ref this.Unknown8);
			}
			#endregion
		};

		public float SquadSpawnDelay;
		public BODSTDrop[] ActiveDrops;
		public BProtoObjectID ProjectileProtoID, ODSTProtoSquadID;
		public BAIMissionID AddToMissionID;
		public bool ReadyForShutdown;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.Stream(ref this.SquadSpawnDelay);
			BSaveGame.StreamArray(s, ref this.ActiveDrops);
			s.Stream(ref this.ProjectileProtoID); s.Stream(ref this.ODSTProtoSquadID);
			s.Stream(ref this.AddToMissionID);
			s.Stream(ref this.ReadyForShutdown);
		}
		#endregion
	};
}