// Decompiled with JetBrains decompiler
// Type: QuickFont.QFontDrawing
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

#nullable disable
namespace QuickFont
{
  public class QFontDrawing : IDisposable
  {
    private const string SHADER_VERSION_STRING130 = "#version 130\n\n";
    private const string SHADER_VERSION_STRING140 = "#version 140\n\n";
    private const string SHADER_VERSION_STRING150 = "#version 150\n\n";
    private static QFontSharedState _sharedState;
    public QVertexArrayObject VertexArrayObject;
    private QFontSharedState _instanceSharedState;
    private readonly List<QFontDrawingPrimitive> _glFontDrawingPrimitives;
    private readonly bool _useDefaultBlendFunction;
    private Matrix4 _projectionMatrix;
    private bool _disposed;

    public QFontDrawing(bool useDefaultBlendFunction = true, QFontSharedState state = null)
    {
      this._useDefaultBlendFunction = useDefaultBlendFunction;
      this._glFontDrawingPrimitives = new List<QFontDrawingPrimitive>();
      this.InitialiseState(state);
    }

    public static QFontSharedState SharedState => QFontDrawing._sharedState;

    public QFontSharedState InstanceSharedState
    {
      get => this._instanceSharedState ?? QFontDrawing.SharedState;
    }

    public Matrix4 ProjectionMatrix
    {
      get => this._projectionMatrix;
      set => this._projectionMatrix = value;
    }

    public List<QFontDrawingPrimitive> DrawingPrimitives => this._glFontDrawingPrimitives;

    private static string LoadShaderFromResource(string path)
    {
      Assembly executingAssembly = Assembly.GetExecutingAssembly();
      path = ((IEnumerable<string>) executingAssembly.GetManifestResourceNames()).Where<string>((Func<string, bool>) (f => f.EndsWith(path))).First<string>();
      Stream manifestResourceStream = executingAssembly.GetManifestResourceStream(path);
      if (manifestResourceStream == null)
        throw new AccessViolationException("Error loading shader resource");
      using (StreamReader streamReader = new StreamReader(manifestResourceStream))
        return streamReader.ReadToEnd();
    }

    private static void InitialiseStaticState()
    {
      GL.Enable(EnableCap.Texture2D);
      int shader1 = GL.CreateShader(ShaderType.VertexShader);
      int shader2 = GL.CreateShader(ShaderType.FragmentShader);
      if (shader1 == -1 || shader2 == -1)
        throw new Exception(string.Format("Error creating shader name for {0}", shader1 == -1 ? (shader2 == -1 ? (object) "vert and frag shaders" : (object) "vert shader") : (object) "frag shader"));
      string[] strArray = new string[3]
      {
        "#version 130\n\n",
        "#version 140\n\n",
        "#version 150\n\n"
      };
      int params1 = 0;
      int params2 = 0;
      foreach (string str in strArray)
      {
        GL.ShaderSource(shader1, str + QFontDrawing.LoadShaderFromResource("simple.vs"));
        GL.ShaderSource(shader2, str + QFontDrawing.LoadShaderFromResource("simple.fs"));
        GL.CompileShader(shader1);
        GL.CompileShader(shader2);
        GL.GetShader(shader1, ShaderParameter.CompileStatus, out params1);
        GL.GetShader(shader2, ShaderParameter.CompileStatus, out params2);
        if (params1 != 0 && params2 != 0)
          break;
      }
      if (params1 == 0 || params2 == 0)
        throw new Exception(string.Format("Shaders were not compiled correctly. Info logs are\nVert:\n{0}\nFrag:\n{1}", (object) GL.GetShaderInfoLog(shader1), (object) GL.GetShaderInfoLog(shader2)));
      int program = GL.CreateProgram();
      GL.AttachShader(program, shader1);
      GL.AttachShader(program, shader2);
      GL.LinkProgram(program);
      int params3;
      GL.GetProgram(program, GetProgramParameterName.LinkStatus, out params3);
      if (params3 == 0)
        throw new Exception(string.Format("Program was not linked correctly. Info log is\n{0}", (object) GL.GetProgramInfoLog(program)));
      GL.DetachShader(program, shader1);
      GL.DetachShader(program, shader2);
      GL.DeleteShader(shader1);
      GL.DeleteShader(shader2);
      int uniformLocation1 = GL.GetUniformLocation(program, "proj_matrix");
      int uniformLocation2 = GL.GetUniformLocation(program, "modelview_matrix");
      int uniformLocation3 = GL.GetUniformLocation(program, "tex_object");
      int attribLocation1 = GL.GetAttribLocation(program, "in_position");
      int attribLocation2 = GL.GetAttribLocation(program, "in_tc");
      int attribLocation3 = GL.GetAttribLocation(program, "in_colour");
      QFontDrawing._sharedState = new QFontSharedState(TextureUnit.Texture0, new ShaderLocations()
      {
        ShaderProgram = program,
        ProjectionMatrixUniformLocation = uniformLocation1,
        ModelViewMatrixUniformLocation = uniformLocation2,
        TextureCoordAttribLocation = attribLocation2,
        PositionCoordAttribLocation = attribLocation1,
        SamplerLocation = uniformLocation3,
        ColorCoordAttribLocation = attribLocation3
      });
    }

    private void InitialiseState(QFontSharedState state)
    {
      if (state != null)
      {
        this._instanceSharedState = state;
      }
      else
      {
        if (QFontDrawing.SharedState != null)
          return;
        QFontDrawing.InitialiseStaticState();
      }
    }

    public void Draw()
    {
      GL.UseProgram(this.InstanceSharedState.ShaderVariables.ShaderProgram);
      if (this._useDefaultBlendFunction)
      {
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
      }
      GL.UniformMatrix4(this.InstanceSharedState.ShaderVariables.ProjectionMatrixUniformLocation, false, ref this._projectionMatrix);
      GL.Uniform1(this.InstanceSharedState.ShaderVariables.SamplerLocation, 0);
      GL.ActiveTexture(this.InstanceSharedState.DefaultTextureUnit);
      int first = 0;
      this.VertexArrayObject.Bind();
      foreach (QFontDrawingPrimitive drawingPrimitive in this._glFontDrawingPrimitives)
      {
        GL.UniformMatrix4(this.InstanceSharedState.ShaderVariables.ModelViewMatrixUniformLocation, false, ref drawingPrimitive.ModelViewMatrix);
        PrimitiveType mode = PrimitiveType.Triangles;
        GL.ActiveTexture(QFontDrawing.SharedState.DefaultTextureUnit);
        if (drawingPrimitive.ShadowVertexRepr.Count > 0)
        {
          GL.BindTexture(TextureTarget.Texture2D, drawingPrimitive.Font.FontData.DropShadowFont.FontData.Pages[0].TextureID);
          GL.DrawArrays(mode, first, drawingPrimitive.ShadowVertexRepr.Count);
          first += drawingPrimitive.ShadowVertexRepr.Count;
        }
        GL.BindTexture(TextureTarget.Texture2D, drawingPrimitive.Font.FontData.Pages[0].TextureID);
        GL.DrawArrays(mode, first, drawingPrimitive.CurrentVertexRepr.Count);
        first += drawingPrimitive.CurrentVertexRepr.Count;
      }
      GL.BindVertexArray(0);
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }

    public void DisableShader()
    {
      GL.UseProgram(0);
      this.VertexArrayObject.DisableAttributes();
    }

    public static void RefreshViewport() => ViewportHelper.InvalidateViewport();

    public void RefreshBuffers()
    {
      if (this.VertexArrayObject == null)
        this.VertexArrayObject = new QVertexArrayObject(QFontDrawing.SharedState);
      this.VertexArrayObject.Reset();
      foreach (QFontDrawingPrimitive drawingPrimitive in this._glFontDrawingPrimitives)
      {
        this.VertexArrayObject.AddVertexes(drawingPrimitive.ShadowVertexRepr);
        this.VertexArrayObject.AddVertexes(drawingPrimitive.CurrentVertexRepr);
      }
      this.VertexArrayObject.Load();
    }

    public SizeF Print(QFont font, ProcessedText text, Vector3 position, QFontRenderOptions opt)
    {
      QFontDrawingPrimitive drawingPrimitive = new QFontDrawingPrimitive(font, opt);
      this.DrawingPrimitives.Add(drawingPrimitive);
      return drawingPrimitive.Print(text, position, opt.ClippingRectangle);
    }

    public SizeF Print(QFont font, ProcessedText processedText, Vector3 position, Color? colour = null)
    {
      QFontDrawingPrimitive drawingPrimitive = new QFontDrawingPrimitive(font);
      this.DrawingPrimitives.Add(drawingPrimitive);
      return !colour.HasValue ? drawingPrimitive.Print(processedText, position) : drawingPrimitive.Print(processedText, position, colour.Value);
    }

    public SizeF Print(
      QFont font,
      string text,
      Vector3 position,
      QFontAlignment alignment,
      QFontRenderOptions opt)
    {
      QFontDrawingPrimitive drawingPrimitive = new QFontDrawingPrimitive(font, opt);
      this.DrawingPrimitives.Add(drawingPrimitive);
      return drawingPrimitive.Print(text, position, alignment, opt.ClippingRectangle);
    }

    public SizeF Print(
      QFont font,
      string text,
      Vector3 position,
      QFontAlignment alignment,
      Color? color = null,
      Rectangle clippingRectangle = default (Rectangle))
    {
      QFontDrawingPrimitive drawingPrimitive = new QFontDrawingPrimitive(font);
      this.DrawingPrimitives.Add(drawingPrimitive);
      return color.HasValue ? drawingPrimitive.Print(text, position, alignment, color.Value, clippingRectangle) : drawingPrimitive.Print(text, position, alignment, clippingRectangle);
    }

    public SizeF Print(
      QFont font,
      string text,
      Vector3 position,
      SizeF maxSize,
      QFontAlignment alignment,
      Rectangle clippingRectangle = default (Rectangle))
    {
      QFontDrawingPrimitive drawingPrimitive = new QFontDrawingPrimitive(font);
      this.DrawingPrimitives.Add(drawingPrimitive);
      return drawingPrimitive.Print(text, position, maxSize, alignment, clippingRectangle);
    }

    public SizeF Print(
      QFont font,
      string text,
      Vector3 position,
      SizeF maxSize,
      QFontAlignment alignment,
      QFontRenderOptions opt)
    {
      QFontDrawingPrimitive drawingPrimitive = new QFontDrawingPrimitive(font, opt);
      this.DrawingPrimitives.Add(drawingPrimitive);
      return drawingPrimitive.Print(text, position, maxSize, alignment, opt.ClippingRectangle);
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this._disposed)
        return;
      if (disposing && this.VertexArrayObject != null)
      {
        this.VertexArrayObject.Dispose();
        this.VertexArrayObject = (QVertexArrayObject) null;
      }
      this._disposed = true;
    }
  }
}
