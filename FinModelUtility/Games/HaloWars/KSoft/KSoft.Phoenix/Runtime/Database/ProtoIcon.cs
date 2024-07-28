
namespace KSoft.Phoenix.Runtime
{
	partial class BDatabase
	{
		public struct ProtoIcon
			: IO.IEndianStreamSerializable
		{
			public string Type, Name;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalString32(ref this.Type);
				s.StreamPascalString32(ref this.Name);
			}
			#endregion
		};
	};
}