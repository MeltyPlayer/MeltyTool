#nullable enable


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using fin.data.dictionaries;
using fin.io;
using fin.model;
using fin.scene;
using fin.util.asserts;

namespace HaloWarsTools {
  public class HWVisResource : HWXmlResource {
    public SceneImpl Scene { get; private set; }
    public HWModel[] Models { get; private set; }

    public static new HWVisResource
        FromFile(HWContext context, string filename) {
      return (HWVisResource) GetOrCreateFromFile(
          context, filename, HWResourceType.Vis);
    }

    protected override void Load(byte[] bytes) {
      base.Load(bytes);
      this.Models = this.ImportModels_();
    }

    private HWModel[] ImportModels_() {
      var models = new List<HWModel>();

      this.Scene = new SceneImpl {
          // TODO: Fix this
          FileBundle = null,
          Files = new HashSet<IReadOnlyGenericFile>(),
      };

      var visModels = new List<VisModel>();
      var visModelMap = new Dictionary<string, VisModel>();

      var modelTags = this.XmlData.Elements("model");
      foreach (var modelTag in modelTags) {
        var modelName = modelTag.Attribute("name").Value;

        // Gets model(s).
        ISet<string> modelPaths;
        {
          var component = modelTag.Elements("component").Single();

          var logicAssets =
              component.Elements("logic")
                       .Elements("logicdata")
                       .Elements("asset");
          var directAssets = component.Elements("asset");

          modelPaths =
              logicAssets.Concat(directAssets)
                         .Where(assetTag =>
                                    assetTag.Attribute("type").Value == "Model")
                         .Elements("file")
                         .Select(file => file.Value)
                         .ToHashSet();
        }

        ISet<string> animationPaths;
        ISet<VisSubModelRef> subModelRefs;
        {
          var animTags = modelTag.Elements("anim");

          animationPaths =
              animTags.Elements("asset")
                      .Where(assetTag =>
                                 assetTag.Attribute("type").Value == "Anim")
                      .Elements("file")
                      .Select(fileTag => fileTag.Value)
                      .ToHashSet();

          subModelRefs =
              animTags.Elements("attach")
                      .Where(attachTag =>
                                 attachTag.Attribute("type").Value ==
                                 "ModelRef")
                      .Select(
                          attachTag => {
                            return new VisSubModelRef {
                                ModelName = attachTag.Attribute("name").Value,
                                ToBone = attachTag.Attribute("tobone")?.Value,
                                FromBone =
                                    attachTag.Attribute("frombone")?.Value,
                            };
                          })
                      .ToHashSet();
        }

        var visModel = new VisModel {
            Name = modelName,
            ModelPaths = modelPaths,
            AnimationPaths = animationPaths,
            SubModelRefs = subModelRefs,
        };

        visModels.Add(visModel);
        visModelMap[modelName] = visModel;
      }

      var sceneArea = this.Scene.AddArea();
      var sceneObj = sceneArea.AddObject();

      var firstModel = visModels[0];

      var modelQueue = new Queue<(VisModel, VisSubModelRef?, bool)>();
      modelQueue.Enqueue((firstModel, null, true));

      var attachmentPointMap = new NullFriendlyDictionary<string, (ISceneModel, IReadOnlyBone)>();

      while (modelQueue.Count > 0) {
        var (visModel, subModelRef, flipFaces) = modelQueue.Dequeue();

        foreach (var modelPath in visModel.ModelPaths) {
          var file = Path.Combine("art", modelPath);

          var extension = Path.GetExtension(file);
          if (extension.Length > 0 && extension != ".ugx") {
            continue;
          }

          // TODO: Sometimes models are missing, why is this??
          try {
            var ugx = HWUgxResource.FromFile(this.Context,
                file,
                flipFaces);
            var model = ugx.Mesh;

            ISceneModel sceneModel;
            var attachmentBoneName = subModelRef?.ToBone;
            if (attachmentBoneName != null) {
              var (parentSceneModel, attachmentPointBone)
                  = attachmentPointMap[attachmentBoneName];
              sceneModel
                  = parentSceneModel.AddModelOntoBone(
                      model,
                      attachmentPointBone);
            } else {
              sceneModel = sceneObj.AddSceneModel(model);
            }

            foreach (var bone in model.Skeleton) {
              attachmentPointMap[bone.Name.AssertNonnull()] = (sceneModel, bone);
            }
          } catch(Exception e) {
            ;
          }
          /*if (resource != null) {
            models.Add(
                new HWModel(visModel., resource));
          }*/
        }

        foreach (var child in visModel.SubModelRefs) {
          // Sometimes model references are missing--just ignore em.
          if (visModelMap.TryGetValue(child.ModelName, out var childModel)) {
            modelQueue.Enqueue((childModel, child, !flipFaces));
          }
        }
      }

      return models.ToArray();
    }

    internal class VisModel {
      public string Name { get; set; }
      public ISet<string> ModelPaths { get; init; } = new HashSet<string>();
      public ISet<string> AnimationPaths { get; init; } = new HashSet<string>();

      public ISet<VisSubModelRef> SubModelRefs { get; init; } =
        new HashSet<VisSubModelRef>();
    }

    public class VisSubModelRef {
      public string ModelName { get; set; }
      public string? ToBone { get; set; }
      public string? FromBone { get; set; }

      public override bool Equals(object other) {
        var subModelOther = other as VisSubModelRef;
        if (subModelOther == null) {
          return false;
        }

        return this.ModelName == subModelOther.ModelName &&
               this.ToBone == subModelOther.ToBone &&
               this.FromBone == subModelOther.FromBone;
      }

      // TODO: Super naive, will almost certainly be slow.
      public override int GetHashCode()
        => (this.ModelName + this.ToBone + this.FromBone).GetHashCode();
    }
  }
}