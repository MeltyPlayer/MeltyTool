
namespace KSoft.Phoenix.Runtime
{
	sealed class BSaveTeam
		: IO.IEndianStreamSerializable
	{
		public int[] Players;
		public byte[] Relations; // BRelationType

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamArray(s, ref this.Players);
			BSaveGame.StreamArray(s, ref this.Relations);
		}
		#endregion
	};
}