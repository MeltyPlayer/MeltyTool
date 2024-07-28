using System.Collections.Generic;

using BVector = System.Numerics.Vector4;
using BEntityID = System.Int32;
using BPlayerID = System.Int32;

namespace KSoft.Phoenix.Runtime
{
	using BPowerTypeStreamer = IO.EnumBinaryStreamer<Phx.BPowerType, uint>;

	sealed class BUser
		: IO.IEndianStreamSerializable
	{
		public struct HUDItemEnabledStates
			: IO.IEndianStreamSerializable
		{
	  public bool Minimap, Resources, Time,
				PowerStatus, Units, DpadHelp,
				ButtonHelp, Reticle, Score,
				UnitStats, CircleMenuExtraInfo;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref Minimap); s.Stream(ref Resources); s.Stream(ref Time);
				s.Stream(ref PowerStatus); s.Stream(ref Units); s.Stream(ref DpadHelp);
				s.Stream(ref ButtonHelp); s.Stream(ref Reticle); s.Stream(ref Score);
				s.Stream(ref UnitStats); s.Stream(ref CircleMenuExtraInfo);
			}
			#endregion
		};

		public sealed class BObjectiveArrow
			: IO.IEndianStreamSerializable
		{
			public bool HaveArrow;

			public BVector Origin, Target;
			public float Offset;
			public BEntityID ObjectID, LocationObjectID;
			public BPlayerID PlayerID;
			public bool FlagVisible, FlagUseTarget, FlagTargetDirty,
				FlagForceTargetVisible;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref HaveArrow);
				if (!HaveArrow) return;

				s.StreamV(ref Origin); s.StreamV(ref Target);
				s.Stream(ref Offset);
				s.Stream(ref ObjectID); s.Stream(ref LocationObjectID);
				s.Stream(ref PlayerID);
				s.Stream(ref FlagVisible); s.Stream(ref FlagUseTarget); s.Stream(ref FlagTargetDirty);
				s.Stream(ref FlagForceTargetVisible);
				s.StreamSignature(cSaveMarker.Arrow);
			}
			#endregion
		};

		public int UserMode, SubMode;
		#region UserMode==16
		public float UIPowerRadius;
		public int UIProtoPowerID;
		public float UIModeRestoreCameraZoomMin, UIModeRestoreCameraZoomMax, UIModeRestoreCameraZoom,
			UIModeRestoreCameraPitchMin, UIModeRestoreCameraPitchMax, UIModeRestoreCameraPitch,
			UIModeRestoreCameraYaw;
		public bool FlagUIModeRestoreCameraZoomMin, FlagUIModeRestoreCameraZoomMax, FlagUIModeRestoreCameraZoom,
			FlagUIModeRestoreCameraPitchMin, FlagUIModeRestoreCameraPitchMax, FlagUIModeRestoreCameraPitch,
			FlagUIModeRestoreCameraYaw,
			FlagCameraScrollEnabled, FlagCameraYawEnabled, FlagCameraZoomEnabled, FlagCameraAutoZoomInstantEnabled,
			FlagCameraAutoZoomEnabled, FlagRestoreCameraEnableUserScroll, FlagRestoreCameraEnableUserYaw, FlagRestoreCameraEnableUserZoom,
			FlagRestoreCameraEnableAutoZoomInstant, FlagRestoreCameraEnableAutoZoom;
		public Phx.BPowerType PowerType;
		public BPowerUser PowerUser;
		#endregion
		public BEntityID[] SelectionList;

		public float CameraZoomMin, CameraZoomMax,
			CameraPitchMin, CameraPitchMax,
			CameraPitch, CameraYaw,
			CameraZoom, CameraFOV;
		public BVector HoverPoint, CameraHoverPoint;
		public float CameraHoverPointOffsetHeight;
		public BVector LastCameraLoc, LastCameraHoverPoint;
		public bool HaveHoverPoint, HoverPointOverTerrain;
		public HUDItemEnabledStates HUDItemEnabled = new HUDItemEnabledStates();
		public List<BObjectiveArrow> ObjectiveArrows = new List<BObjectiveArrow>();

		#region IEndianStreamSerializable Members
		void SerializeUserMode16(IO.EndianStream s)
		{
			s.Stream(ref UIPowerRadius);
			s.Stream(ref UIProtoPowerID);
			s.Stream(ref UIModeRestoreCameraZoomMin); s.Stream(ref UIModeRestoreCameraZoomMax); s.Stream(ref UIModeRestoreCameraZoom);
			s.Stream(ref UIModeRestoreCameraPitchMin); s.Stream(ref UIModeRestoreCameraPitchMax); s.Stream(ref UIModeRestoreCameraPitch);
			s.Stream(ref UIModeRestoreCameraYaw);

			s.Stream(ref FlagUIModeRestoreCameraZoomMin); s.Stream(ref FlagUIModeRestoreCameraZoomMax); s.Stream(ref FlagUIModeRestoreCameraZoom);
			s.Stream(ref FlagUIModeRestoreCameraPitchMin); s.Stream(ref FlagUIModeRestoreCameraPitchMax); s.Stream(ref FlagUIModeRestoreCameraPitch);
			s.Stream(ref FlagUIModeRestoreCameraYaw);
			s.Stream(ref FlagCameraScrollEnabled); s.Stream(ref FlagCameraYawEnabled); s.Stream(ref FlagCameraZoomEnabled); s.Stream(ref FlagCameraAutoZoomInstantEnabled);
			s.Stream(ref FlagCameraAutoZoomEnabled); s.Stream(ref FlagRestoreCameraEnableUserScroll); s.Stream(ref FlagRestoreCameraEnableUserYaw); s.Stream(ref FlagRestoreCameraEnableUserZoom);
			s.Stream(ref FlagRestoreCameraEnableAutoZoomInstant); s.Stream(ref FlagRestoreCameraEnableAutoZoom);

			s.Stream(ref PowerType, BPowerTypeStreamer.Instance);
			s.Stream(ref PowerUser,
				() => BPowerUser.FromType(PowerType));
		}
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref UserMode); s.Stream(ref SubMode);
			if (UserMode == 16)
				SerializeUserMode16(s);

			BSaveGame.StreamArray16(s, ref SelectionList);
			s.Stream(ref CameraZoomMin); s.Stream(ref CameraZoomMax);
			s.Stream(ref CameraPitchMin); s.Stream(ref CameraPitchMax);
			s.Stream(ref CameraPitch); s.Stream(ref CameraYaw);
			s.Stream(ref CameraZoom); s.Stream(ref CameraFOV);
			s.StreamV(ref HoverPoint); s.StreamV(ref CameraHoverPoint);
			s.Stream(ref CameraHoverPointOffsetHeight);
			s.StreamV(ref LastCameraLoc); s.StreamV(ref LastCameraHoverPoint);
			s.Stream(ref HaveHoverPoint); s.Stream(ref HoverPointOverTerrain);
			s.Stream(ref HUDItemEnabled);
			BSaveGame.StreamCollection(s, ObjectiveArrows);
		}
		#endregion
	};
}