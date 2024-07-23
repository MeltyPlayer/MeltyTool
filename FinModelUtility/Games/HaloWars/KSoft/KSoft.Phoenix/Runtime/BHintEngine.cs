using System.Collections.Generic;
#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			HintEngine = 0x2710,
			Concept = 0x2711,
			ParameterPage = 0x2712
			;
	};

	sealed class BParameterPage
		: IO.IEndianStreamSerializable
	{
		public const ushort kDoneIndex = 0x3E9;
		const int kMaxEntitiesPerList = 0x3E8;

		public BVector Vector;
		public BEntityID[] SquadList, UnitList;
		public BEntityFilterSet EntityFilterSet = new BEntityFilterSet();
		public float Float;
		public int ObjectType;
		public uint LocStringID;
		public bool HasVector, HasSquadList, HasUnitList,
			HasEntityFilterSet, HasFloat, HasObjectType,
			HasLocStringID;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamV(ref Vector);
			BSaveGame.StreamArray16(s, ref SquadList);
			Contract.Assert(SquadList.Length <= kMaxEntitiesPerList);
			BSaveGame.StreamArray16(s, ref UnitList);
			Contract.Assert(SquadList.Length <= kMaxEntitiesPerList);
			s.Stream(EntityFilterSet);
			s.Stream(ref Float);
			s.Stream(ref ObjectType);
			s.Stream(ref LocStringID);
			s.Stream(ref HasVector); s.Stream(ref HasSquadList); s.Stream(ref HasUnitList);
			s.Stream(ref HasEntityFilterSet); s.Stream(ref HasFloat); s.Stream(ref HasObjectType);
			s.Stream(ref HasLocStringID);
		}
		#endregion
	};

	sealed class BConcept
		: IO.IEndianStreamSerializable
	{
		public const int kMaxCount = BParameterPage.kDoneIndex-1;

		static readonly CondensedListInfo kPagesListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(short),
			MaxCount=kMaxCount,
			DoneIndex=BParameterPage.kDoneIndex,
		};

		public bool HasCommand;
		public byte NewCommand;
		public bool StateChanged;
		public byte NewState;
		public bool HasPreconditionResult;
		public byte PreconditionResult;
		public uint PreconditionTime;
		public byte State;
		public int GamesReinforced, TimesReinforced, HintDisplayedCount;
		public List<CondensedListItem16<BParameterPage>> Pages = new List<CondensedListItem16<BParameterPage>>();
		public int TimesReinforcedThisGame;
		public bool EventReady, Active, Permission;
		public float InitialWaitTimeRemaining, TerminalWaitTimeRemaining;
		public uint CoolDownTimer, LastCoolDownAmount;
		public float CoolDownTimerAccumulator;
		public int[] SubHints; // ids
		public int ParentHint;
		public bool PrereqsMet, DirtyProfile;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref HasCommand); s.Stream(ref NewCommand);
			s.Stream(ref StateChanged); s.Stream(ref NewState);
			s.Stream(ref HasPreconditionResult); s.Stream(ref PreconditionResult);
			s.Stream(ref PreconditionTime);
			s.Stream(ref State);
			s.Stream(ref GamesReinforced); s.Stream(ref TimesReinforced); s.Stream(ref HintDisplayedCount);
			BSaveGame.StreamList(s, Pages, kPagesListInfo);
			s.Stream(ref TimesReinforcedThisGame);
			s.Stream(ref EventReady); s.Stream(ref Active); s.Stream(ref Permission);
			s.Stream(ref InitialWaitTimeRemaining); s.Stream(ref TerminalWaitTimeRemaining);
			s.Stream(ref CoolDownTimer); s.Stream(ref LastCoolDownAmount);
			s.Stream(ref CoolDownTimerAccumulator);
			BSaveGame.StreamArray(s, ref SubHints);
			s.Stream(ref ParentHint); Contract.Assert(ParentHint <= kMaxCount);
			s.Stream(ref PrereqsMet); s.Stream(ref DirtyProfile);
			s.StreamSignature(cSaveMarker.Concept);
		}
		#endregion
	};

	sealed class BHintEngine
		: IO.IEndianStreamSerializable
	{
		static readonly CondensedListInfo kConceptsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(uint),
			MaxCount=BConcept.kMaxCount,
			DoneIndex=int.MaxValue,
		};

		public List<CondensedListItem32<BConcept>> Concepts { get; private set; } = new List<CondensedListItem32<BConcept>>();
		public float TimeSinceLastHint;
		public bool HintMessageOn;
		public int[] AllowedConcepts;
		public float WaitForNextRescore;
		public uint LastGameTime;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamList(s, Concepts, kConceptsListInfo);
			s.Stream(ref TimeSinceLastHint);
			s.Stream(ref HintMessageOn);
			BSaveGame.StreamArray16(s, ref AllowedConcepts);
			Contract.Assert(AllowedConcepts.Length <= BConcept.kMaxCount);
			s.Stream(ref WaitForNextRescore);
			s.Stream(ref LastGameTime);
			s.StreamSignature(cSaveMarker.HintEngine);
		}
		#endregion
	};
}
