using System.Text;

using fin.data;
using fin.data.lazy;
using fin.util.asserts;

using OpenTK.Graphics.OpenGL4;


namespace fin.ui.rendering.gl;

public sealed partial class GlShaderProgram : IShaderProgram {
  private const bool ASSERT_COMPILATION = true;
  private const bool ASSERT_LINK = true;

  private bool isDisposed_;
  private readonly CachedShaderProgram cachedShaderProgram_;

  private BShaderUniform[]? cachedUniforms_;

  private static ReferenceCountCacheDictionary<string, int>
      vertexShaderCache_ = new(
          src => CreateAndCompileShader_(src, ShaderType.VertexShader),
          (_, id) => {
            if (id != UNDEFINED_ID) {
              GL.DeleteShader(id);
            }
          },
          count => DebugService.VertexShaderCount = count);

  private static ReferenceCountCacheDictionary<string, int>
      fragmentShaderCache_ = new(
          src => CreateAndCompileShader_(src, ShaderType.FragmentShader),
          (_, id) => {
            if (id != UNDEFINED_ID) {
              GL.DeleteShader(id);
            }
          },
          count => DebugService.FragmentShaderCount = count);

  private static
      ReferenceCountCacheDictionary<(string vertexSrc, string fragmentSrc),
          CachedShaderProgram> programCache_ =
          new(vertexAndFragmentSrc => {
                var (vertexSrc, fragmentSrc) = vertexAndFragmentSrc;
                var vertexShaderId =
                    vertexShaderCache_.GetAndIncrement(
                        vertexSrc);
                var fragmentShaderId =
                    fragmentShaderCache_.GetAndIncrement(
                        fragmentSrc);

                var programId = GL.CreateProgram();

                GL.AttachShader(programId, vertexShaderId);
                GL.AttachShader(programId, fragmentShaderId);
                GL.LinkProgram(programId);

                return new CachedShaderProgram {
                    ProgramId = programId,
                    VertexShaderSource = vertexSrc,
                    FragmentShaderSource = fragmentSrc,
                };
              },
              (vertexAndFragmentSrc, cachedShaderProgram) => {
                GL.DeleteProgram(cachedShaderProgram.ProgramId);

                var (vertexSrc, fragmentSrc) = vertexAndFragmentSrc;
                vertexShaderCache_.DecrementAndMaybeDispose(vertexSrc);
                fragmentShaderCache_.DecrementAndMaybeDispose(fragmentSrc);
              },
              count => DebugService.ProgramCount = count);

  private const int UNDEFINED_ID = -1;

  public static GlShaderProgram FromShaders(string vertexShaderSrc,
                                            string fragmentShaderSrc)
    => new(vertexShaderSrc, fragmentShaderSrc);

  private GlShaderProgram(string vertexShaderSrc,
                          string fragmentShaderSrc) {
    this.cachedShaderProgram_ =
        programCache_.GetAndIncrement((vertexShaderSrc, fragmentShaderSrc));
  }

  ~GlShaderProgram() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.isDisposed_ = true;
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    // TODO: How on earth is this possible???
    if (this.cachedShaderProgram_ == null) {
      return;
    }

    programCache_.DecrementAndMaybeDispose(
        (this.VertexShaderSource, this.FragmentShaderSource));
  }

  private static int CreateAndCompileShader_(string src,
                                             ShaderType shaderType) {
    var shaderId = GL.CreateShader(shaderType);
    GL.ShaderSource(shaderId, 1, [src], (int[]) null);
    GL.CompileShader(shaderId);

    if (ASSERT_COMPILATION) {
      GL.GetShader(shaderId,
                   ShaderParameter.CompileStatus,
                   out var compileStatus);

      if (compileStatus == 0) {
        var errorSb = new StringBuilder("Failed to compile shader: ");

        GL.GetShader(shaderId,
                     ShaderParameter.InfoLogLength,
                     out var infoLogLength);
        GL.GetShaderInfoLog(
            shaderId,
            infoLogLength,
            out _,
            out var compileError);
        errorSb.Append(compileError);

        Asserts.Fail(errorSb.ToString());
      }
    }

    return shaderId;
  }

  public int ProgramId => this.cachedShaderProgram_.ProgramId;
  public override string ToString() => $"Program #{this.ProgramId}";

  public string VertexShaderSource
    => this.cachedShaderProgram_.VertexShaderSource;

  public string FragmentShaderSource
    => this.cachedShaderProgram_.FragmentShaderSource;

  private bool hasBeenChecked_ = false;

  public void Use() {
    if (this.isDisposed_) {
      return;
    }

    if (!this.hasBeenChecked_) {
      this.hasBeenChecked_ = true;

      if (ASSERT_LINK) {
        GL.GetProgram(
            this.ProgramId,
            GetProgramParameterName.LinkStatus,
            out var linkStatus);

        if (linkStatus == 0) {
          var errorSb
              = new StringBuilder("Failed to link shader program: ");

          GL.GetProgram(
              this.ProgramId,
              GetProgramParameterName.InfoLogLength,
              out var infoLogLength);
          GL.GetProgramInfoLog(
              this.ProgramId,
              infoLogLength,
              out _,
              out var linkError);
          errorSb.Append(linkError);

          Asserts.Fail(errorSb.ToString());
        }
      }
    }

    this.cachedUniforms_ ??= this.cachedUniformsByName_.Values.ToArray();

    GlUtil.UseProgram(this.ProgramId);
    foreach (var uniform in this.cachedUniforms_) {
      uniform.PassValueToProgramIfDirty();
    }
  }

  private class CachedShaderProgram {
    private readonly LazyDictionary<string, int> lazyUniforms_;

    public CachedShaderProgram() {
      this.lazyUniforms_ = new(uniformName
                                   => GL.GetUniformLocation(
                                       this.ProgramId,
                                       uniformName));
    }

    public required int ProgramId { get; init; }
    public required string VertexShaderSource { get; init; }
    public required string FragmentShaderSource { get; init; }

    public int GetUniformLocation(string uniformName)
      => this.lazyUniforms_[uniformName];
  }
}