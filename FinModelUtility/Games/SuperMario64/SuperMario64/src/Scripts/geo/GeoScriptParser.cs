using f3dzex2.io;

using fin.util.asserts;

using schema.binary;

using sm64.schema;

namespace sm64.scripts.geo {
  public class GeoScriptParser {
    private class GeoCommandList : IGeoCommandList {
      private readonly List<IGeoCommand> commands_ = [];

      public void AddCommand(IGeoCommand command)
        => this.commands_.Add(command);

      public IReadOnlyList<IGeoCommand> Commands => this.commands_;
    }

    public enum ReturnType {
      UNDEFINED,
      TERMINATED,
      RETURNED
    }

    public IGeoCommandList Parse(uint address, byte? areaId)
      => Asserts.CastNonnull(this.ParseImpl_(address, areaId).Value).Item1;

    private (IGeoCommandList, ReturnType)?
        ParseImpl_(uint address, byte? areaId) {
      IoUtils.SplitSegmentedAddress(address, out var seg, out var off);

      ROM rom = ROM.Instance;
      byte[] data = rom.getSegment(seg, areaId)!;

      if (data == null) {
        return null;
      }

      if (off >= data.Length) {
        return null;
      }

      using var br =
          new SchemaBinaryReader(data, SchemaConstants.SM64_ENDIANNESS);
      br.Position = off;

      var commands = new GeoCommandList();

      var returnType = ReturnType.UNDEFINED;
      while (returnType == ReturnType.UNDEFINED) {
        var cmdIdByte = br.ReadByte();
        var cmdId = (GeoCommandId) cmdIdByte;
        br.Position--;

        var startPos = br.Position;
        var expectedLen = GetCmdLength_(cmdIdByte);

        IGeoCommand? command = null;

        switch (cmdId) {
          case GeoCommandId.BRANCH_AND_STORE: {
            var branchAndStoreCommand = br.ReadNew<GeoBranchAndStoreCommand>();
            command = branchAndStoreCommand;

            ReturnType branchReturnType = ReturnType.UNDEFINED;
            var commandListAndReturnValue = this.ParseImpl_(
                branchAndStoreCommand.GeoCommandSegmentedAddress,
                areaId);
            if (commandListAndReturnValue != null) {
              (branchAndStoreCommand.GeoCommandList, branchReturnType) =
                  commandListAndReturnValue.Value;
            }

            if (branchReturnType == ReturnType.TERMINATED) {
              returnType = ReturnType.TERMINATED;
            }

            break;
          }
          case GeoCommandId.TERMINATE: {
            command = br.ReadNew<GeoTerminateCommand>();
            returnType = ReturnType.TERMINATED;
            break;
          }
          case GeoCommandId.BRANCH: {
            var branchCommand = br.ReadNew<GeoBranchCommand>();
            command = branchCommand;

            ReturnType branchReturnType = ReturnType.UNDEFINED;
            var commandListAndReturnValue =
                this.ParseImpl_(branchCommand.GeoCommandSegmentedAddress,
                                areaId);

            if (commandListAndReturnValue != null) {
              (branchCommand.GeoCommandList, branchReturnType) =
                  commandListAndReturnValue.Value;
            }

            if (branchReturnType == ReturnType.TERMINATED) {
              returnType = ReturnType.TERMINATED;
            }

            if (!branchCommand.StoreReturnAddress) {
              returnType = ReturnType.RETURNED;
            }

            break;
          }
          case GeoCommandId.RETURN_FROM_BRANCH: {
            command = br.ReadNew<GeoReturnFromBranchCommand>();
            returnType = ReturnType.RETURNED;
            break;
          }
          case GeoCommandId.OPEN_NODE: {
            command = br.ReadNew<GeoOpenNodeCommand>();
            break;
          }
          case GeoCommandId.CLOSE_NODE: {
            command = br.ReadNew<GeoCloseNodeCommand>();
            break;
          }
          case GeoCommandId.VIEWPORT: {
            command = br.ReadNew<GeoViewportCommand>();
            break;
          }
          case GeoCommandId.ORTHO_MATRIX: {
            command = br.ReadNew<GeoOrthoMatrixCommand>();
            break;
          }
          case GeoCommandId.CAMERA_FRUSTUM: {
            command = br.ReadNew<GeoCameraFrustumCommand>();
            break;
          }
          case GeoCommandId.START_LAYOUT: {
            command = br.ReadNew<GeoStartLayoutCommand>();
            break;
          }
          case GeoCommandId.TOGGLE_DEPTH_BUFFER: {
            command = br.ReadNew<GeoToggleDepthBufferCommand>();
            break;
          }
          case GeoCommandId.SET_RENDER_RANGE: {
            command = br.ReadNew<GeoSetRenderRangeCommand>();
            break;
          }
          case GeoCommandId.SWITCH: {
            command = br.ReadNew<GeoSwitchCommand>();
            // TODO: How does getting cases work??
            break;
          }
          case GeoCommandId.CAMERA_LOOK_AT: {
            command = br.ReadNew<GeoCameraLookAtCommand>();
            break;
          }
          case GeoCommandId.TRANSLATE_AND_ROTATE: {
            var translateAndRotateCommand = new GeoTranslateAndRotateCommand();
            br.ReadByte();
            translateAndRotateCommand.Params = br.ReadByte();

            switch (translateAndRotateCommand.Format) {
              case GeoTranslateAndRotateFormat.TRANSLATION_AND_ROTATION: {
                br.ReadUInt16();
                translateAndRotateCommand.Translation.Read(br);
                translateAndRotateCommand.Rotation.Read(br);
                break;
              }
              case GeoTranslateAndRotateFormat.TRANSLATION: {
                translateAndRotateCommand.Translation.Read(br);
                break;
              }
              case GeoTranslateAndRotateFormat.ROTATION: {
                translateAndRotateCommand.Rotation.Read(br);
                break;
              }
              case GeoTranslateAndRotateFormat.YAW: {
                translateAndRotateCommand.Rotation.Y = br.ReadInt16();
                break;
              }
              default: throw new ArgumentOutOfRangeException();
            }

            if (translateAndRotateCommand.HasDisplayList) {
              translateAndRotateCommand.DisplayListSegmentedAddress =
                  br.ReadUInt32();
            }

            command = translateAndRotateCommand;
            break;
          }
          case GeoCommandId.TRANSLATE: {
            command = br.ReadNew<GeoTranslationCommand>();
            break;
          }
          case GeoCommandId.ROTATE: {
            command = br.ReadNew<GeoRotationCommand>();
            break;
          }
          case GeoCommandId.ANIMATED_PART: {
            command = br.ReadNew<GeoAnimatedPartCommand>();
            break;
          }
          case GeoCommandId.BILLBOARD: {
            command = br.ReadNew<GeoBillboardCommand>();
            break;
          }
          case GeoCommandId.DISPLAY_LIST: {
            command = br.ReadNew<GeoDisplayListCommand>();
            break;
          }
          case GeoCommandId.SHADOW: {
            command = br.ReadNew<GeoShadowCommand>();
            break;
          }
          case GeoCommandId.OBJECT_LIST: {
            command = br.ReadNew<GeoObjectListCommand>();
            break;
          }
          case GeoCommandId.DISPLAY_LIST_FROM_ASM: {
            command = br.ReadNew<GeoDisplayListFromAsm>();
            break;
          }
          case GeoCommandId.BACKGROUND: {
            command = br.ReadNew<GeoBackgroundCommand>();
            break;
          }
          case GeoCommandId.NOOP_1A: {
            command = br.ReadNew<GeoNoopCommand>();
            break;
          }
          case GeoCommandId.HELD_OBJECT: {
            command = br.ReadNew<GeoHeldObjectCommand>();
            break;
          }
          case GeoCommandId.SCALE: {
            command = br.ReadNew<GeoScaleCommand>();
            break;
          }
          case GeoCommandId.CULLING_RADIUS: {
            command = br.ReadNew<GeoCullingRadiusCommand>();
            break;
          }
          default: {
            throw new NotImplementedException();
          }
        }

        var actualLen = br.Position - startPos;
        if (expectedLen != actualLen) {
          var translateAndRotateCommand =
              command as GeoTranslateAndRotateCommand;
          if (translateAndRotateCommand == null) {
            Asserts.Fail();
          }
        }

        commands.AddCommand(Asserts.CastNonnull(command));
      }

      return (commands, returnType);
    }

    private static byte GetCmdLength_(byte cmd) {
      switch (cmd) {
        case 0x00:
        case 0x02:
        case 0x0D:
        case 0x0E:
        case 0x11:
        case 0x12:
        case 0x14:
        case 0x15:
        case 0x16:
        case 0x18:
        case 0x19:
        case 0x1A:
        case 0x1D:
        case 0x1E:
          return 0x08;
        case 0x08:
        case 0x0A:
        case 0x13:
        case 0x1C:
          return 0x0C;
        case 0x1F:
          return 0x10;
        case 0x0F:
        case 0x10:
          return 0x14;
        default:
          return 0x04;
      }
    }
  }
}