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
			s.StreamV(ref this.Vector);
			BSaveGame.StreamArray16(s, ref this.SquadList);
			Contract.Assert(this.SquadList.Length <= kMaxEntitiesPerList);
			BSaveGame.StreamArray16(s, ref this.UnitList);
			Contract.Assert(this.SquadList.Length <= kMaxEntitiesPerList);
			s.Stream(this.EntityFilterSet);
			s.Stream(ref this.Float);
			s.Stream(ref this.ObjectType);
			s.Stream(ref this.LocStringID);
			s.Stream(ref this.HasVector); s.Stream(ref this.HasSquadList); s.Stream(ref this.HasUnitList);
			s.Stream(ref this.HasEntityFilterSet); s.Stream(ref this.HasFloat); s.Stream(ref this.HasObjectType);
			s.Stream(ref this.HasLocStringID);
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
			s.Stream(ref this.HasCommand); s.Stream(ref this.NewCommand);
			s.Stream(ref this.StateChanged); s.Stream(ref this.NewState);
			s.Stream(ref this.HasPreconditionResult); s.Stream(ref this.PreconditionResult);
			s.Stream(ref this.PreconditionTime);
			s.Stream(ref this.State);
			s.Stream(ref this.GamesReinforced); s.Stream(ref this.TimesReinforced); s.Stream(ref this.HintDisplayedCount);
			BSaveGame.StreamList(s, this.Pages, kPagesListInfo);
			s.Stream(ref this.TimesReinforcedThisGame);
			s.Stream(ref this.EventReady); s.Stream(ref this.Active); s.Stream(ref this.Permission);
			s.Stream(ref this.InitialWaitTimeRemaining); s.Stream(ref this.TerminalWaitTimeRemaining);
			s.Stream(ref this.CoolDownTimer); s.Stream(ref this.LastCoolDownAmount);
			s.Stream(ref this.CoolDownTimerAccumulator);
			BSaveGame.StreamArray(s, ref this.SubHints);
			s.Stream(ref this.ParentHint); Contract.Assert(this.ParentHint <= kMaxCount);
			s.Stream(ref this.PrereqsMet); s.Stream(ref this.DirtyProfile);
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
			BSaveGame.StreamList(s, this.Concepts, kConceptsListInfo);
			s.Stream(ref this.TimeSinceLastHint);
			s.Stream(ref this.HintMessageOn);
			BSaveGame.StreamArray16(s, ref this.AllowedConcepts);
			Contract.Assert(this.AllowedConcepts.Length <= BConcept.kMaxCount);
			s.Stream(ref this.WaitForNextRescore);
			s.Stream(ref this.LastGameTime);
			s.StreamSignature(cSaveMarker.HintEngine);
		}
		#endregion
	};
}
