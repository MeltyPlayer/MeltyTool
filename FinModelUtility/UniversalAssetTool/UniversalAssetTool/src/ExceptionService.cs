namespace uni;

public static class ExceptionService {
  public static void HandleException(Exception e) => OnException?.Invoke(e);

  public static event Action<Exception> OnException;
}