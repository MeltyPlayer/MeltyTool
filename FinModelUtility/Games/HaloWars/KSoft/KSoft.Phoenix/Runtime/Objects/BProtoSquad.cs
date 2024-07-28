
using BProtoSquadID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			ProtoSquad1 = 0x2710
			;
	};

	sealed class BProtoSquad
		: BProtoObjectBase
	{
		public BProtoSquadID ProtoID;
		public float MaxHP, MaxSP, MaxAmmo; // SP=shield points?
		public int Level, TechLevel, DisplayNameIndex,
			CircleMenuIconID, AltCircleMenuIconID, HPBar;
		public bool OneTimeSpawnUsed, KBAware;
		public bool HasOverrideNodes;
		public BProtoSquadNodeOverride[] OverrideNodes;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);
			var sg = s.Owner as BSaveGame;

			s.Stream(ref this.ProtoID);
			s.Stream(ref this.BuildPoints);
			sg.StreamBCost(s, ref this.Cost);
			s.Stream(ref this.MaxHP); s.Stream(ref this.MaxSP); s.Stream(ref this.MaxAmmo);
			s.Stream(ref this.Level); s.Stream(ref this.TechLevel); s.Stream(ref this.DisplayNameIndex);
			s.Stream(ref this.CircleMenuIconID); s.Stream(ref this.AltCircleMenuIconID); s.Stream(ref this.HPBar);
			s.Stream(ref this.Available); s.Stream(ref this.Forbid);
			s.Stream(ref this.OneTimeSpawnUsed); s.Stream(ref this.KBAware);
			s.Stream(ref this.HasOverrideNodes);
			if (this.HasOverrideNodes) BSaveGame.StreamArray(s, ref this.OverrideNodes);
			s.StreamSignature(cSaveMarker.ProtoSquad1);
		}
		#endregion
	};
}