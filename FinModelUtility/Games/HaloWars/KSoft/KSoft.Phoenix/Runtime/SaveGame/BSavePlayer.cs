
namespace KSoft.Phoenix.Runtime
{
	sealed class BSavePlayer
		: IO.IEndianStreamSerializable
	{
		public string Name;
		public string DisplayName;
		public int MPID, ScenarioID, CivID, TeamID, LeaderID;
		public ushort Difficulty; // BHalfFloat
		public sbyte PlayerType;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamPascalString32(ref this.Name);
			s.StreamPascalWideString32(ref this.DisplayName);
			s.Stream(ref this.MPID);
			s.Stream(ref this.ScenarioID);
			s.Stream(ref this.CivID);
			s.Stream(ref this.TeamID);
			s.Stream(ref this.LeaderID);
			s.Stream(ref this.Difficulty);
			s.Stream(ref this.PlayerType);
			s.StreamSignature(cSaveMarker.SetupPlayer);
		}
		#endregion
	};
}