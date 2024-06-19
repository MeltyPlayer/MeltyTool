using System.Collections.Generic;
using System.IO;
using TextMateSharp.Grammars;
using TextMateSharp.Internal.Grammars.Reader;
using TextMateSharp.Internal.Types;
using TextMateSharp.Registry;

using TextMateSharp.Themes;

namespace uni.ui.avalonia.resources.model.materials {
  /// <summary>
  ///   Shamelessly stolen from:
  ///   - https://github.com/nesrak1/UABEA/blob/5adb448deeefa1b88881f1fa44243009b352db3a/UABEAvalonia/TextHighlighting/UABEDumpRegistryOptions.cs#L18
  ///   - https://github.com/polym0rph/GLSL.tmbundle/blob/master/Syntaxes/GLSL.tmLanguage
  /// </summary>
  public class GlslRegistryOptions : IRegistryOptions {
    private readonly RegistryOptions impl_ = new(ThemeName.DarkPlus);

    public IRawGrammar GetGrammar(string _) {
      var ms = new MemoryStream();
      var sw = new StreamWriter(ms);
      sw.Write("""
                 {
                   "name": "GLSL",
                   "scopeName": "source.glsl",
                   "fileTypes": [".glsl"],
                   "patterns": [
                     {
                       "name": "comment.line.version.glsl",
                       "match": "#version[ \t]+[0-9]+"
                     },
                     {
                       "name": "keyword.control.glsl",
                       "match": "\\b(break|case|continue|default|discard|do|else|for|if|return|switch|while)\\b"
                     },
                     {
                       "name": "storage.type.glsl",
                       "match": "\\b(void|bool|u?int|float|[biu]?vec[2-4]|mat[2-4]|mat2x2|mat2x3|mat2x4|mat3x2|mat3x3|mat3x4|mat4x2|mat4x3|mat4x4|sampler[1-3]D|samplerCube|sampler2DRect|sampler[12]DShadow|sampler2DRectShadow|sampler[12]DArray|sampler[12]DArrayShadow|samplerBuffer|sampler2DMS|sampler2DMSArray|struct|isampler[1|2|3]D|isamplerCube|isampler2DRect|isampler[1|2]DArray|isamplerBuffer|isampler2DMS|isampler2DMSArray|usampler[1|2|3]D|usamplerCube|usampler2DRect|usampler[1|2]DArray|usamplerBuffer|usampler2DMS|usampler2DMSArray)\\b"
                     },
                     {
                       "name": "storage.modifier.glsl",
                       "match": "\\b(attribute|centroid|const|flat|in|inout|invariant|noperspective|out|smooth|uniform|varying)\\b"
                     },
                     {
                       "name": "support.variable.glsl",
                       "match": "\\b(gl_BackColor|gl_BackLightModelProduct|gl_BackLightProduct|gl_BackMaterial|gl_BackSecondaryColor|gl_ClipDistance|gl_ClipPlane|gl_ClipVertex|gl_Color|gl_DepthRange|gl_DepthRangeParameters|gl_EyePlaneQ|gl_EyePlaneR|gl_EyePlaneS|gl_EyePlaneT|gl_Fog|gl_FogCoord|gl_FogFragCoord|gl_FogParameters|gl_FragColor|gl_FragCoord|gl_FragDat|gl_FragDept|gl_FrontColor|gl_FrontFacing|gl_FrontLightModelProduct|gl_FrontLightProduct|gl_FrontMaterial|gl_FrontSecondaryColor|gl_InstanceID|gl_Layer|gl_LightModel|gl_LightModelParameters|gl_LightModelProducts|gl_LightProducts|gl_LightSource|gl_LightSourceParameters|gl_MaterialParameters|gl_ModelViewMatrix|gl_ModelViewMatrixInverse|gl_ModelViewMatrixInverseTranspose|gl_ModelViewMatrixTranspose|gl_ModelViewProjectionMatrix|gl_ModelViewProjectionMatrixInverse|gl_ModelViewProjectionMatrixInverseTranspose|gl_ModelViewProjectionMatrixTranspose|gl_MultiTexCoord[0-7]|gl_Normal|gl_NormalMatrix|gl_NormalScale|gl_ObjectPlaneQ|gl_ObjectPlaneR|gl_ObjectPlaneS|gl_ObjectPlaneT|gl_Point|gl_PointCoord|gl_PointParameters|gl_PointSize|gl_Position|gl_PrimitiveIDIn|gl_ProjectionMatrix|gl_ProjectionMatrixInverse|gl_ProjectionMatrixInverseTranspose|gl_ProjectionMatrixTranspose|gl_SecondaryColor|gl_TexCoord|gl_TextureEnvColor|gl_TextureMatrix|gl_TextureMatrixInverse|gl_TextureMatrixInverseTranspose|gl_TextureMatrixTranspose|gl_Vertex|gl_VertexIDh)\\b"
                     },
                     {
                       "name": "support.constant.glsl",
                       "match": "\\b(gl_MaxClipPlanes|gl_MaxCombinedTextureImageUnits|gl_MaxDrawBuffers|gl_MaxFragmentUniformComponents|gl_MaxLights|gl_MaxTextureCoords|gl_MaxTextureImageUnits|gl_MaxTextureUnits|gl_MaxVaryingFloats|gl_MaxVertexAttribs|gl_MaxVertexTextureImageUnits|gl_MaxVertexUniformComponents)\\b"
                     },
                     {
                       "name": "support.function.glsl",
                       "match": "\\b(abs|acos|all|any|asin|atan|ceil|clamp|cos|cross|degrees|dFdx|dFdy|distance|dot|equal|exp|exp2|faceforward|floor|fract|ftransform|fwidth|greaterThan|greaterThanEqual|inversesqrt|length|lessThan|lessThanEqual|log|log2|matrixCompMult|max|min|mix|mod|noise[1-4]|normalize|not|notEqual|outerProduct|pow|radians|reflect|refract|shadow1D|shadow1DLod|shadow1DProj|shadow1DProjLod|shadow2D|shadow2DLod|shadow2DProj|shadow2DProjLod|sign|sin|smoothstep|sqrt|step|tan|texture1D|texture1DLod|texture1DProj|texture1DProjLod|texture2D|texture2DLod|texture2DProj|texture2DProjLod|texture3D|texture3DLod|texture3DProj|texture3DProjLod|textureCube|textureCubeLod|transpose)\\b"
                     },
                     {
                       "name": "invalid.illegal.glsl",
                       "match": "\\b(asm|double|enum|extern|goto|inline|long|short|sizeof|static|typedef|union|unsigned|volatile)\\b"
                     },
                     { "include": "source.c" }
                   ]
                 }
                 """);
      sw.Flush();
      ms.Position = 0;

      var sr = new StreamReader(ms);
      return GrammarReader.ReadGrammarSync(sr);
    }


    public IRawTheme GetDefaultTheme() => this.impl_.GetDefaultTheme();

    public ICollection<string> GetInjections(string scopeName)
      => this.impl_.GetInjections(scopeName);

    public IRawTheme? GetTheme(string scopeName)
      => this.impl_.GetTheme(scopeName);
  }
}
