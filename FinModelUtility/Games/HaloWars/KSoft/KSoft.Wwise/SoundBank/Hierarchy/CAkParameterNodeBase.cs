#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Wwise.SoundBank
{
	public class CAkParameterNodeBase
		: IO.IEndianStreamSerializable
	{
		struct FXData
			: IO.IEndianStreamSerializable
		{
			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Pad8(); // FXIndex
				s.Pad32(); // fxID
				s.Pad8(); // IsRendered
				s.Pad32(); // ulSize
			}
			#endregion
		};
		struct AKBKStateItem
			: IO.IEndianStreamSerializable
		{
			/// <summary>Size of this struct on disk</summary>
			internal const uint kSizeOf = 9;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Pad32(); // State
				s.Pad8(); // IsCustom
				s.Pad32(); // ID
			}
			#endregion
		};

		byte NumFx;
		FXData[] FX;

		public uint OverrideBusId, DirectParentID;
		byte Priority;
		bool PriorityOverrideParent, PriorityApplyDistFactor;
		sbyte PriorityDistanceOffset;

		bool PositioningInfoOverrideParent;

		ushort NumStates;

		ushort NumRTPC;

		#region IEndianStreamSerializable Members
		void SerializeFxParams(IO.EndianStream s)
		{
			s.Pad8(); // IsOverrideParentFX
			s.Stream(ref this.NumFx);
			Contract.Assert(this.NumFx <= 4);
			if (s.IsReading)
				this.FX = new FXData[this.NumFx];
			if (this.NumFx > 0) s.Pad8(); // bitsFXBypass
			s.StreamArray(this.FX);
		}
		void SerializeParams(IO.EndianStream s)
		{
			s.Pad32(); // float VolumeMain
			s.Pad32(); // float
			s.Pad32(); // float
			s.Pad32(); // float LFEVolumeMain
			s.Pad32(); // float
			s.Pad32(); // float
			s.Pad32(); // float PitchMain
			s.Pad32(); // float
			s.Pad32(); // float
			s.Pad32(); // float LPFMain
			s.Pad32(); // float
			s.Pad32(); // float
			s.Pad32(); // StateGroupID
		}
		void SerializePositioningParams(IO.EndianStream s)
		{
			s.Stream(ref this.PositioningInfoOverrideParent);
			Contract.Assert(!this.PositioningInfoOverrideParent);
			if (this.PositioningInfoOverrideParent)
			{
				s.Pad32(); // CenterPct
			}
		}
		void SerializeAdvSettingsParams(IO.EndianStream s)
		{
			s.Pad8(); // AkVirtualQueueBehavior VirtualQueueBehavior
			s.Pad8(); // KillNewest
			s.Pad16(); // MaxNumInstance
			s.Pad8(); // AkBelowThresholdBehavior BelowThresholdBehavior
			s.Pad8(); // bool MaxNumInstOverrideParent
			s.Pad8(); // bool VVoicesOptOverrideParent
		}
		void SerializeRTPC(IO.EndianStream s)
		{
			const int k_size_of =
				4 + // FXID
				1 + // FXIndex
				4 + // RTPCID
				4 + // ParamID
				4 + // rtpcCurveID
				1 + // AkCurveScaling Scaling
				2 // Size
				;

			s.Stream(ref this.NumRTPC);
			s.Pad((int)(this.NumRTPC * k_size_of));
		}
		void SerializeFeedbackInfo(IO.EndianStream s)
		{
			if (!(s.Owner as AkSoundBank).HasFeedback) return;

			s.Pad32(); // BusId
			s.Pad32(); // float FeedbackVolume
			s.Pad32(); // float FeedbackModifierMin
			s.Pad32(); // float FeedbackModifierMax
			s.Pad32(); // float FeedbackLPF
			s.Pad32(); // float FeedbackLPFModMin
			s.Pad32(); // float FeedbackLPFModMax
		}
		public void Serialize(IO.EndianStream s)
		{
			this.SerializeFxParams(s); // 0x2...
			{ // 0xC
				s.Stream(ref this.OverrideBusId);
				s.Stream(ref this.DirectParentID);
				s.Stream(ref this.Priority);
				s.Stream(ref this.PriorityOverrideParent);
				s.Stream(ref this.PriorityApplyDistFactor);
				s.Stream(ref this.PriorityDistanceOffset);
			}
			this.SerializeParams(s);            // 0x34
			this.SerializePositioningParams(s); // 0x1...
			this.SerializeAdvSettingsParams(s);    // 0x7
			{                                   // 0x2...
				s.Pad8();                       // StateSyncType
				s.Stream(ref this.NumStates);
				s.Pad((int)(this.NumStates * AKBKStateItem.kSizeOf));
			}
			this.SerializeRTPC(s);      // 0x2...
			this.SerializeFeedbackInfo(s); // // 0x1C
		}
		#endregion
	};
}