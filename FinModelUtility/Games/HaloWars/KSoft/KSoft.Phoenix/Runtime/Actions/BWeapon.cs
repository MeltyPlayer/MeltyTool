
namespace KSoft.Phoenix.Runtime
{
	// WTF: Some player's cov_bldg_turret_01's ProtoAction's DamagePerAttack
	// and some other object's (same player) Weapon's DamagePerSecond where
	// being read and somehow getting at least .125 to .25 added to them 
	// So I'm reading them as raw instead

	sealed class BWeapon
		: IO.IEndianStreamSerializable
	{
		public const int kMaxCount = 0xC8;

		public /*float*/uint DamagePerSecond;
		public float DOTrate, DOTduration, 
			MaxRange, MinRange, AOERadius, 
			AOEPrimaryTargetFactor, AOEDistanceFactor, AOEDamageFactor, 
			Accuracy, MovingAccuracy, MaxDeviation,
			MovingMaxDeviation, AccuracyDistanceFactor, AccuracyDeviationFactor,
			MaxVelocityLead, MaxDamagePerRam, ReflectDamageFactor, 
			AirBurstSpan;
		public int ProjectileObjectID, ImpactEffectProtoID;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.DamagePerSecond); s.Stream(ref this.DOTrate); s.Stream(ref this.DOTduration); 
			s.Stream(ref this.MaxRange); s.Stream(ref this.MinRange); s.Stream(ref this.AOERadius); 
			s.Stream(ref this.AOEPrimaryTargetFactor); s.Stream(ref this.AOEDistanceFactor); s.Stream(ref this.AOEDamageFactor); 
			s.Stream(ref this.Accuracy); s.Stream(ref this.MovingAccuracy); s.Stream(ref this.MaxDeviation);
			s.Stream(ref this.MovingMaxDeviation); s.Stream(ref this.AccuracyDistanceFactor); s.Stream(ref this.AccuracyDeviationFactor);
			s.Stream(ref this.MaxVelocityLead); s.Stream(ref this.MaxDamagePerRam); s.Stream(ref this.ReflectDamageFactor); 
			s.Stream(ref this.AirBurstSpan);
			s.Stream(ref this.ProjectileObjectID); s.Stream(ref this.ImpactEffectProtoID);
			s.StreamSignature(cSaveMarker.Weapon);
		}
		#endregion
	};
}