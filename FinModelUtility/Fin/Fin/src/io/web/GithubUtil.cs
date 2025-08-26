﻿using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Text;

using fin.io.bundles;
using fin.util.strings;

using Microsoft.AspNetCore.WebUtilities;

namespace fin.io.web;

public interface IExceptionContext {
  string? Title { get; }
  string Steps { get; }
}

public class LoadFileException(IReadOnlySystemFile file)
    : IExceptionContext {
  public string Title
    => $"[Bug] Failed to load {file.FullPath}";

  public string Steps
    => $"1. Attempted to load {file.FullPath}.";
}

public class LoadFileBundleExceptionContext(IGameAndLocalPath fb)
    : IExceptionContext {
  public string Title
    => $"[Bug] Failed to load {fb.GameAndLocalPath}";

  public string Steps
    => $"1. Attempted to load {fb.GameAndLocalPath}.";
}

public class RenderFileBundleExceptionContext(IFileBundle fb)
    : IExceptionContext {
  public string Title
    => $"[Bug] Failed to render {fb.TrueFullPath}";

  public string Steps
    => $"1. Attempted to render {fb.TrueFullPath}.";
}

public static class GitHubUtil {
  public const string GITHUB_URL
      = "https://github.com/MeltyPlayer/FinModelUtility";

  public const string GITHUB_NEW_ISSUE_URL = $"{GITHUB_URL}/issues/new";

  public const string GITHUB_CHOOSE_NEW_ISSUE_URL
      = $"{GITHUB_NEW_ISSUE_URL}/choose";

  public static string GetNewIssueUrl(Exception? exception,
                                      IExceptionContext? context) {
    if (exception == null) {
      return GITHUB_CHOOSE_NEW_ISSUE_URL;
    }

    var body = new StringBuilder();
    body.AppendLine("**Stack trace**");
    body.AppendLine("```");
    body.AppendLine(
        $"{exception.GetType().ToString()}: {exception.Message}");

    var stackTrace = new StackTrace(exception, true);
    foreach (var frame in stackTrace.GetFrames()) {
      var method = frame.GetMethod();
      body.Append($"    at {method.DeclaringType}.{method.Name}");

      if (method.IsGenericMethod) {
        body.Append('<');
        var genericArguments = method.GetGenericArguments();
        for (var i = 0; i < genericArguments.Length; ++i) {
          if (i > 0) {
            body.Append(", ");
          }

          body.Append(genericArguments[i].Name);
        }

        body.Append('>');
      }

      {
        body.Append('(');
        var parameters = method.GetParameters();
        for (var i = 0; i < parameters.Length; ++i) {
          if (i > 0) {
            body.Append(", ");
          }

          var parameter = parameters[i];
          body.Append($"{parameter.ParameterType} {parameter.Name}");
        }

        body.Append(')');
      }

      var fileName = frame.GetFileName();
      if (fileName != null) {
        var abbreviatedFileName = fileName
                                  .Replace('\\', '/')
                                  .SubstringAfter("FinModelUtility/");
        abbreviatedFileName
            = abbreviatedFileName.Replace("FinModelUtility/FinModelUtility",
                                          "FinModelUtility")
                                 .Replace("Github", "GitHub");

        body.Append(" in //").Append(abbreviatedFileName);
      }

      body.AppendLine($":line {frame.GetFileLineNumber()}");
    }

    body.AppendLine("```");

    body.AppendLine(
        """

        **To Reproduce**
        Steps to reproduce the behavior:

        """
    );

    body.AppendLine(
        context?.Steps ??
        """
        1. Go to '...'
        2. Click on '....'
        3. Scroll down to '....'
        4. See error
        """);

    body.Append(
        """

        **Additional context**
        Add any other context about the problem here.
        """);

    var queryParams = new Dictionary<string, string?> {
        ["body"] = body.ToString(),
        ["template"] = "bug_report.md",
        ["title"] = context?.Title ?? "[Enhancement] (Enter a description)",
    };

    var baseIssueUrl
        = "https://github.com/MeltyPlayer/FinModelUtility/issues/new";
    return QueryHelpers.AddQueryString(baseIssueUrl, queryParams);
  }
}