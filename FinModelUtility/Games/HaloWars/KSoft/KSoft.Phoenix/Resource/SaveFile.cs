
namespace KSoft.Phoenix.Resource
{
	public sealed class SaveFile
		: IO.IEndianStreamSerializable
		//, IO.IIndentedTextWritable
	{
		Runtime.BSettings mSettings = new Runtime.BSettings();
		Runtime.BSaveGame mSaveGame = new Runtime.BSaveGame();

		long mLeftoversPos;
		byte[] mLeftovers;

		#region IEndianStreamSerializable Members
		void SerializeLeftovers(IO.EndianStream s)
		{
			s.TraceAndDebugPosition(ref this.mLeftoversPos);

			if (s.IsReading)
				this.mLeftovers = new byte[s.BaseStream.Length - s.BaseStream.Position];

			s.Stream(this.mLeftovers, 0, this.mLeftovers.Length);
		}
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(this.mSettings);
			s.Stream(this.mSaveGame);
			this.SerializeLeftovers(s);
		}
		#endregion

		#region IIndentedStreamWritable Members
#if false
		public void ToStream(IO.IndentedTextWriter s)
		{
			using (s.EnterOwnerBookmark(this))
			{
				mSaveGame.ToStream(s);
			}
		}
#endif
		#endregion
	};
}