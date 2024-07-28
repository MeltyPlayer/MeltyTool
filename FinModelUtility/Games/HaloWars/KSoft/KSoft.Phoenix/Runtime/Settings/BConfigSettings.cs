
namespace KSoft.Phoenix.Runtime
{
	sealed class BConfigSettings
		: IO.IEndianStreamSerializable
	{
		const uint kVersion = 0;

		public bool 
			NoFogMask,			AIDisable,			AIShadow,			NoVismap,					NoRandomPlayerPlacement, 
			DisableOneBuilding,	BuildingQueue,		UseTestLeaders,		EnableFlight,				NoBirthAnims, 
			Veterancy,			TrueLOS,			NoDestruction,		CoopSharedResources,		MaxProjectileHeightForDecal,
			EnableSubbreakage,	EnableThrowPart,	AllowAnimIsDirty,	NoVictoryCondition,			AIAutoDifficulty,
			Demo,				AsyncWorldUpdate,	EnableHintSystem,	PercentFadeTimeCorpseSink,	CorpseSinkSpeed,
			CorpseMinScale,		BlockOutsideBounds,	AINoAttack,			PassThroughOwnVehicles,		EnableCapturePointResourceSharing,
			NoUpdatePathingQuad,SlaveUnitPosition,	Turning,			HumanAttackMove,			MoreNewMovement3,
			OverrideGroundIK,	DriveWarthog,		EnableCorpses,		DisablePathingLimits,		DisableVelocityMatchingBySquadType,
			ActiveAbilities,	AlphaTest,			NoDamage,			IgnoreAllPlatoonmates,		ClassicPlatoonGrouping,
			NoShieldDamage,		EnableSubUpdating,	MPSubUpdating,		AlternateSubUpdating,		DynamicSubUpdateTime,
			DecoupledUpdate;

		public float
			PlatoonRadius,	ProjectionTime,	OverrideGroundIKRange,	OverrideGroundIKTiltFactor,	GameSpeed;

		#region IEndianStreamSerializable Members
		void Read(IO.EndianReader s, out float value)
		{
			if (s.ReadBoolean())
				value = s.ReadSingle();
			else
				value = 0f;
		}
		void Write(IO.EndianWriter s, float value)
		{
			bool not_zero = value != 0f;
			s.Write(not_zero);

			if (not_zero) s.Write(value);
		}
		void ReadFloats(IO.EndianReader s)
		{
			this.Read(s, out this.PlatoonRadius);
			this.Read(s, out this.ProjectionTime);
			this.Read(s, out this.OverrideGroundIKRange);
			this.Read(s, out this.OverrideGroundIKTiltFactor);
			this.Read(s, out this.GameSpeed);
		}
		void WriteFloats(IO.EndianWriter s)
		{
			this.Write(s, this.PlatoonRadius);
			this.Write(s, this.ProjectionTime);
			this.Write(s, this.OverrideGroundIKRange);
			this.Write(s, this.OverrideGroundIKTiltFactor);
			this.Write(s, this.GameSpeed);
		}

		public void Serialize(IO.EndianStream s)
		{
			s.StreamVersion(kVersion);

			s.Stream(ref this.NoFogMask);			s.Stream(ref this.AIDisable);			s.Stream(ref this.AIShadow);			s.Stream(ref this.NoVismap);					s.Stream(ref this.NoRandomPlayerPlacement);
			s.Stream(ref this.DisableOneBuilding);	s.Stream(ref this.BuildingQueue);		s.Stream(ref this.UseTestLeaders);	s.Stream(ref this.EnableFlight);				s.Stream(ref this.NoBirthAnims);
			s.Stream(ref this.Veterancy);			s.Stream(ref this.TrueLOS);				s.Stream(ref this.NoDestruction);	s.Stream(ref this.CoopSharedResources);		s.Stream(ref this.MaxProjectileHeightForDecal);
			s.Stream(ref this.EnableSubbreakage);	s.Stream(ref this.EnableThrowPart);		s.Stream(ref this.AllowAnimIsDirty);	s.Stream(ref this.NoVictoryCondition);		s.Stream(ref this.AIAutoDifficulty);
			s.Stream(ref this.Demo);					s.Stream(ref this.AsyncWorldUpdate);		s.Stream(ref this.EnableHintSystem);	s.Stream(ref this.PercentFadeTimeCorpseSink);s.Stream(ref this.CorpseSinkSpeed);
			s.Stream(ref this.CorpseMinScale);		s.Stream(ref this.BlockOutsideBounds);	s.Stream(ref this.AINoAttack);		s.Stream(ref this.PassThroughOwnVehicles);	s.Stream(ref this.EnableCapturePointResourceSharing);
			s.Stream(ref this.NoUpdatePathingQuad);	s.Stream(ref this.SlaveUnitPosition);	s.Stream(ref this.Turning);			s.Stream(ref this.HumanAttackMove);			s.Stream(ref this.MoreNewMovement3);
			s.Stream(ref this.OverrideGroundIK);		s.Stream(ref this.DriveWarthog);			s.Stream(ref this.EnableCorpses);	s.Stream(ref this.DisablePathingLimits);		s.Stream(ref this.DisableVelocityMatchingBySquadType);
			s.Stream(ref this.ActiveAbilities);		s.Stream(ref this.AlphaTest);			s.Stream(ref this.NoDamage);			s.Stream(ref this.IgnoreAllPlatoonmates);	s.Stream(ref this.ClassicPlatoonGrouping);
			s.Stream(ref this.NoShieldDamage);		s.Stream(ref this.EnableSubUpdating);	s.Stream(ref this.MPSubUpdating);	s.Stream(ref this.AlternateSubUpdating);		s.Stream(ref this.DynamicSubUpdateTime);
			s.Stream(ref this.DecoupledUpdate);

			s.StreamMethods(this.ReadFloats, this.WriteFloats);
		}
		#endregion
	};
}