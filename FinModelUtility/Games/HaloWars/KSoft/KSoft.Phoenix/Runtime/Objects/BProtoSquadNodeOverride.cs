
namespace KSoft.Phoenix.Runtime
{
	struct BProtoSquadNodeOverride
		: IO.IEndianStreamSerializable
	{
		public int BaseNodeUnitType, NodeUnitType;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.BaseNodeUnitType);
			s.Stream(ref this.NodeUnitType);
		}
		#endregion
	};
}