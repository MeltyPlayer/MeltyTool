#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BProtoObjectID = System.Int32;
using BProtoObjectTrainLimit = System.Int32; // idk, 4 bytes

namespace KSoft.Phoenix.Runtime
{
	partial class cSaveMarker
	{
		public const ushort
			ProtoObject = 0x2710
			;
	};

	sealed class BProtoObject
		: BProtoObjectBase
	{
		const int kTrainLimitsMaxCount = 0x64;

		public BProtoObjectID ProtoID;
		public BProtoObjectTrainLimit[] TrainLimits;
		public BHardpoint[] Hardpoints;

		public int ProtoVisualIndex;
		public float DesiredVelocity, MaxVelocity;
		public float Hitpoints, Shieldpoints;
		public float LOS;
		public int SimLOS;

		public float Bounty;
		public BTactic Tactic;
		public float AmmoMax, AmmoRegenRate, RateAmount;
		public int MaxContained, DisplayNameIndex, CircleMenuIconID;
		public int DeathSpawnSquad;
		public byte[] CommandDisabled; // utbitvector; count=4
		public byte[] CommandSelectable; // utbitvector; count=4
		public bool AbilityDisabled,
			AutoCloak, CloakMove, CloakAttack,
			UniqueInstance;

		public void CommandDisabledNone()
		{
			for (int x = 0; x < this.CommandDisabled.Length; x++)
				this.CommandDisabled[x] = 0;
		}

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			base.Serialize(s);
			var sg = s.Owner as BSaveGame;

			s.Stream(ref this.ProtoID);
			BSaveGame.StreamArray(s, ref this.TrainLimits);
			Contract.Assert(this.TrainLimits.Length <= kTrainLimitsMaxCount);
			BSaveGame.StreamArray(s, ref this.Hardpoints);
			Contract.Assert(this.Hardpoints.Length <= BHardpoint.kMaxCount);
			sg.StreamBCost(s, ref this.Cost);
			s.Stream(ref this.ProtoVisualIndex);
			s.Stream(ref this.DesiredVelocity); s.Stream(ref this.MaxVelocity);
			s.Stream(ref this.Hitpoints); s.Stream(ref this.Shieldpoints);
			s.Stream(ref this.LOS);
			s.Stream(ref this.SimLOS);
			s.Stream(ref this.BuildPoints);
			s.Stream(ref this.Bounty);
			s.StreamNotNull(ref this.Tactic);
			s.Stream(ref this.AmmoMax); s.Stream(ref this.AmmoRegenRate); s.Stream(ref this.RateAmount);
			s.Stream(ref this.MaxContained); s.Stream(ref this.DisplayNameIndex); s.Stream(ref this.CircleMenuIconID);
			s.Stream(ref this.DeathSpawnSquad);
			BSaveGame.StreamArray(s, ref this.CommandDisabled);
			BSaveGame.StreamArray(s, ref this.CommandSelectable);
			s.Stream(ref this.Available); s.Stream(ref this.Forbid); s.Stream(ref this.AbilityDisabled);
			s.Stream(ref this.AutoCloak); s.Stream(ref this.CloakMove); s.Stream(ref this.CloakAttack);
			s.Stream(ref this.UniqueInstance);
			s.StreamSignature(cSaveMarker.ProtoObject);
		}
		#endregion

		#region IIndentedStreamWritable Members
#if false
		public void ToStream(IO.IndentedTextWriter s)
		{
			s.WriteLine("{0} {1}{2}{3}{4}", "CommandDisabled",
				CommandDisabled[0].ToString("X2"), CommandDisabled[1].ToString("X2"),
				CommandDisabled[2].ToString("X2"), CommandDisabled[3].ToString("X2"));
			s.WriteLine("{0} {1}{2}{3}{4}", "CommandSelectable",
				CommandSelectable[0].ToString("X2"), CommandSelectable[1].ToString("X2"),
				CommandSelectable[2].ToString("X2"), CommandSelectable[3].ToString("X2"));
		}
#endif
		#endregion
	};
}