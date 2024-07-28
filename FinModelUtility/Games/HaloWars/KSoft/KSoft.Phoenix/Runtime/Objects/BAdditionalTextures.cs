
using BVec2 = System.Numerics.Vector2;

namespace KSoft.Phoenix.Runtime
{
	public struct BAdditionalTextures
		: IO.IEndianStreamSerializable
	{
		public int RenderType, Texture;
		public BVec2 TexUVOfs;
		public float TexUVScale, TexInten, TexScrollSpeed;
		// relative in gamefiles, TexTimeout - gWorld->getGametimeFloat()
		public float TexTimeoutOffset;
		public bool ModulateOffset
			, ModulateIntensity
			, ShouldBeCopied
			, TexClamp
			, TexScrollLoop
			;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.RenderType);
			s.Stream(ref this.Texture);
			s.StreamV(ref this.TexUVOfs);
			s.Stream(ref this.TexUVScale);
			s.Stream(ref this.TexInten);
			s.Stream(ref this.TexScrollSpeed);
			s.Stream(ref this.TexTimeoutOffset);
			s.Stream(ref this.ModulateOffset);
			s.Stream(ref this.ModulateIntensity);
			s.Stream(ref this.ShouldBeCopied);
			s.Stream(ref this.TexClamp);
			s.Stream(ref this.TexScrollLoop);
		}
		#endregion
	};
}