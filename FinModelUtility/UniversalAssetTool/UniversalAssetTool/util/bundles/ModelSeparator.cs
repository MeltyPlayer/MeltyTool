using fin.io;
using fin.util.asserts;

namespace uni.util.bundles {
  public interface IModelBundle {
    IFileHierarchyFile ModelFile { get; }
    IList<IFileHierarchyFile> AnimationFiles { get; }
  }

  public interface IModelSeparatorMethod {
    IEnumerable<IModelBundle> Separate(
        IList<IFileHierarchyFile> modelFiles,
        IList<IFileHierarchyFile> animationFiles);
  }

  public interface IModelSeparator {
    IModelSeparator Register(
        string directoryId,
        IModelSeparatorMethod method);

    IModelSeparator Register(
        IModelSeparatorMethod method,
        params string[] directoryIds);

    bool Contains(IFileHierarchyDirectory directory);

    IEnumerable<IModelBundle> Separate(
        IFileHierarchyDirectory directory,
        IList<IFileHierarchyFile> modelFiles,
        IList<IFileHierarchyFile> animationFiles);
  }

  public class ModelSeparator : IModelSeparator {
    private readonly Func<IFileHierarchyDirectory, string> directoryToId_;
    private readonly Dictionary<string, IModelSeparatorMethod> impl_ = new();

    public ModelSeparator(Func<IFileHierarchyDirectory, string> directoryToId)
      => this.directoryToId_ = directoryToId;

    public IModelSeparator Register(
        string directoryId,
        IModelSeparatorMethod method) {
      this.impl_[directoryId] = method;
      return this;
    }

    public IModelSeparator Register(
        IModelSeparatorMethod method,
        params string[] directoryIds) {
      foreach (var directoryId in directoryIds) {
        this.Register(directoryId, method);
      }

      return this;
    }

    public bool Contains(IFileHierarchyDirectory directory)
      => this.impl_.ContainsKey(this.directoryToId_(directory));

    public IEnumerable<IModelBundle> Separate(
        IFileHierarchyDirectory directory,
        IList<IFileHierarchyFile> modelFiles,
        IList<IFileHierarchyFile> animationFiles) {
      if (modelFiles.Count == 1) {
        return new[] { new ModelBundle(modelFiles[0], animationFiles) };
      }

      if (animationFiles.Count == 0) {
        return modelFiles
               .Select(modelFile => new ModelBundle(modelFile, animationFiles))
               .ToArray();
      }

      return this.impl_[this.directoryToId_(directory)]
                 .Separate(modelFiles, animationFiles);
    }
  }

  public class NoAnimationsModelSeparatorMethod
      : IModelSeparatorMethod {
    public IEnumerable<IModelBundle> Separate(
        IList<IFileHierarchyFile> modelFiles,
        IList<IFileHierarchyFile> animationFiles)
      => modelFiles
          .Select(
              modelFile => new ModelBundle(
                  modelFile,
                  Array.Empty<IFileHierarchyFile>()));
  }

  public class AllAnimationsModelSeparatorMethod
      : IModelSeparatorMethod {
    public IEnumerable<IModelBundle> Separate(
        IList<IFileHierarchyFile> modelFiles,
        IList<IFileHierarchyFile> animationFiles)
      => modelFiles
          .Select(
              modelFile => new ModelBundle(
                  modelFile,
                  animationFiles));
  }

  public class PrimaryModelSeparatorMethod : IModelSeparatorMethod {
    private readonly string primaryModelName_;

    public PrimaryModelSeparatorMethod(string primaryModelName) {
      this.primaryModelName_ = primaryModelName;
    }

    public IEnumerable<IModelBundle> Separate(
        IList<IFileHierarchyFile> modelFiles,
        IList<IFileHierarchyFile> animationFiles) {
      return new[] {
          new ModelBundle(
              modelFiles.Single(file => file.Name == this.primaryModelName_),
              animationFiles)
      }.Concat(modelFiles
               .Where(file => file.Name != this.primaryModelName_)
               .Select(modelFile
                           => new ModelBundle(modelFile,
                                              new IFileHierarchyFile[] { })));
    }
  }

  public abstract class BUnclaimedMatchModelSeparatorMethod
      : IModelSeparatorMethod {
    public virtual IList<IFileHierarchyFile> PreprocessModelFiles(
        IList<IFileHierarchyFile> modelFiles) => modelFiles;

    public virtual IList<IFileHierarchyFile> PreprocessAnimationFiles(
        IList<IFileHierarchyFile> animationFiles) => animationFiles;

    public IEnumerable<IModelBundle> Separate(
        IList<IFileHierarchyFile> modelFiles,
        IList<IFileHierarchyFile> animationFiles) {
      var processedModelFiles = this.PreprocessModelFiles(modelFiles);
      var processedAnimationFiles =
          this.PreprocessAnimationFiles(animationFiles);

      var modelFilesToAnimationFiles =
          new Dictionary<IFileHierarchyFile, IList<IFileHierarchyFile>>();
      var unclaimedAnimationFiles = processedAnimationFiles.ToHashSet();
      foreach (var modelFile in processedModelFiles) {
        var claimedAnimationFiles =
            this.GetAnimationsForModel(modelFile, processedAnimationFiles)
                .ToArray();
        modelFilesToAnimationFiles[modelFile] = claimedAnimationFiles;

        foreach (var claimedAnimationFile in claimedAnimationFiles) {
          unclaimedAnimationFiles.Remove(claimedAnimationFile);
        }
      }

      Asserts.Equal(0, unclaimedAnimationFiles.Count);

      return modelFilesToAnimationFiles
          .Select(kvp => new ModelBundle(kvp.Key, kvp.Value));
    }

    public abstract IEnumerable<IFileHierarchyFile> GetAnimationsForModel(
        IFileHierarchyFile modelFile,
        IList<IFileHierarchyFile> animationFiles);
  }

  public class ExactCasesMethod : BUnclaimedMatchModelSeparatorMethod {
    private Dictionary<string, ISet<string>> impl_ = new();

    public ExactCasesMethod Case(
        string modelFileName,
        params string[] animationFileNames) {
      this.impl_[modelFileName] = animationFileNames.ToHashSet();
      return this;
    }

    public override IEnumerable<IFileHierarchyFile> GetAnimationsForModel(
        IFileHierarchyFile modelFile,
        IList<IFileHierarchyFile> animationFiles) {
      if (this.impl_.TryGetValue(modelFile.Name, out var animationFileNames)) {
        return animationFiles
            .Where(file => animationFileNames.Contains(file.Name));
      }

      return Enumerable.Empty<IFileHierarchyFile>();
    }
  }

  public class PrefixCasesMethod : BUnclaimedMatchModelSeparatorMethod {
    private Dictionary<string, IList<string>> impl_ = new();

    public PrefixCasesMethod Case(
        string modelFilePrefix,
        params string[] animationFilePrefixes) {
      this.impl_[modelFilePrefix] = animationFilePrefixes;
      return this;
    }

    public override IEnumerable<IFileHierarchyFile> GetAnimationsForModel(
        IFileHierarchyFile modelFile,
        IList<IFileHierarchyFile> animationFiles) {
      foreach (var kvp in this.impl_) {
        var (modelFilePrefix, animationFilePrefixes) = kvp;
        if (modelFile.Name.StartsWith(modelFilePrefix)) {
          return animationFiles
              .Where(file => animationFilePrefixes.Any(
                         prefix => file.Name.StartsWith(prefix)));
        }
      }

      return Enumerable.Empty<IFileHierarchyFile>();
    }
  }

  /*public class BoneCountMethod : BUnclaimedMatchModelSeparatorMethod {
    private readonly Func<IFileHierarchyFile, int> getBoneCountFromModelFile_;

    private readonly Func<IFileHierarchyFile, int>
        getBoneCountFromAnimationFile_;

    private readonly Dictionary<int, IList<IFileHierarchyFile>>
        animationsByBoneCount_ = new();

    public BoneCountMethod(
        Func<IFileHierarchyFile, int> getBoneCountFromModelFile,
        Func<IFileHierarchyFile, int> getBoneCountFromAnimationFile) {
      this.getBoneCountFromModelFile_ = getBoneCountFromModelFile;
      this.getBoneCountFromAnimationFile_ = getBoneCountFromAnimationFile;
    }

    public virtual IList<IFileHierarchyFile> PreprocessAnimationFiles(
        IList<IFileHierarchyFile> animationFiles) {
      foreach (var animationFile in animationFiles) {
        var boneCount = this.getBoneCountFromAnimationFile_(animationFile);
        if (!this.animationsByBoneCount_.TryGetValue(
                out var animationsWithBoneCount)) {
          this.animationsByBoneCount_
        }
        this.animationsByBoneCount_[animationFile] =
      }

      return animationFiles;
    }

    public override IList<IFileHierarchyFile> GetAnimationsForModel(
        IFileHierarchyFile modelFile,
        IList<IFileHierarchyFile> animationFiles) {
      foreach (var kvp in this.impl_) {
        var (modelFilePrefix, animationFilePrefixes) = kvp;
        if (modelFile.Name.StartsWith(modelFilePrefix)) {
          return animationFiles
                 .Where(file => animationFilePrefixes.Any(
                            prefix => file.Name.StartsWith(prefix)))
                 .ToArray();
        }
      }

      return Array.Empty<IFileHierarchyFile>();
    }
  }*/

  public class PrefixModelSeparatorMethod
      : BUnclaimedMatchModelSeparatorMethod {
    public override IList<IFileHierarchyFile> PreprocessModelFiles(
        IList<IFileHierarchyFile> modelFiles)
      => modelFiles
         .OrderByDescending(file => file.NameWithoutExtension.Length)
         .ToArray();

    public override IEnumerable<IFileHierarchyFile> GetAnimationsForModel(
        IFileHierarchyFile modelFile,
        IList<IFileHierarchyFile> animationFiles) {
      var prefix = modelFile.NameWithoutExtension;
      return animationFiles.Where(file => file.Name.StartsWith(prefix));
    }
  }

  public class SameNameSeparatorMethod
      : BUnclaimedMatchModelSeparatorMethod {
    public override IEnumerable<IFileHierarchyFile> GetAnimationsForModel(
        IFileHierarchyFile modelFile,
        IList<IFileHierarchyFile> animationFiles) {
      var prefix = modelFile.NameWithoutExtension;
      return animationFiles
             .Where(file => file.NameWithoutExtension.Equals(prefix));
    }
  }


  public class NameModelSeparatorMethod
      : BUnclaimedMatchModelSeparatorMethod {
    private readonly string name_;

    public NameModelSeparatorMethod(string name) => this.name_ = name.ToLower();

    public override IEnumerable<IFileHierarchyFile> GetAnimationsForModel(
        IFileHierarchyFile modelFile,
        IList<IFileHierarchyFile> animationFiles)
      => modelFile.Name.ToLower().Contains(this.name_)
          ? animationFiles
          : Enumerable.Empty<IFileHierarchyFile>();
  }

  public class SuffixModelSeparatorMethod
      : BUnclaimedMatchModelSeparatorMethod {
    private readonly int suffixLength_;

    public SuffixModelSeparatorMethod(int suffixLength)
      => this.suffixLength_ = suffixLength;

    public override IEnumerable<IFileHierarchyFile> GetAnimationsForModel(
        IFileHierarchyFile modelFile,
        IList<IFileHierarchyFile> animationFiles) {
      var suffix =
          modelFile.NameWithoutExtension.Substring(
              modelFile.NameWithoutExtension.Length -
              this.suffixLength_);
      
      return animationFiles.Where(file => file.Name.StartsWith(suffix));
    }
  }

  public class ModelBundle : IModelBundle {
    public ModelBundle(
        IFileHierarchyFile modelFile,
        IList<IFileHierarchyFile> animationFiles) {
      this.ModelFile = modelFile;
      this.AnimationFiles = animationFiles;
    }

    public IFileHierarchyFile ModelFile { get; }
    public IList<IFileHierarchyFile> AnimationFiles { get; }
  }
}