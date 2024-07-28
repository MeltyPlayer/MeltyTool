#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BVector = System.Numerics.Vector4;
using BBitVector32 = System.UInt32;
using BPlayerID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			Object1 = 0x2710
			;
	};

	public struct BObjectAnimationState
		: IO.IEndianStreamSerializable
	{
		public short AnimType;
		public short TweenToAnimation;
		public float TweenTime, MoveSpeed;
		public int ForceAnimID;
		public sbyte State;
		public sbyte ExitAction;
		public bool
			FlagDirty, FlagMoving, FlagTurning,
			FlagReset, FlagApplyInstantly, FlagLock,
			FlagOverrideExitAction
			;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.AnimType);
			s.Stream(ref this.TweenToAnimation);
			s.Stream(ref this.TweenTime); s.Stream(ref this.MoveSpeed);
			s.Stream(ref this.ForceAnimID);
			s.Stream(ref this.State);
			s.Stream(ref this.ExitAction);
			s.Stream(ref this.FlagDirty);	s.Stream(ref this.FlagMoving);			s.Stream(ref this.FlagTurning);
			s.Stream(ref this.FlagReset);	s.Stream(ref this.FlagApplyInstantly);	s.Stream(ref this.FlagLock);
			s.Stream(ref this.FlagOverrideExitAction);
		}
		#endregion
	};

	public sealed class BObject
		: BEntity
	{
		const int kUVOffsetsSize = 0x18;
		const int cMaximumObjectAttachments = 0xFA;
		const int cMaximumAdditionalTextures = 0xFA;
		const int cMaximumHardpoints = 0x64;

		public BVector CenterOffset, IconColorSize;
		public byte[] UVOffsets = new byte[kUVOffsetsSize];
		public uint MultiframeTextureIndex;
		public int VisualVariationIndex;
		public BVisual Visual;
		public float AnimationRate, Radius, MoveAnimationPosition, HighlightIntensity;
		public uint SubUpdateNumber;
		public BBitVector32 PlayerVisibility, DoppleBits;
		public int SimX, SimZ;
		public float LOSScalar;
		public int LastSimLOS;
		public BObjectAttachments[] ObjectAttachments;
		public BAdditionalTextures[] AdditionalTextures;
		public BHardpointState[] HardpointState;
		public BObjectAnimationState AnimationState;
		public uint AnimationLockEnds;
		public int ProtoID;
		public BPlayerID ColorPlayerID;
		public uint OverrideTint,
			OverrideFlashInterval, OverrideFlashIntervalTimer, OverrideFlashDuration,
			LifespanExpiration;
		public float CurrentAlphaTime, AlphaFadeDuration,
			SelectionPulseTime, SelectionPulsePercent, SelectionFlashTime, SelectionPulseSpeed,
			LastRealtime;
		public byte AOTintValue, TeamSelectionMask;
		public float LOSRevealTime;

		#region Flags
		public bool
			FlagVisibility, FlagLOS, FlagHasLifespan, FlagDopples,
			FlagIsFading, FlagAnimationDisabled, FlagIsRevealer, FlagDontInterpolate,
			FlagBlockLOS, FlagCloaked, FlagCloakDetected, FlagGrayMapDopples,
			FlagLOSMarked, FlagUseLOSScalar, FlagLOSDirty, FlagAnimationLocked,
			FlagUpdateSquadPositionOnAnimationUnlock, FlagExistSoundPlaying, FlagNoUpdate, FlagSensorLockTagged,
			FlagNoReveal, FlagBuilt, FlagBeingCaptured, FlagInvulnerable,
			FlagVisibleForOwnerOnly, FlagVisibleToAll, FlagNearLayer, FlagIKDisabled,
			FlagHasTrackMask, FlagLODFading, FlagOccluded, FlagFadeOnDeath,
			FlagObscurable, FlagNoRender, FlagTurning, FlagAppearsBelowDecals,
			FlagSkipMotionExtraction, FlagOverrideTint, FlagMotionCollisionChecked,
			FlagIsDopple, FlagIsImpactEffect, FlagDebugRenderAreaAttackRange, FlagDontLockMovementAnimation,
			FlagRemainVisible, FlagVisibleForTeamOnly, FlagDontAutoAttackMe, FlagAlwaysAttackReviveUnits,
			FlagNoRenderForOwner, FlagNoRenderDuringCinematic, FlagUseCenterOffset, FlagNotDoppleFriendly,
			FlagForceVisibilityUpdateNextFrame, FlagTurningRight, FlagIsUnderCinematicControl, FlagNoWorldUpdate;
		#endregion

		public bool IsObstruction; // "mpObstructionNode != NULL"

		public bool HasObjectAttachments { get { return this.ObjectAttachments != null; } }
		public bool HasAdditionalTextures { get { return this.AdditionalTextures != null; } }

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);

			s.StreamV(ref this.CenterOffset); s.StreamV(ref this.IconColorSize);
			s.Stream(this.UVOffsets);
			s.Stream(ref this.MultiframeTextureIndex);
			s.Stream(ref this.VisualVariationIndex);
			BVisualManager.Stream(s, ref this.Visual);
			s.Stream(ref this.AnimationRate); s.Stream(ref this.Radius); s.Stream(ref this.MoveAnimationPosition); s.Stream(ref this.HighlightIntensity);
			s.Stream(ref this.SubUpdateNumber);
			s.Stream(ref this.PlayerVisibility); s.Stream(ref this.DoppleBits);
			s.Stream(ref this.SimX); s.Stream(ref this.SimZ);
			s.Stream(ref this.LOSScalar);
			s.Stream(ref this.LastSimLOS);

			if (s.StreamCond(this, me => me.HasObjectAttachments))
				BSaveGame.StreamArray(s, ref this.ObjectAttachments, cMaximumObjectAttachments);

			if (s.StreamCond(this, me => me.HasAdditionalTextures))
				BSaveGame.StreamArray(s, ref this.AdditionalTextures, cMaximumAdditionalTextures);

			BSaveGame.StreamArray(s, ref this.HardpointState, cMaximumHardpoints);
			s.Stream(ref this.AnimationState);
			s.Stream(ref this.AnimationLockEnds);
			s.Stream(ref this.ProtoID);
			s.Stream(ref this.ColorPlayerID);
			s.Stream(ref this.OverrideTint);
			s.Stream(ref this.OverrideFlashInterval); s.Stream(ref this.OverrideFlashIntervalTimer); s.Stream(ref this.OverrideFlashDuration);
			s.Stream(ref this.LifespanExpiration);
			s.Stream(ref this.CurrentAlphaTime); s.Stream(ref this.AlphaFadeDuration);
			s.Stream(ref this.SelectionPulseTime); s.Stream(ref this.SelectionPulsePercent); s.Stream(ref this.SelectionFlashTime); s.Stream(ref this.SelectionPulseSpeed);
			s.Stream(ref this.LastRealtime);
			s.Stream(ref this.AOTintValue);
			s.Stream(ref this.TeamSelectionMask);
			s.Stream(ref this.LOSRevealTime);

			#region Flags
			s.Stream(ref this.FlagVisibility); s.Stream(ref this.FlagLOS); s.Stream(ref this.FlagHasLifespan); s.Stream(ref this.FlagDopples);
			s.Stream(ref this.FlagIsFading); s.Stream(ref this.FlagAnimationDisabled); s.Stream(ref this.FlagIsRevealer); s.Stream(ref this.FlagDontInterpolate);
			s.Stream(ref this.FlagBlockLOS); s.Stream(ref this.FlagCloaked); s.Stream(ref this.FlagCloakDetected); s.Stream(ref this.FlagGrayMapDopples);
			s.Stream(ref this.FlagLOSMarked); s.Stream(ref this.FlagUseLOSScalar); s.Stream(ref this.FlagLOSDirty); s.Stream(ref this.FlagAnimationLocked);
			s.Stream(ref this.FlagUpdateSquadPositionOnAnimationUnlock); s.Stream(ref this.FlagExistSoundPlaying); s.Stream(ref this.FlagNoUpdate); s.Stream(ref this.FlagSensorLockTagged);
			s.Stream(ref this.FlagNoReveal); s.Stream(ref this.FlagBuilt); s.Stream(ref this.FlagBeingCaptured); s.Stream(ref this.FlagInvulnerable);
			s.Stream(ref this.FlagVisibleForOwnerOnly); s.Stream(ref this.FlagVisibleToAll); s.Stream(ref this.FlagNearLayer); s.Stream(ref this.FlagIKDisabled);
			s.Stream(ref this.FlagHasTrackMask); s.Stream(ref this.FlagLODFading); s.Stream(ref this.FlagOccluded); s.Stream(ref this.FlagFadeOnDeath);
			s.Stream(ref this.FlagObscurable); s.Stream(ref this.FlagNoRender); s.Stream(ref this.FlagTurning); s.Stream(ref this.FlagAppearsBelowDecals);
			s.Stream(ref this.FlagSkipMotionExtraction); s.Stream(ref this.FlagOverrideTint); s.Stream(ref this.FlagMotionCollisionChecked); s.Stream(ref this.FlagIsDopple);
			s.Stream(ref this.FlagIsImpactEffect); s.Stream(ref this.FlagDebugRenderAreaAttackRange); s.Stream(ref this.FlagDontLockMovementAnimation); s.Stream(ref this.FlagRemainVisible);
			s.Stream(ref this.FlagVisibleForTeamOnly); s.Stream(ref this.FlagDontAutoAttackMe); s.Stream(ref this.FlagAlwaysAttackReviveUnits); s.Stream(ref this.FlagNoRenderForOwner);
			s.Stream(ref this.FlagNoRenderDuringCinematic); s.Stream(ref this.FlagUseCenterOffset); s.Stream(ref this.FlagNotDoppleFriendly); s.Stream(ref this.FlagForceVisibilityUpdateNextFrame);
			s.Stream(ref this.FlagTurningRight); s.Stream(ref this.FlagIsUnderCinematicControl); s.Stream(ref this.FlagNoWorldUpdate);
			#endregion

			s.Stream(ref this.IsObstruction);

			Contract.Assert(false);// mpPhysicsObject

			s.StreamSignature(cSaveMarker.Object1);
		}
		#endregion
	};
}