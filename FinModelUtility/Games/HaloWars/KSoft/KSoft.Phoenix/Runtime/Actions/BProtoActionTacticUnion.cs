
using BProtoObjectID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	// lol idk
	struct BProtoActionTacticUnion
		: IO.IEndianStreamSerializable
	{
		public bool ProtoAction;
		public sbyte PlayerID;
		public BProtoObjectID ProtoObjectID;
		public sbyte ProtoActionIndex;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.ProtoAction);
			s.Stream(ref this.PlayerID);
			s.Stream(ref this.ProtoObjectID);
			s.Stream(ref this.ProtoActionIndex);
		}
		#endregion
	};
}