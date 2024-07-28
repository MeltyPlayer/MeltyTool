
namespace KSoft.Phoenix.Runtime
{
	partial class BDatabase
	{
		public struct TemplateEntry
			: IO.IEndianStreamSerializable
		{
			public string Name; // path
			public int ModelIndex;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.StreamPascalString32(ref this.Name);
				s.Stream(ref this.ModelIndex);
			}
			#endregion
		};
	};
}