
using BVector = System.Numerics.Vector4;
using BCost = System.Single;
using BCueIndex = System.Int32;
using BEntityID = System.Int32;
using BProtoObjectID = System.Int32;
using BObjectTypeID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	sealed class BPowerCovRage
		: BPower
	{
		public sealed class BInterpTable
			: IO.IEndianStreamSerializable
		{
			public float[] Keys;
			public uint[] Values; // Type

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				BSaveGame.StreamArray16(s, ref this.Keys);
				BSaveGame.StreamArray16(s, ref this.Values);
			}
			#endregion
		};

		public sealed class BCameraEffectData
			: IO.IEndianStreamSerializable
		{
			public string Name;
			public BInterpTable ColorTransformRTable = new BInterpTable(), ColorTransformGTable = new BInterpTable(), 
				ColorTransformBTable = new BInterpTable();
			public BInterpTable ColorTransformFactorTable = new BInterpTable(), 
				BlurFactorTable = new BInterpTable(), // same data gets written 3x :s
				FOVTable = new BInterpTable(), ZoomTable = new BInterpTable(), YawTable = new BInterpTable(),
				PitchTable = new BInterpTable();
			public bool RadialBlur, Use3DPosition, ModeCameraEffect,
				UserHoverPointAs3DPosition;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalString32(ref this.Name);
				s.Stream(this.ColorTransformRTable); s.Stream(this.ColorTransformGTable); s.Stream(this.ColorTransformBTable);
				s.Stream(this.ColorTransformFactorTable);
				s.Stream(this.BlurFactorTable); s.Stream(this.BlurFactorTable); s.Stream(this.BlurFactorTable); // yes, 3x
				s.Stream(this.FOVTable); s.Stream(this.ZoomTable); s.Stream(this.YawTable);
				s.Stream(this.PitchTable);
				s.Stream(ref this.RadialBlur); s.Stream(ref this.Use3DPosition); s.Stream(ref this.ModeCameraEffect);
				s.Stream(ref this.UserHoverPointAs3DPosition);
			}
			#endregion
		};

		public double NextTickTime;
		public BEntityID TargettedSquad;
		public BVector LastDirectionInput, TeleportDestination, PositionInput;
		public float TimeUntilTeleport, TimeUntilRetarget;
		public BCueIndex AttackSound;
		public BParametricSplineCurve JumpSplineCurve = new BParametricSplineCurve();
		public BCameraEffectData CameraEffectData = new BCameraEffectData();
		public BCost[] CostPerTick, CostPerTickAttacking, CostPerJump;
		public float TickLength, DamageMultiplier, DamageTakenMultiplier, 
			SpeedMultiplier, NudgeMultiplier, ScanRadius;
		public BProtoObjectID ProjectileObject, HandAttachObject, TeleportAttachObject;
		public float AudioReactionTimer, TeleportTime,
			TeleportLateralDistance, TeleportJumpDistance, TimeBetweenRetarget, 
			MotionBlurAmount, MotionBlurDistance, MotionBlurTime,
			DistanceVsAngleWeight, HealPerKillCombatValue, AuraRadius, AuraDamageBonus;
		public BProtoObjectID AuraAttachObjectSmall, AuraAttachObjectMedium, AuraAttachObjectLarge, 
			HealAttachObject;
		public BEntityID[] SquadsInAura;
		public BObjectTypeID FilterTypeID;
		public bool CompletedInitialization, HasSuccessfullyAttacked, UsePather;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);
			var sg = s.Owner as BSaveGame;

			s.Stream(ref this.NextTickTime);
			s.Stream(ref this.TargettedSquad);
			s.StreamV(ref this.LastDirectionInput); s.StreamV(ref this.TeleportDestination); s.StreamV(ref this.PositionInput);
			s.Stream(ref this.TimeUntilTeleport); s.Stream(ref this.TimeUntilRetarget);
			s.Stream(ref this.AttackSound);
			s.Stream(this.JumpSplineCurve);
			s.Stream(this.CameraEffectData);
			sg.StreamBCost(s, ref this.CostPerTick); sg.StreamBCost(s, ref this.CostPerTickAttacking); sg.StreamBCost(s, ref this.CostPerJump);
			s.Stream(ref this.TickLength); s.Stream(ref this.DamageMultiplier); s.Stream(ref this.DamageTakenMultiplier);
			s.Stream(ref this.SpeedMultiplier); s.Stream(ref this.NudgeMultiplier); s.Stream(ref this.ScanRadius);
			s.Stream(ref this.ProjectileObject); s.Stream(ref this.HandAttachObject); s.Stream(ref this.TeleportAttachObject);
			s.Stream(ref this.AudioReactionTimer); s.Stream(ref this.TeleportTime);
			s.Stream(ref this.TeleportLateralDistance); s.Stream(ref this.TeleportJumpDistance); s.Stream(ref this.TimeBetweenRetarget);
			s.Stream(ref this.MotionBlurAmount); s.Stream(ref this.MotionBlurDistance); s.Stream(ref this.MotionBlurTime);
			s.Stream(ref this.DistanceVsAngleWeight); s.Stream(ref this.HealPerKillCombatValue); s.Stream(ref this.AuraRadius); s.Stream(ref this.AuraDamageBonus);
			s.Stream(ref this.AuraAttachObjectSmall); s.Stream(ref this.AuraAttachObjectMedium); s.Stream(ref this.AuraAttachObjectLarge);
			s.Stream(ref this.HealAttachObject);
			BSaveGame.StreamArray(s, ref this.SquadsInAura);
			s.Stream(ref this.FilterTypeID);
			s.Stream(ref this.CompletedInitialization); s.Stream(ref this.HasSuccessfullyAttacked); s.Stream(ref this.UsePather);
		}
		#endregion
	};
}