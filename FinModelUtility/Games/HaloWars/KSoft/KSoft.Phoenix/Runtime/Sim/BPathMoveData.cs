
using BPathLevel = System.Byte;

namespace KSoft.Phoenix.Runtime
{
	public sealed class BPathMoveData
		: IO.IEndianStreamSerializable
	{
		internal static readonly FreeListInfo kFreeListInfo = new FreeListInfo(cSaveMarker.PathMoveData)
		{
			MaxCount = 0x4E20,
		};

		public BPath Path { get; private set; } = new BPath();
		public int CurrentWaypoint;
		public uint PathTime;
		public int LinkedPath = TypeExtensions.kNone;
		public BPathLevel PathLevel;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(this.Path);
			s.Stream(ref this.CurrentWaypoint);
			s.Stream(ref this.PathTime);
			BSaveGame.StreamFreeListItemPtr(s, ref this.LinkedPath);
			s.Stream(ref this.PathLevel);
		}
		#endregion
	};
}
