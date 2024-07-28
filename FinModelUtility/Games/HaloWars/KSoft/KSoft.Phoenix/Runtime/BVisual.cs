#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

using BVector = System.Numerics.Vector4;
using BMatrix = System.Numerics.Matrix4x4;

using BVisualAsset = System.UInt64; // unknown fields

namespace KSoft.Phoenix.Runtime
{
	public sealed class BVisualItem
		: IO.IEndianStreamSerializable
	{
		const int kUVOffsetsSize = 0x18;
	public BMatrix Matrix;
		public uint SubUpdateNumber, GrannySubUpdateNumber;
		public BMatrix Matrix1, Matrix2;
		public BVector
			CombinedMinCorner, CombinedMaxCorner,
			MinCorner, MaxCorner;
		public BVisualAsset ModelAsset;
		public byte[] ModelUVOffsets = new byte[kUVOffsetsSize]; // BVisualModelUVOffsets
		public uint Flags;
		public BVisualItem[] Attachments;

		#region IEndianStreamSerializable Members
		void StreamFlags(IO.EndianStream s)
		{
			const byte k_size_in_bytes = sizeof(uint);

			s.StreamSignature(k_size_in_bytes);
			s.Stream(ref this.Flags);
		}
		void StreamAttachments(IO.EndianStream s)
		{
			Contract.Assert(false); // TODO
		}
		public void Serialize(IO.EndianStream s)
		{
			BSaveGame.StreamMatrix(s, ref this.Matrix);
			s.Stream(ref this.SubUpdateNumber);
			s.Stream(ref this.GrannySubUpdateNumber);
			BSaveGame.StreamMatrix(s, ref this.Matrix1);
			BSaveGame.StreamMatrix(s, ref this.Matrix2);
			s.StreamV(ref this.CombinedMinCorner); s.StreamV(ref this.CombinedMaxCorner);
			s.StreamV(ref this.MinCorner); s.StreamV(ref this.MaxCorner);
			s.Stream(ref this.ModelAsset);
			if (s.StreamCond(this.ModelUVOffsets, offsets => !offsets.EqualsZero()))
				s.Stream(this.ModelUVOffsets);
			this.StreamFlags(s);
			this.StreamAttachments(s);
		}
		#endregion
	};

	public sealed class BVisual
		: IO.IEndianStreamSerializable
	{
		public int ProtoId;

		public long UserData;

		#region IEndianStreamSerializable Members
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.UserData);
		}
		#endregion
	};

	public static class BVisualManager
	{
		static BVisual NewVisual()
		{
			return new BVisual();
		}
		static void SetProtoId(BVisual visual, int id)
		{
			visual.ProtoId = id;
		}
		static int GetProtoId(BVisual visual)
		{
			return visual.ProtoId;
		}
		internal static void Stream(IO.EndianStream s, ref BVisual visual)
		{
			if (BSaveGame.StreamObjectId(s, ref visual, NewVisual, SetProtoId, GetProtoId))
			{
				visual.Serialize(s);
			}
		}
	};
}