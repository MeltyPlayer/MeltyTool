
namespace KSoft.Phoenix.Runtime
{
	abstract class BProtoObjectBase
		: BProtoBuildableObject
	{
		public short BaseType;

		public bool Available;

		#region IEndianStreamSerializable Members
		public override void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.BaseType);
		}
		#endregion
	};
}