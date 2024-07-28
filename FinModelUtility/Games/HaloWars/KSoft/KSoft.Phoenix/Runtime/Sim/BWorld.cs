using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			World1 = 0x2710, World2 = 0x2711, World3 = 0x2712, World4 = 0x2713, World5 = 0x2714,
			Players = 0x2715,
			Teams = 0x2716,
			SimOrder = 0x2717,
			UnitOpp = 0x2718,
			PathMoveData = 0x2719,
			Platoons = 0x271A,
			Dopples = 0x271B,
			Projectiles = 0x271C,
			AirSpots = 0x271D,
			Armies = 0x271E,
			Squads = 0x271F,

			Units = 0x2720,
			ObjectiveManager = 0x2721,
			GeneralEvents = 0x2722,
			Triggers = 0x2723,
			Visibilty = 0x2724,
			ScoreManager = 0x2725,
			StoredAnimEventManager = 0x2726,
			EntityScheduler = 0x2727,
			CollectiblesManager = 0x2728,
			Objects = 0x2729
			;

		public const ushort ObjectAnimEventTagQueue_DoneIndex = 0x9C5;

		public const byte BActionController__cNumberControllers = 2;
	};

	sealed class BWorld
		: IO.IEndianStreamSerializable
	{
		public const byte cMaximumSupportedPlayers = 9,
			cMaxPlayerColorCategories = 2,
			cMaximumSupportedTeams = 5;

		public sealed class ObjectGroup
			: IO.IEndianStreamSerializable
		{
			public short Id;

			public int[] Objects; // not sure if BProtoObjectID, etc
			public int[] TriggeredTeams;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.Id);
				BSaveGame.StreamArray(s, ref this.Objects);
				BSaveGame.StreamArray(s, ref this.TriggeredTeams);
			}
			#endregion
		};

		public struct BExplorationGroupTimerEntry
			: IO.IEndianStreamSerializable
		{
			public uint Unknown0, Unknown4, Unknown8;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.Unknown0); s.Stream(ref this.Unknown4); s.Stream(ref this.Unknown8);
			}
			#endregion
		};

		public struct PlayerColorCategory
			: IO.IEndianStreamSerializable
		{
			public uint Objects, Corpse, Selection, 
				Minimap, UI;
			public byte Index;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.Objects); s.Stream(ref this.Corpse); s.Stream(ref this.Selection);
				s.Stream(ref this.Minimap); s.Stream(ref this.UI);
				s.Stream(ref this.Index);
			}
			#endregion
		};

		ObjectGroup[] NumExplorationGroups;
		BExplorationGroupTimerEntry[] ActiveExplorationGroups;
		public BPlayer[] Players;
		PlayerColorCategory[,] PlayerColorCategories = new PlayerColorCategory[cMaxPlayerColorCategories, cMaximumSupportedPlayers];
		List<CondensedListItem16<BSimOrder>> SimOrders = new List<CondensedListItem16<BSimOrder>>();
		List<CondensedListItem16<BUnitOpp>> UnitOpps = new List<CondensedListItem16<BUnitOpp>>();
		List<CondensedListItem16<BPathMoveData>> PathMoveData = new List<CondensedListItem16<BPathMoveData>>();

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			var sg = s.Owner as BSaveGame;

			if (s.IsReading)
			{
				this.Players = new BPlayer[sg.Players.Count];
				for(int x = 0; x < this.Players.Length; x++)
					this.Players[x] = new BPlayer();
			}

			BSaveGame.StreamArray16(s, ref this.NumExplorationGroups, isIterated:true);
			BSaveGame.StreamArray(s, ref this.ActiveExplorationGroups);
			s.StreamSignature(cSaveMarker.World1);
			foreach (var player in this.Players)
				s.Stream(player);
			s.StreamSignature(cSaveMarker.Players);
			s.StreamSignature(cMaximumSupportedPlayers);
			s.StreamSignature(cMaxPlayerColorCategories);
 			for (int x = 0; x < cMaxPlayerColorCategories; x++)
 				for (int y = 0; y < cMaximumSupportedPlayers; y++)
 					s.Stream(ref this.PlayerColorCategories[x, y]);
			s.StreamSignature(cSaveMarker.World2);
			BSaveGame.StreamFreeList(s, this.SimOrders, BSimOrder.kFreeListInfo);
			BSaveGame.StreamFreeList(s, this.UnitOpps, BUnitOpp.kFreeListInfo);
			BSaveGame.StreamFreeList(s, this.PathMoveData, BPathMoveData.kFreeListInfo);

			//...
		}
		#endregion
	};
}