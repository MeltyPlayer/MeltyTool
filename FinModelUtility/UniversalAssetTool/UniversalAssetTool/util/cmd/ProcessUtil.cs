using System.Diagnostics;

using fin.io;
using fin.log;
using fin.util.asserts;

namespace uni.util.cmd;

public class ProcessUtil {
  public static Process ExecuteBlocking(
      IReadOnlySystemFile exeFile,
      params string[] args) {
      var processSetup = new ProcessSetup(exeFile, args) {
          Method = ProcessExecutionMethod.BLOCK,
      };
      return ProcessUtil.Execute(processSetup);
    }

  public static Process ExecuteBlockingSilently(
      IReadOnlySystemFile exeFile,
      params string[] args) {
      var processSetup = new ProcessSetup(exeFile, args) {
          Method = ProcessExecutionMethod.BLOCK, WithLogging = false,
      };
      return ProcessUtil.Execute(processSetup);
    }

  public enum ProcessExecutionMethod {
    MANUAL,
    BLOCK,
    TIMEOUT,
    ASYNC
  }

  public class ProcessSetup {
    public IReadOnlySystemFile ExeFile { get; set; }
    public string[] Args { get; set; }

    public ProcessExecutionMethod Method { get; set; } =
      ProcessExecutionMethod.BLOCK;

    public bool WithLogging { get; set; } = true;

    public ProcessSetup(IReadOnlySystemFile exeFile, params string[] args) {
        this.ExeFile = exeFile;
        this.Args = args;
      }
  }

  public static Process Execute(ProcessSetup processSetup) {
      var exeFile = processSetup.ExeFile;
      Asserts.True(
          exeFile.Exists,
          $"Attempted to execute a program that doesn't exist: {exeFile}");

      var args = processSetup.Args;
      var argString = "";
      for (var i = 0; i < args.Length; ++i) {
        // TODO: Is this safe?
        var arg = args[i];

        if (i > 0) {
          argString += " ";
        }

        argString += arg;
      }

      var processStartInfo =
          new ProcessStartInfo($"\"{exeFile.FullPath}\"", argString) {
              CreateNoWindow = true,
              RedirectStandardOutput = true,
              RedirectStandardError = true,
              RedirectStandardInput = true,
              UseShellExecute = false,
          };

      var process = Asserts.CastNonnull(Process.Start(processStartInfo));
      ChildProcessTracker.AddProcess(process);

      var logger = Logging.Create(exeFile.FullPath);
      if (processSetup.WithLogging) {
        process.OutputDataReceived += (_, args) => {
          if (args.Data != null) {
            logger!.LogInformation("  " + args.Data);
          }
        };
        process.ErrorDataReceived += (_, args) => {
          if (args.Data != null) {
            logger!.LogError("  " + args.Data);
          }
        };
      } else {
        process.OutputDataReceived += (_, _) => { };
        process.ErrorDataReceived += (_, _) => { };
      }

      process.BeginOutputReadLine();
      process.BeginErrorReadLine();

      switch (processSetup.Method) {
        case ProcessExecutionMethod.MANUAL: {
          break;
        }

        case ProcessExecutionMethod.BLOCK: {
          process.WaitForExit();
          break;
        }

        default:
          throw new NotImplementedException();
      }

      // TODO: https://stackoverflow.com/questions/139593/processstartinfo-hanging-on-waitforexit-why
     /*
      sing var outputWaitHandle = new AutoResetEvent(false);
      sing var errorWaitHandle = new AutoResetEvent(false);

      rocess.OutputDataReceived += (sender, e) => {
        f (e.Data == null) {
          / ReSharper disable once AccessToDisposedClosure
          utputWaitHandle.Set();
         else {
          ogger.LogInformation(e.Data);
        
      ;
      rocess.ErrorDataReceived += (sender, e) => {
        f (e.Data == null) {
          / ReSharper disable once AccessToDisposedClosure
          rrorWaitHandle.Set();
         else {
          ogger.LogError(e.Data);
        
      ;

      rocess.Start();

      rocess.BeginOutputReadLine();
      rocess.BeginErrorReadLine();

      / TODO: Allow passing in timeouts
      f (outputWaitHandle.WaitOne() &&
          rrorWaitHandle.WaitOne()) {
        rocess.WaitForExit();
        / Process completed. Check process.ExitCode here.
       else {
        / Timed out.
      *//

      return process;
    }
}