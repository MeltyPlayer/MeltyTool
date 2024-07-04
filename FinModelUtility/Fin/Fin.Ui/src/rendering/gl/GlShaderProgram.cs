using fin.data;
using fin.data.lazy;

using OpenTK.Graphics.OpenGL;


namespace fin.ui.rendering.gl;

public partial class GlShaderProgram : IShaderProgram {
  private bool isDisposed_;
  private readonly CachedShaderProgram cachedShaderProgram_;

  private static ReferenceCountCacheDictionary<string, int>
      vertexShaderCache_ = new(
          src => CreateAndCompileShader_(src, ShaderType.VertexShader),
          (_, id) => {
            if (id != UNDEFINED_ID) {
              GL.DeleteShader(id);
            }
          });

  private static ReferenceCountCacheDictionary<string, int>
      fragmentShaderCache_ = new(
          src => CreateAndCompileShader_(src, ShaderType.FragmentShader),
          (_, id) => {
            if (id != UNDEFINED_ID) {
              GL.DeleteShader(id);
            }
          });

  private static
      ReferenceCountCacheDictionary<(string vertexSrc, string fragmentSrc),
          CachedShaderProgram> programCache_ =
          new(vertexAndFragmentSrc => {
                var (vertexSrc, fragmentSrc) = vertexAndFragmentSrc;
                var vertexShaderId =
                    GlShaderProgram.vertexShaderCache_.GetAndIncrement(
                        vertexSrc);
                var fragmentShaderId =
                    GlShaderProgram.fragmentShaderCache_.GetAndIncrement(
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
              });

  private const int UNDEFINED_ID = -1;

  public static GlShaderProgram FromShaders(string vertexShaderSrc,
                                            string fragmentShaderSrc)
    => new(vertexShaderSrc, fragmentShaderSrc);

  private GlShaderProgram(string vertexShaderSrc,
                          string fragmentShaderSrc) {
    this.cachedShaderProgram_ =
        GlShaderProgram.programCache_.GetAndIncrement(
            (vertexShaderSrc, fragmentShaderSrc));
  }

  ~GlShaderProgram() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.isDisposed_ = true;
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_()
    => GlShaderProgram.programCache_.DecrementAndMaybeDispose(
        (this.VertexShaderSource, this.FragmentShaderSource));

  private static int CreateAndCompileShader_(string src,
                                             ShaderType shaderType) {
    var shaderId = GL.CreateShader(shaderType);
    GL.ShaderSource(shaderId, 1, [src], (int[]) null);
    GL.CompileShader(shaderId);

    // TODO: Throw/return this error
    var bufferSize = 10000;
    GL.GetShaderInfoLog(
        shaderId,
        bufferSize,
        out var shaderErrorLength,
        out var shaderError);

    if (shaderError?.Length > 0) {
      ;
    }

    return shaderId;
  }

  public int ProgramId => this.cachedShaderProgram_.ProgramId;

  public string VertexShaderSource
    => this.cachedShaderProgram_.VertexShaderSource;

  public string FragmentShaderSource
    => this.cachedShaderProgram_.FragmentShaderSource;


  public void Use() {
    if (this.isDisposed_) {
      return;
    }

    GlUtil.UseProgram(this.ProgramId);
    foreach (var uniform in this.cachedUniforms_.Values) {
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