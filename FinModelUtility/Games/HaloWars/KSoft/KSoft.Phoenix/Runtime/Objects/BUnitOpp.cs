
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
			BSaveGame.StreamVectorArray(s, ref this.Path, cMaximumPathLength);
			s.Stream(this.Target);
			s.Stream(ref this.Source);
			s.Stream(ref this.ID);
			s.Stream(ref this.Type);
			s.Stream(ref this.UserData);
			s.Stream(ref this.Priority);
			s.Stream(ref this.UserData2);
			s.Stream(ref this.WaitCount);
			s.Stream(ref this.Evaluated); s.Stream(ref this.ExistForOneUpdate); s.Stream(ref this.ExistUntilEvaluated);
			s.Stream(ref this.AllowComplete); s.Stream(ref this.NotifySource); s.Stream(ref this.Leash);
			s.Stream(ref this.ForceLeash); s.Stream(ref this.Trigger); s.Stream(ref this.RemoveActions);
			s.Stream(ref this.Complete); s.Stream(ref this.CompleteValue); s.Stream(ref this.PreserveDPS);
			s.Stream(ref this.MustComplete); s.Stream(ref this.UserDataSet);
		}
		#endregion
	};
}
