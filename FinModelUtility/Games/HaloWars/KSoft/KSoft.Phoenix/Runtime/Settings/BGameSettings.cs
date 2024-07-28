using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	sealed class BGameSettings
		: IO.IEndianStreamSerializable
	{
		const uint kVersion = 1;
		const int kDefaultSettingsCapacity = 113;

		public List<BGameSetting> Settings { get; private set; } = new List<BGameSetting>(kDefaultSettingsCapacity);

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Pad8();
			s.StreamVersion(kVersion);
			BSaveGame.StreamCollection(s, this.Settings);
		}
		#endregion
	};
}
