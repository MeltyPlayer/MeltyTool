
using BVector = System.Numerics.Vector4;

namespace KSoft.Phoenix.Runtime
{
	sealed class BSaveUser
		: IO.IEndianStreamSerializable
	{
		public int CurrentPlayer, CoopPlayer;
		public BVector HoverPoint, CameraHoverPoint, CameraPosition, 
			CameraForward, CameraRight, CameraUp;
		public float CameraDefaultPitch, CameraDefaultYaw, CameraDefaultZoom,
			CameraPitch, CameraYaw, CameraZoom,
			CameraFOV, CameraHoverPointOffsetHeight;
		public bool HaveHoverPoint, DefaultCamera;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.CurrentPlayer);
			s.Stream(ref this.CoopPlayer);
			s.StreamV(ref this.HoverPoint); s.StreamV(ref this.CameraHoverPoint); s.StreamV(ref this.CameraPosition);
			s.StreamV(ref this.CameraForward); s.StreamV(ref this.CameraRight); s.StreamV(ref this.CameraUp);
			s.Stream(ref this.CameraDefaultPitch); s.Stream(ref this.CameraDefaultYaw); s.Stream(ref this.CameraDefaultZoom);
			s.Stream(ref this.CameraPitch); s.Stream(ref this.CameraYaw); s.Stream(ref this.CameraZoom);
			s.Stream(ref this.CameraFOV);
			s.Stream(ref this.CameraHoverPointOffsetHeight);
			s.Stream(ref this.HaveHoverPoint);
			s.Stream(ref this.DefaultCamera);
			s.StreamSignature(cSaveMarker.User1);
		}
		#endregion
	};
}