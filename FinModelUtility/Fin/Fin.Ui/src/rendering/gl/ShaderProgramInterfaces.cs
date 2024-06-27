using System.Numerics;


namespace fin.ui.rendering.gl;

public interface IShaderUniform<T> {
  void SetAndMarkDirty(in T value);
  void SetAndMaybeMarkDirty(in T value);
}

public interface IShaderUniformArray<T> {
  void SetAndMarkDirty(int index, in T value);
  void SetAndMaybeMarkDirty(int index, in T value);
}

public interface IShaderProgram : IDisposable {
  IShaderUniform<bool> GetUniformBool(string name);
  IShaderUniform<int> GetUniformInt(string name);
  IShaderUniform<float> GetUniformFloat(string name);
  IShaderUniform<Vector2> GetUniformVec2(string name);
  IShaderUniform<Vector3> GetUniformVec3(string name);
  IShaderUniform<Vector4> GetUniformVec4(string name);
  IShaderUniform<Matrix3x2> GetUniformMat3x2(string name);
  IShaderUniform<Matrix4x4> GetUniformMat4(string name);
  IShaderUniformArray<Matrix4x4> GetUniformMat4s(string name, int length);
}