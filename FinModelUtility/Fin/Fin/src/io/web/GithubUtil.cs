using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Text;

using fin.util.strings;

using Microsoft.AspNetCore.WebUtilities;

namespace fin.io.web;

public static class GitHubUtil {
  public const string GITHUB_URL
      = "https://github.com/MeltyPlayer/FinModelUtility";

  public const string GITHUB_NEW_ISSUE_URL = $"{GITHUB_URL}/issues/new";

  public const string GITHUB_CHOOSE_NEW_ISSUE_URL
      = $"{GITHUB_NEW_ISSUE_URL}/choose";

  public static string GetNewIssueUrl(Exception? exception) {
    if (exception == null) {
      return GITHUB_CHOOSE_NEW_ISSUE_URL;
    }

    var description = new StringBuilder();
    description.AppendLine("**Stack trace**");
    description.AppendLine(
        $"{exception.GetType().ToString()}: {exception.Message}");

    var stackTrace = new StackTrace(exception, true);
    foreach (var frame in stackTrace.GetFrames()) {
      var method = frame.GetMethod();
      description.Append($"    at {method.DeclaringType}.{method.Name}");

      if (method.IsGenericMethod) {
        description.Append('<');
        var genericArguments = method.GetGenericArguments();
        for (var i = 0; i < genericArguments.Length; ++i) {
          if (i > 0) {
            description.Append(", ");
          }

          description.Append(genericArguments[i].Name);
        }

        description.Append('>');
      }

      {
        description.Append('(');
        var parameters = method.GetParameters();
        for (var i = 0; i < parameters.Length; ++i) {
          if (i > 0) {
            description.Append(", ");
          }

          var parameter = parameters[i];
          description.Append($"{parameter.ParameterType} {parameter.Name}");
        }

        description.Append(')');
      }

      description.Append(" in //")
                 .Append(frame.GetFileName()
                              .Replace('\\', '/')
                              .SubstringAfter("FinModelUtility/"));

      description.AppendLine($":line {frame.GetFileLineNumber()}");
    }

    description.Append(
        """

        **To Reproduce**
        Steps to reproduce the behavior:
        1. Go to '...'
        2. Click on '....'
        3. Scroll down to '....'
        4. See error

        **Additional context**
        Add any other context about the problem here.

        """);

    var queryParams = new Dictionary<string, string?> {
        ["description"] = description.ToString(),
        ["template"] = "bug_report.md",
        ["title"] = "[Enhancement] (Enter a description)",
    };

    var baseIssueUrl
        = "https://github.com/MeltyPlayer/FinModelUtility/issues/new";
    return QueryHelpers.AddQueryString(baseIssueUrl, queryParams);
  }
}