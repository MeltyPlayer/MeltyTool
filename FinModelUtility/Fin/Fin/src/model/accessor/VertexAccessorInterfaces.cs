namespace fin.model.accessor {
  public interface IVertexTargeter {
    void Target(IReadOnlyVertex vertex);
  }

  public interface IVertexAccessor
      : IVertexNormalAccessor,
        IVertexTangentAccessor,
        IVertexColorAccessor,
        IVertexUvAccessor {
    static abstract IVertexAccessor GetAccessorForModel(IModel model);
  }

  public interface IVertexNormalAccessor : IVertexTargeter,
                                           IReadOnlyNormalVertex { }

  public interface IVertexTangentAccessor : IVertexTargeter,
                                            IReadOnlyTangentVertex { }

  public interface IVertexColorAccessor : IVertexTargeter,
                                          IReadOnlySingleColorVertex,
                                          IReadOnlyMultiColorVertex { }

  public interface IVertexUvAccessor : IVertexTargeter,
                                       IReadOnlySingleUvVertex,
                                       IReadOnlyMultiUvVertex { }

}
