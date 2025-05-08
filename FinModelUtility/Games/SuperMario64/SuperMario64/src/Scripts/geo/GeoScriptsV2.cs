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

      var root = new GeoScriptNode(null, FinMatrix4x4.IDENTITY);
      mdlLods.Node = root;

      this.Add_(mdlLods, lvl, commandList, root);
    }

    private void Add_(
        Model3DLods mdlLods,
        Level lvl,
        IGeoCommandList commandList,
        GeoScriptNode root) {
      GeoScriptNode parent = root;

      var matrixStack = new Matrix4x4Stack();
      matrixStack.Push(parent.matrix.Impl);

      var id = 0;

      foreach (var command in commandList.Commands) {
        var mulMatrix = command switch {
            GeoAnimatedPartCommand geoAnimatedPartCommand
                => this.CreateTranslationMatrix_(
                    geoAnimatedPartCommand.Translation),
            GeoBillboardCommand geoBillboardCommand
                => this.CreateTranslationMatrix_(
                    geoBillboardCommand.Translation),
            GeoRotationCommand geoRotationCommand
                => this.CreateRotationMatrix_(geoRotationCommand.Rotation),
            GeoScaleCommand geoScaleCommand
                => FinMatrix4x4Util.FromScale(geoScaleCommand.Scale / 65536.0f),
            GeoTranslateAndRotateCommand geoTranslateAndRotateCommand
                => this.CreateTranslationAndRotationMatrix_(
                    geoTranslateAndRotateCommand.Translation,
                    geoTranslateAndRotateCommand.Rotation),
            GeoTranslationCommand geoTranslationCommand
                => this.CreateTranslationMatrix_(
                    geoTranslationCommand.Translation),
            _ => FinMatrix4x4.IDENTITY,
        };

        matrixStack.Top *= mulMatrix.Impl;

        var current = new GeoScriptNode(parent,
                                        new FinMatrix4x4(matrixStack.Top));
        current.ID = id++;
        current.parent = parent;

        mdlLods.Node = current;

        if (command is IGeoCommandWithBranch commandWithBranch) {
          if (commandWithBranch.GeoCommandList != null) {
            this.Add_(mdlLods,
                      lvl,
                      commandWithBranch.GeoCommandList,
                      current);
          }

          if (!commandWithBranch.StoreReturnAddress) {
            return;
          }
        }

        switch (command) {
          case GeoOpenNodeCommand geoOpenNodeCommand: {
            parent = current;
            matrixStack.Push();
            break;
          }
          case GeoCloseNodeCommand: {
            parent = current.parent;
            matrixStack.Pop();
            break;
          }
          case GeoSetRenderRangeCommand geoSetRenderRangeCommand: {
            mdlLods.AddLod(current);
            break;
          }
        }

        if (command is IGeoCommandWithDisplayList commandWithDisplayList) {
          this.AddDisplayList(
              mdlLods,
              lvl,
              commandWithDisplayList.DisplayListSegmentedAddress);
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