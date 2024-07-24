using System;

namespace KSoft.Granny3D
{
	public enum granny_member_type
	{
		EndMember,

		InlineMember,
		ReferenceMember,
		ReferenceToArrayMember,
		ArrayOfReferencesMember,
		VariantReferenceMember,
		SwitchableTypeMember,
		ReferenceToVariantArrayMember,
		StringMember,
		TransformMember,
		Real32Member,
		Int8Member,
		UInt8Member,
		BinormalInt8Member,
		NormalUInt8Member,
		Int16Member,
		UInt16Member,
		BinormalInt16Member,
		NormalUInt16Member,
		Int32Member,
		UInt32Member,

		Bool32Member = Int32Member,
	};

	[Flags]
	public enum granny_transform_flags
	{
		HasPosition = 1<<0,
		HasOrientation = 1<<1,
		HasScaleShear = 1<<2,
	};
	[Flags]
	public enum granny_transform_file_flags
	{
		RenormalizeNormals = 1<<0,
		ReorderTriangleIndices = 1<<1,
	};

	public enum granny_texture_type
	{
		ColorMapTextureType,
		CubeMapTextureType,
	};

	public enum granny_texture_encoding
	{
		UserTextureEncoding,
		RawTextureEncoding,
		S3TCTextureEncoding,
		BinkTextureEncoding,
	};

	public enum granny_material_texture_type
	{
		UnknownTextureType,
		AmbientColorTexture,
		DiffuseColorTexture,
		SpecularColorTexture,
		SelfIlluminationTexture,
		OpacityTexture,
		BumpHeightTexture,
		ReflectionTexture,
		RefractionTexture,
		DisplacementTexture,
	};

	[Flags]
	public enum granny_track_group_flags
	{
		AccumulationExtracted = 1<<0,
		TrackGroupIsSorted = 1<<1,
		AccumulationIsVDA = 1<<2,
	};

	public enum granny_deformation_type
	{
		DeformPosition = 1,
		DeformPositionNormal,
		DeformPositionNormalTangent,
		DeformPositionNormalTangentBinormal,
	};

	public enum granny_deformer_tail_flags
	{
		DontAllowUncopiedTail,
		AllowUncopiedTail,
	};
}
