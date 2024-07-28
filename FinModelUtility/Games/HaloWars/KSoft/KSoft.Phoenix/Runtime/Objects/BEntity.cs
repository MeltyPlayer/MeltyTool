
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BEntityRef = System.UInt64; // idk, 8 bytes
using BPlayerID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			Entity1 = 0x2710
			;
	};

	public abstract class BEntity
		: IO.IEndianStreamSerializable
	{
		const int cMaximumEntityRefs = 0x3E8;

		public BVector Position, Up, Forward, Velocity;
		public ActionListEntry[] Actions;
		public BEntityRef[] EntityRefs;
		public BEntityID ID;
		public BPlayerID PlayerID;
		public float YDisplacement,
			ObstructionRadiusX, ObstructionRadiusY, ObstructionRadiusZ;
		#region Flags
		public bool FlagCollidable	// 0x9C, 7
			, FlagMoving			// 0x9C, 5
			, FlagDestroy			// 0x9C, 6
			, FlagFirstUpdate		// 0x9C, 4
			, FlagTiesToGround		// 0x9C, 3
			, FlagUseMaxHeight		// 0x9C, 2
			, FlagPhysicsControl	// 0x9C, 1
			, FlagRotateObstruction	// 0x9C, 0
			, FlagFlying			// 0x9D, 7
			, FlagValid				// 0x9D, 6
			, FlagNonMobile			// 0x9D, 5
			, FlagLockedDown		// 0x9D, 4
			, FlagEntityRefsLocked	// 0x9D, 3
			, FlagFlyingHeightFixup	// 0x9D, 2
			, FlagGarrisoned		// 0x9D, 1
			, FlagPassiveGarrisoned	// 0x9D, 0
			, FlagMoved				// 0x9E, 6
			, FlagTeleported		// 0x9E, 5
			, FlagInSniper			// 0x9E, 4
			, FlagIsBuilt			// 0x9E, 3
			, FlagHasSounds			// 0x9E, 2
			, FlagHitched			// 0x9E, 1
			, FlagSprinting			// 0x9E, 0
			, FlagRecovering		// 0x9E, 7
			, FlagInCover			// 0x9F, 6
			, FlagSelectable		// 0x9F, 5
			, FlagUngarrisonValid	// 0x9F, 4
			, FlagGarrisonValid		// 0x9F, 3
			, FlagIsPhysicsReplacement	// 0x9F, 2
			, FlagIsDoneBuilding	// 0x9F, 1
			;
		#endregion

		public bool HasRefs { get { return this.EntityRefs != null; } }

		#region IEndianStreamSerializable Members
		public virtual void Serialize(IO.EndianStream s)
		{
			s.StreamV(ref this.Position); s.StreamV(ref this.Up); s.StreamV(ref this.Forward); s.StreamV(ref this.Velocity);
			BActionManager.StreamActionList(s, ref this.Actions);

			bool has_refs = s.IsReading ? false : this.HasRefs;
			s.Stream(ref has_refs);
			if (has_refs)
				BSaveGame.StreamArray16(s, ref this.EntityRefs, cMaximumEntityRefs);

			s.Stream(ref this.ID);
			s.Stream(ref this.PlayerID);
			s.Stream(ref this.YDisplacement);
			s.Stream(ref this.ObstructionRadiusX); s.Stream(ref this.ObstructionRadiusY); s.Stream(ref this.ObstructionRadiusZ);

			#region Flags
			s.Stream(ref this.FlagCollidable);		s.Stream(ref this.FlagMoving);			s.Stream(ref this.FlagDestroy);
			s.Stream(ref this.FlagFirstUpdate);		s.Stream(ref this.FlagTiesToGround);		s.Stream(ref this.FlagUseMaxHeight);
			s.Stream(ref this.FlagPhysicsControl);	s.Stream(ref this.FlagRotateObstruction);s.Stream(ref this.FlagFlying);
			s.Stream(ref this.FlagValid);			s.Stream(ref this.FlagNonMobile);		s.Stream(ref this.FlagLockedDown);
			s.Stream(ref this.FlagEntityRefsLocked);	s.Stream(ref this.FlagFlyingHeightFixup);s.Stream(ref this.FlagGarrisoned);
			s.Stream(ref this.FlagPassiveGarrisoned);s.Stream(ref this.FlagMoved);			s.Stream(ref this.FlagTeleported);
			s.Stream(ref this.FlagInSniper);			s.Stream(ref this.FlagIsBuilt);			s.Stream(ref this.FlagHasSounds);
			s.Stream(ref this.FlagHitched);			s.Stream(ref this.FlagSprinting);		s.Stream(ref this.FlagRecovering);
			s.Stream(ref this.FlagInCover);			s.Stream(ref this.FlagSelectable);		s.Stream(ref this.FlagUngarrisonValid);
			s.Stream(ref this.FlagGarrisonValid);	s.Stream(ref this.FlagIsPhysicsReplacement);	s.Stream(ref this.FlagIsDoneBuilding);
			#endregion

			s.StreamSignature(cSaveMarker.Entity1);
		}
		#endregion
	};
}