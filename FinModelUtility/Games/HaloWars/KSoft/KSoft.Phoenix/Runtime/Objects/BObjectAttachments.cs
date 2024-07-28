
using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	public struct BObjectAttachments
		: IO.IEndianStreamSerializable
	{
		public BVector Position, Up, Forward;
		public BEntityID AttachmentObjectID;
		public int ToBoneHandle, FromBoneHandle;
		public bool IsUnitAttachment
			, UseOffset
			;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.StreamV(ref this.Position); s.StreamV(ref this.Up); s.StreamV(ref this.Forward);
			s.Stream(ref this.AttachmentObjectID);
			s.Stream(ref this.ToBoneHandle); s.Stream(ref this.FromBoneHandle);
			s.Stream(ref this.IsUnitAttachment); s.Stream(ref this.UseOffset);
		}
		#endregion
	};
}