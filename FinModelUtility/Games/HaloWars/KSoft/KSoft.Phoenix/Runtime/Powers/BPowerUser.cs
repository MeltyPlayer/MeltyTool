
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	abstract class BPowerUser : IO.IEndianStreamSerializable
	{
		public uint Value;
		public uint Type;
		public bool Initialized, Destroy, NoCost, CheckPowerLocation;
		public sbyte ProtoPowerID, PowerLevel;
		public BEntityID OwnerSquadID;
		public bool UsedByPrimaryUser;
		public double Elapsed;

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.Value);
			s.Stream(ref this.Type);
			s.Stream(ref this.Initialized); s.Stream(ref this.Destroy); s.Stream(ref this.NoCost); s.Stream(ref this.CheckPowerLocation);
			s.Stream(ref this.ProtoPowerID); s.Stream(ref this.PowerLevel);
			s.Stream(ref this.OwnerSquadID);
			s.Stream(ref this.UsedByPrimaryUser);
			s.Stream(ref this.Elapsed);
		}
		#endregion

		internal static BPowerUser FromType(Phx.BPowerType type)
		{
			switch (type)
			{
			case Phx.BPowerType.Cleansing: return new BPowerCovGlassingUser();
			case Phx.BPowerType.Orbital: return new BPowerUnscMacUser();
			case Phx.BPowerType.CarpetBombing: return new BPowerUnscCarpetBombUser();
			case Phx.BPowerType.Cryo: return new BPowerUnscCryoUser();
			case Phx.BPowerType.Rage: return new BPowerCovRageUser();
			case Phx.BPowerType.Wave: return new BPowerCovDebrisUser();
			case Phx.BPowerType.Disruption: return new BPowerUnscDisruptionUser();
			case Phx.BPowerType.Transport: return new BPowerTransportUser();
			case Phx.BPowerType.ODST: return new BPowerUnscOdstUser();
			case Phx.BPowerType.Repair: return new BPowerUnscHealUser();

			default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
	};
}