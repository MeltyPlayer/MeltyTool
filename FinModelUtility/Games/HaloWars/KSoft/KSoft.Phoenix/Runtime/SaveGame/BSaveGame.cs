using System.Collections.Generic;

namespace KSoft.Phoenix.Runtime
{
	static partial class cSaveMarker
	{
		public const ushort 
			Start = 0x2710, End = 0x2711,
			Versions = 0x2712,
			DB = 0x2713,
			SetupPlayer = 0x2714, SetupTeam = 0x2715, User1 = 0x2716, // User1 = SetupUser
			User2 = 0x2717,
			World = 0x2718,
			UI = 0x2719,

			FatalityManager = 0x2710,

			Arrow = 0x2712 // BObjectiveManager
			;
	};

	sealed partial class BSaveGame
		: IO.IEndianStreamSerializable
//		, IO.IIndentedTextWritable
	{
		int SaveFileType;
		public BDatabase Database { get; private set; } = new BDatabase();

		public List<BSavePlayer> Players { get; private set; } = new List<BSavePlayer>();
		public List<BSaveTeam> Teams { get; private set; } = new List<BSaveTeam>();
		public BSaveUser UserSave { get; private set; } = new BSaveUser();

		public BWorld World { get; private set; } = new BWorld();
		public BUIManager UIManager { get; private set; } = new BUIManager();
		public BUser User { get; private set; } = new BUser();

		#region IEndianStreamSerializable Members
		void SerializeSetup(IO.EndianStream s)
		{
			StreamCollection(s, this.Players);
			StreamCollection(s, this.Teams);
			s.StreamSignature(cSaveMarker.SetupTeam);
			s.Stream(this.UserSave);
		}
		void SerializeGameState(IO.EndianStream s)
		{
			s.Stream(this.World);
// 			s.StreamSignature(cSaveMarker.World);
// 			s.StreamObject(UIManager);
// 			s.StreamSignature(cSaveMarker.UI);
// 			s.StreamObject(User);
// 			s.StreamSignature(cSaveMarker.User2);
		}

		public void Serialize(IO.EndianStream s)
		{
			using (s.EnterOwnerBookmark(this))
			{
				s.StreamSignature(kClassVersions.BSaveGame);
				s.Stream(ref this.SaveFileType);

				s.StreamSignature(cSaveMarker.Start);
				kClassVersions.Serialize(s);
				s.Stream(this.Database);
				this.SerializeSetup(s);
				this.SerializeGameState(s);
				//s.StreamSignature(cSaveMarker.End);
			}
		}
		#endregion

		#region IIndentedStreamWritable Members
#if false
		public void ToStream(IO.IndentedTextWriter s)
		{
			using (s.EnterOwnerBookmark(this))
			{
				World.ToStream(s);
			}
		}
#endif
		#endregion
	};
}
