#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Phoenix.Runtime
{
	using BActionTypeStreamer = IO.EnumBinaryStreamer<Phx.BActionType, byte>;

	partial class cSaveMarker
	{
		public const ushort
			#region 0x1
			cSaveMarkerBEntityActionIdle = 0x2710,
			cSaveMarkerBEntityActionListen = 0x2711,
			cSaveMarkerBUnitActionMove = 0x2712,
			cSaveMarkerBUnitActionMoveAir = 0x2713,

			#endregion

			cSaveMarkerBUnitActionRage = 0x276A
			;
	};

	public struct ActionListEntry
		: IO.IEndianStreamSerializable
	{
		internal static ActionListEntry Invalid = new ActionListEntry();

		public bool Action;
		public Phx.BActionType ActionType;
		public int ActionPtr;

	#region IEndianStreamSerializable Members
	public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.Action);
			if (this.Action)
			{
				s.Stream(ref this.ActionType, BActionTypeStreamer.Instance);
				BSaveGame.StreamFreeListItemPtr(s, ref this.ActionPtr);
			}
			else
			{
				this.ActionType = Phx.BActionType.Invalid;
				this.ActionPtr = TypeExtensions.kNone;
			}
		}
		#endregion
	};

	public sealed class BActionManager
	{
		const int cActionListMaximumCount = 0xC8;

		public static IO.EndianStream StreamActionList(IO.EndianStream s, ref ActionListEntry[] actionList)
		{
			Contract.Requires(s.IsReading || actionList != null);

			bool reading = s.IsReading;

			var count = (byte)(reading ? 0 : actionList.Length);
			s.Stream(ref count);
			if (reading)
			{
				Contract.Assert(count <= cActionListMaximumCount);
				actionList = new ActionListEntry[count];
			}

			for (byte x = 0; x < count; x++)
			{
				var expected_index = x;
				s.Stream(ref expected_index);
				if (reading)
				{
					Contract.Assert(expected_index != cSaveMarker.IteratorEndUInt8);
					Contract.Assert(expected_index == x);
				}

				var t = reading ? ActionListEntry.Invalid : actionList[x];
				t.Serialize(s);
				if (reading)
					actionList[x] = t;
			}

			s.StreamSignature(cSaveMarker.IteratorEndUInt8);

			return s;
		}
	};
}