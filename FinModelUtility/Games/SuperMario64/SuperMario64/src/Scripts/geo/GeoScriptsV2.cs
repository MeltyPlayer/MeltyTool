using System.Numerics;

using f3dzex2.io;

using fin.math.matrix.four;
using fin.model.impl;
using fin.schema.vector;

using sm64.LevelInfo;
using sm64.memory;
using sm64.scripts;
using sm64.scripts.geo;

namespace sm64.Scripts {
  public class GeoScriptsV2 : IGeoScripts {
    private GeoScriptNode rootNode;
    private GeoScriptNode nodeCurrent;

    public GeoScriptsV2() {
      this.rootNode = new GeoScriptNode(null);
      this.nodeCurrent = this.rootNode;
    }

    public void parse(
        IReadOnlySm64Memory n64Memory,
        Model3DLods mdlLods,
        ref Level lvl,
        byte seg,
        uint off) {
      var commandList =
          new GeoScriptParser().Parse(IoUtils.MergeSegmentedAddress(seg, off),
                                      n64Memory.AreaId);
      if (commandList == null) {
        return;
      }

      mdlLods.Node = this.nodeCurrent;

      this.Add_(mdlLods, lvl, commandList);
    }

    private void Add_(
        Model3DLods mdlLods,
        Level lvl,
        IGeoCommandList commandList) {
      foreach (var command in commandList.Commands) {
        switch (command) {
          case GeoAnimatedPartCommand geoAnimatedPartCommand: {
            var translation = geoAnimatedPartCommand.Offset;
            this.nodeCurrent.matrix.MultiplyInPlace(this.CreateTranslationMatrix_(translation));
            this.AddDisplayList(
                mdlLods,
                lvl,
                geoAnimatedPartCommand.DisplayListSegmentedAddress);
            break;
          }
          case GeoBillboardCommand geoBillboardCommand: break;
          case GeoBranchAndStoreCommand geoBranchAndStoreCommand: {
            if (geoBranchAndStoreCommand.GeoCommandList != null) {
              var currentNode = this.nodeCurrent;
              this.Add_(mdlLods,
                        lvl,
                        geoBranchAndStoreCommand.GeoCommandList);
              mdlLods.Node = currentNode;
            }

            break;
          }
          case GeoBranchCommand geoBranchCommand: {
            if (geoBranchCommand.GeoCommandList != null) {
              var currentNode = this.nodeCurrent;
              this.Add_(mdlLods, lvl, geoBranchCommand.GeoCommandList);
              if (geoBranchCommand.StoreReturnAddress) {
                mdlLods.Node = currentNode;
              }
            }

            break;
          }
          case GeoCloseNodeCommand: {
            if (this.nodeCurrent != this.rootNode) {
              this.nodeCurrent = this.nodeCurrent.parent;
              mdlLods.Node = this.nodeCurrent;
            }

            break;
          }
          case GeoDisplayListCommand geoDisplayListCommand: {
            this.AddDisplayList(
                mdlLods,
                lvl,
                geoDisplayListCommand.DisplayListSegmentedAddress);
            break;
          }
          case GeoDisplayListFromAsm geoDisplayListFromAsm: break;
          case GeoHeldObjectCommand geoHeldObjectCommand:   break;
          case GeoObjectListCommand geoObjectListCommand:   break;
          case GeoOpenNodeCommand geoOpenNodeCommand: {
            GeoScriptNode newNode = new GeoScriptNode(this.nodeCurrent);
            newNode.ID = this.nodeCurrent.ID + 1;
            newNode.parent = this.nodeCurrent;
            this.nodeCurrent = newNode;
            mdlLods.Node = this.nodeCurrent;
            break;
          }
          case GeoRotationCommand geoRotationCommand: {
            var rotation = geoRotationCommand.Rotation;
            this.nodeCurrent.matrix.MultiplyInPlace(this.CreateRotationMatrix_(rotation));
            this.AddDisplayList(
                mdlLods,
                lvl,
                geoRotationCommand.DisplayListSegmentedAddress);
            break;
          }
          case GeoScaleCommand geoScaleCommand: {
            var scale = (geoScaleCommand.Scale / 65536.0f);
            this.nodeCurrent.matrix.MultiplyInPlace(
                FinMatrix4x4Util.FromScale(scale));
            this.AddDisplayList(
                mdlLods,
                lvl,
                geoScaleCommand.DisplayListSegmentedAddress);
            break;
          }
          case GeoSetRenderRangeCommand geoSetRenderRangeCommand: {
            mdlLods.AddLod(this.nodeCurrent!);
            break;
          }
          case GeoShadowCommand geoShadowCommand: break;
          case GeoSwitchCommand geoSwitchCommand: break;
          case GeoTranslateAndRotateCommand geoTranslateAndRotateCommand: {
            var translation = geoTranslateAndRotateCommand.Translation;
            var rotation = geoTranslateAndRotateCommand.Rotation;
            this.nodeCurrent.matrix.MultiplyInPlace(this.CreateTranslationAndRotationMatrix_(translation, rotation));
            this.AddDisplayList(
                mdlLods,
                lvl,
                geoTranslateAndRotateCommand.DisplayListSegmentedAddress);
            break;
          }
          case GeoTranslationCommand geoTranslationCommand: {
            var translation = geoTranslationCommand.Translation;
            this.nodeCurrent.matrix.MultiplyInPlace(this.CreateTranslationMatrix_(translation));
            this.AddDisplayList(
                mdlLods,
                lvl,
                geoTranslationCommand.DisplayListSegmentedAddress);
            break;
          }
          case GeoBackgroundCommand geoBackgroundCommand: break;
          case GeoCameraFrustumCommand geoCameraFrustumCommand: break;
          case GeoCameraLookAtCommand geoCameraLookAtCommand: break;
          case GeoCullingRadiusCommand geoCullingRadiusCommand: break;
          case GeoNoopCommand geoNoopCommand: break;
          case GeoOrthoMatrixCommand geoOrthoMatrixCommand: break;
          case GeoReturnFromBranchCommand geoReturnFromBranchCommand: break;
          case GeoStartLayoutCommand geoStartLayoutCommand: break;
          case GeoTerminateCommand geoTerminateCommand: break;
          case GeoToggleDepthBufferCommand geoToggleDepthBufferCommand: break;
          case GeoViewportCommand geoViewportCommand: break;
          default: throw new ArgumentOutOfRangeException(nameof(command));
        }
      }
    }

    public IFinMatrix4x4 CreateTranslationAndRotationMatrix_(
        Vector3s position,
        Vector3s rotation)
      => this.CreateRotationMatrix_(rotation)
             .MultiplyInPlace(this.CreateTranslationMatrix_(position));

    public IFinMatrix4x4 CreateTranslationMatrix_(Vector3s position)
      => FinMatrix4x4Util.FromTranslation(
          new Vector3(position.X, position.Y, position.Z));

    public IFinMatrix4x4 CreateRotationMatrix_(Vector3s rotation)
      => FinMatrix4x4Util
         .FromRotation(
             new RotationImpl().SetDegrees(0, 0, rotation.Z))
         .MultiplyInPlace(
             FinMatrix4x4Util.FromRotation(
                 new RotationImpl().SetDegrees(rotation.X, 0, 0)))
         .MultiplyInPlace(
             FinMatrix4x4Util.FromRotation(
                 new RotationImpl().SetDegrees(0, rotation.Y, 0)));

    public void AddDisplayList(
        Model3DLods mdlLods,
        Level lvl,
        uint? displayListAddress) {
      if ((displayListAddress ?? 0) != 0) {
        mdlLods.AddDl(displayListAddress.Value);
      }
    }
  }
}