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

  public class ProcessSetup(IReadOnlySystemFile exeFile, params string[] args) {
    public IReadOnlySystemFile ExeFile { get; set; } = exeFile;
    public string[] Args { get; set; } = args;

    public ProcessExecutionMethod Method { get; set; } =
      ProcessExecutionMethod.BLOCK;

    public bool WithLogging { get; set; } = true;
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
    using var outputWaitHandle = new AutoResetEvent(false);
    using var errorWaitHandle = new AutoResetEvent(false);

    process.OutputDataReceived += (sender, e) => {
      if (e.Data == null) {
        // ReSharper disable once AccessToDisposedClosure
        outputWaitHandle.Set();
      } else {
        logger.LogInformation(e.Data);
      }
    };
    process.ErrorDataReceived += (sender, e) => {
      if (e.Data == null) {
        // ReSharper disable once AccessToDisposedClosure
        errorWaitHandle.Set();
      } else {
        logger.LogError(e.Data);
      }
    };

    process.Start();

    process.BeginOutputReadLine();
    process.BeginErrorReadLine();

    // TODO: Allow passing in timeouts
    if (outputWaitHandle.WaitOne() &&
        errorWaitHandle.WaitOne()) {
      process.WaitForExit();
      // Process completed. Check process.ExitCode here.
    } else {
      // Timed out.
    }*/

    return process;
  }
}