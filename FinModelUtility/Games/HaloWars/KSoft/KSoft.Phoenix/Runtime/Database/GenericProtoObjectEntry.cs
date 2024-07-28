
namespace KSoft.Phoenix.Runtime
{
	partial class BDatabase
	{
		public struct GenericProtoObjectEntry
			: IO.IEndianStreamSerializable
		{
			public string Name;
			public int Id;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalString32(ref this.Name);
				s.Stream(ref this.Id);
			}
			#endregion
		};
	};
}