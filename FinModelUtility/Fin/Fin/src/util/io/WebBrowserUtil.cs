using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using fin.util.asserts;

namespace fin.util.io;

public static class WebBrowserUtil {
  public static void OpenGithub()
    => OpenUrl("https://github.com/MeltyPlayer/FinModelUtility");

  public static void OpenGithubNewIssue()
    => OpenUrl("https://github.com/MeltyPlayer/FinModelUtility/issues/new");

  public static void OpenUrl(string urlString) {
    Asserts.True(Uri.IsWellFormedUriString(urlString, UriKind.Absolute));
    Asserts.True(Uri.TryCreate(urlString, UriKind.Absolute, out var url));
    OpenUrl(url!);
  }

  public static void OpenUrl(Uri url) {
    Asserts.True(url.IsWellFormedOriginalString());
    Asserts.True(url.Scheme == Uri.UriSchemeHttp ||
                 url.Scheme == Uri.UriSchemeHttps);

    var urlString = url.ToString();

    // Shamelessly stolen from: https://stackoverflow.com/a/43232486
    try {
      Process.Start(urlString);
    } catch {
      // hack because of this: https://github.com/dotnet/corefx/issues/10361
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
        urlString = urlString.Replace("&", "^&");
        Process.Start(new ProcessStartInfo(urlString) { UseShellExecute = true });
      } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
        Process.Start("xdg-open", urlString);
      } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
        Process.Start("open", urlString);
      } else {
        throw;
      }
    }
  }
}