
namespace KSoft.Phoenix.Runtime
{
	partial class BDatabase
	{
		public struct ProtoSquadEntry
			: IO.IEndianStreamSerializable
		{
			public string Name;
			public bool FlagObjectProtoSquad;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalString32(ref this.Name);
				s.Stream(ref this.FlagObjectProtoSquad);
			}
			#endregion
		};
	};
}