using System;
using System.Runtime.InteropServices;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;
using granny_matrix_4x4 = System.Numerics.Matrix4x4;

namespace KSoft.Granny3D
{
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_data_type_definition
	{
		public granny_member_type MemberType;
		public CharPtr Name;
		public IntPtr/*TPtr<granny_data_type_definition>*/ ReferenceTypeInternal;
		public int ArrayWidth;
		public int Extra0;
		public int Extra1;
		public int Extra2;
		IntPtr Ignored;

		// #64BIT: Workaround encountered issues trying to define a field which was a TPtr of the same parent type
		public TPtr<granny_data_type_definition> ReferenceType { get { return new TPtr<granny_data_type_definition>(this.ReferenceTypeInternal); } }
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_variant
	{
		public TPtr<granny_data_type_definition> Type;
		public IntPtr Object;
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct granny_transform
	{
		public granny_transform_flags Flags;
		public Vector3 Position;
		public Vector4 Orientation;
		public Vector3 ScaleShear0;
		public Vector3 ScaleShear1;
		public Vector3 ScaleShear2;
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct granny_matrix_3x3
	{
		public Vector3 Row0;
		public Vector3 Row1;
		public Vector3 Row2;
	};


	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_file_info
	{
		public TPtr<granny_art_tool_info> ArtToolInfo;
		public IntPtr /*granny_exporter_info*/ ExporterInfo;
		public CharPtr FromFileName;
		public ArrayOfRefsPtr<granny_texture> Textures;
		public ArrayOfRefsPtr<granny_material> Materials;
		public ArrayOfRefsPtr<granny_skeleton> Skeletons;
		public ArrayOfRefsPtr<granny_vertex_data> VertexDatas;
		public ArrayOfRefsPtr<granny_tri_topology> TriTopologies;
		public ArrayOfRefsPtr<granny_mesh> Meshes;
		public ArrayOfRefsPtr<granny_model> Models;
		public ArrayOfRefsPtr<granny_track_group> TrackGroups;
		public ArrayOfRefsPtr<granny_animation> Animations;
		public granny_variant ExtendedData;
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_art_tool_info
	{
		public CharPtr FromArtToolName;
		public int ArtToolMajorRevision;
		public int ArtToolMinorRevision;
		public float UnitsPerMeter;
		public Vector3 Origin;
		public Vector3 RightVector;
		public Vector3 UpVector;
		public Vector3 BackVector;
		public granny_variant ExtendedData;
	};

	#region granny_texture
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_texture
	{
		public CharPtr FromFileName;
		public granny_texture_type TextureType;
		public int Width;
		public int Height;
		public granny_texture_encoding Encoding;
		public int SubFormat;
		public TPtr<granny_pixel_layout> Layout;
		public ArrayPtr<granny_texture_image> Images;
		public granny_variant ExtendedData;
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct granny_pixel_layout;

	[StructLayout(LayoutKind.Sequential)]
	public struct granny_texture_image;
	#endregion

	#region granny_material
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_material
	{
		public CharPtr Name;
		public ArrayPtr<granny_material_map> Maps;
		public TPtr<granny_texture> Texture;
		public granny_variant ExtendedData;
	};

	[StructLayout(LayoutKind.Sequential)]
	public struct granny_material_map;
	#endregion

	#region granny_skeleton
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_skeleton
	{
		public CharPtr Name;
		public ArrayPtr<granny_bone> Bones;
		public int LODType;
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_bone
	{
		public CharPtr Name;
		public int ParentIndex;
		public granny_transform LocalTransform;
		public granny_matrix_4x4 InverseWorld4x4;
		public float LODError;
		public granny_variant ExtendedData;
	};
	#endregion

	#region granny_mesh
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_mesh
	{
		public CharPtr Name;
		public TPtr<granny_vertex_data> PrimaryVertexData;
		public ArrayPtr<granny_morph_target> MorphTargets;
		public TPtr<granny_tri_topology> PrimaryTopology;
		public ArrayPtr<granny_material_binding> MaterialBindings;
		public ArrayPtr<granny_bone_binding> BoneBindings;
		public granny_variant ExtendedData;
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_vertex_data
	{
		public TPtr<granny_data_type_definition> VertexType;
		public ArrayPtr Vertices;
		public ArrayCharPtr VertexComponentNames;
		public ArrayPtr<granny_vertex_annotation_set> VertexAnnotationSets;
	};
	[StructLayout(LayoutKind.Sequential)]
	public struct granny_vertex_annotation_set;

	[StructLayout(LayoutKind.Sequential)]
	public struct granny_morph_target;

	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_tri_topology
	{
		public ArrayPtr<granny_tri_material_group> Groups;
		public ArrayPtr Indices;
		public ArrayPtr<ushort> Indices16;
		public ArrayPtr VertexToVertexMap;
		public ArrayPtr VertexToTriangleMap;
		public ArrayPtr SideToNeightborMap;
		public ArrayPtr BonesForTriangle;
		public ArrayPtr TriangleToBoneIndices;
		public ArrayPtr<granny_tri_annotation_set> TriAnnotationSets;
	};
	[StructLayout(LayoutKind.Sequential)]
	public struct granny_tri_material_group
	{
		public int MaterialIndex;
		public int TriFirst;
		public int TriCount;
	};
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_tri_annotation_set
	{
		public CharPtr Name;
		public TPtr<granny_data_type_definition> TriAnnotationType;
		public ArrayPtr TriAnnotations;
		public int IndicesMapFromTriToAnnotation; // BOOL
		public ArrayPtr<int> TriAnnotationIndices;
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_material_binding
	{
		public TPtr<granny_material> Material;
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_bone_binding
	{
		public CharPtr BoneName;
		public Vector3 OBBMin;
		public Vector3 OBBMax;
		public ArrayPtr<int> TriangleIndices;
	};
	#endregion

	#region granny_model
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_model
	{
		public CharPtr Name;
		public TPtr<granny_skeleton> Skeleton;
		public granny_transform InitialPlacement;
		public ArrayPtr<granny_model_mesh_binding> MeshBindings;
	};

	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_model_mesh_binding
	{
		public TPtr<granny_mesh> Mesh;
	};
	#endregion

	#region granny_track_group
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_track_group
	{
		public CharPtr Name;
		public ArrayPtr /*granny_vector_track */ VectorTracks;
		public ArrayPtr /*granny_transform_track */ TransformTracks;
		public ArrayPtr<float> TransformLODErrors;
		public ArrayPtr /*granny_text_track */ TextTracks;
		public granny_transform InitialPlacement;
		public granny_track_group_flags Flags;
		public Vector3 LoopTranslation;
		public IntPtr /*granny_periodic_loop */ PeriodicLoop;
		public IntPtr /*granny_transform_track */ RootMotion;
		public granny_variant ExtendedData;
	};
	#endregion

	#region granny_animation
	[StructLayout(LayoutKind.Sequential, Pack=Granny2DLL.kAssumedPointerSize)]
	public struct granny_animation
	{
		public CharPtr Name;
		public float Duration;
		public float TimeStep;
		public float Oversampling;
		public ArrayOfRefsPtr<granny_track_group> TrackGroups;
	};
	#endregion

	[StructLayout(LayoutKind.Sequential)]
	public struct granny_model_instance;

	[StructLayout(LayoutKind.Sequential)]
	public struct granny_control;

	[StructLayout(LayoutKind.Sequential)]
	public struct granny_model_control_binding;

	[StructLayout(LayoutKind.Sequential)]
	public struct granny_world_pose;

	[StructLayout(LayoutKind.Sequential)]
	public struct granny_mesh_binding;

	[StructLayout(LayoutKind.Sequential)]
	public struct granny_mesh_deformer;

	[StructLayout(LayoutKind.Sequential)]
	public struct granny_local_pose;
}
