using System.Collections.Generic;

using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	static partial class cSaveMarker
	{
		public const ushort 
			UICallouts1 = 0x2710,
			UICallouts2 = 0x2711,
			UICallout = 0x2712
			;
	};

	sealed class BUICallouts
		: IO.IEndianStreamSerializable
	{
		const byte cNumCallouts = 5;

		public sealed class BUICallout
			: IO.IEndianStreamSerializable
		{
			public int ID;
			public uint Type;
			public BVector Location;
			public BEntityID EntityID, CalloutEntityID;
			public int LocStringIndex, UICalloutID, X, Y;
			public bool Visible;

			internal BVector Position; // Not actually part of this struct

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.ID);
				s.Stream(ref this.Type);
				s.StreamV(ref this.Location);
				s.Stream(ref this.EntityID); s.Stream(ref this.CalloutEntityID);
				s.Stream(ref this.LocStringIndex); s.Stream(ref this.UICalloutID); s.Stream(ref this.X); s.Stream(ref this.Y);
				s.Stream(ref this.Visible);
				s.StreamSignature(cSaveMarker.UICallout);

				s.StreamV(ref this.Position);
			}
			#endregion
		};
		static readonly CondensedListInfo kCalloutsListInfo = new CondensedListInfo()
		{
			SerializeCapacity=true,
			IndexSize=sizeof(short),
		};

		public List<CondensedListItem16<BUICallout>> Callouts = new List<CondensedListItem16<BUICallout>>();
		public int[] CalloutWidgets = new int[cNumCallouts];
		public int NextCalloutID;
		public bool PanelVisible, CalloutsVisible;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamList(s, this.Callouts, kCalloutsListInfo);
			s.StreamSignature(cSaveMarker.UICallouts1);
			s.StreamSignature(cNumCallouts);
			for (int x = 0; x < this.CalloutWidgets.Length; x++)
				s.Stream(ref this.CalloutWidgets[x]);
			s.Stream(ref this.NextCalloutID);
			s.Stream(ref this.PanelVisible); s.Stream(ref this.CalloutsVisible);
			s.StreamSignature(cSaveMarker.UICallouts2);
		}
		#endregion
	};
}