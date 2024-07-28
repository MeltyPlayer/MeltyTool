
namespace KSoft.Phoenix.Runtime
{
	sealed class BGameSetting
		: IO.IEndianStreamSerializable
	{
		string mName;
		BGameSettingVariant mValue;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.mName);
			s.Stream(ref this.mValue);
		}
		#endregion
	};
}