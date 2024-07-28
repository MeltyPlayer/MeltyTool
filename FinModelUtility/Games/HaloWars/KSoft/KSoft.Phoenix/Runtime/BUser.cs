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
				s.Stream(ref this.Minimap); s.Stream(ref this.Resources); s.Stream(ref this.Time);
				s.Stream(ref this.PowerStatus); s.Stream(ref this.Units); s.Stream(ref this.DpadHelp);
				s.Stream(ref this.ButtonHelp); s.Stream(ref this.Reticle); s.Stream(ref this.Score);
				s.Stream(ref this.UnitStats); s.Stream(ref this.CircleMenuExtraInfo);
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
				s.Stream(ref this.HaveArrow);
				if (!this.HaveArrow) return;

				s.StreamV(ref this.Origin); s.StreamV(ref this.Target);
				s.Stream(ref this.Offset);
				s.Stream(ref this.ObjectID); s.Stream(ref this.LocationObjectID);
				s.Stream(ref this.PlayerID);
				s.Stream(ref this.FlagVisible); s.Stream(ref this.FlagUseTarget); s.Stream(ref this.FlagTargetDirty);
				s.Stream(ref this.FlagForceTargetVisible);
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
			s.Stream(ref this.UIPowerRadius);
			s.Stream(ref this.UIProtoPowerID);
			s.Stream(ref this.UIModeRestoreCameraZoomMin); s.Stream(ref this.UIModeRestoreCameraZoomMax); s.Stream(ref this.UIModeRestoreCameraZoom);
			s.Stream(ref this.UIModeRestoreCameraPitchMin); s.Stream(ref this.UIModeRestoreCameraPitchMax); s.Stream(ref this.UIModeRestoreCameraPitch);
			s.Stream(ref this.UIModeRestoreCameraYaw);

			s.Stream(ref this.FlagUIModeRestoreCameraZoomMin); s.Stream(ref this.FlagUIModeRestoreCameraZoomMax); s.Stream(ref this.FlagUIModeRestoreCameraZoom);
			s.Stream(ref this.FlagUIModeRestoreCameraPitchMin); s.Stream(ref this.FlagUIModeRestoreCameraPitchMax); s.Stream(ref this.FlagUIModeRestoreCameraPitch);
			s.Stream(ref this.FlagUIModeRestoreCameraYaw);
			s.Stream(ref this.FlagCameraScrollEnabled); s.Stream(ref this.FlagCameraYawEnabled); s.Stream(ref this.FlagCameraZoomEnabled); s.Stream(ref this.FlagCameraAutoZoomInstantEnabled);
			s.Stream(ref this.FlagCameraAutoZoomEnabled); s.Stream(ref this.FlagRestoreCameraEnableUserScroll); s.Stream(ref this.FlagRestoreCameraEnableUserYaw); s.Stream(ref this.FlagRestoreCameraEnableUserZoom);
			s.Stream(ref this.FlagRestoreCameraEnableAutoZoomInstant); s.Stream(ref this.FlagRestoreCameraEnableAutoZoom);

			s.Stream(ref this.PowerType, BPowerTypeStreamer.Instance);
			s.Stream(ref this.PowerUser,
				() => BPowerUser.FromType(this.PowerType));
		}
		public void Serialize(IO.EndianStream s)
		{
			s.Stream(ref this.UserMode); s.Stream(ref this.SubMode);
			if (this.UserMode == 16)
				this.SerializeUserMode16(s);

			BSaveGame.StreamArray16(s, ref this.SelectionList);
			s.Stream(ref this.CameraZoomMin); s.Stream(ref this.CameraZoomMax);
			s.Stream(ref this.CameraPitchMin); s.Stream(ref this.CameraPitchMax);
			s.Stream(ref this.CameraPitch); s.Stream(ref this.CameraYaw);
			s.Stream(ref this.CameraZoom); s.Stream(ref this.CameraFOV);
			s.StreamV(ref this.HoverPoint); s.StreamV(ref this.CameraHoverPoint);
			s.Stream(ref this.CameraHoverPointOffsetHeight);
			s.StreamV(ref this.LastCameraLoc); s.StreamV(ref this.LastCameraHoverPoint);
			s.Stream(ref this.HaveHoverPoint); s.Stream(ref this.HoverPointOverTerrain);
			s.Stream(ref this.HUDItemEnabled);
			BSaveGame.StreamCollection(s, this.ObjectiveArrows);
		}
		#endregion
	};
}