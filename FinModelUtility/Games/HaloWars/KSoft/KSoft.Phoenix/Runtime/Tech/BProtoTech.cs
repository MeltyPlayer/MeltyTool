
namespace KSoft.Phoenix.Runtime
{
	sealed class BProtoTech
		: BProtoBuildableObject
	{
		public float ResearchPoints { get { return this.BuildPoints; } }

		public bool OwnStaticData, Unobtainable, Unique,
			Shadow, OrPrereqs, Perpetual,
			NoSound, Instant;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			var sg = s.Owner as BSaveGame;

			sg.StreamBCost(s, ref this.Cost);
			s.Stream(ref this.BuildPoints);
			s.Stream(ref this.OwnStaticData); s.Stream(ref this.Unobtainable); s.Stream(ref this.Unique);
			s.Stream(ref this.Shadow); s.Stream(ref this.OrPrereqs); s.Stream(ref this.Perpetual);
			s.Stream(ref this.Forbid);
			s.Stream(ref this.NoSound); s.Stream(ref this.Instant);
		}
		#endregion
	};
}