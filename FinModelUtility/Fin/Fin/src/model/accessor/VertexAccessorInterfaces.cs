namespace fin.model.accessor;

public interface IVertexTargeter {
  void Target(IReadOnlyVertex vertex);
}

public interface IVertexAccessor<out TVertexAccessor>
    : IVertexNormalAccessor,
      IVertexTangentAccessor,
      IVertexColorAccessor,
      IVertexUvAccessor 
    where TVertexAccessor : IVertexAccessor<TVertexAccessor> {
  static abstract TVertexAccessor GetAccessorForModel(IReadOnlyModel model);
}

public interface IVertexNormalAccessor : IVertexTargeter,
                                         IReadOnlyNormalVertex;

public interface IVertexTangentAccessor : IVertexTargeter,
                                          IReadOnlyTangentVertex;

public interface IVertexColorAccessor : IVertexTargeter,
                                        IReadOnlySingleColorVertex,
                                        IReadOnlyMultiColorVertex;

public interface IVertexUvAccessor : IVertexTargeter,
                                     IReadOnlySingleUvVertex,
                                     IReadOnlyMultiUvVertex;