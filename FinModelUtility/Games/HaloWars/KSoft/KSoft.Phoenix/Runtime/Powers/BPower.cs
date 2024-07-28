
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BPowerUserID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	abstract class BPower
		: IO.IEndianStreamSerializable
	{
		public int ID;
		public uint Type;
		public BPowerUserID PowerUserID;
		public sbyte ProtoPowerID, PowerLevel;
		public float MaintenanceSupplies;
		public double Elapsed;
		public sbyte PlayerID;
		public BEntityID OwnerID;
		public BVector TargetLocation;
		public bool Destroy, IgnoreAllReqs, CheckPowerLocation;

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.ID);
			s.Stream(ref this.Type);
			s.Stream(ref this.PowerUserID);
			s.Stream(ref this.ProtoPowerID); s.Stream(ref this.PowerLevel);
			s.Stream(ref this.MaintenanceSupplies);
			s.Stream(ref this.Elapsed);
			s.Stream(ref this.PlayerID);
			s.Stream(ref this.OwnerID);
			s.StreamV(ref this.TargetLocation);
			s.Stream(ref this.Destroy); s.Stream(ref this.IgnoreAllReqs); s.Stream(ref this.CheckPowerLocation);
		}
		#endregion

		internal static BPower FromType(Phx.BPowerType type)
		{
			switch (type)
			{
			case Phx.BPowerType.Cleansing: return new BPowerCovGlassing();
			case Phx.BPowerType.Orbital: return new BPowerUnscMac();
			case Phx.BPowerType.CarpetBombing: return new BPowerUnscCarpetBomb();
			case Phx.BPowerType.Cryo: return new BPowerUnscCryo();
			case Phx.BPowerType.Rage: return new BPowerCovRage();
			case Phx.BPowerType.Wave: return new BPowerCovDebris();
			case Phx.BPowerType.Disruption: return new BPowerUnscDisruption();
			case Phx.BPowerType.Transport: return new BPowerTransport();
			case Phx.BPowerType.ODST: return new BPowerUnscOdst();
			case Phx.BPowerType.Repair: return new BPowerUnscHeal();

			default: throw new KSoft.Debug.UnreachableException(type.ToString());
			}
		}
	};
}