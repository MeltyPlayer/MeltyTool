namespace fin.shaders.glsl;

public interface IGlslStringBuilder {
  void AddStruct(string text);

  void AddBuffer();

  void AddUniform(string type, string name);
  
  void AddIn(string type, string name);
  void AddOut(string type, string name);

  void AddMethod(string text);
}