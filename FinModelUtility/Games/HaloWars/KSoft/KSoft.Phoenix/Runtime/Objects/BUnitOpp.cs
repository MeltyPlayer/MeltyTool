
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BUnitOppID = System.Int32;
using BUnitOppType = System.Byte;

namespace KSoft.Phoenix.Runtime
{
	sealed class BUnitOpp
		: IO.IEndianStreamSerializable
	{
		const int cMaximumPathLength = 0xC8;

		internal static readonly FreeListInfo kFreeListInfo = new FreeListInfo(cSaveMarker.UnitOpp)
		{
			MaxCount=0x4E20,
		};

		public BVector[] Path;
		public BSimTarget Target { get; private set; } = new BSimTarget();
		public BEntityID Source;
		public BUnitOppID ID;
		public BUnitOppType Type;
		public ushort UserData;
		public byte Priority;
		public byte UserData2;
		public ushort WaitCount;
		public bool Evaluated, ExistForOneUpdate, ExistUntilEvaluated,
			AllowComplete, NotifySource, Leash,
			ForceLeash, Trigger, RemoveActions,
			Complete, CompleteValue, PreserveDPS,
			MustComplete, UserDataSet;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamVectorArray(s, ref Path, cMaximumPathLength);
			s.Stream(Target);
			s.Stream(ref Source);
			s.Stream(ref ID);
			s.Stream(ref Type);
			s.Stream(ref UserData);
			s.Stream(ref Priority);
			s.Stream(ref UserData2);
			s.Stream(ref WaitCount);
			s.Stream(ref Evaluated); s.Stream(ref ExistForOneUpdate); s.Stream(ref ExistUntilEvaluated);
			s.Stream(ref AllowComplete); s.Stream(ref NotifySource); s.Stream(ref Leash);
			s.Stream(ref ForceLeash); s.Stream(ref Trigger); s.Stream(ref RemoveActions);
			s.Stream(ref Complete); s.Stream(ref CompleteValue); s.Stream(ref PreserveDPS);
			s.Stream(ref MustComplete); s.Stream(ref UserDataSet);
		}
		#endregion
	};
}
