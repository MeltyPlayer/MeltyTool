
using BVector = System.Numerics.Vector4;
using BObjectiveID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	static partial class cSaveMarker
	{
		public const ushort 
			UIWidget = 0x2710
			;
	};

	sealed class BUIWidget
		: IO.IEndianStreamSerializable
	{
		public struct BReticulePointer
			: IO.IEndianStreamSerializable
		{
			public ulong Unknown0, Unknown8;
			public uint Unknown10;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.Unknown0); s.Stream(ref this.Unknown8); s.Stream(ref this.Unknown10);
			}
			#endregion
		};

		public sealed class BUITalkingHeadControl
			: IO.IEndianStreamSerializable
		{
			public string TalkingHeadText;
			public int ObjectiveID, LastCount;
			public bool ShowBackground, ObjectiveVisible, TalkingHeadVisible, IsShown;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalWideString32(ref this.TalkingHeadText);
				s.Stream(ref this.ObjectiveID); s.Stream(ref this.LastCount);
				s.Stream(ref this.ShowBackground); s.Stream(ref this.ObjectiveVisible); s.Stream(ref this.TalkingHeadVisible); s.Stream(ref this.IsShown);
			}
			#endregion
		};

		public sealed class BUIObjectiveProgressControl
			: IO.IEndianStreamSerializable
		{
			public struct BUILabel
				: IO.IEndianStreamSerializable
			{
				public bool IsShown;
				public string Text;

				#region IEndianStreamSerializable Members
				public void Serialize(IO.EndianStream s)
				{
					s.Stream(ref this.IsShown);
					s.StreamPascalWideString32(ref this.Text);
				}
				#endregion
			};

			public struct BUIObjectiveProgressData
				: IO.IEndianStreamSerializable
			{
				public BObjectiveID ObjectiveID;
				public int LastCount;
				public uint FadeTime;
				public int LabelIndex, MinIncrement;

				#region IEndianStreamSerializable Members
				public void Serialize(IO.EndianStream s)
				{
					s.Stream(ref this.ObjectiveID);
					s.Stream(ref this.LastCount);
					s.Stream(ref this.FadeTime);
					s.Stream(ref this.LabelIndex); s.Stream(ref this.MinIncrement);
				}
				#endregion
			};

			public BUILabel[] ObjectiveLabels = new BUILabel[4];
			public BUIObjectiveProgressData[] Objectives;

			public BUIObjectiveProgressControl()
			{
				for (int x = 0; x < this.ObjectiveLabels.Length; x++)
					this.ObjectiveLabels[x] = new BUILabel();
			}

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				for (int x = 0; x < this.ObjectiveLabels.Length; x++) s.Stream(ref this.ObjectiveLabels[x]);
				BSaveGame.StreamArray(s, ref this.Objectives);
			}
			#endregion
		};

		public int CounterCurrent, CounterMax, TimerID, TimerLabelID;
		public float ElapsedTimerTime;
		public int NumCitizensSaved, NumCitizensNeeded;

		public ushort GarrisonContainerVisible0; public byte GarrisonContainerVisible2;
		public int GarrisonContainerEntities0, GarrisonContainerEntities4, GarrisonContainerEntities8;
		public ushort GarrisonContainerUseEntity0; public byte GarrisonContainerUseEntity2;
		public int GarrisonContainerCounts0, GarrisonContainerCounts4, GarrisonContainerCounts8;

		public uint ReticulePointersVisible0; public byte ReticulePointersVisible4;
		public BReticulePointer ReticulePointerType;
		public BVector[] ReticulePointerArea = new BVector[3];
		public BReticulePointer ReticulePointerEntities, PointerRotation, PointerRotationFloat;
		public BUITalkingHeadControl TalkingHeadControl = new BUITalkingHeadControl();
		public BUIObjectiveProgressControl ObjectiveProgressControl = new BUIObjectiveProgressControl();
		public bool WidgetPanelVisible, TimerVisible, CitizensSavedVisible,
			CounterVisible, TimerShown;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.CounterCurrent); s.Stream(ref this.CounterMax); s.Stream(ref this.TimerID); s.Stream(ref this.TimerLabelID);
			s.Stream(ref this.ElapsedTimerTime);
			s.Stream(ref this.NumCitizensSaved); s.Stream(ref this.NumCitizensNeeded);

			s.Stream(ref this.GarrisonContainerVisible0); s.Stream(ref this.GarrisonContainerVisible2);
			s.Stream(ref this.GarrisonContainerEntities0); s.Stream(ref this.GarrisonContainerEntities4); s.Stream(ref this.GarrisonContainerEntities8);
			s.Stream(ref this.GarrisonContainerUseEntity0); s.Stream(ref this.GarrisonContainerUseEntity2);
			s.Stream(ref this.GarrisonContainerCounts0); s.Stream(ref this.GarrisonContainerCounts4); s.Stream(ref this.GarrisonContainerCounts8);

			s.Stream(ref this.ReticulePointersVisible0); s.Stream(ref this.ReticulePointersVisible4);
			s.Stream(ref this.ReticulePointerType);
			for (int x = 0; x < this.ReticulePointerArea.Length; x++) s.StreamV(ref this.ReticulePointerArea[x]);
			s.Stream(ref this.ReticulePointerEntities);
			s.Stream(ref this.PointerRotation);
			s.Stream(ref this.PointerRotationFloat);
			s.Stream(this.TalkingHeadControl);
			s.Stream(this.ObjectiveProgressControl);
			s.Stream(ref this.WidgetPanelVisible); s.Stream(ref this.TimerVisible); s.Stream(ref this.CitizensSavedVisible);
			s.Stream(ref this.CounterVisible); s.Stream(ref this.TimerShown);
			s.StreamSignature(cSaveMarker.UIWidget);
		}
		#endregion
	};
}