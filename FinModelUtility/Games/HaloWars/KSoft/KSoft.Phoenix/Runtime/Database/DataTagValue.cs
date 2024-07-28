
namespace KSoft.Phoenix.Runtime
{
	partial class BDatabase
	{
		public struct DataTagValue
			: IO.IEndianStreamSerializable
		{
			public string Name;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalString32(ref this.Name);
			}
			#endregion
		};
		static readonly CondensedListInfo kDataTagsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(int),
			MaxCount=0x3E8,
		};
	};
}